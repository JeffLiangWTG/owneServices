using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IdentityApplication
{
	internal class EdiIdentityApplicationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ClientModuleRegistration.EdiIdentityApplication;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.EdiIdentityApplication;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.EdiIdentityApplication);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EdiIdentityApplicationFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EdiIdentityApplicationFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EdiIdentityApplicationCollection(Factory);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems();

			var azureApplication = new ZMenuItem("New Azure Application", (object sender, EventArgs e) => ShowNewForm());
			var customerApplication = new ZMenuItem("New Customer Application", (object sender, EventArgs e) => ShowNewCustomerApplicationForm());

			NewMenuItem.MenuItems.Add(azureApplication);
			NewMenuItem.MenuItems.Add(customerApplication);

			return menuItems;
		}

		void ShowNewCustomerApplicationForm()
		{
			var controller = (EdiIdentityApplicationController)ZControllerFactory.Create(ClientControllerRegistration.EdiIdentityApplication);
			controller.ShowNewCustomerApplicationForm();
		}

		public override bool AllowNew => true;
		public override bool AllowEdit => true;
		public override bool AllowDelete => false;
		public override bool AllowView => true;
	}
}
