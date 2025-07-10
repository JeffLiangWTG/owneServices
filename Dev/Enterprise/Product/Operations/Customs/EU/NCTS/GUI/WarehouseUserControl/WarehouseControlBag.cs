using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class WarehouseControlBag : ControlBag
	{
		WarehouseControlBag()
		{
			PartGuidFindBox = RegisterControl(nameof(WarehouseUserControl.PartGuidFindBox));
			BondedWhsQuantityCalcDropEdit = RegisterControl(nameof(WarehouseUserControl.BondedWhsQuantityCalcDropEdit));
			WarehouseEntryNumberTextBox = RegisterControl(nameof(WarehouseUserControl.WarehouseEntryNumberTextBox));
			WarehouseEntryLineNoCalcEdit = RegisterControl(nameof(WarehouseUserControl.WarehouseEntryLineNoCalcEdit));
			BondedWHSOrderNumberTextBox = RegisterControl(nameof(WarehouseUserControl.BondedWHSOrderNumberTextBox));
			BondedWHSOrderLineNumberCalcEdit = RegisterControl(nameof(WarehouseUserControl.BondedWHSOrderLineNumberCalcEdit));
		}

		public static WarehouseControlBag Instance => instance ?? (instance = new WarehouseControlBag());

		[ThreadStatic]
		static WarehouseControlBag instance;

		public ControlReference PartGuidFindBox { get; }

		public ControlReference BondedWhsQuantityCalcDropEdit { get; }

		public ControlReference WarehouseEntryNumberTextBox { get; }

		public ControlReference WarehouseEntryLineNoCalcEdit { get; }

		public ControlReference BondedWHSOrderNumberTextBox { get; }

		public ControlReference BondedWHSOrderLineNumberCalcEdit { get; }

		protected override Control CreateTemplate() => new WarehouseUserControl();
	}
}
