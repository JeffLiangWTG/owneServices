using CargoWise.EntityFramework;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementLinkFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public IncidentManagementLinkFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		IncidentManagementLink IncidentManagementLink
		{
			get { return BusinessObject as IncidentManagementLink; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(IncidentManagementGroupSchema.PK, IncidentManagementLink.INL_ING_Group);
			Factory.AddFetchHint(IncidentMainSchema.PK, IncidentManagementLink.INL_IM_Incident);

			foreach (var column in columns)
			{
				var columnName = column.ColumnName;
				switch (columnName)
				{
					case IncidentManagementLink.Schema.CustomerWaitingTime:
						var incidentSubQuery = new ZDBOnlySubQuery(typeof(SupportIncident), IncidentMainSchema.IM_INC_Request);
						incidentSubQuery.AddToFilter(IncidentMainSchema.PK, IncidentManagementLink.INL_IM_Incident);
						var incidentRequestQuery = new ZDBOnlyQuery(typeof(IncidentRequest));
						incidentRequestQuery.AddSubQuery(incidentSubQuery, JoinCondition.And);

						var jobConversationSubQuery = new ZDBOnlySubQuery(typeof(JobConversation), JobConversationSchema.PK);
						jobConversationSubQuery.AddToFilter(JobConversationSchema.PK, IncidentManagementLink.INL_IM_Incident);
						var jobConversationMessageQuery = new ZDBOnlyQuery(typeof(JobConversationMessage));
						jobConversationMessageQuery.AddSubQuery(JobConversationMessageSchema.JCM_JCC_Conversation, jobConversationSubQuery, JoinCondition.And);
						var jobConversationParticipantQuery = new ZDBOnlyQuery(typeof(JobConversationParticipant));
						jobConversationParticipantQuery.AddSubQuery(JobConversationParticipantSchema.JCP_JCC_Conversation, jobConversationSubQuery, JoinCondition.And);

						Factory.AddFetchHint(IncidentRequestSchema.Instance, incidentRequestQuery);
						Factory.AddFetchHint(JobConversationSchema.JCC_ParentID, IncidentManagementLink.INL_ING_Group);
						Factory.AddFetchHint(JobConversationSchema.JCC_ParentID, IncidentManagementLink.INL_IM_Incident);
						Factory.AddFetchHint(JobConversationMessageSchema.Instance, jobConversationMessageQuery);
						Factory.AddFetchHint(JobConversationParticipantSchema.Instance, jobConversationParticipantQuery);
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, IncidentManagementLink.INL_IM_Incident);
						break;
				}
			}
		}
	}
}
