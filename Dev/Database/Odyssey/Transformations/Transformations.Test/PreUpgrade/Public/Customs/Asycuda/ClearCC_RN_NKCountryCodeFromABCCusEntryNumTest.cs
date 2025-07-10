using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Customs.Asycuda.Testing
{
	sealed class ClearCC_RN_NKCountryCodeFromABCCusEntryNumTest : TransactionedTestCase
	{
		public void TestTransform()
		{
			PrepareTestData();

			ClearCountryCodeFromABCCusEntryNum.Transform(new DummyUpgradeManager());
			AssertTransformationResults();

			ClearCountryCodeFromABCCusEntryNum.Transform(new DummyUpgradeManager());
			AssertTransformationResults();
		}

		Guid CE_PK1 = Guid.NewGuid();
		Guid CE_PK2 = Guid.NewGuid();
		Guid CE_PK3 = Guid.NewGuid();
		Guid CE_PK4 = Guid.NewGuid();
		Guid ABC_PK = Guid.NewGuid();

		void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery(
				"alter table CusEntryNum drop constraint if exists Constraint_CE_ParentTable_NoCheck;");

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, Db.DatabaseName, "AsycudaBillCountry",
				@"CREATE TABLE [AsycudaBillCountry] (
   [ABC_PK] UNIQUEIDENTIFIER NOT NULL,
   [ABC_ABL_Bill] UNIQUEIDENTIFIER NOT NULL,
   [ABC_RN_NKCountry] CHAR(2) NOT NULL DEFAULT '',
   [ABC_BillStatus] VARCHAR(3) NOT NULL DEFAULT '',
   [ABC_BillIssuer] VARCHAR(35) NOT NULL DEFAULT '',
   [ABC_SenderReference] VARCHAR(35) NOT NULL DEFAULT '',
   [ABC_MessageStatus] VARCHAR(3) NOT NULL DEFAULT '',
   [ABC_GoodsLocation] VARCHAR(80) NOT NULL DEFAULT '',
   [ABC_LocationInformation] VARCHAR(80) NOT NULL DEFAULT '',
   [ABC_ShipmentType] VARCHAR(3) NOT NULL DEFAULT '',
);");

			string sqlData = @"
INSERT INTO dbo.CusEntryNum ([CE_PK], [CE_ParentID], [CE_ParentTable], [CE_IssueDate], [CE_ExpiryDate], [CE_RN_NKCountryCode], [CE_SystemCreateTimeUtc], [CE_SystemCreateUser], [CE_SystemLastEditTimeUtc], [CE_SystemLastEditUser]) 
	SELECT @CE_PK1, @ABC_PK, 'AsycudaBillCountry', GETDATE(), GETDATE(), '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
	UNION
	SELECT @CE_PK2, @ABC_PK, 'AsycudaBillCountry', GETDATE(), GETDATE(), 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
	UNION
	SELECT @CE_PK3, @ABC_PK, 'AsycudaBillCountry', GETDATE(), GETDATE(), 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
	UNION
	SELECT @CE_PK4, @ABC_PK, 'AsycudaBillCountry', GETDATE(), GETDATE(), 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
";

			var cmd = Db.Connection.Command(sqlData);

			cmd.AddParameter("@CE_PK1", SqlDbType.UniqueIdentifier, CE_PK1);
			cmd.AddParameter("@CE_PK2", SqlDbType.UniqueIdentifier, CE_PK2);
			cmd.AddParameter("@CE_PK3", SqlDbType.UniqueIdentifier, CE_PK3);
			cmd.AddParameter("@CE_PK4", SqlDbType.UniqueIdentifier, CE_PK4);
			cmd.AddParameter("@ABC_PK", SqlDbType.UniqueIdentifier, ABC_PK);
			cmd.AddParameter("@ABC_ABL_Bill", SqlDbType.UniqueIdentifier, ABC_PK);

			cmd.ExecuteNonQuery();
		}

		void AssertTransformationResults()
		{
			var sqlCountCountryNotEmpty = "SELECT COUNT(*) FROM dbo.CusEntryNum WHERE [CE_ParentID] = @ABC_PK AND [CE_RN_NKCountryCode] <> ''";
			var sqlCountCountryEmpty = "SELECT COUNT(*) FROM dbo.CusEntryNum WHERE [CE_ParentID] = @ABC_PK AND [CE_RN_NKCountryCode] = '' ";

			var cmd = Db.Connection.Command(sqlCountCountryNotEmpty);
			cmd.AddParameter("@ABC_PK", SqlDbType.UniqueIdentifier, ABC_PK);

			var notEmptyCount = (int)cmd.ExecuteScalar();

			cmd = Db.Connection.Command(sqlCountCountryEmpty);
			cmd.AddParameter("@ABC_PK", SqlDbType.UniqueIdentifier, ABC_PK);

			var emptyCount = (int)cmd.ExecuteScalar();

			AssertEquals(0, notEmptyCount);
			AssertEquals(4, emptyCount);
		}
	}
}
