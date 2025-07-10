using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class AirlineMessagingBill : TransactionalWithSubCodeSystemBill
	{
		public AirlineMessagingBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.AirlineMessaging, factory)
		{
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return GetChargeCodeName((AirlineMessagingUsage)usage);
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			return EDIDataRegistry.Instance.AirlineMessagingDiscountAndRemitChargeCode.Value;
		}

		ZString FWBChargeCodeName
		{
			get { return EDIDataRegistry.Instance.AirlineMessagingFWBChargeCode.Value; }
		}

		ZString FHLChargeCodeName
		{
			get { return EDIDataRegistry.Instance.AirlineMessagingFHLChargeCode.Value; }
		}

		ZString FSUChargeCodeName
		{
			get { return EDIDataRegistry.Instance.AirlineMessagingFSUChargeCode.Value; }
		}

		ZString RemitableChargeCodeName
		{
			get { return GetDiscountChargeCodeName(null); }
		}

		#endregion

		#region Calculate Group Amounts

		protected override void CalculateGroupAmounts()
		{
			CalculateDiscountsAndSurcharges();
			Amount = DiscountWithProviderCalculations.Values.Sum(x => x.Amount);
			DiscountAmount = DiscountWithProviderCalculations.Values.Sum(x => x.DiscountAmount);
			SurchargeAmount = -SurchargeWithProviderCalculations.Values.Sum(x => x.DiscountAmount);
		}

		void CalculateDiscountsAndSurcharges()
		{
			DiscountWithProviderCalculations.Clear();
			SurchargeWithProviderCalculations.Clear();
			var allDiscounts = new List<ClientLicenceBillingDiscount>();
			var localDiscounts = Discounts;

			var airlineUsages = SystemUsages.Cast<AirlineMessagingUsage>();

			foreach (var usagesGroupedByMonth in airlineUsages.GroupBy(x => x.PeriodStart))
			{
				foreach (var usagesGroupedByProvider in usagesGroupedByMonth.Where(x => !x.ProviderCodeForDiscount.IsEmpty).GroupBy(x => x.ProviderCodeForDiscount))
				{
					var firstDayOfMonth = usagesGroupedByMonth.Key;
					var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
					var firstUsage = usagesGroupedByProvider.First();
					var prices = firstUsage.PriceHeader;
					allDiscounts.Clear();
					if (prices != null)
					{
						allDiscounts.AddRange(prices.Discounts);
					}
					if (localDiscounts != null)
					{
						allDiscounts.AddRange(localDiscounts);
					}

					amountToDiscount = usagesGroupedByProvider.Sum(x => x.Amount);
					ZInt unitCount = usagesGroupedByProvider.Where(x => x.UnitPrice > 0).Sum(x => x.UnitCount);
					ZDecimal unitPrice = firstUsage.UnitPrice;

					DiscountCalculation discountCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalDiscount(allDiscounts, this, unitCount, unitPrice, firstDayOfMonth, lastDayOfMonth, usagesGroupedByProvider.Key);
					DiscountWithProviderCalculations.Add(new Tuple<ZDateTime, ZString, ZString>(firstDayOfMonth, firstUsage.ProviderCodeForDiscount, firstUsage.ProviderName), discountCalculation);
					DiscountCalculationsToSystemUsages.Add(discountCalculation, usagesGroupedByProvider.ToArray());

					DiscountCalculation surchargeCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalSurcharge(allDiscounts, this, discountCalculation.Amount, firstDayOfMonth, lastDayOfMonth, usagesGroupedByProvider.Key);
					SurchargeWithProviderCalculations.Add(new Tuple<ZDateTime, ZString, ZString>(firstDayOfMonth, firstUsage.ProviderCodeForDiscount, firstUsage.ProviderName), surchargeCalculation);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Dictionary<Tuple<ZDateTime, ZString, ZString>, DiscountCalculation> DiscountWithProviderCalculations
		{
			get { return discountWithProviderCalculations ?? (discountWithProviderCalculations = new Dictionary<Tuple<ZDateTime, ZString, ZString>, DiscountCalculation>()); }
		}
		Dictionary<Tuple<ZDateTime, ZString, ZString>, DiscountCalculation> discountWithProviderCalculations;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Dictionary<Tuple<ZDateTime, ZString, ZString>, DiscountCalculation> SurchargeWithProviderCalculations
		{
			get { return surchargeWithProviderCalculations ?? (surchargeWithProviderCalculations = new Dictionary<Tuple<ZDateTime, ZString, ZString>, DiscountCalculation>()); }
		}
		Dictionary<Tuple<ZDateTime, ZString, ZString>, DiscountCalculation> surchargeWithProviderCalculations;

		protected override IEnumerable<DiscountDetailedInfo> DiscountDetailedInfos => DiscountWithProviderCalculations.Values.SelectMany(x => x.Details);
		protected override IEnumerable<DiscountDetailedInfo> SurchargeDetailedInfos => SurchargeWithProviderCalculations.Values.SelectMany(x => x.Details);

		public override IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution()
		{
			var result = new List<SystemMinimumFee>(1);

			var minimumFeeContribution = new Dictionary<Tuple<ZGuid, ZDateTime>, ZDecimal>();
			foreach (var periodDiscountCalculation in DiscountWithProviderCalculations)
			{
				var period = periodDiscountCalculation.Key.Item1;
				SystemUsage[] usages = null;

				if (DiscountCalculationsToSystemUsages.TryGetValue(periodDiscountCalculation.Value, out usages))
				{
					var usageCount = usages.Sum(x => x.UnitCount);
					var amount = periodDiscountCalculation.Value.Amount;
					var discountAmount = periodDiscountCalculation.Value.DiscountAmount;
					var surchargeAmount = -SurchargeWithProviderCalculations[periodDiscountCalculation.Key].DiscountAmount;

					if (usageCount > 0)
					{
						foreach (var usagesGroupedByDatabase in usages.Where(x => !x.User.DatabasePK.IsEmpty).GroupBy(x => x.User.DatabasePK))
						{
							var ratio = usagesGroupedByDatabase.Sum(x => x.UnitCount) / (decimal)usageCount;
							var dbContribution = (amount - discountAmount + surchargeAmount) * ratio;

							var periodDatabaseKey = new Tuple<ZGuid, ZDateTime>(usagesGroupedByDatabase.Key, period);
							if (minimumFeeContribution.ContainsKey(periodDatabaseKey))
							{
								minimumFeeContribution[periodDatabaseKey] += dbContribution;
							}
							else
							{
								minimumFeeContribution[periodDatabaseKey] = dbContribution;
							}
						}
					}
				}
			}

			foreach (var contribution in minimumFeeContribution)
			{
				var minimumFee = new SystemMinimumFee(contribution.Key.Item1, contribution.Key.Item2, contribution.Value, CurrencyCode, "");
				result.Add(minimumFee);
			}

			return result;
		}

		#endregion

		#region Summary Sections

		protected override PriceItemUsage[] GetGeneralSummarySectionsUsages(ZGuid organisationPK)
		{
			var usages = SystemUsages.Cast<AirlineMessagingUsage>().Where(x => x.OrganisationPK == organisationPK && (x.IsNonChargeableTraxonUsage || x.Amount != 0)).ToArray();
			Array.Sort(usages, new AirlineMessagingUsageComparer());
			return usages;
		}

		protected override IEnumerable<Predicate<PriceItemUsage>> GetGeneralSummarySectionsUsageFilters()
		{
			foreach (var usagesProviderGroup in SystemUsages.GroupBy(u => ((AirlineMessagingUsage)u).ProviderName))
			{
				yield return (u => !((AirlineMessagingUsage)u).IsRemitUsage && !((AirlineMessagingUsage)u).IsNonChargeableTraxonUsage && ((AirlineMessagingUsage)u).ProviderName == usagesProviderGroup.Key);
				yield return (u => ((AirlineMessagingUsage)u).IsRemitUsage);
				yield return (u => ((AirlineMessagingUsage)u).IsNonChargeableTraxonUsage);
			}
		}

		class AirlineMessagingUsageComparer : IComparer<AirlineMessagingUsage>
		{
			public int Compare(AirlineMessagingUsage x, AirlineMessagingUsage y)
			{
				var providerX = x.SubCode.SubstringSafe(1, 1);
				var providerY = y.SubCode.SubstringSafe(1, 1);
				if (providerX != providerY)
				{
					if (providerX == "R") { return 1; }
					else if (providerY == "R") { return -1; }
					else if (providerX == "E") { return 1; }
					else if (providerY == "E") { return -1; }
					else { return providerX.CompareTo(providerY); }
				}

				if (x.Organisation.OH_Code != y.Organisation.OH_Code)
				{
					return x.Organisation.OH_Code.CompareTo(y.Organisation.OH_Code);
				}

				if (x.PeriodStart != y.PeriodStart)
				{
					return x.PeriodStart.CompareTo(y.PeriodStart);
				}

				if (x.IsRemitUsage != y.IsRemitUsage)
				{
					return x.IsRemitUsage ? 1 : -1;
				}

				var billingTypeX = x.SubCode.SubstringSafe(2, 1);
				var billingTypeY = y.SubCode.SubstringSafe(2, 1);
				if (billingTypeX != billingTypeY)
				{
					if (billingTypeX == "N") { return 1; }
					else if (billingTypeY == "N") { return -1; }
					else { return billingTypeX.CompareTo(billingTypeY); }
				}

				var messageTypeX = x.SubCode.SubstringSafe(0, 1);
				var messageTypeY = y.SubCode.SubstringSafe(0, 1);
				return GetMessageTypeOrder(messageTypeX).CompareTo(GetMessageTypeOrder(messageTypeY));
			}

			int GetMessageTypeOrder(ZString messageType)
			{
				if (messageType == "W") { return 1; }
				else if (messageType == "H") { return 2; }
				else if (messageType == "S") { return 3; }
				else { return 4; }
			}
		}

		#endregion

		#region Group Summary

		public override SummarySection[] GetGroupSummarySections()
		{
			var result = new List<SummarySection>(1);
			var options = GetGroupSummaryOptions();

			var usageProviderGroups = SystemUsages.Cast<AirlineMessagingUsage>().OrderBy(x => x.ProviderName).GroupBy(x => x.ProviderName);
			foreach (var providerGroup in usageProviderGroups)
			{
				var section = new SummarySection(Factory);
				section.Header.MainDescription = SystemDescription + " Group Summary - " + providerGroup.Key;
				ZDecimal totalAmount = 0m;

				var usageGroups = providerGroup.OrderBy(x => x.Organisation.OH_Code).ThenBy(x => x.PeriodStart).GroupBy(x => new { x.Organisation.OH_Code, x.LicenceNineCode, x.PeriodStartAsText });
				foreach (var group in usageGroups)
				{
					ZDecimal usageAmount = group.Sum(x => x.Amount);
					if (usageAmount != 0m)
					{
						SummaryLine groupLine = section.Lines.AddNew();
						groupLine.MainDescription = group.Key.OH_Code;
						groupLine.Amount = usageAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
						BuildSummaryLine(groupLine, options, group.First());
						totalAmount += usageAmount;
					}
				}

				section.Header.TotalAmount = totalAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				section.Header.AmountDescription = ZString.Format("Total Price ({0})", CurrencyCode);
				section.Header.TotalDescription = ZString.Format("Total {0} ({1})", providerGroup.Key, CurrencyCode);

				result.Add(section);
			}

			return result.ToArray();
		}

		#endregion

		#region Discount Summary

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetDiscountOrSurchargeSummarySections(DiscountWithProviderCalculations);
		}

		public override SummarySection[] GetSurchargeSummarySections()
		{
			return GetDiscountOrSurchargeSummarySections(SurchargeWithProviderCalculations);
		}

		SummarySection[] GetDiscountOrSurchargeSummarySections(Dictionary<Tuple<ZDateTime, ZString, ZString>, DiscountCalculation> discountOrSurchargeCalculations)
		{
			if (discountOrSurchargeCalculations.Count > 0 && discountOrSurchargeCalculations.Values.Any(x => x.DiscountDescriptions.Any()))
			{
				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = SystemDescription + " Amount Calculations Applied";

				foreach (var periodProviderDiscountCalculation in discountOrSurchargeCalculations)
				{
					foreach (ZString discountDescription in periodProviderDiscountCalculation.Value.DiscountDescriptions)
					{
						SummaryLine summaryLine = result.Lines.AddNew();
						summaryLine.MainDescription = periodProviderDiscountCalculation.Key.Item3 + " - " + discountDescription;

						if (HasUsagesForEarlierPeriods)
						{
							ZString periodAsString = periodProviderDiscountCalculation.Key.Item1.ToString("MMM yyyy", CultureInfo.InvariantCulture);
							summaryLine.MainDescription = periodAsString + " " + summaryLine.MainDescription;
						}
					}
				}

				return new SummarySection[] { result };
			}

			return Array.Empty<SummarySection>();
		}

		#endregion

		#region Invoice

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var usages = SystemUsages.Cast<AirlineMessagingUsage>().ToArray();
			Array.Sort(usages, new AirlineMessagingUsageComparer());

			CreateChargeLines(lines, dateForExchangeRate, invoice, usages);
			CreateDiscountAndSurchargeLines(lines, dateForExchangeRate, invoice, usages);

			if (lines.Count > 0)
			{
				lines.Sort(new AirlineMessagingBillLineComparer());

				string lastProviderName = "";
				string lastTaxCode = "";
				for (int i = lines.Count - 1; i >= 0; i--)
				{
					var line = lines[i];
					line.Sequence = i;
					string providerName = GetProviderNameFromInvoiceLineDescription(line.Description);
					line.RequireCommentLineAfter = providerName != lastProviderName || line.Tax.Code != lastTaxCode;
					lastProviderName = providerName;
					lastTaxCode = line.Tax.Code;
				}
			}
		}

		void CreateChargeLines(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice, AirlineMessagingUsage[] usages)
		{
			foreach (var usageTaxGroup in usages.GroupBy(x => new TaxGroup(x.InvoiceDelivery)))
			{
				foreach (var usageGroup in usageTaxGroup.GroupBy(u => u.SubCode + u.CurrencyCode + u.UnitPrice.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.InvariantCulture)))
				{
					var usage = usageGroup.First();
					var groupUnitCount = usageGroup.Sum(u => u.UnitCount);
					var groupAmount = usageGroup.Sum(u => u.Amount);

					if (groupAmount != 0)
					{
						var line = new BillLine(groupAmount, usageTaxGroup.Key, CurrencyCode, GetChargeCodeName(usage), GetInvoiceDescription(usage, groupUnitCount), lines.Count, SystemCode);
						if (invoice != null)
						{
							line.AmountInInvoiceCurrency = BillingInvoicingHelper.GetAmountInInvoiceCurrency(line.Amount, dateForExchangeRate, line.CurrencyCode, invoice.TransactionCurrency.RX_Code, invoice.Branch, invoice.Branch.Factory);
						}
						lines.Add(line);
					}
				}
			}
		}

		void CreateDiscountAndSurchargeLines(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice, AirlineMessagingUsage[] usages)
		{
			var usagesByProvider = usages.GroupBy(u => new { u.ProviderCodeForDiscount, u.ProviderName });
			foreach (var usagesProviderGroup in usagesByProvider)
			{
				decimal totalWeight = 0m;
				Dictionary<TaxGroup, Dictionary<ZString, ZDecimal>> weightMap = new Dictionary<TaxGroup, Dictionary<ZString, ZDecimal>>();
				foreach (var usageTaxGroup in usagesProviderGroup.GroupBy(x => new TaxGroup(x.InvoiceDelivery)))
				{
					var subWeightMap = new Dictionary<ZString, ZDecimal>();
					weightMap.Add(usageTaxGroup.Key, subWeightMap);
					foreach (var usageSubCodeGroup in usageTaxGroup.GroupBy(y => y.SubCode))
					{
						var weight = usageSubCodeGroup.Sum(z => z.Amount);
						subWeightMap.Add(usageSubCodeGroup.Key, weight);
						totalWeight += weight;
					}
				}

				var providerName = usagesProviderGroup.Key.ProviderName;

				var discountCalculation = DiscountWithProviderCalculations[new Tuple<ZDateTime, ZString, ZString>(PeriodStart, usagesProviderGroup.Key.ProviderCodeForDiscount, providerName)];
				CreateDiscountAndSurchargeLinesCore(lines, dateForExchangeRate, invoice, weightMap, totalWeight, discountCalculation, "Airline Messaging Discount - " + providerName, GetDiscountChargeCodeName(null));

				var surchargeCalculation = SurchargeWithProviderCalculations[new Tuple<ZDateTime, ZString, ZString>(PeriodStart, usagesProviderGroup.Key.ProviderCodeForDiscount, providerName)];
				CreateDiscountAndSurchargeLinesCore(lines, dateForExchangeRate, invoice, weightMap, totalWeight, surchargeCalculation, "Airline Messaging Surcharge - " + providerName, GetSurchargeChargeCodeName());
			}
		}

		void CreateDiscountAndSurchargeLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice, Dictionary<TaxGroup, Dictionary<ZString, ZDecimal>> weightMap, decimal totalWeight, DiscountCalculation discountCalculation, ZString lineDescription, ZString chargeCodeName)
		{
			if (discountCalculation != null && discountCalculation.DiscountAmount != 0)
			{
				var discountAmount = -discountCalculation.DiscountAmount;

				if (discountAmount != 0)
				{
					var discountAmountRemaining = discountAmount;
					var weightMapKeys = weightMap.Keys.ToList();
					for (int i = 0; i < weightMapKeys.Count; ++i)
					{
						var tax = weightMapKeys[i];
						var subWeightMap = weightMap[tax];

						decimal taxGroupAmount = 0m;
						decimal taxGroupAmountInInvoiceCurrency = 0m;
						var subWeightMapKeys = subWeightMap.Keys.ToList();
						for (int j = 0; j < subWeightMapKeys.Count; ++j)
						{
							var subCodeGroupWeight = subWeightMap[subWeightMapKeys[j]];
							decimal subCodeGroupAmount = (i == weightMapKeys.Count - 1 && j == subWeightMapKeys.Count - 1)
								? discountAmountRemaining
								: Utilities.Round(discountAmount * subCodeGroupWeight / totalWeight, BillingConstants.RoundingDecimals);

							taxGroupAmount += subCodeGroupAmount;
							if (invoice != null)
							{
								taxGroupAmountInInvoiceCurrency += BillingInvoicingHelper.GetAmountInInvoiceCurrency(subCodeGroupAmount, dateForExchangeRate, CurrencyCode, invoice.TransactionCurrency.RX_Code, invoice.Branch, invoice.Branch.Factory);
							}
							discountAmountRemaining -= subCodeGroupAmount;
						}

						var line = new BillLine(taxGroupAmount, tax, CurrencyCode, chargeCodeName, lineDescription, lines.Count, SystemCode, ZGuid.Empty);
						line.AmountInInvoiceCurrency = taxGroupAmountInInvoiceCurrency;
						lines.Add(line);
					}
				}
			}
		}

		public ZString GetChargeCodeName(AirlineMessagingUsage usage)
		{
			if (usage.IsRemitUsage) { return RemitableChargeCodeName; }
			if (usage.IsFWBUsage) { return FWBChargeCodeName; }
			if (usage.IsFHLUsage) { return FHLChargeCodeName; }
			if (usage.IsFSUUsage) { return FSUChargeCodeName; }
			else
			{
				return base.GetAmountChargeCodeName(usage);
			}
		}

		public ZString GetInvoiceDescription(AirlineMessagingUsage usage, ZInt unitCount)
		{
			ZString result = usage.IsRemitUsage ? "Remit: " : "";
			if (usage.IsFWBUsage) { result += "FWB Airline Messaging - "; }
			if (usage.IsFHLUsage) { result += "FHL Airline Messaging - "; }
			if (usage.IsFSUUsage) { result += "FSU Airline Messaging - "; }

			result += usage.ProviderName + usage.ProviderChargeInfo;
			if (usage.PriceItem != null)
			{
				result += " - " + unitCount + " transactions ";
				result += "at " + usage.CurrencyCode + " " + Math.Abs(usage.PriceItem.L7_Price).ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.InvariantCulture) + " per transaction";
			}

			return result;
		}

		static ZString GetProviderNameFromInvoiceLineDescription(ZString lineDescription)
		{
			return lineDescription.IndexOf('-') != -1
				&& !lineDescription.EndsWith("-", StringComparison.Ordinal)
				? lineDescription.Split('-')[1].Trim()
				: ZString.Empty;
		}

		class AirlineMessagingBillLineComparer : IComparer<BillLine>
		{
			public int Compare(BillLine x, BillLine y)
			{
				if (x.Tax.Code != y.Tax.Code)
				{
					return x.Tax.Code.CompareTo(y.Tax.Code);
				}

				var providerX = GetProviderNameFromInvoiceLineDescription(x.Description);
				var providerY = GetProviderNameFromInvoiceLineDescription(y.Description);
				return providerX.CompareTo(providerY);
			}
		}

		#endregion
	}
}

