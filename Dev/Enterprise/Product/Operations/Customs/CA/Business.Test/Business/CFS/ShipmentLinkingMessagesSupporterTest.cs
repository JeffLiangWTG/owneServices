using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ShipmentLinkingMessagesSupporterTest : TestCaseWithFactory
	{
		public void TestHookCusEntryNumberChangedEvent()
		{
			var shipment = Factory.New<CFSShipment>();

			var supporter = new ShipmentLinkingMessagesSupporterFotTesting(shipment);
			supporter.HookShipment();
			AssertEquals("Precondition:", 0, supporter.ChangeTimes);
			AssertEquals("Precondition:", ZString.Empty, supporter.CargoControlNumberChanged_Exposed);

			var num = Factory.New<CusEntryNumber>();
			num.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num.CE_EntryNum = "1020 7000067891";
			shipment.Numbers.Add(num);

			AssertEquals("CusEntryNumberChangedEvent should be triggered", 1, supporter.ChangeTimes);
			AssertEquals("Current Cargo Control Number changed to 10207000067891", supporter.CargoControlNumberChanged_Exposed);

			num.CE_EntryNum = "1020 7000067892";
			AssertEquals("CusEntryNumberChangedEvent should be triggered", 2, supporter.ChangeTimes);
			AssertEquals("Current Cargo Control Number changed to 10207000067892", supporter.CargoControlNumberChanged_Exposed);

			num.CE_EntryNum = "10207000067892";
			AssertEquals("CusEntryNumberChangedEvent should not be triggered", 2, supporter.ChangeTimes);

			num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			AssertEquals("CusEntryNumberChangedEvent should be triggered", 3, supporter.ChangeTimes);
			AssertEquals("Current Cargo Control Number changed to ", supporter.CargoControlNumberChanged_Exposed);

			num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			AssertEquals("CusEntryNumberChangedEvent should be triggered", 4, supporter.ChangeTimes);
			AssertEquals("Current Cargo Control Number changed to 10207000067892", supporter.CargoControlNumberChanged_Exposed);

			shipment.Numbers.Remove(num);
			AssertEquals("CusEntryNumberChangedEvent should be triggered", 5, supporter.ChangeTimes);
			AssertEquals("Current Cargo Control Number changed to ", supporter.CargoControlNumberChanged_Exposed);

			num.CE_EntryNum = "1020 7000067891";
			AssertEquals("CusEntryNumberChangedEvent should not be triggered", 5, supporter.ChangeTimes);

			num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			shipment.Numbers.Add(num);
			AssertEquals("CusEntryNumberChangedEvent should not be triggered", 5, supporter.ChangeTimes);

			supporter.UnHookShipment();

			num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num.CE_EntryNum = "1020 7000067892";

			AssertEquals("CusEntryNumberChangedEvent should not be triggered", 5, supporter.ChangeTimes);

			supporter.HookShipment();

			num.CE_EntryNum = "1020 7000067891";
			AssertEquals("CusEntryNumberChangedEvent should be triggered", 6, supporter.ChangeTimes);

			var num2 = Factory.New<CusEntryNumber>();
			num2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			num2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num2.CE_EntryNum = "1020 7000067892";
			shipment.Numbers.Add(num2);

			AssertEquals("CusEntryNumberChangedEvent should be triggered", 7, supporter.ChangeTimes);
			AssertEquals("Current Cargo Control Number changed to ", supporter.CargoControlNumberChanged_Exposed);
		}

		public void TestLinkingMatchingRNSStatusMessagesByCCN()
		{
			var shipment = Factory.New<CFSShipment>();

			string cargoControlNumber1 = "10207000067891";

			var receivedUnattachedMessage1 = CreateEDIReleaseMessage("10207400004061", cargoControlNumber1, EDIReleaseImportEntryStatusList.Codes.GoodsReleased);

			var receivedAttachedMessage1 = CreateEDIReleaseMessage("10207400004062", cargoControlNumber1, EDIReleaseImportEntryStatusList.Codes.GoodsReleased);
			receivedAttachedMessage1.EM_LinkedObject = shipment;

			var receivedAttachedOtherObjectMessage1 = CreateEDIReleaseMessage("10207400004063", cargoControlNumber1, EDIReleaseImportEntryStatusList.Codes.GoodsReleased);
			receivedAttachedOtherObjectMessage1.EM_LinkUniqueID = ZGuid.NewZGuid();
			receivedAttachedOtherObjectMessage1.EM_LinkTable = Enterprise.Messaging.Business.EDIInterchange.Schema.TableName;

			var interchange1 = Factory.New<Enterprise.Messaging.Business.EDIInterchange>();
			interchange1.EI_From = "ME";
			interchange1.EI_To = "YOU";
			receivedAttachedOtherObjectMessage1.EM_EI = interchange1.PK;

			var receivedAttachedErrorMessage1 = CreateEDIReleaseMessage("10207400004064", cargoControlNumber1, EDIReleaseImportEntryStatusList.Codes.Error);
			receivedAttachedErrorMessage1.EM_LinkedObject = shipment;

			var sentUnattachedRNSRequestMessage1 = CreateRNSRequestMessage(cargoControlNumber1);
			var sentAttachedRNSRequestMessage1 = CreateRNSRequestMessage(cargoControlNumber1);
			sentAttachedRNSRequestMessage1.EM_LinkUniqueID = ZGuid.NewZGuid();
			sentAttachedRNSRequestMessage1.EM_LinkTable = EDIMessage.Schema.TableName;

			string cargoControlNumber2 = "10207000067892";

			var receivedUnattachedMessage2 = CreateEDIReleaseMessage("10207400004071", cargoControlNumber2, EDIReleaseImportEntryStatusList.Codes.GoodsReleased);

			var receivedAttachedMessage2 = CreateEDIReleaseMessage("10207400004072", cargoControlNumber2, EDIReleaseImportEntryStatusList.Codes.GoodsReleased);
			receivedAttachedMessage2.EM_LinkedObject = shipment;

			var receivedAttachedOtherObjectMessage2 = CreateEDIReleaseMessage("10207400004073", cargoControlNumber2, EDIReleaseImportEntryStatusList.Codes.GoodsReleased);
			receivedAttachedOtherObjectMessage2.EM_LinkUniqueID = ZGuid.NewZGuid();
			receivedAttachedOtherObjectMessage2.EM_LinkTable = EDIMessage.Schema.TableName;

			var receivedAttachedErrorMessage2 = CreateEDIReleaseMessage("10207400004074", cargoControlNumber2, EDIReleaseImportEntryStatusList.Codes.Error);
			receivedAttachedErrorMessage2.EM_LinkedObject = shipment;

			var sentUnattachedRNSRequestMessage2 = CreateRNSRequestMessage(cargoControlNumber2);
			var sentAttachedRNSRequestMessage2 = CreateRNSRequestMessage(cargoControlNumber2);
			sentAttachedRNSRequestMessage2.EM_LinkUniqueID = ZGuid.NewZGuid();
			sentAttachedRNSRequestMessage2.EM_LinkTable = EDIMessage.Schema.TableName;

			Factory.Save();

			AssertEquals(4, shipment.Messages.Count);
			AssertCollectionContains("Precondition: receivedAttachedMessage1 should be attached to the shipment", receivedAttachedMessage1, shipment.Messages);
			AssertCollectionContains("Precondition: receivedAttachedErrorMessage1 should be attached to the shipment", receivedAttachedErrorMessage1, shipment.Messages);
			AssertCollectionContains("Precondition: receivedAttachedMessage2 should be attached to the shipment", receivedAttachedMessage2, shipment.Messages);
			AssertCollectionContains("Precondition: receivedAttachedErrorMessage2 should be attached to the shipment", receivedAttachedErrorMessage2, shipment.Messages);

			using (var supporter = new ShipmentLinkingMessagesSupporterFotTesting(shipment))
			{
				supporter.HookShipment();

				var num = shipment.Numbers.AddNew();
				num.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
				num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num.CE_EntryNum = "1020 7000067891";
				shipment.Numbers.Add(num);

				AssertEquals("Precondition: CanadaHouseCCN", "1020 7000067891", shipment.CanadaHouseCCN);
				AssertEquals("3 matching RNS status messages should be found", 3, supporter.MatchingRNSStatusMessages_Exposed.Length);

				AssertCollectionContains("MatchingRNSStatusMessages should contains receivedUnattachedMessage1", receivedUnattachedMessage1, supporter.MatchingRNSStatusMessages_Exposed);
				AssertCollectionContains("MatchingRNSStatusMessages should contains receivedAttachedOtherObjectMessage1", receivedAttachedOtherObjectMessage1, supporter.MatchingRNSStatusMessages_Exposed);
				AssertCollectionContains("MatchingRNSStatusMessages should contains sentUnattachedRNSRequestMessage1", sentUnattachedRNSRequestMessage1, supporter.MatchingRNSStatusMessages_Exposed);

				AssertEquals(7, shipment.Messages.Count);

				AssertCollectionContains("receivedAttachedMessage1 should be attached to the shipment", receivedAttachedMessage1, shipment.Messages);
				AssertCollectionContains("receivedAttachedErrorMessage1 should be attached to the shipment", receivedAttachedErrorMessage1, shipment.Messages);
				AssertCollectionContains("receivedAttachedMessage2 should be attached to the shipment", receivedAttachedMessage2, shipment.Messages);
				AssertCollectionContains("receivedAttachedErrorMessage2 should be attached to the shipment", receivedAttachedErrorMessage2, shipment.Messages);

				AssertEquals("receivedUnattachedMessage1 should be attached to the shipment", shipment, receivedUnattachedMessage1.EM_LinkedObject);
				AssertCollectionContains("receivedUnattachedMessage1 should be attached to the shipment", receivedUnattachedMessage1, shipment.Messages);

				AssertEquals("sentUnattachedRNSRequestMessage1 should be attached to the shipment", shipment, sentUnattachedRNSRequestMessage1.EM_LinkedObject);
				AssertCollectionContains("sentUnattachedRNSRequestMessage1 should be attached to the shipment", sentUnattachedRNSRequestMessage1, shipment.Messages);

				var clonedMessage1 = (EDIReleaseMessage)shipment.Messages.First(m => ((EDIMessage)m).TransactionNumber == "10207400004063");

				AssertNotNull("A cloned message should be created and attached to the shipment", clonedMessage1);
				AssertNotEquals("A cloned message should be created and attached to the shipment", clonedMessage1, receivedAttachedOtherObjectMessage1);

				AssertEquals("EM_ReceiveTransmit of cloned message", EDIMessage.Direction.Receive, clonedMessage1.EM_ReceiveTransmit);
				AssertEquals("EM_MessageText of cloned message", "UNH+1+CUSRES:D:96A:UN'BGM+:::257+10207400004063+11'LOC+22+0497:129::3072'DTM+58:201011250820:203'GIS+4'RFF+XC:10207000067891'UNT+7+1'", clonedMessage1.EM_MessageText);
				AssertEquals("EM_MessageSubType of cloned message", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, clonedMessage1.EM_MessageSubType);
				AssertEquals("HTML INTERPRETATION", clonedMessage1.EM_MessageInterpretation);

				num.CE_EntryNum = "1020 7000067892";
				AssertEquals("Precondition: CanadaHouseCCN", "1020 7000067892", shipment.CanadaHouseCCN);

				AssertEquals("3 matching RNS status messages should be found", 3, supporter.MatchingRNSStatusMessages_Exposed.Length);

				AssertCollectionContains("MatchingRNSStatusMessages should contains receivedUnattachedMessage2", receivedUnattachedMessage2, supporter.MatchingRNSStatusMessages_Exposed);
				AssertCollectionContains("MatchingRNSStatusMessages should contains receivedAttachedOtherObjectMessage2", receivedAttachedOtherObjectMessage2, supporter.MatchingRNSStatusMessages_Exposed);
				AssertCollectionContains("MatchingRNSStatusMessages should contains sentUnattachedRNSRequestMessage2", sentUnattachedRNSRequestMessage2, supporter.MatchingRNSStatusMessages_Exposed);

				AssertEquals(7, shipment.Messages.Count);

				AssertCollectionContains("receivedAttachedMessage1 should be attached to the shipment", receivedAttachedMessage1, shipment.Messages);
				AssertCollectionContains("receivedAttachedErrorMessage1 should be attached to the shipment", receivedAttachedErrorMessage1, shipment.Messages);
				AssertCollectionContains("receivedAttachedMessage2 should be attached to the shipment", receivedAttachedMessage2, shipment.Messages);
				AssertCollectionContains("receivedAttachedErrorMessage2 should be attached to the shipment", receivedAttachedErrorMessage2, shipment.Messages);

				AssertEquals("receivedUnattachedMessage2 should be attached to the shipment", shipment, receivedUnattachedMessage2.EM_LinkedObject);
				AssertCollectionContains("receivedUnattachedMessage2 should be attached to the shipment", receivedUnattachedMessage2, shipment.Messages);
				AssertEquals("receivedUnattachedMessage1 should not be attached to the shipment", null, receivedUnattachedMessage1.EM_LinkedObject);
				AssertCollectionNotContains("receivedUnattachedMessage1 should not be attached to the shipment", receivedUnattachedMessage1, shipment.Messages);

				AssertEquals("sentUnattachedRNSRequestMessage2 should be attached to the shipment", shipment, sentUnattachedRNSRequestMessage2.EM_LinkedObject);
				AssertCollectionContains("sentUnattachedRNSRequestMessage2 should be attached to the shipment", sentUnattachedRNSRequestMessage2, shipment.Messages);
				AssertEquals("sentUnattachedRNSRequestMessage1 should not be attached to the shipment", null, sentUnattachedRNSRequestMessage1.EM_LinkedObject);
				AssertCollectionNotContains("sentUnattachedRNSRequestMessage1 should not be attached to the shipment", sentUnattachedRNSRequestMessage1, shipment.Messages);

				var clonedMessage2 = (EDIReleaseMessage)shipment.Messages.First(m => ((EDIMessage)m).TransactionNumber == "10207400004073");

				AssertNotNull("A new message should be created and attached to the shipment", clonedMessage2);
				AssertNotEquals("A new message should be created and attached to the shipment", clonedMessage2, receivedAttachedOtherObjectMessage2);

				AssertEquals(EDIMessage.Direction.Receive, clonedMessage2.EM_ReceiveTransmit);
				AssertEquals("UNH+1+CUSRES:D:96A:UN'BGM+:::257+10207400004073+11'LOC+22+0497:129::3072'DTM+58:201011250820:203'GIS+4'RFF+XC:10207000067892'UNT+7+1'", clonedMessage2.EM_MessageText);
				AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsReleased, clonedMessage2.EM_MessageSubType);
				AssertEquals("HTML INTERPRETATION", clonedMessage2.EM_MessageInterpretation);

				Assert("clonedMessage1 should be deleted", clonedMessage1.IsDeleted);

				num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				Assert("Precondition: CanadaHouseCCN", shipment.CanadaHouseCCN.IsEmpty);

				AssertEquals(4, shipment.Messages.Count);
				AssertCollectionContains("receivedAttachedMessage1 should be attached to the shipment", receivedAttachedMessage1, shipment.Messages);
				AssertCollectionContains("receivedAttachedErrorMessage1 should be attached to the shipment", receivedAttachedErrorMessage1, shipment.Messages);
				AssertCollectionContains("receivedAttachedMessage2 should be attached to the shipment", receivedAttachedMessage2, shipment.Messages);
				AssertCollectionContains("receivedAttachedErrorMessage2 should be attached to the shipment", receivedAttachedErrorMessage2, shipment.Messages);

				Assert("clonedMessage2 should be deleted", clonedMessage2.IsDeleted);
			}
		}

		public void TestLinkingMatchingForwardedManifestMessagesByCCN()
		{
			var shipment = Factory.New<CFSShipment>();

			string cargoControlNumber1 = "10207000067891";

			var unattachedMessage1 = CreateForwardedManifestMessage(cargoControlNumber1, SecondaryNotifyPartyTypeList.Codes.Warehouse);

			var attachedMessage1 = CreateForwardedManifestMessage(cargoControlNumber1, SecondaryNotifyPartyTypeList.Codes.Warehouse);
			attachedMessage1.EM_LinkedObject = shipment;

			var unattachedBrokerMessage1 = CreateForwardedManifestMessage(cargoControlNumber1, SecondaryNotifyPartyTypeList.Codes.CustomsBroker);

			string cargoControlNumber2 = "10207000067892";

			var unattachedMessage2 = CreateForwardedManifestMessage(cargoControlNumber2, SecondaryNotifyPartyTypeList.Codes.Warehouse);

			var attachedMessage2 = CreateForwardedManifestMessage(cargoControlNumber2, SecondaryNotifyPartyTypeList.Codes.Warehouse);
			attachedMessage2.EM_LinkedObject = shipment;

			var unattachedBrokerMessage2 = CreateForwardedManifestMessage(cargoControlNumber2, SecondaryNotifyPartyTypeList.Codes.CustomsBroker);

			Factory.Save();

			AssertEquals(2, shipment.Messages.Count);
			AssertCollectionContains("Precondition: attachedMessage1 should be attached to the shipment", attachedMessage1, shipment.Messages);
			AssertCollectionContains("Precondition: attachedMessage2 should be attached to the shipment", attachedMessage2, shipment.Messages);

			using (var supporter = new ShipmentLinkingMessagesSupporterFotTesting(shipment))
			{
				supporter.HookShipment();

				var num = shipment.Numbers.AddNew();
				num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num.CE_EntryNum = "1020 7000067891";
				shipment.Numbers.Add(num);

				AssertEquals("Precondition: CanadaHouseCCN", "1020 7000067891", shipment.CanadaHouseCCN);

				AssertEquals("1 matching Forwarded Manifest messages should be found", 1, supporter.MatchingForwardedManifestMessages_Exposed.Length);
				AssertCollectionContains("MatchingRNSStatusMessages should contains receivedUnattachedMessage1", unattachedMessage1, supporter.MatchingForwardedManifestMessages_Exposed);

				AssertEquals(3, shipment.Messages.Count);

				AssertCollectionContains("attachedMessage1 should be attached to the shipment", attachedMessage1, shipment.Messages);
				AssertCollectionContains("attachedMessage2 should be attached to the shipment", attachedMessage2, shipment.Messages);

				AssertEquals("unattachedMessage1 should be attached to the shipment", shipment, unattachedMessage1.EM_LinkedObject);
				AssertCollectionContains("unattachedMessage1 should be attached to the shipment", unattachedMessage1, shipment.Messages);

				num.CE_EntryNum = "1020 7000067892";
				AssertEquals("Precondition: CanadaHouseCCN", "1020 7000067892", shipment.CanadaHouseCCN);

				AssertEquals("1 matching Forwarded Manifest messages should be found", 1, supporter.MatchingForwardedManifestMessages_Exposed.Length);
				AssertCollectionContains("MatchingForwardedManifestMessages should contains unattachedMessage2", unattachedMessage2, supporter.MatchingForwardedManifestMessages_Exposed);

				AssertEquals(3, shipment.Messages.Count);

				AssertCollectionContains("attachedMessage1 should be attached to the shipment", attachedMessage1, shipment.Messages);
				AssertCollectionContains("attachedMessage2 should be attached to the shipment", attachedMessage2, shipment.Messages);

				AssertEquals("unattachedMessage2 should be attached to the shipment", shipment, unattachedMessage2.EM_LinkedObject);
				AssertCollectionContains("unattachedMessage2 should be attached to the shipment", unattachedMessage2, shipment.Messages);
				AssertEquals("unattachedMessage1 should not be attached to the shipment", null, unattachedMessage1.EM_LinkedObject);
				AssertCollectionNotContains("unattachedMessage1 should not be attached to the shipment", unattachedMessage1, shipment.Messages);

				num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				Assert("Precondition: CanadaHouseCCN", shipment.CanadaHouseCCN.IsEmpty);

				AssertEquals("Only 2 messages left", 2, shipment.Messages.Count);
				AssertCollectionContains("attachedMessage1 should be attached to the shipment", attachedMessage1, shipment.Messages);
				AssertCollectionContains("attachedMessage2 should be attached to the shipment", attachedMessage2, shipment.Messages);
			}
		}

		EDIMessage CreateEDIReleaseMessage(ZString transactionNumber, ZString cargoControlNumber, ZString subMessageType)
		{
			var message = Factory.New<EDIReleaseMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = string.Format("UNH+1+CUSRES:D:96A:UN'BGM+:::257+{0}+11'LOC+22+0497:129::3072'DTM+58:201011250820:203'GIS+4'RFF+XC:{1}'UNT+7+1'", transactionNumber, cargoControlNumber);
			message.EM_MessageSubType = subMessageType;
			message.EM_MessageInterpretation = "HTML INTERPRETATION";
			message.EM_ApplicationReference = cargoControlNumber.Replace(" ", "");
			message.EM_Status = EDIMessage.Status.Received;
			message.SetSystemDefinedValue(EDIMessage.Schema.TransactionNumber, new ZString(transactionNumber));
			return message;
		}

		EDIMessage CreateRNSRequestMessage(ZString cargoControlNumber)
		{
			var message = Factory.New<RNSRequestMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = string.Format("UNH+1+CUSREP:D:96A:UN'BGM+998'DTM+132:201009200644:203'RFF+ABT:{0}'UNT+5+145'", cargoControlNumber);
			message.EM_ApplicationReference = cargoControlNumber.Replace(" ", "");
			message.EM_Status = EDIMessage.Status.Sent;
			return message;
		}

		EDIMessage CreateForwardedManifestMessage(ZString cargoControlNumber, ZString snpType)
		{
			var message = Factory.New<ACIHouseBillMessage>();
			message.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = string.Format(
				@"UNH+1+GOVCBR:D:11B:UN'BGM+714+{0}+4'RFF+AFM:10207:{1}'RFF+UCN:UCR555'DOC+23+:24'DOC+85+201009200644'RCS+15'FTX+ACB+++SOM B 2 B COMMENTS'TDT+11++1'UNS+D",
				cargoControlNumber, snpType);
			message.EM_ApplicationReference = cargoControlNumber.Replace(" ", "");
			message.EM_Status = EDIMessage.Status.Received;
			message.SetSystemDefinedValue(EDIMessage.Schema.SNPType, snpType);

			return message;
		}
	}
}
