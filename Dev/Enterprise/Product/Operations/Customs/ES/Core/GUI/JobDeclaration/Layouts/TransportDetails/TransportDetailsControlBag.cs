using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class TransportDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new TransportDetailsUserControl();

		public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

		[ThreadStatic]
		static TransportDetailsControlBag instance;

		TransportDetailsControlBag()
		{
			MasterBillAndIATAUserControl = RegisterControl(nameof(TransportDetailsUserControl.MasterBillAndIATAUserControl));
			TransportInlandRailUserControl = RegisterControl(nameof(TransportDetailsUserControl.TransportInlandRailUserControl));
		}

		public ControlReference MasterBillAndIATAUserControl { get; }
		public ControlReference TransportInlandRailUserControl { get; }
	}
}
