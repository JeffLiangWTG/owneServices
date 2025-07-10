using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.Test.ArchiveStages
{
	[UseSnapshotProtection]
	class HARArchiveStageTest : TestCaseWithFactory
	{
		public void TestArchiveToImages()
		{
			SetUpArchiveDirectory();
			var archiveSystem = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
			var archiveStage = new HARArchiveStage(archiveSystem.GetArchiveStageDescriptors(config).First(), archiveSystem);
			var schedule = new TestArchiveSchedule();
			var logger = new TestArchiveLogger();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var hvlvConsignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var mainArchiveItem = new ArchiveItem(shipment.PKSchemaColumn, shipment.PK.ToGuid(), null, System.Guid.Empty, false, shipment.TablePrefix);
			var archiveItem = new ArchiveItem(hvlvConsignmentHeader.PKSchemaColumn, hvlvConsignmentHeader.PK.ToGuid(), shipment.PKSchemaColumn, shipment.PK.ToGuid(), false, hvlvConsignmentHeader.TablePrefix);
			var archiveSet = new TestArchiveSet(archiveSystem, "Dummy Test Name", Guid.NewGuid(), mainArchiveItem, JobShipmentSchema.Constants.JS_UniqueConsignRef, new List<IArchiveItem>() { archiveItem });

			CombineAssertions("Precondition", () =>
			{
				Assert($"{nameof(mainArchiveItem)} should be purgeable.", mainArchiveItem.Purgeable);
				Assert($"{nameof(archiveItem)} should be purgeable.", archiveItem.Purgeable);
			});

			archiveStage.BeginRun(config, schedule, logger);
			_ = archiveStage.ArchiveToImages(archiveSet);

			CombineAssertions(() =>
			{
				Assert($"{nameof(mainArchiveItem)} should not be purgeable.", !mainArchiveItem.Purgeable);
				Assert($"{nameof(archiveItem)} should not be purgeable.", !archiveItem.Purgeable);

				Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		void SetUpArchiveDirectory()
		{
			DeleteArchiveDirectory();
			DummyArchiveStorage.ArchiveDirectory = Path.Combine(Env.TempPath, "DummyArchive");
			_ = Directory.CreateDirectory(DummyArchiveStorage.ArchiveDirectory);
		}

		void DeleteArchiveDirectory()
		{
			if (Directory.Exists(DummyArchiveStorage.ArchiveDirectory))
			{
				Directory.Delete(DummyArchiveStorage.ArchiveDirectory, recursive: true);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteArchiveDirectory();
		}
	}
}
