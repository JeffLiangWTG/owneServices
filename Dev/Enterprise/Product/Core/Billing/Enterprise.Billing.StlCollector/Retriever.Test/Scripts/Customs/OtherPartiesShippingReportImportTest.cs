using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesShippingReportImport))]
	sealed class OtherPartiesShippingReportImportTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @BtPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BtPk02 UNIQUEIDENTIFIER = newid();
				INSERT dbo.CusSeaManTranHead (BT_PK, BT_SendersMessageReference, BT_VesselName, BT_SystemCreateTimeUtc, BT_SystemCreateUser) VALUES
					(@BtPk01, 'FR01', 'V1', '2014-01-01', 'US1'),
					(@BtPk02, 'FR02', 'V2', '2013-07-31', 'US2'),
					(newid(), 'FR03', 'V3', '2013-06-30', 'US3');
				INSERT dbo.CusSeaManOBLHeader (BO_PK, BO_BT, BO_OceanBill) VALUES
					(newid(), @BtPk01, 'OBL4567123'),
					(newid(), @BtPk02, 'OBL1234567');
				INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser) VALUES
					(newid(), 'CusSeaManOBLHeader', @BtPk01, 'ADD', '2014-01-02', '2014-01-03', 'US5'),
					(newid(), 'CusSeaManOBLHeader', @BtPk02, 'ADD', '2013-07-30', '2013-07-29', 'US4');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 7, 31), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "FR02", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "OBL1234567", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 7);
			}
		}
	}
}
