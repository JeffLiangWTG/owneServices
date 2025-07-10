using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class LinkedIncidentsModuleButtonGrid : ZModuleButtonGrid
	{
		IncidentManagementGroup IncidentManagementGroup => (IncidentManagementGroup)Form.BusinessEntity;

		public LinkedIncidentsModuleButtonGrid()
		{
			ModuleID = ClientModuleRegistration.SupportIncident;
		}

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			DetachMessage = Res.GetData("ea3a43c5-4a9a-4a96-9336-96e9e29a5cb4",
				"You are about to detach incidents. Would you like to continue?");
			base.DetachButton_Click(sender, e);
		}

		protected override void Detach(BusinessObject selected)
		{
			base.Detach(selected);
			OnLinkDetached((IncidentManagementLink)selected);
		}

		void OnLinkDetached(IncidentManagementLink link)
		{
			var group = link?.IncidentManagementGroup;
			var incident = link?.SupportIncident;
			if (group != null && incident != null)
			{
				incident.RelatedItems.Remove(group);
				if (link.IsControlled)
				{
					if (!group.NowStage.IncidentCompleted)
					{
						link.ReopenIncidentTasksClosedByGroup();
					}

					PostUCKEvent(link);
				}

				PostDTCEvent(link);
			}
			else
			{
				ErrorReporter.ReportOnce("The key property is null in an IncidentManagementLink instance",
					$@"Link PK:{link?.PK ?? ZGuid.Empty}
Link has been deleted:{link?.IsDeleted.ToString() ?? string.Empty}
Linked Incident PK:{link?.SupportIncident?.PK ?? ZGuid.Empty}
Linked Group PK:{link?.IncidentManagementGroup?.PK ?? ZGuid.Empty}");
			}

			link?.Delete();
		}

		static string GetBaseDetachEventReference(IncidentManagementLink link, string description)
		{
			return EventLogReferenceBuilder.New()
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, description)
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, link.SupportIncident.Number)
				.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, link.IncidentManagementGroup.Number).Build();
		}

		static string GetDTCEventReference(IncidentManagementLink link)
		{
			return GetBaseDetachEventReference(link, $"Incident {link.SupportIncident.Number} removed from Incident Group {link.IncidentManagementGroup.Number}");
		}

		static string GetUCKEventReference(IncidentManagementLink link)
		{
			return GetBaseDetachEventReference(link, $"Control disabled for incident {link.SupportIncident.Number}");
		}

		static void PostDTCEvent(IncidentManagementLink link)
		{
			var reference = GetDTCEventReference(link);
			link.IncidentManagementGroup.Logs.AddNew(AutoEvents.Detached, reference);
			link.SupportIncident.Logs.AddNew(AutoEvents.Detached, reference);
		}

		static void PostUCKEvent(IncidentManagementLink link)
		{
			var reference = GetUCKEventReference(link);
			link.IncidentManagementGroup.Logs.AddNew(AutoEvents.UnlockForEdit, reference);
			link.SupportIncident.Logs.AddNew(AutoEvents.UnlockForEdit, reference);
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new LinkedIncidentsGridAttacher(destinationCollection, findBoxList, moduleID, IncidentManagementGroup);
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm(((IncidentManagementLink)selected).SupportIncident);
		}

		protected override bool NeedsSaveToShowEditForm(BusinessObject selected) => false;

		#region Attacher

		/// <summary>
		/// Since our Grid List is bounded to IncidentManagementLink But DataBinding is for SupportIncident
		/// We needed to create a class to override some of the functions in ZRecordAttacher
		/// </summary>
		internal class LinkedIncidentsGridAttacher : ZRecordAttacher
		{
			readonly IncidentManagementGroup incidentManagementGroup;

			public LinkedIncidentsGridAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, IncidentManagementGroup incidentManagementGroup)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.incidentManagementGroup = incidentManagementGroup;
			}

			protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
			{
				var loadedIncident = bizO.Factory.Load<SupportIncident>(bizO.PK);
				if (loadedIncident != null && !loadedIncident.IsDeleted)
				{
					//item.RelatedItems // TODO: add as parent of Incident

					var result = GetDialogResultForCheckValidStatus(loadedIncident);
					if (result != DialogResult.Cancel)
					{
						var link = incidentManagementGroup.Factory.New<IncidentManagementLink>();
						link.INL_IM_Incident = loadedIncident.PK;
						link.INL_ING_Group = incidentManagementGroup.PK;
						link.INL_GS_NKResponder = incidentManagementGroup.ING_GS_NKDefaultResponder;
						link.INL_IsGroupControlled = result == DialogResult.Yes;
						// RelatedItems.Add is used to add incidentManagementGroup to the RelatedItems of the incident, listToBulkAdd.Add is used to display the newly added items in the linked incidents grid before clicking the save button.
						link.SupportIncident.RelatedItems.Add(incidentManagementGroup);
						listToBulkAdd.Add(link);
						return true;
					}
				}

				return false;
			}

			DialogResult GetDialogResultForCheckValidStatus(SupportIncident incident)
			{
				if (!incidentManagementGroup.IncidentCompleted && incident.IM_Status.Equals(IncidentMainLookups.Status.Closed))
				{
					var message = ResString.GetMultilingualString("56ABFB44-1CE7-49A3-BEF2-BD71A08B4C55",
						"You are attaching a completed incident to an active incident group. Do wish to add as a controlled incident?");
					var caption = ResString.GetMultilingualString("12507A37-DE8D-4C35-AFC8-77448CFE74AA", "Confirm control status for incident(s)");

					return Globals.Message.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
				}

				return DialogResult.Yes;
			}

			protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
			{
				return ((IncidentManagementLink)bizObj).INL_IM_Incident;
			}
		}

		#endregion

		public override void Refresh()
		{
			InnerGrid?.ListManager?.Refresh();
			base.Refresh();
		}

		public void ForceRefresh()
		{
			IncidentManagementGroup.LinkedIncidents.Reload(true);
			IncidentManagementGroup.LinkedIncidents.FireListReset();
			this.InnerGrid?.ForcePreFetch();
			this.InnerGrid?.Refresh();
		}
	}
}
