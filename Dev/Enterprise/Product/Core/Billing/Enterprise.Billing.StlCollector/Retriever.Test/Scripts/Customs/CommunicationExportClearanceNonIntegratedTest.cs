using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationExportClearanceNonIntegrated))]
	sealed class CommunicationExportClearanceNonIntegratedTest : RefStlScriptWithDefaultsTest
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
					(newid(), 'US', 'DECUS01', 'EXP', @GbPkUS, @GcPkUS, '2019-02-01', 'US1', 'SEA', '', 1),
					(newid(), 'PR', 'DECPR01', 'EXP', @GbPkPR, @GcPkPR, '2019-02-01', 'PR1', 'SEA', '', 2),
					(newid(), 'SG', 'DECSG01', 'EXP', @GbPkSG, @GcPkSG, '2019-02-01', 'SG1', 'SEA', '', 3),
					(newid(), 'AU', 'DECAU01', 'EXP', @GbPkAU, @GcPkAU, '2019-02-01', 'AU1', 'SEA', '', 4),
					(newid(), 'NZ', 'DECNZ01', 'EXP', @GbPkNZ, @GcPkNZ, '2019-02-01', 'NZ1', 'SEA', '', 5),
					(newid(), 'GB', 'DECGB01', 'EXP', @GbPkGB, @GcPkGB, '2019-02-01', 'GB1', 'SEA', '', 6),
					(newid(), 'CZ', 'DECCA01', 'EXP', @GbPkCA, @GcPkCA, '2019-02-01', 'CA1', 'SEA', '', 7),
					(newid(), 'ZA', 'DECZA01', 'EXP', @GbPkZA, @GcPkZA, '2019-02-01', 'ZA1', 'SEA', '', 8),
					(newid(), 'DE', 'DECDE01', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE1', 'SEA', '', 9),
					(newid(), 'DE', 'DECDE02', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE2', 'AIR', '', 10),
					(newid(), 'DE', 'DECDE03', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE3', 'ROA', '', 11),
					(newid(), 'DE', 'DECDE04', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE4', 'RAI', '', 12),
					(newid(), 'DE', 'DECDE05', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE5', 'TRK', '', 13),
					(newid(), 'DE', 'DECDE06', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE6', 'SEA', '', 14),
					(newid(), 'DE', 'DECDE07', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE7', 'SEA', 'ITF', 15),
					(newid(), 'DE', 'DECDE08', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE8', 'AIR', 'ITF', 16),
					(newid(), 'DE', 'DECDE09', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE9', 'ROA', 'ITF', 17),
					(newid(), 'DE', 'DECDE10', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DE0', 'RAI', 'ITF', 18),
					(newid(), 'DE', 'DECDE11', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DEA', 'TRK', 'ITF', 19),
					(newid(), 'DE', 'DECDE12', 'IMP', @GbPkDE, @GcPkDE, '2019-02-01', 'DEB', 'SEA', 'ITF', 20),
					(newid(), 'DE', 'DECDE13', 'EXP', @GbPkDE, @GcPkDE, '2019-02-01', 'DEC', 'SEA', 'BLT', 21);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 11, transactions.Count());

				AssertRowMatchingRef1(transactions, "1", "DEN", "DEB", new DateTime(2019, 02, 01), "DE0", 1, "DECDE10", "DE");
				AssertRowMatchingRef1(transactions, "2", "DEN", "DEB", new DateTime(2019, 02, 01), "DE1", 1, "DECDE01", "DE");
				AssertRowMatchingRef1(transactions, "3", "DEN", "DEB", new DateTime(2019, 02, 01), "DE2", 1, "DECDE02", "DE");
				AssertRowMatchingRef1(transactions, "4", "DEN", "DEB", new DateTime(2019, 02, 01), "DE3", 1, "DECDE03", "DE");
				AssertRowMatchingRef1(transactions, "5", "DEN", "DEB", new DateTime(2019, 02, 01), "DE4", 1, "DECDE04", "DE");
				AssertRowMatchingRef1(transactions, "6", "DEN", "DEB", new DateTime(2019, 02, 01), "DE5", 1, "DECDE05", "DE");
				AssertRowMatchingRef1(transactions, "7", "DEN", "DEB", new DateTime(2019, 02, 01), "DE7", 1, "DECDE07", "DE");
				AssertRowMatchingRef1(transactions, "8", "DEN", "DEB", new DateTime(2019, 02, 01), "DE8", 1, "DECDE08", "DE");
				AssertRowMatchingRef1(transactions, "9", "DEN", "DEB", new DateTime(2019, 02, 01), "DE9", 1, "DECDE09", "DE");
				AssertRowMatchingRef1(transactions, "10", "DEN", "DEB", new DateTime(2019, 02, 01), "DEA", 1, "DECDE11", "DE");
				AssertRowMatchingRef1(transactions, "11", "ZAN", "ZAB", new DateTime(2019, 02, 01), "ZA1", 1, "DECZA01", "ZA");
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2019, 2);
	}
}
