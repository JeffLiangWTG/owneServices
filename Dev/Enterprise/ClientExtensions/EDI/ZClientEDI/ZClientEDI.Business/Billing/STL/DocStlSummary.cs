using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentWrappers;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DocStlSummary : DocBaseWrapper
	{
		protected DocStlSummary(StlMonthlyUsage monthlyUsage, BusinessObjectFactory factory)
			: base(monthlyUsage, factory)
		{
			organisation = monthlyUsage.Bill.Organisation;
			itemLines = new UsageLineCollection(factory);
			borderWiseItemLines = new UsageLineCollection(factory);
			disbursementItemLines = new UsageLineCollection(factory);
			FormatOptions = BillingConstants.FormatOptions.GetOptionsByProduct(monthlyUsage.Database?.LD_Product, factory);

			Initialize();

			AddPriceItemSection();
			AddDiscountSummarySection();
			AddDepositSummarySection();
			AddFeeSection();

			AddPriceAndPaymentSummarySection();
			AddPrepaymentSummarySection();
			AddUsageSummarySection();
			AddCompanySummarySection();
		}

		DepositUseSummary depositSummary;

		void Initialize()
		{
			depositSummary = MonthlyUsage.DepositSummary;
			if (depositSummary == null)
			{
				// Standalone summary generation - not part of an invoice.
				// Need to generate some invoice information.

				using (BillingInvoicingHelper.BranchContext(Bill.InvoicingBranch.PK.ToGuid()))
				{
					var depositChargeCodeToBalance = StlBill.GetDepositChargeCodeToPositiveBalance(MonthlyUsage.InvoicedOrganisationPK);
					if (depositChargeCodeToBalance.Count > 0)
					{
						var lines = new List<SystemBill.BillLine>();
						MonthlyUsage.Bill.CreateInvoiceLines(MonthlyUsage, lines, depositChargeCodeToBalance);
						depositSummary = MonthlyUsage.DepositSummary;
					}
				}
			}
		}

		public static DocStlSummary New(StlMonthlyUsage monthlyUsage, BusinessObjectFactory factory)
		{
			return (monthlyUsage != null) ? new DocStlSummary(monthlyUsage, factory) : null;
		}

		StlMonthlyUsage MonthlyUsage
		{
			get { return (StlMonthlyUsage)WrappedObject; }
		}

		StlBill Bill
		{
			get { return MonthlyUsage.Bill; }
		}

		public UsageLineCollection ItemLines
		{
			get { return itemLines; }
		}
		readonly UsageLineCollection itemLines;

		public UsageLineCollection BorderWiseItemLines
		{
			get { return borderWiseItemLines; }
		}
		readonly UsageLineCollection borderWiseItemLines;

		public UsageLineCollection DisbursementItemLines
		{
			get { return disbursementItemLines; }
		}
		readonly UsageLineCollection disbursementItemLines;

		#region Summary Lines

		void AddPriceItemSection()
		{
			// Sort order here must match the order in the doc template or the price currency will show out of order.
			var orderedList = MonthlyUsage.UsageLinesInOrder.Where(x => !x.IsDisbursement).ToList();

			bool hasBorderWisePurchasedSeats = orderedList.Any(x => x.IsBorderWise && x.IncludedUnitCount != 0);

			if (!hasBorderWisePurchasedSeats)
			{
				ItemLines.AddRange(orderedList);
			}
			else
			{
				ItemLines.AddRange(orderedList.Where(x => !x.IsBorderWise));
				BorderWiseItemLines.AddRange(orderedList.Where(x => x.IsBorderWise));
			}

			ItemLines.AddRange(MonthlyUsage.CommitmentLines);
			DisbursementItemLines.AddRange(SplitDisbursementItemLines(MonthlyUsage.UsageLinesInOrder.Where(x => x.IsDisbursement)));

			SetFirstRolePriceCurrency(ItemLines);
			SetFirstRolePriceCurrency(BorderWiseItemLines);
			SetFirstRolePriceCurrency(DisbursementItemLines);
		}

		//Split the aggregated disbursement lines by company code.
		//Calculate the amount / unit by the actual percentage.
		IEnumerable<UsageLine> SplitDisbursementItemLines(IEnumerable<UsageLine> lines)
		{
			var result = new List<UsageLine>();
			foreach (var line in lines)
			{
				if (line.SingleUsage != null)
				{
					result.Add(line);
				}
				else
				{
					var totalPrice = line.Usages.Sum(x => x.ChargeableUsage.U1_TotalPrice);

					foreach (var group in line.Usages.GroupBy(x => x.CompanyCode))
					{
						var groupTotalPrice = group.Sum(x => x.ChargeableUsage.U1_TotalPrice);
						var groupUnitCount = group.Sum(x => x.ChargeableUsage.U1_UnitCount);

						var newUsageLine = new UsageLine(line.Factory, line.PeriodStart, line.PeriodStart, "", line.PriceItem,
															line.RoleParentItem, line.ModuleParentItem, line.FunctionParentItem);
						newUsageLine.AddUsage(group.Single());
						newUsageLine.FeeBasisText = line.FeeBasisText;
						newUsageLine.PriceCurrency = line.PriceCurrency;
						newUsageLine.Discounts = line.Discounts;
						newUsageLine.TotalUnitCount = groupUnitCount;
						newUsageLine.Direction = line.Direction;

						var unroundedPreDiscount = line.PreDiscountAmount * groupTotalPrice / totalPrice;
						var unroundedPostDiscount = line.PostDiscountAmount * groupTotalPrice / totalPrice;
						newUsageLine.SetAmounts(unroundedPreDiscount, unroundedPostDiscount);

						result.Add(newUsageLine);
					}
				}
			}

			return result.OrderBy(x => x.PriceItemDesc)
				.ThenBy(x => x.CompanyCode)
				.ThenBy(x => x.PriceCurrency);
		}

		static void SetFirstRolePriceCurrency(UsageLineCollection lines)
		{
			UsageLine prevLine = null;
			foreach (UsageLine line in lines)
			{
				if (prevLine == null || prevLine.PriceCurrency != line.PriceCurrency)
				{
					line.FirstRolePriceCurrency = line.PriceCurrency;
				}
				prevLine = line;
			}
		}

		public ZBool IsEmpty
		{
			get
			{
				return UsageSummaryLines.Count == 0
					&& DiscountSummaryLines.Count == 0
					&& DepositSummaryLines.Count == 0
					&& PaymentSummaryLines.Count == 0
					&& PrepaymentSummaryLines.Count == 0
					&& ItemLines.Count == 0
					&& BorderWiseItemLines.Count == 0
					&& DisbursementItemLines.Count == 0;
			}
		}

		void AddPriceAndPaymentSummarySection()
		{
			bool hasDepositUse = depositSummary != null && depositSummary.DepositUseLines.Any();
			var priceCurrencies = MonthlyUsage.PriceCurrencies;
			bool hasMultiplePriceCurrencies = priceCurrencies.Count() > 1;

			foreach (var currency in priceCurrencies.OrderBy(x => x))
			{
				var amounts = MonthlyUsage.GetAmountsForPriceCurrency(currency);
				var preDiscountAmount = amounts.TotalPreDiscountAmount;
				var postDiscountAmount = amounts.TotalPostDiscountAmount;
				var billSurchargeAmount = amounts.BillSurchargeAmount;
				var versionSurchargeAmount = amounts.VersionSurchargeAmount;
				var usageDiscountAmount = amounts.UsagePostDiscountAmount - amounts.UsagePreDiscountAmount;

				var finalAmount = postDiscountAmount + billSurchargeAmount + versionSurchargeAmount;
				var line = PriceSummaryLines.AddNew();
				line.Currency = currency;
				line.UndiscountedAmount = preDiscountAmount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture);
				line.DiscountAmount = usageDiscountAmount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture);

				if (billSurchargeAmount != 0m)
				{
					line.InvoiceSurchargeDescription = Res.GetString("03e852ae-78a8-4085-8dd4-a1f0339e9b9f", "Surcharge for {0}", Bill.SurchargeDescription?.ToString() ?? "");
					line.InvoiceSurchargeAmount = billSurchargeAmount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture);
				}

				if (versionSurchargeAmount != 0m)
				{
					line.VersionSurchargeDescription = EDIDataRegistry.Instance.VersionSurchargeDescription.Value
						+ " (" + MonthlyUsage.VersionSurchargeNonCurrentVersion.ToString() + ")"
						+ " (" + MonthlyUsage.VersionSurchargePercent.ToString("0.##", CultureInfo.InvariantCulture) + "%)";
					line.VersionSurchargeAmount = versionSurchargeAmount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture);
				}

				line.FinalAmount = finalAmount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture);

				if (!hasDepositUse || (MonthlyUsage.Bill.InvoiceCurrencyCode == currency && !hasMultiplePriceCurrencies))
				{
					var pay = PaymentSummaryLines.AddNew();
					pay.Currency = currency;
					decimal amount = postDiscountAmount + billSurchargeAmount + versionSurchargeAmount;
					if (depositSummary != null)
					{
						amount += depositSummary.DepositUseLines.Where(x => x.CurrencyCode == currency).Sum(x => x.ClosingBalance - x.OpeningBalance);
					}
					pay.FinalAmount = amount.ToString(FormatOptions.DecimalFormat, CultureInfo.InvariantCulture);
				}
			}

			if (PaymentSummaryLines.Count == 0)
			{
				// There is deposit use and multiple currencies
				// Yuk
			}
		}

		void AddFeeSection()
		{
			if (MonthlyUsage.FeeUsages == null)
			{
				return;
			}

			AddFeeSection(MonthlyUsage.FeeUsages.Where(x => x.DiscountAmount == 0), FeeLinesWithoutPrepay);
			AddFeeSection(MonthlyUsage.FeeUsages.Where(x => x.DiscountAmount != 0), FeeLinesWithPrepay);
		}

		void AddFeeSection(IEnumerable<FeeUsage> feeUsages, SummaryLineCollection lines)
		{
			foreach (var feesByCurrency in feeUsages.GroupBy(x => x.Currency).OrderBy(x => x.Key))
			{
				decimal total = 0;
				var header = new SummaryLine(Factory);
				header.Currency = feesByCurrency.Key;
				foreach (var fee in feesByCurrency.OrderBy(x => x.Fee.L8_Order))
				{
					var line = lines.AddNew();
					line.MainDescription = fee.Fee.L8_DescriptionMultilingual;
					line.Amount = fee.PreDiscountAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					line.FinalAmount = fee.PostDiscountAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					line.Currency = feesByCurrency.Key;
					line.Header = header;
					total += fee.PostDiscountAmount;
				}

				header.TotalAmount = total.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			}
		}

		void AddCompanySummarySection()
		{
			foreach (var clientCompany in MonthlyUsage.UsingClientCompanies)
			{
				var line = CompanySummaryLines.AddNew();
				line.MainDescription = clientCompany.LCC_Code;
				line.AdditionalDescription = clientCompany.LCC_Name;
			}
		}

		void AddPrepaymentSummarySection()
		{
			var bill = MonthlyUsage.Bill;
			var line = PrepaymentSummaryLines.AddNew();
			line.Currency = bill.InvoiceCurrencyCode;
			var prepay = bill.PrepayNext;
			line.Column1 = prepay.CurrentPrepaymentBalance.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			line.Column2 = prepay.PrepaymentBalanceRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			line.Column3 = EDIDataRegistry.Instance.RevisedPrepaymentBalanceDescription.Value;
			line.Column4 = prepay.FuturePrepaymentBalanceRequired != 0m ?
				prepay.FuturePrepaymentBalanceRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture) : "";

			var topUpRequired = Math.Max(new[] { prepay.FuturePrepaymentBalanceRequired, prepay.PrepaymentBalanceRequired }.FirstOrDefault(x => x != 0m) - prepay.CurrentPrepaymentBalance, 0m);
			line.Column5 = topUpRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			var totalPaymentRequired = topUpRequired + prepay.CurrentInvoiceTotalAmount;
			line.FinalAmount = totalPaymentRequired.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			line.Column10 = prepay.CurrentInvoiceTotalTaxAmount.ToString(CultureInfo.InvariantCulture);
		}

		void AddUsageSummarySection()
		{
			var line = UsageSummaryLines.AddNew();
			decimal used = MonthlyUsage.TotalUsedLicenceUnits;
			decimal commitment = MonthlyUsage.Commitment?.LicenceUnits ?? 0;
			decimal systemTotal = MonthlyUsage.TotalLicenceUnits;

			decimal buyingGroup = MonthlyUsage.BuyingGroup != null ? MonthlyUsage.BuyingGroup.TotalLicenceUnits - systemTotal : 0;

			decimal totalLicenceUnits = buyingGroup + systemTotal;

			line.Column1 = used.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture);
			if (commitment != 0)
			{
				line.Column2 = commitment.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture);
			}
			if (buyingGroup != 0)
			{
				line.Column3 = buyingGroup.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture);
			}
			if (MonthlyUsage.CommitmentGroup != null)
			{
				line.Column4 = MonthlyUsage.CommitmentGroup.TotalUsedLicenceUnits.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture);
			}
			line.FinalAmount = totalLicenceUnits.ToString(BillingConstants.AmountOneDecimalFormat, CultureInfo.InvariantCulture);
		}

		void AddDiscountSummarySection()
		{
			foreach (var usageLinesByDiscountKey in MonthlyUsage.UsageLines.Where(x => !x.DiscountKey.IsEmpty).GroupBy(x => x.DiscountKey).OrderBy(x => x.Key))
			{
				var line = DiscountSummaryLines.AddNew();
				var discountSet = usageLinesByDiscountKey.First().Discounts;
				line.MainDescription = discountSet.Key;
				line.Amount = discountSet.EffectivePercentText + '%';
				line.AdditionalDescription = discountSet.CombinedDescription;
			}
		}

		void AddDepositSummarySection()
		{
			if (depositSummary != null)
			{
				depositSummary.PopulateSummaryLines(DepositSummaryLines, Bill.InvoicingBranch);
			}
		}

		public PriceSummaryLineCollection PriceSummaryLines
			=> priceSummaryLines ?? (priceSummaryLines = new PriceSummaryLineCollection(Factory));
		PriceSummaryLineCollection priceSummaryLines;

		public SummaryLineCollection UsageSummaryLines
		{
			get { return usageSummaryLines ?? (usageSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection usageSummaryLines;

		public SummaryLineCollection DiscountSummaryLines
		{
			get { return discountSummaryLines ?? (discountSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection discountSummaryLines;

		public SummaryLineCollection DepositSummaryLines
		{
			get { return depositSummaryLines ?? (depositSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection depositSummaryLines;

		public SummaryLineCollection PaymentSummaryLines
		{
			get { return paymentSummaryLines ?? (paymentSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection paymentSummaryLines;

		public SummaryLineCollection PrepaymentSummaryLines
		{
			get { return prepaymentSummaryLines ?? (prepaymentSummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection prepaymentSummaryLines;

		public SummaryLineCollection CompanySummaryLines
		{
			get { return companySummaryLines ?? (companySummaryLines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection companySummaryLines;

		public SummaryLineCollection FeeLinesWithPrepay
		{
			get { return feeLinesWithPrepay ?? (feeLinesWithPrepay = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection feeLinesWithPrepay;

		public SummaryLineCollection FeeLinesWithoutPrepay
		{
			get { return feeLinesWithoutPrepay ?? (feeLinesWithoutPrepay = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection feeLinesWithoutPrepay;

		#endregion

		#region Properties

		public ZString EnterpriseCode
		{
			get { return MonthlyUsage.EnterpriseCode; }
		}

		public ZString ServerCode
		{
			get { return MonthlyUsage.ServerCode; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(organisation, Factory); }
		}
		readonly EDIOrgHeader organisation;

		public ZString Period
		{
			get { return Bill.PeriodStart.ToString("MMM yyyy", CultureInfo.InvariantCulture); }
		}

		public ZString InvoiceCurrency
		{
			get { return Bill.InvoiceCurrencyCode; }
		}

		readonly BillingConstants.FormatOptions FormatOptions;

		#endregion
	}
}

