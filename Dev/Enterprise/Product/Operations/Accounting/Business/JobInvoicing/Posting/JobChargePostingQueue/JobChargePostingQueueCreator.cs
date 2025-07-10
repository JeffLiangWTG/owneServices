using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobChargePostingQueueCreator
	{
		public JobChargePostingQueueCreator(BusinessObjectFactory factoryToCreateQueueRecords, ZGuid jobParentPK, ZString jobParentTablePrefix)
		{
			Argument.NotNull(factoryToCreateQueueRecords, nameof(factoryToCreateQueueRecords));

			this.factoryToCreateQueueRecords = factoryToCreateQueueRecords;
			this.jobParentPK = jobParentPK;
			this.jobParentTablePrefix = jobParentTablePrefix;
			costCollection = new List<Charge>();
			revenueCollection = new List<Charge>();
		}

		readonly BusinessObjectFactory factoryToCreateQueueRecords;
		readonly ZGuid jobParentPK;
		readonly ZString jobParentTablePrefix;

		readonly List<Charge> costCollection;
		readonly List<Charge> revenueCollection;

		public void AddChargePostingInstruction(PostingInstruction? postingInstruction, Charge charge)
		{
			if (charge == null)
			{
				return;
			}

			switch (postingInstruction)
			{
				case PostingInstruction.PostCost:
					costCollection.Add(charge);
					break;
				case PostingInstruction.PostRevenue:
					revenueCollection.Add(charge);
					break;
				case PostingInstruction.PostRevenueAndCost:
					costCollection.Add(charge);
					revenueCollection.Add(charge);
					break;
				case null:
					break;
				default:
					throw new ArgumentException("postingInstruction has invalid value and cannot be handle.");
			}
		}

		public void CreateJobChargePostingQueueRecords()
		{
			if (costCollection.Any() || revenueCollection.Any())
			{
#if NETFRAMEWORK
				var costCollectionDistinctOnes = costCollection.DistinctBy(x => x.PK);
#else
				var costCollectionDistinctOnes = System.Linq.Enumerable.DistinctBy(costCollection, x => x.PK);
#endif
				foreach (var charge in costCollectionDistinctOnes)
				{
					JobChargePostingQueue.CreateNew(factoryToCreateQueueRecords, JobChargePostingQueueLookups.PostCost, charge, jobParentPK, jobParentTablePrefix);
				}
#if NETFRAMEWORK
				var revenueCollectionDistinctOnes = revenueCollection.DistinctBy(x => x.PK);
#else
				var revenueCollectionDistinctOnes = System.Linq.Enumerable.DistinctBy(revenueCollection, x => x.PK);
#endif
				foreach (var charge in revenueCollectionDistinctOnes)
				{
					JobChargePostingQueue.CreateNew(factoryToCreateQueueRecords, JobChargePostingQueueLookups.PostRevenue, charge, jobParentPK, jobParentTablePrefix);
				}

				factoryToCreateQueueRecords.Saved += FactorySavedEventHandler;
			}
		}

		void FactorySavedEventHandler(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= FactorySavedEventHandler;

			if (savedSuccessfully)
			{
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("JPQ");
			}
		}

#if DEBUG
		public IList<Charge> CostCollection_ForTestOnly { get => costCollection; }
		public IList<Charge> RevenueCollection_ForTestOnly { get => revenueCollection; }
#endif
	}
}
