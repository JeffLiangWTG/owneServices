using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MailManager.Test
{
	class MessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestProcessorFactory()
		{
			var processorFactory = new MessageProcessorFactory("TS1", new IMessageFilterConfig[]
			{
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter1)),
				new MessageFilterAttribute("TS2", DummyBizoSchema.Constants.TableName, typeof(TestFilter2)),
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter3)),
			});

			var logger = new TestServiceLogger();
			var ctx = processorFactory.GetContext(logger);
			var processor = processorFactory.GetProcessor<DummyBusinessObject>();

			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_VarCharMax = "AAA";
			Assert(processor.Process(ctx, bo));
			AssertEquals("Information|TestFilter1", logger[0]);

			bo.Z0_VarCharMax = "AAABB";
			Assert(processor.Process(ctx, bo));
			AssertEquals("Information|TestFilter3", logger[1]);

			bo.Z0_VarCharMax = "AAABBBCC";
			Assert(!processor.Process(ctx, bo));
		}

		public void TestProcessorTypeIsNotBusinessObject()
		{
			var processorFactory = new MessageProcessorFactory("TS1", new IMessageFilterConfig[]
			{
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter1)),
				new MessageFilterAttribute("TS2", DummyBizoSchema.Constants.TableName, typeof(TestFilter2)),
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter3)),
			});

			var processor = processorFactory.GetProcessor<IDummyBizo>(DummyBizoSchema.Constants.TableName);
			var ctx = processorFactory.GetContext(new TestServiceLogger());

			var bo = Factory.NewWithValidTestData<DummyWithInterface>();
			bo.Z0_VarCharMax = "AAA";

			Assert(processor.IsMatch(ctx, bo));
		}

		public void TestProcessorCanProcessOnPODO()
		{
			var processorFactory = new MessageProcessorFactory("TS1", new IMessageFilterConfig[]
			{
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter1)),
				new MessageFilterAttribute("TS2", DummyBizoSchema.Constants.TableName, typeof(TestFilter2)),
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter3)),
			});

			var processor = processorFactory.GetProcessor<IDummyBizo>(DummyBizoSchema.Constants.TableName);
			var ctx = processorFactory.GetContext(new TestServiceLogger());

			var bo = new DummyPODO();
			bo.Z0_VarCharMax = "AAA";

			Assert(processor.IsMatch(ctx, bo));
		}

		public void TestGetProcessorAntContext()
		{
			var processorFactory = new MessageProcessorFactory("TS1", new IMessageFilterConfig[]
			{
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter1)),
				new MessageFilterAttribute("TS2", DummyBizoSchema.Constants.TableName, typeof(TestFilter2)),
				new MessageFilterAttribute("TS1", DummyBizoSchema.Constants.TableName, typeof(TestFilter3)),
			});

			var processor1 = processorFactory.GetProcessor<DummyBusinessObject>();
			var processor2 = processorFactory.GetProcessor<DummyBusinessObject>();
			Assert(Object.ReferenceEquals(processor1, processor2));

			var processor3 = processorFactory.GetProcessor<DummyChildBusinessObject>();
			Assert(!Object.ReferenceEquals(processor1, processor3));

			var ctx1 = processorFactory.GetContext(new TestServiceLogger());
			var ctx2 = processorFactory.GetContext(new TestServiceLogger());
			Assert(!Object.ReferenceEquals(ctx1, ctx2));
		}

		class TestFilter1
		{
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_VarCharMax, "^[A-Z]{1,3}$")]
			public bool FilterMethod(DummyBusinessObject bizObj, ILogger logger)
			{
				logger.Log(LogType.Information, GetType().Name);
				return true;
			}
		}

		class TestFilter2
		{
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_VarCharMax, "^[A-Z]{7,9}$")]
			public bool FilterMethod(DummyBusinessObject bizObj, ILogger logger)
			{
				logger.Log(LogType.Information, GetType().Name);
				return true;
			}
		}

		class TestFilter3
		{
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_VarCharMax, "^[A-Z]{4,6}$")]
			public bool FilterMethod(DummyBusinessObject bizObj, ILogger logger)
			{
				logger.Log(LogType.Information, GetType().Name);
				return true;
			}
		}
	}

	interface IDummyBizo : IDummy
	{
		string Z0_VarCharMax { get; }
	}

	class DummyWithInterface : DummyBusinessObject, IDummyBizo
	{
		public DummyWithInterface(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		string IDummyBizo.Z0_VarCharMax => throw new NotImplementedException();
	}

	class DummyPODO : IDummyBizo
	{
		public string Z0_VarCharMax { get; set; }
	}
}
