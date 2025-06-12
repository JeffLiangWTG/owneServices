using System;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class eHub2MessageHandlerTests
	{
		[TestMethod]
		public void TestSendMessageWillSendSuccessfully()
		{
			var mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };

			var reciever = mockRepository.Create<IEHub2Reciever>();
			reciever.Setup(_ => _.SendMessage(It.IsAny<eHub2GatewayMessage>()));
			var endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IEHub2Reciever)))
			{
				Address = new EndpointAddress(new Uri("net.tcp://localhost:11809/eHub2Gateway/eHub2Gateway.svc"))
			};
			var channelFactory = mockRepository.Create<ChannelFactory<IEHub2Reciever>>(endpoint);
			channelFactory.Setup(_ => _.CreateChannel(It.IsAny<EndpointAddress>(), It.IsAny<Uri>())).Returns(reciever.Object);
			channelFactory.As<IDisposable>().Setup(c => c.Dispose());
			var handler = mockRepository.Create<eHub2MessageHandler>();
			handler.Setup(_ => _.NewChannelFactory()).Returns(channelFactory.Object);

			handler.Object.Handle("TSTCLIENT", Guid.NewGuid(), CreateTestMessage());

			mockRepository.VerifyAll();
		}


		[TestMethod]
		public void TestSendMessageEnvironmentalExceptionsAreRethrownAsSystemUnderMaintanance()
		{
			var insufficientMemoryException = new InsufficientMemoryException(
				"Insufficient winsock resources available to complete socket connection initiation.",
				new SocketException(10055));

			var mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };

			var reciever = mockRepository.Create<IEHub2Reciever>();
			reciever.Setup(_ => _.SendMessage(It.IsAny<eHub2GatewayMessage>())).Throws(insufficientMemoryException);
			var endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof (IEHub2Reciever)))
			{
				Address = new EndpointAddress(new Uri("net.tcp://localhost:11809/eHub2Gateway/eHub2Gateway.svc"))
			};
			var channelFactory = mockRepository.Create<ChannelFactory<IEHub2Reciever>>(endpoint);
			channelFactory.Setup(_ => _.CreateChannel(It.IsAny<EndpointAddress>(), It.IsAny<Uri>())).Returns(reciever.Object);
			channelFactory.As<IDisposable>().Setup(c => c.Dispose());
			var handler = mockRepository.Create<eHub2MessageHandler>();
			handler.Setup(_ => _.NewChannelFactory()).Returns(channelFactory.Object);

			try
			{
				handler.Object.Handle("TSTCLIENT", Guid.NewGuid(), CreateTestMessage());
				Assert.Fail("SystemException is expected.");
			}
			catch (SystemUnderMaintananceException e)
			{
				Assert.AreEqual(insufficientMemoryException, e.InnerException);
			}

			mockRepository.VerifyAll();
		}

		[TestMethod]
		public void TestSendMessageFaultExceptionsAreRethrown()
		{
			var faultException = new FaultException();

			var mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };

			var reciever = mockRepository.Create<IEHub2Reciever>();
			reciever.Setup(_ => _.SendMessage(It.IsAny<eHub2GatewayMessage>())).Throws(faultException);
			var endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IEHub2Reciever)))
			{
				Address = new EndpointAddress(new Uri("net.tcp://localhost:11809/eHub2Gateway/eHub2Gateway.svc"))
			};
			var channelFactory = mockRepository.Create<ChannelFactory<IEHub2Reciever>>(endpoint);
			channelFactory.Setup(_ => _.CreateChannel(It.IsAny<EndpointAddress>(), It.IsAny<Uri>())).Returns(reciever.Object);
			channelFactory.As<IDisposable>().Setup(c => c.Dispose());
			var handler = mockRepository.Create<eHub2MessageHandler>();
			handler.Setup(_ => _.NewChannelFactory()).Returns(channelFactory.Object);

			try
			{
				handler.Object.Handle("TSTCLIENT", Guid.NewGuid(), CreateTestMessage());
				Assert.Fail("FaultException is expected.");
			}
			catch (FaultException e)
			{
				Assert.AreEqual(faultException, e);
			}

			mockRepository.VerifyAll();
		}

		[TestMethod]
		public void TestSendMessage_FaultExceptionWithHRESULT0xC0C0163CFromBizTalkAdapterWcf_RethrownAsSystemUnderMaintanance()
		{
			var faultExceptionHRESULT0xC0C0163C = new FaultException(@"Exception from HRESULT: 0xC0C0163C (Fault Detail is equal to An ExceptionDetail, likely created by IncludeExceptionDetailInFaults=true, whose value is:
System.Runtime.InteropServices.COMException: Exception from HRESULT: 0xC0C0163C
   at Microsoft.BizTalk.Adapter.Wcf.Runtime.BizTalkAsyncResult.End()
   at Microsoft.BizTalk.Adapter.Wcf.Runtime.BizTalkServiceInstance.EndOperation(IAsyncResult result)
   at Microsoft.BizTalk.Adapter.Wcf.Runtime.BizTalkServiceInstance.Microsoft.BizTalk.Adapter.Wcf.Runtime.ITwoWayAsyncVoid.EndTwoWayMethod(IAsyncResult result)
   at AsyncInvokeEndEndTwoWayMethod(Object , Object[] , IAsyncResult )
   at System.ServiceModel.Dispatcher.AsyncMethodInvoker.InvokeEnd(Object instance, Object[]& outputs, IAsyncResult result)
   at System.ServiceModel.Dispatcher.DispatchOperationRuntime.InvokeEnd(MessageRpc& rpc)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage7(MessageRpc& rpc)
   at System.ServiceModel.Dispatcher.MessageRpc.Process(Boolean isOperationContextSet)).");

			var mockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };
			var reciever = mockRepository.Create<IEHub2Reciever>();
			reciever.Setup(_ => _.SendMessage(It.IsAny<eHub2GatewayMessage>())).Throws(faultExceptionHRESULT0xC0C0163C);
			var endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IEHub2Reciever)))
			{
				Address = new EndpointAddress(new Uri("net.tcp://localhost:11809/eHub2Gateway/eHub2Gateway.svc"))
			};
			var channelFactory = mockRepository.Create<ChannelFactory<IEHub2Reciever>>(endpoint);
			channelFactory.Setup(_ => _.CreateChannel(It.IsAny<EndpointAddress>(), It.IsAny<Uri>())).Returns(reciever.Object);
			channelFactory.As<IDisposable>().Setup(c => c.Dispose());
			var handler = mockRepository.Create<eHub2MessageHandler>();
			handler.Setup(_ => _.NewChannelFactory()).Returns(channelFactory.Object);

			try
			{
				handler.Object.Handle("TSTCLIENT", Guid.NewGuid(), CreateTestMessage());
				Assert.Fail("SystemException is expected.");
			}
			catch (SystemUnderMaintananceException e)
			{
				Assert.AreEqual(faultExceptionHRESULT0xC0C0163C, e.InnerException);
			}

			mockRepository.VerifyAll();
		}

		static eHubGatewayMessage CreateTestMessage()
		{
			return new eHubGatewayMessage
			{
				ApplicationCode = "TST",
				ClientID = "TSTCLIENT",
				EmailSubject = "EMAIL SUBJECT",
				FileName = "FILE.NAME",
				MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE STREAM")).CompressAndEncode()
			};
		}
	}
}
