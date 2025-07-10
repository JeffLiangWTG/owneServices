using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationImportClearanceGeneral_Pre_24_10_22_62))]
	sealed class CommunicationImportClearanceGeneral_Pre_24_10_22_62Test : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkAu UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkCa UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkZa UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkNz UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkUs UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkPr UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkAu UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkCa UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkZA UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkNz UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkUs UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkPr UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPkSg, 'SGN', 'SG company', 'SG'),
					(@GcPkAu, 'AUN', 'AU company', 'AU'),
					(@GcPkCa, 'CAN', 'CA company', 'CA'),
					(@GcPkZa, 'ZAN', 'ZA company', 'ZA'),
					(@GcPkNz, 'NZN', 'NZ company', 'NZ'),
					(@GcPkUs, 'USN', 'US company', 'US'),
					(@GcPkPr, 'PRN', 'PR company', 'PR');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkSg, 'SGB', @GcPkSg),
					(@GbPkAu, 'AUB', @GcPkAu),
					(@GbPkCa, 'CAB', @GcPkCa),
					(@GbPkZA, 'ZAB', @GcPkZa),
					(@GbPkNz, 'NZB', @GcPkNz),
					(@GbPkUS, 'USB', @GcPkUs),
					(@GbPkPR, 'PRB', @GcPkPr);
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_TransportMode, JE_ApplicationCode, JE_MessageSubType, JE_ClusterKey) VALUES
					(newid(), 'SG', 'DEC01', 'INP', @GbPkSg, @GcPkSg, '2010-02-13', 'US1', 'SEA', '', '', 1),
					(newid(), 'AU', 'DEC02', 'EXW', @GbPkAu, @GcPkAu, '2010-02-28', 'US2', 'AIR', '', '', 2),
					(newid(), 'CA', 'DEC031', 'IMP', @GbPkCa, @GcPkCa, '2010-02-02', 'US3', 'SEA', '', '', 31),
					(newid(), 'CA', 'DEC032', 'IMP', @GbPkCa, @GcPkCa, '2010-02-02', 'US3', 'RAI', '', '', 32),
					(newid(), 'SG', 'DEC04', 'XXX', @GbPkSg, @GcPkSg, '2010-02-27', 'US4', 'ROA', '', '', 4),
					(newid(), 'SG', 'DEC05', 'XXX', @GbPkSg, @GcPkSg, '2010-02-27', 'US5', 'RAI', '', '', 5),
					(newid(), 'AU', 'DEC06', 'IMP', @GbPkAu, @GcPkAu, '2010-03-01', 'US6', 'ROA', '', '', 6),
					(newid(), 'AU', 'DEC07', 'IMP', @GbPkAu, @GcPkAu, '2010-03-01', 'US6', 'RAI', '', '', 7),
					(newid(), 'CA', 'DEC08', 'XXX', @GbPkCa, @GcPkCa, '2010-02-05', 'US7', 'ROA', '', '', 8),
					(newid(), 'CA', 'DEC09', 'XXX', @GbPkCa, @GcPkCa, '2010-02-05', 'US7', 'RAI', '', '', 9),
					(newid(), 'ZA', 'DEC10', 'IMP', @GbPkZa, @GcPkZa, '2010-02-02', 'US7', '', '', '', 10),
					(newid(), 'ZA', 'DEC11', 'IMP', @GbPkZa, @GcPkZa, '2010-02-02', 'US8', 'ROA', 'BLT', '', 11),
					(newid(), 'NZ', 'DEC12', 'IMP', @GbPkNz, @GcPkNz, '2010-02-02', 'US9', 'AIR', '', 'IPI', 12),
					(newid(), 'US', 'DECUS1', 'IMP', @GbPkUs, @GcPkUs, '2010-02-02', 'US9', 'AIR', '', '', 13),
					(newid(), 'US', 'DECUS2', 'EXP', @GbPkUs, @GcPkUs, '2010-02-02', 'US9', 'AIR', '', '', 14),
					(newid(), 'US', 'DECUS3', 'IMP', @GbPkUs, @GcPkUs, '2010-02-02', 'US9', 'ROA', '', '', 15),
					(newid(), 'US', 'DECUS4', 'IMP', @GbPkUs, @GcPkUs, '2010-02-02', 'US9', 'AIR', '', '', 16),
					(newid(), 'PR', 'DECPR1', 'IMP', @GbPkPr, @GcPkPr, '2010-02-02', 'US9', 'SEA', '', '', 17),
					(newid(), 'PR', 'DECPR2', 'EXP', @GbPkPr, @GcPkPr, '2010-02-02', 'US9', 'AIR', '', '', 18),
					(newid(), 'PR', 'DECPR3', 'IMP', @GbPkPr, @GcPkPr, '2010-02-02', 'US9', 'TRK', '', '', 19),
					(newid(), 'PR', 'DECPR4', 'IMP', @GbPkPr, @GcPkPr, '2010-02-02', 'US9', 'MAI', '', '', 20),
					(newid(), 'CA', 'DEC13', 'LVS', @GbPkCa, @GcPkCa, '2010-02-02', 'US3', 'SEA', '', '', 21),
					(newid(), 'CA', 'DEC14', 'LVS', @GbPkCa, @GcPkCa, '2010-02-02', 'US3', 'RAI', '', '', 22);

				DECLARE @GcPkDE UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkDE UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPkDE, 'DEN', 'DE company', 'DE');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkDE, 'DEB', @GcPkDE);

				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_TransportMode, JE_ApplicationCode, JE_ClusterKey) VALUES
					(newid(), 'DE', 'DECDE01', 'IMP', @GbPkDE, @GcPkDE, '2010-02-11', 'DE1', 'SEA', '', 23),
					(newid(), 'DE', 'DECDE02', 'EXP', @GbPkDE, @GcPkDE, '2010-02-11', 'DE2', 'SEA', 'BLT', 24),
					(newid(), 'DE', 'DECDE03', 'IMP', @GbPkDE, @GcPkDE, '2010-02-11', 'DE3', 'SEA', 'BLT', 25),
					(newid(), 'DE', 'DECDE04', 'IMP', @GbPkDE, @GcPkDE, '2010-02-11', 'DE4', 'SEA', 'ITF', 26),
					(newid(), 'DE', 'DECDE05', 'MSC', @GbPkDE, @GcPkDE, '2010-02-11', 'DE5', 'SEA', 'BLT', 27);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 8, transactions.Count());

				AssertRowMatchingRef1(transactions, "1", "AUN", "AUB", new DateTime(2010, 02, 28), "US2", 1, "DEC02", "AU");
				AssertRowMatchingRef1(transactions, "2", "CAN", "CAB", new DateTime(2010, 02, 02), "US3", 1, "DEC031", "CA");
				AssertRowMatchingRef1(transactions, "3", "NZN", "NZB", new DateTime(2010, 02, 02), "US9", 1, "DEC12", "NZ");
				AssertRowMatchingRef1(transactions, "4", "PRN", "PRB", new DateTime(2010, 02, 02), "US9", 1, "DECPR1", "PR");
				AssertRowMatchingRef1(transactions, "5", "PRN", "PRB", new DateTime(2010, 02, 02), "US9", 1, "DECPR4", "PR");
				AssertRowMatchingRef1(transactions, "6", "SGN", "SGB", new DateTime(2010, 02, 13), "US1", 1, "DEC01", "SG");
				AssertRowMatchingRef1(transactions, "7", "USN", "USB", new DateTime(2010, 02, 02), "US9", 1, "DECUS1", "US");
				AssertRowMatchingRef1(transactions, "8", "USN", "USB", new DateTime(2010, 02, 02), "US9", 1, "DECUS4", "US");
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
