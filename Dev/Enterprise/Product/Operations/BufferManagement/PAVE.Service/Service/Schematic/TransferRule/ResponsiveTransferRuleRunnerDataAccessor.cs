using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Service
{
	class ResponsiveTransferRuleRunnerDataAccessor : TransferRuleRunnerDataAccessor
	{
		public ResponsiveTransferRuleRunnerDataAccessor(ITransferRuleRunnerLogger logger, IEnumerable<Guid> transferablesPKs)
			: base(logger)
		{
			workflowsPKsToProcess = transferablesPKs;
		}

		readonly IEnumerable<Guid> workflowsPKsToProcess;

		protected override BusinessObjectFactory CreateStartingFactory()
		{
			var factory = base.CreateStartingFactory();
			factory.RefreshEnabled = true;
			return factory;
		}

		protected override ZQuery GetLinkAssociatedWorkflowQueryCore(BMComponentLink link)
		{
			var query = base.GetLinkAssociatedWorkflowQueryCore(link);

			query.AllowTableValuedParameters = true;
			query.AddToFilter(ProcessHeaderSchema.PK, workflowsPKsToProcess);
			return query;
		}

		public UnupdatedWorkflowBatchLoader GetUnupdatedWorkflowBatchLoader(IReadOnlyCollection<Guid> unupdatedWorkflowPKs, Guid systemPK)
		{
			return new UnupdatedWorkflowBatchLoader(this, Logger, () => GetUnupdatedWorkflowQuery(unupdatedWorkflowPKs, systemPK));
		}

		internal static (ZQuery query, string queryName) GetUnupdatedWorkflowQuery(IReadOnlyCollection<Guid> workflowPKs, Guid systemPK)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddToFilter(ProcessHeaderSchema.PK, workflowPKs);
			query.AddToFilter(ProcessHeaderSchema.FH_FC_DedicatedBuffer, SQLComparisonOperator.NotEqual, DBNull.Value); // no need to reset Dedicated Buffer on workflows that has no Dedicated Buffer set
			query.AddToFilter(ProcessHeaderSchema.FH_IsActive, true); // we don't update inactive workflows
			query.AllowTableValuedParameters = true;

			var componentSubquery = new ZDBOnlySubQuery(typeof(BMComponent), BMComponentSchema.PK);
			componentSubquery.AddToFilter(BMComponentSchema.FC_FS_System, systemPK);
			query.AddSubQuery(ProcessHeaderSchema.FH_FC_CurrentComponent, componentSubquery, JoinCondition.And);

			var queryType = (NoResString)"Transfer Rules (resetting Dedicated Buffer for workflows no longer eligible for release)"; // Query names should not be translated
			var queryName = FormattableString.Invariant($@"
-- Database:  {Db.DatabaseName}
-- Type:	  {queryType}");

			return (query, queryName);
		}
	}
}
