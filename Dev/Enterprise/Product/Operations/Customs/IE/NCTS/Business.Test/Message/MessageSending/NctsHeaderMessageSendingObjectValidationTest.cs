using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectValidation))]
	sealed class NctsHeaderMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2025, 2, 8, 10, 45, 31)]
		public void TestCheckMessageType_EstimatedArrivalDateForTransit()
		{
			const string expectedMessageError = "The Estimated Arrival Date Time for Customs Office of Transit can't be earlier or equal to current date time. Please check Details -> Customs Offices";

			var messageSendingObject = new NctsHeaderMessageSendingObject(header);
			var targetPropertyInfo = messageSendingObject.MessageTypeInfo;
			var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
			customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
			customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 30);

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData;
			messageSendingObject.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("When Message type is 015 and Transit data is in the past", targetPropertyInfo, expectedMessageError);

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationAmendment;
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageErrorContaining("When Message type is 013 and Transit data is in the past", targetPropertyInfo, expectedMessageError);

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData;
			customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 32);
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageErrorContaining("When Message type is 015 and Transit data is in the future", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckMessageType_NoCustomsStatus()
		{
			const string expectedMessageError = "Message Type should be Declaration (015) when Customs Status is empty";

			var messageSendingObject = new NctsHeaderMessageSendingObject(header);
			var targetPropertyInfo = messageSendingObject.MessageTypeInfo;

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationAmendment;
			messageSendingObject.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("When Message type is 013", targetPropertyInfo, expectedMessageError);

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData;
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageErrorContaining("When Message type is 015", targetPropertyInfo, expectedMessageError);

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.PresentationNotification;
			messageSendingObject.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("When Message type is 170", targetPropertyInfo, expectedMessageError);

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMessageSendingObject = new NctsHeaderMessageSendingObject(arrivalHeader);
			var arrivalTargetPropertyInfo = arrivalMessageSendingObject.MessageTypeInfo;
			arrivalMessageSendingObject.MessageType = NCTSOutgoingArrivalMessageTypeList.Codes.UnloadingRemarks;
			arrivalMessageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageErrorContaining("Arrival movement", arrivalTargetPropertyInfo, expectedMessageError);
		}

		public void TestCheckMessageType_PlaceOfLoadingRequiredFor170()
		{
			const string placeOfLoadingRequiredMessage = "[C0404] Place of Loading is required for IE170, when it was not sent as part of IE015 or IE013.";
			var messageSendingObject = new NctsHeaderMessageSendingObject(header);
			var targetPropertyInfo = messageSendingObject.MessageTypeInfo;

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData;
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageError("Not 170 message, no check for Place of Loading.", targetPropertyInfo, placeOfLoadingRequiredMessage);

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.PresentationNotification;
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageError("170 message, BM_AdditionalDeclarationType not D, no check for Place of Loading.", targetPropertyInfo, placeOfLoadingRequiredMessage);

			var movementHeader = header.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			messageSendingObject.Validation.ValidateMessageType();
			AssertHasMessageError("No previous 015 message, check for Place of Loading.", targetPropertyInfo, placeOfLoadingRequiredMessage);

			var history015MessageWithoutValidPlaceOfLoading = movementHeader.Messages.AddNew(typeof(NCTSOutboundEDIMessage));
			history015MessageWithoutValidPlaceOfLoading.EM_LinkedObject = movementHeader;
			history015MessageWithoutValidPlaceOfLoading.EM_ApplicationCode = NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS;
			history015MessageWithoutValidPlaceOfLoading.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			history015MessageWithoutValidPlaceOfLoading.EM_ReceiveTransmit = NCTSInboundEDIMessage.Direction.Transmit;
			history015MessageWithoutValidPlaceOfLoading.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-2);
			history015MessageWithoutValidPlaceOfLoading.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			history015MessageWithoutValidPlaceOfLoading.EM_MessageText = MessageStaticHelperTest.CC015TemplateForPlaceOfLoadingTest.Replace(MessageStaticHelperTest.PlaceOfLoadingPlaceHolder, string.Empty);
			movementHeader.BM_PortOfPresentationCode = "IE";
			messageSendingObject.Validation.ValidateMessageType();
			AssertHasMessageError("Previous 015 message without valid PlaceOfLoading, check for Place of Loading.", targetPropertyInfo, placeOfLoadingRequiredMessage);

			movementHeader.BM_PlaceOfLoading = string.Empty;
			movementHeader.BM_PortOfPresentationCode = "IEGWY";
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageError("BM_PortOfPresentationCode has value, Check for Place of Loading(pass).", targetPropertyInfo, placeOfLoadingRequiredMessage);
			movementHeader.BM_PortOfPresentationCode = string.Empty;

			var history015MessageWithValidPlaceOfLoading = movementHeader.Messages.AddNew(typeof(NCTSOutboundEDIMessage));
			history015MessageWithValidPlaceOfLoading.EM_LinkedObject = movementHeader;
			history015MessageWithValidPlaceOfLoading.EM_ApplicationCode = NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS;
			history015MessageWithValidPlaceOfLoading.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			history015MessageWithValidPlaceOfLoading.EM_ReceiveTransmit = NCTSInboundEDIMessage.Direction.Transmit;
			history015MessageWithValidPlaceOfLoading.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-2);
			history015MessageWithValidPlaceOfLoading.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			history015MessageWithValidPlaceOfLoading.EM_MessageText = MessageStaticHelperTest.CC015TemplateForPlaceOfLoadingTest.Replace(MessageStaticHelperTest.PlaceOfLoadingPlaceHolder, MessageStaticHelperTest.PlaceOfLoadingText);
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageError("Previous 015 message with valid PlaceOfLoading, no check for Place of Loading.", targetPropertyInfo, placeOfLoadingRequiredMessage);

			movementHeader.BM_PlaceOfLoading = "GWY";
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageError("Check for Place of Loading(pass).", targetPropertyInfo, placeOfLoadingRequiredMessage);
		}

		public void TestCheckMessageType_NoLRN()
		{
			const string expectedMessageError = "LRN or MRN required to send message other than Declaration (015)";

			var messageSendingObject = new NctsHeaderMessageSendingObject(header);
			var targetPropertyInfo = messageSendingObject.MessageTypeInfo;

			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationAmendment;
			messageSendingObject.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("When LRN and MRN are blank and message type is 013", targetPropertyInfo, expectedMessageError);

			messageSendingObject.NctsHeader.LocalReferenceNumber = "12345";
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageErrorContaining("When LRN is filled", targetPropertyInfo, expectedMessageError);

			messageSendingObject.NctsHeader.LocalReferenceNumber = "";
			messageSendingObject.MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.QueryOnGuarantees;
			messageSendingObject.Validation.ValidateMessageType();
			AssertHasMessageErrorContaining("When LRN and MRN are blank and message type is 034", targetPropertyInfo, expectedMessageError);

			messageSendingObject.NctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "IE23ROS12489725";
			messageSendingObject.Validation.ValidateMessageType();
			AssertNoMessageErrorContaining("When LRN is blank but MRN is filled", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckDestinationCustomsOfficeCode()
		{
			const string expectedMessageError = "[C0315] You have not entered an Actual Destination Office when TC11 Delivery Date is entered";

			var messageSendingObject = new NctsHeaderMessageSendingObject(header);
			messageSendingObject.MessageType = "141";
			var targetPropertyInfo = messageSendingObject.DestinationCustomsOfficeCodeInfo;

			messageSendingObject.TC11DeliveryDate = new ZDateTime(2023, 8, 1);
			messageSendingObject.DestinationCustomsOfficeCode = "";
			messageSendingObject.Validation.ValidateDestinationCustomsOfficeCode();
			AssertHasMessageErrorContaining("When Delivery Date is not empty and Destination Customs office is empty", targetPropertyInfo, expectedMessageError);

			messageSendingObject.DestinationCustomsOfficeCode = "IE12343";
			messageSendingObject.Validation.ValidateDestinationCustomsOfficeCode();
			AssertNoMessageErrorContaining("When Delivery Date and Destination Customs Office is not empty", targetPropertyInfo, expectedMessageError);

			messageSendingObject.DestinationCustomsOfficeCode = "";
			messageSendingObject.TC11DeliveryDate = ZDateTime.Empty;
			messageSendingObject.Validation.ValidateDestinationCustomsOfficeCode();
			AssertNoMessageErrorContaining("When Delivery Date and Destination Customs Office are empty", targetPropertyInfo, expectedMessageError);

			messageSendingObject.MessageType = "013";
			messageSendingObject.TC11DeliveryDate = new ZDateTime(2023, 7, 1);
			messageSendingObject.DestinationCustomsOfficeCode = "";
			AssertNoMessageErrorContaining("When Delivery Date is filled and Destination Customs Office is empty and is ReadOnly", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckEnquiryText()
		{
			const string expectedMessageError = "[C0220] You have not entered an Enquiry Text when TC11 Delivery Date is entered";

			var messageSendingObject = new NctsHeaderMessageSendingObject(header);
			messageSendingObject.MessageType = "141";
			var targetPropertyInfo = messageSendingObject.EnquiryTextInfo;

			messageSendingObject.TC11DeliveryDate = new ZDateTime(2023, 8, 1);
			messageSendingObject.EnquiryText = "";
			messageSendingObject.Validation.ValidateEnquiryText();
			AssertHasMessageErrorContaining("When Delivery Date is not empty and Enquiry Text is empty", targetPropertyInfo, expectedMessageError);

			messageSendingObject.EnquiryText = "IE12343";
			messageSendingObject.Validation.ValidateEnquiryText();
			AssertNoMessageErrorContaining("When Delivery Date and Enquiry Text is not empty", targetPropertyInfo, expectedMessageError);

			messageSendingObject.EnquiryText = "";
			messageSendingObject.TC11DeliveryDate = ZDateTime.Empty;
			messageSendingObject.Validation.ValidateEnquiryText();
			AssertNoMessageErrorContaining("When Delivery Date and Enquiry Text are empty", targetPropertyInfo, expectedMessageError);

			messageSendingObject.MessageType = "013";
			messageSendingObject.TC11DeliveryDate = new ZDateTime(2023, 7, 1);
			messageSendingObject.EnquiryText = "";
			AssertNoMessageErrorContaining("When Delivery Date is filled and Enquiry Text is empty and is ReadOnly", targetPropertyInfo, expectedMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
	}
}
