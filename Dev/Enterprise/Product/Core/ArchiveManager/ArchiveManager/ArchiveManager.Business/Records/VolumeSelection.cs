using CargoWise.Types;

namespace Enterprise.ArchiveManager.Business.Records
{
	public class VolumeSelection : AutoVolumeSelection
	{
		public VolumeSelection(int volumeNoToFind)
		{
			VolumeNoToFind = volumeNoToFind;

			VolumeManager = new ArchiveVolumeManager("", 620);
			Hint = Res.GetString("870d986e-069f-42f8-8279-d74f6e1c7429", "Archived documents for the selected record are currently offline. In order to temporarily restore online for viewing, please select the file path for the directory or drive where the archive volume files have been stored with volume number: {0}.", volumeNoToFind);
		}

		public int VolumeNoToFind { get; private set; }

		public ArchiveVolumeManager VolumeManager { get; private set; }

		public override ZString VolumeLocation
		{
			get
			{
				return base.VolumeLocation;
			}
			set
			{
				base.VolumeLocation = value;
				VolumeManager.ChangeRootDirectory(value);
			}
		}
	}
}
