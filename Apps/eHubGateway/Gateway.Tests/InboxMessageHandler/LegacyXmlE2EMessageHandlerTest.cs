using System;
using System.IO;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class LegacyXmlE2EMessageHandlerTest
	{
		[TestMethod]
		public void TestLegacyXmlE2EMessageHandler_GetHandler()
		{
			var legacyMessage = new eHubGatewayMessage
				{
					ApplicationCode = "XMS",
					ClientID = "CLIENT",
					SchemaName = "http://www.edi.com.au/EnterpriseService"
				};

			string senderID = "SENDER";
			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsCW1System(senderID)).Return(true);
			mockPartyAccessor.Stub(x => x.IsCW1System(legacyMessage.ClientID)).Return(true);

			MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
			var handler = MessageHandlerFactory.CreateMessageHandler(senderID, legacyMessage);
			Assert.IsInstanceOfType(handler, typeof(LegacyXmlE2EMessageHandler));
			Assert.IsInstanceOfType(handler, typeof(DefaultInboxMessageHandler));
		}

		[TestMethod]
		public void TestLegacyXmlE2EMessageHandler_Fail()
		{
			string senderID = "SENDER";
			Guid envelopeID = Guid.NewGuid();
			var message = new eHubGatewayMessage();
			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			mockPartyAccessor.Stub(x => x.IsLegacyXmlAllowedClient(senderID)).Return(false);
			mockPartyAccessor.Stub(x => x.IsLegacyXmlAllowedClient(message.ClientID)).Return(false);

			var handler = MockRepository.GeneratePartialMock<LegacyXmlE2EMessageHandler>();
			handler.Stub(x => x.PartyAccessor()).Return(mockPartyAccessor);

			try
			{
				handler.Handle(senderID, envelopeID, message);
				Assert.Fail("E2E does not support Legacy messages.");
			}
			catch (InvalidDataException e)
			{
				Assert.AreEqual(
					@"You are using E2E with the superseded legacy application Type, XMS that is no longer supported.
Please change your E2E settings as per WiseLearning documents 'How To Set up E2E'.
The receiving CW1 system needs to also be adjusted as per the documents 'How To Set up E2E'",
					e.Message);
			}

			mockPartyAccessor.AssertWasCalled(x => x.IsLegacyXmlAllowedClient(senderID));
			mockPartyAccessor.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestLegacyXmlE2EMessageHandler_Handle()
		{
			string senderID = "SENDER";
			Guid envelopeID = Guid.NewGuid();
			var message = new eHubGatewayMessage
			{
				ApplicationCode = "XMS",
				ClientID = "CLIENT",
				SchemaName = "http://www.edi.com.au/EnterpriseService"
			};

			var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			var mockInboxAccessor = MockRepository.GenerateMock<IInboxAccessor>();
			mockPartyAccessor.Stub(x => x.IsLegacyXmlAllowedClient(senderID)).Return(true);
			mockPartyAccessor.Stub(x => x.IsLegacyXmlAllowedClient(message.ClientID)).Return(true);

			var handler = MockRepository.GeneratePartialMock<LegacyXmlE2EMessageHandler>();
			handler.Stub(x => x.NewInboxAccessor).Return(mockInboxAccessor);
			handler.Stub(x => x.PartyAccessor()).Return(mockPartyAccessor);

			handler.Handle(senderID, envelopeID, message);

			mockInboxAccessor.AssertWasCalled(x => x.InsertToInbox(senderID, envelopeID, message, true));
			mockPartyAccessor.VerifyAllExpectations();
			mockInboxAccessor.VerifyAllExpectations();
		}
	}
}
