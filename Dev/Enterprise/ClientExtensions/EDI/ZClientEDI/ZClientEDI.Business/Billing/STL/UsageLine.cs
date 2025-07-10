using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Basic element of usage for reporting and calculating totals.
	/// Using entity + price + unit count + discount.
	/// 
	/// If price includes multiple subusage types (e.g., Sales and Marketing includes Client Intel + Campaign Manager)
	/// then may also persist subusages. One subusage can contribute to multiple price items, 
	/// e.g., a message may have a price and contribute to a total usage fee.
	/// </summary>
	public class UsageLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UsageLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UsageLine(BusinessObjectFactory factory, ZDateTime billingPeriod, ZDateTime usagePeriod, ZString additionalDescription)
			: this(factory)
		{
			PeriodStart = usagePeriod;
			AdditionalDescription = billingPeriod == usagePeriod
				? additionalDescription
				: UsageLine.JoinNonEmpty(" - ", additionalDescription, usagePeriod.ToDateTime().ToString("MMM yyyy", CultureInfo.InvariantCulture));
		}

		public UsageLine(BusinessObjectFactory factory, ZDateTime billingPeriod, ZDateTime usagePeriod, ZString additionalDescription,
			ClientLicencePriceItem priceItem, ClientLicencePriceItem roleItem, ClientLicencePriceItem moduleItem, ClientLicencePriceItem functionItem)
			: this(factory, billingPeriod, usagePeriod, additionalDescription)
		{
			SetPriceItems(priceItem, roleItem, moduleItem, functionItem);
		}

		public UsageLine(BusinessObjectFactory factory, ZDateTime billingPeriod, ZDateTime usagePeriod, ZString additionalDescription, PriceNode node)
			: this(factory, billingPeriod, usagePeriod, additionalDescription,
				  node.Item,
				  node.FindCategoryItemByIndentLevel(0),
				  node.FindCategoryItemByIndentLevel(1),
				  node.FindCategoryItemByIndentLevel(2))
		{
		}

		public ZString PriceItemCode { get { return PriceItem != null ? PriceItem.L7_Code : ZString.Empty; } }
		public ZString PriceItemCategory { get { return PriceItem != null ? PriceItem.L7_Category : ZString.Empty; } }
		public ZString AdditionalDescription { get; set; }
		public ZString PriceItemOnlyDesc { get { return PriceItem != null ? PriceItem.L7_DescriptionLocalized : ZString.Empty; } }
		public ZString PriceItemDesc { get { return JoinNonEmpty(" - ", PriceItemOnlyDesc, AdditionalDescription); } }
		public ZDecimal RawUnitCount { get { return SingleUsage != null ? SingleUsage.UnitCount : ZDecimal.Zero; } }
		public ZDecimal UnitCount => Math.Max(TotalUnitCount - IncludedUnitCount, 0m);
		public ZInt IncludedUnitCount { get; set; }
		public ZDecimal TotalUnitCount { get; set; }

		public ZDecimal LicenceUnits { get { return licenceUnitsOverride != null ? licenceUnitsOverride.Value : (PriceItem != null ? PriceItem.L7_LicenceUnits : ZDecimal.Zero); } }
		public ZString EnterpriseCode { get { return SingleUsage != null && SingleUsage.Database != null ? SingleUsage.Database.EnterpriseCode : ZString.Empty; } }
		public ZString CompanyCode { get { return SingleUsage != null ? SingleUsage.CompanyCode : ""; } }
		public ZString CountryCodeForDiscount { get; set; }
		public ZString ServerCode { get { return SingleUsage != null && SingleUsage.Database != null ? SingleUsage.Database.LD_ServerCode : ZString.Empty; } }
		public ZInt Sequence { get { return PriceItem != null ? PriceItem.L7_Order : ZInt.Zero; } }
		public ZDateTime PeriodStart { get; set; }
		public bool IsBorderWise => usages.FirstOrDefault()?.ChargeableUsage?.U1_Code.ToString() == BillingConstants.BillingSystem.BorderWise;

		/// <summary>
		/// Copy of ClientLicencePriceHeader.L6_DiscountCode from the STL pricelist.
		/// Used so we can discount items from a universal pricelist which do not have a L6_DiscountCode.
		/// </summary>
		public ZString DiscountVersionCode { get; set; }

		public ClientLicencePriceItem PriceItem
		{
			get;
			internal set;
		}

		public IEnumerable<Usage> Usages
		{
			get { return usages; }
		}
		readonly List<Usage> usages = new List<Usage>(1);

		public int UsageCount
		{
			get { return usages.Count; }
		}

		public void AddUsage(Usage usage)
		{
			AddUsages(new Usage[] { usage });
		}

		public void AddUsages(IEnumerable<Usage> usagesToAdd)
		{
			foreach (var usage in usagesToAdd)
			{
				if (usage.PeriodStart.IsEmpty)
				{
					throw new InvalidOperationException("PeriodStart must have a value");
				}
				if (usage.OwnerDelivery == null)
				{
					throw new InvalidOperationException("OwnerDelivery must have a value");
				}
			}
			usages.AddRange(usagesToAdd);
		}

		/// <summary>
		/// There is one contributing usage.
		/// Otherwise the usages go to lines in the SubUsage collection.
		/// </summary>
		public Usage SingleUsage
		{
			get { return usages.Count == 1 ? usages[0] : null; }
		}

		public ZDecimal Price { get; set; }
		public ZDecimal TotalPrice { get; set; }
		public ZString Direction { get; set; }
		public bool IsDisbursement { get; set; }
		public ZString PriceCurrency { get; set; }
		public ZDecimal DiscountedPrice { get; set; }

		public void SetAmounts(decimal unroundedPreDiscount, decimal unroundedPostDiscount)
		{
			var currency = Factory?.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, PriceCurrency);
			var currencyDecimals = FormatOptions.RoundingDecimals != BillingConstants.RoundingDecimals ? FormatOptions.RoundingDecimals :
										(currency?.Decimals ?? BillingConstants.RoundingDecimals);

			var preDiscount = Utilities.Round(unroundedPreDiscount, FormatOptions.RoundingDecimals);
			var postDiscount = Utilities.Round(unroundedPostDiscount, currencyDecimals);

			PreDiscountAmount = preDiscount;
			PostDiscountAmount = postDiscount;

			UnadjustedPreDiscountAmount = PreDiscountAmount;
			UnadjustedPostDiscountAmount = PostDiscountAmount;
		}

		/// <summary>
		/// Adjust for commitment
		/// </summary>
		/// <param name="adjustmentFactor"></param>
		public void AdjustAmounts(decimal adjustmentFactor)
		{
			PreDiscountAmount = Utilities.Round(PreDiscountAmount * adjustmentFactor, FormatOptions.RoundingDecimals);
			PostDiscountAmount = Utilities.Round(PostDiscountAmount * adjustmentFactor, FormatOptions.RoundingDecimals);
		}

		// Final amounts including adjustments for commitments - does not include Non-Current Version surcharge
		public ZDecimal PreDiscountAmount { get; private set; }
		public ZDecimal PostDiscountAmount { get; private set; }
		public ZDecimal DiscountAmount { get { return PostDiscountAmount - PreDiscountAmount; } }

		// Non-Current Version Surcharge
		public decimal VersionSurchargeAmount { get; internal set; }
		public decimal LocalVersionSurchargeAmount { get; internal set; }

		// Amounts without adjustments for commitments
		public ZDecimal UnadjustedPreDiscountAmount { get; private set; }
		public ZDecimal UnadjustedPostDiscountAmount { get; private set; }
		public ZDecimal UnadjustedDiscountAmount { get { return UnadjustedPostDiscountAmount - UnadjustedPreDiscountAmount; } }

		public ZBool ShouldApplyDiscounts { get; set; } = true;
		public ZString DiscountKey { get { return Discounts != null ? Discounts.Key : ""; } }
		public StlBilling.DiscountSet Discounts { get; set; }

		public ZString LocalCurrency { get; set; }
		public ZDecimal LocalPreDiscountAmount { get; set; }
		public ZDecimal LocalPostDiscountAmount { get; set; }
		public ZDecimal LocalDiscountAmount { get; set; }

		public decimal InvoiceCurrencyRateAsMultiplier { get; set; }
		public int InvoiceCurrencyDecimals { get; set; }

		#region Document Properties

		public ZString PriceText { get { return Price.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture); } }
		public ZString DiscountedPriceText { get { return DiscountedPrice.ToString(StlBilling.DiscountedPriceFormat, CultureInfo.InvariantCulture); } }
		public ZString LicenceUnitsText { get { return LicenceUnits.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture); } }
		public ZString TotalLicenceUnitsText { get { return (LicenceUnits * TotalUnitCount).ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture); } }
		public ZString PreDiscountAmountText { get { return UnadjustedPreDiscountAmount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture); } }
		public ZString PostDiscountAmountText { get { return UnadjustedPostDiscountAmount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture); } }

		public BillingConstants.FormatOptions FormatOptions { get; set; } = new BillingConstants.FormatOptions();

		/// <summary>
		/// Fee basis - defaults upon construction to PriceItem fee basis, but can be changed later.
		/// </summary>
		public ZString FeeBasisText { get; set; }

		public ZString FirstRolePriceCurrency { get; set; }

		void SetPriceItems(ClientLicencePriceItem priceItem, ClientLicencePriceItem roleItem, ClientLicencePriceItem moduleItem, ClientLicencePriceItem functionItem)
		{
			PriceItem = priceItem;
			licenceUnitsOverride = null;
			if (priceItem != null && !priceItem.L7_L6.IsEmpty)
			{
				DiscountVersionCode = priceItem.Parent?.L6_DiscountCode ?? ZString.Empty;
			}

			roleParentItem = roleItem;
			moduleParentItem = moduleItem;
			functionParentItem = functionItem;

			RoleIndex = roleParentItem != null ? roleParentItem.L7_Order : NoRoleIndex;

			if (roleItem == null && PriceItem != null)
			{
				if (PriceItem.Parent.L6_SystemCode == BillingConstants.PriceHeaderType.EHub)
				{
					role = BillingConstants.BillingSystemList.GetMultilingualDescriptionFromCode(BillingConstants.BillingSystem.ClientMapping);
					RoleIndex = EHubRoleIndex;
				}
			}

			FeeBasisText = PriceItem != null ? (!PriceItem.L7_ChargeBasisMultilingual.IsEmpty ? PriceItem.L7_ChargeBasisMultilingual : PriceItem.L7_FeeTypeDescMultilingual) : ZString.Empty;
		}

	public const int EHubRoleIndex = 100000;
		public const int NoRoleIndex = 100001;
		public const int CommitmentRoleIndex = 100002;

		public ClientLicencePriceItem RoleParentItem => roleParentItem;
		ClientLicencePriceItem roleParentItem;

		public ClientLicencePriceItem ModuleParentItem => moduleParentItem;
		ClientLicencePriceItem moduleParentItem;

		public ClientLicencePriceItem FunctionParentItem => functionParentItem;
		ClientLicencePriceItem functionParentItem;

		MultilingualString role;

		public ZInt RoleIndex { get; set; }

		public ZString RoleAndCurrency
		{
			get { return ((int)RoleIndex).ToString("0000000000", CultureInfo.InvariantCulture) + PriceCurrency; }
		}

		/// <summary>
		/// Used in summary document template to order module sections
		/// </summary>
		public ZInt ModuleIndex
			=> moduleParentItem?.L7_Order ?? RoleIndex;

		/// <summary>
		/// Used in summary document template to order function sections
		/// </summary>
		public ZInt FunctionIndex
			=> functionParentItem?.L7_Order ?? ModuleIndex;

		public ZString Role
		{
			get
			{
				var result = ZString.Empty;
				if (roleParentItem != null)
				{
					result = roleParentItem.L7_DescriptionLocalized;
				}
				else if (role != null)
				{
					result = role;
				}

				return result.TrimStart();
			}
		}

		public ZString Module
		{
			get { return moduleParentItem != null ? moduleParentItem.L7_DescriptionLocalized.TrimStart() : ZString.Empty; }
		}

		public ZString Function
		{
			get { return functionParentItem != null ? functionParentItem.L7_DescriptionLocalized.TrimStart() : ZString.Empty; }
		}

		#endregion

		public static ZString JoinNonEmpty(string separator, ZString a, ZString b)
		{
			if (a.IsEmpty)
			{
				return b;
			}
			else if (b.IsEmpty)
			{
				return a;
			}
			else
			{
				return a + separator + b;
			}
		}

		public void SetLicenceUnits(ZDecimal licUnits)
		{
			licenceUnitsOverride = licUnits;
		}

		ZDecimal? licenceUnitsOverride;

		public UsageLine ParentUsageLine { get; set; }
	}
}

