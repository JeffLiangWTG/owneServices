using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Billing.Integration;
using Enterprise.Billing.StlCollector.Retriever.Testing.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.eServices
{
	[TestedType(typeof(UsageSummaryCollectorScript))]
	class UsageSummaryCollectorScriptTest : UsageScriptTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				var startDate = new DateTime(2021, 7, 1);
				return new RecurringRange(startDate, startDate.AddDays(3));
			}
		}

		protected override void AssertTransactions(IEnumerable<IStlTransaction> transactions)
		{
			var usageTransactions = transactions.Select(t => (UsageTransaction)t);
			AssertEquals("Number of Transactions", 3, usageTransactions.Count());

			var transaction1 = usageTransactions.Single(t => t.ServiceOccuredUTC == new DateTime(2021, 6, 30, 14, 0, 0));
			AssertEquals("[T1] ItemCount", 2, transaction1.BillableCount);
			AssertEquals("[T1] AdditionalRefs", "{UsageId: 'Usage1'}", transaction1.AdditionalRefs);
			AssertEquals("[T1] UsageCode", "USS", transaction1.UsageCode);
			AssertEnvironmentProperties(transaction1);

			var transaction2 = usageTransactions.Single(t => t.ServiceOccuredUTC == new DateTime(2021, 7, 1, 14, 0, 0));
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] AdditionalRefs", "{UsageId: 'Usage2'}", transaction2.AdditionalRefs);
			AssertEquals("[T2] UsageCode", "USS", transaction2.UsageCode);
			AssertEnvironmentProperties(transaction2);

			var transaction3 = usageTransactions.Single(t => t.ServiceOccuredUTC == new DateTime(2021, 7, 2, 14, 0, 0));
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] AdditionalRefs", "{UsageId: 'Usage2'}", transaction3.AdditionalRefs);
			AssertEquals("[T3] UsageCode", "USS", transaction3.UsageCode);
			AssertEnvironmentProperties(transaction3);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
--Insert a company whose country code is not GB
INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (0x1, 'AU', 'WTG', 'Wisetech')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x1, 0x1, 'CBR')
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (0x1, 'DEP')

INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_ApplicationCode, EM_Status, EM_SystemCreateUser, EM_IsActive, EM_MessageData, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
	(0x1, 0x1, 0x1, '2021-07-01 00:00:00', 'TRX', 'USS', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage1''}'), '2021-07-01 00:00:00', 'DAT'), -- All valid data, minimum allowed EM_SystemCreateTimeUtc
	(0x2, 0x1, 0x1, '2021-07-01 12:00:00', 'TRX', 'USS', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage1''}'), '2021-07-01 00:00:00', 'DAT'), -- Same data different timestamp in that day, should be combined
	(0x3, 0x1, 0x1, '2021-07-01 23:58:30', 'TRX', 'USS', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage2''}'), '2021-07-01 23:58:30', 'DAT'), -- Sll valid data, maximum allowed EM_SystemCreateTimeUtc
	(0x4, 0x1, 0x1, '2021-07-02 23:58:30', 'TRX', 'USS', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage2''}'), '2021-07-01 23:58:30', 'DAT'), -- Same data different timestamp in the next day, should not be combined
	(0x5, 0x1, 0x1, '2021-07-01 23:58:30', 'TRX', 'USS', 'USS', 'CAP', 'DAT', 0, CONVERT(VARBINARY(max),'{UsageId: ''Usage3''}'), '2021-07-01 23:58:30', 'DAT'), -- inactive
	(0x6, 0x1, 0x1, '2021-07-01 23:58:30', 'TRX', 'USS', 'UNK', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage4''}'), '2021-07-01 23:58:30', 'DAT'), -- different application code
	(0x7, 0x1, 0x1, '2021-07-01 23:58:30', 'TRX', 'UNK', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage5''}'), '2021-07-01 23:58:30', 'DAT'), -- different message type
	(0x8, 0x1, 0x1, '2021-07-01 23:58:30', 'TRX', 'USS', 'USS', 'SNT', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage6''}'), '2021-07-01 23:58:30', 'DAT'), -- different status
	(0x9, 0x1, 0x1, '2021-07-01 23:58:30', 'RCV', 'USS', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage7''}'), '2021-07-01 23:58:30', 'DAT'), -- a receive
	(0xA, 0x1, 0x1, '2021-06-29 23:58:30', 'TRX', 'USS', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage8''}'), '2021-06-30 23:58:30', 'DAT'), -- before range
	(0xB, 0x1, 0x1, '2021-07-04 00:00:00', 'TRX', 'USS', 'USS', 'CAP', 'DAT', 1, CONVERT(VARBINARY(max),'{UsageId: ''Usage9''}'), '2021-07-01 00:00:00', 'DAT') -- after range
";
			TestConnection.Command(sqlText).ExecuteNonQuery();
		}

		protected override Type GetExpectedTransactionType(bool enableSendingNonbilledItemsAsUsage) => typeof(UsageTransaction);
	}
}
