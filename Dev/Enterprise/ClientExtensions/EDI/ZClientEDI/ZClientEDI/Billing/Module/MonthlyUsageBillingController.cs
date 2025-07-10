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
	public class MonthlyUsageBillingController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.MonthlyUsageBilling; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MonthlyUsageBilling); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MonthlyUsageBillingForm((MonthlyUsageBilling)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new MonthlyUsageBilling(Factory);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
