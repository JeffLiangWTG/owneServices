using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementLinkCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public IncidentManagementLinkCollectionFetchStrategy(IncidentManagementLinkCollection collection) : base(collection)
		{
			factory = collection?.Factory;
		}

		readonly BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);
			if (factory == null)
			{
				return;
			}

			var links = businessObjects.OfType<IncidentManagementLink>();

			var shouldReloadEventLogs = false;
			var shouldUpdateRegistryValue = false;
			var incidentQuery = new ZDBOnlyQuery(typeof(SupportIncident)).AddToFilter(IncidentMainSchema.PK, links.Select(x => x.INL_IM_Incident));
			incidentQuery.ReLoadExistingRows = true;
			var incidents = factory.Load<SupportIncident>(incidentQuery);

			if (columns.Any(x => x.ColumnName.StartsWith("SupportIncident")))
			{
				if (columns.Any(x => x.ColumnName.StartsWith("SupportIncident+Client")))
				{
					var clientQuery = new ZDBOnlyQuery(typeof(OrgHeader)).AddToFilter(OrgHeaderSchema.PK, incidents.Select(x => x.IM_OH_Client));
					clientQuery.ReLoadExistingRows = true;
					factory.Load(typeof(OrgHeader), clientQuery);
				}

				if (columns.Any(x => x.ColumnName.StartsWith("SupportIncident+WorkItem")))
				{
					var pivotQuery = new ZDBOnlyQuery(typeof(GenPivot));
					foreach (var incident in incidents)
					{
						pivotQuery.AddToFilter(GenPivotCollectionRelationship.GetRelatedActivitiesQuery(incident, Core.Constants.GenPivotTypes.ProcessManagement, includeChildren: true, includeParents: true), JoinCondition.Or);
					}

					pivotQuery.ReLoadExistingRows = true;
					factory.Load<GenPivot>(pivotQuery);

					var workItemPKs = new List<ZGuid>();
					foreach (var incident in incidents)
					{
						workItemPKs.AddRange(incident.RelatedWorkItemsPivot.Where(x => x.XX_Relation1TableCode == WorkItemSchema.Constants.Prefix).Select(y => y.XX_Relation1ID));
						workItemPKs.AddRange(incident.RelatedWorkItemsPivot.Where(x => x.XX_Relation2TableCode == WorkItemSchema.Constants.Prefix).Select(y => y.XX_Relation2ID));
					}
					var query = new ZDBOnlyQuery(typeof(NewWorkItem)).AddToFilter(WorkItemSchema.PK, workItemPKs);
					query.ReLoadExistingRows = true;
					factory.Load(typeof(NewWorkItem), query);
				}
			}

			if (columns.Any(x => x.ColumnName.Equals("Flagged")))
			{
				var requestQuery = new ZQuery(IncidentRequestSchema.PK, incidents.Select(x => x.IM_INC_Request));
				factory.AddFetchHint(typeof(IncidentRequest), requestQuery);

				var conversationQuery = new ZQuery(JobConversationSchema.JCC_ParentID, incidents.Select(x => x.IM_INC_Request));
				factory.AddFetchHint(typeof(JobConversation), conversationQuery);

				var conversationPKList = incidents.Select(x => x.EConversation.Conversation.PK);
				var participantQuery = new ZQuery(JobConversationParticipantSchema.JCP_JCC_Conversation, conversationPKList);
				factory.AddFetchHint(typeof(JobConversationParticipant), participantQuery);

				var messageQuery = new ZQuery(JobConversationMessageSchema.JCM_JCC_Conversation, conversationPKList);
				messageQuery.ReLoadExistingRows = true;
				factory.Load(typeof(JobConversationMessage), messageQuery);

				links.ForEach(x => x.ClearLastUnflaggedEvent());
				shouldReloadEventLogs = true;
				shouldUpdateRegistryValue = true;
			}

			if (columns.Any(x => x.ColumnName.StartsWith("Responder")))
			{
				var query = new ZQuery(GlbStaffSchema.GS_Code, links.Select(x => x.INL_GS_NKResponder).Distinct());
				query.ReLoadExistingRows = true;
				factory.Load(typeof(GlbStaff), query);
			}

			links.GroupBy(x => x.INL_ING_Group).ForEach(y =>
			{
				var incidentGroup = y.FirstOrDefault()?.IncidentManagementGroup;
				if (shouldUpdateRegistryValue || columns.Any(x => x.ColumnName.Equals("IsControlled")))
				{
					incidentGroup?.RefreshStages();
					shouldUpdateRegistryValue = false;
				}

				if (shouldReloadEventLogs || columns.Any(x => x.ColumnName.Equals("LastCommunication")))
				{
					(incidentGroup?.Logs as ILogsInternals)?.ReloadFromDB();
					shouldReloadEventLogs = false;
				}
			});
		}
	}
}
