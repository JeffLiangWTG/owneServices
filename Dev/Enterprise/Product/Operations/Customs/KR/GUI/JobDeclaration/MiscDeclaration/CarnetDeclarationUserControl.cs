using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class CarnetDeclarationUserControl : ZUserControl
	{
		public CarnetDeclarationUserControl()
		{
			InitializeComponent();

			SupplierOrganisationControl.OrgAddressFormatter = GetAddressFormatter();
			ImporterOrganisationControl.OrgAddressFormatter = GetAddressFormatter();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CarnetDetailsPanel.UpdateLayout(new CarnetDetailsLayout());
			CustomsAreaDetailsPanel.UpdateLayout(new CarnetCustomsAreaDetailsLayout());
			MiscellaneousOptionsPanel.UpdateLayout(new CarnetMiscellaneousOptionsLayout());
		}
		Func<BusinessObjectFactory, OrgAddress, AddressFormatter> GetAddressFormatter()
		{
			return (factory, orgAddress) => new KRAddressFormatter(factory, orgAddress);
		}
	}
}
