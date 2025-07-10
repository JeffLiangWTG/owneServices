using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationImportLandBorderClearances))]
	sealed class CommunicationImportLandBorderClearancesTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @JePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk17 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk18 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'EDI');
				DECLARE @GcPkCa UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GcPkNz UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkZa UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkIe UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg ORDER BY GB_Code);
				DECLARE @GbPkAu UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkAu ORDER BY GB_Code);
				DECLARE @GbPkCa UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkCa ORDER BY GB_Code);
				DECLARE @GbPkNz UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkIe UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkNz, 'NZN', 'NZ company', 'NZ');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkNz, 'NZB', @GcPkNz);
				DECLARE @GbPkZa UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkZa, 'ZAN', 'ZA company', 'ZA');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkZa, 'ZAB', @GcPkZa);
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkGb, 'GBN', 'GB company', 'GB');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkGb, 'GBB', @GcPkGb);
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkIe, 'IEN', 'IE company', 'IE');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkIe, 'IEB', @GcPkIe);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'CA', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkCa;
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_TransportMode,JE_ApplicationCode, JE_MessageSubType, JE_ClusterKey) VALUES
					(@JePk01, 'SG', 'DEC01', 'INP', @GbPkSg, @GcPkSg, '2010-02-13', 'US1', 'TRK', 'BLT', '', 1),
					(@JePk02, 'AU', 'DEC02', 'EXW', @GbPkAu, @GcPkAu, '2010-02-28', 'US2', 'ROA', 'BLT', '', 2),
					(@JePk03, 'CA', 'DEC03', 'IMP', @GbPkCa, @GcPkCa, '2010-02-02', 'US3', 'RAI', 'BLT', '', 3),
					(newid(), 'SG', 'DEC04', 'XXX', @GbPkSg, @GcPkSg, '2010-02-27', 'US4', 'SEA', 'BLT', '', 4),
					(newid(), 'SG', 'DEC05', 'XXX', @GbPkSg, @GcPkSg, '2010-02-27', 'US5', 'AIR', 'BLT', '', 5),
					(newid(), 'AU', 'DEC06', 'IMP', @GbPkAu, @GcPkAu, '2010-03-01', 'US6', 'MAI', 'BLT', '', 6),
					(newid(), 'NZ', 'DEC07', 'IMP', @GbPkNz, @GcPkNz, '2010-02-02', 'US7', 'TRK', 'BLT', 'IPI', 7),
					(newid(), 'ZA', 'DEC08', 'IMP', @GbPkZa, @GcPkZa, '2010-02-03', 'US8', 'TRK', 'ITF', '', 8),
					(newid(), 'ZA', 'DEC09', 'EXW', @GbPkZa, @GcPkZa, '2010-02-04', 'US9', 'RAI', 'ITF', '', 9),
					(newid(), 'ZA', 'DEC10', 'IMP', @GbPkZa, @GcPkZa, '2010-02-05', 'US0', 'ROA', 'ITF', '', 10),
					(newid(), 'ZA', 'DEC11', 'IMP', @GbPkZa, @GcPkZa, '2010-02-03', 'USA', 'TRK', 'BLT', '', 11),
					(newid(), 'ZA', 'DEC12', 'EXW', @GbPkZa, @GcPkZa, '2010-02-04', 'USB', 'RAI', 'BLT', '', 12),
					(newid(), 'ZA', 'DEC13', 'IMP', @GbPkZa, @GcPkZa, '2010-02-05', 'USC', 'ROA', 'BLT', '', 13),
					(newid(), 'GB', 'DEC14', 'IMP', @GbPkGb, @GcPkGb, '2010-02-11', 'USD', 'ROA', 'CDS', '', 14),
					(newid(), 'GB', 'DEC15', 'EXW', @GbPkGb, @GcPkGb, '2010-02-13', 'USE', 'ROA', 'CDS', '', 15),
					(newid(), 'CA', 'DEC161', 'LVS', @GbPkCa, @GcPkCa, '2010-02-02', 'USF', 'AIR', 'BLT', '', 161),
					(newid(), 'CA', 'DEC162', 'LVS', @GbPkCa, @GcPkCa, '2010-02-02', 'USF', 'RAI', 'BLT', '', 162),
					(@JePk17, 'IE', 'DEC17', 'IMP', @GbPkIe, @GcPkIe, '2010-02-02', 'USF', 'ROA', 'V1', '', 17),
					(@JePk18, 'IE', 'DEC18', 'IMP', @GbPkIe, @GcPkIe, '2010-02-02', 'USF', 'RAI', 'V2', '', 18);
				INSERT dbo.CusEntryHeader (CH_PK, CH_JE, CH_EntrySubmittedDate, CH_BGMReference, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser, CH_ClusterKey, CH_DataModel) VALUES
					(newid(), @JePk17, '2010-02-02', 'CH170', '2010-02-02', 'USF', '2010-02-02', 'USF', 17, 'IE'),
					(newid(), @JePk17, '2010-02-03', 'CH171', '2010-02-02', 'USF', '2010-02-02', 'USF', 17, 'IE'),
					(newid(), @JePk18, '2010-02-02', 'CH180', '2010-02-02', 'USF', '2010-02-02', 'USF', 18, 'IE');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 14, transactions.Count());

				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2010, 2, 2), "US3", 1, "DEC03", null, "CA");
				AssertRowMatchingRef1(transactions, "2", "EDI", "BNE", new DateTime(2010, 2, 28), "US2", 1, "DEC02", null, "AU");
				AssertRowMatchingRef1(transactions, "3", "GBN", "GBB", new DateTime(2010, 2, 11), "USD", 1, "DEC14", null, "GB");
				AssertRowMatchingRef1(transactions, "4", "NZN", "NZB", new DateTime(2010, 2, 2), "US7", 1, "DEC07", null, "NZ");
				AssertRowMatchingRef1(transactions, "5", "SIN", "SIN", new DateTime(2010, 2, 13), "US1", 1, "DEC01", null, "SG");
				AssertRowMatchingRef1(transactions, "6", "ZAN", "ZAB", new DateTime(2010, 2, 3), "US8", 1, "DEC08", null, "ZA");
				AssertRowMatchingRef1(transactions, "7", "ZAN", "ZAB", new DateTime(2010, 2, 3), "USA", 1, "DEC11", null, "ZA");
				AssertRowMatchingRef1(transactions, "8", "ZAN", "ZAB", new DateTime(2010, 2, 4), "US9", 1, "DEC09", null, "ZA");
				AssertRowMatchingRef1(transactions, "9", "ZAN", "ZAB", new DateTime(2010, 2, 4), "USB", 1, "DEC12", null, "ZA");
				AssertRowMatchingRef1(transactions, "10", "ZAN", "ZAB", new DateTime(2010, 2, 5), "US0", 1, "DEC10", null, "ZA");
				AssertRowMatchingRef1(transactions, "11", "ZAN", "ZAB", new DateTime(2010, 2, 5), "USC", 1, "DEC13", null, "ZA");
				AssertRowMatchingRef1AndRef2(transactions, "12", "IEN", "IEB", new DateTime(2010, 2, 2), "USF", 1, "DEC17", null, "IE");
				AssertRowMatchingRef1(transactions, "13", "IEN", "IEB", new DateTime(2010, 2, 2), "USF", 1, "DEC18", null, "IE");
				AssertRowMatchingRef1AndRef2(transactions, "14", "IEN", "IEB", new DateTime(2010, 2, 3), "USF", 1, "DEC17", "CH171", "IE");
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
