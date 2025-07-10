using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_Validation_BankAccountTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "PAY", null, 100, transactionNumber: "00001099");

			var errorMessage = RunAndAssertAggregationWithError();
			var expected =
@"Cash Book Transactions With No Bank Account Have Been Found:
	AP	PAY	00001099
";
			AssertEquals("Exception Message", expected, errorMessage);
		}

		[ExpectNoExceptions()]
		public void TestSampleCallWithoutException()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 100);

			var expected = new[]
			{
				(-100m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(100m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}
	}
}

