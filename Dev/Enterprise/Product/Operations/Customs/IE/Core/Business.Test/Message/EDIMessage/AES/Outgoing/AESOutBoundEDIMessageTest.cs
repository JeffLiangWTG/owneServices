using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(AESOutboundEDIMessage))]
	class AESOutboundEDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<AESOutboundEDIMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsExport, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
			});
		}

		public void TestMessageNumberPlaceHolderReplacement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<AESOutboundEDIMessage>();
			message.EM_MessageText = $"<GREETING>{CargoWise.Customs.IE.MessageContracts.Constants.AESMessageRecipientHolder}</GREETING>";
			entry.Messages.Add(message);
			Factory.Save();
			AssertEquals("message.EM_MessageText", $"<GREETING>{message.EM_MessageNum}</GREETING>", message.EM_MessageText);
		}

		public void TestUniqueBatchNumberPlaceHolderReplacement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<AESOutboundEDIMessage>();
			message.EM_MessageText = $"<GREETING>{CargoWise.Customs.IE.MessageContracts.Constants.AESMessageSenderHolder}</GREETING>";
			entry.Messages.Add(message);
			Factory.Save();
			AssertEquals("message.EM_MessageText", $"<GREETING>{companyCredential.GP_MailBoxID}</GREETING>", message.EM_MessageText);
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<AESOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IEMessageControlNumber sequense", "IEE00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<AESOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IEMessageControlNumber sequense", "IEE00000000000002", message2.EM_MessageNum);
		}

		[TestDate(2022, 11, 01)]
		public void TestSendersReferencePlaceHolderReplacement()
		{
			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var message1 = Factory.New<AESOutboundEDIMessage>();
			message1.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			message1.EM_MessageText = $"<ExportOriginal>{AESOutboundEDIMessage.LRNPlaceHolder}</ExportOriginal>";
			entry1.Messages.Add(message1);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var message2 = Factory.New<AESOutboundEDIMessage>();
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message2.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExitOriginal;
			message2.EM_MessageText = $"<ExitOriginal>{AESOutboundEDIMessage.LRNPlaceHolder}</ExitOriginal>";
			entry2.Messages.Add(message2);

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			var message3 = Factory.New<AESOutboundEDIMessage>();
			message3.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message3.EM_MessageType = AESOutgoingMessageTypeList.Codes.ReExport;
			message3.EM_MessageText = $"<ReExport>{AESOutboundEDIMessage.LRNPlaceHolder}</ReExport>";
			entry3.Messages.Add(message3);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Should replace with generated LRN for ExportOriginal", "<ExportOriginal>EDIDATBIE2200000001V01</ExportOriginal>", message1.EM_MessageText);
				AssertEquals("Should replace with generated LRN for ExitOriginal", "<ExitOriginal>EDIDATBIE2200000002V01</ExitOriginal>", message2.EM_MessageText);
				AssertEquals("Should replace with empty string for ReExport", "<ReExport>EDIDATBIE2200000003V01</ReExport>", message3.EM_MessageText);
			});
		}
	}
}
