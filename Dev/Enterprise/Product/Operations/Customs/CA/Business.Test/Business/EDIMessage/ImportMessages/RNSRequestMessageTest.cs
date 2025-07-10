using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(RNSRequestMessage))]
	sealed class RNSRequestMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals(EDIMessage.ApplicationCodes.CAIMP, message.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.RNSRequest, message.EM_MessageType);
			message.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			AssertEquals(RNSMessageTypes.Descriptions.ArrivalCertification, message.EM_MessageSubTypeDescription);
		}

		public void TestMessageNumberFilledIn()
		{
			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAIMP).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = " + number, message.EM_MessageText);
		}

		public void TestGetRecentArrivalCertificationMessage()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "1";

			RNSRequestMessage message1 = Factory.New<RNSRequestMessage>();
			message1.EM_LinkedObject = shipment;
			message1.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2014, 11, 19);
			message1.EM_EI = interchange.PK;
			message1.EM_MessageNum = "2";

			RNSRequestMessage message2 = Factory.New<RNSRequestMessage>();
			message2.EM_LinkedObject = shipment;
			message2.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2014, 11, 18);
			message2.EM_EI = interchange.PK;
			message2.EM_MessageNum = "1";

			RNSRequestMessage message3 = Factory.New<RNSRequestMessage>();
			message3.EM_LinkedObject = shipment;
			message3.EM_MessageSubType = RNSMessageTypes.Codes.StatusQuery;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2014, 11, 19);

			RNSRequestMessage message4 = Factory.New<RNSRequestMessage>();
			message4.EM_LinkedObject = shipment;
			message4.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_SystemCreateTimeUtc = new ZDateTime(2014, 11, 19);

			EDIReleaseMessage message5 = Factory.New<EDIReleaseMessage>();
			message5.EM_LinkedObject = shipment;
			message5.EM_SystemCreateTimeUtc = new ZDateTime(2014, 11, 19);

			Factory.Save();

			message1.EM_MessageNum = "2";
			message2.EM_MessageNum = "1";
			Factory.Save();

			EDIMessageCollection messages = new EDIMessageCollection(shipment);
			messages.Load();

			RNSRequestMessage rnsRequestMessage = RNSRequestMessage.GetRecentArrivalCertificationMessage(messages);

			AssertEquals("The recent Arrival Certification Message should be message1", message1.PK, rnsRequestMessage.PK);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<RNSRequestMessage>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (RNSRequestMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		EDIMessage message;

		#endregion
	}
}
