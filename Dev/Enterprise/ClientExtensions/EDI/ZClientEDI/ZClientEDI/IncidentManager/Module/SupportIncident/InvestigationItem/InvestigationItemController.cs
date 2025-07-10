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
	public class InvestigationItemController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.InvestigationItem;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.InvestigationItem;

		public override Type TypeOfTopLevelBusinessObject => typeof(InvestigationItem);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.CustomerServiceInvestigationItemView;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.CustomerServiceInvestigationItemNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.CustomerServiceInvestigationItemEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new InvestigationItemForm((InvestigationItem)businessEntity);
		}
	}
}
