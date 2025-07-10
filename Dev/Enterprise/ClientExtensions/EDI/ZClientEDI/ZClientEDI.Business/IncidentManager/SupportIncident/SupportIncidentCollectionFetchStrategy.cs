using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	internal class SupportIncidentCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public SupportIncidentCollectionFetchStrategy(SupportIncidentCollection collection) : base(collection)
		{
			this.collection = collection;
		}
		readonly SupportIncidentCollection collection;

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);

			var columnNames = columns.Select(x => x.ColumnName);
			var notNullIncident = businessObjects.OfType<SupportIncident>();

			AddFetchHintForRowColors(notNullIncident);

			AddFetchHintForRelatedItems(notNullIncident, columnNames);

			AddFetchHintForWorkflow(notNullIncident, columnNames);

			AddFetchHintForOtherRelatedObject(notNullIncident, columnNames);
		}

		void AddFetchHintForRowColors(IEnumerable<SupportIncident> supportIncidents)
		{
			foreach (var incident in supportIncidents)
			{
				collection.Factory.AddFetchHint(ClientCompanySchema.PK, incident.IM_LCC);
			}
		}

		/*
		* This method is used to add FetchHints for RelatedItems.
		* Fetch hints are quite particular about how they can be used.
		* When a load happens, RowFactory.FilterSetAlreadyMet will determine whether we can get data that has been previously retrieved without hitting the database.
		* If queries in a fetch hint are too complex to be matched using FilterSetAlreadyMet, the db hit will happen despite the fetch hint
		* This is the case with WorkItems and WorkProject (EDIProject) which are related to IncidentMain via GenPivot.
		* The workaround used here is to: 
		*								Add the fetch hints for the GenPivots 
		*							 => Load them 
		*							 => Add the fetch hints for the WorkItems 
		*							 => Load them 
		*							 => Add the fetch hints for their ProcessTasks and ProcessHeaders
		* If you want to refactor this part of code, please pass the stress test after modifying(TestFetchHintsHaveBeenAddedIfNeeded)
		*/
		void AddFetchHintForRelatedItems(IEnumerable<SupportIncident> supportIncidents, IEnumerable<string> columnNames)
		{
			if (columnNames.Any(x => x.StartsWith("WorkItem") || x.StartsWith("RelatedProject") || x.StartsWith("ManagementGroup")))
			{
				foreach (var incident in supportIncidents)
				{
					var wrkQuery = GenPivotCollectionRelationship.GetRelatedActivitiesQuery(incident, Core.Constants.GenPivotTypes.ProcessManagement, includeChildren: true, includeParents: true);
					collection.Factory.AddFetchHint(GenPivotSchema.Instance, wrkQuery);
				}

				foreach (var incident in supportIncidents)
				{
					foreach (var pivot in incident.RelatedWorkItemsPivot)
					{
						collection.Factory.AddFetchHint(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(pivot.XX_Relation1TableCode), pivot.XX_Relation1ID);
						collection.Factory.AddFetchHint(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(pivot.XX_Relation2TableCode), pivot.XX_Relation2ID);
					}
				}

				if (columnNames.Any(x => x.StartsWith("WorkItem")))
				{
					foreach (var incident in supportIncidents)
					{
						foreach (var relatedWorkItem in incident.RelatedWorkItemsReadOnly.Where(x => x != null))
						{
							collection.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, relatedWorkItem.PK);
							collection.Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, relatedWorkItem.PK);
						}
					}
				}

				if (columnNames.Any(x => x.StartsWith("ManagementGroup")))
				{
					foreach (var incident in supportIncidents)
					{
						collection.Factory.AddFetchHint(IncidentManagementLinkSchema.Instance, new ZQuery(IncidentManagementLinkSchema.INL_IM_Incident, incident.PK));
					}
				}
			}
		}

		/*
		* This method is used to add FetchHints for WorkFlow.
		* Fetch hints are quite particular about how they can be used.
		* When a load happens, RowFactory.FilterSetAlreadyMet will determine whether we can get data that has been previously retrieved without hitting the database.
		* If queries in a fetch hint are too complex to be matched using FilterSetAlreadyMet, the db hit will happen despite the fetch hint
		* This is the case with ProcessTasks and ProcessHeader which are related to IncidentMain via XX_ParentID.
		* The workaround used here is to: 
		*								Add the fetch hints for the ProcessTask(ParentID) 
		*							 => Load them 
		*							 => Add the fetch hints for the ProcessHeader and GlbCapability
		*							 => Load them
		* If you want to refactor this part of code, please pass the stress test after modifying(TestFetchHintsHaveBeenAddedIfNeeded)
		*/
		void AddFetchHintForWorkflow(IEnumerable<SupportIncident> supportIncidents, IEnumerable<string> columnNames)
		{
			if (columnNames.Any(x => x.StartsWith("CurrentTask") || x.StartsWith("OverallAssignedTo") || x.StartsWith("LastTaskClosedByStaff")))
			{
				collection.Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, supportIncidents.Select(x => x.PK)));

				var activeProcessTasks = supportIncidents.SelectMany(x => x.WorkflowItems.OfType<ProcessTask>().Where(t => t.IsOpen));

				var activeProcessTasksQueryFilter = new ZQuery(ProcessTasksSchema.P9_ParentID, supportIncidents.Select(x => x.PK));
				activeProcessTasksQueryFilter.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, new string[] { ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled });

				var activeProcessTasksSubQueryForProcessHeader = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_FH_ProcessHeader);
				activeProcessTasksSubQueryForProcessHeader.AddToFilter(activeProcessTasksQueryFilter, JoinCondition.And);
				var processHeaderSearchQuery = new ZDBOnlyQuery(typeof(ProcessHeader));
				processHeaderSearchQuery.AddSubQuery(activeProcessTasksSubQueryForProcessHeader, JoinCondition.And);

				var activeProcessTasksSubQueryForCapability = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_G4_RequiredCapability);
				activeProcessTasksSubQueryForCapability.AddToFilter(activeProcessTasksQueryFilter, JoinCondition.And);
				var glbCapabilitySearchQuery = new ZDBOnlyQuery(typeof(GlbCapability));
				glbCapabilitySearchQuery.AddSubQuery(activeProcessTasksSubQueryForCapability, JoinCondition.And);

				var activeProcessTasksSubQueryForStaff = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_GS_NKAssignedStaffMember);
				activeProcessTasksSubQueryForStaff.AddToFilter(activeProcessTasksQueryFilter, JoinCondition.And);
				var glbStaffSearchQuery = new ZDBOnlyQuery(typeof(GlbStaff));
				glbStaffSearchQuery.AddSubQuery(GlbStaffSchema.GS_Code, activeProcessTasksSubQueryForStaff, JoinCondition.And);

				collection.Factory.Load(typeof(ProcessHeader), processHeaderSearchQuery);
				collection.Factory.Load(typeof(GlbCapability), glbCapabilitySearchQuery);
				collection.Factory.Load(typeof(GlbStaff), glbStaffSearchQuery);

				foreach (var incident in supportIncidents)
				{
					var openedTasks = incident.WorkflowItems.Tasks.OfType<ProcessTask>().
						Where(x => x.IsOpen).
						OrderBy(t => t.P9_Sequence).
						ThenBy(t => t.P9_TaskID);
					foreach (var task in openedTasks)
					{
						if (task.ProcessHeader != null)
						{
							collection.Factory.AddFetchHint(ProcessTasksSchema.P9_FH_ProcessHeader, task.ProcessHeader.PK);
						}
					}
				}
			}
		}

		void AddFetchHintForOtherRelatedObject(IEnumerable<SupportIncident> supportIncidents, IEnumerable<string> columnNames)
		{
			if (columnNames.Any(x => x.StartsWith("Client")))
			{
				foreach (var incident in supportIncidents)
				{
					collection.Factory.AddFetchHint(nameof(OrgHeader), incident.IM_OH_Client);
				}
			}

			if (columnNames.Any(x => x.StartsWith("Client+MiscServ")))
			{
				foreach (var incident in supportIncidents)
				{
					collection.Factory.AddFetchHint(OrgMiscServSchema.OM_OH, incident.IM_OH_Client);
				}
			}

			if (columnNames.Any(x => x.StartsWith("Database")))
			{
				foreach (var incident in supportIncidents)
				{
					if (!incident.IM_LD.IsEmpty)
					{
						collection.Factory.AddFetchHint(nameof(LicenceDatabase), incident.IM_LD);
					}
					else
					{
						var query = new ZQuery();
						query.AddToFilter(LicenceDatabaseSchema.LD_OH_WebAccessOrg, incident.IM_OH_Client);
						query.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production);
						query.AddToFilter(LicenceDatabaseSchema.LD_Product, incident.IM_Product);
						collection.Factory.AddFetchHint(LicenceDatabaseSchema.Instance, query);
					}
				}

				foreach (var incident in supportIncidents)
				{
					var incidentDb = incident.Database;
					if (incidentDb != null)
					{
						collection.Factory.AddFetchHint(nameof(ReleaseBuild), incidentDb.LD_HL_CurrentRunningVersion);
					}

					var incidentDbOrPrimaryDb = incident.DatabaseOrPrimaryProductionSystem;
					if (incidentDbOrPrimaryDb != null)
					{
						collection.Factory.AddFetchHint(nameof(LicenceEnterprise), incidentDbOrPrimaryDb.LD_LE);
						collection.Factory.AddFetchHint(nameof(ReleaseBuild), incidentDbOrPrimaryDb.LD_HL_CurrentRunningVersion);
					}
				}
			}

			if (columnNames.Any(x => x.StartsWith("EstimateForBinding")))
			{
				foreach (var incident in supportIncidents)
				{
					collection.Factory.AddFetchHint(ClientIncidentEstimateSchema.Instance, new ZQuery(ClientIncidentEstimateSchema.CIE_IM, incident.PK));
				}
			}

			if (columnNames.Any(x => x.StartsWith("QuoteForBinding")))
			{
				foreach (var incident in supportIncidents)
				{
					collection.Factory.AddFetchHint(ClientIncidentQuoteSchema.Instance, new ZQuery(ClientIncidentQuoteSchema.CIQ_IM, incident.PK));
				}
			}

			if (columnNames.Any(x => x.StartsWith("IncidentComment")))
			{
				foreach (var incident in supportIncidents)
				{
					collection.Factory.AddFetchHint(StmNoteSchema.Instance, new ZQuery(StmNoteSchema.ST_ParentID, incident.PK));
				}
			}

			if (columnNames.Any(x => x.StartsWith("AddedBy")))
			{
				foreach (var incident in supportIncidents)
				{
					collection.Factory.AddFetchHint(GlbStaffSchema.GS_Code, incident.IM_SystemCreateUser);
				}
			}

			if (columnNames.Any(x => x.StartsWith("Triage")))
			{
				foreach (var incident in supportIncidents)
				{
					collection.Factory.AddFetchHint(IncidentTriageSchema.PK, incident.IM_IMT_Triage);
				}
			}
		}
	}
}
