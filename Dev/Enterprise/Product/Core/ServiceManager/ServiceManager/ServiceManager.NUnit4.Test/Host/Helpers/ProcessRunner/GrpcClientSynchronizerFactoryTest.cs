using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.ProcessRunner
{
	public class GrpcClientSynchronizerFactoryTest
	{
		[SetUp]
		public void SetUp()
		{
			factory = new GrpcClientSynchronizerFactory();
		}

		[Test]
		public void TestCreate()
		{
			// Arrange
			// Act
			// Assert
			var synchronizer = factory!.Create(new GrpcEventHandleNames());
			Assert.That(synchronizer, Is.Not.EqualTo(default(IGrpcClientSynchronizer)));
		}

		GrpcClientSynchronizerFactory? factory;
	}
}
