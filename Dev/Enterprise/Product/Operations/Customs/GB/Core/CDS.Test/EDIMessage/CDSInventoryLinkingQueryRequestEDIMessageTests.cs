using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingQueryRequestEDIMessage))]
	public class CDSInventoryLinkingQueryRequestEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingQueryRequestEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObjectAndInterpretation()
		{
			var message = Factory.New<CDSInventoryLinkingQueryRequestEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingQueryRequestEDIMessage.Serialize(new inventoryLinkingQueryRequest
			{
				queryUCR = new ucrBlock
				{
					ucr = "DUCR",
					ucrType = ucrType.M
				}
			});
			var messasgeDataObject = message.MessageDataObject;
			AssertEquals("DUCR", messasgeDataObject.queryUCR.ucr);
			AssertEquals(ucrType.M, messasgeDataObject.queryUCR.ucrType);
			AssertEquals(ExpectedHTMLInterpretation, message.EM_MessageInterpretation);
		}

		internal static ZString ExpectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Query Request to CDS:</H3><p><strong>UCR: </strong>DUCR<br><strong>UCR Type: </strong>M</p>";
	}
}
