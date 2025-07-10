using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public class MessageStaticHelperTest : TestCaseWithFactory
	{
		public void TestPreviouslySentPlaceOfLoading()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals("No message attached, PreviouslySentPlaceOfLoading returns false.", false, MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader));

			var history015MessageWithoutValidPlaceOfLoading = nctsHeader.MovementHeader.Messages.AddNew(typeof(NCTSOutboundEDIMessage));
			history015MessageWithoutValidPlaceOfLoading.EM_LinkedObject = nctsHeader;
			history015MessageWithoutValidPlaceOfLoading.EM_ApplicationCode = NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS;
			history015MessageWithoutValidPlaceOfLoading.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			history015MessageWithoutValidPlaceOfLoading.EM_ReceiveTransmit = NCTSInboundEDIMessage.Direction.Transmit;
			history015MessageWithoutValidPlaceOfLoading.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-2);
			history015MessageWithoutValidPlaceOfLoading.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			history015MessageWithoutValidPlaceOfLoading.EM_MessageText = CC015TemplateForPlaceOfLoadingTest.Replace(PlaceOfLoadingPlaceHolder, string.Empty);
			AssertEquals("Previous CC015C exists but no valid PlaceOfLoading, PreviouslySentPlaceOfLoading returns false.", false, MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader));

			var history015MessageWithValidPlaceOfLoading = nctsHeader.MovementHeader.Messages.AddNew(typeof(NCTSOutboundEDIMessage));
			history015MessageWithValidPlaceOfLoading.EM_LinkedObject = nctsHeader;
			history015MessageWithValidPlaceOfLoading.EM_ApplicationCode = NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS;
			history015MessageWithValidPlaceOfLoading.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			history015MessageWithValidPlaceOfLoading.EM_ReceiveTransmit = NCTSInboundEDIMessage.Direction.Transmit;
			history015MessageWithValidPlaceOfLoading.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-2);
			history015MessageWithValidPlaceOfLoading.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			history015MessageWithValidPlaceOfLoading.EM_MessageText = CC015TemplateForPlaceOfLoadingTest.Replace(PlaceOfLoadingPlaceHolder, PlaceOfLoadingText);
			AssertEquals("Previous CC015C exists with valid PlaceOfLoading, PreviouslySentPlaceOfLoading returns true.", true, MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader));
		}

		public void TestHasPlaceOfLoading_CC015CType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var history015Message = nctsHeader.MovementHeader.Messages.AddNew(typeof(NCTSOutboundEDIMessage));
			history015Message.EM_LinkedObject = nctsHeader;
			history015Message.EM_ApplicationCode = NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS;
			history015Message.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			history015Message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-2);
			history015Message.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			history015Message.EM_MessageText = CC015TemplateForPlaceOfLoadingTest.Replace(PlaceOfLoadingPlaceHolder, string.Empty);
			AssertEquals("CC015C message without PlaceOfLoading, PreviouslySentPlaceOfLoading returns false", false, MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader));

			history015Message.EM_MessageText = CC015TemplateForPlaceOfLoadingTest.Replace(PlaceOfLoadingPlaceHolder, PlaceOfLoadingText);
			AssertEquals("CC015C message valid PlaceOfLoading, PreviouslySentPlaceOfLoading returns true", true, MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader));
		}

		public void TestHasPlaceOfLoading_CC013CType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var history013Message = nctsHeader.MovementHeader.Messages.AddNew(typeof(NCTSOutboundEDIMessage));
			history013Message.EM_LinkedObject = nctsHeader;
			history013Message.EM_ApplicationCode = NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS;
			history013Message.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment;
			history013Message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-2);
			history013Message.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			history013Message.EM_MessageText = CC013TemplateForPlaceOfLoadingTest.Replace(PlaceOfLoadingPlaceHolder, string.Empty);
			AssertEquals("CC013C message without PlaceOfLoading, PreviouslySentPlaceOfLoading returns false", false, MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader));

			history013Message.EM_MessageText = CC013TemplateForPlaceOfLoadingTest.Replace(PlaceOfLoadingPlaceHolder, PlaceOfLoadingText);
			AssertEquals("CC013C message valid PlaceOfLoading, PreviouslySentPlaceOfLoading returns true", true, MessageStaticHelper.PreviouslySentPlaceOfLoading(nctsHeader));
		}

		public void TestCanAmendGuarantees()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_InBondEntryType = "A";
			var guarantee = Factory.New<NctsGuarantee>();
			guarantee.PW_BondType = "3";
			movementHeader.Guarantees.Add(guarantee);

			var originalIE015sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var originalIE015sendingAction = new NctsMessageSendingAction(originalIE015sendingObject);
			originalIE015sendingAction.MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			var sender = new NctsMessageSender(originalIE015sendingAction);
			sender.Send();

			movementHeader.Messages[0].EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent;
			Factory.Save();

			// guarantees unchanged, other fields changed
			movementHeader.BM_InBondEntryType = "Z";

			AssertEquals(true, MessageStaticHelper.CanAmendGuarantees(nctsHeader));

			// guarantees changed (added), other fields unchanged (same as original message)
			movementHeader.BM_InBondEntryType = "A";
			movementHeader.Guarantees.AddNew();

			AssertEquals(true, MessageStaticHelper.CanAmendGuarantees(nctsHeader));

			// guarantees changed and other fields changed
			guarantee.PW_Password = "n3w";
			movementHeader.BM_InBondEntryType = "Y";

			AssertEquals(false, MessageStaticHelper.CanAmendGuarantees(nctsHeader));
		}

		public const string PlaceOfLoadingPlaceHolder = "PLACE_OF_LOADING_PLACE_HOLDER";

		public const string PlaceOfLoadingText = @"<PlaceOfLoading>
		<UNLocode>DE222</UNLocode>
	</PlaceOfLoading>";

		public const string CC015TemplateForPlaceOfLoadingTest = @"<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
	<messageSender>_NCTSMessageSenderHolder_</messageSender>
	<messageRecipient>IE</messageRecipient>
	<preparationDateAndTime>2023-10-12T07:26:12</preparationDateAndTime>
	<messageIdentification>_NCTSMessageRecipientHolder_</messageIdentification>
	<messageType>CC015C</messageType>
	<TransitOperation>
	<LRN>NCTS__LRN_Holder</LRN>
	<declarationType>TIR</declarationType>
	<additionalDeclarationType>D</additionalDeclarationType>
	<security>1</security>
	<reducedDatasetIndicator>0</reducedDatasetIndicator>
	<communicationLanguageAtDeparture>IE</communicationLanguageAtDeparture>
	<bindingItinerary>0</bindingItinerary>
	</TransitOperation>
	<CustomsOfficeOfDeparture>
	<referenceNumber>AD000001</referenceNumber>
	</CustomsOfficeOfDeparture>
	<CustomsOfficeOfTransitDeclared>
	<sequenceNumber>1</sequenceNumber>
	<referenceNumber />
	<arrivalDateAndTimeEstimated>0001-01-01T00:00:00</arrivalDateAndTimeEstimated>
	</CustomsOfficeOfTransitDeclared>
	<Consignment>
	<containerIndicator>0</containerIndicator>
		<grossMass>1.000000</grossMass>
		<LocationOfGoods>
			<typeOfLocation />
			<qualifierOfIdentification />
		</LocationOfGoods>
		<ActiveBorderTransportMeans>
			<sequenceNumber>1</sequenceNumber>
		</ActiveBorderTransportMeans>
		PLACE_OF_LOADING_PLACE_HOLDER
		<HouseConsignment>
			<sequenceNumber>1</sequenceNumber>
			<grossMass>1.000000</grossMass>
			<ConsignmentItem>
			<goodsItemNumber>1</goodsItemNumber>
			<declarationGoodsItemNumber>1</declarationGoodsItemNumber>
			<Commodity>
				<CommodityCode>
				<harmonizedSystemSubHeadingCode />
				</CommodityCode>
				<GoodsMeasure>
				<grossMass>1.000000</grossMass>
				</GoodsMeasure>
			</Commodity>
			<PreviousDocument>
				<sequenceNumber>1</sequenceNumber>
				<type />
				<referenceNumber>1</referenceNumber>
				<numberOfPackages>1</numberOfPackages>
			</PreviousDocument>
			<PreviousDocument>
				<sequenceNumber>2</sequenceNumber>
				<type />
				<referenceNumber />
				<numberOfPackages>1</numberOfPackages>
			</PreviousDocument>
			</ConsignmentItem>
		</HouseConsignment>
	</Consignment>
</q1:CC015C>";

		public const string CC013TemplateForPlaceOfLoadingTest = @"<q1:CC013C xmlns:q1=""http://ncts.dgtaxud.ec"">
	<messageSender>_NCTSMessageSenderHolder_</messageSender>
	<messageRecipient>IE</messageRecipient>
	<preparationDateAndTime>2023-10-13T08:10:42</preparationDateAndTime>
	<messageIdentification>_NCTSMessageRecipientHolder_</messageIdentification>
	<messageType>CC013C</messageType>
	<TransitOperation>
		<LRN>NCTS__LRN_Holder</LRN>
		<declarationType>TIR</declarationType>
		<additionalDeclarationType>D</additionalDeclarationType>
		<security>1</security>
		<reducedDatasetIndicator>0</reducedDatasetIndicator>
		<communicationLanguageAtDeparture>IE</communicationLanguageAtDeparture>
		<bindingItinerary>0</bindingItinerary>
		<amendmentTypeFlag>0</amendmentTypeFlag>
	</TransitOperation>
	<CustomsOfficeOfDeparture>
		<referenceNumber>AD000001</referenceNumber>
	</CustomsOfficeOfDeparture>
	<CustomsOfficeOfTransitDeclared>
		<sequenceNumber>1</sequenceNumber>
		<referenceNumber />
		<arrivalDateAndTimeEstimated>0001-01-01T00:00:00</arrivalDateAndTimeEstimated>
	</CustomsOfficeOfTransitDeclared>
	<Consignment>
		<containerIndicator>0</containerIndicator>
		<grossMass>1.000000</grossMass>
		<LocationOfGoods>
			<typeOfLocation />
			<qualifierOfIdentification />
		</LocationOfGoods>
		<ActiveBorderTransportMeans>
			<sequenceNumber>1</sequenceNumber>
		</ActiveBorderTransportMeans>
		PLACE_OF_LOADING_PLACE_HOLDER
		<PlaceOfUnloading>
			<UNLocode>DE222</UNLocode>
		</PlaceOfUnloading>
		<HouseConsignment>
			<sequenceNumber>1</sequenceNumber>
			<grossMass>1.000000</grossMass>
			<ConsignmentItem>
				<goodsItemNumber>1</goodsItemNumber>
				<declarationGoodsItemNumber>1</declarationGoodsItemNumber>
				<Commodity>
					<CommodityCode>
						<harmonizedSystemSubHeadingCode />
					</CommodityCode>
					<GoodsMeasure>
						<grossMass>1.000000</grossMass>
					</GoodsMeasure>
				</Commodity>
				<PreviousDocument>
					<sequenceNumber>1</sequenceNumber>
					<type />
					<referenceNumber>1</referenceNumber>
					<numberOfPackages>1</numberOfPackages>
				</PreviousDocument>
				<PreviousDocument>
					<sequenceNumber>2</sequenceNumber>
					<type />
					<referenceNumber />
					<numberOfPackages>1</numberOfPackages>
				</PreviousDocument>
			</ConsignmentItem>
		</HouseConsignment>
	</Consignment>
</q1:CC013C>
";
	}
}
