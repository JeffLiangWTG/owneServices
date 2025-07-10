using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class TransportAndPackagingControlBag : ControlBag
	{
		public TransportAndPackagingControlBag()
		{
			TransportMethodOfPaymentDropEdit = RegisterControl(nameof(TransportAndPackagingUserControl.TransportMethodOfPaymentDropEdit));
			CarrierDocAddressControl = RegisterControl(nameof(TransportAndPackagingUserControl.CarrierDocAddressControl));
		}

		public static TransportAndPackagingControlBag Instance => instance ?? (instance = new TransportAndPackagingControlBag());

		[ThreadStatic]
		static TransportAndPackagingControlBag instance;

		public ControlReference TransportMethodOfPaymentDropEdit { get; }

		public ControlReference CarrierDocAddressControl { get; }

		protected override Control CreateTemplate() => new TransportAndPackagingUserControl();
	}
}
