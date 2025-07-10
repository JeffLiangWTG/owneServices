using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing;

[TestedType(typeof(FinalizeAVABREntryMessageSendingAction))]
public class FinalizeAVABREntryMessageSendingActionTest : NonPersistentBusinessObjectTestCase
{
	public void TestDeclarationType()
	{
		AssertEquals("AVABR", action.DeclarationType);
	}

	public void TestDescription()
	{
		AssertEquals("IPR - Billing", action.Description);
	}

	public void TestEntryStatus()
	{
		AssertEquals("TX8", action.EntryStatus);
	}

	public void TestRegistrationNumber()
	{
		AssertEquals("MRN1234", action.RegistrationNumber);
	}

	public void TestEntryInstruction()
	{
		AssertSame(entryInstruction, action.EntryInstruction);
	}

	public void TestCanSend()
	{
		entry.CH_EntryStatus = ZString.Empty;
		AssertEquals(true, action.CanSend);

		entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX8;
		AssertEquals(false, action.CanSend);
	}

	public void TestShouldSend_ReadOnly()
	{
		entry.CH_EntryStatus = ZString.Empty;
		AssertEquals("Action can be selected when status != TX8", false, action.ShouldSendInfo.ReadOnly);

		entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX8;
		AssertEquals("Cannot select for finalizing when status is TX8", true, action.ShouldSendInfo.ReadOnly);
	}

	public void TestShouldSend_Caption()
	{
		AssertEquals("Finalize?", DataBoundResourceStrings.GetDataForProperty(typeof(FinalizeAVABREntryMessageSendingAction), nameof(FinalizeAVABREntryMessageSendingAction.ShouldSend)).Caption);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new FinalizeAVABREntryMessageSendingAction(entry, actionParent);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		entry = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_Style = "AVABR";
		entryInstruction.CEI_Description = "IPR - Billing";

		entry.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX8;
		entry.CH_CEI_Instruction = entryInstruction.PK;

		entry.MovementReferenceNumberSetter("MRN1234");
		actionParent = new FinalizeAVABRMessageSendingActionParent(declaration);
		action = (FinalizeAVABREntryMessageSendingAction)GetNewBusinessObject();
	}

	FinalizeAVABREntryMessageSendingAction action;
	FinalizeAVABRMessageSendingActionParent actionParent;

	CusEntryHeader entry;
	CusEntryInstruction entryInstruction;
	JobDeclaration declaration;
}
