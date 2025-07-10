using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.Sales;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Test
{
	[TestedType(typeof(ViewCommissionAgreement))]
	class ViewCommissionAgreementTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestEffectiveDate()
		{
			SetupEffectiveDateData();

			var selectSql = $"SELECT * FROM dbo.ViewCommissionAgreement WHERE VCA_OH_Customer IN ('{OrgPk1}', '{OrgPk2}', '{OrgPk3}', '{OrgPk4}') ORDER BY VCA_LastApprovedDateTimeUtc";
			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCA_OH_Customer", OrgPk1, reader["VCA_OH_Customer"]);
					AssertEquals("VCA_EffectiveDate", new DateTime(2005, 1, 1), reader["VCA_EffectiveDate"]);
				});

				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCA_OH_Customer", OrgPk2, reader["VCA_OH_Customer"]);
					AssertEquals("VCA_EffectiveDate", new DateTime(2002, 1, 2), reader["VCA_EffectiveDate"]);
				});

				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCA_OH_Customer", OrgPk3, reader["VCA_OH_Customer"]);
					AssertEquals("VCA_EffectiveDate", new DateTime(2002, 1, 3), reader["VCA_EffectiveDate"]);
				});

				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCA_OH_Customer", OrgPk4, reader["VCA_OH_Customer"]);
					AssertEquals("VCA_EffectiveDate", new DateTime(2002, 3, 2), reader["VCA_EffectiveDate"]);
				});

				Assert(!reader.Read());
			}
		}

		public void TestForAccountingIndexesUsed()
		{
			SetupEffectiveDateData();
			CreateExtraAccountingHeaderRows(20);

			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS {AccTransactionHeaderSchema.Constants.TableName} WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS {AccTransactionLinesSchema.Constants.TableName} WITH FULLSCAN");

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var selectSql = $"SELECT * FROM dbo.ViewCommissionAgreement WHERE VCA_OH_Customer = '{OrgPk1}'";
				using (var command = TestConnection.Command(selectSql))
				using (var reader = command.ExecuteReader())
				{
					Assert(reader.Read());
				}

				var allQueryPlans = TestConnection.ExecutedCommandsAndQueryPlans;
				var viewQueryPlan = allQueryPlans?.FirstOrDefault(p => p.Item1.Contains("ViewCommissionAgreement"));
				AssertNotNull("Should be Query Plan for ViewCommissionAgreement", viewQueryPlan);
				var planalyzer = new QueryPlanalyzer(viewQueryPlan.Item2.Last());

				var expectedIndexes = new List<(string Index, string Table)>();
				expectedIndexes.Add(("FK_RX__AH_OH_AH_Ledger_AH_TransactionType", "AccTransactionHeader"));
				expectedIndexes.Add(("FK_RX__AL_AH", "AccTransactionLines"));

				foreach (var expectedIndex in expectedIndexes)
				{
					var allTableOperations = AllTableOperations(planalyzer, expectedIndex.Table);
					var found = planalyzer.IndexSeeks.Any(s => s.IndexName == expectedIndex.Index && s.TableName == expectedIndex.Table);
					AssertEquals($"Index {expectedIndex.Index}, Table {expectedIndex.Table} should be used with Index Seek {allTableOperations}", true, found);
				}
			}
		}

		string AllTableOperations(QueryPlanalyzer planalyzer, string tableName)
		{
			var stringBuilder = new StringBuilder(10);
			stringBuilder.AppendLine();
			stringBuilder.Append("All Table Operations");
			stringBuilder.AppendLine();
			foreach (var operation in planalyzer.IndexSeeks.Where(s => s.TableName == tableName))
			{
				stringBuilder.AppendFormat("Index Seek: {0}", operation.IndexName);
				stringBuilder.AppendLine();
			}

			foreach (var operation in planalyzer.IndexScans.Where(s => s.TableName == tableName))
			{
				stringBuilder.AppendFormat("Index Scan: {0}", operation.IndexName);
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		void SetupEffectiveDateData()
		{
			OrgPk1 = Guid.NewGuid();
			OrgPk2 = Guid.NewGuid();
			OrgPk3 = Guid.NewGuid();
			OrgPk4 = Guid.NewGuid();

			var insertSql = $@"
	DECLARE @BranchPk UNIQUEIDENTIFIER = '366275CB-FD5F-4cb4-A6A1-62555EFC79DA';
	DECLARE @DepartmentPk UNIQUEIDENTIFIER = '91842181-BB56-4824-A985-97945CB005FE';
	DECLARE @RevenueAudCompanyPk UNIQUEIDENTIFIER = 'DADF67BB-EED1-4f84-8D7C-AEC6CA50CF6C';
	DECLARE @AgreementPk1 UNIQUEIDENTIFIER = '5599AF18-AB4C-4a66-9811-5399C55E5A81';
	DECLARE @AgreementPk2 UNIQUEIDENTIFIER = 'B0C29153-8593-459d-83D9-FBE24D424FBB';
	DECLARE @AgreementPk3 UNIQUEIDENTIFIER = '0F4A7DD0-2E10-48a3-8B17-145092EBE912';
	DECLARE @AgreementPk4 UNIQUEIDENTIFIER = '1FA3566F-E99B-4401-8AC8-A6F08A9B4897';
	DECLARE @OpportunityPk UNIQUEIDENTIFIER = '2F076A72-F7C8-4da3-A59E-71478EC5BC3A';
	DECLARE @OrgPk1 UNIQUEIDENTIFIER = '{OrgPk1}';
	DECLARE @OrgPk2 UNIQUEIDENTIFIER = '{OrgPk2}';
	DECLARE @OrgPk3 UNIQUEIDENTIFIER = '{OrgPk3}';
	DECLARE @OrgPk4 UNIQUEIDENTIFIER = '{OrgPk4}';
	DECLARE @PartyPk UNIQUEIDENTIFIER = '3C7609DF-FBFD-4956-8CD4-9A6A7A0FAB98'
	DECLARE @OrgMiscServPk UNIQUEIDENTIFIER = '2AAA5A48-AF70-4c1e-99FD-9BA47473DF81';
	DECLARE @TransHeader1Pk UNIQUEIDENTIFIER = 'D0FFA11F-4704-4b7b-BF6A-9BBC3F0A59DE';
	DECLARE @TransHeader2Pk UNIQUEIDENTIFIER = 'F4B6328E-BE8E-4de9-A920-B013389BF053';
	DECLARE @TransLine1Pk UNIQUEIDENTIFIER = '061C21D8-7AB0-4b7a-AD04-BC4E7FDA1680';
	DECLARE @TransLine2Pk UNIQUEIDENTIFIER = 'E5E96C91-405B-4040-99F7-9085705DCCD8';

			
	INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES
				 (@RevenueAudCompanyPk, 'RAU', 'AU company', 'AU', 'AUD')

	INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @RevenueAudCompanyPk)
	INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)

	INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk1, 'TESTORG1')
	INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk2, 'TESTORG2')
	INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk3, 'TESTORG3')
	INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk4, 'TESTORG4')

	INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@PartyPk, 'TESTPARTY')
	INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_GC_CMPreferredPaymentCompany, OM_CMClientCommenced) VALUES (NEWID(), @OrgPk1, @RevenueAudCompanyPk, '2005-1-1')

	INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_OpportunityID, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser) VALUES (@OpportunityPk, @RevenueAudCompanyPk, @OrgPk1, 'OppId', GetUtcDate(), 'E', GetUtcDate(), 'E')
	INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser) VALUES (@AgreementPk1, '#1', @OpportunityPk, @OrgPk1, 'CCD', 'REV', '2002-1-1', '2002-1-1', 'XX', '2002-1-1', 'XX')
	INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser) VALUES (@AgreementPk2, '#2', @OpportunityPk, @OrgPk2, '1AR', 'REV', '2002-1-2', '2002-1-2', 'XX', '2002-1-2', 'XX')
	INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_EffectiveDate, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser) VALUES (@AgreementPk3, '#3', @OpportunityPk, @OrgPk3, 'MAN', 'REV', '2002-1-3', '2002-1-3', '2002-1-3', 'XX', '2002-1-3', 'XX')
	INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser) VALUES (@AgreementPk4, '#4', @OpportunityPk, @OrgPk4, 'ERR', 'REV', '2002-1-4', '2002-1-4', 'XX', '2002-1-4', 'XX')

	INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_GC, AH_GB, AH_GE, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_PostDate) VALUES(@TransHeader1Pk, @RevenueAudCompanyPk, @BranchPk, @DepartmentPk, @OrgPk2, '2002-1-2', 'AR', 'INV', 1, '2002-1-2')
	INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_GC, AH_GB, AH_GE, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_PostDate) VALUES(@TransHeader2Pk, @RevenueAudCompanyPk, @BranchPk, @DepartmentPk, @OrgPk4, '2002-2-2', 'AR', 'INV', 2, '2002-2-2')
	INSERT INTO dbo.AccTransactionLines(AL_PK, AL_LineType, AL_ReverseDate, AL_GB, AL_GE, AL_GC, AL_AH) VALUES(@TransLine1Pk, 'REV', '2002-3-1', @BranchPk, @DepartmentPk, @RevenueAudCompanyPk, @TransHeader1Pk)
	INSERT INTO dbo.AccTransactionLines(AL_PK, AL_LineType, AL_ReverseDate, AL_GB, AL_GE, AL_GC, AL_AH) VALUES(@TransLine2Pk, 'REV', '2002-3-2', @BranchPk, @DepartmentPk, @RevenueAudCompanyPk, @TransHeader2Pk)";

			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}
		}

		void CreateExtraAccountingHeaderRows(int count)
		{
			for (int i = 0; i < count; i++)
			{
				var insertSql = $@"
					DECLARE @BranchPk UNIQUEIDENTIFIER = '366275CB-FD5F-4cb4-A6A1-62555EFC79DA';
					DECLARE @DepartmentPk UNIQUEIDENTIFIER = '91842181-BB56-4824-A985-97945CB005FE';
					DECLARE @RevenueAudCompanyPk UNIQUEIDENTIFIER = 'DADF67BB-EED1-4f84-8D7C-AEC6CA50CF6C';
					DECLARE @OrgPk4 UNIQUEIDENTIFIER = '{OrgPk4}';
					DECLARE @TransactionNumber varchar(38) = 'TEST200{i}';
					DECLARE @TransHeaderPk UNIQUEIDENTIFIER = newid();
					INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_GC, AH_GB, AH_GE, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_PostDate) VALUES(@TransHeaderPk, @RevenueAudCompanyPk, @BranchPk, @DepartmentPk, @OrgPk4, '2003-1-2', 'AR', 'INV', @TransactionNumber, '2003-1-2')
					INSERT INTO dbo.AccTransactionLines(AL_PK, AL_LineType, AL_ReverseDate, AL_GB, AL_GE, AL_GC, AL_AH) VALUES(newid(), 'REV', '2003-3-1', @BranchPk, @DepartmentPk, @RevenueAudCompanyPk, @TransHeaderPk)";

				using (var command = TestConnection.Command(insertSql))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		protected Guid OrgPk1;
		protected Guid OrgPk2;
		protected Guid OrgPk3;
		protected Guid OrgPk4;
	}
}
