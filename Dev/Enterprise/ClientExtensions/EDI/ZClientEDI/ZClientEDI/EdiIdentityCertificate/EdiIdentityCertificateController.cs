using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IdentityCertificate
{
	public class EdiIdentityCertificateController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.EdiIdentityCertificate;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.EdiIdentityCertificate;

		public override Type TypeOfTopLevelBusinessObject => typeof(EdiIdentityCertificate);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.EdiIdentityCertificates;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.EdiIdentityCertificatesNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.EdiIdentityCertificates;

		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.EdiIdentityCertificates;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotImplementedException();
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return ShowLoadedForm(sourceEntity, FormAction.Edit);
		}
	}
}
