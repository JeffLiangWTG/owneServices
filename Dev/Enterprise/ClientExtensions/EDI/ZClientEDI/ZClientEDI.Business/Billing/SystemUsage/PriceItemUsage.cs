using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IPriceItemUsage
	{
		ClientLicencePriceItem PriceItem { get; }
	}

	public class PriceItemUsage : SystemUsage, IPriceItemUsage
	{
		public PriceItemUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZString systemCode, bool isTransactional)
			: base(factory, user, periodStart)
		{
			this.systemCode = systemCode;
			IsTransactional = isTransactional;
			if (isTransactional)
			{
				UnitCountDescription = "Transactions";
			}
		}

		#region Properties

		public override ZString CurrencyCode => CurrencyFromPriceList;

		/// <summary>
		/// Currency from price item if entered, otherwise from price header.
		/// </summary>
		public ZString CurrencyFromPriceList
		{
			get
			{
				var result = CurrencyFromPriceItem;
				if (result.IsEmpty)
				{
					result = base.CurrencyCode;
				}
				return result;
			}
		}

		/// <summary>
		/// Currency just from the price item. 
		/// Blank if one hasn't been entered, or the price item is null.
		/// </summary>
		public ZString CurrencyFromPriceItem
		{
			get
			{
				var item = PriceItem;
				if (item != null && !item.L7_RX_NKCurrency.IsEmpty)
				{
					return item.L7_RX_NKCurrency;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public override ZDecimal UnitPrice
		{
			get { return PriceItem != null ? PriceItem.L7_Price : ZDecimal.Zero; }
		}

		public bool HasPriceItem
		{
			get { return PriceItem != null; }
		}

		public void CalculatePriceItem()
		{
			if (priceItem == null && PriceHeader != null)
			{
				priceItem = GetPriceItemCore();
			}
		}

		public ClientLicencePriceItem PriceItem
		{
			get
			{
				CalculatePriceItem();
				return priceItem;
			}
		}
		ClientLicencePriceItem priceItem;

		public override void ResetPriceHeader()
		{
			priceItem = null;
			base.ResetPriceHeader();
		}

		protected virtual ZDecimal PriceItemLicenceUnits => (PriceItem?.L7_LicenceUnits ?? ZDecimal.Zero) * LicenceUnitsMultiplier;
		public decimal LicenceUnitsMultiplier { get; set; } = 1m;

		/// <summary>
		/// Total unit count. Not all may be charged if there is an IncludedUnitCount.
		/// </summary>
		public ZInt TotalUnitCount { get; protected set; }

		/// <summary>
		/// The count of units that are included without charge.
		/// Only units in excess of this amount are charged.
		/// </summary>
		public virtual ZInt IncludedUnitCount => (IsTransactional && PriceItem != null && !PriceItem.IsTransactionalOneVolumeBreak) ? PriceItem.L7_UnitBreak : ZInt.Zero;

		public override ZInt UnitCount => Math.Max(TotalUnitCount - IncludedUnitCount, 0);

		protected virtual ClientLicencePriceItem GetPriceItemCore()
		{
			var item = PriceHeader.LocalOrStandardItems.FindByCode(PriceItemCode);
			if (!PriceItemIsCorrectFeeType(item))
			{
				item = null;
			}
			return item;
		}

		protected virtual bool PriceItemIsCorrectFeeType(ClientLicencePriceItem item) => !IsTransactional || (item != null && item.IsTransactional);
		public virtual ZString PriceItemCode => SystemCode;
		public override bool HasLicenceUnits => HasPriceItem && PriceItem.HasLicenceUnits;

		public void CalculatePriceItemByTotalUnitCount()
		{
			CalculatePriceItemForUnitBreak(TotalUnitCount);
		}

		public void CalculatePriceItemForUnitBreak(int totalUnitCountForBreak)
		{
			var currentPriceItem = PriceItem;
			if (currentPriceItem != null && currentPriceItem.IsTransactionalOneVolumeBreak)
			{
				var expectedPriceItem = currentPriceItem.Parent.Items
					.Where(x => x.L7_Code == currentPriceItem.L7_Code && totalUnitCountForBreak > x.L7_UnitBreak)
					.OrderByDescending(x => x.L7_UnitBreak)
					.FirstOrDefault();
				if (currentPriceItem != expectedPriceItem)
				{
					priceItem = expectedPriceItem;
				}
			}
		}

		#region Transactional Properties

		readonly bool IsTransactional;
		readonly ZString systemCode;
		public override ZString SystemCode => systemCode;
		public ZInt TransactionCount { get => TotalUnitCount; set => TotalUnitCount = value; }
		public ZString Reference1 { get; set; }
		public ZString Reference2 { get; set; }
		public ZString Reference3 { get; set; }
		public ZString Reference4 { get; set; }

		#endregion

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = BuildCommon();
			return new SummarySection[] { result };
		}

		SummarySection BuildCommon(bool includeLicenceUnits = false)
		{
			var mainDescription = !SummaryHeaderDescription.IsEmpty ? SummaryHeaderDescription : DefaultSummaryHeaderDescription;

			var result = new SummarySection(Factory);
			var line = result.Lines.AddNew();

			line.MainDescription = mainDescription;
			line.UnitPrice = UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.CurrentCulture);
			line.UnitCount = UnitCount.ToString("G", CultureInfo.CurrentCulture);
			line.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.CurrentCulture);

			var header = result.Header;
			header.MainDescription = mainDescription;
			header.UnitPrice = "Price";
			header.UnitCount = UnitCountDescription;
			header.Amount = "Total";
			header.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.CurrentCulture);
			header.ClientCompanyDescription = ClientCompanyDescription;

			if (includeLicenceUnits && HasLicenceUnits)
			{
				line.LicenceUnits = PriceItemLicenceUnits.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.CurrentCulture);
				line.LicenceUnitsAmount = LicenceUnitsAmount.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.CurrentCulture);
			}

			if (IncludedUnitCount != 0)
			{
				line.PurchasedCount = IncludedUnitCount.ToString("G", CultureInfo.CurrentCulture);
				line.TotalUnitCount = TotalUnitCount.ToString("G", CultureInfo.CurrentCulture);

				header.PurchasedCount = !IncludedUnitCountDescription.IsEmpty ? (string)IncludedUnitCountDescription : "Included";
				header.TotalUnitCount = !TotalUnitCountDescription.IsEmpty ? (string)TotalUnitCountDescription : "Total Units";
			}

			return result;
		}

		public SummarySection BuildGeneralSummarySectionWithPriceItemDescription(bool includeLicenceUnits = false)
		{
			var result = BuildCommon(includeLicenceUnits);

			var item = PriceItem;
			if (item != null)
			{
				var line = result.Lines[0];
				line.MainDescription = item.L7_DescriptionLocalized;
			}

			return result;
		}

		public ZString SummaryHeaderDescription { get; set; }
		public ZString UnitCountDescription { get; protected set; }
		public ZString IncludedUnitCountDescription { get; protected set; }
		public ZString TotalUnitCountDescription { get; protected set; }

		protected ZString DefaultSummaryHeaderDescription
		{
			get { return SystemDescription + " Usage"; }
		}

		#endregion
	}
}

