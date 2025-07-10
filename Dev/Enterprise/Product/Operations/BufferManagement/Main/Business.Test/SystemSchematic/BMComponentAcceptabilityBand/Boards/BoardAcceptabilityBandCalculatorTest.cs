using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.PAVE.Common.Cache;
using CargoWise.Types;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Moq;
using static Enterprise.BufferManagement.Business.BoardAcceptabilityBandCalculator;

namespace Enterprise.BufferManagement.Business.Test
{
	class BoardAcceptabilityBandCalculatorTest : BMSTestCaseWithFactory
	{
		public void TestShouldReportErrorAndCalculateOtherABs_WhenOneAB_FailToCalculate()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var goodAB = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "GoodAB", type: AcceptabilityBandTypes.Codes.Count);
			var badAB = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "BadAB", type: AcceptabilityBandTypes.Codes.Count);

			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			var boardBand1 = BMSTestHelper.AddAcceptabilityBandToSection(section, badAB);
			var boardBand2 = BMSTestHelper.AddAcceptabilityBandToSection(section, goodAB);
			var boardBands = new[] { boardBand1, boardBand2 };

			Factory.Save();

			distributedCacheMock.Reset();

			var expectedException = new Exception("This is a bad band!");
			distributedCacheMock.Setup(m => m.Get<ABCacheItem[]>(It.IsAny<string>()))
				.Returns((string key) =>
				{
					if (key.Contains(badAB.PK.ToString()))
					{
						throw expectedException;
					}

					return null;
				});

			var results = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, config.ReleaseGroup.PK, boardBands);

			AssertEquals(1, results.Length);
			AssertEquals(0m, results[0].Result.Value);

			AssertEquals(expectedException, ErrorReporter.LastExceptionReported);
			AssertEquals("Band name: [BadAB], Type: [NUM]", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestShouldNotCalculateOrThrowException_WhenBandIsInactive()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_IsActive = false;

			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			var boardBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);
			var boardBands = new[] { boardBand };

			Factory.Save();

			var result = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, config.ReleaseGroup.PK, boardBands);

			AssertEquals(0, result.Length);
		}

		public void TestShouldHandleNonExistentBand()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			Factory.Save();

			var result = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, config.ReleaseGroup.PK, "Nonexistent band");

			AssertEquals(0, result.Length);
		}

		public void TestShouldHandleBandByName()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "My Band", type: AcceptabilityBandTypes.Codes.Count);

			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			Factory.Save();

			var result = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, config.ReleaseGroup.PK, "My Band");

			AssertEquals(1, result.Length);
		}

		public void TestShouldCacheLocally_WhenFindResultsOnCentralizedCache()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersByReleaseGroup = false;
			band.BAB_FiltersBySection = false;

			CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			var boardBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);
			var boardBands = new[] { boardBand };

			Factory.Save();

			var result = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, config.ReleaseGroup.PK, boardBands);
			var localCache = GetLocalCache(band.PK);

			AssertEquals(1m, result[0].Result.Value);
			AssertEquals(1, localCache.Keys.Count);
			AssertEquals(1, centralizedCache.Keys.Count);

			ClearLocalCache();
			localCache = GetLocalCache(band.PK);

			AssertEquals(0, localCache.Keys.Count);
			AssertEquals(1, centralizedCache.Keys.Count);

			result = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, config.ReleaseGroup.PK, boardBands);
			localCache = GetLocalCache(band.PK);

			AssertEquals(1m, result[0].Result.Value);
			AssertEquals(1, localCache.Keys.Count);
			AssertEquals(1, centralizedCache.Keys.Count);
		}

		public void TestShouldCachePerReleaseGroup_WhenIsFilteringByReleaseGroup()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersByReleaseGroup = true;
			band.BAB_FiltersBySection = false;

			var otherReleaseGroup = BMSTestHelper.CreateGroup(Factory, "OTH");
			BMSTestHelper.CreateReleaseGroup(config.System, otherReleaseGroup);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: otherReleaseGroup.PK);
			var workflow3 = CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: config.ReleaseGroup.PK);

			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			var boardBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);
			var boardBands = new[] { boardBand };

			Factory.Save();

			var result1 = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, config.ReleaseGroup.PK, boardBands);
			var result2 = BoardAcceptabilityBandCalculator.Calculate(Factory, section.PK, otherReleaseGroup.PK, boardBands);
			AssertEquals(2m, result1[0].Result.Value);
			AssertEquals(1m, result2[0].Result.Value);

			var localCache = GetLocalCache(band.PK);

			AssertEquals("Should have create cache for each release group", 2, localCache.Count);
			AssertEquals(1, localCache.Keys.Count(k => k.Contains(config.ReleaseGroup.PK.ToString())));
			AssertEquals(1, localCache.Keys.Count(k => k.Contains(otherReleaseGroup.PK.ToString())));
			AssertEquals(1, centralizedCache.Keys.Count(k => k.Contains(config.ReleaseGroup.PK.ToString())));
			AssertEquals(1, centralizedCache.Keys.Count(k => k.Contains(otherReleaseGroup.PK.ToString())));
		}

		#region Helpers

		static Dictionary<string, ABCacheItem[]> GetLocalCache(ZGuid bandPK) => MemoryCache.Default.Where(c => c.Key.StartsWith("BoardAcceptabilityBandLocalCache.ABResult:" + bandPK))
				.ToDictionary(kv => kv.Key, kv => kv.Value as ABCacheItem[]);
		readonly Dictionary<string, (ABCacheItem[] value, DistributedCacheOptions options)> centralizedCache = new Dictionary<string, (ABCacheItem[] value, DistributedCacheOptions options)>();

		static void ClearLocalCache()
		{
			var memoryCache = MemoryCache.Default;

			var cacheItemKeys = memoryCache.Where(c => c.Key.StartsWith("BoardAcceptabilityBandLocalCache.ABResult:"))
				.Select(c => c.Key)
				.ToArray();

			foreach (var cacheKey in cacheItemKeys)
			{
				memoryCache.Remove(cacheKey);
			}
		}

		#endregion

		#region SetUp TearDown

		protected override void SetUp()
		{
			base.SetUp();
			centralizedCache.Clear();
			ClearLocalCache();
			MockCentralizedCache();
			BMSTestHelper.EnableAcceptabilityBandResultClientCache();
			BMSRegistry.Instance.AcceptabilityBandServerCacheTimePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Convert.ToInt32(BMSRegistry.Instance.AcceptabilityBandServerCacheTimePeriod.DefaultValue));
		}

		protected override void TearDown()
		{
			base.TearDown();
			objectFactorySubstituteDisposable.Dispose();
			ClearLocalCache();
		}

		IDisposable objectFactorySubstituteDisposable;
		Mock<IDistributedCache> distributedCacheMock;

		void MockCentralizedCache()
		{
			distributedCacheMock = new Mock<IDistributedCache>();
			distributedCacheMock.Setup(m => m.Set(It.IsAny<string>(), It.IsAny<ABCacheItem[]>(), It.IsAny<DistributedCacheOptions>(), It.IsAny<string>()))
				.Callback((string key, ABCacheItem[] value, DistributedCacheOptions options, string userCode) =>
				{
					centralizedCache[key] = (value, options);
				});

			distributedCacheMock.Setup(m => m.Get<ABCacheItem[]>(It.IsAny<string>()))
				.Returns((string key) =>
				{
					if (!centralizedCache.TryGetValue(key, out var cacheItem))
					{
						return null;
					}

					if (cacheItem.options.AbsoluteExpiration < ZDateTimeOffset.UtcNow)
					{
						return null;
					}

					return cacheItem.value;
				});

			var distributedCacheFactoryMock = new Mock<IDistributedCacheFactory>();
			distributedCacheFactoryMock.Setup(m => m.Create()).Returns(distributedCacheMock.Object);
			distributedCacheFactoryMock.Setup(m => m.Create(It.IsAny<IDBConnectionFactory>())).Returns(distributedCacheMock.Object);

			objectFactorySubstituteDisposable = ObjectFactory.Substitute(distributedCacheFactoryMock.Object);
		}

		#endregion
	}
}
