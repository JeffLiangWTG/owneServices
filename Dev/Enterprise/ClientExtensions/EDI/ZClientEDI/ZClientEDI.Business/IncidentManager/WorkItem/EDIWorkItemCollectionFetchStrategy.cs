using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	internal class EDIWorkItemCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public EDIWorkItemCollectionFetchStrategy(EDIWorkItemCollection collection) : base(collection)
		{
			this.collection = collection;
		}
		readonly EDIWorkItemCollection collection;

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);

			var columnNames = columns.Select(x => x.ColumnName);
			AddFetchHintForRelatedItems(businessObjects.Cast<EDIWorkItem>(), columnNames);
		}

		/*
		* This method is used to add FetchHints for RelatedItems.
		* Fetch hints are quite particular about how they can be used.
		* When a load happens, RowFactory.FilterSetAlreadyMet will determine whether we can get data that has been previously retrieved without hitting the database.
		* If queries in a fetch hint are too complex to be matched using FilterSetAlreadyMet, the db hit will happen despite the fetch hint
		* This is the case with Incidents which are related to WorkItem via GenPivot.
		* The workaround used here is to: 
		*								Add the fetch hints for the GenPivots 
		*							 => Load them 
		*							 => Add the fetch hints for the Incidents 
		*							 => Load them
		* If you want to refactor this part of code, please pass the stress test after modifying(TestFetchHintsHaveBeenAddedIfNeeded)
		*/
		void AddFetchHintForRelatedItems(IEnumerable<EDIWorkItem> workItems, IEnumerable<string> columnNames)
		{
			var hasRelatedClientCodeColumn = columnNames.Any(x => x == nameof(NewWorkItem.RelatedClientCode));
			var hasProjectIDsColumn = columnNames.Any(x => x == nameof(NewWorkItem.ProjectIDs));
			var hasProjectManagerNamesColumn = columnNames.Any(x => x == nameof(NewWorkItem.ProjectManagerNames));
			var hasCriticalityBasedOnRelatedIncidentsColumn = columnNames.Any(x => x == nameof(NewWorkItem.CriticalityBasedOnRelatedIncidents));
			var hasNumberOfRelatedIncidentsColumn = columnNames.Any(x => x == nameof(NewWorkItem.NumberOfRelatedIncidents));

			if (columnNames.Any(x => x.StartsWith(nameof(NewWorkItem.AssignedToCode)) || x.StartsWith(nameof(NewWorkItem.OverallTaskStatusCode)) ||
			x.StartsWith(nameof(NewWorkItem.OverallTaskStatusDescription)) || x.StartsWith(nameof(NewWorkItem.CurrentOrNextTask)) ||
			x.StartsWith(nameof(NewWorkItem.CurrentTask)) || x.StartsWith(nameof(NewWorkItem.JobAgreedDeliveryDate))))
			{
				foreach (var workItem in workItems)
				{
					collection.Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, workItem.PK);
				}

				foreach (var workItem in workItems)
				{
					var processHeaders = workItem.Workflows;

					foreach (var item in processHeaders)
					{
						var processHeader = (IProcessHeader)item;
						collection.Factory.AddFetchHint(ProcessTasksSchema.P9_FH_ProcessHeader, processHeader.PK);
						collection.Factory.AddFetchHint(ProcessHeaderLinkSchema.FP_FH_HeaderTo, processHeader.PK);
						collection.Factory.AddFetchHint(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, processHeader.PK);
					}
				}
			}

			if (hasRelatedClientCodeColumn || hasProjectIDsColumn || hasProjectManagerNamesColumn || hasCriticalityBasedOnRelatedIncidentsColumn || hasNumberOfRelatedIncidentsColumn)
			{
				foreach (var workItem in workItems)
				{
					var wrkQuery = GenPivotCollectionRelationship.GetRelatedActivitiesQuery(workItem, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement, true, true);
					collection.Factory.AddFetchHint(GenPivotSchema.Instance, wrkQuery);

					var zQuery = new ZQuery(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, workItem.PK);
					collection.Factory.AddFetchHint(WorkItemRequestLinkSchema.Instance, zQuery);
				}

				foreach (var workItem in workItems)
				{
					var incidentLinks = workItem.RelatedItemsPivot.ToArray().Where(x => x.XX_RelationType == Enterprise.Core.Constants.GenPivotTypes.ProcessManagement);

					foreach (var link in incidentLinks)
					{
						collection.Factory.AddFetchHint(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(link.XX_Relation1TableCode), link.XX_Relation1ID);
						collection.Factory.AddFetchHint(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(link.XX_Relation2TableCode), link.XX_Relation2ID);
					}
				}

				foreach (var workItem in workItems)
				{
					var incidents = workItem.RelatedItems.OfType<SupportIncident>();
					foreach (var item in incidents)
					{
						if (hasRelatedClientCodeColumn)
						{
							collection.Factory.AddFetchHint(OrgHeaderSchema.PK, item.IM_OH_Client);
						}

						if (hasProjectIDsColumn || hasProjectManagerNamesColumn)
						{
							//WorkTaskRelatedItemCollection.PivotCollections.get
							var wrkQuery = GenPivotCollectionRelationship.GetRelatedActivitiesQuery(item, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement, true, true);
							collection.Factory.AddFetchHint(GenPivotSchema.Instance, wrkQuery);

							var oppQuery = GenPivotCollectionRelationship.GetRelatedActivitiesQuery(item, Enterprise.Core.Constants.GenPivotTypes.Opportunity, true, true);
							collection.Factory.AddFetchHint(GenPivotSchema.Instance, oppQuery);

							var clientAddressQuery = new ZQuery(GenPivotSchema.XX_RelationType, EDIGenPivotTypes.FeatureRequestClientAddress);
							clientAddressQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, item.PK);
							clientAddressQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
							collection.Factory.AddFetchHint(GenPivotSchema.Instance, clientAddressQuery);

							//SupportIncident.RelatedItems.get -> WorkTaskRelatedItemCollection.LoadCore -> GenPivot.Relation1Object.get
							foreach (var pivot in item.RelatedWorkItemsPivot)
							{
								collection.Factory.AddFetchHint(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(pivot.XX_Relation1TableCode), pivot.XX_Relation1ID);
								collection.Factory.AddFetchHint(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(pivot.XX_Relation2TableCode), pivot.XX_Relation2ID);
							}
						}
					}
				}
			}
		}
	}
}
