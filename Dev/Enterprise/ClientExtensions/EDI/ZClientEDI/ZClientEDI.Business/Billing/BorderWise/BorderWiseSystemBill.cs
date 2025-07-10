using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.BorderWise
{
	public class BorderWiseSystemBill : PriceItemBill, IOdplDiscountable
	{
		public BorderWiseSystemBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.BorderWise, factory)
		{
		}

		protected override void CalculateGroupAmounts()
		{
			Amount = DiscountCalculation.Amount;
			DiscountAmount = DiscountCalculation.DiscountAmount;
		}

		protected override void ValidateUnitPrice(BusinessObject notificationOwner)
		{
			base.ValidateUnitPricePerSubCode(notificationOwner);
		}

		#region Discounts

		public DiscountCalculation DiscountCalculation
		{
			get { return discountCalculation ?? (discountCalculation = CalculateDiscounts()); }
		}
		DiscountCalculation discountCalculation;

		OdplSystemBill combinedOdplBill;

		internal void CombineWith(OdplSystemBill odplBill)
		{
			Argument.NotNull(odplBill, nameof(odplBill));

			if (combinedOdplBill != null)
			{
				throw new InvalidOperationException("BorderWiseSystemBill is already combined with ODPL");
			}

			odplBill.AddExtraForVolumeDiscountBreak(SystemUsages.Sum(x => x.Amount), SystemUsages.Sum(x => x.LicenceUnitsAmount));
			combinedOdplBill = odplBill;
			ApplyCurrencyCode(odplBill.CurrencyCode);
			discountCalculation = null;
			CalculateGroupAmounts();
		}

		internal static ZDateTime DateForFixedCurrencyPerItem => new ZDateTime(2018, 7, 1);

		internal void ApplyCurrencyCode(string currencyCode)
		{
			foreach (UniversalPriceSystemUsage usage in SystemUsages)
			{
				if (usage.CurrencyCode.IsEmpty || usage.PeriodStart < DateForFixedCurrencyPerItem)
				{
					usage.ApplyCurrencyCode(currencyCode);
				}
			}
		}

		DiscountCalculation CalculateDiscounts()
		{
			var bwDiscounts = OdplDiscountCalculator.GetDiscounts(this);
			DiscountCalculation result = OdplDiscountCalculator.Calculate(bwDiscounts, this, SystemUsages[0].PriceHeader?.L6_LicenceUnitRate);

			if (result.DiscountAmount != result.Amount)
			{
				AddOdplDiscounts(result);

				if (result.DiscountAmount > result.Amount)
				{
					result.ErrorDescriptions = result.ErrorDescriptions.Concat(new ZString[] { "Discount exceeds amount" }).ToArray();
				}
			}
			else
			{
				// 100% discount
			}

			return result;
		}

		void AddOdplDiscounts(DiscountCalculation result)
		{
			if (combinedOdplBill != null && combinedOdplBill.DiscountCalculation.Details.Any())
			{
				var amountToDiscount = result.TotalAmount;
				var discountList = BillingConstants.GetDiscountTypeList();

				var odplCalculation = combinedOdplBill.DiscountCalculation;
				foreach (var detail in odplCalculation.Details.Where(x => x.DiscountType != BillingConstants.DiscountType.ModuleSpecific))
				{
					var calc = new DiscountCalculation();
					calc.Amount = amountToDiscount;
					calc.DiscountAmount = Utilities.Round(amountToDiscount * detail.DiscountPercent / 100m, BillingConstants.RoundingDecimals);
					var typeDescription = discountList.GetDescriptionFromCode(detail.DiscountType);
					var description = BillingConstants.BillingSystem.Descriptions.ODM
						+ " " + typeDescription
						+ " Discount: "
						+ (-calc.DiscountAmount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture)
						+ " (" + (-detail.DiscountPercent).ToString(CultureInfo.InvariantCulture) + "% * "
						+ calc.Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture)
						+ ")";
					calc.SetDiscountDetails(description, detail.DiscountType, calc.DiscountAmount, detail.DiscountPercent);
					result.MergeDiscountWith(calc);
				}
			}
		}

		#endregion

		#region IOdplDiscountable

		ZDecimal IOdplDiscountable.LicenceUnitsToDiscount => SystemUsages.Sum(x => x.LicenceUnitsAmount);
		int IOdplDiscountable.CoreOnDemandUsers => 0;
		ZDecimal IOdplDiscountable.MixedAmountAsMoney => SystemUsages.Sum(x => x.Amount);
		ZDecimal IOdplDiscountable.MixedAmountAsLicenceUnits => SystemUsages.Sum(x => x.LicenceUnitsAmount);
		ZBool IOdplDiscountable.IsHostedOnWiseCloud => false;
		ZString IDiscountable.SystemCode => SystemCode;
		ZDecimal IDiscountable.AmountToDiscount => SystemUsages.Sum(x => x.Amount);

		ZDecimal IOdplDiscountable.AmountToDiscountForModule(string moduleCode, out string moduleName)
		{
			ZDecimal amount = 0m;
			moduleName = "";

			foreach (var usage in SystemUsages.Cast<UniversalPriceSystemUsage>())
			{
				if (usage.SubCode == moduleCode)
				{
					amount += usage.Amount;
					moduleName = usage.PriceItem?.L7_DescriptionLocalized.Trim().ToString() ?? "";
				}
			}

			return amount;
		}

		#endregion

		#region Summary

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			var organisationUsages = SystemUsages.Where(x => x.OrganisationPK == organisationPK).ToArray();
			var resultSection = BuildSingleSummarySection(organisationUsages, checkForSubCode: false, usageFilter: null, includeLicenceUnits: true);
			return resultSection != null ? new SummarySection[] { resultSection } : Array.Empty<SummarySection>();
		}

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetSinglePeriodDiscountSummarySections(DiscountCalculation);
		}

		public override SummarySection[] GetGroupSummarySections()
		{
			var result = BuildGroupSummarySectionOneLinePerOrg(HasLicenceUnits);
			return new SummarySection[] { result };
		}

		#endregion

		#region Invoice

		public override ZString GetAmountChargeCodeName(SystemUsage systemUsage)
		{
			var usage = (UniversalPriceSystemUsage)systemUsage;
			return usage.PriceItem?.L7_ChargeCode ?? ZString.Empty;
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage systemUsage)
		{
			var usage = (UniversalPriceSystemUsage)systemUsage;
			return usage.PriceItem?.L7_DiscountChargeCode ?? ZString.Empty;
		}

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var totalWeight = SystemUsages.Sum(x => x.Amount);
			if (totalWeight == 0)
			{
				return;
			}

			var usages = SystemUsages.Cast<UniversalPriceSystemUsage>().Where(x => Amount != 0);
			decimal amountRemaining = Amount;
			decimal discountRemaining = DiscountAmount;

			var chargeGroups = usages
				.GroupBy(x => new
				{
					AmountChargeCode = (string)GetAmountChargeCodeName(x),
					DiscountChargeCode = DiscountAmount != 0 ? (string)GetDiscountChargeCodeName(x) : ""
				})
				.OrderBy(x => x.First().PriceItem.L7_Order)
				.ToList();

			var discountLines = new List<BillLine>();
			for (int i = 0; i < chargeGroups.Count; ++i)
			{
				var chargeGroup = chargeGroups[i];
				bool isLastGroup = (i == chargeGroups.Count - 1);

				var chargeGroupWeight = chargeGroup.Sum(x => x.Amount);
				var weightedAmount = isLastGroup
									? amountRemaining
									: chargeGroupWeight * Amount / totalWeight;
				var weightedDiscount = isLastGroup
									? discountRemaining
									: chargeGroupWeight * DiscountAmount / totalWeight;

				if (weightedAmount != 0)
				{
					var taxGroupWeights = BuildTaxGroupWeights(chargeGroup, false);
					amountRemaining -= weightedAmount;
					discountRemaining -= weightedDiscount;

					CreateProRataLines(lines, taxGroupWeights, totalWeight, weightedAmount, chargeGroup.Key.AmountChargeCode, "");

					if (weightedDiscount != 0)
					{
						CreateProRataLines(discountLines, taxGroupWeights, totalWeight, -weightedDiscount, chargeGroup.Key.DiscountChargeCode, "");
					}
				}
			}

			lines.AddRange(discountLines);
		}
	}

	#endregion
}

