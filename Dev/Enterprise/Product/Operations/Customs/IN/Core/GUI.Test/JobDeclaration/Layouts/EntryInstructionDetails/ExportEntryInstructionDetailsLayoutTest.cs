using System.Collections.Generic;
using Enterprise.Customs.Common.IN;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportEntryInstructionDetailsLayout))]
sealed class ExportEntryInstructionDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestControlsVisibility()
	{
		var layout = GetLayout();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(false, layout.IsVisible(INBag.TotalContainerZIntEdit, instruction));
			AssertEquals(false, layout.IsVisible(INBag.LoosePackagesCalcDropEdit, instruction));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = INContainerModeList.Codes.Liquid;
			AssertEquals(false, layout.IsVisible(INBag.TotalContainerZIntEdit, instruction));
			AssertEquals(false, layout.IsVisible(INBag.LoosePackagesCalcDropEdit, instruction));

			declaration.JE_ContainerMode = INContainerModeList.Codes.ContainerisedAndPackaged;
			AssertEquals(true, layout.IsVisible(INBag.TotalContainerZIntEdit, instruction));
			AssertEquals(true, layout.IsVisible(INBag.LoosePackagesCalcDropEdit, instruction));
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBag.StyleDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.SubStyleDropEdit, ControlWidthClass.Long);
			yield return (INBag.PackagesQtyCalcDropEdit, ControlWidthClass.Long);
			yield return (INBag.LoosePackagesCalcDropEdit, ControlWidthClass.Long);
			yield return (INBag.TotalContainerZIntEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (INBag.TotalGrossWeightAndNetWeightGroupBox, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (INBag.ShippingBillOverrideUserControl, ControlWidthClass.LongNoCaption);
			yield return (INBag.RBIWaiverNumberTextBox, ControlWidthClass.Long);
			yield return (INBag.RBIWaiverDateEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsDetailsLayoutBuilder();

	EntryInstructionBasicDetailsControlBag CommonBag => EntryInstructionBasicDetailsControlBag.Instance;

	EntryInstructionDetailsControlBag INBag => EntryInstructionDetailsControlBag.Instance;

	PanelLayout GetLayout() => GetPanelLayoutProvider().Layout;

	IPanelLayoutProvider GetPanelLayoutProvider() => new ExportEntryInstructionDetailsLayout();
}
