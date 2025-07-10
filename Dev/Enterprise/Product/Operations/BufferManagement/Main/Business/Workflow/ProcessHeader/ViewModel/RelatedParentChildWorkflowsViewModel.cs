using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class RelatedParentChildWorkflowsViewModel : NonPersistentBusinessObject
	{
		public RelatedParentChildWorkflowsViewModel(ProcessJobHeader jobHeader, RelationshipDirection relationshipDirection)
			: base(jobHeader.Factory)
		{
			this.jobHeader = jobHeader;
			this.relationshipDirection = relationshipDirection;
			SubscribeToProcessHeaders();
		}

		readonly ProcessJobHeader jobHeader;
		readonly RelationshipDirection relationshipDirection;

		public ProcessHeaderLinkCollection ExternalWorkflowLinks => externalWorkflowLinks ?? (externalWorkflowLinks = new ProcessHeaderLinkCollection(jobHeader.Factory));
		ProcessHeaderLinkCollection externalWorkflowLinks;

		public void AddLinks(IReadOnlyCollection<ProcessHeader> selectedProcessHeaders)
		{
			foreach (var processHeader in selectedProcessHeaders)
			{
				var link = ExternalWorkflowLinks.AddNew();
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

				if (relationshipDirection == RelationshipDirection.From)
				{
					link.FP_FH_HeaderFrom = jobHeader.PK;
					link.FP_FH_HeaderTo = processHeader.PK;
				}
				else
				{
					link.FP_FH_HeaderTo = jobHeader.PK;
					link.FP_FH_HeaderFrom = processHeader.PK;
				}

				link.ProcessNewLink();
			}
		}

		public void RemoveLinks(IReadOnlyCollection<ProcessHeaderLink> selectedLinks)
		{
			ProcessHeaderLinkViewModelHelper.RemoveLinks(selectedLinks, ExternalWorkflowLinks);
		}

		#region Implementation

		ProcessHeaderCollection allProcessHeaders;

		void SubscribeToProcessHeaders()
		{
			allProcessHeaders = ProcessHeaderCollection.GetCollectionForJobIncludingJobLevelWorkflow(jobHeader.Parent, jobHeader);
			((IBindingList)allProcessHeaders).ListChanged += RelatedParentAndChildWorkflowViewModel_ListChanged;
			RelatedParentAndChildWorkflowViewModel_ListChanged(null, null);
		}

		void RelatedParentAndChildWorkflowViewModel_ListChanged(object sender, ListChangedEventArgs e)
		{
			var allProcessHeaderPKs = allProcessHeaders.Select(h => h.PK).Distinct().ToList();
			var query = new ZQuery();

			if (relationshipDirection == RelationshipDirection.From)
			{
				query.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, allProcessHeaderPKs));
				query.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, SQLComparisonOperator.NotEqual, allProcessHeaderPKs));
			}
			else
			{
				query.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, allProcessHeaderPKs));
				query.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, SQLComparisonOperator.NotEqual, allProcessHeaderPKs));
			}
			query.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.ParentChild));
			ExternalWorkflowLinks.AdditionalFilter = query;
		}

		#endregion
	}
}
