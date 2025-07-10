using System;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_INVCRDADJTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("AP", "INV");
			var linePK = TestHelper.InsertTransactionLine("CST", headerPK);

			DbHelper.InsertCashBasisVAT(linePK, TestHelper.DefaultPostDate, taxAmount: 6);

			var expected = new[]
			{
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_SUSPENSE_CONTROL_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(6m, "", 200701, "SYD", "FIR", "EDI", "GL_PENDING_GST_INPUT_ACCOUNT"),
				(-3m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-3m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(260m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_SUSPENSE_CONTROL_ACCOUNT")
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("AR", "INV");
			var linePK = TestHelper.InsertTransactionLine("REV", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("AR", "INV", useAnotherBranch: true);
			var linePK2 = TestHelper.InsertTransactionLine("REV", headerPK2, useAnotherBranch: true);

			DbHelper.InsertCashBasisVAT(linePK, TestHelper.DefaultPostDate, taxAmount: 6);
			DbHelper.InsertCashBasisVAT(linePK2, TestHelper.DefaultPostDate, taxAmount: 6);

			var expected = new[]
			{
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(6m, "", 200701, "SYD", "FIR", "EDI", "GL_PENDING_GST_OUTPUT_ACCOUNT"),
				(-6m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(260m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),

				(-250m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "BNE", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(6m, "", 200701, "BNE", "FIR", "EDI", "GL_PENDING_GST_OUTPUT_ACCOUNT"),
				(-6m, "", 200701, "BNE", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(260m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT")
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("AR", "INV");
			var linePK = TestHelper.InsertTransactionLine("REV", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("AR", "INV", useAnotherDepartment: true);
			var linePK2 = TestHelper.InsertTransactionLine("REV", headerPK2, useAnotherDepartment: true);

			DbHelper.InsertCashBasisVAT(linePK, TestHelper.DefaultPostDate, taxAmount: 6);
			DbHelper.InsertCashBasisVAT(linePK2, TestHelper.DefaultPostDate, taxAmount: 6);

			var expected = new[]
			{
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(6m, "", 200701, "SYD", "FIR", "EDI", "GL_PENDING_GST_OUTPUT_ACCOUNT"),
				(-6m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(260m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),

				(-250m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FEA", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(6m, "", 200701, "SYD", "FEA", "EDI", "GL_PENDING_GST_OUTPUT_ACCOUNT"),
				(-6m, "", 200701, "SYD", "FEA", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(260m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("AR", "INV");
			var linePK = TestHelper.InsertTransactionLine("REV", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("AR", "INV", useAnotherGLAccount: true);
			var linePK2 = TestHelper.InsertTransactionLine("REV", headerPK2, useAnotherGLAccount: true);

			DbHelper.InsertCashBasisVAT(linePK, TestHelper.DefaultPostDate, taxAmount: 6);
			DbHelper.InsertCashBasisVAT(linePK2, TestHelper.DefaultPostDate, taxAmount: 6);

			var expected = new[]
			{
				(-500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-20m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(12m, "", 200701, "SYD", "FIR", "EDI", "GL_PENDING_GST_OUTPUT_ACCOUNT"),
				(-12m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(520m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),

				(-250m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("AR", "INV");
			var linePK = TestHelper.InsertTransactionLine("REV", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("AR", "INV", useNextPeriodPostDate: true);
			var linePK2 = TestHelper.InsertTransactionLine("REV", headerPK2, useNextPeriodPostDate: true, useNextPeriodReverseDate: true);

			DbHelper.InsertCashBasisVAT(linePK, TestHelper.DefaultPostDate, taxAmount: 6);
			DbHelper.InsertCashBasisVAT(linePK2, TestHelper.DefaultPostDate.AddMonths(1), taxAmount: 6);

			var expected = new[]
			{
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(6m, "", 200701, "SYD", "FIR", "EDI", "GL_PENDING_GST_OUTPUT_ACCOUNT"),
				(-6m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(260m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),

				(-250m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200702, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(6m, "", 200702, "SYD", "FIR", "EDI", "GL_PENDING_GST_OUTPUT_ACCOUNT"),
				(-6m, "", 200702, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(260m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-250m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public void TestAggregationOfREversalsInDifferentPeriodOnSecondRun()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("AR", "CRD");
			var linePK = TestHelper.InsertTransactionLine("REV", headerPK, 150, reverseDate: DateTime.MinValue);

			var expected = new[]
			{
				(-150m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(+160m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);

			expected = expected.Concat(new[]
			{
				(-150m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+150m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			}).ToArray();

			TestHelper.UpdateTransactionLineReveseDate(linePK, TestHelper.DefaultPostDate.AddMonths(1));
			RunAndAssertAggregation(expected);
		}

		public void TestCMTChargeLinesAreIgnorredButMarkedAsPostedToGL()
		{
			PrepareForAggregation();

			DataSetup(useCommentChargeCode: true);
			var expected = Array.Empty<(decimal, string, int, string, string, string, string)>();
			RunAndAssertAggregation(expected);

			DataSetup(useCommentChargeCode: false);
			expected = expected.Concat(new[]
			{
				(-250m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_SUSPENSE_CONTROL_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(6m, "", 200701, "SYD", "FIR", "EDI", "GL_PENDING_GST_INPUT_ACCOUNT"),
				(-3m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-3m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(260m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-250m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(250m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_SUSPENSE_CONTROL_ACCOUNT")
			}).ToArray();
			RunAndAssertAggregation(expected);

			void DataSetup(bool useCommentChargeCode)
			{
				var headerPK = TestHelper.InsertTransactionHeader("AP", "INV");
				var linePK = TestHelper.InsertTransactionLine("CST", headerPK, useCommentChargeCode: useCommentChargeCode);

				DbHelper.InsertCashBasisVAT(linePK, TestHelper.DefaultPostDate, taxAmount: useCommentChargeCode ? 0 : 6);
			}
		}

		public void TestCashVATPostDatePeriodValidation()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("AP", "INV", transactionNumber: "INV123", postToGL: true);
			var linePK = TestHelper.InsertTransactionLine("CST", headerPK, postToGL: true, reverseToGL: true);

			DbHelper.InsertCashBasisVAT(linePK, TestHelper.DefaultPostDate.AddYears(-1), taxAmount: 6);

			var errorMessage = RunAndAssertAggregationWithError(true);
			var expectedErrorMessage = @"The following transactions have Cash VAT post dates for which accounting periods do not exist. Please create the appropriate periods.
AP	INV	INV123	Jan  5 2006";
			AssertMultilineASCIIEquals(expectedErrorMessage, errorMessage);
		}
	}
}
