using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class OrganisationDetailsControlBag : ControlBag
	{
		OrganisationDetailsControlBag()
		{
			ConsigneeOrganisationControl = RegisterControl(nameof(OrganisationDetailsUserControl.ConsigneeOrganisationControl));
			SupplierOrganisationControl = RegisterControl(nameof(OrganisationDetailsUserControl.SupplierOrganisationControl));
		}

		public static OrganisationDetailsControlBag Instance => instance ??= new OrganisationDetailsControlBag();

		[ThreadStatic]
		static OrganisationDetailsControlBag instance;

		public ControlReference ConsigneeOrganisationControl { get; }
		public ControlReference SupplierOrganisationControl { get; }

		protected override Control CreateTemplate() => new OrganisationDetailsUserControl();
	}
}
