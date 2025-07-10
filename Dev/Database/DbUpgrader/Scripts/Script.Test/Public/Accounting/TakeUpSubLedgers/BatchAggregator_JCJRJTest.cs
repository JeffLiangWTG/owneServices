using System;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_JCJRJTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			var pk = TestHelper.InsertTransactionHeader("JC", "JRJ");
			TestHelper.InsertTransactionLine("REV", pk);
			TestHelper.InsertTransactionLine("CST", pk);

			var expected = new[]
			{
				(-500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),		//Actual Posting to Control suspense account with AL_PostDate
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),			//Control Account Posting
				(-500m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),                //Actual Posting with AL_ReverseDate
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT")		//Actual Posting to Control suspense account with AL_ReverseDate with opposite amount
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JRJ");
			TestHelper.InsertTransactionLine("REV", headerPK);
			TestHelper.InsertTransactionLine("CST", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JRJ", useAnotherBranch: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, useAnotherBranch: true);
			TestHelper.InsertTransactionLine("CST", headerPK2, useAnotherBranch: true);

			var expected = new[]
			{
				(-500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+500m, "", 200701, "BNE", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+500m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JRJ");
			TestHelper.InsertTransactionLine("REV", headerPK);
			TestHelper.InsertTransactionLine("CST", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JRJ", useAnotherDepartment: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, useAnotherDepartment: true);
			TestHelper.InsertTransactionLine("CST", headerPK2, useAnotherDepartment: true);

			var expected = new[]
			{
				(-500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+500m, "", 200701, "SYD", "FEA", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
				(+500m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}
		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JRJ");
			TestHelper.InsertTransactionLine("REV", headerPK);
			TestHelper.InsertTransactionLine("CST", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JRJ", useAnotherGLAccount: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, useAnotherGLAccount: true);
			TestHelper.InsertTransactionLine("CST", headerPK2, useAnotherGLAccount: true);

			var expected = new[]
			{
				(-1000m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+1000m, "", 200701, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+1000m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JRJ");
			TestHelper.InsertTransactionLine("REV", headerPK);
			TestHelper.InsertTransactionLine("CST", headerPK);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JRJ", useNextPeriodPostDate: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, useNextPeriodPostDate: true, useNextPeriodReverseDate: true);
			TestHelper.InsertTransactionLine("CST", headerPK2, useNextPeriodPostDate: true, useNextPeriodReverseDate: true);

			var expected = new[]
			{
				(-500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-500m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+500m, "", 200702, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+500m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT")
			};

			RunAndAssertAggregation(expected);
		}

		public void TestJRJAccountValidation()
		{
			PrepareForAggregation(skipJRJAccountSetup: true);

			var pk = TestHelper.InsertTransactionHeader("JC", "JRJ");
			TestHelper.InsertTransactionLine("REV", pk);

			var actualMessage = RunAndAssertAggregationWithError();
			var expectedMessage = "Please set up the following Control Accounts in the registry: Accounting > General Ledger Defaults > Control Account > Job Revenue Journal Control Account";

			AssertEquals("Exception Message", expectedMessage, actualMessage);
		}

		public void TestAggregationOfREversalsInDifferentPeriodOnSecondRun()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JRJ");
			var linePK = TestHelper.InsertTransactionLine("REV", headerPK, 150, reverseDate: DateTime.MinValue);

			var expected = new[]
			{
				(-150m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+150m, "", 200701, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
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
				(-500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT"),
				(-500m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+500m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT")
			}).ToArray();
			RunAndAssertAggregation(expected);

			void DataSetup(bool useCommentChargeCode)
			{
				var pk = TestHelper.InsertTransactionHeader("JC", "JRJ");
				TestHelper.InsertTransactionLine("REV", pk, useCommentChargeCode: useCommentChargeCode);
				TestHelper.InsertTransactionLine("CST", pk, useCommentChargeCode: useCommentChargeCode);
			}
		}
	}
}

