using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Types;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(GatewayShipment))]
	sealed class GatewayShipmentTest : RefStlScriptWithDefaultsTest
	{
		readonly ZGuid consol1Pk = ZGuid.NewZGuid();
		readonly ZGuid consol2Pk = ZGuid.NewZGuid();
		readonly DateTime startDate = new DateTime(2021, 11, 11);

		protected override IDateTimeRange TestDateTimeRange => new RecurringRange(startDate, startDate.AddDays(1));

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = transactions.Single(t => t.GetCompanyCode() == "AAA" && t.Reference1 == "JK01");
			AssertEquals("[T1] TransactionDateUtc", startDate, transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionGuidReference", consol1Pk.ToString().ToUpper(), transaction1.Reference5);
			AssertEquals("[T1] TransactionReference02", "00001001", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
			AssertEquals("[T1] AdditionalRefs", null, transaction1.AdditionalRefs);
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 2, transaction1.BillableCount);

			var transaction2 = transactions.Single(t => t.GetCompanyCode() == "AAA" && t.Reference1 == "JK02");
			AssertEquals("[T2] TransactionDateUtc", startDate, transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionGuidReference", consol2Pk.ToString().ToUpper(), transaction2.Reference5);
			AssertEquals("[T2] TransactionReference02", "00001004", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
			AssertEquals("[T2] AdditionalRefs", null, transaction2.AdditionalRefs);
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 5, transaction2.BillableCount);

			var transaction3 = transactions.Single(t => t.GetCompanyCode() == "NNN" && t.Reference1 == "JK01");
			AssertEquals("[T3] TransactionDateUtc", startDate, transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] TransactionGuidReference", consol1Pk.ToString().ToUpper(), transaction3.Reference5);
			AssertEquals("[T3] TransactionReference02", "00001001", transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", null, transaction3.Reference3);
			AssertEquals("[T3] TransactionReference04", null, transaction3.Reference4);
			AssertEquals("[T3] AdditionalRefs", null, transaction3.AdditionalRefs);
			AssertEquals("[T3] BranchCode", null, transaction3.GetBranchCode());
			AssertEquals("[T3] UserCode", "US1", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 8, transaction3.BillableCount);

			var transaction4 = transactions.Single(t => t.GetCompanyCode() == "NNN" && t.Reference1 == "JK02");
			AssertEquals("[T4] TransactionDateUtc", startDate, transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] TransactionGuidReference", consol2Pk.ToString().ToUpper(), transaction4.Reference5);
			AssertEquals("[T4] TransactionReference02", "00001004", transaction4.Reference2);
			AssertEquals("[T4] TransactionReference03", null, transaction4.Reference3);
			AssertEquals("[T4] TransactionReference04", null, transaction4.Reference4);
			AssertEquals("[T4] AdditionalRefs", null, transaction4.AdditionalRefs);
			AssertEquals("[T4] BranchCode", null, transaction4.GetBranchCode());
			AssertEquals("[T4] UserCode", "US2", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 11, transaction4.BillableCount);
		}

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @Company1Pk UNIQUEIDENTIFIER = newid();
				DECLARE @Company2Pk UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES
					(@Company1Pk, 'AU', 'AAA', 'AU company'),
					(@Company2Pk, 'NZ', 'NNN', 'NZ company');
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsForwarding, JK_AgentType, JK_SendingForwarderHandlingType) VALUES
					('{0}', 'JK01', 1, 'AGT', 'GTA'),
					('{1}', 'JK02', 1, 'AGT', 'GTA');
				INSERT dbo.AccBillingHeader (ABH_PK, ABH_InternalReferenceNumber, ABH_BillingCode, ABH_ParentId, ABH_ParentTableCode, ABH_ParentReferenceNumber, ABH_EventType, ABH_EventTimeUtc, ABH_GS_NKEventUser, ABH_BillingCounter, ABH_GC_Company, ABH_SystemCreateTimeUtc, ABH_SystemCreateUser,ABH_SystemLastEditTimeUtc, ABH_SystemLastEditUser) VALUES
					(newid(), '00001000', 'GSH', '{0}', 'JK', 'JK01', 'CST', '2021-11-10', 'US1', 1, @Company1Pk, '2021-11-10', 'US1', '2021-11-10', 'US1'),
					(newid(), '00001001', 'GSH', '{0}', 'JK', 'JK01', 'CST', '2021-11-11', 'US1', 2, @Company1Pk, '2021-11-11', 'US1', '2021-11-11', 'US1'),
					(newid(), '00001002', 'GSH', '{0}', 'JK', 'JK01', 'CST', '2021-11-12', 'US1', 3, @Company1Pk, '2021-11-12', 'US1', '2021-11-11', 'US1'),
					(newid(), '00001003', 'GSH', '{1}', 'JK', 'JK02', 'PST', '2021-11-10', 'US2', 4, @Company1Pk, '2021-11-10', 'US2', '2021-11-10', 'US2'),
					(newid(), '00001004', 'GSH', '{1}', 'JK', 'JK02', 'PST', '2021-11-11', 'US2', 5, @Company1Pk, '2021-11-11', 'US2', '2021-11-11', 'US2'),
					(newid(), '00001005', 'GSH', '{1}', 'JK', 'JK02', 'PST', '2021-11-12', 'US2', 6, @Company1Pk, '2021-11-12', 'US2', '2021-11-11', 'US2'),
					(newid(), '00001000', 'GSH', '{0}', 'JK', 'JK01', 'REV', '2021-11-10', 'US1', 7, @Company2Pk, '2021-11-10', 'US1', '2021-11-10', 'US1'),
					(newid(), '00001001', 'GSH', '{0}', 'JK', 'JK01', 'REV', '2021-11-11', 'US1', 8, @Company2Pk, '2021-11-11', 'US1', '2021-11-11', 'US1'),
					(newid(), '00001002', 'GSH', '{0}', 'JK', 'JK01', 'REV', '2021-11-12', 'US1', 9, @Company2Pk, '2021-11-12', 'US1', '2021-11-11', 'US1'),
					(newid(), '00001003', 'GSH', '{1}', 'JK', 'JK02', 'APP', '2021-11-10', 'US2', 10, @Company2Pk, '2021-11-10', 'US2', '2021-11-10', 'US2'),
					(newid(), '00001004', 'GSH', '{1}', 'JK', 'JK02', 'APP', '2021-11-11', 'US2', 11, @Company2Pk, '2021-11-11', 'US2', '2021-11-11', 'US2'),
					(newid(), '00001005', 'GSH', '{1}', 'JK', 'JK02', 'APP', '2021-11-12', 'US2', 12, @Company2Pk, '2021-11-12', 'US2', '2021-11-11', 'US2');";
			TestConnection.ExecuteNonQuery(string.Format(sqlText, consol1Pk, consol2Pk));
		}

		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("21.11.11.162", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 21.11.12.194", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("21.11.15.19", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions above 21.11.12.194", ScriptToTest.IsActive);
			}
		}
	}
}
