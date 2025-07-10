using System.Collections.Generic;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ECREntryInstructionLayouts))]
	sealed class ECREntryInstructionLayoutsTest : LayoutsAbstractTest
	{
		public void TestSpecialCargoCodeCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var layout = new ECREntryInstructionLayouts().Layout;
			layout.TryGetCaption(ECREntryInstructionControlBag.Instance.SpecialCargoCodeFindBox, entryInstruction, out var specialCargoCodeResourceStringData);
			AssertEquals("Dangerous Goods", specialCargoCodeResourceStringData.Caption);
		}

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (ECREntryInstructionControlBag.Instance.CusEntryInstructionNSITextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ExportControlNumberUserControl, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ExportCodeTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ExportNameTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.DeclarantCodeTextBox, ControlWidthClass.Auto);
				yield return (ExportEntryInstructionControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (ECREntryInstructionControlBag.Instance.CustomsVolumeCalcDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonDeclarationControlBag.Instance.DeclarationReferenceTextBox, ControlWidthClass.Auto);
				yield return (ECREntryInstructionControlBag.Instance.ECRNotesTextBox, ControlWidthClass.Auto);
				yield return (ECREntryInstructionControlBag.Instance.SpecialCargoCodeFindBox, ControlWidthClass.Auto);
				yield return (ECREntryInstructionControlBag.Instance.ECRCargoTypeDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonDeclarationControlBag.Instance.AllEntryInsSeparatorUserControl, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.CarrierCodeCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.VesselNameCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.VoyageFlightNoBoundTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.DateOfArrivalBoundDateEdit, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ExportDateBoundDateEdit, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.BookingNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ReceiptModeDropEdit, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.DeliveryModeDropEdit, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 4;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ECREntryInstructionLayoutBuilder();
	}
}
