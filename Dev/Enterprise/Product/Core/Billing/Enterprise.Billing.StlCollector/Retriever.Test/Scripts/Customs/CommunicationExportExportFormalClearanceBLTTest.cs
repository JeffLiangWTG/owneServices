using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationExportExportFormalClearanceBLT))]
	sealed class CommunicationExportExportFormalClearanceBLTTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = $@"
				DECLARE @ZAJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @ZAJePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @ZAJePk03 UNIQUEIDENTIFIER = newid();
				DECLARE @DEJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @DEJePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @DEJePk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk UNIQUEIDENTIFIER = newid(); 
				DECLARE @NZJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GBJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GBJePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkZa UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkZa UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkZa);
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'ZA', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkZa;
				DECLARE @GcPkDe UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkDe UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkDe, 'DEN', 'DE company', 'DE');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkDe, 'DEB', @GcPkDe);
				DECLARE @GcPkNz UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkNz UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkNz, 'NZN', 'NZ company', 'NZ');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkNz, 'NZB', @GcPkNz);
				DECLARE @GcPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGb UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkGb, 'GBN', 'GB company', 'GB');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkGb, 'GBB', @GcPkGb);
				DECLARE @GcPkFr UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkFr UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkFr, 'FRN', 'FR company', 'FR');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkFr, 'FRB', @GcPkFr);
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_ISBooking) VALUES
				(@JsPk, 'SHP01', 1, 0);
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ApplicationCode, JE_ClusterKey, JE_JS) VALUES
					(@ZAJePk01, 'ZA', 'DEC01', 'EXP', @GbPkZa, @GcPkZa, '2014-02-13', 'US1', 'BLT', 1, null),
					(@ZAJePk02, 'ZA', 'DEC02', 'EXP', @GbPkZa, @GcPkZa, '2014-02-28', 'US2', 'BLT', 2, null),
					(@ZAJePk03, 'ZA', 'DEC03', 'EXP', @GbPkZa, @GcPkZa, '2014-02-02', 'US3', 'BLT', 3, null),
					(newid(), 'ZA', 'DEC04', 'XXX', @GbPkZa, @GcPkZa, '2014-02-27', 'US4', 'BLT', 4, null),
					(newid(), 'ZA', 'DEC05', 'XXX', @GbPkZa, @GcPkZa, '2014-03-01', 'US5', 'BLT', 5, null),
					(newid(), 'ZA', 'DEC06', 'XXX', @GbPkZa, @GcPkZa, '2014-02-05', 'US6', 'BLT', 6, null),
					(@DEJePk01, 'DE', 'DEC07', 'EXP', @GbPkDe, @GcPkDe, '2014-02-13', 'US7', 'BLT', 7, null),
					(@DEJePk02, 'DE', 'DEC08', 'EXP', @GbPkDe, @GcPkDe, '2014-02-28', 'US8', 'BLT', 8, null),
					(@DEJePk03, 'DE', 'DEC09', 'EXP', @GbPkDe, @GcPkDe, '2014-02-02', 'US9', 'BLT', 9, null),
					(newid(), 'DE', 'DEC010', 'XXX', @GbPkDe, @GcPkDe, '2014-02-27', 'US0', 'BLT', 10, null),
					(newid(), 'DE', 'DEC011', 'XXX', @GbPkDe, @GcPkDe, '2014-03-01', 'USA', 'BLT', 11, null),
					(newid(), 'DE', 'DEC012', 'XXX', @GbPkDe, @GcPkDe, '2014-02-05', 'USB', 'BLT', 12, null),
					(@NZJePk01, 'NZ', 'DEC13', 'EXP', @GbPkNz, @GcPkNz, '2014-02-02', 'USC', 'BLT', 13, null),
					(@GBJePk01, 'GB', 'DEC14', 'EXP', @GbPkGb, @GcPkGb, '2014-02-13', 'USD', 'CDS', 14, null),
					(@GBJePk02, 'GB', 'DEC15', 'EXW', @GbPkGb, @GcPkGb, '2014-02-13', 'USE', 'CDS', 15, null),
					('{JE1}', 'FR', 'DEC16', 'EXP', @GbPkFr, @GcPkFr, '2014-02-02', 'US0', 'DG', 16, null),
					('{JE2}', 'FR', 'DEC17', 'EXP', @GbPkFr, @GcPkFr, '2014-02-02', 'US0', 'DI', 17, @JsPk);
				INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES
					(newid(), 'ZA', @ZAJePk01, 'BGM Reference 1', NULL, 1, getutcdate(), '~BP', getutcdate(), '~BP'),
					(newid(), 'ZA', @ZAJePk01, 'BGM Reference 1', '2014-02-28', 1, '2014-02-13', 'US1', '2014-02-13', 'US1'),
					(newid(), 'ZA', @ZAJePk02, 'BGM Reference 2', '2014-02-28', 2, '2014-02-13', 'US2', '2014-02-13', 'US2'),
					(newid(), 'ZA', @ZAJePk02, 'BGM Reference 2', '2014-03-01', 2, '2014-02-13', 'US3', '2014-02-13', 'US3'),
					(newid(), 'DE', @DEJePk01, 'BGM Reference 3', NULL, 7, '2014-02-13', 'US4', '2014-02-13', 'US4'),
					(newid(), 'DE', @DEJePk01, 'BGM Reference 3', '2014-02-28', 7, '2014-02-13', 'US5', '2014-02-13', 'US5'),
					(newid(), 'DE', @DEJePk02, 'BGM Reference 4', '2014-02-28', 8, '2014-02-13', 'US6', '2014-02-13', 'US6'),
					(newid(), 'DE', @DEJePk02, 'BGM Reference 4', '2014-03-01', 8, '2014-02-13', 'US7', '2014-02-13', 'US7'),
					(newid(), 'GB', @GBJePk01, 'BGM Reference 5', '2014-02-27', 14, '2014-02-13', 'US8', '2014-02-13', 'US8'),
					(newid(), 'GB', @GBJePk01, 'BGM Reference 5', '2014-02-27', 14, '2014-02-13', 'US9', '2014-02-13', 'US9'),
					(newid(), 'GB', @GBJePk02, 'BGM Reference 6', '2014-02-27', 15, '2014-02-13', 'USA', '2014-02-13', 'USA'),
					(newid(), 'GB', @GBJePk02, 'BGM Reference 6', '2014-02-27', 15, '2014-02-13', 'USB', '2014-02-13', 'USB');
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status, JH_IsDisbursement) VALUES
					('{JH1}', @GcPkFr, '{JE1}', 'JE01', '2014-02-02', @GbPkFr, @GePk, 'US1', 'WRK', 1),
					('{JH2}', @GcPkFr, '{JE2}', 'JE02', '2014-02-02', @GbPkFr, @GePk, 'US2', 'WRK', 0),
					(newid(), @GcPkFr, @JsPk, 'SHP01', '2014-02-02', @GbPkFr, @GePk, 'US1', 'WRK', 1);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 9, transactions.Count());

				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2014, 2, 2), "US3", 1, "DEC03", null, "ZA");
				AssertRowMatchingRef1(transactions, "2", "DEM", "DEM", new DateTime(2014, 2, 13), "US1", 1, "DEC01", null, "ZA");
				AssertRowMatchingRef1(transactions, "3", "DEM", "DEM", new DateTime(2014, 2, 28), "US2", 1, "DEC02", null, "ZA");
				AssertRowMatchingRef1(transactions, "4", "DEN", "DEB", new DateTime(2014, 2, 2), "US9", 1, "DEC09", null, "DE");
				AssertRowMatchingRef1(transactions, "5", "DEN", "DEB", new DateTime(2014, 2, 13), "US7", 1, "DEC07", null, "DE");
				AssertRowMatchingRef1(transactions, "6", "DEN", "DEB", new DateTime(2014, 2, 28), "US8", 1, "DEC08", null, "DE");
				AssertRowMatchingRef1(transactions, "7", "GBN", "GBB", new DateTime(2014, 2, 27), "USD", 1, "DEC14", "BGM Reference 5", "GB");
				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2014, 2, 2), "US3", 1, "DEC03", null, "ZA");
				AssertRowMatchingRef1(transactions, "2", "DEM", "DEM", new DateTime(2014, 2, 13), "US1", 1, "DEC01", null, "ZA");
				AssertRowMatchingRef1(transactions, "16", "FRN", "FRB", new DateTime(2014, 2, 2), "US0", 1, "DEC16", null, "FR", transactionReference04: JH1.ToString().ToUpper());
				AssertRowMatchingRef1(transactions, "17", "FRN", "FRB", new DateTime(2014, 2, 2), "US0", 1, "DEC17", null, "FR", transactionReference04: null);
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 2);
			}
		}

		readonly Guid JE1 = Guid.NewGuid();
		readonly Guid JE2 = Guid.NewGuid();
		readonly Guid JH1 = Guid.NewGuid();
		readonly Guid JH2 = Guid.NewGuid();
	}
}
