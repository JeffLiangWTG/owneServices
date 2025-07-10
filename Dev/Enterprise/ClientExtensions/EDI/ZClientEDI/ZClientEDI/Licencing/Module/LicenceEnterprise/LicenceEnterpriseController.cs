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
	public class LicenceEnterpriseController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public LicenceEnterpriseController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.LicenceEnterprise; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.LicenceEnterprise; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return TypeOfTopLevelBusinessObjectOverride ?? typeof(LicenceEnterprise); }
		}

		public Type TypeOfTopLevelBusinessObjectOverride { get; set; }

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new LicenceEnterpriseForm((LicenceEnterprise)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.OrgLicenceModify; }
		}
	}
}
