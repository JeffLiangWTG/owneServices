using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Client.EDI.UserManagement.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class EdiUserAgreementController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.UserAgreements;
		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.UserAgreements;
		public override Type TypeOfTopLevelBusinessObject => typeof(EdiUserAgreement);

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var agreement = (EdiUserAgreement)businessEntity;
			var agreementForm = new EdiUserAgreementForm(agreement);

			return agreementForm;
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;
		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.UserAgreements;
		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.UserAgreementsNew;
		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.UserAgreementsEdit;
		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.UserAgreementsDelete;

#endregion
			}
}
