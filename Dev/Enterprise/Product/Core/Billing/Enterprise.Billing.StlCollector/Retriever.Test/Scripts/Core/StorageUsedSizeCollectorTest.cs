using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Billing.StlCollector.Retriever.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Integration.Billing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core.Testing
{
	sealed class StorageUsedSizeCollectorTest : TransactionedTestCase
	{
		#region TestRun

		public void TestCorrectS3BucketDataReturned()
		{
			// Arrange
			var collector = new StorageUsedSizeCollector(new BillingTransactionFactory("DUMMYID"));
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(@"{
""bucket"": {
					""bucket_name"": ""testBucket"",
					""object_count"": 1000,
					""size_bytes"": 1000
}
}")
				});

			var savedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "S3"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://S3.wtg.local"))
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testBucket"))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithSpecifiedHttpClient(new HttpClient(httpMessageHandlerMock.Object)))
			using (DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// Act
				var actualDataRows = collector.Run(TestDateTimeRange).Select(t => (IBillingTransaction)t).ToList();

				// Assert
				AssertEquals("Number of Transactions >= 4", true, actualDataRows.Count >= 4);
				AssertStorageUsedSizeRow(actualDataRows, Db.DatabaseName);
				AssertStorageUsedSizeRow(actualDataRows, userRepositoryDb);
				AssertStorageUsedSizeRow(actualDataRows, eDoc999Db);
				AssertStorageUsedSizeRow(actualDataRows, "testBucket", 1);
				AssertEquals("Non CW1 database retrieved?", false, actualDataRows.Any(r => r.Reference1 == nonRelevantDb));
				AssertEquals("System database retrieved?", false, actualDataRows.Any(r => r.Reference1 == Db.SqlMasterDb));
			}

			EnvProxy.SetHostedLocationForTest(savedLocation);
		}

		public void TestCorrectDataReturnedForDatabaseUsage()
		{
			// Arrange
			var collector = new StorageUsedSizeCollector(new BillingTransactionFactory("DUMMYID"));

			// Act
			var actualDataRows = collector.Run(TestDateTimeRange).Select(t => (IBillingTransaction)t).ToList();

			// Assert
			AssertEquals("Number of Transactions >= 3", true, actualDataRows.Count >= 3);
			AssertStorageUsedSizeRow(actualDataRows, Db.DatabaseName);
			AssertStorageUsedSizeRow(actualDataRows, userRepositoryDb);
			AssertStorageUsedSizeRow(actualDataRows, eDoc999Db);
			AssertEquals("Non CW1 database retrieved?", false, actualDataRows.Any(r => r.Reference1 == nonRelevantDb));
			AssertEquals("System database retrieved?", false, actualDataRows.Any(r => r.Reference1 == Db.SqlMasterDb));
		}

		public void TestWorksOnDatabasesWithVeryLargeAllocatedSpace()
		{
			TestConnection.ExecuteNonQuery("ALTER PROCEDURE ep_DatabaseSetInfo AS SELECT 'DoNotCareAboutDbName', 'DoNotCareAboutDbGuid', 2147483647, 2147483648");

			using (var cmd = TestConnection.Command("ep_DatabaseSetInfo"))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				using (var reader = cmd.ExecuteReader())
				{
					Assert("Should return a record but it didn't.", reader.Read());
					AssertEquals("TransactionReference01", "DoNotCareAboutDbName", reader[0].ToString());
					AssertEquals("TransactionGuidReference", "DoNotCareAboutDbGuid", reader[1].ToString());
					AssertEquals("ItemCount", 2147483647, Convert.ToInt32(reader[2]));
				}
			}
		}

		public void TestNotLoadedWhenCollectingRetrospectiveData()
		{
			var utcNow = DateTime.UtcNow;
			var retrospectiveDate = utcNow.Day >= 22 ? utcNow.AddMonths(-1) : utcNow.AddMonths(-2);
			var retrospectiveRange = AusydMonthRange.New(retrospectiveDate.Year, retrospectiveDate.Month);
			AssertEquals($"Is StorageUsedSizeCollector applicable to range {retrospectiveRange}?", false, retrospectiveRange.SelectApplicableScripts(new ScriptWithConfig(new StorageUsedSizeCollector(), new StlItemRegistrySettingsStub())).Any(s => s.Script.StlGrain != StlDataGrain.Daily));

			var currentDate = utcNow.Day >= 20 ? utcNow : utcNow.AddMonths(-1);
			var currentRange = AusydMonthRange.New(currentDate.Year, currentDate.Month);
			AssertEquals($"Is StorageUsedSizeCollector applicable to range {currentRange}?", true, currentRange.SelectApplicableScripts(new ScriptWithConfig(new StorageUsedSizeCollector(), new StlItemRegistrySettingsStub())).Any());
		}

		public void TestFieldMaxSizes()
		{
			var collector = new StorageUsedSizeCollector();

			AssertEquals("FeatureCode.Length <= 3", true, collector.Code.Length <= 3);
			AssertEquals("RoleName.Length <= 50", true, collector.Role.Length <= 50);
			AssertEquals("ModuleName.Length <= 50", true, collector.Module.Length <= 50);
			AssertEquals("FunctionName.Length <= 50", true, collector.Function.Length <= 50);
			AssertEquals("FeatureName.Length <= 75", true, collector.Feature.Length <= 75);
		}

		public void TestStorageUsedSizeCollectorIsInStlCollectorsList()
		{
			var collector = new StorageUsedSizeCollector();
			var stlCollectorsList = ObjectFactory.Get<ListObject>("StlCustomCollectorsList").Cast<IStlItem>();

			AssertEquals("StlCustomCollectorsList does not contain StorageUsedSizeCollector. Please add StorageUsedSizeCollector to the StlCustomCollectorsList of StlCustomCollectorsListConfiguration.xml.", true, stlCollectorsList.Any(stlCollector => stlCollector.Code == collector.Code));
		}

		public void TestCollectorType()
		{
			var collector = new StorageUsedSizeCollector();
			AssertEquals("Incorrect value for property CollectorType", StlCollectorType.Custom, collector.CollectorType);
		}

		public void TestIsNotActiveForSelfHosted()
		{
			var collector = new StorageUsedSizeCollector();
			var savedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("NCW");

			Assert("Shouldn't be active for self hosted", !collector.IsActive);

			EnvProxy.SetHostedLocationForTest(savedLocation);
		}

		public void TestIActiveForInternalTestSystemWithTestingEnabled()
		{
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var collector = new StorageUsedSizeCollector();
				var savedLocation = EnvProxy.HostedLocation;
				EnvProxy.SetHostedLocationForTest("");

				Assert("Should be active for internal test system with testing enabled", collector.IsActive);

				EnvProxy.SetHostedLocationForTest(savedLocation);
			}
		}

		#endregion

		#region Implementation

		int GetDatabaseUsedSize(string dbName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT convert(int, ceiling(sum(used_pages)/128.)) FROM [{0}].sys.allocation_units", dbName);
			int usedSize = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			return usedSize;
		}

		void AssertStorageUsedSizeRow(IEnumerable<IBillingTransaction> resultRows, string mediaName, int? expectedItemCount = null)
		{
			var dataRow = resultRows.Single(r => r.Reference1 == mediaName);
			AssertEquals("ClientID", GlbCompany.CurrentCompany.LicenceKeyIdentifier.ToString(), dataRow.ClientID);
			AssertEquals("ClientNumber", "DUMMYID", dataRow.ClientNumber);
			AssertEquals("Category", "STL", dataRow.Category);
			AssertEquals("PriceItemCode", "STS", dataRow.PriceItemCode);
			AssertEquals("ReportingSource", "ENT", dataRow.ReportingSource);
			AssertEquals("Branch", null, dataRow.GetBranchCode());
			AssertEquals("TransactionDateUtc", TestDateTimeRange.MonthlyRangeStartInclusive, dataRow.ServiceOccuredUTC);
			AssertEquals("UserCode", null, dataRow.ClientStaffCode);
			AssertEquals("TransactionReference01", mediaName, dataRow.Reference1);
			AssertEquals("TransactionReference02", null, dataRow.Reference2);
			AssertEquals("TransactionReference03", null, dataRow.Reference3);
			AssertEquals("TransactionReference04", null, dataRow.Reference4);
			AssertEquals("ItemCount", expectedItemCount ?? GetDatabaseUsedSize(mediaName), dataRow.BillableCount);
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using (var testAdminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(testAdminConnection, userRepositoryDb);
				AdoTestUtils.CreateDbIfNotExists(testAdminConnection, eDoc999Db);
				AdoTestUtils.CreateDbIfNotExists(testAdminConnection, nonRelevantDb);
			}
		}

		protected override void FinalTearDown()
		{
			using (var testAdminConnection = Db.NewAdminConnection())
			{
				// Does not drop UserRepository DB as it could be there prior to the test
				AdoTestUtils.DropDbIfExists(testAdminConnection, eDoc999Db);
				AdoTestUtils.DropDbIfExists(testAdminConnection, nonRelevantDb);
			}

			base.FinalTearDown();
		}

		IDateTimeRange TestDateTimeRange
		{
			get
			{
				if (testDateRange == null)
				{
					var utcNow = DateTime.UtcNow;
					testDateRange = AusydMonthRange.New(utcNow.Year, utcNow.Month);
				}

				return testDateRange;
			}
		}

		IDateTimeRange testDateRange;

		readonly string userRepositoryDb = Db.DatabaseName + DbUserRepository.RepositoryDbSuffix;
		readonly string eDoc999Db = Db.DatabaseName + "_SD999";
		readonly string nonRelevantDb = Db.DatabaseName + "_NonCw1Db";

		#endregion
	}
}
