using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class StlBillingController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.StlBilling; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StlBilling); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new StlBillingForm((StlBilling)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new StlBilling(Factory);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
