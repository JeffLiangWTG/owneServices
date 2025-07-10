using System;
using Enterprise.MailManager.MessageProcessor;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MailManager.Test
{
	class MessageProcessorContextTest : TestCase
	{
		public void TestGetFilterInstance()
		{
			var ctx = new MessageProcessorContext(new TestServiceLogger());

			var instance1a = ctx.GetFilterInstance<TestClass1>();
			var instance2a = ctx.GetFilterInstance<TestClass2>();
			var instance1b = ctx.GetFilterInstance<TestClass1>();
			var instance2b = ctx.GetFilterInstance<TestClass2>();

			Assert(instance1a.Id == instance1b.Id);
			Assert(instance2a.Id == instance2b.Id);
			Assert(instance1a.Id != instance2a.Id);
		}

		class TestClass1
		{
			public TestClass1()
			{
				Id = Guid.NewGuid();
			}

			public Guid Id { get; private set; }
		}

		class TestClass2 : TestClass1
		{
		}
	}
}
