using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingControlResponseEDIMessage))]
	class CDSInventoryLinkingControlResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingControlResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<CDSInventoryLinkingControlResponseEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingControlResponseEDIMessage.Serialize(new inventoryLinkingControlResponse
			{
				actionCode = "01",
				error = new errorBlock[]
				{
					new errorBlock { errorCode = "1" }
				},
				messageCode = messageCodeAll.EAA,
				movementReference = "123",
				ucr = new ucrBlock
				{
					ucr = "UCR",
					ucrType = ucrType.M
				}
			});
			var messageDataObject = message.MessageDataObject;
			AssertEquals("01", messageDataObject.ActionCode);
			AssertEquals("1", messageDataObject.ErrorCodes[0]);
			AssertEquals("EAA", messageDataObject.MessageCode);
			AssertEquals("123", messageDataObject.MovementReference);
			AssertEquals("UCR", messageDataObject.UCR);
			AssertEquals("M", messageDataObject.UCRType);

			var message2 = Factory.New<CDSInventoryLinkingControlResponseEDIMessage>();
			message2.EM_MessageText = CDSInventoryLinkingControlResponseEDIMessage.Serialize(new inventoryLinkingControlResponse
			{
				actionCode = "2",
				error = new errorBlock[]
				{
					new errorBlock { errorCode = "1" },
					new errorBlock { errorCode = "2" },
					new errorBlock { errorCode = "3" }
				},
				messageCode = messageCodeAll.EAC,
				movementReference = "234",
				ucr = new ucrBlock
				{
					ucr = "UCR",
					ucrType = ucrType.D
				}
			});
			AssertContainsExactElementsInExactOrder("Multiple error codes", new[] { "1", "2", "3" }, message2.MessageDataObject.ErrorCodes);
		}

		public void TestMessageInterpretation()
		{
			var errorResponse1 = Factory.New<CDSInventoryLinkingControlResponseEDIMessage>();
			errorResponse1.EM_MessageText = @"<inventoryLinkingControlResponse xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>QUE</messageCode>
  <actionCode>3</actionCode>
  <ucr>
    <ucr>3GB896458895015-B60005412</ucr>
    <ucrType>D</ucrType>
  </ucr>
  <error>
    <errorCode>16</errorCode>
  </error>
</inventoryLinkingControlResponse>";
			AssertContains("16 - Unknown Declaration ID, DUCR or MUCR (When DUCR or MUCR is unknown in the query service)", errorResponse1.EM_MessageInterpretation);

			var errorResponse2 = Factory.New<CDSInventoryLinkingControlResponseEDIMessage>();
			errorResponse2.EM_MessageText = @"<inventoryLinkingControlResponse xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
  <messageCode>EAC</messageCode>
  <actionCode>3</actionCode>
  <ucr>
    <ucr>3GB896458895015-B60005412</ucr>
    <ucrPartNo>A</ucrPartNo>
    <ucrType>D</ucrType>
  </ucr>
  <error>
    <errorCode>54 E10003 Errors on Document</errorCode>
  </error>
  <error>
    <errorCode>6 E408 Unique Consignment reference does not exist</errorCode>
  </error>
</inventoryLinkingControlResponse>";
			AssertContains("54 E10003 Errors on Document", errorResponse2.EM_MessageInterpretation);
			AssertContains("6 - Cannot find the consolidation - E408 Unique Consignment reference does not exist", errorResponse2.EM_MessageInterpretation);
		}
	}
}
