using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class TransportBorderControlBag : ControlBag
	{
		public TransportBorderControlBag()
		{
			BorderTransportModeDropEdit = RegisterControl(nameof(TransportBorderUserControl.BorderTransportModeDropEdit));
			BorderTransportTypeOfIdDropEdit = RegisterControl(nameof(TransportBorderUserControl.BorderTransportTypeOfIdDropEdit));
			BorderTransportIdAndNationalityUserControl = RegisterControl(nameof(TransportBorderUserControl.BorderTransportIdAndNationalityUserControl));
			BorderConveyanceNumberTextBox = RegisterControl(nameof(TransportBorderUserControl.BorderConveyanceNumberTextBox));
			BorderOfficeDropEdit = RegisterControl(nameof(TransportBorderUserControl.BorderOfficeDropEdit));
			AdditionalTransportBorderUserControl = RegisterControl(nameof(TransportBorderUserControl.AdditionalTransportBorderUserControl));
		}

		public static TransportBorderControlBag Instance => instance ?? (instance = new TransportBorderControlBag());

		[ThreadStatic]
		static TransportBorderControlBag instance;

		public ControlReference BorderTransportModeDropEdit { get; }

		public ControlReference BorderTransportTypeOfIdDropEdit { get; }

		public ControlReference BorderTransportIdAndNationalityUserControl { get; }

		public ControlReference BorderConveyanceNumberTextBox { get; }

		public ControlReference BorderOfficeDropEdit { get; }

		public ControlReference AdditionalTransportBorderUserControl { get; }

		protected override Control CreateTemplate() => new TransportBorderUserControl();
	}
}
