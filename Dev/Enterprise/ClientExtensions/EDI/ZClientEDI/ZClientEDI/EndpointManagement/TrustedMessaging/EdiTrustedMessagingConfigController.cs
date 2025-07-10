using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.EndpointManagement.GUI;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.EndpointManagement.Module
{
	public class EdiTrustedMessagingConfigController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public EdiTrustedMessagingConfigController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.EdiTrustedMessagingConfig; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.EdiTrustedMessagingConfig; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return TypeOfTopLevelBusinessObjectOverride ?? typeof(EdiTrustedMessagingConfig); }
		}

		public Type TypeOfTopLevelBusinessObjectOverride { get; set; }

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDITrustedMessagingConfigForm((EdiTrustedMessagingConfig)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = base.GetNewBusinessEntityInLocalFactory();
			((EdiTrustedMessagingConfig)result).ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;
			return result;
		}

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.EdiTrustedMessagingConfig;
		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.EdiTrustedMessagingConfig;
		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.EdiTrustedMessagingConfig;
		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.EdiTrustedMessagingConfig;
	}
}
