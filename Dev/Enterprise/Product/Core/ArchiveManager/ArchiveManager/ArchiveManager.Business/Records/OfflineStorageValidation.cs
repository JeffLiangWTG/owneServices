using System;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Business.Records
{
	public class OfflineStorageValidation : AutoOfflineStorageValidation
	{
		public OfflineStorageValidation(AutoOfflineStorage parent)
			: base(parent) { }

		public new OfflineStorage Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OfflineStorage)base.Parent; }
		}

		#region Implementation

		protected override void CheckArchiveDateTo()
		{
			base.CheckArchiveDateTo();
			MandatoryValidation.CheckEntered(Parent.ArchiveDateToInfo);

			if (Parent.RequiredStorageCapacityCalculated && Parent.StorageCapacityRequiredInMB <= 0)
			{
				if (Parent.StorageMainRecordsForArchiving == 0)
				{
					Parent.ArchiveDateToInfo.AddError(Res.GetString("E0E3F039-D931-494e-A088-33761BCFB3B9", "There are no Archived Records to archive offline. Try choosing a later date."));
				}
				else
				{
					Parent.ArchiveDateToInfo.AddError(Res.GetString("27e6a91c-ca66-440e-b1c2-d6f24e32a3e0", "The Archived Records in the selected date range have less than 1 MB of eDocs to archive in total.\r\nTry choosing a later date to include more eDocs to archive onto DVD."));
				}
			}
		}

		protected override void CheckOfflineLocation()
		{
			base.CheckOfflineLocation();
			MandatoryValidation.CheckEntered(Parent.OfflineLocationInfo);

			if (!string.IsNullOrEmpty(Parent.OfflineLocation))
			{
				if (!Directory.Exists(Parent.OfflineLocation))
				{
					Parent.OfflineLocationInfo.AddError(Res.GetString("36d3ed8b-831f-4e24-ad7f-de2640e30ef4", "Directory or Drive does not exist. Please enter a different path."));
				}
				else
				{
					DriveInfo drive = null;
					try
					{
						drive = new DriveInfo(Path.GetPathRoot(Parent.OfflineLocation));

						if (drive.DriveType != DriveType.Fixed)
						{
							Parent.OfflineLocationInfo.AddError(Res.GetString("b80436e3-aacc-4cfc-8cfd-e5f60fb608e9", "You must specify a physical hard disk drive to store the files."));
						}
					}
					catch (ArgumentException)
					{
						// not a valid drive.
					}

					if (drive != null)
					{
						var freeSpace = drive.AvailableFreeSpace / 1024 / 1024;
						var requiredSpace = Parent.StorageCapacityRequiredInMB * 1.30M; //30% buffer
						if (freeSpace <= requiredSpace)
						{
							Parent.OfflineLocationInfo.AddError(Res.GetString("0709447c-c495-4f61-9748-4f096f6fe67d", "Path only has {0} MB available. Archive volume files require {1} MB of free space.", freeSpace, requiredSpace));
						}
					}
				}
			}
		}

		protected override void CheckFreeSpaceInMB()
		{
			base.CheckFreeSpaceInMB();

			if (Parent.FreeSpaceInMB < Parent.StorageCapacityRequiredInMB)
			{
				Parent.FreeSpaceInMBInfo.AddError(Res.GetString("F9BD28B7-2A61-4813-835E-DCC6AFA0758D", "Not enough storage capacity in disk. You must select a disk with capacity greater than {0} MB.", Parent.StorageCapacityRequiredInMB));
			}
		}

		#endregion
	}
}
