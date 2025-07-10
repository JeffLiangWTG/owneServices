using System;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class AustraliaComplianceFinancialYear : IComplianceFinancialYear
	{
		public DateRange FinancialYearRange(int year)
		{
			bool isValidYear = year.ToString().Length == 4;

			if (!isValidYear)
			{
				throw new ArgumentOutOfRangeException(nameof(year), $"Invalid year {year}.");
			}

			return new DateRange(new DateTime(year - 1, 7, 1), new DateTime(year, 6, 30));
		}

		public int FinancialYear(DateTime dateInYear)
		{
			if (dateInYear.Month >= 7)
			{
				return dateInYear.Year + 1;
			}

			return dateInYear.Year;
		}
	}
}
