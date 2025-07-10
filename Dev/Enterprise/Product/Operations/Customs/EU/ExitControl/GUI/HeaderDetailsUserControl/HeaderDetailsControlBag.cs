using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class HeaderDetailsControlBag : ControlBag
	{
		public HeaderDetailsControlBag()
		{
			BranchGuidFindBox = RegisterControl(nameof(HeaderDetailsUserControl.BranchGuidFindBox));
			BrokerCodeFindBox = RegisterControl(nameof(HeaderDetailsUserControl.BrokerCodeFindBox));
			ExporterOrgAddressControl = RegisterControl(nameof(HeaderDetailsUserControl.ExporterOrgAddressControl));
			CarrierAddressWithContactControl = RegisterControl(nameof(HeaderDetailsUserControl.CarrierAddressWithContactControl));
		}

		public static HeaderDetailsControlBag Instance => instance ??= new HeaderDetailsControlBag();

		[ThreadStatic]
		static HeaderDetailsControlBag instance;

		public ControlReference BranchGuidFindBox { get; }

		public ControlReference BrokerCodeFindBox { get; }

		public ControlReference ExporterOrgAddressControl { get; }

		public ControlReference CarrierAddressWithContactControl { get; }

		protected override Control CreateTemplate() => new HeaderDetailsUserControl();
	}
}
