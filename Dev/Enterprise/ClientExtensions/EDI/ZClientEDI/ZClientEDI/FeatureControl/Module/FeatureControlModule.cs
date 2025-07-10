using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.FeatureControl.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.FeatureControl.Module
{
	public class FeatureControlModule : ZFilterGridModule
	{
		#region Implementation
		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.FeatureControl;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.FeatureControl;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new FeatureControlController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new FeatureControlFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new FeatureControlFilterControl(GridCollection, (FeatureControlFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new FeatureControlHeaderCollection(Factory);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems())
			{
				new ZMenuItem("Export...", ExportMenuItem)
			};
			return result.ToArray();
		}

		void ExportMenuItem(object sender, EventArgs arg)
		{
			var checkpoint = EDISecurityCheckpoints.FeatureControlView;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			ZFormModaliser.ShowDialogAndDispose(new FeatureControlExportPopupForm(new FeatureControlExportBizObj()));
		}

		public override bool AllowDelete => false;

		#endregion
	}
}
