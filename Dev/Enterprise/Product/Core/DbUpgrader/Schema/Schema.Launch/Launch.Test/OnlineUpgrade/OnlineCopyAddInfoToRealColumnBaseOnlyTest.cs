using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[TestedType(typeof(OnlineCopyAddInfoToRealColumnTestClass))]
	class OnlineCopyAddInfoToRealColumnBaseOnlyTest : OnlineCopyAddInfoToRealColumnTest<CopyAddInfoToRealColumnTestClass, OnlineCopyAddInfoToRealColumnTestClass>
	{
		void InsertAddInfo(Guid invLinePK, int clusterKey, string declarationReference, Guid companyPK, Guid branchPK)
		{
			var insertSql = $@"DECLARE @DECPK UNIQUEIDENTIFIER = NEWID(), @INVPK UNIQUEIDENTIFIER = NEWID()
INSERT dbo.JobDeclaration (JE_PK, JE_DeclarationReference, JE_GC, JE_GB, JE_ClusterKey) VALUES (@DECPK, @DeclarationReference, @CompanyPK, @BranchPK, @ClusterKey);
INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_JE, JZ_ClusterKey) VALUES (@INVPK, @DECPK, @ClusterKey);
INSERT dbo.JobComInvoiceLine (JI_PK, JI_JZ, JI_ClusterKey, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser, JI_AddInfo, JI_PartNo)
VALUES (@invLinePK, @INVPK, @ClusterKey, '2021-10-15 14:56:00', 'BOB', '2021-10-16 09:35:00', 'JOE', 'DisposalDate=2021-10-15*ScheduledReExportDate=2021-09-14 23:45:31*GREETING=HELLO*AdditionalTariffCode=20210813*AdditionalDutyRate=123456.78', 'IMP')";
			var cmd = Db.Connection.Command(insertSql);
			cmd.AddParameter("@invLinePK", SqlDbType.UniqueIdentifier, invLinePK);
			cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
			cmd.AddParameter("@DeclarationReference", SqlDbType.VarChar, declarationReference);
			cmd.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
			cmd.ExecuteNonQuery();
		}

		const string invLineUS2SqlInsertScript = @"
DECLARE @DEC2PK UNIQUEIDENTIFIER = NEWID(), @INV2PK UNIQUEIDENTIFIER = NEWID()
INSERT dbo.JobDeclaration (JE_PK, JE_DeclarationReference, JE_GC, JE_GB, JE_ClusterKey) VALUES (@DEC2PK, 'DECUS02', @CompanyUS2PK, @BranchUS2PK, 2);
INSERT dbo.JobComInvoiceHeader (JZ_PK, JZ_JE, JZ_ClusterKey) VALUES (@INV2PK, @DEC2PK, 2);
INSERT dbo.JobComInvoiceLine (JI_PK, JI_JZ, JI_ClusterKey, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser, JI_AddInfo, JI_PartNo)
VALUES (@InvLineUS2PK, @INV2PK, 2, '2021-10-15 14:56:00', 'BOB', '2021-10-16 09:35:00', 'JOE', 'DisposalDate=2021-10-15*ScheduledReExportDate=2021-09-14 23:45:31*GREETING=HELLO*AdditionalTariffCode=20210813*AdditionalDutyRate=123456.78', 'IMP');
";

		protected override void CreateTestData()
		{
			DeleteAlreadyProcessedData();
			SetCompaniesAndBranches();

			var sqlSetupData = invLineUS2SqlInsertScript;

			var cmd = Db.Connection.Command(sqlSetupData);
			InvLineUS2PK = Guid.NewGuid();
			cmd.AddParameter("@CompanyUS2PK", SqlDbType.UniqueIdentifier, CompanyUS2PK);
			cmd.AddParameter("@BranchUS2PK", SqlDbType.UniqueIdentifier, BranchUS2PK);
			cmd.AddParameter("@InvLineUS2PK", SqlDbType.UniqueIdentifier, InvLineUS2PK);
			cmd.ExecuteNonQuery();
		}

		void AssertTestDataResult()
		{
			AssertNoDataChanged(InvLineUS2PK, 2);
		}

		protected override void AssertFirstRunResult(OnlineCopyAddInfoToRealColumnTestClass testTransformation, string[] logs)
		{
			var expectedLogForAssertResultsTwice = $@"Creating OffLine Processing Table {OffLineProcessingTableName} and trigger
Populate JobComInvoiceLine (JI_ValuationDateOverride, JI_CustomDate4, JI_OrderNumber, JI_CustomDecimal1) columns from dbo.JobComInvoiceLine JI_AddInfo
Creating Pre-Add database.
table 1/1 (0 MB/0 MB) [{Db.DatabaseName}].[dbo].[JobComInvoiceLine]
    (+) JI_ClusterKey int NOT NULL  CONSTRAINT [DF_JobComInvoiceLine_JI_ClusterKey] DEFAULT ((0))
    (+) JI_ValuationDateOverride smalldatetime NULL 
    (+) JI_CustomDate4 smalldatetime NULL 
    (+) JI_CustomDecimal1 decimal(9, 3) NOT NULL  CONSTRAINT [DF_JobComInvoiceLine_JI_CustomDecimal1] DEFAULT ((0))
    (+) JI_OrderNumber varchar(25) NOT NULL  CONSTRAINT [DF_JobComInvoiceLine_JI_OrderNumber] DEFAULT ('')
    : Refreshing dependent scripts
    : Recording PKs
    : Updating with default value
	              2 records remaining";

			AssertMultilineASCIIEquals("logs", expectedLogForAssertResultsTwice, string.Join(System.Environment.NewLine, logs));
			AssertTestDataResult();
			var insertUpdateAddInfoSourceTriggerName = InsertUpdateAddInfoSourceTriggerName;
			AssertEquals($"After transformation. Update/Insert Trigger ({insertUpdateAddInfoSourceTriggerName}) does exists", true, DbObjectCreator.TriggerExists(Db.Connection, OfflineTransformation.SourceTableName, insertUpdateAddInfoSourceTriggerName));
			AssertEquals($"After transformation. {OffLineProcessingTableName} table exists", true, DbObjectCreator.TableExists(Db.Connection, OffLineProcessingTableName));
			AssertEquals($"After transformation. {OffLineProcessingTableName} no of records", 0, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM {OffLineProcessingTableName}"));
			var updateClusterKeySourceTriggerName = UpdateClusterKeySourceTriggerName;
			AssertEquals($"After transformation. Update Trigger ({updateClusterKeySourceTriggerName}) does exists", true, DbObjectCreator.TriggerExists(Db.Connection, OfflineTransformation.SourceTableName, updateClusterKeySourceTriggerName));
		}

		protected override void AssertSecondRunResult(OnlineCopyAddInfoToRealColumnTestClass testTransformation, string[] logs)
		{
			AssertEquals("Second run, no column need to be transformed", 0, logs.Length);
			AssertTestDataResult();
			var insertUpdateAddInfoSourceTriggerName = InsertUpdateAddInfoSourceTriggerName;
			AssertEquals($"Second run. Update/Insert Trigger ({insertUpdateAddInfoSourceTriggerName}) still exists", true, DbObjectCreator.TriggerExists(Db.Connection, OfflineTransformation.SourceTableName, insertUpdateAddInfoSourceTriggerName));
			AssertEquals($"Second run. {OffLineProcessingTableName} table still exists", true, DbObjectCreator.TableExists(Db.Connection, OffLineProcessingTableName));
			AssertEquals($"Second run. {OffLineProcessingTableName} no of records", 0, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM {OffLineProcessingTableName}"));

			AssertUpdatingSourceWillAddToOfflineProcessingTable(testTransformation);
		}

		void AssertUpdatingSourceWillAddToOfflineProcessingTable(OnlineCopyAddInfoToRealColumnTestClass testTransformation)
		{
			var addInfoColumnName = OfflineTransformation.SourceAddInfoColumn.Name;
			UpdateAddInfo(InvLineUS2PK, "GREETING=HI");
			AssertEquals($"Updating {addInfoColumnName} on non-applicable row should not trigger offline processing", 0, GetOffLineProcessingTableCount(InvLineUS2PK));

			var obj5PK = Guid.NewGuid();
			InsertAddInfo(obj5PK);
			AssertEquals($"Insert {addInfoColumnName} for applicable row should trigger offline processing", 1, GetOffLineProcessingTableCount(obj5PK));

			var obj6NonUpdatePK = Guid.NewGuid();
			InsertAddInfoOnNonApplicableRow(obj6NonUpdatePK); // InsertAddInfo(obj6NonUpdatePK, 6, "DECUS06", CompanyUS2PK, BranchUS2PK);
			AssertEquals($"Insert {addInfoColumnName} on non-applicable row should not trigger offline processing", 0, GetOffLineProcessingTableCount(obj6NonUpdatePK));
		}
		void InsertAddInfo(Guid objPK) => InsertAddInfo(objPK, 5, "DECUS05", CompanyUS2PK, BranchUS2PK);

		void InsertAddInfoOnNonApplicableRow(Guid objPK) => InsertAddInfo(objPK, 6, "DECUS06", CompanyUS2PK, BranchUS2PK);

		protected override OnlineCopyAddInfoToRealColumnTestClass CreateOnlineTransformation() => new OnlineCopyAddInfoToRealColumnTestClass(manager, Db.DatabaseName, TemplateDb);

		void SetCompaniesAndBranches()
		{
			var sqlSetupData = @"
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyUS2PK, 'US2', 'US company', 'US', 'USD');
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@BranchUS2PK, @CompanyUS2PK, 'US2');
					";

			var cmd = Db.Connection.Command(sqlSetupData);
			CompanyUS2PK = Guid.NewGuid();
			BranchUS2PK = Guid.NewGuid();
			cmd.AddParameter("@CompanyUS2PK", SqlDbType.UniqueIdentifier, CompanyUS2PK);
			cmd.AddParameter("@BranchUS2PK", SqlDbType.UniqueIdentifier, BranchUS2PK);
			cmd.ExecuteNonQuery();
		}

		void AssertNoDataChanged(Guid objPK, int reference)
		{
			AssertData(objPK, reference, DBNull.Value, DBNull.Value, string.Empty, 0m, "DisposalDate=2021-10-15*ScheduledReExportDate=2021-09-14 23:45:31*GREETING=HELLO*AdditionalTariffCode=20210813*AdditionalDutyRate=123456.78");
		}

		void AssertData(Guid objPK, int reference, object valuationDateOverride, object customDate4, string orderNumber, decimal customDecimal1, string addInfo)
		{
			AssertJobComInvoiceLine(objPK, reference,
				(valuationDateOverride, JobComInvoiceLineSchema.JI_ValuationDateOverride),
				(customDate4, JobComInvoiceLineSchema.JI_CustomDate4),
				(orderNumber, JobComInvoiceLineSchema.JI_OrderNumber),
				(customDecimal1, JobComInvoiceLineSchema.JI_CustomDecimal1),
				(addInfo, JobComInvoiceLineSchema.JI_AddInfo));
		}

		void AssertJobComInvoiceLine(Guid objPK, int reference, params (object data, SchemaColumn column)[] expectedColumns)
		{
			var sqlText = $"SELECT {string.Join(", ", expectedColumns.Select(x => x.column.Name))} FROM dbo.JobComInvoiceLine WHERE JI_PK = @invLinePK";
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@invLinePK", SqlDbType.UniqueIdentifier, objPK);
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals("Should return data", true, reader.Read());
				foreach (var expectedColumn in expectedColumns)
				{
					var column = expectedColumn.column;
					AssertEquals($"{reference} - {column.Name}", expectedColumn.data, reader[column.Name]);
				}
			}
		}

		protected override ITableSchema TargetTableSchema => JobComInvoiceLineSchema.Instance;

		Guid CompanyUS2PK;
		Guid BranchUS2PK;
		Guid InvLineUS2PK;
	}
}
