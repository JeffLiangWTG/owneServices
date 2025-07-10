using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.Test
{
	internal class PurgeActionTest : TestCaseWithFactory
	{
		public void TestPurgeActionLogMessage_WhenArchiveSetHasRelatedRecords()
		{
			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var dummyBusinessObject = Factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();
				dummyBusinessObject.Z0_Code = "DUMMY";
				var dummyDependantBusinessObject1 = dummyBusinessObject.Dependents.AddNew();
				var dummyDependantBusinessObject2 = dummyBusinessObject.Dependents.AddNew();

				var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
				var archiveItem = new ArchiveItem(dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), null, System.Guid.Empty, false, dummyBusinessObject.TablePrefix);
				var archiveItemDependent1 = new ArchiveItem(dummyDependantBusinessObject1.PKSchemaColumn, dummyDependantBusinessObject1.PK.ToGuid(), dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), false, dummyBusinessObject.TablePrefix);
				var archiveItemDependent2 = new ArchiveItem(dummyDependantBusinessObject2.PKSchemaColumn, dummyDependantBusinessObject2.PK.ToGuid(), dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), false, dummyBusinessObject.TablePrefix);

				var set = new TestArchiveSet(systemDescriptor, "Dummy Test Name", Guid.NewGuid(), archiveItem, dummyBusinessObject.Z0_Code, new List<IArchiveItem>() { archiveItemDependent1, archiveItemDependent2 });
				var logger = new TestArchiveLogger();
				var purgeAction = new PurgeAction(set, logger);

				purgeAction.Execute();
				var log = $"Information|DMS|Deleting {AutoDummyBizo.Schema.TableName} '{dummyBusinessObject.Z0_Code}' and {dummyBusinessObject.Dependents.Count} related records";

				CombineAssertions("", () =>
				{
					AssertEquals(1, logger.ListOfMessages.Count);
					AssertEquals($"Archive Logs should contain the following log message, but didn't: ", log, logger.ListOfMessages[0]);
				});
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		public void TestPurgeActionLogMessage_WhenArchiveSetHasNoRelatedRecords()
		{
			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var dummyBusinessObject = Factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();

				var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
				var archiveItem = new ArchiveItem(dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), null, System.Guid.Empty, false, dummyBusinessObject.TablePrefix);

				var set = new TestArchiveSet(systemDescriptor, "Dummy Test Name", Guid.NewGuid(), archiveItem, dummyBusinessObject.Z0_Code);
				var logger = new TestArchiveLogger();
				var purgeAction = new PurgeAction(set, logger);

				purgeAction.Execute();
				var log = $"Information|DMS|Deleting {AutoDummyBizo.Schema.TableName} '{dummyBusinessObject.Z0_Code}' and {dummyBusinessObject.Dependents.Count} related records";

				CombineAssertions("", () =>
				{
					AssertEquals(1, logger.ListOfMessages.Count);
					AssertCollectionContains($"Archive Logs should contain the following log message, but didn't: ", log, logger.ListOfMessages);
				});
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		public void TestPurgeActionLogMessage_WhenArchiveSetHasRelatedRecords_ButAreNotPurgeable()
		{
			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var dummyBusinessObject = Factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();
				var dummyDependantBusinessObject1 = dummyBusinessObject.Dependents.AddNew();
				var dummyDependantBusinessObject2 = dummyBusinessObject.Dependents.AddNew();

				var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
				var archiveItem = new ArchiveItem(dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), null, System.Guid.Empty, false, dummyBusinessObject.TablePrefix);
				var archiveItemDependent1 = new ArchiveItem(dummyDependantBusinessObject1.PKSchemaColumn, dummyDependantBusinessObject1.PK.ToGuid(), dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), false, dummyBusinessObject.TablePrefix);
				var archiveItemDependent2 = new ArchiveItem(dummyDependantBusinessObject2.PKSchemaColumn, dummyDependantBusinessObject2.PK.ToGuid(), dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), false, dummyBusinessObject.TablePrefix);
				archiveItemDependent1.Purgeable = false;
				archiveItemDependent2.Purgeable = false;

				var set = new TestArchiveSet(systemDescriptor, "Dummy Test Name", Guid.NewGuid(), archiveItem, dummyBusinessObject.Z0_Code, new List<IArchiveItem>() { archiveItemDependent1, archiveItemDependent2 });
				var logger = new TestArchiveLogger();
				var purgeAction = new PurgeAction(set, logger);

				purgeAction.Execute();
				var log = $"Information|DMS|Deleting {AutoDummyBizo.Schema.TableName} '{dummyBusinessObject.Z0_Code}' and 0 related records";

				CombineAssertions("", () =>
				{
					AssertEquals(1, logger.ListOfMessages.Count);
					AssertCollectionContains($"Archive Logs should contain the following log message, but didn't: ", log, logger.ListOfMessages);
				});
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		public void TestPurgeAction_WhenRelatedRecordsAreInTheSameTable()
		{
			var parentShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var childShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			childShipment.JS_JS_ColoadMasterShipment = parentShipment.PK;

			Factory.Save();

			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveItem = new ArchiveItem(parentShipment.PKSchemaColumn, parentShipment.PK.ToGuid(), null, System.Guid.Empty, false, parentShipment.TablePrefix);
			var childArchiveItem = new ArchiveItem(childShipment.PKSchemaColumn, childShipment.PK.ToGuid(), parentShipment.PKSchemaColumn, parentShipment.PK.ToGuid(), false, parentShipment.TablePrefix);
			archiveItem.Purgeable = true;
			childArchiveItem.Purgeable = true;

			CombineAssertions("Precondition: JobShipments should exist.", () =>
			{
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.PK, parentShipment.PK)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.PK, childShipment.PK)));
			});

			var set = new TestArchiveSet(systemDescriptor, "Dummy OPS Stage", Guid.NewGuid(), archiveItem, parentShipment.JS_UniqueConsignRef, new List<IArchiveItem>() { childArchiveItem });
			var logger = new TestArchiveLogger();
			var purgeAction = new PurgeAction(set, logger);

			purgeAction.Execute();

			CombineAssertions("JobShipments should have been archived.", () =>
			{
				Assert(logger.ListOfMessages.Exists(log => log.Contains("Deleting JobShipment")));

				AssertEquals("Parent shipment should be deleted", 0, Factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.PK, parentShipment.PK)));
				AssertEquals("Child shipment should be deleted", 0, Factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.PK, childShipment.PK)));
			});
		}

		public void TestPurgeActionLogMessage_WhenArchiveSetHasNoNK()
		{
			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var dummyBusinessObject = Factory.NewWithValidTestData<DummyBusinessObject>();

				var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
				var archiveItem = new ArchiveItem(dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), null, System.Guid.Empty, false, dummyBusinessObject.TablePrefix);

				var set = new TestArchiveSet(systemDescriptor, "Dummy Test Name", Guid.NewGuid(), archiveItem);
				var logger = new TestArchiveLogger();
				var purgeAction = new PurgeAction(set, logger);

				purgeAction.Execute();
				var log = $"Information|DMS|Deleting {AutoDummyBizo.Schema.TableName} '{dummyBusinessObject.PK}' and 0 related records";

				CombineAssertions("", () =>
				{
					AssertEquals(1, logger.ListOfMessages.Count);
					AssertEquals($"Archive Logs should contain the following log message, but didn't: ", log, logger.ListOfMessages[0]);
				});
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		public void TestPurgeActionUsesMoreThanOneTVPWhenRelatedRecordsAreInAnotherTable()
		{
			var dummyBusinessObject = Factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();
			dummyBusinessObject.Z0_Code = "DUMMY";
			var dummyDependantBusinessObject1 = dummyBusinessObject.Dependents.AddNew();
			var dummyDependantBusinessObject2 = dummyBusinessObject.Dependents.AddNew();

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var archiveItem = new ArchiveItem(dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), null, System.Guid.Empty, false, dummyBusinessObject.TablePrefix);
			var archiveItemDependent1 = new ArchiveItem(dummyDependantBusinessObject1.PKSchemaColumn, dummyDependantBusinessObject1.PK.ToGuid(), dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), false, dummyBusinessObject.TablePrefix);
			var archiveItemDependent2 = new ArchiveItem(dummyDependantBusinessObject2.PKSchemaColumn, dummyDependantBusinessObject2.PK.ToGuid(), dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(), false, dummyBusinessObject.TablePrefix);

			var set = new TestArchiveSet(systemDescriptor, "Dummy Test Name", Guid.NewGuid(), archiveItem, dummyBusinessObject.Z0_Code, new List<IArchiveItem>() { archiveItemDependent1, archiveItemDependent2 });
			var logger = new TestArchiveLogger();
			var purgeAction = new PurgeAction(set, logger);
			var purgeQuery = purgeAction.GetPurgeQuery();

			CombineAssertions("TVPs for both tables should be present", () =>
			{
				AssertContains("TVP for top-level table should be present", "DummyBizoTVP", purgeQuery);
				AssertContains("TVP for dependent table should be present", "DummyDependentBizoTVP", purgeQuery);
			});
		}

		public void TestPurgeActionUsesTheSameTVPForTableWithSelfReferences()
		{
			var parentShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var childShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			childShipment.JS_JS_ColoadMasterShipment = parentShipment.PK;

			Factory.Save();

			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveItem = new ArchiveItem(parentShipment.PKSchemaColumn, parentShipment.PK.ToGuid(), null, System.Guid.Empty, false, parentShipment.TablePrefix);
			var childArchiveItem = new ArchiveItem(childShipment.PKSchemaColumn, childShipment.PK.ToGuid(), parentShipment.PKSchemaColumn, parentShipment.PK.ToGuid(), false, parentShipment.TablePrefix);
			archiveItem.Purgeable = true;
			childArchiveItem.Purgeable = true;

			var set = new TestArchiveSet(systemDescriptor, "Dummy OPS Stage", Guid.NewGuid(), archiveItem, parentShipment.JS_UniqueConsignRef, new List<IArchiveItem>() { childArchiveItem });
			var logger = new TestArchiveLogger();
			var purgeAction = new PurgeAction(set, logger);
			var purgeQuery = purgeAction.GetPurgeQuery();
			var expectedPurgeQuery = @"Begin Try
Declare @top bigint = 9223372036854775807
DELETE TOP (@top) Child from	@JobShipmentTVP as tmp join JobShipment as Child on Child.JS_JS_ColoadMasterShipment = tmp.Value where 1 = 1 AND Child.JS_JS_ColoadMasterShipment IS NOT NULL OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))
DELETE TOP (@top) Child from	@JobShipmentTVP as tmp join JobShipment as Child on Child.JS_JS_SplitSwitchShipment = tmp.Value where 1 = 1 AND Child.JS_JS_SplitSwitchShipment IS NOT NULL OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))
DELETE TOP (@top) JobShipment FROM @JobShipmentTVP AS tmp JOIN JobShipment ON tmp.Value = JS_PK OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))
End Try
Begin Catch
	DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
	RAISERROR(@ErrorMessage, 16, 1);
End Catch";

			AssertEquals("Purge query should be as expected", expectedPurgeQuery, purgeQuery);
		}
	}
}
