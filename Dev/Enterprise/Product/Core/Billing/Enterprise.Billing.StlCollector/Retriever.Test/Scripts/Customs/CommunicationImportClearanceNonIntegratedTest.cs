using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationImportClearanceNonIntegrated))]
	sealed class CommunicationImportClearanceNonIntegratedTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPkUS UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkPR UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSG UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkAU UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkNZ UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkCA UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkZA UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkDE UNIQUEIDENTIFIER = newid();

				DECLARE @GbPkUS UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkPR UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkSG UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkAU UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkNZ UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkCA UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkZA UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkDE UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPkUS, 'USN', 'US company', 'US'),
					(@GcPkPR, 'PRN', 'PR company', 'PR'),
					(@GcPkSG, 'SGN', 'SG company', 'SG'),
					(@GcPkAU, 'AUN', 'AU company', 'AU'),
					(@GcPkNZ, 'NZN', 'NZ company', 'NZ'),
					(@GcPkGB, 'GBN', 'GB company', 'GB'),
					(@GcPkCA, 'CAN', 'CA company', 'CA'),
					(@GcPkZA, 'ZAN', 'ZA company', 'ZA'),
					(@GcPkDE, 'DEN', 'DE company', 'DE');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkUS, 'USB', @GcPkUS),
					(@GbPkPR, 'PRB', @GcPkPR),
					(@GbPkSG, 'SGB', @GcPkSG),
					(@GbPkAU, 'AUB', @GcPkAU),
					(@GbPkNZ, 'NZB', @GcPkNZ),
					(@GbPkGB, 'GBB', @GcPkGB),
					(@GbPkCA, 'CAB', @GcPkCA),
					(@GbPkZA, 'ZAB', @GcPkZA),
					(@GbPkDE, 'DEB', @GcPkDE);

				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_TransportMode, JE_ApplicationCode, JE_ClusterKey) VALUES
					(newid(), 'US', 'DECUS01', 'IMP', @GbPkUS, @GcPkUS, '2019-02-01', 'US1', 'SEA', '', 1),
					(newid(), 'PR', 'DECPR01', 'IMP', @GbPkPR, @GcPkPR, '2019-02-01', 'PR1', 'SEA', '', 2),
					(newid(), 'SG', 'DECSG01', 'IMP', @GbPkSG, @GcPkSG, '2019-02-01', 'SG1', 'SEA', '', 3),
					(newid(), 'AU', 'DECAU01', 'IMP', @GbPkAU, @GcPkAU, '2019-02-01', 'AU1', 'SEA', '', 4),
					(newid(), 'NZ', 'DECNZ01', 'IMP', @GbPkNZ, @GcPkNZ, '2019-02-01', 'NZ1', 'SEA', '', 5),
					(newid(), 'GB', 'DECGB01', 'IMP', @GbPkGB, @GcPkGB, '2019-02-01', 'GB1', 'SEA', '', 6),
					(newid(), 'CA', 'DECCA01', 'IMP', @GbPkCA, @GcPkCA, '2019-02-01', 'CA1', 'SEA', '', 7),
					(newid(), 'ZA', 'DECZA01', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA1', 'SEA', '', 8),
					(newid(), 'ZA', 'DECZA02', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA2', 'AIR', '', 9),
					(newid(), 'ZA', 'DECZA03', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA3', 'ROA', '', 10),
					(newid(), 'ZA', 'DECZA04', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA4', 'RAI', '', 11),
					(newid(), 'ZA', 'DECZA05', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA5', 'TRK', '', 12),
					(newid(), 'ZA', 'DECZA06', 'MSC', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA6', 'SEA', '', 13),
					(newid(), 'ZA', 'DECZA07', 'IMX', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA7', 'SEA', '', 14),
					(newid(), 'ZA', 'DECZA08', 'EXP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA8', 'SEA', '', 15),
					(newid(), 'ZA', 'DECZA09', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA9', 'SEA', 'ITF', 16),
					(newid(), 'ZA', 'DECZA10', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA0', 'AIR', 'ITF', 17),
					(newid(), 'ZA', 'DECZA11', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZAA', 'ROA', 'ITF', 18),
					(newid(), 'ZA', 'DECZA12', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZAB', 'RAI', 'ITF', 19),
					(newid(), 'ZA', 'DECZA13', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZAC', 'TRK', 'ITF', 20),
					(newid(), 'ZA', 'DECZA14', 'IMP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZAD', 'ROA', 'ITF', 21),
					(newid(), 'ZA', 'DECZA15', 'MSC', @GbPkZA, @GcPkZA, '2019-02-01', 'ZAE', 'SEA', 'ITF', 22),
					(newid(), 'ZA', 'DECZA16', 'IMX', @GbPkZA, @GcPkZA, '2019-02-01', 'ZAF', 'SEA', 'ITF', 23),
					(newid(), 'ZA', 'DECZA17', 'EXP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZAG', 'SEA', 'ITF', 24),
					(newid(), 'DE', 'DECDE01', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE1', 'SEA', 'ITF', 25),
					(newid(), 'DE', 'DECDE02', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE2', 'AIR', 'ITF', 26),
					(newid(), 'DE', 'DECDE03', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE3', 'ROA', 'ITF', 27),
					(newid(), 'DE', 'DECDE04', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE4', 'RAI', 'ITF', 28),
					(newid(), 'DE', 'DECDE05', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE5', 'TRK', 'ITF', 29),
					(newid(), 'DE', 'DECDE06', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE6', 'SEA', 'ITF', 30),
					(newid(), 'DE', 'DECDE07', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE7', 'SEA', '', 31),
					(newid(), 'DE', 'DECDE08', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE8', 'AIR', '', 32),
					(newid(), 'DE', 'DECDE09', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE9', 'ROA', '', 33),
					(newid(), 'DE', 'DECDE10', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE0', 'RAI', '', 34),
					(newid(), 'DE', 'DECDE11', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DEA', 'TRK', '', 35),
					(newid(), 'DE', 'DECDE12', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DEB', 'SEA', '', 36),
					(newid(), 'DE', 'DECDE13', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DEC', 'SEA', 'BLT', 37);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 8, transactions.Count());
				AssertRowMatchingRef1(transactions, "1", "DEN", "DEB", new DateTime(2019, 02, 01), "DE1", 1, "DECDE01", "DE");
				AssertRowMatchingRef1(transactions, "2", "DEN", "DEB", new DateTime(2019, 02, 01), "DE2", 1, "DECDE02", "DE");
				AssertRowMatchingRef1(transactions, "3", "DEN", "DEB", new DateTime(2019, 02, 01), "DE7", 1, "DECDE07", "DE");
				AssertRowMatchingRef1(transactions, "4", "DEN", "DEB", new DateTime(2019, 02, 01), "DE8", 1, "DECDE08", "DE");
				AssertRowMatchingRef1(transactions, "5", "ZAN", "ZAB", new DateTime(2019, 02, 01), "ZA0", 1, "DECZA10", "ZA");
				AssertRowMatchingRef1(transactions, "6", "ZAN", "ZAB", new DateTime(2019, 02, 01), "ZA1", 1, "DECZA01", "ZA");
				AssertRowMatchingRef1(transactions, "7", "ZAN", "ZAB", new DateTime(2019, 02, 01), "ZA2", 1, "DECZA02", "ZA");
				AssertRowMatchingRef1(transactions, "8", "ZAN", "ZAB", new DateTime(2019, 02, 01), "ZA9", 1, "DECZA09", "ZA");
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2019, 2);
	}
}
