using System;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class ZACustomsMessageHandlerTests
	{
		[TestMethod]
		public void ZACustomsMessageHandler_GetHandler()
		{
			var zacMsg = new eHubGatewayMessage
			{
				ApplicationCode = "ZAC",
				ClientID = "ZACustoms",
				SchemaName = "ZACustoms",
				SchemaType = MessageSchemaType.FlatFile
			};

			var zacHandler = MessageHandlerFactory.CreateMessageHandler(zacMsg);

			Assert.IsInstanceOfType(zacHandler, typeof(ZACustomsMessageHandler));
		}

		[TestMethod]
		public void ZACustomsMessageHandler_Handle()
		{
			string senderID = "SENDER";
			Guid envelopeID = Guid.NewGuid();
			var message1 = new eHubGatewayMessage { ClientID = "ZACustoms" };
			var message2 = new eHubGatewayMessage { ClientID = "ZACustoms" };

			var stubInboxAccessor = MockRepository.GenerateStub<IInboxAccessor>();
			var stubEnterpriseExeDetailAccessor = MockRepository.GenerateStub<IEnterpriseExeDetailAccessor>();
			var handler = MockRepository.GeneratePartialMock<ZACustomsMessageHandler>();
			handler.Stub(x => x.GetInboxAccessor()).Return(stubInboxAccessor);
			handler.Stub(x => x.NewEnterpriseExeDetailAccessor).Return(stubEnterpriseExeDetailAccessor);
			stubEnterpriseExeDetailAccessor.Stub(x => x.GetLicenceType(senderID)).Return("PRD").Repeat.Once();
			stubEnterpriseExeDetailAccessor.Stub(x => x.GetLicenceType(senderID)).Return("TST").Repeat.Once();

			handler.Handle(senderID, envelopeID, message1);
			handler.Handle(senderID, envelopeID, message2);

			stubInboxAccessor.AssertWasCalled(x => x.InsertToInboxAndOutbox(senderID, envelopeID, message1));
			Assert.AreEqual("ZACustoms", message1.ClientID);
			stubInboxAccessor.AssertWasCalled(x => x.InsertToInboxAndOutbox(senderID, envelopeID, message2));
			Assert.AreEqual("ZACustomsTest", message2.ClientID);
		}

		[TestMethod]
		public void ZACustomsMessageHandler_MonitoringSender_Handle()
		{
            string senderID = "T_____ZAC";
			Guid envelopeID = Guid.NewGuid();
			var message1 = new eHubGatewayMessage { ClientID = "ZACustoms" };

			var stubInboxAccessor = MockRepository.GenerateStub<IInboxAccessor>();
			var stubEnterpriseExeDetailAccessor = MockRepository.GenerateStub<IEnterpriseExeDetailAccessor>();
			var handler = MockRepository.GeneratePartialMock<ZACustomsMessageHandler>();
			handler.Stub(x => x.GetInboxAccessor()).Return(stubInboxAccessor);
			handler.Stub(x => x.NewEnterpriseExeDetailAccessor).Return(stubEnterpriseExeDetailAccessor);

			handler.Handle(senderID, envelopeID, message1);

			stubInboxAccessor.AssertWasCalled(x => x.InsertToInboxAndOutbox(senderID, envelopeID, message1));
			Assert.AreEqual("ZACustoms", message1.ClientID);
			stubEnterpriseExeDetailAccessor.AssertWasNotCalled((x => x.GetLicenceType(senderID)));
			stubEnterpriseExeDetailAccessor.AssertWasNotCalled(x => x.GetLicenceType(senderID));
		}
	}
}
