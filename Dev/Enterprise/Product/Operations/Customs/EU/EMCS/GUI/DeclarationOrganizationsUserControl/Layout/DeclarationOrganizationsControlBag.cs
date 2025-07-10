using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public sealed class DeclarationOrganizationsControlBag : ControlBag
	{
		public DeclarationOrganizationsControlBag()
		{
			OwnerDocAddressUserControl = RegisterControl(nameof(DeclarationOrganizationsUserControl.OwnerDocAddressUserControl));
			ConsigneeDocAddressControl = RegisterControl(nameof(DeclarationOrganizationsUserControl.ConsigneeDocAddressControl));
			ConsignorDocAddressControl = RegisterControl(nameof(DeclarationOrganizationsUserControl.ConsignorDocAddressControl));
		}

		public static DeclarationOrganizationsControlBag Instance => instance ?? (instance = new DeclarationOrganizationsControlBag());

		[ThreadStatic]
		static DeclarationOrganizationsControlBag instance;

		public ControlReference OwnerDocAddressUserControl { get; }

		public ControlReference ConsigneeDocAddressControl { get; }

		public ControlReference ConsignorDocAddressControl { get; }

		protected override Control CreateTemplate() => new DeclarationOrganizationsUserControl();
	}
}
