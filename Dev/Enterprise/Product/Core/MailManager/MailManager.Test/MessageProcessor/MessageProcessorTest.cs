using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MailManager.Test
{
	class MessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var processor = new MessageProcessor<DummyBusinessObject>(new IMessageFilterConfig[]
			{
				new MessageFilterAttribute("", DummyBizoSchema.Constants.TableName, typeof(TestFilter1)),
				new MessageFilterAttribute("", DummyBizoSchema.Constants.TableName, typeof(TestFilter2))
			});

			var logger = new TestServiceLogger();
			IMessageProcessorContext ctx = new MessageProcessorContext(logger);

			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_VarCharMax = "ABC";
			bo.Z0_Bool = true;
			Assert(processor.Process(ctx, bo));
			AssertEquals("Information|FilterMethod1a", logger[0]);

			bo.Z0_Bool = false;
			Assert(processor.Process(ctx, bo));
			AssertEquals("Information|FilterMethod2a", logger[1]);

			bo.Z0_VarCharMax = "ABCD";
			Assert(!processor.Process(ctx, bo));

			bo.Z0_Description = "Log: Test!";
			Assert(processor.Process(ctx, bo));
			AssertEquals("Information|Test!", logger[2]);
		}

		public void TestIsMatch()
		{
			var matchMethodsa = Factory.NewWithValidTestData<DummyBusinessObject>();
			matchMethodsa.Z0_VarCharMax = "XXX";

			var matchMethod1b = Factory.NewWithValidTestData<DummyBusinessObject>();
			matchMethod1b.Z0_VarCharMax = "XXXX";
			matchMethod1b.Z0_Description = "LOG: BLAH";

			var matchesNothing = Factory.NewWithValidTestData<DummyBusinessObject>();
			matchesNothing.Z0_VarCharMax = "69696";

			var logger = new TestServiceLogger();
			var ctx = new MessageProcessorContext(logger);

			var processor = new MessageProcessor<DummyBusinessObject>(new IMessageFilterConfig[]
			{
				new MessageFilterAttribute("", DummyBizoSchema.Constants.TableName, typeof(TestFilter1)),
				new MessageFilterAttribute("", DummyBizoSchema.Constants.TableName, typeof(TestFilter2))
			});

			Assert("Matches two filters, so should be included", processor.IsMatch(ctx, matchMethodsa));
			Assert("Matches one filters, so should be included", processor.IsMatch(ctx, matchMethod1b));
			Assert("Does not match any filter, so should not be included", !processor.IsMatch(ctx, matchesNothing));
		}

		bool UsesMessageFilters(IHostedServiceAttribute attribute)
		{
			if (!string.IsNullOrEmpty(attribute.TypeName))
			{
				var taskType = Type.GetType(attribute.TypeName + "," + attribute.TypeAssemblyName);
				return taskType.GetInterface(nameof(IMessageFiltersUser)) != null;
			}

			return false;
		}

		class TestFilter1
		{
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_VarCharMax, "^[A-Z]{1,3}$")]
			public bool FilterMethod1a(DummyBusinessObject bizObj, ILogger logger)
			{
				if (bizObj.Z0_Bool)
				{
					logger.Log(LogType.Information, "FilterMethod1a");
				}

				return bizObj.Z0_Bool;
			}

			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_VarCharMax, "^[A-Z]{4,6}$")]
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_Description, @"^log\:", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
			public bool FilterMethod1b(DummyBusinessObject bizObj, ILogger logger)
			{
				logger.Log(LogType.Information, bizObj.Z0_Description.Substring(4).Trim());
				return true;
			}
		}

		class TestFilter2
		{
			[MessageFilterCondition(DummyBizoSchema.Constants.Z0_VarCharMax, "^[A-Z]{1,3}$")]
			public bool FilterMethod2a(ILogger logger, DummyBusinessObject bizObj)
			{
				if (!bizObj.Z0_Bool)
				{
					logger.Log(LogType.Information, "FilterMethod2a");
				}

				return !bizObj.Z0_Bool;
			}
		}
	}
}
