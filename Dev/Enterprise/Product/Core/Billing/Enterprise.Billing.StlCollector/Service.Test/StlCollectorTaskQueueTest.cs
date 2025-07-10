using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.Billing.StlCollector.Retriever.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Service.Testing
{
	class StlCollectorTaskQueueTest : TestCaseWithFactory
	{
		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogOfNone()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1", "TS2")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 0, queue.QueueResult.QueueSize);
				AssertEquals("Wrong queue age reported", TimeSpan.Zero, queue.QueueResult.MaximumItemAge);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestLegacyWatermarkIgnored()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1", "TS2")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				SystemDataRegistry.Instance.StlCollectorHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 0, queue.QueueResult.QueueSize);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogOfOne()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				SetWatermark("TS1", "Testing Only", TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 1, queue.QueueResult.QueueSize);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogOfTwo()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1", "TS2")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				SetWatermark("TS1", "Testing Only", TestDateAttribute.Date.AddHours(-3));
				SetWatermark("TS2", "Testing Only", TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 2, queue.QueueResult.QueueSize);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogOnlyIncludesActiveCollectors()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				SetWatermark("TS1", "Testing Only", TestDateAttribute.Date.AddHours(-3));
				SetWatermark("TS2", "Testing Only", TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 1, queue.QueueResult.QueueSize);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogOfOneTheOtherUpToDate()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1", "TS2")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				SetWatermark("TS1", "Testing Only", TestDateAttribute.Date.AddHours(-2));
				SetWatermark("TS2", "Testing Only", TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 1, queue.QueueResult.QueueSize);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogIsZeroWhenSTLDisabledThroughRefData()
		{
			var configType = Factory.New<Customs.Shared.IRefSysConfigType>();
			configType.ZRT_ConfigCode = "STOPSTLUSS";
			configType.ZRT_Description = "Stop collecting and submitting usage data";
			configType.ZRT_LongDescription = "Stop collecting and submitting usage data";
			var config = Factory.New<Customs.Shared.IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = "STOPSTLUSS";
			config.ZRC_BitValue = true;
			config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			Factory.Save();

			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1", "TS2")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				SetWatermark("TS1", "Testing Only", TestDateAttribute.Date.AddHours(-2));
				SetWatermark("TS2", "Testing Only", TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 0, queue.QueueResult.QueueSize);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogIsZeroWhenServiceTaskDisabledOnTestSystem()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test, isInternalSystem: true))
			{
				SetWatermark("TS1", "Testing Only", TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 0, queue.QueueResult.QueueSize);
			}
		}

		[TestDate(2020, 7, 2, 14, 0, 0)]
		public void TestBacklogOfOneWhenTestingEnabled()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1")))
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test))
			{
				SetWatermark("TS1", "Testing Only", TestDateAttribute.Date.AddHours(-3));

				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 1, queue.QueueResult.QueueSize);
			}
		}

		public void TestBacklogWhenNoCurrentCompany()
		{
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test))
			using (Env.Instance.SetTemporaryUserContext(new UserContext()))
			{
				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 0, queue.QueueResult.QueueSize);
			}
		}

		public void TestBacklogAge()
		{
			using (ObjectFactory.Substitute(GetDummyScriptFactory("TS1", "TS2")))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var watermarkDate = new DateTime(2023, 11, 1);
				SetWatermark("TS1", "Testing Only", watermarkDate);

				var provider = new StlCollectorTaskQueue();
				var expectedAge = DateTime.UtcNow - watermarkDate;
				AssertCloseEnough((int)expectedAge.TotalSeconds, (int)provider.QueueResult.MaximumItemAge.TotalSeconds, 60);
			}
		}

		static IScriptFactory GetDummyScriptFactory(params string[] codes)
		{
			return new DummyScriptFactory(codes.Select(c => new DummyScriptItem(isActive: true, c)));
		}

		static void SetWatermark(string code, string feature, DateTime time)
		{
			SystemDataRegistry.Instance.GetStlCollectorHighWaterMark(code, feature).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, time);
		}

		public void TestQueueResultDoesNotThrowErrorWhenDynamicCollectorsAreCreated()
		{
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			using (Env.Instance.SetTemporaryUserContext(new UserContext()))
			{
				var queue = new StlCollectorTaskQueue();
				AssertEquals("Wrong queue size reported", 0, queue.QueueResult.QueueSize);
			}
		}
	}
}
