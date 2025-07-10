using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF415;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF415;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class RefundApplicationMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendRF415Message_UCC5() => TestSendRF415Message("V1", "IE5");

		public void TestSendRF415Message_UCC6() => TestSendRF415Message("V2", "IEI");

		void TestSendRF415Message(string je_ApplicationCode, string expectedEM_ApplicationCode)
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

			var sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
			sendingAction.MessageType = AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15;
			var sender = new RefundApplicationMessageSender(sendingAction);
			sender.Send();

			var createdMessage = (OutboundEDIMessage)entryHeader.Messages.Single();
			AssertEquals("Application code", expectedEM_ApplicationCode, createdMessage.EM_ApplicationCode);
			if (je_ApplicationCode == ImportDeclarationApplicationCodeList.Codes.V1)
			{
				var deserializedMessage = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<Rf415>(createdMessage.EM_MessageText);
				AssertNotNull(deserializedMessage);
				AssertEquals("12MRN345CDEFG678R9", deserializedMessage.Mrn);
			}
			else if (je_ApplicationCode == ImportDeclarationApplicationCodeList.Codes.V2)
			{
				var deserializedMessage = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<Rf415Type>(createdMessage.EM_MessageText);
				AssertNotNull(deserializedMessage);
				AssertEquals("12MRN345CDEFG678R9", deserializedMessage.Mrn);
			}
		}
	}
}
