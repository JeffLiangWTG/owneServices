using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSOutboundEDIMessage))]
	sealed class NCTSOutboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<NCTSOutboundEDIMessage>();
			AssertEquals("EM_ReceiveTransmit", NCTSOutboundEDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", NCTSOutboundEDIMessage.ApplicationCodes.IECustomsNCTS, message.EM_ApplicationCode);
		}

		public void TestLinkedDepartureMovementHeader()
		{
			var message = Factory.New<NCTSOutboundEDIMessage>();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.Messages.Add(message);
			AssertSame(message.LinkedDepartureMovementHeader, movementHeader);
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<NCTSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IEMessageControlNumber sequense", "IEN00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<NCTSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IEMessageControlNumber sequense", "IEN00000000000002", message2.EM_MessageNum);
		}

		public void TestCorrectlyTypeDeciding()
		{
			var message = Factory.New<NCTSOutboundEDIMessage>();
			Factory.Save();
			AssertType<NCTSOutboundEDIMessage>("Using base EDIMessage", NewFactory().Load<Enterprise.Messaging.Business.EDIMessage>(message.PK));
			AssertType<NCTSOutboundEDIMessage>("Using IE EDIMessage", NewFactory().Load<IE.Business.EDIMessage>(message.PK));
		}

		[TestDate(2023, 02, 28)]
		public void TestSendersReferencePlaceHolderReplacement()
		{
			var company = MessageTestHelper.CreateCompany(Factory);
			var branch = MessageTestHelper.CreateBranch(company);
			var nctsHeader1 = Factory.New<NctsHeader>();
			nctsHeader1.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader1.BH_GB = branch.PK;

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader2.BH_GB = branch.PK;

			var message1 = Factory.New<NCTSOutboundEDIMessage>();
			message1.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message1.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			message1.EM_MessageText = $"<LRN>{NCTSOutboundEDIMessage.LRNPlaceHolder}</LRN>";
			nctsHeader1.Messages.Add(message1);

			var message2 = Factory.New<NCTSOutboundEDIMessage>();
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message2.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			message2.EM_MessageText = $"<LRN>{NCTSOutboundEDIMessage.LRNPlaceHolder}</LRN>";
			nctsHeader2.Messages.Add(message2);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Should replace message 1 placeholder with generated LRN", "<LRN>EDIDATIEB2300000001V01</LRN>", message1.EM_MessageText);
				AssertEquals("Should replace message 2 placeholder with generated LRN", "<LRN>EDIDATIEB2300000002V01</LRN>", message2.EM_MessageText);
			});

			var message1amend = Factory.New<NCTSOutboundEDIMessage>();
			message1amend.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message1amend.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment;
			message1amend.EM_MessageText = $"<LRN>{NCTSOutboundEDIMessage.LRNPlaceHolder}</LRN>";
			nctsHeader1.Messages.Add(message1amend);

			nctsHeader2.MovementHeader.BM_PaperlessInbondNum = "EDIDATIEB2300000002V01";
			var message2v2 = Factory.New<NCTSOutboundEDIMessage>();
			message2v2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message2v2.EM_MessageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData;
			message2v2.EM_MessageText = $"<LRN>{NCTSOutboundEDIMessage.LRNPlaceHolder}</LRN>";
			nctsHeader2.Messages.Add(message2v2);

			Factory.Save();

			AssertEquals("Amendment Message should use same LRN as original", "<LRN>EDIDATIEB2300000001V01</LRN>", message1amend.EM_MessageText);
			AssertEquals("Declaration Message should replace placeholder with generated LRN v2", "<LRN>EDIDATIEB2300000002V02</LRN>", message2v2.EM_MessageText);
		}

		public void TestMessageNumberPlaceHolderReplacement()
		{
			var company = MessageTestHelper.CreateCompany(Factory);
			var branch = MessageTestHelper.CreateBranch(company);
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_GB = branch.PK;

			var message = Factory.New<NCTSOutboundEDIMessage>();
			message.EM_MessageText = $"<GREETING>{CargoWise.Customs.IE.MessageContracts.Constants.NCTSMessageRecipientHolder}</GREETING>";
			nctsHeader.Messages.Add(message);
			Factory.Save();
			AssertEquals("message.EM_MessageText", $"<GREETING>{message.EM_MessageNum}</GREETING>", message.EM_MessageText);
		}

		public void TestUniqueBatchNumberPlaceHolderReplacement()
		{
			var company = MessageTestHelper.CreateCompany(Factory);
			var branch = MessageTestHelper.CreateBranch(company);
			var companyCredential = IE.Messaging.Testing.InterchangeProcessorTestHelper.CreateValidCredential(company);
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_GB = branch.PK;

			var message = Factory.New<NCTSOutboundEDIMessage>();
			message.EM_MessageText = $"<GREETING>{CargoWise.Customs.IE.MessageContracts.Constants.NCTSMessageSenderHolder}</GREETING>";
			nctsHeader.Messages.Add(message);
			Factory.Save();
			AssertEquals("message.EM_MessageText", $"<GREETING>{companyCredential.GP_MailBoxID}</GREETING>", message.EM_MessageText);
		}
	}
}
