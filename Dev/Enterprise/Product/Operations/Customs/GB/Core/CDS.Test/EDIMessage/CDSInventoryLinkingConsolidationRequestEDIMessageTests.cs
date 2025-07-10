using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingConsolidationRequestEDIMessage))]
	public class CDSInventoryLinkingConsolidationRequestEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingConsolidationRequestEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObjecttAndInterpretation()
		{
			var message = Factory.New<CDSInventoryLinkingConsolidationRequestEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingConsolidationRequestEDIMessage.Serialize(new inventoryLinkingConsolidationRequest
			{
				masterUCR = "MUCR",
				messageCode = messageCodeConsolidation.EAC,
				ucrBlock = new ucrBlock
				{
					ucr = "DUCR",
					ucrType = ucrType.M
				}
			});
			var messasgeDataObject = message.MessageDataObject;
			AssertEquals("MUCR", messasgeDataObject.masterUCR);
			AssertEquals(messageCodeConsolidation.EAC, messasgeDataObject.messageCode);
			AssertEquals("DUCR", messasgeDataObject.ucrBlock.ucr);
			AssertEquals(ucrType.M, messasgeDataObject.ucrBlock.ucrType);
		}

		internal static ZString ExpectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Consolidation Request to CDS:</H3><p><strong>Close consol : </strong>MUCR</p>";
	}
}
