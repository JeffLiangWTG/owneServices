using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(WarehouseLayout))]
	sealed class WarehouseLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new WarehouseLayoutBuilder<NctsDepartureCargoDesc>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (WarehouseControlBag.Instance.PartGuidFindBox, ControlWidthClass.Auto);
				yield return (WarehouseControlBag.Instance.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (WarehouseControlBag.Instance.WarehouseEntryNumberTextBox, ControlWidthClass.Auto);
				yield return (WarehouseControlBag.Instance.WarehouseEntryLineNoCalcEdit, ControlWidthClass.Auto);
				yield return (WarehouseControlBag.Instance.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
				yield return (WarehouseControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);
			}
		}
	}
}
