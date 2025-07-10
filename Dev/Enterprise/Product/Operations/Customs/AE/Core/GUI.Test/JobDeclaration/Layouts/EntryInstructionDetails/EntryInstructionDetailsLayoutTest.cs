using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsLayout))]
sealed class EntryInstructionDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionDetailsLayoutBuilder();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBag.StyleDropEdit, ControlWidthClass.Long);
			yield return (AEBag.TradeTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (AEBag.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (AEBag.ToWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (AEBag.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (AEBag.FromWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (CommonBag.BondHolderOrganisationControl, ControlWidthClass.LongNoCaption);
			yield return (CommonBag.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBag.AssessmentDateEdit, ControlWidthClass.Long);
			yield return (CommonBag.RemoverOrganisationControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (AEBag.DeclarationPurposeDropEdit, ControlWidthClass.Auto);
			yield return (AEBag.DeclarationPurposeDetailsTextBox, ControlWidthClass.Auto);
		}
	}

	EntryInstructionBasicDetailsControlBag CommonBag => EntryInstructionBasicDetailsControlBag.Instance;

	EntryInstructionDetailsControlBag AEBag => EntryInstructionDetailsControlBag.Instance;
}
