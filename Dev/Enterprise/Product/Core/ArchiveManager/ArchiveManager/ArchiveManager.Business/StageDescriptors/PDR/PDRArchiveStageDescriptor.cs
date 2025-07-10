using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Business
{
	public class PDRArchiveStageDescriptor : CommonArchiveStageDescriptor
	{
		public override string Name
			=> Res.GetString("9E699616-8F94-4A31-8EDF-1B99B8B48E9E", "Purge Documents and Records");

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageDeleteAction = ObjectFactory.Get<IArchiveImageDeletionAction>();
			imageDeleteAction.Setup(logger, set, cache, config);
			yield return imageDeleteAction;
		}

		public override void OnArchiveSetProcessed(IArchiveSet set)
		{
			base.OnArchiveSetProcessed(set);

			OnArchiveSetProcessedHelpers.AddOrUpdateDocumentsDeletedCount(set, ProcessingInfoPerTable);
		}
	}
}
