using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
#if DEBUG
	internal
#endif
	class InvoiceBulkOperationWorkflowModuleTextFilter : WorkflowModuleTextFilter
	{
		public InvoiceBulkOperationWorkflowModuleTextFilter(ZString description, GetTextQuery queryDelegate, IList list, Type bizObjType, ZString jobType)
			: base(description, queryDelegate, list, bizObjType, jobType)
		{
			SchemaColumnOverride = typeof(GenericConsol).IsAssignableFrom(bizObjType) ? ViewGenericConsolSchema.PK : JobHeaderSchema.JH_ParentID;

			if (typeof(AccTransactionLines).IsAssignableFrom(bizObjType))
			{
				RelatedParentSubQueries = new ZDBOnlySubQuery[] { new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH) };
			}
			else if (typeof(JobHeader).IsAssignableFrom(bizObjType))
			{
				RelatedParentSubQueries = new ZDBOnlySubQuery[] { new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK) };
			}
		}

		readonly SchemaColumn SchemaColumnOverride;

		protected override ZQuery GetQuery()
		{
			var result = new ZQuery();
			if (!IsEmpty)
			{
				var milestoneQueryWithTransportBookingParents = InvoiceBulkOperationWorkflowFilterStripsHelper.AddTransportBookingParentsInMilestoneFilter(GetMilestoneQuery());
				result = WorkflowModuleFilterQueryBuilder.BuildQuery(businessObjectType, milestoneQueryWithTransportBookingParents, RelatedParentSubQueries, SchemaColumnOverride);
			}
			return result;
		}

		protected override void AddParentTableCodeQuery(ZDBOnlySubQuery query)
		{
			// no parent table code query should be added.
		}
	}
}
