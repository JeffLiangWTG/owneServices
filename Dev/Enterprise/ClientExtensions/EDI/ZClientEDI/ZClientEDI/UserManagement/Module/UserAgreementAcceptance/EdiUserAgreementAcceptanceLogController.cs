using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class EdiUserAgreementAcceptanceLogController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.UserAgreementAcceptances;
		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.UserAgreementAcceptances;
		public override Type TypeOfTopLevelBusinessObject => typeof(EdiUserAgreementAcceptanceLog);

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion
	}
}