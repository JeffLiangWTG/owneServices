using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class JobHeaderHelper
	{
		public static bool LoadOrCreateJobWithAddress(Guid branchPk, Guid? localClientAddressPk, BusinessObject operationsJob)
		{
			var job = LoadOrCreateJob(branchPk, operationsJob);
			if (job != null)
			{
				if (job.LocalCharges == null && localClientAddressPk != null)
				{
					job.JH_OA_LocalChargesAddr = localClientAddressPk.Value;
				}

				return true;
			}

			return false;
		}

		public static Job LoadOrCreateJobWithClient(Guid branchPk, Guid? localClientPk, BusinessObject operationsJob)
		{
			var job = LoadOrCreateJob(branchPk, operationsJob);
			if (job != null)
			{
				if (job.LocalCharges == null && localClientPk != null)
				{
					job.LocalChargesPK = localClientPk.Value;
				}

				return job;
			}

			return null;
		}

		static Job LoadOrCreateJob(Guid branchPk, BusinessObject operationsJob)
		{
			var jobHeaderParent = operationsJob as IJobHeaderParent;
			if (jobHeaderParent != null)
			{
				var branch = operationsJob.Factory.Load<GlbBranch>(branchPk);
				if (branch != null)
				{
					return new Job.Loader(jobHeaderParent).TryLoadOrCreateWithMutex(branch);
				}
			}

			return null;
		}

		public static ZGuid GetOverseasCreditorPK(Job invJob, JobInvoicingConsumerType consumerType = null)
		{
			if (consumerType == null)
			{
				consumerType = invJob.JobType;
			}

			if (consumerType is ConsolConsumerType)
			{
				if (invJob.Direction == Core.Constants.FreightShipmentDirection.Code.Import)
				{
					return invJob.GetSendingAgentPK();
				}
				else if (invJob.Direction == Core.Constants.FreightShipmentDirection.Code.Export)
				{
					return invJob.GetReceivingAgentPK();
				}
			}
			else if (consumerType is ShipmentConsumerType)
			{
				return invJob.AgentCollectPK;
			}

			return ZGuid.Empty;
		}

		public static ZGuid GetSendingAgentPK(this Job invoicingJob) => invoicingJob?.PlugInData?.InvoicingSupporter.SendingAgent?.PK ?? ZGuid.Empty;

		public static ZGuid GetReceivingAgentPK(this Job invoicingJob) => invoicingJob?.PlugInData?.InvoicingSupporter.ReceivingAgent?.PK ?? ZGuid.Empty;

		public static void DeactivateAllRelatedEmptyJobHeaders(ZGuid parentPK, BusinessObjectFactory factory)
		{
			var jobHeadersForDeactivation = new JobHeaderCollection(factory);
			jobHeadersForDeactivation.Load(GetQueryForClearing(parentPK));
			foreach (JobHeader jobHeader in jobHeadersForDeactivation)
			{
				if (!jobHeader.HasChanges)
				{
					jobHeader.SetContext(BusinessContext.JobDeactivationForAllCompanies);
				}
				jobHeader.MarkAsInactive();
			}
		}

		static ZDBOnlyQuery GetJobHeaderBaseQuery(ZGuid parentPK)
		{
			var filter = new ZDBOnlyQuery(typeof(JobHeader));
			var subFilter = new ZQuery();
			subFilter.AddToFilter(JobHeaderSchema.JH_ParentID, parentPK);
			filter.AddToFilter(subFilter, JoinCondition.And);
			filter.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			return filter;
		}

		static ZDBOnlyQuery GetQueryForClearing(ZGuid parentPK)
		{
			var filter = GetJobHeaderBaseQuery(parentPK);

			var jobChargeSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH, true);
			filter.AddSubQuery(jobChargeSubQuery, JoinCondition.And);

			var accTransactionLinesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_JH, true);
			accTransactionLinesSubQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, null);
			filter.AddSubQuery(accTransactionLinesSubQuery, JoinCondition.And);

			var accTransactionHeaderSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_JH, true);
			accTransactionHeaderSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_JH, SQLComparisonOperator.NotEqual, null);
			filter.AddSubQuery(accTransactionHeaderSubQuery, JoinCondition.And);

			var accHotChequeSubQuery = new ZDBOnlySubQuery(typeof(IAccHotCheque), AccHotChequeSchema.AQ_JH, true);
			accHotChequeSubQuery.AddToFilter(AccHotChequeSchema.AQ_JH, SQLComparisonOperator.NotEqual, null);
			filter.AddSubQuery(accHotChequeSubQuery, JoinCondition.And);

			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_JH_ParentJob, notIn: true);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_JH_ParentJob, SQLComparisonOperator.NotEqual, null);
			filter.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);

			return filter;
		}
	}
}

#region Test

// see Enterprise\Product\Operations\Accounting\Business.Testing\Rating\AccountingRatingServiceTest.cs
// and
// see Enterprise\Product\Operations\Accounting\Business.Testing\JobInvoicing\AccountingBillingServiceTest.cs

#endregion
