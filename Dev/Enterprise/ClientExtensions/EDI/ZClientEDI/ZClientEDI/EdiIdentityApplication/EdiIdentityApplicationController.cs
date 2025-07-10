using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityApplication.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IdentityApplication
{
	public class EdiIdentityApplicationController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.EdiIdentityApplication;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.EdiIdentityApplication;

		public override Type TypeOfTopLevelBusinessObject => typeof(EdiIdentityApplication);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.EdiIdentityApplication;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.EdiIdentityApplicationNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.EdiIdentityApplicationEdit;

		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.EdiIdentityApplication;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var application = businessEntity as EdiIdentityApplication;

			return application.IsCustomerApplication ? new EdiIdentityCustomerApplicationForm(application) : new EdiIdentityApplicationForm(application);
		}

		internal IZForm ShowNewCustomerApplicationForm()
		{
			var application = (EdiIdentityApplication)GetNewBusinessEntityInLocalFactory();
			application.IsCustomerApplication = true;

			return ShowFormForNewEntity(application);
		}
	}
}
