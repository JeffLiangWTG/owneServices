using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class AustraliaComplianceFinancialYearTest : TestCase
	{
		public void TestValidFinancialYearRange()
		{
			var cfy = new AustraliaComplianceFinancialYear();
			var expected = new DateRange(new DateTime(2000, 7, 1), new DateTime(2001, 6, 30));
			var actual = cfy.FinancialYearRange(2001);
			AssertEquals(expected, actual);
		}

		public void TestInvalidFinancialYearRange()
		{
			var cfy = new AustraliaComplianceFinancialYear();

			AssertExceptionThrown<ArgumentOutOfRangeException>("200101", () => cfy.FinancialYearRange(200101));
			AssertExceptionThrown<ArgumentOutOfRangeException>("20010101", () => cfy.FinancialYearRange(20010101));
		}

		public void TestFinancialYear()
		{
			var cfy = new AustraliaComplianceFinancialYear();

			AssertEquals("1 July is following year", 2001, cfy.FinancialYear(new DateTime(2000, 7, 1)));
			AssertEquals("31 June is current year", 2000, cfy.FinancialYear(new DateTime(2000, 7, 1).AddSeconds(-1)));
			AssertEquals("1 Jan is current year", 2000, cfy.FinancialYear(new DateTime(2000, 1, 1)));
			AssertEquals("31 Dec is following year", 2001, cfy.FinancialYear(new DateTime(2000, 12, 31)));
		}
	}
}
