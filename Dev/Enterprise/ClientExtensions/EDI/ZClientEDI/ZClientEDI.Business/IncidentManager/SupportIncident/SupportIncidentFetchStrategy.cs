using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	internal class SupportIncidentFetchStrategy : IncidentMainBase.Strategy
	{
		public SupportIncidentFetchStrategy(SupportIncident businessObject)
				: base(businessObject)
		{
			incident = businessObject;
		}

		readonly SupportIncident incident;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			Factory.AddFetchHint(ClientCompanySchema.PK, incident.IM_LCC);

			foreach (var column in columns)
			{
				if (column.ColumnName.StartsWith("OverallAssignedTo") || column.ColumnName.StartsWith("LastTaskClosedByStaff"))
				{
					if (column.ColumnName.Equals("OverallAssignedToStaffNK"))
					{
						Factory.AddFetchHint(GlbStaffSchema.GS_Code, incident.OverallAssignedToStaffNK);
					}

					if (column.ColumnName.Equals("OverallAssignedToCode", StringComparison.Ordinal))
					{
						Factory.AddFetchHint(GlbStaffSchema.GS_Code, incident.OverallAssignedToCode);
					}

					var lastClosedTask = incident.WorkflowItems.Tasks.OfType<ProcessTask>()
					.Where(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed && !task.P9_GS_NKAssignedStaffMember.IsEmpty)
					.OrderByDescending(task => task.P9_Sequence)
					.FirstOrDefault();
					if (lastClosedTask != null)
					{
						Factory.AddFetchHint(GlbStaffSchema.GS_Code, lastClosedTask.P9_GS_NKAssignedStaffMember);
					}
				}
				else if (column.ColumnName.StartsWith("Client"))
				{
					Factory.AddFetchHint(OrgHeaderSchema.PK, incident.IM_OH_Client);
				}
				else if (column.ColumnName.Contains("Database"))
				{
					Factory.AddFetchHint(LicenceDatabaseSchema.PK, incident.IM_LD);
				}
				else if (column.ColumnName.StartsWith("Request"))
				{
					Factory.AddFetchHint(IncidentRequestSchema.PK, incident.IM_INC_Request);
				}
				else if (column.ColumnName.StartsWith(IncidentMainSchema.Constants.IM_GS_NKCustDefectCausedBy))
				{
					Factory.AddFetchHint(GlbStaffSchema.GS_Code, incident.IM_GS_NKCustDefectCausedBy);
				}
				else if (column.ColumnName.StartsWith(IncidentMainSchema.Constants.IM_GS_NKCustServiceContact))
				{
					Factory.AddFetchHint(GlbStaffSchema.GS_Code, incident.IM_GS_NKCustDefectCausedBy);
				}
				else if (column.ColumnName.StartsWith("EstimateForBinding"))
				{
					var estimateQuery = new ZQuery(ClientIncidentEstimateSchema.CIE_IM, incident.PK);
					estimateQuery.MaximumRows = 1;
					Factory.AddFetchHint(ClientIncidentEstimateSchema.Instance, estimateQuery);
				}
				else if (column.ColumnName.StartsWith("QuoteForBinding"))
				{
					var quoteQuery = new ZQuery(ClientIncidentQuoteSchema.CIQ_IM, incident.PK);
					quoteQuery.MaximumRows = 1;
					Factory.AddFetchHint(ClientIncidentQuoteSchema.Instance, quoteQuery);
				}
				else if (column.ColumnName.StartsWith("Contact"))
				{
					Factory.AddFetchHint(typeof(OrgContact), incident.IM_OC_Contact);
				}
				else if (column.ColumnName.StartsWith("ManagementGroup"))
				{
					var managementGroupQuery = new ZQuery(IncidentManagementLinkSchema.INL_IM_Incident, incident.PK);
					managementGroupQuery.MaximumRows = 1;
					Factory.AddFetchHint(IncidentManagementLinkSchema.Instance, managementGroupQuery);
				}
			}
		}
	}
}
