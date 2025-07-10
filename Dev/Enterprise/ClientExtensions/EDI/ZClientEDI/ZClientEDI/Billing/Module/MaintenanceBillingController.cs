using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business.Maintenance;
using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class MaintenanceBillingController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.MaintenanceBilling; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MaintenanceBilling); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MaintenanceBillingForm((MaintenanceBilling)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new MaintenanceBilling(Factory);
		}

		protected override ZArchitecture.Core.ODisplayMode GetDisplayModeForNew()
		{
			return ZArchitecture.Core.ODisplayMode.NewSaved;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
