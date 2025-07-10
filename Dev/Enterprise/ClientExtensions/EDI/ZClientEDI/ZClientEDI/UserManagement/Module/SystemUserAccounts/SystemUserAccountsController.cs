using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Client.EDI.UserManagement.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	class SystemUserAccountsController : ZSingletonController
	{
		public override ControllerID ID => ClientControllerRegistration.SystemUserAccounts;

		public override Type TypeOfTopLevelBusinessObject => typeof(EdiCustomerUserAccount);

		public override ModuleIdentifier ModuleID => null;

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var wizard = new SystemUserAccountsWizard(Factory);
			return new SystemUserAccountsWizardForm(wizard);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => null;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.SystemUserAccountsView;

		protected override ODisplayMode GetDisplayModeForNew() => ODisplayMode.Browse;

		#endregion Implementation
	}
}
