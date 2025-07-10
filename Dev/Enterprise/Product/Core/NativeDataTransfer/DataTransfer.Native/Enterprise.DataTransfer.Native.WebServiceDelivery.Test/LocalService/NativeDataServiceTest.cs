using System;
using Enterprise.DataTransfer.Native.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService.Testing
{
	sealed class NativeDataServiceTest : TestCase
	{
		public void TestNoClientGetsErrorNotException()
		{
			var service = new NativeDataService();
			var result = service.Update(null);
			AssertNotNull("service.Update(null)", result);
			AssertEquals(true, result.HasError);
			AssertEquals("Fail to create Client application", result.ErrorMessage);
		}

		[ExpectNoExceptions]
		public void TestClientCloseWhenExceptionThrow()
		{
			client.Setup(c => c.Update(It.IsAny<IRequestMessage>())).Throws(new Exception());
			try
			{
				service.Update(null);
			}
			catch
			{
			}
			client.Verify(c => c.Close(), Times.Once);
			client.Verify(c => c.Update(It.IsAny<IRequestMessage>()), Times.Exactly(4));
		}

		protected override void SetUp()
		{
			mocks = new MockRepository(MockBehavior.Loose);
			service = new NativeDataService();
			client = mocks.Create<INativeServiceClient>();
			service.Client = client.Object;
		}
		MockRepository mocks;
		Mock<INativeServiceClient> client;
		NativeDataService service;
	}
}
