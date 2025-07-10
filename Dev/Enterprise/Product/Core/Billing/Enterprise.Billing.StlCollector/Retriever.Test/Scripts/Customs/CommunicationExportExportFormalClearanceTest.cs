using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationExportExportFormalClearance))]
	sealed class CommunicationExportExportFormalClearanceTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = $@"
				DECLARE @JePk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk04 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk UNIQUEIDENTIFIER = newid(); 
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'EDI');
				DECLARE @GcPkNz UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GcPkZa UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg ORDER BY GB_Code);
				DECLARE @GbPkAu UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkAu ORDER BY GB_Code);
				DECLARE @GbPkNz UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkNz ORDER BY GB_Code);
				DECLARE @GbPkZA UNIQUEIDENTIFIER = newid();
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);

				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'NZ', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkNz;
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES (@GcPkZa, 'ZAN', 'ZA company', 'ZA');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES(@GbPkZA, 'ZAB', @GcPkZa);

				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_ISBooking) VALUES
				(@JsPk, 'SHP01', 1, 0);
				
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_MessageSubType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ApplicationCode, JE_ClusterKey) VALUES
					(@JePk01, 'SG', 'DEC01', 'OUT', 'XXX', @GbPkSg, @GcPkSg, '2014-02-13', 'US1', '', 1),
					(@JePk02, 'AU', 'DEC02', 'EXP', 'XXX', @GbPkAu, @GcPkAu, '2014-02-28', 'US2', '', 2),
					(@JePk03, 'NZ', 'DEC03', 'EXP', 'NOR', @GbPkNz, @GcPkNz, '2014-02-02', 'US3', '', 3),
					(newid(), 'SG', 'DEC04', 'XXX', 'XXX', @GbPkSg, @GcPkSg, '2014-02-27', 'US4', '', 4),
					(newid(), 'AU', 'DEC05', 'EXP', 'XXX', @GbPkAu, @GcPkAu, '2014-03-01', 'US5', '', 5),
					(newid(), 'NZ', 'DEC06', 'EXP', 'XXX', @GbPkNz, @GcPkNz, '2014-02-05', 'US6', '', 6),
					(@JePk04, 'ZA', 'DEC07', 'EXP', 'XXX', @GbPkZa, @GcPkZa, '2014-02-02', 'US7', '', 7),
					(newid(), 'ZA', 'DEC08', 'EXP', 'XXX', @GbPkZa, @GcPkZa, '2014-02-02', 'US8', 'BLT', 8);

				DECLARE @GcPkUS UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkPR UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkCA UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkDE UNIQUEIDENTIFIER = newid();

				DECLARE @GbPkUS UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkPR UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkCA UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkDE UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPkUS, 'USN', 'US company', 'US'),
					(@GcPkPR, 'PRN', 'PR company', 'PR'),
					(@GcPkGB, 'GBN', 'GB company', 'GB'),
					(@GcPkCA, 'CAN', 'CA company', 'CA'),
					(@GcPkDE, 'DEN', 'DE company', 'DE');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkUS, 'USB', @GcPkUS),
					(@GbPkPR, 'PRB', @GcPkPR),
					(@GbPkGB, 'GBB', @GcPkGB),
					(@GbPkCA, 'CAB', @GcPkCA),
					(@GbPkDE, 'DEB', @GcPkDE);

				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_TransportMode, JE_ApplicationCode, JE_ClusterKey, JE_JS) VALUES
					('{JE2}', 'US', 'DEC09US', 'EXP', @GbPkUS, @GcPkUS, '2014-02-11', 'US1', 'SEA', '', 9, null),
					(newid(), 'PR', 'DEC10PR', 'EXP', @GbPkPR, @GcPkPR, '2014-02-11', 'PR1', 'SEA', '', 10, @JsPK),
					(newid(), 'GB', 'DEC11GB', 'EXP', @GbPkGB, @GcPkGB, '2014-02-11', 'GB1', 'SEA', '', 11, null),
					(newid(), 'CA', 'DEC12CA', 'EXP', @GbPkCA, @GcPkCA, '2014-02-11', 'CA1', 'SEA', '', 12, null),
					(newid(), 'DE', 'DEC13DE', 'EXP', @GbPkDE, @GcPkDE, '2014-02-11', 'DE1', 'SEA', '', 13, null),
					(newid(), 'DE', 'DEC14DE', 'IMP', @GbPkDE, @GcPkDE, '2014-02-11', 'DE2', 'SEA', 'BLT', 14, null),
					(newid(), 'DE', 'DEC15DE', 'EXP', @GbPkDE, @GcPkDE, '2014-02-11', 'DE3', 'SEA', 'BLT', 15, null),
					(newid(), 'DE', 'DEC16DE', 'EXP', @GbPkDE, @GcPkDE, '2014-02-11', 'DE4', 'SEA', 'ITF', 16, null);

				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status, JH_IsDisbursement) VALUES
					('{JH1}', @GcPkSg, @JePk01, 'JE01', '2014-02-13', @GbPkSg, @GePk, 'US1', 'WRK', 1),
					('{JH2}', @GcPkUS, '{JE2}', 'JE02', '2014-02-11', @GbPkUS, @GePk, 'US2', 'WRK', 0),
					(newid(), @GcPkSg, @JsPk, 'SHP01', '2014-02-10', @GbPkSg, @GePk, 'US1', 'WRK', 1);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 7, transactions.Count());
				AssertRowMatchingRef1(transactions, "1", "CAN", "CAB", new DateTime(2014, 2, 11), "CA1", 1, "DEC12CA", "CA");
				AssertRowMatchingRef1(transactions, "2", "DEM", "DEM", new DateTime(2014, 2, 2), "US3", 1, "DEC03", "NZ");
				AssertRowMatchingRef1(transactions, "3", "EDI", "BNE", new DateTime(2014, 2, 28), "US2", 1, "DEC02", "AU");
				AssertRowMatchingRef1(transactions, "4", "GBN", "GBB", new DateTime(2014, 2, 11), "GB1", 1, "DEC11GB", "GB");
				AssertRowMatchingRef1(transactions, "5", "PRN", "PRB", new DateTime(2014, 2, 11), "PR1", 1, "DEC10PR", "PR", transactionReference04: null);
				AssertRowMatchingRef1(transactions, "6", "SIN", "SIN", new DateTime(2014, 2, 13), "US1", 1, "DEC01", "SG", transactionReference04: JH1.ToString().ToUpper());
				AssertRowMatchingRef1(transactions, "7", "USN", "USB", new DateTime(2014, 2, 11), "US1", 1, "DEC09US", "US");
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 2);
			}
		}

		readonly Guid JE2 = Guid.NewGuid();
		readonly Guid JH1 = Guid.NewGuid();
		readonly Guid JH2 = Guid.NewGuid();
	}
}
