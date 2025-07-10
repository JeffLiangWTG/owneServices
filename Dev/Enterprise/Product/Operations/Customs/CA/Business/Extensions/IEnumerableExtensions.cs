using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business
{
	public static class IEnumerableExtensions
	{
		public static ZString ConcatenateWithPageDelimiter(this IEnumerable<ZString> listOfString, string delimiter, int maximumNoOfCharsOnFirstPage, int maximumNoOfCharsOnContinuationPage)
		{
			var result = new ZStringBuilder();
			var noOfPages = 1;
			var noOfStringsProccessed = 0;
			var noOfStrings = listOfString.Count();
			var page = new ZStringBuilder();
			foreach (var aString in listOfString.Where(x => !x.IsEmpty))
			{
				noOfStringsProccessed++;
				var currentPageLength = page.ToStringWithDelimiterBetweenAppends(delimiter).Length;
				var maximumNoOfCharsOnPage = noOfPages == 1 ? maximumNoOfCharsOnFirstPage : maximumNoOfCharsOnContinuationPage;
				if (currentPageLength > 0 && currentPageLength + aString.Length + 1 > maximumNoOfCharsOnPage)
				{
					if (noOfPages > 1)
					{
						result.Append("*" + (noOfPages - 1) + "*"); // page delimiter
					}
					result.Append(page.ToStringWithDelimiterBetweenAppends(delimiter));
					page = new ZStringBuilder();
					noOfPages++;
				}
				page.Append(aString);
			}
			if (page.Length > 0)
			{
				if (noOfPages > 1)
				{
					result.Append("*" + (noOfPages - 1) + "*"); // page delimiter
				}
				result.Append(page.ToStringWithDelimiterBetweenAppends(delimiter));
			}

			return result.ToString();
		}

		public static ITotalAmounts GetEmptyTotalAmounts(this IEnumerable<IClassificationLine1> classificationLines)
		{
			return new TotalAmounts();
		}

		public static ITotalAmounts GetTotalAmounts(this IEnumerable<IClassificationLine1> classificationLines, ZString recordIdentifier, ZDecimal deposit)
		{
			var totalAmounts = new TotalAmounts();
			totalAmounts.Deposit = deposit;
			totalAmounts.TotalCustomsDuty = totalAmounts.Deposit;

			if (classificationLines != null && classificationLines.Any())
			{
				foreach (var line1 in classificationLines)
				{
					if (line1.RecordIdentifier == recordIdentifier)
					{
						totalAmounts.TotalExciseTax += line1.ExciseTaxAmount;
						totalAmounts.TotalGST += line1.GSTAmount;
						totalAmounts.TotalSIMAAssessment += IDutyAndTaxDataExtensions.IsSimaAmountPayable(line1.SIMACode) ? line1.SIMAAssessment : ZDecimal.Zero;
						totalAmounts.TotalCustomsDuty += line1.ClassificationLines.Sum(l => l.CustomsDutyAmount);
					}
				}

				if (recordIdentifier == MessageConstants.B3RecordIdentifiers.Negative)
				{
					const int multiplier = -1;
					totalAmounts.TotalExciseTax *= multiplier;
					totalAmounts.TotalGST *= multiplier;
					totalAmounts.TotalCustomsDuty *= multiplier;
					totalAmounts.TotalSIMAAssessment = 0;
				}
			}
			return totalAmounts;
		}
	}
}
