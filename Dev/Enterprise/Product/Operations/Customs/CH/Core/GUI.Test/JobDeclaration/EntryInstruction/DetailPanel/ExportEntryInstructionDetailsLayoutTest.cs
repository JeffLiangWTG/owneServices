using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ExportEntryInstructionDetailsLayout))]
sealed class ExportEntryInstructionDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EntryInstructionBasicDetailsControlBag.Instance.DetailsLabel, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionsDetailsControlBag.Instance.ProcedureCodeDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionsDetailsControlBag.Instance.NextProcedureDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionsDetailsControlBag.Instance.PartialDeliveryCheckBox, ControlWidthClass.Long);
			yield return (EntryInstructionsDetailsControlBag.Instance.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
		}
	}

	public void TestSubStyleDropEditVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("Export / Declaration Time invisible", false, Layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, entryInstruction));

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("ExportDeclarationActivation / Declaration Time visible", true, Layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, entryInstruction));
	});

	public PanelLayout Layout => layout ?? (layout = new ExportEntryInstructionDetailsLayout().Layout);
	PanelLayout layout;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
}
