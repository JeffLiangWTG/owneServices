using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.ArchiveManager.Business.Records
{
	public partial class OfflineStorage
	{
		class OfflineStoragePreparationAction : IArchivePreparationAction
		{
			public OfflineStoragePreparationAction(IArchiveLogger logger, IArchiveSet set, ArchiveVolumeManager volumeManager)
			{
				this.logger = logger;
				this.set = set;
				this.volumeManager = volumeManager;

				_ = Argument.NotNull(volumeManager, "volumeManager");
			}

			readonly ArchiveVolumeManager volumeManager;

			public void Execute()
			{
				if (volumeManager.CurrentVolume == null || volumeManager.CurrentVolume.ExceededCapacity)
				{
					volumeManager.CreateNewVolume(logger, set.SystemDescriptor);
					logger.LogInfo(set.SystemDescriptor.Code, Res.GetString("52609209-f14b-48bb-828f-903604000615", "Producing new volume: {0}", volumeManager.CurrentVolume.VolumeNo));
				}

				set.MainArchiveItem.Purgeable = false;
				var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var storageMain = factory.Load<ArchiveStorageMain>(set.MainArchiveItem.PK);
				storageMain.SM_CD1 = (ZInt)volumeManager.CurrentVolume.VolumeNo;

				storageMain.ArchiveTo(volumeManager.CurrentVolume);

				factory.Save();
			}

			readonly IArchiveLogger logger;
			readonly IArchiveSet set;
		}
	}
}
