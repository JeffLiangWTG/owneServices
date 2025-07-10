using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.HK.Business.Testing
{
	[TestedType(typeof(TraxonConsolStatus))]
	class TraxonConsolStatusTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return new DeclarationsCreatedCancelledBusinessObjectFactory();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return testConsolStatus;
		}

		public void TestTraxonMessageCollection()
		{
			AssertNotNull("fMessages is not null", testConsolStatus.OrderedMessages);
		}

		public void TestFreightConsol()
		{
			AssertNotNull("Consol has been set correctly", testConsolStatus.Consol);
		}

		public void TestTraxon_MessageStatusIsNoMessagesSent()
		{
			responseMessage.Delete();
			sentMessage.Delete();
			AssertEquals("That Traxon_MessageStatus has no message sent", TraxonConsolStatus.NoMessagesSent, testConsolStatus.Traxon_MessageStatus);
		}

		public void TestTraxon_MessageStatusIsAwaitingResponse()
		{
			responseMessage.Delete();
			AssertEquals("That Traxon_MessageStatus has no message sent", TraxonConsolStatus.AwaitingResponse, testConsolStatus.Traxon_MessageStatus);
		}

		public void TestTraxon_MessageStatusAcknowledgeIsOutOfTheMessageIEM()
		{
			responseMessage.EM_MessageText = TestIEMFMAMessageResponse;
			AssertEquals("The Status Message says that everything is fine with the shipment", "Acknowledged : AWB 000-00022911 UPDATED AT 1600 15JAN1995. REPLACE:2, TOTAL:2.", testConsolStatus.Traxon_MessageStatus);
		}

		public void TestTraxon_MessageStatusAcknowledgeIsOutOfTheMessageCIM()
		{
			responseMessage.EM_MessageText = TestCIMFMAMessageResponse;
			AssertEquals("The Status Message says that everything is fine with the shipment", "Acknowledged : AWB 020-65565006 UPDATED ON 20040403", testConsolStatus.Traxon_MessageStatus);
		}

		public void TestTraxon_MessageStatusErrorIsOutOfTheMessageIEM()
		{
			responseMessage.EM_MessageText = TestIEMFNAMessageResponse;
			AssertEquals("The Status Message says that everything is fine with the shipment", "Error : DUPLICATE HWB NO", testConsolStatus.Traxon_MessageStatus);
		}

		public void TestTraxon_MessageStatusErrorIsOutOfTheMessageCIM()
		{
			responseMessage.EM_MessageText = TestCIMFNAMessageResponse;
			AssertEquals("The Status Message says that everything is fine with the shipment", "Error : ERROR IN 618-57218965,HWB/FLIGHT HAS BEEN FROZEN CIMXHL", testConsolStatus.Traxon_MessageStatus);
		}

		public void TestTraxon_MessageStatusInfoAndValidate()
		{
			AssertEquals("Assert that there are no notifications on the info", false, testConsolStatus.Traxon_MessageStatusInfo.HasErrors());
			testConsolStatus.Traxon_MessageStatusInfo.AddError("This is a test error");
			AssertEquals("Pre-Condition, The Traxon_MessageStatusInfo has errors", true, testConsolStatus.Traxon_MessageStatusInfo.HasErrors());
			testConsolStatus.ValidateTraxon_MessageStatus();
			AssertEquals("Validate clears that errors", false, testConsolStatus.Traxon_MessageStatusInfo.HasErrors());
		}

		#region Implementation

		protected EDIMessage responseMessage;
		protected EDIMessage sentMessage;
		protected TraxonConsolStatus testConsolStatus;

		protected override void SetUp()
		{
			base.SetUp();

			ForwardingConsol testConsol = Factory.New<ForwardingConsol>();
			testConsolStatus = new TraxonConsolStatus(testConsol);

			sentMessage = Factory.New<EDIMessage>();
			sentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_LinkTable = ForwardingConsol.Schema.TableName;
			sentMessage.EM_LinkUniqueID = testConsol.PK;

			responseMessage = Factory.New<EDIMessage>();
			responseMessage.EM_IsActive = ZBool.True;
			responseMessage.EM_IsTestMessage = ZBool.False;
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
			responseMessage.EM_MessageType = EDIMessage.ApplicationCodes.Traxon;
			responseMessage.EM_MessageSubType = EDIMessage.ApplicationCodes.Traxon;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "2";
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_LinkTable = ForwardingConsol.Schema.TableName;
			responseMessage.EM_LinkUniqueID = testConsol.PK;
		}

		#endregion

		#region TestMessages

		protected const string TestIEMFMAMessageResponse = @"UNH+2+IEMFMA:D:95A:IA:IATA01'BGM'UCM+1+CUSEXP:D:95A:UN'FTX+Z03+++AWB 000-00022911 UPDATED AT 1600 15JAN1995. REPLACE?:2, TOTAL?:2.'ERC+1'UNT+6+2'";
		protected const string TestIEMFNAMessageResponse = @"UNH+3+IEMFNA:D:95A:IA:IATA01'BGM'UCM+10+CUSEXP:D:95A:UN'FTX+AA0+++DUPLICATE HWB NO'ERC+1'UNT+6+3'";
		protected const string TestCIMFMAMessageResponse = @"UNH+119812+CIMFMA:0+HMF6556500X020'FMAACK/020-65565006 UPDATED ON 20040403:0932/1 HWB UPDATED CIMXHL'UNT+3+119812'";
		protected const string TestCIMFNAMessageResponse = @"UNH+119813+CIMFNA:0+HMF5721896X618'FNAACK/ERROR IN 618-57218965,HWB/FLIGHT HAS BEEN FROZEN CIMXHL'UNT+3+119813'";

		#endregion

	}
}
