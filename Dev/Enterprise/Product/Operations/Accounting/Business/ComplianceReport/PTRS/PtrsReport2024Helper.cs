using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReport2024Helper : IPtrsReport2024Helper
	{
		const int OtherPartialPayment = 0;
		const int OtherFullPayment = 1;
		const int SmallBusinessPartialPayment = 2;
		const int SmallBusinessFullPayment = 3;
		const int DecimalPlaces = 2;

		public PtrsReport2024Data CalculatePtrsReport2024Data(AccComplianceReport complianceReport, PtrsAllPaymentsReport2024Data tcpData)
		{
			var reportData = new PtrsReport2024Data();

			var numberInvoicesPaidWithin30Days = ZInt.Zero;
			var numberInvoicesPaidBetween31And60Days = ZInt.Zero;
			var numberInvoicesPaidInMoreThan60Days = ZInt.Zero;
			var numberInvoicedPaidWithinPaymentTerms = ZInt.Zero;

			var paymentimesList = new List<int>();

			var paymentTermsList = new Dictionary<int, int>();

			foreach (var line in complianceReport.ReportLines.OfType<AccComplianceReportLine>())
			{
				var times = line.PreCalculatedCount;
				paymentimesList.Add(times);

				if (times <= 30)
				{
					numberInvoicesPaidWithin30Days++;
				}
				else if (times <= 60)
				{
					numberInvoicesPaidBetween31And60Days++;
				}
				else
				{
					numberInvoicesPaidInMoreThan60Days++;
				}

				if (line.PreCalculatedCount2 >= 0)
				{
					numberInvoicedPaidWithinPaymentTerms++;
				}

				var days = line.PreCalculatedCount3;
				if (paymentTermsList.ContainsKey(days))
				{
					paymentTermsList[days]++;
				}
				else
				{
					paymentTermsList.Add(days, 1);
				}
			}

			var totalNumberInvoicesPaid = numberInvoicesPaidWithin30Days + numberInvoicesPaidBetween31And60Days + numberInvoicesPaidInMoreThan60Days;

			if (totalNumberInvoicesPaid > 0)
			{
				paymentimesList.Sort();

				reportData.PercentagePaidWithin30days = RoundDecimalValue((ZDecimal)numberInvoicesPaidWithin30Days * 100m / (ZDecimal)totalNumberInvoicesPaid);
				reportData.PercentagePaidBetween31And60Days = RoundDecimalValue((ZDecimal)numberInvoicesPaidBetween31And60Days * 100m / (ZDecimal)totalNumberInvoicesPaid);
				reportData.PercentagePaidAfter60Days = RoundDecimalValue((ZDecimal)numberInvoicesPaidInMoreThan60Days * 100m / (ZDecimal)totalNumberInvoicesPaid);

				reportData.AveragePaymentTime = RoundDecimalValue(paymentimesList.Average());

				reportData.MedianPaymentTime = CalculateMedianValue(paymentimesList);

				reportData.PaymentTimeOf80thPercentile = GetPercentileValueFromList(paymentimesList, 80);

				reportData.PaymentTimeOf95thPercentile = GetPercentileValueFromList(paymentimesList, 95);

				reportData.PercentagePaidWithinTerm = RoundDecimalValue((ZDecimal)numberInvoicedPaidWithinPaymentTerms * 100m / (ZDecimal)totalNumberInvoicesPaid);

				var sortedList = paymentTermsList.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();
				if (sortedList.Count > 0)
				{
					reportData.MostCommonPaymentTerm = sortedList[0].Key;
					var validPaymentTermList = sortedList.Where(x => x.Key > 0).ToList();
					reportData.PaymentTermMin = validPaymentTermList.Min(x => x.Key);
					reportData.PaymentTermMax = validPaymentTermList.Max(x => x.Key);
				}
			}

			//Calculate SmallBusinessPaymentPercentage
			if (tcpData.AllPaymentAmount != 0m)
			{
				var percentage = tcpData.SmallBusinessPaymentAmount * 100m / tcpData.AllPaymentAmount;
				if ((percentage < 0.005m) && (tcpData.SmallBusinessPaymentAmount != 0m))
				{
					reportData.SmallBusinessPaymentPercentage = 0.01m;
				}
				else
				{
					reportData.SmallBusinessPaymentPercentage = RoundDecimalValue(percentage);
				}
			}

			return reportData;
		}

		public PtrsAllPaymentsReport2024Data CalculatePtrsAllPaymentsReport2024Data(AccComplianceReport complianceReport)
		{
			var reportData = new PtrsAllPaymentsReport2024Data();

			foreach (var line in complianceReport.ReportLines.OfType<AccComplianceReportLine>().Where(x => !x.PreCalculatedAmount.IsEmpty))
			{
				var flag = line.PreCalculatedCount;
				var amount = line.PreCalculatedAmount;

				switch (flag)
				{
					case OtherPartialPayment:
						reportData.OthersPartialPaymentAmount += amount;
						break;
					case OtherFullPayment:
						reportData.OthersFullPaymentAmount += amount;
						break;
					case SmallBusinessPartialPayment:
						reportData.SmallBusinessPartialPaymentAmount += amount;
						break;
					case SmallBusinessFullPayment:
						reportData.SmallBusinessFullPaymentAmount += amount;
						break;
				}
			}
			return reportData;
		}

		internal ZDecimal CalculateMedianValue(List<int> list)
		{
			ZDecimal value;
			if (list.Count == 0)
			{
				value = 0;
			}
			else if (list.Count % 2 == 1)
			{
				var index = list.Count / 2;
				value = list[index];
			}
			else
			{
				var index = list.Count / 2 - 1;
				value = RoundDecimalValue((ZDecimal)(list[index] + list[index + 1]) / 2);
			}
			return value;
		}

		internal int GetPercentileValueFromList(List<int> list, int percentile)
		{
			if ((percentile <= 0) || (percentile > 100))
			{
				throw new System.ArgumentOutOfRangeException(nameof(percentile));
			}
			var value = 0;
			if (list.Count > 0)
			{
				var precentileIndex = (list.Count - 1) * percentile / 100;
				value = list[precentileIndex];
			}
			return value; 
		}

		internal ZDecimal RoundDecimalValue(ZDecimal decimalValue)
		{
			return Utilities.Round(decimalValue, DecimalPlaces);
		}
	}
}
