using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class RemotePrintHubDispatcherTest : TestCase
	{
		public void TestAuthorizeRequest_ShouldReturnFalseWhenJsonSerializationExceptionIsThrown()
		{
			var queryString = new Mock<INameValueCollection>();
			queryString.Setup(q => q["connectionData"]).Returns("1);if(typeof dqojj===\"undefined\"){a=new Date();do{b=new Date();}while(b-a<20000);}dqojj=1;//");

			var mockRequest = new Mock<IRequest>();
			mockRequest.Setup(r => r.QueryString).Returns(queryString.Object);

			var hubConfiguration = new HubConfiguration();
			var dispatcher = new RemotePrintHubDispatcher(hubConfiguration);
			dispatcher.Initialize(hubConfiguration.Resolver);

			var result = true;
			AssertNoExceptionThrown(() => result = dispatcher.Authorize(mockRequest.Object));
			AssertEquals("Should return false without error", false, result);
		}
	}
}
