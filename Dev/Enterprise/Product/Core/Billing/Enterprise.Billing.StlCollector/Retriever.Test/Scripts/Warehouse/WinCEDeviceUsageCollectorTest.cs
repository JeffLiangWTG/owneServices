using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Billing.Collectors.Warehouse;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(WinCEDeviceUsageCollector))]
	sealed class WinCEDeviceUsageCollectorTest : RefStlScriptWithDefaultsTest
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

		readonly DateTime YearForTest = new DateTime(2022, 01, 01);

		List<StmActivityLog> ExpectedResults { get; set; }

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			var expectedResult1 = ExpectedResults.FirstOrDefault(r => r.S7_DeviceID == "Device1");
			var t1 = transactions.FirstOrDefault(r => Guid.Parse(r.Reference5) == expectedResult1.PK);
			AssertEquals("[T1] TransactionReference01", "Device1", t1.Reference1);
			AssertEquals("[T1] TransactionReference02", "Device1", t1.Reference2);
			AssertEquals("[T1] TransactionReference03", "PRB,USB", t1.Reference3);
			Assert("[T1] TransactionReference04", VerifyHash("Device1", t1.Reference4));
			AssertEquals("[T1] TransactionDateUtc", RangeMonthAsDate, t1.ServiceOccuredUTC);
			AssertEquals("[T1] AdditionalRefs", null, t1.AdditionalRefs);
			AssertEquals("[T1] BranchCode", null, t1.GetBranchCode());
			AssertEquals("[T1] ItemCount", 1, t1.BillableCount);

			var expectedResult2 = ExpectedResults.FirstOrDefault(r => r.S7_DeviceID == "Device2");
			var t2 = transactions.FirstOrDefault(r => Guid.Parse(r.Reference5) == expectedResult2.PK);
			AssertEquals("[T2] TransactionReference01", "Device2", t2.Reference1);
			AssertEquals("[T2] TransactionReference02", "Device2", t2.Reference2);
			AssertEquals("[T2] TransactionReference03", "USB", t2.Reference3);
			Assert("[T2] TransactionReference04", VerifyHash("Device2", t2.Reference4));
			AssertEquals("[T2] TransactionDateUtc", RangeMonthAsDate, t2.ServiceOccuredUTC);
			AssertEquals("[T2] AdditionalRefs", null, t2.AdditionalRefs);
			AssertEquals("[T2] BranchCode", null, t2.GetBranchCode());
			AssertEquals("[T2] ItemCount", 1, t2.BillableCount);

			var expectedDevice3 = "1".PadLeft(49, 'B').PadLeft(50, 'A');
			var expectedResult3 = ExpectedResults.FirstOrDefault(r => r.S7_DeviceID == expectedDevice3);
			var t3 = transactions.FirstOrDefault(r => Guid.Parse(r.Reference5) == expectedResult3.PK);
			AssertEquals("[T3] TransactionReference01", expectedDevice3.Substring(0, 50), t3.Reference1);
			AssertEquals("[T3] TransactionReference02", expectedDevice3.Substring(expectedDevice3.Length - 50), t3.Reference2);
			AssertEquals("[T3] TransactionReference03", "PRB", t3.Reference3);
			Assert("[T3] TransactionReference04", VerifyHash(expectedDevice3, t3.Reference4));
			AssertEquals("[T3] TransactionDateUtc", RangeMonthAsDate, t3.ServiceOccuredUTC);
			AssertEquals("[T3] AdditionalRefs", null, t3.AdditionalRefs);
			AssertEquals("[T3] BranchCode", null, t3.GetBranchCode());
			AssertEquals("[T3] ItemCount", 1, t3.BillableCount);

			var expectedResult4 = ExpectedResults.FirstOrDefault(r => r.S7_DeviceID == "Device4");
			var t4 = transactions.FirstOrDefault(r => Guid.Parse(r.Reference5) == expectedResult4.PK);
			AssertEquals("[T4] TransactionReference01", "Device4", t4.Reference1);
			AssertEquals("[T4] TransactionReference02", "Device4", t4.Reference2);
			AssertEquals("[T4] TransactionReference03", "PRB,USB", t4.Reference3);
			Assert("[T4] TransactionReference04", VerifyHash("Device4", t4.Reference4));
			AssertEquals("[T4] TransactionDateUtc", RangeMonthAsDate, t4.ServiceOccuredUTC);
			AssertEquals("[T4] AdditionalRefs", null, t4.AdditionalRefs);
			AssertEquals("[T4] BranchCode", null, t4.GetBranchCode());
			AssertEquals("[T4] ItemCount", 1, t4.BillableCount);

			var expectedResult5 = ExpectedResults.FirstOrDefault(r => r.S7_DeviceID == "Device5");
			var t5 = transactions.FirstOrDefault(r => Guid.Parse(r.Reference5) == expectedResult5.PK);
			AssertEquals("[T5] TransactionReference01", "Device5", t5.Reference1);
			AssertEquals("[T5] TransactionReference02", "Device5", t5.Reference2);
			AssertEquals("[T5] TransactionReference03", "USB", t5.Reference3);
			Assert("[T5] TransactionReference04", VerifyHash("Device5", t5.Reference4));
			AssertEquals("[T5] TransactionDateUtc", RangeMonthAsDate, t5.ServiceOccuredUTC);
			AssertEquals("[T5] AdditionalRefs", null, t5.AdditionalRefs);
			AssertEquals("[T5] BranchCode", null, t5.GetBranchCode());
			AssertEquals("[T5] ItemCount", 1, t5.BillableCount);

			var expectedDevice6 = "2".PadLeft(49, 'B').PadLeft(50, 'A');
			var expectedResult6 = ExpectedResults.FirstOrDefault(r => r.S7_DeviceID == expectedDevice6);
			var t6 = transactions.FirstOrDefault(r => Guid.Parse(r.Reference5) == expectedResult6.PK);
			AssertEquals("[T6] TransactionReference01", expectedDevice6.Substring(0, 50), t6.Reference1);
			AssertEquals("[T6] TransactionReference02", expectedDevice6.Substring(expectedDevice6.Length - 50), t6.Reference2);
			AssertEquals("[T6] TransactionReference03", "PRB", t6.Reference3);
			Assert("[T6] TransactionReference04", VerifyHash(expectedDevice6, t6.Reference4));
			AssertEquals("[T6] TransactionDateUtc", RangeMonthAsDate, t6.ServiceOccuredUTC);
			AssertEquals("[T6] AdditionalRefs", null, t6.AdditionalRefs);
			AssertEquals("[T6] BranchCode", null, t6.GetBranchCode());
			AssertEquals("[T6] ItemCount", 1, t6.BillableCount);
		}

		protected override void PrepareTestData()
		{
			var company1 = new GlbCompany("CM1", "AU").InsertAndReturnObject(TestConnection);
			var company2 = new GlbCompany("CM2", "US").InsertAndReturnObject(TestConnection);

			var branch1 = new GlbBranch("USB", company1.PK).InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("PRB", company2.PK).InsertAndReturnObject(TestConnection);

			ExpectedResults = new List<StmActivityLog>();

			var deviceIDLong1 = "1".PadLeft(49, 'B').PadLeft(50, 'A');
			var deviceIDLong2 = "2".PadLeft(49, 'B').PadLeft(50, 'A');

			new StmActivityLog(branch1.PK, "GB", "RFM") { S7_DeviceID = "", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch1.PK, "GB", "RFM") { S7_DeviceID = "Device1", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			ExpectedResults.Add(new StmActivityLog(branch1.PK, "GB", "RFM") { S7_DeviceID = "Device2", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection));
			ExpectedResults.Add(new StmActivityLog(branch2.PK, "GB", "RFM") { S7_DeviceID = "Device1", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection));
			ExpectedResults.Add(new StmActivityLog(branch2.PK, "GB", "RFM") { S7_DeviceID = deviceIDLong1, S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection));
			new StmActivityLog(branch1.PK, "GB", "RFM") { S7_DeviceID = "Device1", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch1.PK, "GB", "RFM") { S7_DeviceID = "AndroidID - Device1", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch2.PK, "GB", "RFM") { S7_DeviceID = "AndroidID - Device2", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch1.PK, "GB", "RFM") { S7_DeviceID = "", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			ExpectedResults.Add(new StmActivityLog(branch1.PK, "GB", "EMT") { S7_DeviceID = "Device4", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection));
			ExpectedResults.Add(new StmActivityLog(branch1.PK, "GB", "EMT") { S7_DeviceID = "Device5", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection));
			new StmActivityLog(branch2.PK, "GB", "EMT") { S7_DeviceID = "Device4", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			ExpectedResults.Add(new StmActivityLog(branch2.PK, "GB", "EMT") { S7_DeviceID = deviceIDLong2, S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection));
			new StmActivityLog(branch1.PK, "GB", "EMT") { S7_DeviceID = "Device4", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch1.PK, "GB", "EMT") { S7_DeviceID = "AndroidID - Device3", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch2.PK, "GB", "EMT") { S7_DeviceID = "AndroidID - Device4", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch1.PK, "GB", "COR") { S7_DeviceID = "", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch1.PK, "GB", "COR") { S7_DeviceID = "InvalidDevice", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch1.PK, "GB", "COR") { S7_DeviceID = "", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch2.PK, "GB", "COR") { S7_DeviceID = "", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch2.PK, "GB", "COR") { S7_DeviceID = "InvalidDevice", S7_OpenDateTimeUtc = new DateTime(2021, 01, 01) }.InsertAndReturnObject(TestConnection);
			new StmActivityLog(branch2.PK, "GB", "COR") { S7_DeviceID = "", S7_OpenDateTimeUtc = new DateTime(2020, 01, 01) }.InsertAndReturnObject(TestConnection);
		}

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
