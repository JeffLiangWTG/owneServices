using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class BackgroundThreadActionQueueFactoryTest
	{
		[Test]
		public void TestFactoryCreatesSingleton()
		{
			var actionQueue1 = backgroundThreadActionQueueFactory!.BackgroundThreadActionQueue;
			var actionQueue2 = backgroundThreadActionQueueFactory!.BackgroundThreadActionQueue;

			Assert.That(actionQueue1, Is.EqualTo(actionQueue2));
		}

		[SetUp]
		public void SetUp()
		{
			backgroundThreadActionQueueFactory = new BackgroundThreadActionQueueFactory(Mock.Of<ICancellationTokenProvider>());
		}

		BackgroundThreadActionQueueFactory? backgroundThreadActionQueueFactory;
	}
}
