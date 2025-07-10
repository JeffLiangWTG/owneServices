using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPBillPartiesControlBag : ControlBag
	{
		public static JPBillPartiesControlBag Instance => instance ??= new JPBillPartiesControlBag();

		[ThreadStatic]
		static JPBillPartiesControlBag instance;

		JPBillPartiesControlBag()
		{
			ConsigneeRegNoPanel = RegisterControl("ConsigneeRegNoPanel");
			ShipperRegNoPanel = RegisterControl("ShipperRegNoPanel");
			NotifyPartyRegNoPanel = RegisterControl("NotifyPartyRegNoPanel");
		}

		protected override Control CreateTemplate() => new JPManifestBillPartiesSpecificUserControl();

		public ControlReference ConsigneeRegNoPanel { get; }
		public ControlReference ShipperRegNoPanel { get; }
		public ControlReference NotifyPartyRegNoPanel { get; }
	}
}
