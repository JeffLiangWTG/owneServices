using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class ShipmentTypeControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentTypeUserControl();

		public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

		[ThreadStatic]
		static ShipmentTypeControlBag instance;

		ShipmentTypeControlBag()
		{
			DeclarantTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.DeclarantTypeDropEdit));
			OperationTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.OperationTypeDropEdit));
			SpecialTransportDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.SpecialTransportDropEdit));
			IsMultimodalCheckBox = RegisterControl(nameof(ShipmentTypeUserControl.IsMultimodalCheckBox));
			DispatchModalityDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.DispatchModalityDropEdit));
			BRTransportModeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.BRTransportModeDropEdit));
		}

		public ControlReference DeclarantTypeDropEdit;
		public ControlReference OperationTypeDropEdit;
		public ControlReference SpecialTransportDropEdit;
		public ControlReference IsMultimodalCheckBox;
		public ControlReference DispatchModalityDropEdit;
		public ControlReference BRTransportModeDropEdit;
	}
}
