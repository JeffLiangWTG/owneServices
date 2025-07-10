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
	public class GlobalChargeCodeIntercompanyModule : ZFilterGridModule
	{
		public GlobalChargeCodeIntercompanyModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlobalChargeCodeIntercompany; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlobalChargeCodeIntercompany);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlobalChargeCodeIntercompanyFilterControl(GridCollection, (GlobalChargeCodeIntercompanyFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlobalChargeCodeMapIntercompanyCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlobalChargeCodeIntercompanyFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewActionMenuItems());
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("Enterprise.Accounting.Business.GlobalChargeCode.IntercompanyChargeCodeMapping", "Intercompany Charge Code Mapping"), ShowIntercompanyChargeCodeMappingForm));
			return menu.ToArray();
		}

		void ShowIntercompanyChargeCodeMappingForm(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new IntercompanyChargeCodeMappingForm(new BusinessObjectFactory()));
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GlobalChargeCodeIntercompany; }
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
