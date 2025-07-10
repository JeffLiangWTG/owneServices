using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_Validation_Post_GLAggregation))]
	class BatchAggregator_Validation_Post_GLAggregationTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 123);

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200701, "SYD", "FIR", "EDI", "Test Account 1"),
				(-123m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(123m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			var updateGlAccountSQL = $@"
UPDATE dbo.AccGLHeader
SET AG_AccountType = 'NTE',
	AG_SystemLastEditTimeUtc = GETUTCDATE(),
	AG_SystemLastEditUser = 'TST'
WHERE AG_PK = @AG_PK";
			DbHelper.RunSQL(new { AG_PK = glAccountPK }, updateGlAccountSQL);

			CleanAggregationTable();
			CreateNonBalancingRecord(glAccountPK);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 123);

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
				(-123m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(123m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			CleanAggregationTable();
			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated_NoAggregations()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
			};

			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated_TaxGLMovements()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			var headerPK = TestHelper.InsertTransactionHeader("AP", "INV", postToGL: true);
			var linePK = TestHelper.InsertTransactionLine("CST", headerPK, postToGL: true, reverseToGL: true);
			var taxConfigurationPK = DbHelper.InsertTaxConfiguration("ADT", TestDbHelper.DefaultCompanyPK);
			var taxIdPK = DbHelper.InsertTaxRate("TAX1");
			var taxTransactionPK = DbHelper.InsertTaxTransaction(headerPK, TestDbHelper.DefaultCompanyPK, TestHelper.Branch.PK, TestHelper.Department.PK, taxConfigurationPK, taxIdPK);
			var glMovementPK = DbHelper.InsertTaxGLMovement(taxTransactionPK, TestHelper.HeaderGLAccount.PK, TestHelper.HeaderGLAccount2.PK, period: 200701);

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "BANK CHARGES"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "FINANCE EXPENSES")
			};

			CleanAggregationTable();
			DbHelper.Insert(AccTaxGLMovementQueueSchema.Constants.TableName, new { ATQ_ATM = glMovementPK });
			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated_CashVAT()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			var headerPK = TestHelper.InsertTransactionHeader("AP", "INV", postToGL: true);
			var linePK = TestHelper.InsertTransactionLine("CST", headerPK, postToGL: true, reverseToGL: true);
			InsertCashVAT();

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
				(-3.5m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(7m, "", 200701, "SYD", "FIR", "EDI", "GL_PENDING_GST_INPUT_ACCOUNT"),
				(-3.5m, "", 200701, "SYD", "FIR", "EDI", "GROSS FREIGHT REVENUE"),
			};

			CleanAggregationTable();
			InsertCashVAT();
			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);

			void InsertCashVAT()
			{
				DbHelper.InsertCashBasisVAT(linePK, postDate: TestHelper.DefaultPostDate, taxAmount: 7);
			}
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated_InvoiceLine()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			var headerPK = TestHelper.InsertTransactionHeader("AP", "INV", transactionDate: TestHelper.DefaultPostDate.AddMonths(1));
			TestHelper.InsertTransactionLine("CST", headerPK, reverseDate: DateTime.MinValue); //It is not real case when post date on line is not the same as transaction header. This is just to test that we collect periods for this level of aggregation

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", "GROSS FREIGHT REVENUE"),
				(260m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_SUSPENSE_CONTROL_ACCOUNT")
			};

			CleanAggregationTable();
			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated_InvoiceLineReverseToGL()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			var headerPK = TestHelper.InsertTransactionHeader("AP", "INV", postToGL: true);
			TestHelper.InsertTransactionLine("CST", headerPK);

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GROSS FREIGHT REVENUE"),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_SUSPENSE_CONTROL_ACCOUNT")
			};

			CleanAggregationTable();
			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated_LinePostToGL()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			TestHelper.InsertTransactionLine("WIP", reverseDate: DateTime.MinValue);

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GROSS FREIGHT REVENUE"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT")
			};

			CleanAggregationTable();
			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		public void TestAggregationValidationDoNotValidatePeriodsOlderThenAggregated_LineReverseToGL()
		{
			PrepareForAggregation();

			var glAccountPK = DbHelper.InsertGLAccount("2201.02.01", "Test Account 1", "P&L", "");

			TestHelper.InsertTransactionLine("WIP", postToGL: true);

			CreateNonBalancingRecord(glAccountPK);

			var exceptionMessage = RunAndAssertAggregationWithError(isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd: true);
			var expectedMessage =
@"The following periods do not balance:
200701";
			AssertStartsWith("Exception Message", expectedMessage, exceptionMessage);

			var expected = new[]
			{
				(1m, "ABC", 200612, "SYD", "FIR", "EDI", "Test Account 1"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GROSS FREIGHT REVENUE"),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT")
			};

			CleanAggregationTable();
			CreateNonBalancingRecord(glAccountPK, 200612);
			RunAndAssertAggregation(expected, skipExpectedAggregatesBalanceCheck: true);
		}

		void CreateNonBalancingRecord(Guid glAccountPK, int period = 200701)
		{
			var createNonBalancingRecordSQL = $@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory)
VALUES(NEWID(), 1, {period}, '{glAccountPK}', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2B67864D-42E9-4A43-A9C8-09D2083C4227', 'ABC')";
			TestConnection.Command(createNonBalancingRecordSQL).ExecuteNonQuery();
		}

		void CleanAggregationTable()
		{
			TestConnection.Command("DELETE FROM dbo.AccGLAggregate").ExecuteNonQuery();
		}
	}
}

