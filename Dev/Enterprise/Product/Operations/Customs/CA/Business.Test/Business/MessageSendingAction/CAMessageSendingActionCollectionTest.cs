using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAMessageSendingActionCollection))]
	sealed class CAMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CAMessageSendingActionCollection>
	{
		public void TestSendMessage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;

			CAMessageSendingActionCollection actions = new CAMessageSendingActionCollection(declaration, MessageSendingMessageType.Original);
			actions[0].CA_SendMessage = false;

			AssertEquals("SendMessage", false, actions.SendMessage);
			AssertEquals(ZDateTime.Empty, entry1.CH_EntrySubmittedDate);

			actions[0].CA_SendMessage = true;
			AssertEquals("SendMessage", true, actions.SendMessage);
			AssertNotNull("", entry1.CH_EntrySubmittedDate);
		}

		[TestDate(2007, 10, 12, 1, 23, 34)]
		public void TestGenerateEntrySubmittedDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			CAMessageSendingActionCollection actions = new CAMessageSendingActionCollection(declaration, MessageSendingMessageType.Original);

			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Messages generated for ens entry", true, messagesGenerated);

			AssertEquals(1, entry.Messages.Count);
			AssertEquals(ZDateTime.Now, entry.CH_EntrySubmittedDate);
		}

		public void TestDefaultCA_SendMessageIfThereIsOneElementPopulated()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CusEntryHeader dlmEntry = declaration.CustomsEntryHeaders.AddNew();
			dlmEntry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = dlmEntry.MergedLines.AddNew().PK;

			CAMessageSendingActionCollection actions = new CAMessageSendingActionCollection(declaration, MessageSendingMessageType.Original);
			AssertEquals("There should be only one element", 1, actions.Count);
			AssertEquals(dlmEntry, actions[0].entry);
			AssertEquals("CA_SendMessage is defaulted to true", true, actions[0].CA_SendMessage);
		}

		public void TestHasAtLeastOneToSendMessageFor()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CusEntryHeader dlmEntry = declaration.CustomsEntryHeaders.AddNew();
			dlmEntry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;

			CusEntryHeader dlmEntry2 = declaration.CustomsEntryHeaders.AddNew();
			dlmEntry2.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;

			CAMessageSendingActionCollection coll = new CAMessageSendingActionCollection(declaration, MessageSendingMessageType.Original);
			AssertEquals("HasAtLeastOneToSendMessageFor", false, coll.HasAtLeastOneToSendMessageFor);

			CAMessageSendingAction dlmAction = coll.FindFirstElement(MessageType.DataLoadingModule);
			AssertNotNull(dlmAction);
			dlmAction.CA_SendMessage = true;
			AssertEquals("HasAtLeastOneToSendMessageFor", true, coll.HasAtLeastOneToSendMessageFor);
		}

		public void TestPopulateElementsAndFind()
		{
			CusEntryHeader dlmEntry = Declaration.CustomsEntryHeaders.AddNew();
			dlmEntry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;

			CAMessageSendingActionCollection collection = new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original);

			CAMessageSendingAction dlmAction = collection.FindFirstElement(MessageType.DataLoadingModule);
			AssertNotNull(dlmAction);
			AssertEquals("dlmAction is wrapping ens entry", dlmEntry, dlmAction.entry);
		}

		public void TestAllowNew()
		{
			CAMessageSendingActionCollection collection = new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original);
			AssertEquals("Users do not add a new element to this collection in the grid", false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			CAMessageSendingActionCollection collection = new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original);
			AssertEquals("Users cannot remove an element from this collection in the grid", false, collection.AllowRemove);
		}

		protected override CAMessageSendingActionCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CAMessageSendingActionCollection coll = this.Coll;
			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			return new CAMessageSendingAction(entry, MessageType.DataLoadingModule, coll);
		}

		JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fDeclaration;

		CAMessageSendingActionCollection Coll
		{
			get { return fColl ?? (fColl = new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original)); }
		}
		CAMessageSendingActionCollection fColl;
	}
}
