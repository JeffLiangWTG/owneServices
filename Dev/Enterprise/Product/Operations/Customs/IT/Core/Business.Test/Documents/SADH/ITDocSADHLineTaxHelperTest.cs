using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITDocSADHLineTaxHelperTest : TestCaseWithFactory
{
	public void TestExcludeDutiesInTotals()
	{
		CombineAssertions("Exclude Duties In Totals", () =>
		{
			AssertEquals(true, ITDocSADHLineTaxHelper.ExcludeDutiesInTotals("O"));
			AssertEquals(true, ITDocSADHLineTaxHelper.ExcludeDutiesInTotals("R"));
			AssertEquals(true, ITDocSADHLineTaxHelper.ExcludeDutiesInTotals("S"));
			AssertEquals(true, ITDocSADHLineTaxHelper.ExcludeDutiesInTotals("U"));
			AssertEquals(true, ITDocSADHLineTaxHelper.ExcludeDutiesInTotals("V"));
		});

		AssertEquals("When not in [O, R, S, U, V]", false, ITDocSADHLineTaxHelper.ExcludeDutiesInTotals("G"));
	}
}
