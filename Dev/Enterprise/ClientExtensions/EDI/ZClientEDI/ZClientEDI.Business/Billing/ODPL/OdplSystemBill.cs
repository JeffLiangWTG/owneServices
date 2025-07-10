using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	public class OdplSystemBill : SystemBill, IOdplDiscountable, ISystemMinimumFeeContributionBill
	{
		public OdplSystemBill(BillingRunContext context)
			: this(context.Factory)
		{
			this.context = context;
		}

		public OdplSystemBill(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		readonly BillingRunContext context;

		#region Calculate Group Amounts

		protected override void CalculateGroupAmounts()
		{
			GroupProductionAmount = 0;
			PurchasedLicenceUnits = 0;

			foreach (OdplUsage odplUsage in SystemUsages)
			{
				GroupProductionAmount += odplUsage.Amount;
				PurchasedLicenceUnits += odplUsage.PurchasedLicenceUnits;
				if (odplUsage.LicenceUnitRate != 0)
				{
					LicenceUnitRate = odplUsage.LicenceUnitRate;
				}
			}

			Amount = DiscountCalculation.Amount;
			DiscountAmount = DiscountCalculation.DiscountAmount;
			SurchargeAmount = -SurchargeCalculation.DiscountAmount;
		}

		public IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution()
		{
			var result = new List<SystemMinimumFee>(1);

			if (Amount > 0)
			{
				foreach (var usagesGroup in SystemUsages.Cast<OdplUsage>().Where(x => !x.User.DatabasePK.IsEmpty).GroupBy(x => new { x.PeriodStart, x.User.DatabasePK }))
				{
					var ratio = usagesGroup.Sum(x => x.Amount) / Amount;
					var dbContribution = (Amount - DiscountAmount + SurchargeAmount) * ratio;

					var minimumFee = new SystemMinimumFee(usagesGroup.Key.DatabasePK, usagesGroup.Key.PeriodStart, dbContribution, CurrencyCode, "");
					result.Add(minimumFee);
				}
			}

			return result;
		}

		#endregion

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage systemUsage)
		{
			var usage = (OdplUsage)systemUsage;
			return usage != null && usage.IsHybrid
				? EDIDataRegistry.Instance.OdplHybridUsageChargeCode.Value
				: EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage systemUsage)
		{
			var usage = (OdplUsage)systemUsage;
			return usage != null && usage.IsHybrid
				? EDIDataRegistry.Instance.OdplHybridDiscountChargeCode.Value
				: EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
		}

		#endregion

		#region Amounts

		public ZDecimal GroupProductionAmount { get; private set; }

		public ZInt PurchasedLicenceUnits { get; private set; }
		public ZDecimal LicenceUnitRate { get; private set; }

		#endregion

		#region Discounts

		public ZBool OdplCommitmentOnly
		{
			get
			{
				return Amount != 0m
					&& SystemUsages.Count == 1
					&& SystemUsages[0].Amount == 0;
			}
		}

		public OdplDiscountCalculation DiscountCalculation
		{
			get { return discountCalculation ?? (discountCalculation = OdplDiscountCalculator.CalculateDiscount(this)); }
		}
		OdplDiscountCalculation discountCalculation;

		OdplDiscountCalculation SurchargeCalculation
		{
			get { return surchargeCalculation ?? (surchargeCalculation = OdplDiscountCalculator.CalculateSurcharge(this)); }
		}
		OdplDiscountCalculation surchargeCalculation;

		public void ClearDiscounts()
		{
			discountCalculation = null;
			surchargeCalculation = null;
		}

		#endregion

		#region IOdplDiscountable Members

		public int CoreOnDemandUsers
		{
			get
			{
				int coreOnDemandUsers = 0;

				foreach (OdplUsage odplUsage in SystemUsages)
				{
					var coreUsage = odplUsage.CoreUsage;
					if (coreUsage != null)
					{
						coreOnDemandUsers += coreUsage.UnitCount;
					}
				}

				return coreOnDemandUsers;
			}
		}

		ZString IDiscountable.SystemCode
		{
			get { return SystemCode; }
		}

		ZDecimal IDiscountable.AmountToDiscount
		{
			get { return Math.Max(0, GroupProductionAmount - PurchasedLicenceUnits * LicenceUnitRate); }
		}

		public ZDecimal AmountToDiscountForModule(string moduleCode, out string moduleName)
		{
			ZDecimal amount = 0m;
			moduleName = "";

			foreach (OdplUsage odplUsage in SystemUsages.Cast<OdplUsage>())
			{
				foreach (OdplModuleUsage moduleUsage in odplUsage.ModuleUsages)
				{
					if (moduleUsage.ModuleCode == moduleCode)
					{
						amount += moduleUsage.Amount;
						moduleName = moduleUsage.ModuleName;
					}
				}
			}

			return amount;
		}

		internal void AddExtraForVolumeDiscountBreak(decimal amount, decimal licenceUnits)
		{
			if (licenceUnits == 0)
			{
				return;
			}

			ExtraVolumeAmount += amount;
			ExtraVolumeLicenceUnits += licenceUnits;
			ClearDiscounts();
			CalculateGroupAmounts();
		}

		decimal ExtraVolumeLicenceUnits { get; set; }
		decimal ExtraVolumeAmount { get; set; }

		public ZDecimal LicenceUnitsToDiscount
		{
			get { return Math.Max(0, SystemUsages.Cast<OdplUsage>().Sum(x => x.LicenceUnitsAmount) - PurchasedLicenceUnits); }
		}

		public ZDecimal MixedAmountAsMoney
		{
			get { return ExtraVolumeAmount + SystemUsages.Cast<OdplUsage>().Sum(x => x.MixedAmountAsMoney); }
		}

		public ZDecimal MixedAmountAsLicenceUnits
		{
			get { return ExtraVolumeLicenceUnits + SystemUsages.Cast<OdplUsage>().Sum(x => x.MixedAmountAsLicenceUnits); }
		}

		public ZBool IsHostedOnWiseCloud => SystemUsages.OfType<OdplUsage>().Any(x => x.Database?.IsHostedOnWiseCloud ?? false);

		#endregion

		#region Group Summary

		public override SummarySection[] GetGroupSummarySections()
		{
			List<SummarySection> result = new List<SummarySection>();
			var totalAmount = GroupProductionAmount;
			if (totalAmount > 0m)
			{
				var section = GetGroupSummarySection(SystemDescription + " Production Group Summary", delegate(SystemUsage x) { return x.Amount; });
				section.Header.AdjustmentDescription = "Less Purchased Licenses";
				section.Header.TotalAmountAdjustment = (-PurchasedLicenceUnits * LicenceUnitRate).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				section.Header.TotalLicenceUnitsAdjustment = (-PurchasedLicenceUnits).ToString(CultureInfo.InvariantCulture);

				section.Header.TotalAmountAfterAdjustment = Math.Max(0m, (totalAmount - PurchasedLicenceUnits * LicenceUnitRate)).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				section.Header.TotalLicenceUnitsAfterAdjustment = LicenceUnitsToDiscount.ToString();
				result.Add(section);
			}

			return result.ToArray();
		}

		#endregion

		#region Discounts / Surcharges Summary

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetDiscountOrSurchargeSections(DiscountCalculation);
		}

		public override SummarySection[] GetSurchargeSummarySections()
		{
			return GetDiscountOrSurchargeSections(SurchargeCalculation);
		}

		SummarySection[] GetDiscountOrSurchargeSections(OdplDiscountCalculation discountOrSurcharge)
		{
			if (discountOrSurcharge.DiscountDescriptions.Any())
			{
				var result = new SummarySection(Factory);
				foreach (var description in discountOrSurcharge.DiscountDescriptions)
				{
					var summaryLine = result.Lines.AddNew();
					summaryLine.MainDescription = description;
				}
				result.Header.MainDescription = "On Demand Amount Calculations Applied";

				return new SummarySection[] { result };
			}

			return Array.Empty<SummarySection>();
		}

		#endregion

		#region On Invoice Saving

		protected override void OnInvoiceFactorySavingCore(ARInvoice invoice)
		{
			base.OnInvoiceFactorySavingCore(invoice);
			var discounts = Discounts;
			if (discounts != null)
			{
				discounts.UpdateCappedDiscount(invoice.Factory, invoice.PK);
				discounts.InitDateRange(SystemCode, invoice.Factory);
			}
		}

		protected override void SetChargeableUsagesInvoiceCore(ARInvoice invoice)
		{
			if (OdplCommitmentOnly)
			{
				var systemUsage = SystemUsages[0];
				ClientChargeableUsage[] chargeableUsages = null;
				ClientChargeableUsage chargeableUsageToUpdate = null;
				if (systemUsage.ChargeableUsagePKs.Count > 0)
				{
					chargeableUsages = invoice.Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.PK, systemUsage.ChargeableUsagePKs));

					foreach (var chargeableUsage in chargeableUsages)
					{
						chargeableUsage.U1_UnitCount = 0;
						chargeableUsage.U1_InvoicedUnitCount = 0;
					}

					chargeableUsageToUpdate = chargeableUsages.FirstOrDefault(s => s.U1_Parent == OrganisationPK);
				}

				CreateOrUpdateChargeableUsage(invoice, chargeableUsageToUpdate, systemUsage);
			}
		}

		internal const string CommitDiscountUsageSubCode = "$CM";

		internal static void CreateOrUpdateChargeableUsage(ARInvoice invoice,
			ClientChargeableUsage chargeableUsageToUpdate,
			SystemUsage systemUsage)
		{
			ClientChargeableUsage usage = chargeableUsageToUpdate
				?? invoice.Factory.New<ClientChargeableUsage>();
			usage.U1_AH_Invoice = invoice.PK;
			usage.U1_Code = systemUsage.SystemCode;
			usage.U1_SubCode = CommitDiscountUsageSubCode;
			usage.U1_InvoicedUnitCount = 1;
			usage.U1_LC = systemUsage.Organisation.LicCompany != null ? systemUsage.Organisation.LicCompany.PK : ZGuid.Empty;
			usage.U1_Parent = systemUsage.OrganisationPK;
			usage.U1_PeriodStart = systemUsage.PeriodStart;
			usage.U1_UnitCount = 1;
			usage.U1_UnitPrice = 0m;
			usage.U1_UpdateTime = ZDateTime.UtcNow;
		}

		#endregion

		#region Add Amount Line

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var hybridUsages = SystemUsages.Cast<OdplUsage>().Where(x => x.IsHybrid);
			var nonHybridUsages = SystemUsages.Cast<OdplUsage>().Where(x => !x.IsHybrid);
			var totalAmountIsZero = SystemUsages.Sum(x => x.Amount) == 0m;

			var hybridGroupWeights = BuildTaxGroupWeights(hybridUsages, totalAmountIsZero);
			var nonHybridGroupWeights = BuildTaxGroupWeights(nonHybridUsages, totalAmountIsZero);

			decimal hybridTotalWeight = hybridGroupWeights.Sum(x => x.Value);
			decimal nonHybridTotalWeight = nonHybridGroupWeights.Sum(x => x.Value);

			decimal hybridAmount = CalculateHybridAmount(Amount, hybridTotalWeight, nonHybridTotalWeight);
			decimal nonHybridAmount = Amount - hybridAmount;

			CreateProRataLines(lines, hybridGroupWeights, hybridTotalWeight, hybridAmount, EDIDataRegistry.Instance.OdplHybridUsageChargeCode.Value, BillingSystemDescription + " Hybrid Usage");
			CreateProRataLines(lines, nonHybridGroupWeights, nonHybridTotalWeight, nonHybridAmount, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, BillingSystemDescription + " Usage");

			if (DiscountAmount > 0)
			{
				decimal hybridDiscountAmount = CalculateHybridAmount(DiscountAmount, hybridTotalWeight, nonHybridTotalWeight);
				decimal nonHybridDiscountAmount = DiscountAmount - hybridDiscountAmount;
				CreateProRataLines(lines, hybridGroupWeights, hybridTotalWeight, -hybridDiscountAmount, EDIDataRegistry.Instance.OdplHybridDiscountChargeCode.Value, BillingSystemDescription + " Hybrid Discount");
				CreateProRataLines(lines, nonHybridGroupWeights, nonHybridTotalWeight, -nonHybridDiscountAmount, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value, BillingSystemDescription + " Discount");
			}

			if (SurchargeAmount > 0)
			{
				decimal hybridSurchargeAmount = CalculateHybridAmount(SurchargeAmount, hybridTotalWeight, nonHybridTotalWeight);
				decimal nonHybridSurchargeAmount = SurchargeAmount - hybridSurchargeAmount;
				CreateProRataLines(lines, hybridGroupWeights, hybridTotalWeight, hybridSurchargeAmount, GetSurchargeChargeCodeName(), BillingSystemDescription + " Hybrid Surcharge");
				CreateProRataLines(lines, nonHybridGroupWeights, nonHybridTotalWeight, nonHybridSurchargeAmount, GetSurchargeChargeCodeName(), BillingSystemDescription + " Surcharge");
			}
		}

		static decimal CalculateHybridAmount(decimal amount, decimal hybridTotalWeight, decimal nonHybridTotalWeight)
		{
			return hybridTotalWeight > 0
				? (nonHybridTotalWeight > 0 ? Utilities.Round(amount * hybridTotalWeight / (hybridTotalWeight + nonHybridTotalWeight), BillingConstants.RoundingDecimals) : amount)
				: 0m;
		}

		#endregion

		#region Revenue Breakdown

		public override void CreateRevenueBreakdown(ARInvoice invoice, ZDecimal signedProcessingFeePercentage)
		{
			decimal totalPreDiscount = Amount;
			decimal totalUsageAmount = SystemUsages.Sum(x => x.Amount);
			if (totalPreDiscount == 0 || totalUsageAmount == 0 || SystemUsages.Count == 0)
			{
				return;
			}

			decimal total = TotalAmount;
			decimal totalProcessingFee = signedProcessingFeePercentage / 100m * total;
			decimal totalPostDiscount = TotalAmount + totalProcessingFee;

			foreach (OdplUsage usage in SystemUsages)
			{
				var currency = usage.CurrencyCode;
				var dateForExchangeRate = BillingInvoicingHelper.GetDateForExchangeRate(invoice);
				var chargeCodePk = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, GetAmountChargeCodeName(usage));
				ZGuid discountChargeCodePk = ZGuid.Empty;
				if (totalPreDiscount != totalPostDiscount)
				{
					discountChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, GetDiscountChargeCodeName(usage));
				}

				foreach (var moduleUsage in usage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.Amount != 0))
				{
					var billed = invoice.Factory.New<EdiBilledUsage>();
					decimal weight = moduleUsage.Amount / totalUsageAmount;
					decimal preDiscount = weight * totalPreDiscount;
					decimal postDiscount = weight * totalPostDiscount;
					decimal processingFee = weight * totalProcessingFee;

					billed.BU9_AC_AmountChargeCode = chargeCodePk;
					if (preDiscount != postDiscount)
					{
						billed.BU9_AC_DiscountChargeCode = discountChargeCodePk;
					}
					billed.BU9_AH_Invoice = invoice.PK;
					billed.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
					billed.BU9_LC = usage.User.LicenceCompanyPK;
					billed.BU9_LCC = usage.User.ClientCompanyPK;
					billed.BU9_LD = usage.User.DatabasePK;
					billed.BU9_LocalAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_LocalAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_LocalProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_PeriodStart = PeriodStart.Date;
					billed.BU9_PriceCurrency = currency;
					billed.BU9_TransactionAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_TransactionAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_TransactionProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_UnitCount = new ZDecimal(moduleUsage.UnitCount);
					billed.BU9_UnitPrice = moduleUsage.UnitPrice;
					billed.BU9_UsageCode = SystemCode;
					billed.BU9_UsageSubCode = moduleUsage.ModuleCode;

					var priceItem = moduleUsage.PriceItem;
					if (priceItem != null)
					{
						billed.BU9_L7 = priceItem.PK;
						billed.BU9_PriceCode = priceItem.L7_Code;
					}

					EdiBilledDiscount.CreateBilledDiscounts(billed, DiscountCalculation.Details, SurchargeCalculation.Details);
				}
			}
		}

		#endregion

		#region Validation

		protected override void ValidateAllCore(BusinessObject notificationOwner)
		{
			base.ValidateAllCore(notificationOwner);

			ClientLicencePriceHeader firstPriceHeader = null;
			var periodStartBillingCoreWithRegisteredUsersCount = context?.PeriodStartBillingRegisteredUser ?? ZDateTime.MaxSmallDateTime;

			foreach (SystemUsage usage in SystemUsages)
			{
				ClientChargeableUsage chargeableUsage = Factory.Load<ClientChargeableUsage>(usage.ChargeableUsagePKs.FirstOrDefault());
				LicenceHeader licHeader = null;
				if (chargeableUsage != null)
				{
					var clientCo = chargeableUsage.ClientCompany;
					licHeader = clientCo != null ? clientCo.UsageOwnerLicence : (chargeableUsage.Database != null ? chargeableUsage.Database.UsageOwnerOrFirstLicence : null);

					if (clientCo != null && usage.PeriodStart >= periodStartBillingCoreWithRegisteredUsersCount)
					{
						var activeUsersQuery = new ZQuery(ClientChargeableUsageSchema.U1_PeriodStart, chargeableUsage.U1_PeriodStart);
						activeUsersQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.STL);
						activeUsersQuery.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, "USR");
						activeUsersQuery.AddToFilter(ClientChargeableUsageSchema.U1_LD, clientCo.LCC_LD);
						bool hasActiveUsers = Factory.ExistsInDatabase(ClientChargeableUsageSchema.Constants.TableName, activeUsersQuery);
						if (!hasActiveUsers)
						{
							notificationOwner.AddRowWarning("Database " + clientCo.Database.LD_ServerCode + " is missing STL Active User data");
						}
					}
				}
				if (licHeader != null &&
					licHeader.LA_AgreedLiveDate.IsEmpty)
				{
					notificationOwner.AddRowWarning(string.Format(CultureInfo.CurrentCulture, "Blank site-live usage from {0} (Server {1}) is included.",
						licHeader.Company.Header.OH_Code,
						licHeader.Database.LD_ServerCode));
				}

				var priceHeader = usage.PriceHeader;

				if (priceHeader == null)
				{
					notificationOwner.AddRowError(string.Format(CultureInfo.CurrentCulture, "{0}: No pricelist found for {1}", BillingSystemDescription, usage.Organisation.OH_Code));
				}
				else if (firstPriceHeader == null)
				{
					firstPriceHeader = priceHeader;
				}
				else
				{
					if (firstPriceHeader.L6_LicenceUnitRate != priceHeader.L6_LicenceUnitRate)
					{
						notificationOwner.AddRowError(string.Format(CultureInfo.CurrentCulture, "{0}: pricelists for {1} has multiple licence unit rate i.e. {2} and {3}", BillingSystemDescription, usage.Organisation.OH_Code, firstPriceHeader.L6_LicenceUnitRate, priceHeader.L6_LicenceUnitRate));
					}
				}

				foreach (var discountError in DiscountCalculation.ErrorDescriptions)
				{
					notificationOwner.AddRowError(string.Format(CultureInfo.CurrentCulture, "{0}: {1} - {2}", BillingSystemDescription, usage.Organisation.OH_Code, discountError));
				}
			}
		}

		#endregion
	}
}

