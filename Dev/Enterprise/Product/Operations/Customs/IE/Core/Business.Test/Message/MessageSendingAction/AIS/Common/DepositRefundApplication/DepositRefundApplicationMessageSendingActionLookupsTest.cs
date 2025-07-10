using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class DepositRefundApplicationMessageSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestExportMovementReferenceNumberList()
		{
			AssertEquals("IE23AB987, IE23CD123", lookups.ExportMovementReferenceNumberList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var previousDocument = entryInstruction.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "MRN";
			previousDocument.CSI_ReferenceNumber = "IE23AB987";
			var previousDocument2 = entryInstruction.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "MRN";
			previousDocument2.CSI_ReferenceNumber = "IE23CD123";
			sendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);
			lookups = sendingAction.Lookups;
		}
		DepositRefundApplicationMessageSendingAction sendingAction;
		DepositRefundApplicationMessageSendingActionLookups lookups;
	}
}
