using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_CusPermitHeader_ApplicationCode))]
	class TG_CusPermitHeader_ApplicationCodeTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestUpdateValid()
		{
			TestConnection.ExecuteNonQuery($@"
UPDATE dbo.CusPermitHeader
SET
	CPH_ApplicationCode = 'PER',
	CPH_SystemLastEditTimeUtc = GETUTCDATE(),
	CPH_SystemLastEditUser = '~BP'
WHERE
	CPH_PK = '{permitHeaderPK1}'");
		}

		public void TestUpdateInvalid()
		{
			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
UPDATE dbo.CusPermitHeader
SET
	CPH_ApplicationCode = 'PER',
	CPH_SystemLastEditTimeUtc = GETUTCDATE(),
	CPH_SystemLastEditUser = '~BP'
WHERE
	CPH_PK = '{permitHeaderPK2}'
				")).Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeaderPK = TestFramework.TestDataCreator.CreateOrganisation("OH1", "Org1");
			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.CusPermitHeader (CPH_PK, CPH_OH_PermitHolder, CPH_StartDate, CPH_Number, CPH_QtyValIndicator, CPH_Type, CPH_RN_NKCountryCode, CPH_ApplicationCode, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) VALUES
				('{permitHeaderPK1}', '{orgHeaderPK}', CURRENT_TIMESTAMP, '1234', 'BTH', 'BBB', 'AU', 'GUA', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				('{permitHeaderPK2}', '{orgHeaderPK}', CURRENT_TIMESTAMP, '5678', 'BTH', 'BBB', 'AU', 'GUA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusPermitLineTransaction (CPL_PK, CPL_CPH_PermitHeader, CPL_TransactionDate, CPL_TransactionCategory, CPL_TransactionType, CPL_Reference, CPL_Comment, CPL_AppId, CPL_SystemCreateTimeUtc, CPL_SystemCreateUser, CPL_SystemLastEditTimeUtc, CPL_SystemLastEditUser) VALUES
				(newid(), '{permitHeaderPK1}', CURRENT_TIMESTAMP, 'CUM', 'OBL', 'REFOBL', 'CMTOBL', 'APPIDOBL', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(newid(), '{permitHeaderPK2}', CURRENT_TIMESTAMP, 'CUM', 'OBA', 'REFOBL', 'CMTOBL', 'APPIDOBL', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");
		}

		readonly Guid permitHeaderPK1 = Guid.NewGuid();
		readonly Guid permitHeaderPK2 = Guid.NewGuid();

		const string TriggerMessage = "CPH_ApplicationCode must be 'GUA' when Header has Transactions(CPL_TransactionType = 'OBA')";
	}
}
