using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.EndpointManagement.GUI;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.EndpointManagement.Module
{
	public class EdiTrustedSystemController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public EdiTrustedSystemController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.EdiTrustedSystem; }
		}

		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.EdiTrustedSystem; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return TypeOfTopLevelBusinessObjectOverride ?? typeof(EdiTrustedSystem); }
		}

		public Type TypeOfTopLevelBusinessObjectOverride { get; set; }

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EdiTrustedSystemForm((EdiTrustedSystem)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.EdiTrustedSystem;
		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.EdiTrustedSystem;
		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.EdiTrustedSystem;
		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.EdiTrustedSystem;
	}
}
