using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.IdentityTenant.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IdentityTenant
{
	public class EdiIdentityTenantController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.EdiIdentityTenant;
		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.EdiIdentityTenant;
		public override Type TypeOfTopLevelBusinessObject => typeof(EdiIdentityTenant);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;
		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.EdiIdentityTenant;
		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.EdiIdentityTenantNew;
		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.EdiIdentityTenantEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EdiIdentityTenantForm(businessEntity as EdiIdentityTenant);
		}
	}
}
