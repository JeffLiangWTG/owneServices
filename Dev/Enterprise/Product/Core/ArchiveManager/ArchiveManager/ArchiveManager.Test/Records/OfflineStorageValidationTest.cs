using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ArchiveManager.Business.Records;

namespace Enterprise.ArchiveManager.Test.Records
{
	internal class OfflineStorageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFreeSpaceInMB()
		{
			var storage = new OfflineStorage();
			AssertEquals("FreeSpaceInMBInfo.ReadOnly", expected: true, storage.FreeSpaceInMBInfo.ReadOnly);

			storage.StorageCapacityRequiredInMB = 100;
			storage.FreeSpaceInMB = 50;

			AssertHasError(storage.FreeSpaceInMBInfo, "Not enough storage capacity in disk. You must select a disk with capacity greater than 100 MB.");
		}

		public void TestValidateConfirmationLabel()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				var storage = new OfflineStorage();

				storage.ArchiveDateTo = ZDateTime.Today;
				storage.StorageMainRecordsForArchiving = 0;
				storage.StorageCapacityRequiredInMB = 0;
				storage.Validation.ValidateConfirmationLabel();
				Assert(!storage.ConfirmationLabelInfo.Notifications.HasErrors());
			}
		}

		public void TestArchiveDateTo()
		{
			var storage = new OfflineStorage();
			storage.ArchiveDateTo = ZDateTime.Today;
			storage.StorageMainRecordsForArchiving = 0;
			storage.StorageCapacityRequiredInMB = 0;
			storage.Validation.ValidateArchiveDateTo();
			AssertHasError(storage.ArchiveDateToInfo, "There are no Archived Records to archive offline. Try choosing a later date.");

			storage.StorageMainRecordsForArchiving = 2;
			storage.Validation.ValidateArchiveDateTo();
			AssertHasError(storage.ArchiveDateToInfo, "The Archived Records in the selected date range have less than 1 MB of eDocs to archive in total." + System.Environment.NewLine + "Try choosing a later date to include more eDocs to archive onto DVD.");
		}

		public void TestSupportsArchivingToNetworkFolder()
		{
			var storage = new OfflineStorage();
			AssertEquals(0, (int)storage.FreeSpaceInMB);

			storage.OfflineLocation = string.Format(@"\\{0}\DatLogs\", System.Environment.MachineName);
			AssertGreaterThan((int)storage.FreeSpaceInMB, 0);
		}

		public void TestInvalidNetworkFolderIsHandledNicely()
		{
			var storage = new OfflineStorage();
			AssertEquals(0, (int)storage.FreeSpaceInMB);

			storage.OfflineLocation = @"\\ThereIsNoWayThisIsAValidMachineNameInWTG\DatLogs\";
			AssertHasError(storage.OfflineLocationInfo, "Directory or Drive does not exist. Please enter a different path.");
			AssertEquals(0, (int)storage.FreeSpaceInMB);
		}
	}
}
