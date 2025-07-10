using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentTriageController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.IncidentTriage;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.IncidentTriage;

		public override Type TypeOfTopLevelBusinessObject => typeof(IncidentTriage);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.CustomerServiceIncidentTriageView;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.CustomerServiceIncidentTriageNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.CustomerServiceIncidentTriageEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new IncidentTriageForm((IncidentTriage)businessEntity);
		}
	}
}
