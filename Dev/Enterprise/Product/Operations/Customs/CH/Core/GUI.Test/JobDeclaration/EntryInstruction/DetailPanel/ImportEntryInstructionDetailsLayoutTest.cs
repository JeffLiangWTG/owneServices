using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ImportEntryInstructionDetailsLayout))]
sealed class ImportEntryInstructionDetailsLayoutTest : LayoutsAbstractTest
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
			yield return (EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			yield return (EntryInstructionsDetailsControlBag.Instance.ReasonDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.AssessmentDateEdit, ControlWidthClass.Long);
		}
	}

	public PanelLayout Layout => layout ??= new ImportEntryInstructionDetailsLayout().Layout;
	PanelLayout layout;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
}
