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
    public class LicenceCheckInboxMessageHandlerTests : LicenceCheckMessageHandlerTests
	{
		private const string ClientIDSG = "SGCustoms";
		private const string ClientIDSGTest = "SGCustomsTest";

        private const string ClientIDCACustoms = "CACustoms";
        private const string ClientIDCACustomsTest = "CACustomsTest";

        private const string ClientIDUSAirAMS = "USAirAMS";
        private const string ClientIDUSAirAMSTest = "USAirAMSTest";

        private const string ClientIDZACustoms = "ZACustoms";
        private const string ClientIDZACustomsDocSuffix = "Doc";
        private const string ClientIDZACustomsDoc = "ZACustomsDoc";
        private const string ClientIDZACustomsDocTest = "ZACustomsDocTest";

		private const string ClientIDNEXDOCS = "NEXDOCS";
		private const string ClientIDNEXDOCSTest = "NEXDOCSTest";

	    private const string ClientIDGBCustoms = "GBCustoms";
        private const string ClientIDGBCustomsTest = "GBCustomsTest";

		private const string ClientIDGEI = "GLB_ELEC_INVOICING";
		private const string ClientIDGEITest = "GLB_ELEC_INVOICINGTest";

		private const string ClientIDUSCustomsEBond = "USCustomsEBond";
		private const string ClientIDUSCustomsEBondTest = "USCustomsEBondTest";

        private const string ClientIDUSDIS = "USDIS";
        private const string ClientIDUSDISTest = "USDISTest";

        private const string SenderID = "SenderID";

		[TestInitialize]
		public void Initialize()
		{
            string clientIdSuffix;
            switch (TestContext.TestName)
            {
                case "TestLicenceCheckInboxMessageHandler_ZADoc_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID":
                case "TestLicenceCheckInboxMessageHandler_ZADoc_SenderHasProductionLicense_MessageClientIDUnchanged":
                    clientIdSuffix = "Doc";
                    break;
                default:
                    clientIdSuffix = string.Empty;
                    break;
            }
            Initialize(clientIdSuffix);
		}

        private void TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(eHubGatewayMessage message)
        {
            Assert.IsTrue(MessageHandlerFactory.CreateMessageHandler(message) is LicenceCheckInboxMessageHandler);
        }

		[TestMethod]
		public void TestNoLicenceCheckInboxMessageHandler_CA_MessageInsertedIntoInbox_Succeeded()
		{
            TestLicenceCheckInboxOutboxMessageHandler_MonitoringSender_MessageClientIDUnchanged("T_____CAC", ClientIDCACustoms, CreateCAMessage());
		}

		#region USCustomsEBond

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_UXB_MessageHandlerFactory()
		{
			TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateUSCustomsEBondMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_UXB_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
		{
			TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDUSCustomsEBondTest, CreateUSCustomsEBondMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_UXB_SenderHasProductionLicense_MessageClientIDUnchanged()
		{
			TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDUSCustomsEBond, CreateUSCustomsEBondMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_UXB_SenderHasInvalidLicense_ExceptionThrown()
		{
			TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDUSCustomsEBond, CreateUSCustomsEBondMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_UXB_MessageInsertedIntoInbox_Succeeded()
		{
			TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDUSCustomsEBond, string.Empty, CreateUSCustomsEBondMessage());
		}
		
		eHubGatewayMessage CreateUSCustomsEBondMessage()
		{
			return new eHubGatewayMessage
			{
				ClientID = ClientIDUSCustomsEBond,
				ApplicationCode = "UXB"
			};
		}

		#endregion

		#region CA Customs

		private eHubGatewayMessage CreateCAMessage()
        {
            return new eHubGatewayMessage
            {
                ClientID = ClientIDCACustoms,
                ApplicationCode = "UDM"
            };
        }

        [TestMethod]
		public void TestLicenceCheckInboxMessageHandler_CA_MessageHandlerFactory()
		{
            TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateCAMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_CA_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
		{
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDCACustomsTest, CreateCAMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_CA_SenderHasProductionLicense_MessageClientIDUnchanged()
		{
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDCACustoms, CreateCAMessage());
		}

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_CA_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDCACustoms, CreateCAMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_CA_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDCACustoms, string.Empty, CreateCAMessage());
        }

		#endregion

		#region SG Customs

		private eHubGatewayMessage CreateSGMessage()
        {
            return new eHubGatewayMessage
            {
                ClientID = ClientIDSG
            };
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_SG_MessageHandlerFactory()
        {
            TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateSGMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_SG_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDSGTest, CreateSGMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_SG_SenderHasProductionLicense_MessageClientIDUnchanged()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDSG, CreateSGMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_SG_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDSG, CreateSGMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_SG_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDSG, string.Empty, CreateSGMessage());
        }

        #endregion

        #region US Air AMS

        private eHubGatewayMessage CreateUSAirAMSMessage()
        {
            return new eHubGatewayMessage
            {
                ClientID = ClientIDUSAirAMS,
                MessageStream = new MemoryStream(Encoding.UTF8.GetBytes("Message Stream"))
            };
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USAirAMS_MessageHandlerFactory()
        {
            TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateUSAirAMSMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USAirAMS_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDUSAirAMSTest, CreateUSAirAMSMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USAirAMS_SenderHasProductionLicense_MessageClientIDUnchanged()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDUSAirAMS, CreateUSAirAMSMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USAirAMS_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDUSAirAMS, CreateUSAirAMSMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USAirAMS_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDUSAirAMS, string.Empty, CreateUSAirAMSMessage());
        }

        #endregion

        #region ZA Customs Doc

        private eHubGatewayMessage CreateZACustomsDocMessage()
        {
            return new eHubGatewayMessage
            {
                ApplicationCode = "UDM",
				ClientID = ClientIDZACustoms,
				SchemaName = "ZACustoms",
				SchemaType = MessageSchemaType.Xml
            };
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_ZADoc_MessageHandlerFactory()
        {
            TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateZACustomsDocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_ZADoc_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDZACustomsDocTest, CreateZACustomsDocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_ZADoc_SenderHasProductionLicense_MessageClientIDUnchanged()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDZACustomsDoc, CreateZACustomsDocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_ZADoc_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDZACustoms, CreateZACustomsDocMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_ZADoc_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDZACustoms, ClientIDZACustomsDocSuffix, CreateZACustomsDocMessage());
        }

        #endregion

		#region AU Customs - NEXDOCS

		private eHubGatewayMessage CreateNEXDOCSMessage()
		{
			return new eHubGatewayMessage
			{
				ClientID = ClientIDNEXDOCS
			};
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_NEXDOCS_MessageHandlerFactory()
		{
            TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateNEXDOCSMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_NEXDOCS_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
		{
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDNEXDOCSTest, CreateNEXDOCSMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_NEXDOCS_SenderHasProductionLicense_MessageClientIDUnchanged()
		{
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDNEXDOCS, CreateNEXDOCSMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_NEXDOCS_SenderHasInvalidLicense_ExceptionThrown()
		{
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDNEXDOCS, CreateNEXDOCSMessage());
		}

		[TestMethod]
		public void TestLicenceCheckInboxMessageHandler_NEXDOCS_MessageInsertedIntoInbox_Succeeded()
		{
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDNEXDOCS, string.Empty, CreateNEXDOCSMessage());
		}

		#endregion

	    #region GB Customs

	    private eHubGatewayMessage CreateGBCustomsMessage()
	    {
	        return new eHubGatewayMessage
	        {
	            ClientID = ClientIDGBCustoms
	        };
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_GBCustoms_MessageHandlerFactory()
	    {
	        TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateGBCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_GBCustoms_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
	    {
	        TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDGBCustomsTest, CreateGBCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_GBCustoms_SenderHasProductionLicense_MessageClientIDUnchanged()
	    {
	        TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDGBCustoms, CreateGBCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_GBCustoms_SenderHasInvalidLicense_ExceptionThrown()
	    {
	        TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDGBCustoms, CreateGBCustomsMessage());
	    }

	    [TestMethod]
	    public void TestLicenceCheckInboxMessageHandler_GBCustoms_MessageInsertedIntoInbox_Succeeded()
	    {
	        TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDGBCustoms, string.Empty, CreateGBCustomsMessage());
	    }

	    #endregion

        #region Global Invoice
        private eHubGatewayMessage CreateGEIMessage()
        {
            return new eHubGatewayMessage
            {
                ClientID = ClientIDGEI
            };
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_GEI_MessageHandlerFactory()
        {
            TestLicenceCheckInboxMessageHandler_MessageHandlerFactory(CreateGEIMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_GEI_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDGEITest, CreateGEIMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_GEI_SenderHasProductionLicense_MessageClientIDUnchanged()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDGEI, CreateGEIMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_GEI_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDGEI, CreateGEIMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_GEI_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDGEI, string.Empty, CreateGEIMessage());
        }
        #endregion

        #region
        private eHubGatewayMessage CreateUSDISMessage()
        {
            return new eHubGatewayMessage
            {
                ApplicationCode = "USD",
                ClientID = "USDIS"
            };
        }

        public void TestLicenceCheckInboxMessageHandler_USDIS_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasNoProductionLicense_TestSuffixAddedToMessageClientID(SenderID, ClientIDUSDISTest, CreateUSDISMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USDIS_SenderHasProductionLicense_MessageClientIDUnchanged()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasProductionLicense_MessageClientIDUnchanged(SenderID, ClientIDUSDIS, CreateUSDISMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USDIS_SenderHasInvalidLicense_ExceptionThrown()
        {
            TestLicenceCheckInboxOutboxMessageHandler_SenderHasInvalidLicense_ExceptionThrown(SenderID, ClientIDUSDIS, CreateUSDISMessage());
        }

        [TestMethod]
        public void TestLicenceCheckInboxMessageHandler_USDIS_MessageInsertedIntoInbox_Succeeded()
        {
            TestLicenceCheckInboxOutboxMessageHandler_MessageInsertedIntoInbox_Succeeded(SenderID, ClientIDUSDIS, string.Empty, CreateUSDISMessage());
        }
        #endregion
    }
}
