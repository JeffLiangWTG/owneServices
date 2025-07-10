using System;
using CargoWise.EntityFramework;
using Enterprise.CRM.Common;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CRM.Module
{
	public class CrmOpportunityController : ZController
	{
		public override ControllerID ID => ControllerIDs.CrmOpportunity;

		public override ModuleIdentifier ModuleID => ModuleIDs.CrmOpportunity;

		public override Type TypeOfTopLevelBusinessObject => typeof(CrmOpportunity);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var opp = sourceEntity as CrmOpportunity;
			WebUrlLauncher.Launch(opp.GlowLink);
			return null;
		}
	}
}
