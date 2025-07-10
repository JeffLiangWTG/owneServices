using System;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInventoryLinkingMovementRequestEDIMessage))]
	public class CDSInventoryLinkingMovementRequestEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSInventoryLinkingMovementRequestEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<CDSInventoryLinkingMovementRequestEDIMessage>();
			message.EM_MessageText = CDSInventoryLinkingMovementRequestEDIMessage.Serialize(new inventoryLinkingMovementRequest
			{
				messageCode = messageCodeMovement.EAL,
				masterUCR = "MUCR",
				ucrBlock = new ucrBlock
				{
					ucr = "DUCR",
					ucrType = ucrType.M
				},
				goodsArrivalDateTime = new DateTime(2018, 8, 18, 18, 18, 18),
				goodsArrivalDateTimeSpecified = true,
				masterOpt = masterOpt.A,
				masterOptSpecified = true,
				goodsLocation = "GLoc",
				movementReference = "MoveRef",
				shedOPID = "Shed",
				transportDetails = new transportDetails
				{
					transportID = "ID",
					transportMode = "AIR",
					transportNationality = "GB"
				}
			});
			var messasgeDataObject = message.MessageDataObject;
			AssertEquals(messageCodeMovement.EAL, messasgeDataObject.messageCode);
			AssertEquals("MUCR", messasgeDataObject.masterUCR);
			AssertEquals("DUCR", UCRHelper.UcrBlockToString(messasgeDataObject.ucrBlock));
			AssertEquals(ucrType.M, messasgeDataObject.ucrBlock.ucrType);
			AssertEquals(new DateTime(2018, 8, 18, 18, 18, 18), messasgeDataObject.goodsArrivalDateTime);
			AssertEquals(true, messasgeDataObject.goodsArrivalDateTimeSpecified);
			AssertEquals(masterOpt.A, messasgeDataObject.masterOpt);
			AssertEquals(true, messasgeDataObject.masterOptSpecified);
			AssertEquals("GLoc", messasgeDataObject.goodsLocation);
			AssertEquals("MoveRef", messasgeDataObject.movementReference);
			AssertEquals("Shed", messasgeDataObject.shedOPID);
			AssertEquals("ID", messasgeDataObject.transportDetails.transportID);
			AssertEquals("AIR", messasgeDataObject.transportDetails.transportMode);
			AssertEquals("GB", messasgeDataObject.transportDetails.transportNationality);
			AssertEquals(expectedHTMLInterpretation, message.EM_MessageInterpretation);
		}

		static readonly string expectedHTMLInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Movement request message of type (EAL, EAA, EDL)</H3><p><strong>Message Code: </strong>EAL<br><strong>MUCR: </strong>MUCR<br><strong>DUCR: </strong>DUCR<br><strong>Goods Arrival Date Time: </strong>18/08/2018 6:18:18 PM<br><strong>Goods Location: </strong>GLoc<br><strong>Movement Reference Number: </strong>MoveRef<br><strong>Shed Operator: </strong>Shed<br><strong>Transport ID: </strong>ID<br><strong>Transport Mode: </strong>AIR<br><strong>Transport Nationality: </strong>GB</p>";
	}
}
