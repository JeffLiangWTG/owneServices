using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionHelper : ICommissionHelper
	{
		public List<Tuple<ICommissionableTransaction, IAccCommissionHeader[]>> GetNonReversedCommissionHeaders(BusinessObjectFactory factory, IJobHeader job)
		{
			var commissionsToReverse = new List<Tuple<ICommissionableTransaction, IAccCommissionHeader[]>>();

			var existingCommissionQuery = GetNonReversedCommissionHeaderQuery(job.PK, JobHeaderSchema.PK);

			var nonReversedCommissionHeaders = factory.Load<IAccCommissionHeader>(existingCommissionQuery);
			if (nonReversedCommissionHeaders.Length == 0)
			{
				return commissionsToReverse;
			}

			var filter = new JobClosedCommissionCreator.JobClosedCommissionTransactionFilter(job, ZDateTime.MinSmallDateTimeValue, job.JH_A_JCL);
			filter.RefreshInvoiceList();

			ReversalTransactionCommissionCreator.AddReversalTransactionFetchHints(factory, filter.Transactions.OfType<ICommissionableTransaction>().ToArray());

			foreach (ICommissionableTransaction transaction in filter.Transactions.OfType<ICommissionableTransaction>())
			{
				var nonReversedCommissionHeadersForInvoice = nonReversedCommissionHeaders.Where(c => c.CH0_AH_Source == transaction.PK).ToArray();
				if (nonReversedCommissionHeadersForInvoice.Length > 0)
				{
					commissionsToReverse.Add(transaction, nonReversedCommissionHeadersForInvoice);
				}
			}

			return commissionsToReverse;
		}

		public (ICommissionableTransaction Transaction, IAccCommissionHeader[] CommissionHeaders) GetNonReversedCommissionHeaders(BusinessObjectFactory factory, ICommissionableTransaction transaction)
		{
			var existingCommissionQuery = GetNonReversedCommissionHeaderQuery(transaction.PK, AccTransactionHeaderSchema.PK);

			var nonReversedCommissionHeaders = factory.Load<IAccCommissionHeader>(existingCommissionQuery);
			if (nonReversedCommissionHeaders.Length == 0)
			{
				return (null, null);
			}

			return (transaction, nonReversedCommissionHeaders);
		}

		ZDBOnlyQuery GetNonReversedCommissionHeaderQuery(ZGuid groupingSourceId, SchemaColumn schemaColumn)
		{
			var querySql = FormattableString.Invariant($@"
	CH0_PK IN
	(
		SELECT CH0_PK
		FROM
			dbo.AccCommissionHeader
			JOIN dbo.AccCommissionLineGroup ON CLG_CH0 = CH0_PK
			JOIN dbo.AccCommissionLine AS Line ON CL0_ParentID = CLG_PK
		WHERE
			CH0_GroupingSourceID = @GroupingSourceID
			AND
			CL0_BelongsToGroup IS NULL
			AND
			CL0_OverridenDateTimeUtc IS NULL
			AND
			CL0_PK IN
			(
				SELECT COALESCE(CL0_BelongsToGroup, CL0_PK)
				FROM dbo.AccCommissionHeader
				INNER JOIN dbo.AccCommissionLineGroup ON CLG_CH0 = CH0_PK
				INNER JOIN dbo.AccCommissionLine ON CL0_ParentID = CLG_PK
				WHERE CH0_GroupingSourceID = @GroupingSourceID
				GROUP BY COALESCE(CL0_BelongsToGroup, CL0_PK)
				HAVING COUNT(*) = 1
			)
		UNION
		SELECT CH0_PK
		FROM
			dbo.AccCommissionHeader
			JOIN dbo.AccCommissionLine AS Line ON CL0_ParentID = CH0_PK
		WHERE
			CH0_GroupingSourceID = @GroupingSourceID
			AND
			CL0_BelongsToGroup IS NULL
			AND
			CL0_OverridenDateTimeUtc IS NULL
			AND
			CL0_PK IN
			(
				SELECT COALESCE(CL0_BelongsToGroup, CL0_PK)
				FROM dbo.AccCommissionHeader
				INNER JOIN dbo.AccCommissionLine ON CL0_ParentID = CH0_PK
				WHERE CH0_GroupingSourceID = @GroupingSourceID
				GROUP BY COALESCE(CL0_BelongsToGroup, CL0_PK)
				HAVING COUNT(*) = 1
			)
	)");

			var paramList = new ZSqlParameterCollection();
			paramList.Add("@GroupingSourceID", groupingSourceId, schemaColumn);

			var existingCommissionQuery = new ZDBOnlyQuery(ObjectFactory.GetType<IAccCommissionHeader>());
			existingCommissionQuery.AddFilterAndZSQLParameterCollection(querySql, paramList);

			return existingCommissionQuery;
		}
	}
}
