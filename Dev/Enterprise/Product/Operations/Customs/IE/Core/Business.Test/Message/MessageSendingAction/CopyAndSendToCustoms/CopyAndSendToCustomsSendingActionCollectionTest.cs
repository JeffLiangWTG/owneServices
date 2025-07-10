using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CopyAndSendToCustomsSendingActionCollection))]
	sealed class CopyAndSendToCustomsSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CopyAndSendToCustomsSendingActionCollection>
	{
		public override void TestAdd()
		{
			Assert("AESMessageSendingActionCollection does not support adding.", true);
		}

		public override void TestDelete()
		{
			Assert("AESMessageSendingActionCollection does not support deleting.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("AESMessageSendingActionCollection does not support RemoveFromRelationship.", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("AESMessageSendingActionCollection haschanges as default", true);
		}

		protected override CopyAndSendToCustomsSendingActionCollection GetCollectionToTest()
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
			var sendingAction = new CopyAndSendToCustomsSendingActionParent(declaration, AESOutgoingMessageTypeList.Codes.ExportOriginal);
			return (CopyAndSendToCustomsSendingActionCollection)sendingAction.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;
	}
}
