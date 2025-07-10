using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	public abstract class DeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected virtual DeclarationMessageSendingObject GetNewMessageSendingObjectForTesting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobDeclarationMessageType;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "EntryInstructionDescription";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_CL = entryHeader.AllEntryLines.AddNew().PK;
			return CreateNewMessageSendingObject(entryHeader);
		}

		protected abstract DeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader);

		public virtual void TestDefaultMessageType()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("Default Message Type", ExpectedDefaultMessageType, sendingObject.MessageType);
		}

		public void TestMessageTypesList()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertContainsExactElementsInAnyOrder(ExpectedMessageTypeCodes, sendingObject.MessageTypesList.GetAllCodes());
		}

		public void TestGetMessageTypeForEDIMessage()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("MessageTypeForEDIMessage", ExpectedMessageTypeForEDIMessage, sendingObject.GetMessageTypeForEDIMessage());
		}

		public void TestEntryInstructionDescription()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("EntryInstructionDescription", "EntryInstructionDescription", sendingObject.EntryInstructionDescription);
		}

		public void TestMessageAttachee()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertType<CusEntryHeader>(sendingObject.MessageAttachee);
			AssertEquals("MessageAttachee", sendingObject.Header, sendingObject.MessageAttachee);
		}

		protected abstract ZString ExpectedDefaultMessageType { get; }
		protected abstract IReadOnlyList<string> ExpectedMessageTypeCodes { get; }
		protected abstract ZString ExpectedMessageTypeForEDIMessage { get; }
		protected abstract ZString ExpectedFriendlyNameForMessageManager { get; }
		protected abstract ZString JobDeclarationMessageType { get; }

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject() => GetNewMessageSendingObjectForTesting();

		#endregion
	}
}
