using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD415;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RD415;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class DepositRefundApplicationMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendRD415Message_UCC5() => TestSendRD415Message(ImportDeclarationApplicationCodeList.Codes.V1, EDIMessage.ApplicationCodes.IECustomsUCC5Import);

		public void TestSendRD415Message_UCC6() => TestSendRD415Message(ImportDeclarationApplicationCodeList.Codes.V2, EDIMessage.ApplicationCodes.IECustomsImport);

		void TestSendRD415Message(string je_ApplicationCode, string expectedEM_ApplicationCode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = je_ApplicationCode;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.MovementReferenceNumberSetter("12MRN345CDEFG678R9");

			var sendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);
			sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtR15;
			var sender = new DepositRefundApplicationMessageSender(sendingAction);
			sender.Send();

			var createdMessage = (OutboundEDIMessage)entryHeader.Messages.Single();
			AssertEquals("Application code", expectedEM_ApplicationCode, createdMessage.EM_ApplicationCode);
			if(je_ApplicationCode == ImportDeclarationApplicationCodeList.Codes.V1)
			{
				var deserializedMessage = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<Rd415>(createdMessage.EM_MessageText);
				AssertNotNull(deserializedMessage);
			}
			else if(je_ApplicationCode == ImportDeclarationApplicationCodeList.Codes.V2)
			{
				var deserializedMessage = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<Rd415Type>(createdMessage.EM_MessageText);
				AssertNotNull(deserializedMessage);
			}
		}
	}
}
