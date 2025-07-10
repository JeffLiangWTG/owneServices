using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class SupportIncidentController : ZController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.SupportIncident; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return Modules.ClientModuleRegistration.SupportIncident; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(SupportIncident); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var form = new SupportIncidentForm((SupportIncident)businessEntity);
			if (OnFormShown != null)
			{
				form.Shown += OnFormShown;
			}
			return form;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return ShowEditFormAccordingToDialogResult(sourceEntity);
		}

		IZForm ShowEditFormAccordingToDialogResult(BusinessObject sourceEntity, ControlledSupportIncidentDialog.Result defaultAnswer = ControlledSupportIncidentDialog.Result.None)
		{
			var incident = (SupportIncident)sourceEntity;
			var groupLink = Factory.LoadTop1<IncidentManagementLink>(new ZQuery(IncidentManagementLinkSchema.INL_IM_Incident, incident.PK));
			IZForm form = null;

			if ((groupLink?.IsControlled ?? false) && !IsFormShownFor(incident))
			{
				using (var dialog = new ControlledSupportIncidentDialog(groupLink, defaultAnswer))
				{
					var dialogAnswer = dialog.ShowDialog();
					switch (dialogAnswer)
					{
						case ControlledSupportIncidentDialog.Result.View:
							form = this.ShowViewForm(incident);
							break;

						case ControlledSupportIncidentDialog.Result.Edit:
							form = base.ShowEditForm(incident);
							break;

						case ControlledSupportIncidentDialog.Result.None:
							form = null;
							break;

						case ControlledSupportIncidentDialog.Result.OpenGroup:
							IncidentGroupController.ShowEditForm(groupLink.IncidentManagementGroup);
							break;

						default:
							throw new InvalidOperationException($"The dialog returned an unexpected result: {dialogAnswer.ToString()}");
					}
				}
			}
			else
			{
				form = base.ShowEditForm(sourceEntity);
			}

			return form;
		}

		IncidentManagementGroupController IncidentGroupController
		{
			get
			{
				if (incidentGroupController == null)
				{
					incidentGroupController = ZControllerFactory.Create(Modules.ClientControllerRegistration.IncidentManagementGroup) as IncidentManagementGroupController;
				}

				return incidentGroupController;
			}
		}

		IncidentManagementGroupController incidentGroupController;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		internal event EventHandler OnFormShown;

		#region Security Check Point

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncidentNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncidentView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncidentEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
