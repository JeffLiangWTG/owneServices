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
	public class IncidentDiagnosticCriteriaController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.IncidentDiagnosticCriteria;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.IncidentDiagnosticCriteria;

		public override Type TypeOfTopLevelBusinessObject => typeof(IncidentDiagnosticCriteria);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.CustomerServiceIncidentDiagnosticCriteriaView;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.CustomerServiceIncidentDiagnosticCriteriaNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.CustomerServiceIncidentDiagnosticCriteriaEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new IncidentDiagnosticCriteriaForm((IncidentDiagnosticCriteria)businessEntity);
		}
	}
}
