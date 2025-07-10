
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class ConsolBaseTransactionCreator
	{
		public ConsolBaseTransactionCreator(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, bool hasJobOnHold, IJobCostingPlugIn consol)
		{
			Argument.NotNull(fallbackFactory, "fallbackFactory");

			this.Factory = jobs.Any() ? jobs.First().Factory : fallbackFactory;
			this.Jobs = jobs;
			this.PostingTime = ZDateTime.Now;
			this.HasJobOnHold = hasJobOnHold;
			this.Consol = consol;
		}

		protected readonly IJobCostingPlugIn Consol;
		protected readonly bool HasJobOnHold;
		protected BusinessObjectFactory Factory;
		protected IEnumerable<Job> Jobs;
		protected ZDateTime PostingTime;

		public virtual bool CreateTransactions(TransactionCreatorHashtable transactions)
		{
			return false;
		}
	}
}