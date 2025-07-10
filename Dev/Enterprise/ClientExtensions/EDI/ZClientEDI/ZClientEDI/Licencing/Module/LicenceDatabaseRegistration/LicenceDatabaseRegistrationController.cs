using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceDatabaseRegistrationController : ZSingletonController
	{
		public override ControllerID ID => ClientControllerRegistration.LicenceDatabaseRegistration;

		public override Type TypeOfTopLevelBusinessObject => typeof(LicenceDatabase);

		public override ModuleIdentifier ModuleID => null;

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			return new LicenceDatabaseRegistrationWizardForm(wizard);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => null;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails;

		protected override ODisplayMode GetDisplayModeForNew() => ODisplayMode.Browse;

		#endregion Implementation
	}
}
