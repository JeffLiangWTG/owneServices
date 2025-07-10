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
	public class IncidentTriageChecklistItemController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.IncidentTriageChecklistItem;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.IncidentTriageChecklistItem;

		public override Type TypeOfTopLevelBusinessObject => typeof(IncidentTriageChecklistItem);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.CustomerServiceIncidentTriageChecklistsView;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.CustomerServiceIncidentTriageChecklistsNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.CustomerServiceIncidentTriageChecklistsEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new IncidentTriageChecklistItemForm((IncidentTriageChecklistItem)businessEntity);
		}
	}
}
