using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ExportEntryMessageSendingAction))]
sealed class ExportEntryMessageSendingActionTest : BEJobDeclarationMessageSendingObjectTest<ExportEntryMessageSendingAction>
{
	public void TestShouldSend_ReadOnly()
	{
		CombineAssertions(() =>
		{
			EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.RetrospectiveLodgementOfAnExportReExportDeclaration;
			AssertEquals("ShouldSend should be read only when SubType is R", true, action.ShouldSendInfo.ReadOnly);
			EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForDeclarationsBOrE;
			AssertEquals("ShouldSend should be editable when SubType is not R", false, action.ShouldSendInfo.ReadOnly);
		});
	}

	public void TestExportCustomsOffice()
	{
		Entry.Declaration.JE_CustomsOffice = "Office3";
		AssertEquals("Office3", action.ExportCustomsOffice);
	}

	public void TestExitType_ReadOnly()
	{
		Assert(action.ExitTypeInfo.ReadOnly);
	}

	public void TestExitCustomsOffice_MaxLength()
	{
		AssertEquals(10, action.ExitCustomsOfficeInfo.MaxLength);
	}

	public void TestExitCustomsOffice_ReadOnly()
	{
		AssertEquals(false, action.ExitCustomsOfficeInfo.ReadOnly);
	}

	public void TestSecurityType_MaxLength()
	{
		AssertEquals(1, action.SecurityTypeInfo.MaxLength);
	}

	public void TestSecurityType_ReadOnly()
	{
		AssertEquals(false, action.SecurityTypeInfo.ReadOnly);
	}

	public void TestExitDate_ReadOnly()
	{
		Assert(action.ExitDateInfo.ReadOnly);
	}

	public void TestAnnotation_Caption()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ExportEntryMessageSendingAction), nameof(ExportEntryMessageSendingAction.Annotation), false, attribute => attribute.Caption == "Annotation");
	}

	public void TestAnnotation_MaxLength()
	{
		AssertEquals(512, action.AnnotationInfo.MaxLength);
	}

	public void TestAnnotation_ReadOnly()
	{
		Assert(action.AnnotationInfo.ReadOnly);
	}

	public void TestJustification_Caption()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ExportEntryMessageSendingAction), nameof(ExportEntryMessageSendingAction.Justification), false, attribute => attribute.Caption == "Justification");
	}

	public void TestJustification_MaxLength()
	{
		AssertEquals(512, action.JustificationInfo.MaxLength);
	}

	public void TestJustification_NotReadOnly()
	{
		Assert(!action.JustificationInfo.ReadOnly);
	}

	public void TestJustificationWithTypeOfEntry()
	{
		action.TypeOfEntry = BEExportEntryTypeList.Codes.CancellationRequest;
		action.Justification = "TestJustificationText";
		CombineAssertions(() =>
		{
			AssertEquals("Justification text is available", "TestJustificationText", action.Justification);
			action.TypeOfEntry = BEExportEntryTypeList.Codes.ExportAmendment;
			AssertEquals("Justification text is cleared", ZString.Empty, action.Justification);
		});
	}

	public void TestDefaultExitCustomsOfficeFromDeclaration()
	{
		AssertEquals("ECO", action.ExitCustomsOffice);
	}

	protected override void SetUp()
	{
		base.SetUp();

		Entry.Declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "ECO");
		action = (ExportEntryMessageSendingAction)GetNewBusinessObject();
	}
	ExportEntryMessageSendingAction action;
}
