using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class TraderDetailsControlBag : ControlBag
	{
		public TraderDetailsControlBag()
		{
			PrincipalDocAddressControl = RegisterControl(nameof(TraderDetailsUserControl.PrincipalDocAddressControl));
			ConsignorDocAddressControl = RegisterControl(nameof(TraderDetailsUserControl.ConsignorDocAddressControl));
			ConsigneeDocAddressControl = RegisterControl(nameof(TraderDetailsUserControl.ConsigneeDocAddressControl));
			RepresentativeDocAddressControl = RegisterControl(nameof(TraderDetailsUserControl.RepresentativeDocAddressControl));
			FromWarehouseGroupBox = RegisterControl(nameof(TraderDetailsUserControl.FromWarehouseGroupBox));
		}

		public static TraderDetailsControlBag Instance => instance ?? (instance = new TraderDetailsControlBag());

		[ThreadStatic]
		static TraderDetailsControlBag instance;

		public ControlReference PrincipalDocAddressControl { get; }

		public ControlReference ConsignorDocAddressControl { get; }

		public ControlReference ConsigneeDocAddressControl { get; }

		public ControlReference RepresentativeDocAddressControl { get; }

		public ControlReference FromWarehouseGroupBox { get; }

		protected override Control CreateTemplate() => new TraderDetailsUserControl();
	}
}
