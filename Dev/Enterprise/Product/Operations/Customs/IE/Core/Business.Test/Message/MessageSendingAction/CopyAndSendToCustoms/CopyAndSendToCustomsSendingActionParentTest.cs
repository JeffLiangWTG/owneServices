using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CopyAndSendToCustomsSendingActionParent))]
	sealed class CopyAndSendToCustomsSendingActionParentTest : CusEntryHeaderMessageSendingActionParentTest<CopyAndSendToCustomsSendingActionParent, CopyAndSendToCustomsSendingAction>
	{
		protected override Type ExpectedSendingObjectCollectionType => typeof(CopyAndSendToCustomsSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			return new CopyAndSendToCustomsSendingActionParent(declaration, AESOutgoingMessageTypeList.Codes.ExportOriginal);
		}
	}
}
