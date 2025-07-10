using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GlobalChargeCodeOrganizationModule : ZFilterGridModule
	{
		public GlobalChargeCodeOrganizationModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlobalChargeCodeOrganization; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlobalChargeCodeOrganization);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlobalChargeCodeOrganizationFilterControl(GridCollection, (GlobalChargeCodeOrganizationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlobalChargeCodeMapOrganizationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlobalChargeCodeOrganizationFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewActionMenuItems());
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("Enterprise.Accounting.Business.GlobalChargeCode.OrganizationChargeCodeMapping", "Organization Charge Code Mapping"), ShowIntercompanyChargeCodeMappingForm));
			return menu.ToArray();
		}

		void ShowIntercompanyChargeCodeMappingForm(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new OrganizationChargeCodeMappingForm(new BusinessObjectFactory()));
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GlobalChargeCodeOrganization; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
