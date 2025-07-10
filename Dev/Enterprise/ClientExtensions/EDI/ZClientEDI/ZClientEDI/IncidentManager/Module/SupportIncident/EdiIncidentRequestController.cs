using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EdiIncidentRequestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SupportIncidentForm(((EdiIncidentRequest)businessEntity).RelatedSupportIncident);
		}

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.EdiIncidentRequest; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ClientModuleRegistration.SupportIncident; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EdiIncidentRequest); }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncidentEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncidentNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncidentView; }
		}

		#endregion
	}
}
