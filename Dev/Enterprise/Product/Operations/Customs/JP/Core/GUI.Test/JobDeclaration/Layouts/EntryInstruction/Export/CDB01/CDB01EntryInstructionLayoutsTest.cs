using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CDB01EntryInstructionLayouts))]
	sealed class CDB01EntryInstructionLayoutsTest : LayoutsAbstractTest
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
				yield return (CDB01EntryInstructionControlBag.Instance.CDB01BillNumberUserControl, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.DateForDutyDateEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.WeightzCalcDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionControlBag.Instance.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.CDB01CargoTypeDropEdit, ControlWidthClass.Auto);
				yield return (ECREntryInstructionControlBag.Instance.SpecialCargoCodeFindBox, ControlWidthClass.Auto);
				yield return (ExportEntryInstructionControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.CDB01PermitNumberTextBox, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.CDB01MoveInUserControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonDeclarationControlBag.Instance.AllEntryInsSeparatorUserControl, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.MAWBTextBox, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.PortOfLoadingPanel, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.FinalDestinationPanel, ControlWidthClass.Auto);
				yield return (CDB01EntryInstructionControlBag.Instance.ExternalBrokerGroupBox, ControlWidthClass.LongNoCaption);
				yield return (CDB01EntryInstructionControlBag.Instance.AirCargoAgentGroupBox, ControlWidthClass.LongNoCaption);
				yield return (CDB01EntryInstructionControlBag.Instance.ForwarderGroupBox, ControlWidthClass.LongNoCaption);
				yield return (CDB01EntryInstructionControlBag.Instance.CarrierGroupBox, ControlWidthClass.LongNoCaption);
			}
		}

		protected override int ControlBagCount => 5;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CDB01EntryInstructionLayoutBuilder();
	}
}
