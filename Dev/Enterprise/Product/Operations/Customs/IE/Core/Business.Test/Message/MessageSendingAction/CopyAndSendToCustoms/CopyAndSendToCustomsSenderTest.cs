using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC515C;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CopyAndSendToCustomsSenderTest : TestCaseWithFactory
	{
		public void TestSend_Original()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var credential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			credential.GP_MailBoxID = "MS12345";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				(var xmlObject, var message) = AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader);
				AssertEquals("MessageSender", "MS12345", xmlObject.MessageSender);
				AssertEquals("MessageIdentification", message.EM_MessageNum, xmlObject.MessageIdentification);
			});
		}

		(T xmlObj, AESOutboundEDIMessage message) AssertSend<T>(string messageType, CusEntryHeader entryHeader)
			where T : IAESMessageXmlObject
		{
			var sendingAction = new CopyAndSendToCustomsSendingAction(entryHeader);
			sendingAction.MessageType = messageType;

			var sender = new CopyAndSendToCustomsSender(sendingAction);

			var createdMessage = (AESOutboundEDIMessage)sender.Send();
			Factory.Save();
			var result = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<T>(createdMessage.EM_MessageText);
			AssertNotNull(result);

			return (result, createdMessage);
		}
	}
}
