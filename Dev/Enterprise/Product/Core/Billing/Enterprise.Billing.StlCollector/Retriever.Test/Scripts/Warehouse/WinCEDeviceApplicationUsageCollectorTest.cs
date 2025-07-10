using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Warehouse;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(WinCEDeviceApplicationUsageCollector))]
	sealed class WinCEDeviceApplicationUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override sealed IDateTimeRange TestDateTimeRange
		{
			get
			{
				return testDateRange ?? (testDateRange = AusydMonthRange.New(YearForTest.Year, YearForTest.Month));
			}
		}
		IDateTimeRange testDateRange;

		readonly DateTime YearForTest = new DateTime(2024, 01, 01);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var portTransportCode = "EMT";
			var warehouseCode = "RFM";

			AssertDeviceResults("WinCE - RFM", warehouseCode);
			AssertDeviceResults("WinCE - EMT", portTransportCode);
			AssertDeviceResults("WinCE - RFMEMT", $"{portTransportCode}, {warehouseCode}");

			void AssertDeviceResults(string expectedFullDeviceID, string expectedApplicationCodes)
			{
				var log = StmActivityLog.ShallowLoadFromDB(TestConnection, l => l.S7_DeviceID == expectedFullDeviceID).OrderBy(l => l.S7_OpenDateTimeUtc).First();
				var expectedReference1 = expectedFullDeviceID.Length > 50 ? expectedFullDeviceID.Substring(0, 50) : expectedFullDeviceID;
				var transaction = transactions.FirstOrDefault(r => r.Reference1 == expectedReference1);
				AssertEquals("TransactionReference01", expectedFullDeviceID, transaction.Reference1);
				AssertEquals("TransactionReference03", expectedApplicationCodes, transaction.Reference3);
				AssertEquals("ItemCount", 1, transaction.BillableCount);
			}
		}

		protected override void PrepareTestData()
		{
			var company = new GlbCompany("CM1", "AU").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("USB", company.PK).InsertAndReturnObject(TestConnection);

			var date = new DateTime(2024, 01, 01);

			CreateActivityLog(deviceID: string.Empty, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, date, formCaption: "RFM");

			CreateActivityLog(deviceID: string.Empty, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, date, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - RFM", date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - RFM", date, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - EMT", date, formCaption: "EMT");
			CreateActivityLog(deviceID: "AndroidID - EMT", date, formCaption: "EMT");

			CreateActivityLog(deviceID: "WinCE - RFMEMT", date, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - RFMEMT", date, formCaption: "EMT");

			CreateActivityLog(deviceID: "WinCE - Wrong Form Caption", date, formCaption: "OTH");
			CreateActivityLog(deviceID: "AndroidID - Wrong Form Caption", date, formCaption: "OTH");

			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", date, formCaption: "RFM");

			CreateActivityLog(deviceID: "AndroidID - OldEntry", date.AddYears(-3), formCaption: "EMT");

			void CreateActivityLog(string deviceID, DateTime openDateTime, string formCaption)
				=> new StmActivityLog(branch.PK, "GB", formCaption) { S7_DeviceID = deviceID, S7_OpenDateTimeUtc = openDateTime }.InsertAndReturnObject(TestConnection);
		}

		protected override bool IsMandatoryForMilestones => false;

		public void TestMinimumVersionRequired()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("22.3.17.1", DateTime.Now, "ALP")))
			{
				Assert("Should not be active in versions below 22.3.17.112", !ScriptToTest.IsActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("22.3.17.120", DateTime.Now, "ALP")))
			{
				Assert("Should be active in versions above 22.3.17.112", ScriptToTest.IsActive);
			}
		}
	}
}
