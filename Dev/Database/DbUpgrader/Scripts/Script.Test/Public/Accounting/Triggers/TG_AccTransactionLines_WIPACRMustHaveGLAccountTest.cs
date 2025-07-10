using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTransactionLines_WIPACRMustHaveGLAccount))]
	class TG_AccTransactionLines_WIPACRMustHaveGLAccountTest : DBCreateTriggerScriptTest
	{
		public void TestInsertWIP()
		{
			AssertInsert("WIP", false);
		}

		public void TestInsertACR()
		{
			AssertInsert("ACR", false);
		}

		public void TestInsertREV()
		{
			AssertInsert("REV", true);
		}

		public void TestInsertCST()
		{
			AssertInsert("CST", true);
		}

		public void TestUpdateWIP()
		{
			AssertUpdate("WIP", false);
		}

		public void TestUpdateACR()
		{
			AssertUpdate("ACR", false);
		}

		public void TestUpdateREV()
		{
			AssertUpdate("REV", true);
		}

		public void TestUpdateCST()
		{
			AssertUpdate("CST", true);
		}

		void AssertInsert(string lineType, bool isEmptyGLAccountAllowed)
		{
			AssertNoExceptionThrown($"Insert {lineType} with GL Account", () => DbHelper.InsertTransactionLine(lineType: lineType, glAccountPK: glAccountPK1));
			AssertDbAction($"Insert {lineType} with empty GL Account", isEmptyGLAccountAllowed, lineType, () => DbHelper.InsertTransactionLine(lineType: lineType, chargeCodePK: chargeCodePK, companyPK: TestDbHelper.DefaultCompanyPK));
		}

		void AssertUpdate(string lineType, bool isEmptyGLAccountAllowed)
		{
			var linePK = DbHelper.InsertTransactionLine(lineType: lineType,chargeCodePK: chargeCodePK, glAccountPK: glAccountPK1, companyPK: TestDbHelper.DefaultCompanyPK);

			AssertNoExceptionThrown($"{lineType} with GL Account", () => DbHelper.RunSQL(new { AL_PK = linePK, AL_AG = glAccountPK2 }, "UPDATE dbo.AccTransactionLines SET AL_AG = @AL_AG, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = @AL_PK"));

			AssertDbAction($"Update {lineType} with empty GL Account", isEmptyGLAccountAllowed, lineType, () => DbHelper.RunSQL(new { AL_PK = linePK }, "UPDATE dbo.AccTransactionLines SET AL_AG = NULL, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' WHERE AL_PK = @AL_PK"));
		}

		static void AssertDbAction(string message, bool isEmptyGLAccountAllowed, string lineType, AnonymousMethod dbAction)
		{
			if (isEmptyGLAccountAllowed)
			{
				AssertNoExceptionThrown(message, dbAction);
			}
			else
			{
				var expectedErrorMessage = $"WIP/ACR line must have GL Account.\nDetails: CompanyCode: EDI, ChargeCode: CC1, LineType: {lineType}\r\nThe transaction ended in the trigger. The batch has been aborted.";

				AssertExceptionThrown<SqlException>(message, expectedErrorMessage, dbAction);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			glAccountPK1 = DbHelper.InsertGLAccount("111.222.01", "TestGLAccount 1");
			glAccountPK2 = DbHelper.InsertGLAccount("111.222.02", "TestGLAccount 2");
			chargeCodePK = DbHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
		}

		Guid glAccountPK1;
		Guid glAccountPK2;
		Guid chargeCodePK;
	}
}

