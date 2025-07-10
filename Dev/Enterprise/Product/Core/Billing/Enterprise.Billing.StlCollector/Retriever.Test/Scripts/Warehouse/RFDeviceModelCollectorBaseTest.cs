using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	abstract class RFDeviceModelCollectorBaseTest : RefStlScriptWithDefaultsTest
	{
		protected override sealed IDateTimeRange TestDateTimeRange
		{
			get
			{
				return testDateRange ?? (testDateRange = AusydMonthRange.New(YearForTest.Year, YearForTest.Month));
			}
		}
		IDateTimeRange testDateRange;

		DateTime RangeMonthAsDate
		{
			get
			{
				if (rangeMonthAsDate == null)
				{
					rangeMonthAsDate = new DateTime(YearForTest.Year, YearForTest.Month, 1);
				}
				return rangeMonthAsDate.Value;
			}
		}
		DateTime? rangeMonthAsDate;

		readonly DateTime YearForTest = new DateTime(2024, 01, 01);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 7, transactions.Count());

			var winCEModelDetails = "MAN:Symbol|MDL:MC9200|OS:WCE";
			var androidModelDetails = "MAN:Zebra|MDL:TC77|OS:AND";

			AssertDeviceResults("WinCE - RFM", winCEModelDetails);
			AssertDeviceResults("WinCE - EMT", winCEModelDetails);
			AssertDeviceResults("AndroidID - RFM", androidModelDetails);
			AssertDeviceResults("AndroidID - EMT", androidModelDetails);

			AssertDeviceResults("AndroidID - DuplicateEntries", androidModelDetails);
			AssertDeviceResults("AndroidID - OldEntry", androidModelDetails);

			var deviceIDLong = "1".PadLeft(49, 'B').PadLeft(50, 'A');
			AssertDeviceResults(deviceIDLong, winCEModelDetails);

			void AssertDeviceResults(string expectedFullDeviceID, string expectedDeviceDetails)
			{
				var log = StmActivityLog.ShallowLoadFromDB(TestConnection, l => l.S7_DeviceID == expectedFullDeviceID).OrderBy(l => l.S7_OpenDateTimeUtc).First();
				var expectedReference2 = expectedFullDeviceID.Length > 50 ? expectedFullDeviceID.Substring(0, 50) : expectedFullDeviceID;
				var expectedReference3 = expectedFullDeviceID.Length > 50 ? expectedFullDeviceID.Substring(expectedFullDeviceID.Length - 50) : expectedFullDeviceID;
				var transaction = transactions.FirstOrDefault(r => r.Reference2 == expectedReference2);
				AssertEquals("TransactionReference01", expectedDeviceDetails, transaction.Reference1);
				AssertEquals("TransactionReference02", expectedReference2, transaction.Reference2);
				AssertEquals("TransactionReference03", expectedReference3, transaction.Reference3);
				Assert("TransactionReference04", VerifyHash(expectedFullDeviceID, transaction.Reference4));
				AssertEquals("TransactionDateUtc", RangeMonthAsDate, transaction.ServiceOccuredUTC);
				AssertEquals("BranchCode", null, transaction.GetBranchCode());
				AssertEquals("ItemCount", 1, transaction.BillableCount);
			}
		}

		protected override void PrepareTestData()
		{
			var company = new GlbCompany("CM1", "AU").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("USB", company.PK).InsertAndReturnObject(TestConnection);

			var date = new DateTime(2024, 01, 01);

			var winCEModelDetails = "MAN:Symbol|MDL:MC9200|OS:WCE";
			var androidModelDetails = "MAN:Zebra|MDL:TC77|OS:AND";

			CreateActivityLog(deviceID: string.Empty, deviceDetails: null, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, deviceDetails: null, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, deviceDetails: null, date, formCaption: "RFM");

			CreateActivityLog(deviceID: "DeviceOnly", deviceDetails: null, date, formCaption: "RFM");
			CreateActivityLog(deviceID: "DeviceOnly", deviceDetails: null, date, formCaption: "RFM");
			CreateActivityLog(deviceID: "DeviceOnly", deviceDetails: null, date, formCaption: "RFM");

			CreateActivityLog(deviceID: string.Empty, deviceDetails: winCEModelDetails, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, deviceDetails: winCEModelDetails, date, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, deviceDetails: winCEModelDetails, date, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - RFM", deviceDetails: winCEModelDetails, date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - RFM", deviceDetails: androidModelDetails, date, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - EMT", deviceDetails: winCEModelDetails, date, formCaption: "EMT");
			CreateActivityLog(deviceID: "AndroidID - EMT", deviceDetails: androidModelDetails, date, formCaption: "EMT");

			CreateActivityLog(deviceID: "WinCE - Wrong Form Caption", deviceDetails: winCEModelDetails, date, formCaption: "OTH");
			CreateActivityLog(deviceID: "AndroidID - Wrong Form Caption", deviceDetails: androidModelDetails, date, formCaption: "OTH");

			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", deviceDetails: androidModelDetails, date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", deviceDetails: androidModelDetails, date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", deviceDetails: androidModelDetails, date, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - DuplicateEntries", deviceDetails: androidModelDetails, date, formCaption: "RFM");

			CreateActivityLog(deviceID: "AndroidID - OldEntry", deviceDetails: androidModelDetails, date.AddYears(-3), formCaption: "EMT");

			var deviceIDLong = "1".PadLeft(49, 'B').PadLeft(50, 'A');
			CreateActivityLog(deviceID: deviceIDLong, deviceDetails: winCEModelDetails, date, formCaption: "RFM");

			void CreateActivityLog(string deviceID, string deviceDetails, DateTime openDateTime, string formCaption)
				=> new StmActivityLog(branch.PK, "GB", formCaption) { S7_DeviceID = deviceID, S7_DeviceDetails = deviceDetails, S7_OpenDateTimeUtc = openDateTime }.InsertAndReturnObject(TestConnection);
		}

		protected override bool IsMandatoryForMilestones => false;

		// Taken from https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-6.0
		static string GetHash(string input)
		{
			var hashAlgorithm = SHA256.Create();
			var data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

			var sBuilder = new StringBuilder();

			for (var i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			return sBuilder.ToString().Substring(0, 50);
		}

		// Taken from https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-6.0
		static bool VerifyHash(string input, string hash)
		{
			var hashOfInput = GetHash(input);
			var comparer = StringComparer.OrdinalIgnoreCase;
			return comparer.Compare(hashOfInput, hash) == 0;
		}
	}
}
