using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE141MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE141MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE141MessageProvider(null, sendingAction));
				AssertNoExceptionThrown("Sending action missing", () => new IE141MessageProvider(nctsHeader, null));
			});
		}

		protected override IE141MessageProvider GetProvider() => new IE141MessageProvider(nctsHeader, sendingAction);

		public void TestTransitOperation()
		{
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRNIERO1234";
			AssertEquals("MRN", "MRNIERO1234", Provider.TransitOperation.MRN);
		}

		public void TestCustomsOfficeOfDestinationActual()
		{
			sendingAction.DestinationCustomsOfficeCode = "CUSOF"; 
			AssertEquals("Enquiry text not set", string.Empty, Provider.CustomsOfficeOfDestinationActual);
			sendingAction.EnquiryText = "Enquiry Text";
			AssertEquals("Customs Office Of Destination Actual", "CUSOF", Provider.CustomsOfficeOfDestinationActual);
		}

		public void TestCustomsOfficeOfEnquiryAtDeparture()
		{
			CombineAssertions(() =>
			{
				var provider = GetProvider();
				AssertNull(provider.CustomsOfficeOfEnquiryAtDeparture);

				var incomingMessage = CreateIncomingMessageToTest();
				provider = GetProvider();
				AssertEquals("Office of Enquiry", "IESNN456", provider.CustomsOfficeOfEnquiryAtDeparture);
			});
		}

		NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			nctsHeader.BH_JobReference = "B00000012";

			var message = (NCTSInboundEDIMessage)nctsHeader.Messages.AddNew(typeof(NCTSInboundEDIMessage));
			message.EM_LinkedObject = nctsHeader;
			message.EM_ApplicationCode = NCTSInboundEDIMessage.ApplicationCodes.IECustomsNCTS;
			message.EM_MessageType = NCTSIncomingMessageTypeList.Codes.IE140;
			message.EM_ReceiveTransmit = NCTSInboundEDIMessage.Direction.Receive;
			var text = InterchangeProcessorTestHelper.GetStandardCC140CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		public void TestHolderOfTheTransitProcedure()
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "IE12345678", "TIR123");
			AssertType<HolderOfTransitProcedureProvider>("HolderOfTheTransit", Provider.HolderOfTheTransitProcedure);
		}

		public void TestEnquiryTC11DeliveryDate()
		{
			var tc11Date = new ZDateTime(2023, 1, 10, 12, 15, 30);
			sendingAction.TC11DeliveryDate = tc11Date;

			var provider = GetProvider();
			AssertEquals("EnquiryDateTime", new DateTime(2023, 1, 10), provider.EnquiryTC11DeliveryDate);
		}

		public void TestEnquiryTC11DeliveryDate_Empty()
		{
			AssertNull("EnquiryDateTime is empty", Provider.EnquiryTC11DeliveryDate);
		}

		public void TestEnquiryTC11DeliveryDate_Invalid()
		{
			sendingAction.TC11DeliveryDate = ZDateTime.Invalid;
			var provider = GetProvider();
			AssertNull("EnquiryDateTime is empty", provider.EnquiryTC11DeliveryDate);
		}

		public void TestEnquiryText()
		{
			sendingAction.EnquiryText = "Enquiry Text";

			var provider = GetProvider();
			AssertEquals("Enquiry Text", "Enquiry Text", provider.EnquiryText);
		}

		public void TestConsignee()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZNTGRP";
			orgHeader.OH_FullName = "ZNET";
			var orgAddress = orgHeader.Addresses.AddNew();

			sendingAction.Consignee = "ZNTGRP";

			CombineAssertions(() =>
			{
				var provider = GetProvider();
				AssertNull("enquiry date and text are empty", provider.Consignee);

				sendingAction.EnquiryText = "Enquiry Text";
				provider = GetProvider();
				AssertNull("enquiry date is empty", provider.Consignee);

				sendingAction.TC11DeliveryDate = new ZDateTime(2023, 1, 10, 12, 15, 30);
				sendingAction.EnquiryText = ZString.Empty;
				provider = GetProvider();
				AssertNull("enquiry text is empty", provider.Consignee);

				sendingAction.EnquiryText = "Enquiry Text";
				provider = GetProvider();
				AssertType<PartyProvider>("PartyProvider", provider.Consignee);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			sendingAction = new NctsHeaderMessageSendingObject(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsHeaderMessageSendingObject sendingAction;
	}
}
