using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ShipmentTypeControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ShipmentTypeUserControl();

		public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

		[ThreadStatic]
		static ShipmentTypeControlBag instance;

		ShipmentTypeControlBag()
		{
			BorderTransportMeansDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.BorderTransportMeansDropEdit));
			MethodOfPaymentDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.MethodOfPaymentDropEdit));
		}

		public ControlReference BorderTransportMeansDropEdit;
		public ControlReference MethodOfPaymentDropEdit;
	}
}
