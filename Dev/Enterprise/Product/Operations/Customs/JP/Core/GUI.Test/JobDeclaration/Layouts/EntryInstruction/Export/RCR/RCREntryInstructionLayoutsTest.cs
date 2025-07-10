using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(RCREntryInstructionLayouts))]
	sealed class RCREntryInstructionLayoutsTest : LayoutsAbstractTest
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
				yield return (RCREntryInstructionControlBag.Instance.RCRActionDropEdit, ControlWidthClass.Auto);
				yield return (RCREntryInstructionControlBag.Instance.PreviousBillNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ExportControlNumberUserControl, ControlWidthClass.Auto);
				yield return (RCREntryInstructionControlBag.Instance.ViaLocationCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ExportCodeTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.ExportNameTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.DeclarantCodeTextBox, ControlWidthClass.Auto);
				yield return (CommonDeclarationControlBag.Instance.DeclarationReferenceTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
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

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ECREntryInstructionLayoutBuilder();
	}
}
