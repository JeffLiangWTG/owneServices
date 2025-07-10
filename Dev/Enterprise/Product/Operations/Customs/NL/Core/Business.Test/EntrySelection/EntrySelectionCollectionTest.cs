using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(EntrySelectionCollection))]
sealed class EntrySelectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntrySelectionCollection>
{
	public void TestAllowNew()
	{
		AssertEquals(false, GetCollectionToTest().AllowNew);
	}

	public void TestAllowRemove()
	{
		AssertEquals(false, GetCollectionToTest().AllowRemove);
	}

	public void TestCreateNonPersistentBusinessObject()
	{
		AssertExceptionThrown<InvalidOperationException>(() => GetCollectionToTest().AddNew());
	}

	public void TestPopulateCollectionForSupplement()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction4 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction5 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction6 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction7 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction8 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction9 = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader4 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader5 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader6 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader7 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader8 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader9 = declaration.ActiveEntryHeaders.AddNew();

		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
		entryHeader3.CH_Status = NLConstants.StatusNew.ReminderReceived;
		entryHeader3.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader3.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryInstruction4.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader4.CH_CEI_Instruction = entryInstruction4.PK;
		entryHeader4.CH_Status = NLConstants.StatusNew.ReminderReceived;
		entryHeader4.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader4.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryInstruction5.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader5.CH_CEI_Instruction = entryInstruction5.PK;
		entryHeader5.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader5.CH_EntryStatus = NLConstants.EntryStatusNew.Amended;
		entryHeader5.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryInstruction6.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader6.CH_CEI_Instruction = entryInstruction6.PK;
		entryHeader6.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader6.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;

		entryInstruction7.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader7.CH_CEI_Instruction = entryInstruction7.PK;
		entryHeader7.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader7.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._514;

		entryInstruction8.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader8.CH_CEI_Instruction = entryInstruction8.PK;
		entryHeader8.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader8.CH_EntryStatus = NLConstants.EntryStatusNew.Received;
		entryHeader8.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._583;

		entryInstruction9.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		entryHeader9.CH_CEI_Instruction = entryInstruction9.PK;
		entryHeader9.CH_Status = "XXX";
		entryHeader9.CH_EntryStatus = "YYY";
		entryHeader9.CH_PhaseStatus = "ZZZ";

		declaration.EntrySelections.PopulateCollectionForSupplement();
		AssertEquals("Only the correct statusses should be in the collection", 8, declaration.EntrySelections.Count);
	}

	public void TestPopulateCollectionForSupplement_InstructionSubStyle()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("No EntrySelection should be created when Instruction.SubStyle = A", 0, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("An EntrySelection should be created when Instruction.SubStyle = B", 1, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("An EntrySelection should be created when Instruction.SubStyle = C", 1, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("No EntrySelection should be created when Instruction.SubStyle = D", 0, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("An EntrySelection should be created when Instruction.SubStyle = E", 1, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("An EntrySelection should be created when Instruction.SubStyle = F", 1, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("No EntrySelection should be created when Instruction.SubStyle = U", 0, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("No EntrySelection should be created when Instruction.SubStyle = V", 0, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("No EntrySelection should be created when Instruction.SubStyle = X", 0, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("No EntrySelection should be created when Instruction.SubStyle = Y", 0, declaration.EntrySelections.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic;
			declaration.EntrySelections.PopulateCollectionForSupplement();
			AssertEquals("No EntrySelection should be created when Instruction.SubStyle = Z", 0, declaration.EntrySelections.Count);
		});
	}

	public void TestPopulateCollectionForSupplement_RemoveBeforeAddingNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;
		entryHeader1.EntryNumber = "TestEntryNum1";

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader2.EntryNumber = "TestEntryNum2";

		declaration.EntrySelections.PopulateCollectionForCancelSupplement();
		AssertNotNull("For cancel", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum1"));

		declaration.EntrySelections.PopulateCollectionForSupplement();
		AssertNotNull("For supplement", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum2"));
	}

	public void TestPopulateCollectionForCancelSupplement()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader4 = declaration.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;

		entryHeader2.CH_Status = NLConstants.StatusNew.Error;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;

		entryHeader3.CH_Status = NLConstants.StatusNew.Invalid;
		entryHeader3.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;

		entryHeader4.CH_Status = "XXX";
		entryHeader4.CH_PhaseStatus = "YYY";

		declaration.EntrySelections.PopulateCollectionForCancelSupplement();
		AssertEquals("Only the correct statusses should be in the collection", 3, declaration.EntrySelections.Count);
	}

	public void TestPopulateCollectionForCancelAmendment_RemoveBeforeAddingNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader1.EntryNumber = "TestEntryNum1";

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader2.EntryNumber = "TestEntryNum2";

		declaration.EntrySelections.PopulateCollectionForAmendment();
		AssertNotNull("For amendment", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum2"));

		declaration.EntrySelections.PopulateCollectionForCancelAmendment();
		AssertNotNull("For cancel", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum1"));
	}

	public void TestPopulateCollectionForAmendment_RemoveBeforeAddingNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader1.EntryNumber = "TestEntryNum1";

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader2.EntryNumber = "TestEntryNum2";

		declaration.EntrySelections.PopulateCollectionForCancelAmendment();
		AssertNotNull("For cancel", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum1"));

		declaration.EntrySelections.PopulateCollectionForAmendment();
		AssertNotNull("For amendment", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum2"));
	}

	public void TestPopulateCollectionForCancelAmendment()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader4 = declaration.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;

		entryHeader2.CH_Status = NLConstants.StatusNew.Rejection;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;

		entryHeader3.CH_Status = NLConstants.StatusNew.Invalid;
		entryHeader3.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;

		entryHeader4.CH_Status = "XXX";
		entryHeader4.CH_PhaseStatus = "YYY";

		declaration.EntrySelections.PopulateCollectionForCancelAmendment();
		AssertEquals("Only the correct statusses should be in the collection", 3, declaration.EntrySelections.Count);
	}

	public void TestPopulateCollectionForCancelAmendmentt_RemoveBeforeAddingNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader1.EntryNumber = "TestEntryNum1";

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader2.EntryNumber = "TestEntryNum2";

		declaration.EntrySelections.PopulateCollectionForAmendment();
		AssertNotNull("For amendment", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum2"));

		declaration.EntrySelections.PopulateCollectionForCancelAmendment();
		AssertNotNull("For cancel", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum1"));
	}

	public void TestPopulateCollectionForCRE()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader4 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader5 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader6 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader7 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader8 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader9 = declaration.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader1.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryHeader2.CH_Status = NLConstants.StatusNew.ReminderReceived;
		entryHeader2.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryHeader3.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader3.CH_EntryStatus = NLConstants.EntryStatusNew.Amended;
		entryHeader3.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryHeader4.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader4.CH_EntryStatus = NLConstants.EntryStatusNew.DocumentsControl;
		entryHeader4.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryHeader5.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader5.CH_EntryStatus = NLConstants.EntryStatusNew.PhysicalInspection;
		entryHeader5.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryHeader6.CH_Status = NLConstants.StatusNew.ReminderReceived;
		entryHeader6.CH_EntryStatus = NLConstants.EntryStatusNew.ProvisionalRelease;
		entryHeader6.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		entryHeader7.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader7.CH_EntryStatus = NLConstants.EntryStatusNew.Received;
		entryHeader7.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._583;

		entryHeader8.CH_Status = NLConstants.StatusNew.Invalid;
		entryHeader8.CH_EntryStatus = NLConstants.EntryStatusNew.DeclarationRejected;
		entryHeader8.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._514;

		entryHeader9.CH_Status = "XXX";
		entryHeader9.CH_EntryStatus = "YYY";
		entryHeader9.CH_PhaseStatus = "ZZZ";

		declaration.EntrySelections.PopulateCollectionForCRE();
		AssertEquals("Only the correct statusses should be in the collection", 8, declaration.EntrySelections.Count);
	}

	public void TestPopulateCollectionForCRE_RemoveBeforeAddingNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;
		entryHeader1.EntryNumber = "TestEntryNum1";

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader2.EntryNumber = "TestEntryNum2";

		declaration.EntrySelections.PopulateCollectionForCancelCRE();
		AssertNotNull("For cancel", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum1"));

		declaration.EntrySelections.PopulateCollectionForCRE();
		AssertNotNull("For CRE", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum2"));
	}

	public void TestPopulateCollectionForCancelCRE()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader3 = declaration.ActiveEntryHeaders.AddNew();
		var entryHeader4 = declaration.ActiveEntryHeaders.AddNew();

		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;

		entryHeader2.CH_Status = NLConstants.StatusNew.Error;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;

		entryHeader3.CH_Status = NLConstants.StatusNew.Invalid;
		entryHeader3.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;

		entryHeader4.CH_Status = "XXX";
		entryHeader4.CH_PhaseStatus = "YYY";

		declaration.EntrySelections.PopulateCollectionForCancelCRE();
		AssertEquals("Only the correct statusses should be in the collection", 3, declaration.EntrySelections.Count);
	}

	public void TestPopulateCollectionForCancelCRE_RemoveBeforeAddingNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.CH_Status = ZString.Empty;
		entryHeader1.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;
		entryHeader1.EntryNumber = "TestEntryNum1";

		var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
		entryHeader2.CH_Status = NLConstants.StatusNew.Accepted;
		entryHeader2.CH_EntryStatus = NLConstants.EntryStatusNew.RequestForInformation;
		entryHeader2.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		entryHeader2.EntryNumber = "TestEntryNum2";

		declaration.EntrySelections.PopulateCollectionForCRE();
		AssertNotNull("For CRE", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum2"));

		declaration.EntrySelections.PopulateCollectionForCancelCRE();
		AssertNotNull("For cancel", declaration.EntrySelections.Cast<EntrySelection>().SingleOrDefault(x => x.EntryNumber == "TestEntryNum1"));
	}

	protected override EntrySelectionCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new EntrySelectionCollection(declaration);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new EntrySelection(entryHeader);
	}
}
