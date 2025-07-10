using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderAirCargoReportExport))]
	sealed class OtherPartiesForwarderAirCargoReportExportTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = newid();
				DECLARE @GbPk UNIQUEIDENTIFIER = newid();
				DECLARE @CePkGe UNIQUEIDENTIFIER = newid();
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_TransportMode) VALUES
					(@JkPk01, 'JKAIR', 'AIR'),
					(@JkPk02, 'JKSEA', 'SEA');
				INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_RN_NKCountryCode, CE_EntryType, CE_Category, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @JkPk01, 'JobConsol', 'CPG01', 'AU', 'CCN', 'CUS', '2014-01-01', 'US1', getutcdate(), '~BP'),
					(newid(), @JkPk01, 'JobConsol', 'CPG02', 'NZ', 'CCN', 'CUS', '2014-09-01', 'US2', getutcdate(), '~BP'),
					(newid(), @JkPk01, 'JobConsol', 'CPG03', 'AU', 'CRN', 'XXX', '2014-09-21', 'US3', getutcdate(), '~BP'),
					(newid(), @JkPk01, 'JobConsol', 'CPG04', 'AU', 'CRN', 'CUS', '2014-09-24', 'US4', getutcdate(), '~BP'),
					(newid(), @JkPk02, 'JobConsol', 'CPG05', 'AU', 'CCN', 'CUS', '2014-09-25', 'US5', getutcdate(), '~BP'),
					(newid(), @JkPk01, 'JobConsol', 'CPG06', 'AU', 'CRN', 'CUS', '2014-08-31', 'US6', getutcdate(), '~BP');
				INSERT dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES(@GcPk, 'AUY', 'AU company', 'AUD', 'AU');
				INSERT dbo.GlbBranch(GB_PK, GB_Code, GB_GC) VALUES(@GbPk, 'AUH', @GcPk);
				INSERT dbo.GlbDepartment (GE_PK) VALUES ( @CePkGe );
				INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_ApplicationCode, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPk, @CePkGe, 'CMR', @JkPk01, 'JobConsol', '2014-09-24', 'TRX', 'ESM', 'ORG', 'SNT', 'US1', '2014-09-24', 'US1');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] CompanyCode", "AUY", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "AUH", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 24), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US4", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "CPG04", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "JKAIR", transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 9);
			}
		}
	}
}
