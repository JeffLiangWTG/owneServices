using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Registry.Testing;

sealed class AccountHelperTest : TestCaseWithFactory
{
	public void TestSplitCodeBySeparator()
	{
		CombineAssertions(() =>
		{
			AssertResult(AccountHelper.SplitCodeBySeparator(""), "", 0);
			AssertResult(AccountHelper.SplitCodeBySeparator("12345678"), "12345678", 0);
			AssertResult(AccountHelper.SplitCodeBySeparator("ABCD123"), "ABCD123", 0);
			AssertResult(AccountHelper.SplitCodeBySeparator("-ABC"), "", 0);
			AssertResult(AccountHelper.SplitCodeBySeparator("-123"), "", 123);
			AssertResult(AccountHelper.SplitCodeBySeparator("-00123"), "", 123);
			AssertResult(AccountHelper.SplitCodeBySeparator("ABCDER12-00123"), "ABCDER12", 123);
		});

		void AssertResult((ZString DeclarantTaxNumber, ZInt WorkstationSequentialNumber) result, ZString expectedDeclarantTaxNumber, ZInt expectedWorkstationSequentialNumber)
		{
			AssertEquals(expectedDeclarantTaxNumber, result.DeclarantTaxNumber);
			AssertEquals(expectedWorkstationSequentialNumber, result.WorkstationSequentialNumber);
		}
	}
}
