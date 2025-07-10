using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	sealed class PBNMessageSenderTest : TestCaseWithFactory
	{
		public void TestSend_CPB()
		{
			AssertSend(PBNMessageTypes.Codes.CreatePBN, true);
		}

		public void TestSend_UPB()
		{
			AssertSend(PBNMessageTypes.Codes.UpdatePBN, true); 
		}

		public void TestSend_LPB()
		{
			AssertSend(PBNMessageTypes.Codes.LookupPBN, false);
		}

		public void TestSend_LPC()
		{
			AssertSend(PBNMessageTypes.Codes.LookupPBNChannel, false);
		}

		public void TestSend_UPD()
		{
			AssertSend(PBNMessageTypes.Codes.UpdatePBNDeclarations, true); 
		}

		void AssertSend(string messageType, bool expectedMessageText)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			manifestHeader.CustomsReferenceCollection.AddNew().CSI_ReferenceNumber = "25IEROS124";

			var sendingAction = new PBNMessageSendingObject(manifestHeader);
			sendingAction.MessageType = messageType;

			var sender = new PBNMessageSender(sendingAction);
			sender.Send();
			Factory.Save();
			var createdMessage = (PBNOutboundEDIMessage)manifestHeader.Messages.Single();
			AssertNotNull("PBNOutboundEDIMessage", createdMessage);
			if (expectedMessageText)
			{
				AssertNotEquals("EM_MessageText", ZString.Empty, createdMessage.EM_MessageText);
			}
			else
			{
				AssertEquals("EM_MessageText", ZString.Empty, createdMessage.EM_MessageText);
			}
		}
	}
}
