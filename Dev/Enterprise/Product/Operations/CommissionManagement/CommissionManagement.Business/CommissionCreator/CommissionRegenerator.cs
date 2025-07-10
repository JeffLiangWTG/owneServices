using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public abstract class CommissionRegenerator : ICommissionRegenerator
	{
		public BusinessObjectFactory Factory { get; private set; }
		public ILogger Logger { get; private set; }

		public CommissionRegenerator(BusinessObjectFactory factory, ILogger logger)
		{
			this.Factory = factory;
			this.Logger = logger;
		}

		public static void QueueSourceItemsForRegeneration(BusinessObjectFactory factory, IEnumerable<ViewCommissionLineGrouping> groupings)
		{
			foreach (var grouping in groupings)
			{
				DeleteExistingQueueItemIfExists(factory, grouping.SourceId);

				var queueItem = factory.New<OrgCommissionCalculationQueue>();
				queueItem.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

				if (grouping.SourceTableCode == JobHeaderSchema.Constants.Prefix)
				{
					queueItem.CAQ_JH = grouping.SourceId;
				}
				else
				{
					queueItem.CAQ_AH = grouping.SourceId;
				}
			}

			factory.Save();
		}

		static void DeleteExistingQueueItemIfExists(BusinessObjectFactory factory, ZGuid sourceId)
		{
			var regenQueueItemQuery = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_Operation, OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration);
			var sourceItemNotNullQuery = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_AH, sourceId);
			sourceItemNotNullQuery.AddToFilter(JoinCondition.Or, OrgCommissionCalculationQueueSchema.CAQ_JH, sourceId);
			regenQueueItemQuery.AddToFilter(sourceItemNotNullQuery);

			var existingItems = factory.Load<OrgCommissionCalculationQueue>(regenQueueItemQuery);
			existingItems.DeleteAll();
		}

		public abstract void RegenerateCommissions();
		protected abstract void ReverseCommissions();
		protected abstract void CreateCommissions();
	}
}
