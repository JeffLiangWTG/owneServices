using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(EntrySelection))]
sealed class EntrySelectionTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidation()
	{
		AssertType<EntrySelectionValidation>(((EntrySelection)GetNewBusinessObject()).Validation);
	}

	public void TestEntryNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.EntryNumber = "TestEntryNum";
		var entrySelection = new EntrySelection(entryHeader);

		AssertEquals("Entry Number Value", "TestEntryNum", entrySelection.EntryNumber);
	}

	public void TestSetStatusToCusEntryHeaderForSUP()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_Status = "ACC";
		entryHeader.CH_PhaseStatus = "515";
		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForSUP();

		AssertEquals("New Status should be set", ZString.Empty, entryHeader.CH_Status);
		AssertEquals("New Phase Status should be set", CustomsEntryPhaseStatusList.Codes.SUP, entryHeader.CH_PhaseStatus);
	}

	public void TestSetStatusToCusEntryHeaderForSUP_PreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForSUP();

		CombineAssertions(() =>
		{
			AssertEquals("A new PreviousDocument should be created", 1, entryInstruction.PreviousDocuments.Count);
			AssertEquals("Qualifier of the PreviousDocument should be 'NMRN'", NLConstants.PreviousDocumentTypes.AcknowledgmentOfMRN, entryInstruction.PreviousDocuments[0].CSI_Code);
			AssertEquals("Reference of the PreviousDocument should be the MRN of the entryHeader", "MRN123", entryInstruction.PreviousDocuments[0].CSI_ReferenceNumber);
		});
	}

	public void TestSetStatusToCusEntryHeaderForSUP_InstructionSubStyle()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForSUP();

		CombineAssertions((() =>
		{
			AssertEquals("SubStyle B should be set to X", EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, entryInstruction.CEI_SubStyle);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
			entrySelection.SetStatusToCusEntryHeaderForSUP();
			AssertEquals("SubStyle C should be set to Y", EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, entryInstruction.CEI_SubStyle);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;
			entrySelection.SetStatusToCusEntryHeaderForSUP();
			AssertEquals("SubStyle E should be set to X", EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, entryInstruction.CEI_SubStyle);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
			entrySelection.SetStatusToCusEntryHeaderForSUP();
			AssertEquals("SubStyle F should be set to Y", EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, entryInstruction.CEI_SubStyle);
		}));
	}

	public void TestSetStatusToCusEntryHeaderForCancelSUP()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "515", ZDateTimeOffset.Now.AddDays(-1));
		entryHeader.Logs.AddNew(Events.MessageStatusChange, "ACC", ZDateTimeOffset.Now.AddDays(-1));

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "SUP", ZDateTimeOffset.Now);
		entryHeader.Logs.AddNew(Events.MessageStatusChange, ZString.Empty, ZDateTimeOffset.Now);

		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForCancelSUP();

		AssertEquals("New Status should be set", "ACC", entryHeader.CH_Status);
		AssertEquals("New Phase Status should be set", "515", entryHeader.CH_PhaseStatus);
	}

	public void TestSetStatusToCusEntryHeaderForAMD()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = "ACC";
		entryHeader.CH_PhaseStatus = "515";
		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForAMD();

		AssertEquals("New Status should be set", ZString.Empty, entryHeader.CH_Status);
		AssertEquals("New Phase Status should be set", CustomsEntryPhaseStatusList.Codes._513, entryHeader.CH_PhaseStatus);
	}

	public void TestSetStatusToCusEntryHeaderForCancelAMD()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "515", ZDateTimeOffset.Now.AddDays(-1));
		entryHeader.Logs.AddNew(Events.MessageStatusChange, "ACC", ZDateTimeOffset.Now.AddDays(-1));

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "513", ZDateTimeOffset.Now);
		entryHeader.Logs.AddNew(Events.MessageStatusChange, ZString.Empty, ZDateTimeOffset.Now);

		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForCancelAMD();

		AssertEquals("New Status should be set", "ACC", entryHeader.CH_Status);
		AssertEquals("New Phase Status should be set", "515", entryHeader.CH_PhaseStatus);
	}

	public void TestSetStatusToCusEntryHeaderForCRE()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = "ACC";
		entryHeader.CH_PhaseStatus = "515";
		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForCRE();

		AssertEquals("New Status should be set", ZString.Empty, entryHeader.CH_Status);
		AssertEquals("New Phase Status should be set", CustomsEntryPhaseStatusList.Codes.CRE, entryHeader.CH_PhaseStatus);
	}

	public void TestSetStatusToCusEntryHeaderForCancelCRE()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "515", ZDateTimeOffset.Now.AddDays(-1));
		entryHeader.Logs.AddNew(Events.MessageStatusChange, "ACC", ZDateTimeOffset.Now.AddDays(-1));

		entryHeader.Logs.AddNew(Events.PhaseStatusChange, "CRE", ZDateTimeOffset.Now);
		entryHeader.Logs.AddNew(Events.MessageStatusChange, ZString.Empty, ZDateTimeOffset.Now);

		var entrySelection = new EntrySelection(entryHeader);
		entrySelection.SetStatusToCusEntryHeaderForCancelCRE();

		AssertEquals("New Status should be set", "ACC", entryHeader.CH_Status);
		AssertEquals("New Phase Status should be set", "515", entryHeader.CH_PhaseStatus);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new EntrySelection(entryHeader);
	}
}
