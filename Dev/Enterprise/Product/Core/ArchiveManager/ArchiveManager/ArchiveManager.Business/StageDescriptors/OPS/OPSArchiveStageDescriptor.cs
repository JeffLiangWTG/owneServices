using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Business
{
	public class OPSArchiveStageDescriptor : CommonArchiveStageDescriptor
	{
		public override string Name
			=> Res.GetString("aec5f778-e2dc-41a7-8841-98252b4b055b", "Operational Jobs Archive");

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageAction = ObjectFactory.Get<IArchiveImageGenerationAction>();
			imageAction.Setup(logger, set, BusinessObjectProviderDictionary, cache);
			yield return imageAction;
		}
	}
}
