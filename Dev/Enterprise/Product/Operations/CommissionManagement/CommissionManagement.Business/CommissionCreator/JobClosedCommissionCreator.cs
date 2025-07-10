using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class JobClosedCommissionCreator : CommissionCreator, IJobClosedCommissionCreator
	{
		public JobClosedCommissionCreator(IJobHeader job, ZDateTime jobCloseTime, DataTable effectiveDateCacheTable = null)
			: base(job.Factory)
		{
			Argument.NotNull(job, "job");

			this.job = job;
			this.jobCloseTime = jobCloseTime;
			this.effectiveDateCacheTable = effectiveDateCacheTable;
		}

		readonly IJobHeader job;
		readonly ZDateTime jobCloseTime;
		readonly DataTable effectiveDateCacheTable;

		public void PostQueueItemOrCreateCommissionsOnJobClosed()
		{
			if (OrganisationsDataRegistry.Instance.RunCommissionCreationInBackground.Value)
			{
				var queueItem = Factory.New<OrgCommissionCalculationQueue>();
				queueItem.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Creation;

				queueItem.CAQ_JH = job.PK;
				queueItem.CAQ_JobClosedDate = jobCloseTime;
			}
			else
			{
				CreateCommissionsOnJobClosed();
			}
		}

		public void CreateCommissionsOnJobClosed()
		{
			var context = new CreateCommissionContext();
			context.OverwriteOldValues = OrganisationsDataRegistry.Instance.OverwriteOldValuesOnJobClosed.GetFallBackValueAtAllLevels(job.JH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			CreateCommissions(context);
		}

		public override void CreateCommissions(CreateCommissionContext context)
		{
			var filter = new JobClosedCommissionTransactionFilter(job, context.FromDate, jobCloseTime);
			filter.RefreshInvoiceList();

			ReversalTransactionCommissionCreator.AddReversalTransactionFetchHints(Factory, filter.Transactions.OfType<ICommissionableTransaction>().ToArray());

			foreach (var transaction in filter.Transactions.OfType<ICommissionableTransaction>().OrderByDescending(t => t.AH_TransactionBelongsToGroup.IsEmpty))
			{
				CreateJobRelatedTransactionCommissions(transaction, context);
			}
		}

		protected virtual void CreateJobRelatedTransactionCommissions(ICommissionableTransaction transaction, bool isAmendingCreditNote)
		{ }

		protected virtual void CreateJobRelatedTransactionCommissions(ICommissionableTransaction transaction, CreateCommissionContext context)
		{
			if (transaction is TransactionHeaderWithLines transactionHeaderWithLines && transactionHeaderWithLines.IsFullAmendingCreditNote())
			{
				new ReversalTransactionCommissionCreator(transaction).CreateCommissions(context);
				CreateJobRelatedTransactionCommissions(transaction, true);
			}
			else
			{
				new JobRelatedTransactionCommissionCreator(transaction, Tuple.Create(job, jobCloseTime), effectiveDateCacheTable).CreateCommissions(context);
				CreateJobRelatedTransactionCommissions(transaction, false);
			}
		}

		public class JobClosedCommissionTransactionFilter : JobInvoicePrintingFilter
		{
			readonly ZGuid jobCompanyPK;

			public JobClosedCommissionTransactionFilter(IJobHeader job, ZDateTime postDateFilterFrom, ZDateTime postDateFilterTo)
				: base(job.Parent as IBusiness, job.PK, job.Factory, postDateFilterFrom, postDateFilterTo)
			{
				jobCompanyPK = job.JH_GC;
				this.IncludePrinted = true;
				this.DebtorOrCreditor = ZGuid.Empty;
			}

			public JobClosedCommissionTransactionFilter(IJobHeader job, BusinessObjectFactory factory)
				: base(job.Parent as IBusiness, job.PK, factory)
			{
				jobCompanyPK = job.JH_GC;
				this.IncludePrinted = true;
				this.DebtorOrCreditor = ZGuid.Empty;
			}

			public JobClosedCommissionTransactionFilter(IJobHeader job, BusinessObjectFactory factory, ZDateTime postDateFilterFrom, ZDateTime postDateFilterTo)
				: base(job.Parent as IBusiness, job.PK, factory, postDateFilterFrom, postDateFilterTo)
			{
				jobCompanyPK = job.JH_GC;
				this.IncludePrinted = true;
				this.DebtorOrCreditor = ZGuid.Empty;
			}

			protected override IEnumerable<string> Ledger
			{
				get
				{
					return TransactionCommissionCreator.CommissionableLedgerTypes;
				}
			}

			public override CodeDescriptionPairList TransactionTypeList
			{
				get
				{
					if (fTransactionTypeList == null)
					{
						fTransactionTypeList = new CodeDescriptionPairList(OLookUpEditType.InvoicePrintingTransactionTypes);
					}
					return fTransactionTypeList;
				}
			}

			public override bool IncludeReversals
			{
				get { return false; }
			}

			protected override OrganisationsFindBoxCollection GetDebtorOrCreditorFindBoxCollectionCore()
			{
				throw new NotSupportedException();
			}

			protected override ZGuid GetCompanyPK()
			{
				return jobCompanyPK;
			}

			protected override void AddAccTransactionHeaderIndexHints(ZDBOnlyQuery query, Guid jobPK, ZString ledgerType)
			{
				if (jobPK != Guid.Empty)
				{
					query.TableIndexHints.Add(new TableIndexHint(AccTransactionHeader.Schema.Index_FK_RX__AH_JH));
					query.TableIndexHints.Add(new TableIndexHint(AccTransactionHeaderSchema.Constants.PkIndex));
					query.IsForceSeek = true;
				}
			}
		}
	}
}
