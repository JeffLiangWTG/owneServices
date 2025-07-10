using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.eAdaptor
{
	/// <summary>
	/// eAdaptor usage for one company on one database for one month across all interfaces.
	/// </summary>
	public class EAdaptorUsage : SystemUsage
	{
		public EAdaptorUsage(BusinessObjectFactory factory, LicenceHeader licHeader, ZDateTime periodStart, ClientCompany clientCompany = null)
			: base(factory, new UsingParty(licHeader, clientCompany), periodStart)
		{
			this.LicHeader = licHeader;
			ClientCo = clientCompany;
		}

		public LicenceHeader LicHeader { get; private set; }
		public readonly ClientCompany ClientCo;

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.eAdaptor; }
		}

		public void SetSubUsage(IEnumerable<SubUsage> list)
		{
			subUsageList = new List<SubUsage>(list.Count());
			this.subUsageList.AddRange(list.OrderBy(x => x.PriceItem != null ? (int)x.PriceItem.L7_Order : int.MaxValue).ThenBy(x => x.PriceItemCode));
		}
		List<SubUsage> subUsageList;

		/// <summary>
		/// Database level usage, stored on this SystemUsage
		/// </summary>
		/// <param name="subUsage"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddDbUsage(Tuple<SubUsage, List<EAdaptorUsage>> dbUsage)
		{
			if (dbUsageList == null)
			{
				dbUsageList = new List<Tuple<SubUsage, List<EAdaptorUsage>>>();
			}
			dbUsageList.Add(dbUsage);
		}
		List<Tuple<SubUsage, List<EAdaptorUsage>>> dbUsageList;

		public IEnumerable<SubUsage> DatabaseUsages
		{
			get { return dbUsageList != null ? dbUsageList.Select(x => x.Item1) : Enumerable.Empty<SubUsage>(); }
		}

		public decimal NonTransactionalAmount { get; private set; }

		public decimal NonTransactionalAmountWithoutDBUsageFee { get; private set; }

		#region Amounts

		public override void CalculateAmount()
		{
			amountCore = 0m;
			decimal licenceUnitAmount = 0;
			decimal transactionalAmount = 0;
			decimal nonTransactionalAmount = 0;
			decimal nonTransactionalAmountWithoutDBUsageFee = 0;

			var transactionalUsages = new List<SubUsage>();
			var nonTransactionalUsages = new List<SubUsage>();
			TransactionalUsages = transactionalUsages;
			NonTransactionalUsages = nonTransactionalUsages;

			foreach (var subUsage in subUsageList)
			{
				licenceUnitAmount += subUsage.LicenceUnitAmount;
				amountCore += subUsage.Amount;

				var priceItem = subUsage.PriceItem;
				if (priceItem != null)
				{
					if (priceItem.L7_FeeType == BillingConstants.FeeType.Transactional ||
						priceItem.L7_FeeType == BillingConstants.FeeType.TransactionalModule)
					{
						transactionalAmount += subUsage.Amount;
						transactionalUsages.Add(subUsage);
					}
					else if (BillingConstants.FeeType.IsPerLicence(priceItem.L7_FeeType))
					{
						nonTransactionalAmountWithoutDBUsageFee += subUsage.Amount;
						nonTransactionalAmount += subUsage.Amount;
						nonTransactionalUsages.Add(subUsage);
					}
				}
			}

			TransactionalAmount = Utilities.Round(transactionalAmount, BillingConstants.RoundingDecimals);
			LicenceUnitsAmount = licenceUnitAmount;
			amountCore = Utilities.Round(amountCore, BillingConstants.RoundingDecimals);

			if (dbUsageList != null)
			{
				nonTransactionalAmount += dbUsageList.Sum(x => x.Item1.Amount);
				nonTransactionalUsages.AddRange(dbUsageList.Select(x => x.Item1));
			}
			NonTransactionalAmount = Utilities.Round(nonTransactionalAmount, BillingConstants.RoundingDecimals);
			NonTransactionalAmountWithoutDBUsageFee = Utilities.Round(nonTransactionalAmountWithoutDBUsageFee, BillingConstants.RoundingDecimals);
		}

		public IEnumerable<SubUsage> TransactionalUsages { get; private set; } = Enumerable.Empty<SubUsage>();
		public IEnumerable<SubUsage> NonTransactionalUsages { get; private set; } = Enumerable.Empty<SubUsage>();

		/// <summary>
		/// Amount for transactions only - excludes the volume fee
		/// </summary>
		public ZDecimal TransactionalAmount { get; private set; }

		protected override ZDecimal AmountCore
		{
			get { return amountCore; }
		}
		ZDecimal amountCore;

		#endregion

		#region SubUsage

		public override SummarySection[] GetGeneralSummarySections()
		{
			var section = new SummarySection(Factory);
			var priceHeader = PriceHeader;

			foreach (var usage in subUsageList)
			{
				var line = section.Lines.AddNew();
				var priceItem = usage.PriceItem;
				line.MainDescription = priceItem != null ? (string)priceItem.L7_DescriptionLocalized : usage.PriceItemCode;
				line.UnitCount = usage.RawUsageCount.ToString(CultureInfo.CurrentCulture);
				line.LicenceUnits = usage.LicenceUnits.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.CurrentCulture);
				line.LicenceUnitsAmount = usage.LicenceUnitAmount.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.CurrentCulture);

				if (priceItem != null)
				{
					bool isIncluded = priceItem.L7_FeeType == BillingConstants.FeeType.Included;
					line.UnitPrice = !isIncluded ? priceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.CurrentCulture) : "Included";
					line.Amount = !isIncluded ? (usage.Amount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.CurrentCulture) : "";

					if (usage.IncludedUserCount != 0)
					{
						line.PurchasedCount = usage.IncludedUserCount.ToString(CultureInfo.InvariantCulture);
						line.TotalUnitCount = (priceItem.L7_UnitBreak * usage.IncludedUserCount).ToString(CultureInfo.CurrentCulture);
					}
				}
			}

			string currency = priceHeader != null ? (string)priceHeader.Currency.RX_Code : "";
			SummaryLine summaryHeader = section.Header;
			summaryHeader.MainDescription = "eAdaptor Interfaces";
			summaryHeader.LicenceUnits = "Licence Units";
			summaryHeader.LicenceUnitsAmount = "Total Licence Units";
			summaryHeader.UnitPrice = string.Format(CultureInfo.CurrentCulture, "Price\r\n({0})", currency);
			summaryHeader.UnitCount = "Count";
			summaryHeader.PurchasedCount = "Module Users";
			summaryHeader.TotalUnitCount = "Included Free";
			summaryHeader.Amount = string.Format(CultureInfo.CurrentCulture, "Total Price\r\n({0})", currency);
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.CurrentCulture);
			summaryHeader.TotalLicenceUnits = LicenceUnitsAmount.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.CurrentCulture);
			summaryHeader.ClientCompanyDescription = ClientCompanyDescription;

			return new[] { section };
		}

		public override bool HasLicenceUnits
		{
			get { return subUsageList.Any(x => x.LicenceUnits > 0); }
		}

		#endregion

		#region Validation

		public void ValidateUnitPrices(BusinessObject notificationOwner, string billingSystemDescription)
		{
			if (PriceHeader != null && subUsageList != null)
			{
				foreach (var subUsage in subUsageList)
				{
					if (subUsage.PriceItem == null)
					{
						notificationOwner.AddRowError(string.Format(CultureInfo.CurrentCulture, "{0}: No price found for code {1}, organization {2}", billingSystemDescription, subUsage.PriceItemCode, Organisation.OH_Code));
					}
				}
			}
		}

		#endregion
	}
}

