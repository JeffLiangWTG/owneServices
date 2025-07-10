using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceDatabaseController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public LicenceDatabaseController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.LicenceDatabase; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.LicenceDatabase; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(LicenceDatabase); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new LicenceDatabaseForm((LicenceDatabase)businessEntity, new LicenceDatabaseViewController());
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails; }
		}
	}
}
