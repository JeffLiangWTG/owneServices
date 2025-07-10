using System;
using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IComplianceFinancialYear
	{
		DateRange FinancialYearRange(int year);
		int FinancialYear(DateTime dateInYear);
	}
}
