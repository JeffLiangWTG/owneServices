using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObject))]
	sealed class JobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageType()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message Type", DataBoundResourceStrings.GetDataForProperty(sendingObject.MessageTypeInfo).Caption);
			AssertEquals("When Import", "275", sendingObject.MessageType);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When Export", "751", sendingObject.MessageType);
		}

		public void TestMessageTypeDescription()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message Type Description", DataBoundResourceStrings.GetDataForProperty(sendingObject.MessageTypeDescriptionInfo).Caption);
			AssertEquals("When Import", "Import Declaration Request", sendingObject.MessageTypeDescription);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When Export", "Export Declaration Request", sendingObject.MessageTypeDescription);
		}

		public void TestDeclarationType()
		{
			var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Entry Type", DataBoundResourceStrings.GetDataForProperty(sendingObject.DeclarationTypeInfo).Caption);
		}

		public void TestEntryStatus()
		{
			var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Entry Status", DataBoundResourceStrings.GetDataForProperty(sendingObject.EntryStatusInfo).Caption);
		}

		public void TestProcedure()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_FormattedProcedure = "1000A";

			var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Procedure", DataBoundResourceStrings.GetDataForProperty(sendingObject.ProcedureInfo).Caption);
			AssertEquals("1000A", sendingObject.Procedure);
		}

		public void TestEntryInstructionDescription()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Description = "Description";

			var sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Entry Type Description", DataBoundResourceStrings.GetDataForProperty(sendingObject.EntryInstructionDescriptionInfo).Caption);
			AssertEquals("Description", sendingObject.EntryInstructionDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new JobDeclarationMessageSendingObject(entryHeader);
		}

		#endregion
	}
}
