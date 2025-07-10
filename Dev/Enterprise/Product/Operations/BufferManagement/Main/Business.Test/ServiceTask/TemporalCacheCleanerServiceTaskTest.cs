using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.PAVE.Common.Cache;
using CargoWise.Types;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TemporalCacheCleanerServiceTask))]
	public class TemporalCacheCleanerServiceTaskTest : ServiceTaskTestCase<TemporalCacheCleanerServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attributes = typeof(TemporalCacheCleanerServiceTask).Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), inherit: false);
			var attribute = Array.Find((HostedServiceAttribute[])attributes, a => a.TypeName == typeof(TemporalCacheCleanerServiceTask).FullName);

			AssertEquals("Minimum Period should be 1 hour", "1hour", attribute.MinimumPeriod);
			AssertEquals("MaximumPeriod Period should be 1 month", "1month", attribute.MaximumPeriod);
			AssertEquals("Can Run InAny Branch ", true, attribute.CanRunInAnyBranch);
			AssertEquals("Is mandatory", true, attribute.IsMandatory);
			AssertEquals("Not Allows Multiple Instances", false, attribute.AllowsMultipleInstances);
		}

		[UseSnapshotProtection]
		[TestDate(2023, 10, 28, 1, 2, 5)]
		public void TestRun_ShouldDeleteCacheItems()
		{
			var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();
			var serviceTask = CreateServiceTask();
			var now = ZDateTimeOffset.UtcNow.ToDateTimeOffset();
			var cn = serviceTask.CreateConnection_ForTest();
			cn.ExecuteNonQuery("DELETE dbo.TemporalCache");

			distributedCache.Set("c1", new CacheItem(), new DistributedCacheOptions(now.AddSeconds(60)), "E");
			distributedCache.Set("c2", new CacheItem(), new DistributedCacheOptions(now.AddSeconds(120)), "E");
			distributedCache.Set("c3", new CacheItem(), new DistributedCacheOptions(now.AddSeconds(180)), "E");

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(30);
			serviceTask.RunTask(CancellationToken.None);

			AssertContains("Information|Cache items deleted: 0", logger.ToString());
			AssertNotNull(distributedCache.Get<CacheItem>("c1"));
			AssertNotNull(distributedCache.Get<CacheItem>("c2"));
			AssertNotNull(distributedCache.Get<CacheItem>("c3"));

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(60);
			serviceTask.RunTask(CancellationToken.None);

			AssertContains("Information|Cache items deleted: 1", logger.ToString());
			AssertEquals(2, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TemporalCache"));
			AssertNull(distributedCache.Get<CacheItem>("c1"));
			AssertNotNull(distributedCache.Get<CacheItem>("c2"));
			AssertNotNull(distributedCache.Get<CacheItem>("c3"));

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(120);
			serviceTask.RunTask(CancellationToken.None);

			AssertContains("Information|Cache items deleted: 2", logger.ToString());
			AssertEquals(0, cn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TemporalCache"));
			AssertNull(distributedCache.Get<CacheItem>("c1"));
			AssertNull(distributedCache.Get<CacheItem>("c2"));
			AssertNull(distributedCache.Get<CacheItem>("c3"));
		}

		[UseSnapshotProtection]
		[TestDate(2023, 10, 28, 1, 2, 5)]
		public void TestRun_ShouldDeleteCacheItemsInBatches()
		{
			var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();
			var serviceTask = CreateServiceTask(batchSize: 3);
			var now = ZDateTimeOffset.UtcNow.ToDateTimeOffset();
			Db.Connection.ExecuteScalar("DELETE dbo.TemporalCache");

			for (int i = 0; i < 5; i++)
			{
				distributedCache.Set("c1Second" + i, new CacheItem(), new DistributedCacheOptions(now.AddSeconds(1)), "E");
			}

			for (int i = 0; i < 9; i++)
			{
				distributedCache.Set("c2Second" + i, new CacheItem(), new DistributedCacheOptions(now.AddSeconds(2)), "E");
			}

			using (serviceTask.CreateConnection_ForTest().TrackExecutedCommands())
			{
				serviceTask.RunTask(CancellationToken.None);
				AssertEquals(1, serviceTask.CurrentConnection.ExecutedCommands.Count(c => c.Contains("DELETE TOP (3) TemporalCache")));
			}

			AssertContains("Information|Cache items deleted: 0", logger.ToString());
			AssertEquals(14, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TemporalCache"));

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			using (serviceTask.CreateConnection_ForTest().TrackExecutedCommands())
			{
				serviceTask.RunTask(CancellationToken.None);
				AssertEquals(2, serviceTask.CurrentConnection.ExecutedCommands.Count(c => c.Contains("DELETE TOP (3) TemporalCache")));
			}

			AssertContains("Information|Cache items deleted: 5", logger.ToString());
			AssertEquals(9, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TemporalCache"));

			logger.ClearLog();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			using (serviceTask.CreateConnection_ForTest().TrackExecutedCommands())
			{
				serviceTask.RunTask(CancellationToken.None);
				AssertEquals(4, serviceTask.CurrentConnection.ExecutedCommands.Count(c => c.Contains("DELETE TOP (3) TemporalCache")));
			}

			AssertContains("Information|Cache items deleted: 9", logger.ToString());
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.TemporalCache"));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		TestServiceLogger logger;

		TemporalCacheCleanerServiceTask_ForTest CreateServiceTask(int? batchSize = null) => new TemporalCacheCleanerServiceTask_ForTest(logger, batchSize);

		protected override void SetUpCore()
		{
			base.SetUpCore();
			logger = new TestServiceLogger();
		}

		class TemporalCacheCleanerServiceTask_ForTest : TemporalCacheCleanerServiceTask
		{
			public TemporalCacheCleanerServiceTask_ForTest(ILogger logger, int? batchSize)
			{
				ServiceLogger = logger;
				this.batchSize = batchSize ?? base.BatchSize;
			}

			readonly int batchSize;

			protected override int BatchSize => batchSize;

			protected override DbConnection GetConnection() => CurrentConnection ?? base.GetConnection();

			public DbConnection CurrentConnection { get; private set; }
			public DbConnection CreateConnection_ForTest() => (CurrentConnection = Db.NewExtraConnectionToMainDb());
		}

		class CacheItem { }
	}
}
