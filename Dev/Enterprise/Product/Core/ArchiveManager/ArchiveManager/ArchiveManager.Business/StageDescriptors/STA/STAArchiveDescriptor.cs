using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.STA
{
	public abstract class STAArchiveDescriptor : CommonArchiveStageDescriptor
	{
		public abstract SchemaColumn ParentIDColumn { get; }
		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = new ZQuery();
			_ = query.AddToFilter(MainDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);

			if (ParentIDColumn != null)
			{
				_ = query.AddToFilter(ParentIDColumn, SQLComparisonOperator.Equal, null);
			}

			query.OrderBy = MainDateFilterColumn.Name;
			return query;
		}

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageAction = ObjectFactory.Get<IArchiveImageGenerationAction>();
			imageAction.Setup(logger, set, BusinessObjectProviderDictionary, cache);
			yield return imageAction;
		}
	}
}
