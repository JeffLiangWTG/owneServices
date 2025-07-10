using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Base.AccStatement.Testing
{
	public class BankTransferValidationTest : TestCaseWithFactory
	{
		public void TestValidateAll()
		{
			BankStatementFormat testBankStatementFormat = new BankStatementFormat(Factory);
			testBankStatementFormat.StatementFileFormatName = "";
			testBankStatementFormat.Validation.ValidateAll();

			AssertEquals(true, testBankStatementFormat.StatementFileFormatNameInfo.HasErrors());
		}

		public void TestValidateStatementFileFormatName()
		{
			BankStatementFormat testBankStatementFormat = new BankStatementFormat(Factory);

			testBankStatementFormat.StatementFileFormatName = "";
			AssertEquals(true, testBankStatementFormat.StatementFileFormatNameInfo.HasErrors());

			testBankStatementFormat.StatementFileFormatName = BankStatementFormat.StatementFileFormatNames.NABAustralia;
			AssertEquals(false, testBankStatementFormat.StatementFileFormatNameInfo.HasErrors());

			testBankStatementFormat.StatementFileFormatName = "NAB AUSTRALIA";
			AssertEquals(true, testBankStatementFormat.StatementFileFormatNameInfo.HasErrors());

			testBankStatementFormat.StatementFileFormatName = "abc";
			AssertEquals(true, testBankStatementFormat.StatementFileFormatNameInfo.HasErrors());
		}
	}
}