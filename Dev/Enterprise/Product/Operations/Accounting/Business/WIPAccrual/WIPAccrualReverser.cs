using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public class WIPAccrualReverser
	{
		public WIPAccrualReverser(BusinessObjectFactory externalFactory)
		{
			Argument.NotNull(externalFactory, "ExternalFactory");
			Factory = externalFactory;
		}

		public WIPAccrualReverser(WIPAccrualCollection wIPsAndAccrualsToReverse)
		{
			Argument.NotNull(wIPsAndAccrualsToReverse, "WIPsAndAccrualsToReverse");

			this.WIPAccrualCollection = wIPsAndAccrualsToReverse;
			Factory = new BusinessObjectFactory();
		}

		public WIPAccrualReverser(Job job)
		{
			PopulateWipAccrualCollectionFromJob(job);
			Factory = new BusinessObjectFactory();
		}

		public WIPAccrualReverser(Job job, BusinessObjectFactory externalFactory)
		{
			PopulateWipAccrualCollectionFromJob(job);
			Factory = externalFactory;
			SaveManagedExternally = true;
		}

		public int ReverseAll()
		{
			return ReverseAll(true);
		}

		public int ReverseAll(bool processApportionedCharges)
		{
			var baseWIPAccrualPKs = new HashSet<ZGuid>();
			foreach (BaseWIPAccrual baseWIPAccrual in WIPAccrualCollection)
			{
				baseWIPAccrualPKs.Add(baseWIPAccrual.PK);
			}
			var reloadedWIPAccruals = Factory.Load<BaseWIPAccrual>(new ZQuery(AccTransactionLinesSchema.PK, baseWIPAccrualPKs));

			return ReverseAll(processApportionedCharges, reloadedWIPAccruals);
		}

		public int ReverseAll(bool processApportionedCharges, IEnumerable<BaseWIPAccrual> wIPAccruals)
		{
			int transactionsReversed = 0;

			if (wIPAccruals != null)
			{
				foreach (BaseWIPAccrual wIPAccrual in wIPAccruals)
				{
					if (!wIPAccrual.IsReversed)
					{
						if (wIPAccrual.Reverse(processApportionedCharges))
						{
							transactionsReversed++;
						}
					}
				}
				if (transactionsReversed > 0 && !SaveManagedExternally)
				{
					Factory.Save();
				}
			}

			return transactionsReversed;
		}

		protected WIPAccrualCollection WIPAccrualCollection;
		public readonly BusinessObjectFactory Factory;
		protected readonly bool SaveManagedExternally;

		protected void PopulateWipAccrualCollectionFromJob(Job job)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			WIPAccrualCollection = new WIPAccrualCollection(newFactory, new ZQuery(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, job.PK));
			WIPAccrualCollection.Load();
		}
	}
}
