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
	public class IncidentManagementGroupController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.IncidentManagementGroup;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.IncidentManagementGroup;

		public override Type TypeOfTopLevelBusinessObject => typeof(IncidentManagementGroup);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security checkpoints
		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.CustomerServiceIncidentManagementGroupView;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.CustomerServiceIncidentManagementGroupNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.CustomerServiceIncidentManagementGroupEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new IncidentManagementGroupForm((IncidentManagementGroup)businessEntity);
		}
	}
}
