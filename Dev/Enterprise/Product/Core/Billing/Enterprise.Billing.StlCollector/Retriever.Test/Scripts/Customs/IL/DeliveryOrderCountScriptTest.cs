using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.Customs.IL
{
	[TestedType(typeof(DeliveryOrderCountScript))]
	sealed class DeliveryOrderCountScriptTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 9);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());
			var entries1 = FindRowsByRef1(transactions, "SHP:S00001001");
			var entries2 = FindRowsByRef1(transactions, "SHP:S00001002");
			AssertEquals("Number of Transactions for first shipment", 2, entries1.Count());
			AssertEquals("Number of Transactions for second shipment", 3, entries2.Count());

			AssertArrayEqualsByElements(
				"Billing entries for the first shipment match the expected",
				[
					$"SHP:S00001001|ORD:112233|2022-09-26",
					$"SHP:S00001001|ORD:112233|2022-09-27"
				],
				entries1.Select(x => $"{x.Reference1}|{x.Reference2}|{x.ServiceOccuredUTC.ToString("yyyy-MM-dd")}").ToArray());

			AssertArrayEqualsByElements(
				"Billing entries for the second shipment match the expected",
				[
					$"SHP:S00001002||2022-09-24",
					$"SHP:S00001002||2022-09-26",
					$"SHP:S00001002||2022-09-27"
				],
				entries2.Select(x => $"{x.Reference1}|{x.Reference2}|{x.ServiceOccuredUTC.ToString("yyyy-MM-dd")}").ToArray());
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"

				DECLARE @GePk UNIQUEIDENTIFIER = NEWID();

				DECLARE @GcPkFr UNIQUEIDENTIFIER = NEWID();
				DECLARE @GcPkIl UNIQUEIDENTIFIER = NEWID();

				DECLARE @GbPkFr UNIQUEIDENTIFIER = NEWID();
				DECLARE @GbPkIl UNIQUEIDENTIFIER = NEWID();

				DECLARE @JsPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPK02 UNIQUEIDENTIFIER = newid();

				INSERT GlbDepartment (GE_PK) VALUES ( @GePk );

				INSERT GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES
					(@GcPkFr, '~FR', 'FR company', 'FR'),
					(@GcPkIl, '~IL', 'IL company', 'IL');

				INSERT GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPkFr, '^FR', @GcPkFr),
					(@GbPkIl, '^IL', @GcPkIl);

				INSERT JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsShipping, JS_BookingReference, JS_RL_NKLoadPort, JS_RL_NKDischargePort) VALUES
					(@JsPk01, 'S00001001', 0, 1, 'BRC001', 'BGSOF', 'ILHFA'),
					(@JsPk02, 'S00001002', 0, 1, 'BRC002', 'PGPOM', 'ILHFA');

				INSERT CusEntryNum(CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_RN_NKCountryCode, CE_EntryType, CE_Category, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(NEWID(), @JsPk01, 'JobShipment', '112233', 'IL', 'DLO', 'CUS', '2022-08-01', 'US1', '2022-09-01', 'US1');

				INSERT EDIMessage (EM_PK, EM_GB, EM_GE, EM_ApplicationCode, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPkFr, @GePk, 'FRC', @JsPk01, 'JobShipment', '2022-09-24', 'TRX', 'OCR', 'ORG', 'PRS', 'US1', '2022-09-24', 'US1'),
					(newid(), @GbPkIl, @GePk, 'ILC', @JsPk02, 'JobShipment', '2022-09-24', 'TRX', 'DLO', '120', 'PRS', 'US1', '2022-09-24', 'US1'),
					(newid(), @GbPkIl, @GePk, 'ILC', @JsPk02, 'JobShipment', '2022-09-26', 'TRX', 'DLO', '120', 'PRS', 'US1', '2022-09-26', 'US1'),
					(newid(), @GbPkIl, @GePk, 'ILC', @JsPk02, 'JobShipment', '2022-09-27', 'TRX', 'DLO', '120', 'PRS', 'US1', '2022-09-27', 'US1'),
					(newid(), @GbPkIl, @GePk, 'ILC', @JsPk01, 'JobShipment', '2022-09-26', 'TRX', 'DLO', '120', 'PRS', 'US1', '2022-09-26', 'US1'),
					(newid(), @GbPkIl, @GePk, 'ILC', @JsPk01, 'JobShipment', '2022-09-27', 'TRX', 'DLO', '120', 'PRS', 'US1', '2022-09-27', 'US1');

			";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
