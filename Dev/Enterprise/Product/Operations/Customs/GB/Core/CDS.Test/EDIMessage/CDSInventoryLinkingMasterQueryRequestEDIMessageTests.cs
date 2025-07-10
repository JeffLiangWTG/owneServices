using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingMasterQueryRequestEDIMessage))]
	public class CDSInventoryLinkingMasterQueryRequestEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingMasterQueryRequestEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.MasterQueryDeclaration, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObjectAndInterpretation()
		{
			var message = Factory.New<CDSInventoryLinkingMasterQueryRequestEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingMasterQueryRequestEDIMessage.Serialize(new inventoryLinkingQueryRequest
			{
				queryUCR = new ucrBlock
				{
					ucr = "MUCR",
					ucrType = ucrType.M
				}
			});
			var messasgeDataObject = message.MessageDataObject;
			AssertEquals("MUCR", messasgeDataObject.queryUCR.ucr);
			AssertEquals(ucrType.M, messasgeDataObject.queryUCR.ucrType);
			AssertEquals(ExpectedHTMLInterpretation, message.EM_MessageInterpretation);
		}

		internal static ZString ExpectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Master Query Request to CDS:</H3><p><strong>UCR: </strong>MUCR<br><strong>UCR Type: </strong>M</p>";
	}
}
