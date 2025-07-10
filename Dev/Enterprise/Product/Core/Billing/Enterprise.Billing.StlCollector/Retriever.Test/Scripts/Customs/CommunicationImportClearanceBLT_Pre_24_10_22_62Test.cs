using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationImportClearanceBLT_Pre_24_10_22_62))]
	sealed class CommunicationImportClearanceBLT_Pre_24_10_22_62Test : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @ZAJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @ZAJePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @ZAJePk03 UNIQUEIDENTIFIER = newid();
				DECLARE @DEJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @DEJePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @DEJePk03 UNIQUEIDENTIFIER = newid();
				DECLARE @NZJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GBJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GBJePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @IEJePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkZa UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkZa UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkZa);
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
				DECLARE @GcPkIe UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkIe UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkIe, 'IEN', 'IE company', 'IE');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkIe, 'IEB', @GcPkIe);
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ApplicationCode, JE_ClusterKey) VALUES
					(@ZAJePk01, 'ZA', 'DEC01', 'IMP', @GbPkZa, @GcPkZa, '2010-02-13', 'US1', 'BLT', 1),
					(@ZAJePk02, 'ZA', 'DEC02', 'IMP', @GbPkZa, @GcPkZa, '2010-02-28', 'US2', 'BLT', 2),
					(@ZAJePk03, 'ZA', 'DEC03', 'IMP', @GbPkZa, @GcPkZa, '2010-02-02', 'US3', 'BLT', 3),
					(newid(), 'ZA', 'DEC04', 'EXW', @GbPkZa, @GcPkZa, '2010-02-27', 'US4', 'BLT', 4),
					(newid(), 'ZA', 'DEC05', 'IMX', @GbPkZa, @GcPkZa, '2010-03-01', 'US5', 'BLT', 5),
					(newid(), 'ZA', 'DEC06', 'EXP', @GbPkZa, @GcPkZa, '2010-02-05', 'US6', 'BLT', 6),
					(@DEJePk01, 'DE', 'DEC07', 'IMP', @GbPkDe, @GcPkDe, '2010-02-13', 'US7', 'BLT', 7),
					(@DEJePk02, 'DE', 'DEC08', 'IMP', @GbPkDe, @GcPkDe, '2010-02-28', 'US8', 'BLT', 8),
					(@DEJePk03, 'DE', 'DEC09', 'IMP', @GbPkDe, @GcPkDe, '2010-02-02', 'US9', 'BLT', 9),
					(newid(), 'DE', 'DEC10', 'EXW', @GbPkDe, @GcPkDe, '2010-02-27', 'US0', 'BLT', 10),
					(newid(), 'DE', 'DEC11', 'IMX', @GbPkDe, @GcPkDe, '2010-02-26', 'USA', 'BLT', 11),
					(newid(), 'DE', 'DEC12', 'EXP', @GbPkDe, @GcPkDe, '2010-02-05', 'USB', 'BLT', 12),
					(@NZJePk01, 'NZ', 'DEC13', 'IMP', @GbPkNz, @GcPkNz, '2010-02-02', 'USC', 'BLT', 13),
					(@GBJePk01, 'GB', 'DEC14', 'IMP', @GbPkGb, @GcPkGb, '2010-02-13', 'USD', 'CDS', 14),
					(@GBJePk02, 'GB', 'DEC15', 'EXW', @GbPkGb, @GcPkGb, '2010-02-13', 'USE', 'CDS', 15),
					(newid(), 'FR', 'DEC16', 'EXW', @GbPkFr, @GcPkFr, '2010-02-27', 'US0', 'DG', 16),
					(newid(), 'FR', 'DEC17', 'EXW', @GbPkFr, @GcPkFr, '2010-02-27', 'US0', 'DI', 17),
					(@IEJePk01, 'IE', 'DEC18', 'IMP', @GbPkIe, @GcPkIe, '2010-02-27', 'US0', 'V1', 18),
					(newid(), 'IE', 'DEC19', 'IMP', @GbPkIe, @GcPkIe, '2010-02-27', 'US0', 'V2', 19);
				INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES
					(newid(), 'ZA', @ZAJePk01, 'BGM Reference 1', NULL, 1, getutcdate(), '~BP', getutcdate(), '~BP'),
					(newid(), 'ZA', @ZAJePk01, 'BGM Reference 1', '2010-02-28', 1, '2014-02-13', 'US1', '2014-02-13', 'US1'),
					(newid(), 'ZA', @ZAJePk02, 'BGM Reference 2', '2010-02-28', 2, '2014-02-13', 'US2', '2014-02-13', 'US2'),
					(newid(), 'ZA', @ZAJePk02, 'BGM Reference 2', '2010-03-01', 2, '2014-02-13', 'US3', '2014-02-13', 'US3'),
					(newid(), 'DE', @DEJePk01, 'BGM Reference 3', NULL, 7, '2014-02-13', 'US4', '2014-02-13', 'US4'),
					(newid(), 'DE', @DEJePk01, 'BGM Reference 3', '2010-02-28', 7, '2014-02-13', 'US5', '2014-02-13', 'US5'),
					(newid(), 'DE', @DEJePk02, 'BGM Reference 4', '2010-02-28', 8, '2014-02-13', 'US6', '2014-02-13', 'US6'),
					(newid(), 'DE', @DEJePk02, 'BGM Reference 4', '2010-03-01', 8, '2014-02-13', 'US7', '2014-02-13', 'US7'),
					(newid(), 'GB', @GBJePk01, 'BGM Reference 5', '2010-02-27', 14, '2014-02-13', 'US8', '2014-02-13', 'US8'),
					(newid(), 'GB', @GBJePk01, 'BGM Reference 5', '2010-02-27', 14, '2014-02-13', 'US9', '2014-02-13', 'US9'),
					(newid(), 'GB', @GBJePk02, 'BGM Reference 6', '2010-02-27', 15, '2014-02-13', 'USA', '2014-02-13', 'USA'),
					(newid(), 'GB', @GBJePk02, 'BGM Reference 6', '2010-02-27', 15, '2014-02-13', 'USB', '2014-02-13', 'USB'),
					(newid(), 'GB', @GBJePk02, 'BGM Reference 6', '2010-02-27', 15, '2014-02-13', 'USA', '2014-02-13', 'USA'),
					(newid(), 'IE', @IEJePk01, 'BGM Reference IE1', '2010-02-27', 18, '2014-02-13', 'US0', '2014-02-13', 'US0'),
					(newid(), 'IE', @IEJePk01, 'BGM Reference IE2', '2010-02-28', 18, '2014-02-13', 'US0', '2014-02-13', 'US0');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 15, transactions.Count());

				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2010, 2, 2), "US3", 1, "DEC03", null, "ZA");
				AssertRowMatchingRef1(transactions, "2", "DEM", "DEM", new DateTime(2010, 2, 13), "US1", 1, "DEC01", null, "ZA");
				AssertRowMatchingRef1(transactions, "3", "DEM", "DEM", new DateTime(2010, 2, 27), "US4", 1, "DEC04", null, "ZA");
				AssertRowMatchingRef1(transactions, "4", "DEM", "DEM", new DateTime(2010, 2, 28), "US2", 1, "DEC02", null, "ZA");
				AssertRowMatchingRef1(transactions, "5", "DEN", "DEB", new DateTime(2010, 2, 2), "US9", 1, "DEC09", null, "DE");
				AssertRowMatchingRef1(transactions, "6", "DEN", "DEB", new DateTime(2010, 2, 13), "US7", 1, "DEC07", null, "DE");
				AssertRowMatchingRef1(transactions, "7", "DEN", "DEB", new DateTime(2010, 2, 26), "USA", 1, "DEC11", null, "DE");
				AssertRowMatchingRef1(transactions, "8", "DEN", "DEB", new DateTime(2010, 2, 27), "US0", 1, "DEC10", null, "DE");
				AssertRowMatchingRef1(transactions, "9", "DEN", "DEB", new DateTime(2010, 2, 28), "US8", 1, "DEC08", null, "DE");
				AssertRowMatchingRef1(transactions, "10", "GBN", "GBB", new DateTime(2010, 2, 27), "USD", 1, "DEC14", "BGM Reference 5", "GB");
				AssertRowMatchingRef1(transactions, "16", "FRN", "FRB", new DateTime(2010, 2, 27), "US0", 1, "DEC16", null, "FR");
				AssertRowMatchingRef1(transactions, "17", "FRN", "FRB", new DateTime(2010, 2, 27), "US0", 1, "DEC17", null, "FR");
				AssertRowMatchingRef1AndRef2(transactions, "18", "IEN", "IEB", new DateTime(2010, 2, 27), "US0", 1, "DEC18", null, "IE");
				AssertRowMatchingRef1(transactions, "19", "IEN", "IEB", new DateTime(2010, 2, 27), "US0", 1, "DEC19", null, "IE");
				AssertRowMatchingRef1AndRef2(transactions, "20", "IEN", "IEB", new DateTime(2010, 2, 28), "US0", 1, "DEC18", "BGM Reference IE2", "IE");
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2010, 2);
			}
		}
	}
}
