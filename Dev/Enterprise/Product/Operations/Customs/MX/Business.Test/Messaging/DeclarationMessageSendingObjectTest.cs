using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DeclarationMessageSendingObject))]
	public class DeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		DeclarationMessageSendingObject GetNewMessageSendingObjectForTesting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "EntryInstructionDescription";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_CL = entryHeader.AllEntryLines.AddNew().PK;
			return CreateNewMessageSendingObject(entryHeader);
		}

		DeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new DeclarationMessageSendingObject(entryHeader);

		public void TestMessageTypesList()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertContainsExactElementsInAnyOrder(ExpectedMessageTypeCodes, sendingObject.MessageTypesList.GetAllCodes());
		}

		string[] ExpectedMessageTypeCodes => new string[] { "NEW", "DEL", "CAN", "PRE", "GLO" };

		public void TestMessageType_ReadOnly()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("Message Type should not be Read Only", false, sendingObject.MessageTypeInfo.ReadOnly);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject() => GetNewMessageSendingObjectForTesting();

		#endregion
	}
}
