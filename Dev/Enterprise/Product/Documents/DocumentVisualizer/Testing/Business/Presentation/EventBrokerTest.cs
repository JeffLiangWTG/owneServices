using System;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class EventBrokerTest : TestCase
	{
		public void TestDoNotTerminateTheStream()
		{
			var broker = new EventBroker();

			broker.GetEvent<TestEvent>().Subscribe(
				args => Assert(true),
				() => Fail("should not signal the end of stream because events will be unsubscribed"));

			broker.Publish(new TestEvent());
		}

		class TestEvent
		{
		}
	}
}