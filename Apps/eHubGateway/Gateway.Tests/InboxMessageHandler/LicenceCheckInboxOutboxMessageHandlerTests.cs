using System;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.IO;
using System.Text;
using CargoWise.eHub.Integration;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
    [TestClass]
    public class LicenceCheckInboxOutboxMessageHandlerTests : LicenceCheckMessageHandlerTests
    {
        private const string ClientIDCACustomsDoc = "CACustomsDoc";
        private const string ClientIDCACustomsDocTest = "CACustomsDocTest";

	    private const string ClientIDITCustoms = "ITCustoms";
	    private const string ClientIDITCustomsTest = "ITCustomsTest";

		private const string LicenseTypeProduction = "PRD";
        private const string LicenseTypeTest = "TST";
        private const string LicenseTypeInvalid = "";

        private const string SenderID = "SenderID";

        [TestInitialize]
        public void Initialize()
        {
            var clientIdSuffix = string.Empty;
            Initialize(clientIdSuffix);
        }

       #region CA Customs Doc

        private eHubGatewayMessage CreateCADocMessage()
        {
            return new eHubGatewayMessage
            {
                ClientID = ClientIDCACustomsDoc,
                ApplicationCode = "UDM"
            };
        }

        private void TestLicenceCheckInboxOutboxMessageHandler_MessageHandlerFactory(eHubGatewayMessage message)
        {
            Assert.IsTrue(MessageHandlerFactory.CreateMessageHandler(message) is LicenceCheckInboxOutboxMessageHandler);
        }

        [TestMethod]
        public void TestLicenceCheckInboxOutboxMessageHandler_CADoc_MessageHandlerFactory()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageHandlerFactory(CreateCADocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxOutboxMessageHandler_CADoc_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDCACustomsDocTest, CreateCADocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxOutboxMessageHandler_CADoc_SenderHasProductionLicense_MessageClientIDUnchanged()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDCACustomsDoc, CreateCADocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxOutboxMessageHandler_CADoc_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDCACustomsDoc, CreateCADocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxOutboxMessageHandler_CADoc_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDCACustomsDoc, string.Empty, CreateCADocMessage());
        }

		#endregion


		#region IT Customs

	    private eHubGatewayMessage CreateITCustomsMessage()
	    {
		    return new eHubGatewayMessage
		    {
			    ApplicationCode = "ITC",
			    ClientID = ClientIDITCustoms,
			    SchemaName = "ITCustoms",
			    SchemaType = MessageSchemaType.Xml
		    };
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_ITCustoms_MessageHandlerFactory()
	    {
		    TestLicenceCheckInboxOutboxMessageHandler_MessageHandlerFactory(CreateITCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_ITCustoms_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
	    {
		    TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDITCustomsTest, CreateITCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_ITCustoms_SenderHasProductionLicense_MessageClientIDUnchanged()
	    {
		    TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDITCustoms, CreateITCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_ITCustoms_SenderHasInvalidLicense_ExceptionThrown()
	    {
		    TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDITCustoms, CreateITCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_ITCustoms_MessageInsertedIntoInbox_Succeeded()
	    {
		    TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDITCustoms, string.Empty, CreateITCustomsMessage());
	    }

        #endregion

        #region HKCustoms

        private const string ClientIDHKCustoms = "HKCustoms";
        private const string ClientIDHKCustomsTest = "HKCustomsTest";

        private eHubGatewayMessage CreateHKCustomsMessage()
        {
            return new eHubGatewayMessage
            {
                ApplicationCode = ApplicationCode.HKCustoms,
                ClientID = ClientIDHKCustoms,
                SchemaName = "HKCustoms",
                SchemaType = MessageSchemaType.Xml
            };
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_HKCustoms_MessageHandlerFactory()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageHandlerFactory(CreateHKCustomsMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_HKCustoms_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDHKCustomsTest, CreateHKCustomsMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_HKCustoms_SenderHasProductionLicense_MessageClientIDUnchanged()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDHKCustoms, CreateHKCustomsMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_HKCustoms_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDHKCustoms, CreateHKCustomsMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_HKCustoms_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDHKCustoms, string.Empty, CreateHKCustomsMessage());
        }

        #endregion

    }
}
