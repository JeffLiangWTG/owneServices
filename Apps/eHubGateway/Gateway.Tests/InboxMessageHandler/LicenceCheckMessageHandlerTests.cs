using System;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.IO;
using System.Text;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
    [TestClass]
    public class LicenceCheckMessageHandlerTests
    {
        private const string LicenseTypeProduction = "PRD";
		private const string LicenseTypeTest = "TST";
		private const string LicenseTypeInvalid = "";


		private string senderID;
		private Guid envelopeTrackingID;
		private eHubGatewayMessage eHubGatewayMessage;

		private IInboxAccessor inboxAccessorStub;
		private IEnterpriseExeDetailAccessor enterpriseExeDetailAccessorStub;
        private LicenceCheckInboxMessageHandler messageHandlerMock;
        
        public TestContext TestContext { get; set; }		

        #region Generic Stubs

        private void SetupTestData(string senderID, eHubGatewayMessage message)
        {
            this.senderID = senderID;
            envelopeTrackingID = Guid.NewGuid();
            this.eHubGatewayMessage = message;
        }

        protected void Initialize(string clientIdSuffix)
        {
            inboxAccessorStub = MockRepository.GenerateStub<IInboxAccessor>();
            enterpriseExeDetailAccessorStub = MockRepository.GenerateStub<IEnterpriseExeDetailAccessor>();
            messageHandlerMock = MockRepository.GeneratePartialMock<LicenceCheckInboxMessageHandler>(clientIdSuffix);

            messageHandlerMock.Stub(x => x.NewInboxAccessor).Return(inboxAccessorStub);
            messageHandlerMock.Stub(x => x.NewEnterpriseExeDetailAccessor).Return(enterpriseExeDetailAccessorStub);
        }

        protected void TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(string senderID, string clientIDTest, eHubGatewayMessage message)
        {
            SetupTestData(senderID, message);

            enterpriseExeDetailAccessorStub.Stub(x => x.GetLicenceType(senderID)).Return(LicenseTypeTest);
            messageHandlerMock.Handle(senderID, envelopeTrackingID, eHubGatewayMessage);

            Assert.AreEqual(clientIDTest, eHubGatewayMessage.ClientID);
        }

        protected void TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(string senderID, string clientID, eHubGatewayMessage message)
        {
            SetupTestData(senderID, message);

            enterpriseExeDetailAccessorStub.Stub(x => x.GetLicenceType(senderID)).Return(LicenseTypeProduction);
            messageHandlerMock.Handle(senderID, envelopeTrackingID, eHubGatewayMessage);

            Assert.AreEqual(clientID, eHubGatewayMessage.ClientID);
        }

        protected void TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(string senderID, string clientID, eHubGatewayMessage message)
        {
            SetupTestData(senderID, message);

            enterpriseExeDetailAccessorStub.Stub(x => x.GetLicenceType(senderID)).Return(LicenseTypeInvalid);
            AssertException<ArgumentException>(
                () => messageHandlerMock.Handle(senderID, envelopeTrackingID, eHubGatewayMessage),
                "Error retrieving licence details for sender."
            );

            Assert.AreEqual(clientID, eHubGatewayMessage.ClientID);
        }

	    protected void TestLicenceCheckInboxOutboxMessageHandler_MonitoringSender_MessageClientIDUnchanged(string senderID, string clientID, eHubGatewayMessage message)
	    {
		    SetupTestData(senderID, message);

		    enterpriseExeDetailAccessorStub.Stub(x => x.GetLicenceType(senderID)).Return(LicenseTypeInvalid);
			messageHandlerMock.Handle(senderID, envelopeTrackingID, eHubGatewayMessage);

			Assert.AreEqual(clientID, eHubGatewayMessage.ClientID);
	    }

		protected void TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(string senderID, string clientID, string clientIdSuffix, eHubGatewayMessage message)
        {
            SetupTestData(senderID, message);

            var inboxAccessorMock = MockRepository.GenerateStrictMock<IInboxAccessor>();
            inboxAccessorMock.Expect(x => x.InsertToInbox(Arg<string>.Is.Equal(senderID), Arg<Guid>.Is.Equal(envelopeTrackingID), Arg<eHubGatewayMessage>.Is.Equal(eHubGatewayMessage), Arg<Boolean>.Is.Equal(true)));

            var enterpriseAccessorMock = MockRepository.GenerateStrictMock<IEnterpriseExeDetailAccessor>();
            enterpriseAccessorMock.Expect(x => x.GetLicenceType(senderID)).Return(LicenseTypeTest);

            var handlerMock = MockRepository.GeneratePartialMock<LicenceCheckInboxMessageHandler>(clientIdSuffix);
            handlerMock.Expect(x => x.NewInboxAccessor).Return(inboxAccessorMock);
            handlerMock.Expect(x => x.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock);

            handlerMock.Handle(senderID, envelopeTrackingID, eHubGatewayMessage);

            handlerMock.VerifyAllExpectations();
            inboxAccessorMock.VerifyAllExpectations();
            enterpriseAccessorMock.VerifyAllExpectations();
        }

        private static void AssertException<T>(Action action, String message = null) where T : Exception
        {
            try
            {
                action();
            }
            catch (AssertFailedException) { throw; }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(T));
                if (message != null)
                {
                    Assert.AreEqual(message, ex.Message);
                }
                return;
            }

            Assert.Fail("Expected exception of type '{0}' was not thrown.", typeof(T));
        }

        #endregion
    }
}
