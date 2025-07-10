using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsBasicLayout))]
	sealed class EntryInstructionDetailsBasicLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EntryInstructionDetailsControlBag.Instance.DateForDutyDateEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.FormattedProcedureDropEdit, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.ToWarehouseAddressControl, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.FromWarehouseAddressControl, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.AutonomyRegionTypeDropEdit, ControlWidthClass.Long);
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EntryInstructionBasicDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
				yield return (EntryInstructionDetailsControlBag.Instance.PackagesQtyCalcDropEdit, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
	}
}
