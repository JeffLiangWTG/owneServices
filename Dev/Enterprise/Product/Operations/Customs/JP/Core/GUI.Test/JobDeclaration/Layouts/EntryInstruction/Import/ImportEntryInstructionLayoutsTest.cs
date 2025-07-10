using System.Collections.Generic;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportEntryInstructionLayouts))]
	sealed class ImportEntryInstructionLayoutsTest : LayoutsAbstractTest
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
				yield return (EntryInstructionControlBag.Instance.ValueTypeDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Auto);
				yield return (ImportEntryInstructionControlBag.Instance.BondedLocationCodeFindBox, ControlWidthClass.Auto);
				yield return (ImportEntryInstructionControlBag.Instance.BondedLocationNameTextBox, ControlWidthClass.Long);
				yield return (ImportEntryInstructionControlBag.Instance.BeforePermitApplicationReasonDropEdit, ControlWidthClass.Auto);
				yield return (ImportEntryInstructionControlBag.Instance.DeclarationCargoTypeDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.TradeTypePanel, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.WeightzCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.ContainerCountCalcEdit, ControlWidthClass.Auto);
				yield return (ImportEntryInstructionControlBag.Instance.ContentInspectionResultDropEdit, ControlWidthClass.Auto);
				yield return (ImportEntryInstructionControlBag.Instance.DutyDrawbackDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CustomsInspectionCodeTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ImportEntryInstructionControlBag.Instance.SpecialDeclarationOfficeGroupBox, ControlWidthClass.LongControl);
			}
		}

		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.J;
			AssertEquals("Special Declaration Office Groupbox should be visible when CEI_Style is J, P or R", true, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.SpecialDeclarationOfficeGroupBox, entryInstruction));
			AssertEquals("DeclarationTypeDropEdit", true, Layout.IsVisible(EntryInstructionControlBag.Instance.DeclarationTypeDropEdit, entryInstruction));
			AssertEquals("BondedLocationCodeFindBox", false, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.BondedLocationCodeFindBox, entryInstruction));
			AssertEquals("BondedLocationNameTextBox", false, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.BondedLocationNameTextBox, entryInstruction));
			AssertEquals("BeforePermitApplicationReasonDropEdit", true, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.BeforePermitApplicationReasonDropEdit, entryInstruction));
			AssertEquals("DeclarationCargoTypeDropEdit", true, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.DeclarationCargoTypeDropEdit, entryInstruction));
			AssertEquals("TradeTypePanel", true, Layout.IsVisible(EntryInstructionControlBag.Instance.TradeTypePanel, entryInstruction));
			AssertEquals("CargoQuantityCalcDropEdit", true, Layout.IsVisible(EntryInstructionControlBag.Instance.CargoQuantityCalcDropEdit, entryInstruction));
			AssertEquals("WeightzCalcDropEdit", true, Layout.IsVisible(EntryInstructionControlBag.Instance.WeightzCalcDropEdit, entryInstruction));
			AssertEquals("CustomsWeightCalcDropEdit", true, Layout.IsVisible(EntryInstructionControlBag.Instance.CustomsWeightCalcDropEdit, entryInstruction));
			AssertEquals("ContainerCountCalcEdit", true, Layout.IsVisible(EntryInstructionControlBag.Instance.ContainerCountCalcEdit, entryInstruction));
			AssertEquals("DutyDrawbackDropEdit", true, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.DutyDrawbackDropEdit, entryInstruction));
			AssertEquals("CustomsInspectionCodeTextBox", true, Layout.IsVisible(EntryInstructionControlBag.Instance.CustomsInspectionCodeTextBox, entryInstruction));
			entryInstruction.CEI_Style = JPImportDeclarationTypeList.Codes.A;
			AssertEquals("Special Declaration Office Groupbox should not be visible when CEI_Style is not J, P or R", false, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.SpecialDeclarationOfficeGroupBox, entryInstruction));
			AssertEquals("BondedLocationCodeFindBox", true, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.BondedLocationCodeFindBox, entryInstruction));
			AssertEquals("BondedLocationNameTextBox", true, Layout.IsVisible(ImportEntryInstructionControlBag.Instance.BondedLocationNameTextBox, entryInstruction));
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ImportEntryInstructionLayoutBuilder();

		PanelLayout Layout => layout ?? (layout = new ImportEntryInstructionLayouts().Layout);

		PanelLayout layout;
	}
}
