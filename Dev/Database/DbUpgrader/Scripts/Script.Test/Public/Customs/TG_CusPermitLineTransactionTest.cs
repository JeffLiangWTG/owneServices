using System;
namespace Enterprise.Build.Database.Script.Public.Customs
{
	abstract class TG_CusPermitLineTransactionTest : DbCreateScriptTest
	{
		protected void AssertExceptionMessage(string message, Action action)
		{
			AssertContains(message, AssertExceptionThrown<SqlException>(() => action()).Message);
		}

		protected void AssertExceptionMessage(string message, string sql)
		{
			AssertExceptionMessage(message, () => TestConnection.ExecuteNonQuery(sql));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeaderPK = TestFramework.TestDataCreator.CreateOrganisation("OH1", "Org1");
			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.CusPermitHeader (CPH_PK, CPH_OH_PermitHolder, CPH_StartDate, CPH_Number, CPH_QtyValIndicator, CPH_Type, CPH_RN_NKCountryCode, CPH_ApplicationCode, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) VALUES
				('{PermitHeaderPK}', '{orgHeaderPK}', CURRENT_TIMESTAMP, '1234', 'BTH', 'BBB', 'AU', 'PER', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				('{GuaranteeHeaderPK}', '{orgHeaderPK}', CURRENT_TIMESTAMP, '5678', 'BTH', 'BBB', 'AU', 'GUA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");
		}

		protected void InsertCusPermitLineTransaction(Guid headerPK, string transactionType = "OBL", bool isAggregated = false, string transactionStatus = "")
		{
			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.CusPermitLineTransaction (CPL_PK, CPL_CPH_PermitHeader, CPL_TransactionDate, CPL_TransactionCategory, CPL_TransactionType, CPL_Reference, CPL_Comment, CPL_AppId, CPL_IsAggregated, CPL_TransactionStatus) VALUES
				(newid(), '{headerPK}', CURRENT_TIMESTAMP, 'CUM', '{transactionType}', 'REF{transactionType}', 'CMT{transactionType}', 'APPID{transactionType}', {(isAggregated ? 1 : 0)}, '{transactionStatus}')");
		}

		protected void DeleteCusPermitLineTransaction(Guid headerPK)
		{
			TestConnection.ExecuteNonQuery($"DELETE dbo.CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{headerPK}'");
		}

		protected readonly Guid PermitHeaderPK = Guid.NewGuid();
		protected readonly Guid GuaranteeHeaderPK = Guid.NewGuid();
	}
}
