using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Business.Records
{
	public class ArchiveVolumeManager
	{
		public ArchiveVolumeManager(string rootDirectory, decimal freeSpaceInMB)
		{
			this.rootDirectory = rootDirectory;
			this.freeSpaceInMB = freeSpaceInMB;
		}

		string rootDirectory;
		readonly decimal freeSpaceInMB;

		public ArchiveVolume CurrentVolume { get; private set; }

		public void ChangeRootDirectory(string rootDirectory)
			=> this.rootDirectory = rootDirectory;

		public void CreateNewVolume(IArchiveLogger logger, IArchiveSystemDescriptor descriptor)
		{
			CurrentVolume?.Close();

			CurrentVolume = ArchiveVolume.CreateNew(rootDirectory, freeSpaceInMB, logger, descriptor);
		}

		public bool VolumeExists(int volumeNo, VolumeState state)
			=> ArchiveVolume.VolumeExists(rootDirectory, volumeNo, state);

		public void LoadVolume(int volumeNo, VolumeState state)
			=> CurrentVolume = ArchiveVolume.LoadVolume(rootDirectory, volumeNo, state);
	}
}

