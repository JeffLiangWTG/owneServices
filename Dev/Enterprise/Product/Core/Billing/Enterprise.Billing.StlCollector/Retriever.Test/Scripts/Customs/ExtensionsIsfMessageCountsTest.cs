using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsIsfMessageCounts))]
	sealed class ExtensionsIsfMessageCountsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @BfPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BfPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGb UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkGb UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkGb);
				DECLARE @GdPK01 UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);

				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'GB', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkGb;

				INSERT dbo.CusISFHeader (BF_PK, BF_GB, BF_JobReference,BF_CustomsReference, BF_SystemCreateTimeUtc, BF_SystemCreateUser) VALUES
					(@BfPk01, @GbPkGb, 'ISF0000001','CR00001', '2021-7-12', 'US1');

				INSERT dbo.CusISFHeader (BF_PK, BF_GB, BF_JobReference,BF_CustomsReference, BF_SystemCreateTimeUtc, BF_SystemCreateUser) VALUES
					(@BfPk02, @GbPkGb, 'ISF0000002','CR00002', '2021-7-13', 'US1');

				INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_ApplicationCode,EM_MessageSubType, EM_ReceiveTransmit, EM_Status, EM_MessageType, EM_MessageText, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPkGb, @GdPK01, @BfPk01, 'CusUnderbond', 'USI', 'ADD', 'RCV', 'QUE', 'SN', 'SF9001   ISF ACCEPTED', '2021-7-12 01:10:50', 'US1', '2021-7-12 01:10:50', 'US1'),
					(newid(), @GbPkGb, @GdPK01, @BfPk02, 'CusUnderbond', 'USI', 'ADD', 'RCV', 'QUE', 'SN', 'SF9002   ISF ACCEPTED', '2021-7-13 01:10:50', 'US1', '2021-7-13 01:10:50', 'US1');

				INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference) VALUES
					(newid(), 'CusISFHeader', '2021-7-12', '2021-7-12', @BfPk01, 'TRF', '|TYP=HVL|JOB=S00000001');
";

			TestConnection.Command(sqlText).ExecuteNonQuery();
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions("collector has worked", () =>
			{
				AssertEquals("Number of Transactions", 2, transactions.Count());

				var transaction1 = FindRowByRef1(transactions, "ISF0000001");
				AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
				AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 12, 01, 10, 50), transaction1.ServiceOccuredUTC);
				AssertEquals("ItemCount", 1, transaction1.BillableCount);
				AssertEquals("TransactionReference02", "CR00001", transaction1.Reference2);
				AssertEquals("TransactionReference03", "001", transaction1.Reference3);
				AssertEquals("TransactionReference04", "HVL", transaction1.Reference4);

				var transaction2 = FindRowByRef1(transactions, "ISF0000002");
				AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
				AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
				AssertEquals("TransactionDateUtc", new DateTime(2021, 7, 13, 01, 10, 50), transaction2.ServiceOccuredUTC);
				AssertEquals("ItemCount", 1, transaction2.BillableCount);
				AssertEquals("TransactionReference02", "CR00002", transaction2.Reference2);
				AssertEquals("TransactionReference03", "002", transaction2.Reference3);
				AssertEquals("TransactionReference04", null, transaction2.Reference4);
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2021, 7);
			}
		}
	}
}
