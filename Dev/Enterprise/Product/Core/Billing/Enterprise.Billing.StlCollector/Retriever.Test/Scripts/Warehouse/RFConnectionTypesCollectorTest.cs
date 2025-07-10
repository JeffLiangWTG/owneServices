using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Billing.Collectors.Warehouse;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Web.Shared;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(RFConnectionTypesCollector))]
	sealed class RFConnectionTypesCollectorTest : RefStlScriptWithDefaultsTest
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
			AssertEquals("Number of Transactions", 14, transactions.Count());

			AssertDeviceResults("WinCE - RFM", "100.00", expectedIsPatchApplied: true);
			AssertDeviceResults("WinCE - EMT", "100.00", expectedIsPatchApplied: true);
			AssertDeviceResults("WinCE - Weird Keystrokes", "0.00", expectedIsPatchApplied: false);
			AssertDeviceResults("AndroidID - Weird Keystrokes", "0.00", expectedIsPatchApplied: false);
			AssertDeviceResults("WinCE - AllTypes", "50.00", expectedIsPatchApplied: false);
			AssertDeviceResults("AndroidID - RFM", "100.00", expectedIsPatchApplied: true);
			AssertDeviceResults("AndroidID - EMT", "100.00", expectedIsPatchApplied: true);
			AssertDeviceResults("AndroidID - AllTypes", "50.00", expectedIsPatchApplied: false);
			AssertDeviceResults("WinCE - HttpAndHttps", "50.00", expectedIsPatchApplied: true);
			AssertDeviceResults("WinCE - HttpAndUndefined", "0.00", expectedIsPatchApplied: false);
			AssertDeviceResults("AndroidID - HttpsAndUndefined", "100.00", expectedIsPatchApplied: false);

			var deviceIDLong = "1".PadLeft(49, 'B').PadLeft(50, 'A');
			AssertDeviceResults(deviceIDLong, "100.00", expectedIsPatchApplied: true);

			AssertDeviceResults("AndroidID - OneThirdOfEntries_WithPatchApplied", "33.33", expectedIsPatchApplied: true);
			AssertDeviceResults("WinCE - OneEighthOfEntries_WithoutPatchApplied", "12.50", expectedIsPatchApplied: false);

			void AssertDeviceResults(string expectedFullDeviceID, string expectedPercentage, bool expectedIsPatchApplied)
			{
				var log = StmActivityLog.ShallowLoadFromDB(TestConnection, l => l.S7_DeviceID == expectedFullDeviceID).OrderBy(l => l.S7_OpenDateTimeUtc).First();
				var expectedReference3 = expectedFullDeviceID.Length > 50 ? expectedFullDeviceID.Substring(0, 50) : expectedFullDeviceID;

				var transaction = transactions.FirstOrDefault(r => r.Reference3 == expectedReference3);
				AssertEquals("TransactionReference01", expectedPercentage, transaction.Reference1);
				var expectedTransactionReference2 = expectedIsPatchApplied ? "1" : "0";
				AssertEquals("TransactionReference02", expectedTransactionReference2, transaction.Reference2);
				Assert("TransactionReference04", VerifyHash(expectedFullDeviceID, transaction.Reference4));
				AssertEquals("TransactionDateUtc", log.PK, Guid.Parse(transaction.Reference5));
				AssertEquals("TransactionDateUtc", RangeMonthAsDate, transaction.ServiceOccuredUTC);
				AssertEquals("BranchCode", null, transaction.GetBranchCode());
				AssertEquals("ItemCount", 1, transaction.BillableCount);
			}
		}

		protected override void PrepareTestData()
		{
			var company = new GlbCompany("CM1", "AU").InsertAndReturnObject(TestConnection);
			var branch = new GlbBranch("USB", company.PK).InsertAndReturnObject(TestConnection);

			var dateWithinRange = new DateTime(2024, 01, 01);
			var dateBeforeRange = dateWithinRange.AddMonths(-1);
			var dateAfterRange = dateWithinRange.AddMonths(1);

			CreateActivityLog(deviceID: string.Empty, dateWithinRange, (int)ConnectionType.Undefined, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateWithinRange, (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateBeforeRange, (int)ConnectionType.Undefined, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateBeforeRange, (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateBeforeRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateAfterRange, (int)ConnectionType.Undefined, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateAfterRange, (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: string.Empty, dateAfterRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - RFM", dateWithinRange.AddDays(1), (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - RFM", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - EMT", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "EMT");
			CreateActivityLog(deviceID: "AndroidID - EMT", dateWithinRange.AddDays(1), (int)ConnectionType.HttpsConnection, formCaption: "EMT");

			CreateActivityLog(deviceID: "WinCE - Wrong Form Caption", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "OTH");
			CreateActivityLog(deviceID: "AndroidID - Wrong Form Caption", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "OTH");

			CreateActivityLog(deviceID: "WinCE - Weird Keystrokes", dateWithinRange, -1, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - Weird Keystrokes", dateWithinRange, 50, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - Wrong Date", dateBeforeRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - Wrong Date", dateAfterRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - Wrong Date", dateBeforeRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - Wrong Date", dateAfterRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - AllTypes", dateWithinRange.AddDays(1), (int)ConnectionType.Undefined, formCaption: "EMT");
			CreateActivityLog(deviceID: "WinCE - AllTypes", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "EMT");
			CreateActivityLog(deviceID: "WinCE - AllTypes", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "EMT");

			CreateActivityLog(deviceID: "AndroidID - AllTypes", dateWithinRange, (int)ConnectionType.Undefined, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - AllTypes", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - AllTypes", dateWithinRange.AddDays(1), (int)ConnectionType.HttpsConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - HttpAndHttps", dateWithinRange, (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - HttpAndHttps", dateWithinRange.AddDays(1), (int)ConnectionType.HttpsConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "AndroidID - HttpsAndUndefined", dateWithinRange.AddDays(1), (int)ConnectionType.Undefined, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - HttpsAndUndefined", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - HttpAndUndefined", dateWithinRange, (int)ConnectionType.Undefined, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - HttpAndUndefined", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");

			var deviceIDLong = "1".PadLeft(49, 'B').PadLeft(50, 'A');
			CreateActivityLog(deviceID: deviceIDLong, dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "AndroidID - OneThirdOfEntries_WithPatchApplied", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - OneThirdOfEntries_WithPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "AndroidID - OneThirdOfEntries_WithPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");

			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange, (int)ConnectionType.HttpsConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.HttpConnection, formCaption: "RFM");
			CreateActivityLog(deviceID: "WinCE - OneEighthOfEntries_WithoutPatchApplied", dateWithinRange.AddDays(1), (int)ConnectionType.Undefined, formCaption: "RFM");

			void CreateActivityLog(string deviceID, DateTime openDateTime, int connectionType, string formCaption)
				=> new StmActivityLog(branch.PK, "GB", formCaption) { S7_DeviceID = deviceID, S7_OpenDateTimeUtc = openDateTime, S7_KeyStrokes = connectionType }.InsertAndReturnObject(TestConnection);
		}

		protected override bool IsMandatoryForMilestones => false;

		#region GetHash

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

		#endregion

		#region VerifyHash

		// Taken from https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-6.0
		static bool VerifyHash(string input, string hash)
		{
			var hashOfInput = GetHash(input);
			var comparer = StringComparer.OrdinalIgnoreCase;
			return comparer.Compare(hashOfInput, hash) == 0;
		}

		#endregion
	}
}
