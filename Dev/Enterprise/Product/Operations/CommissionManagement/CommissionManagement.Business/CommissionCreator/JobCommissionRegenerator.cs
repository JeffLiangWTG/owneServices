using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.CommissionManagement.Business
{
	public class JobCommissionRegenerator : CommissionRegenerator
	{
		readonly IJobHeader Job;

		public JobCommissionRegenerator(IJobHeader job, BusinessObjectFactory factory, ILogger logger = null)
			: base(factory, logger)
		{
			Argument.NotNull(job, nameof(job));
			Argument.NotNull(factory, nameof(factory));

			this.Job = job;
		}

		public override void RegenerateCommissions()
		{
			if (Job.Parent == null)
			{
				((JobHeader)Job).InitializeParentFromGenericJobWithoutSettingDefaults();
			}

			ReverseCommissions();
			CreateCommissions();
		}

		protected override void CreateCommissions()
		{
			var filter = new JobClosedCommissionCreator.JobClosedCommissionTransactionFilter(Job, Factory);
			filter.RefreshInvoiceList();

			foreach (var transaction in filter.Transactions.OfType<ICommissionableTransaction>())
			{
				new JobRelatedTransactionCommissionCreator(transaction, Tuple.Create(Job, ZDateTime.Now)).CreateCommissions(new CreateCommissionContext() { RegeneratingCommissions = true });
			}
		}

		protected override void ReverseCommissions()
		{
			var nonReversedHeaders = new CommissionHelper().GetNonReversedCommissionHeaders(Factory, Job);

			foreach (var transactionHeaderPair in nonReversedHeaders)
			{
				foreach (AccCommissionHeader commissionHeader in transactionHeaderPair.Item2)
				{
					commissionHeader.MarkAsOverriden();
				}
			}
		}
	}
}
