using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public sealed class TransportAndPackagingControlBag : ControlBag
	{
		public TransportAndPackagingControlBag()
		{
			TransportMethodOfPaymentDropEdit = RegisterControl(nameof(TransportAndPackagingUserControl.TransportMethodOfPaymentDropEdit));
			CarrierDocAddressControl = RegisterControl(nameof(TransportAndPackagingUserControl.CarrierDocAddressControl));
			PortOfPresentationCodeFindBox = RegisterControl(nameof(TransportAndPackagingUserControl.PortOfPresentationCodeFindBox));
			ChargePaymentOrDestinationIDDropEdit = RegisterControl(nameof(TransportAndPackagingUserControl.ChargePaymentOrDestinationIDDropEdit));
		}

		public static TransportAndPackagingControlBag Instance => instance ?? (instance = new TransportAndPackagingControlBag());

		[ThreadStatic]
		static TransportAndPackagingControlBag instance;

		public ControlReference TransportMethodOfPaymentDropEdit { get; }

		public ControlReference CarrierDocAddressControl { get; }

		public ControlReference PortOfPresentationCodeFindBox { get; }

		public ControlReference ChargePaymentOrDestinationIDDropEdit { get; }

		protected override Control CreateTemplate() => new TransportAndPackagingUserControl();
	}
}
