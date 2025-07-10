using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportEntryInstructionLayouts))]
	sealed class ExportEntryInstructionLayoutsTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ExportEntryInstructionControlBag.Instance.ExportControlNumberTextBox, ControlWidthClass.Auto);
				yield return (ExportEntryInstructionControlBag.Instance.AwbOrBillNumberTextBox, ControlWidthClass.Auto);
				yield return (ExportEntryInstructionControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.ValueTypeDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Auto);
				yield return (ExportEntryInstructionControlBag.Instance.DeclarationCargoTypeDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.TradeTypePanel, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.WeightzCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.ContainerCountCalcEdit, ControlWidthClass.Auto);
				yield return (ExportEntryInstructionControlBag.Instance.LoadingConfirmationIsRequiredCheckBox, ControlWidthClass.Auto);
				yield return (ExportEntryInstructionControlBag.Instance.PreInspectedCargoDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CustomsInspectionCodeTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ExportEntryInstructionControlBag.Instance.VanningLocationsGroupBox, ControlWidthClass.LongControl);
			}
		}

		public void TestVisibility()
		{
			var declaration = Factory.New<Business.JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("Export Control Number should be visible when SEA", true, Layout.IsVisible(ExportEntryInstructionControlBag.Instance.ExportControlNumberTextBox, entryInstruction));
			AssertEquals("Awb or Bill Number should not be visible when SEA", false, Layout.IsVisible(ExportEntryInstructionControlBag.Instance.AwbOrBillNumberTextBox, entryInstruction));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Export Control Number should not be visible when AIR", false, Layout.IsVisible(ExportEntryInstructionControlBag.Instance.ExportControlNumberTextBox, entryInstruction));
			AssertEquals("Awb or Bill Number should be visible when AIR", true, Layout.IsVisible(ExportEntryInstructionControlBag.Instance.AwbOrBillNumberTextBox, entryInstruction));
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportEntryInstructionLayoutBuilder();

		PanelLayout Layout => layout ??= new ExportEntryInstructionLayouts().Layout;
		PanelLayout layout;
	}
}
