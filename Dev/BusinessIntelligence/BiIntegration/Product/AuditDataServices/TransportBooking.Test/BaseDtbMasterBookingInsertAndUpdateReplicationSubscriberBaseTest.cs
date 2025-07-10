using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.AuditDataServices.TransportBooking.Subscribers;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.AuditDataServices.TransportBooking.Test
{
	abstract class BaseDtbMasterBookingInsertAndUpdateReplicationSubscriberBaseTest : BaseDtbMasterBookingReplicationSubscriberBaseTest
	{
		public override void TestProcessChangesOnInsert()
		{
			InitialiseTestFields();
			SetupPreparatoryData();
			SetupMasterEntityForInsertTest();
			SubscriberFactory.Save();
			AssertPreconditionsOnMasterAndSubEntitiesForInsertTest();

			var changeTable = GetTestDataTable();
			var insertRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.Insert, MasterEntity);

			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory();
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);
			AssertSubsInsertedFromMaster(insertRow, SubParents);

			AuditTestHelper.AssertLog(logger.Logs, ExpectedInsertLogs);
		}

		public virtual void TestProcessChangesOnUpdateForSubParentBookingsRequiringInsert()
		{
			InitialiseTestFields();
			SetupPreparatoryDataWithoutSubs();
			SetupMasterEntityForInsertTest();
			SubscriberFactory.Save();

			var changeTable = GetTestDataTable();

			SetupSubParents();
			AssertPreconditionsOnMasterAndSubEntitiesForInsertTest();
			UpdateEntities(
				new BusinessObject[] { MasterEntity },
				new Tuple<SchemaColumn, object>(SystemLastEditTimeUtcColumn, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(SystemLastEditUserColumn, "ME"));
			SubscriberFactory.Save();
			var afterUpdateRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.UpdateAfterWrapperFiltering, MasterEntity);

			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory();
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);
			AssertSubsInsertedFromMaster(afterUpdateRow, SubParents);

			AuditTestHelper.AssertLog(logger.Logs, ExpectedInsertLogs);
		}

		public virtual void TestProcessChangesOnUpdateAndDelete()
		{
			InitialiseTestFields();
			ClearEntities();
			SetupPreparatoryData();
			SetupMasterAndSubEntities();
			SubscriberFactory.Save();
			AssertPreconditionsOnMasterAndSubEntities();

			var changeTable = GetTestDataTable();

			UpdateMasterEntity();
			SubscriberFactory.Save();
			AssertPreconditionsAfterUpdateOnMasterEntity();
			AddNewRowToChangeTable(changeTable, CdcOperationCodes.UpdateAfterWrapperFiltering, MasterEntity);

			DeleteMasterEntity();
			SubscriberFactory.Save();

			changeTable.AcceptChanges();

			SubscriberFactory.Save();

			var subscriber = NewDataChangeSubscriberUsingFactory();
			var logger = new BetterLoggerForTest();
			AssertNoExceptionThrown("Should not have NullReferenceException due to master entity being deleted", () =>
			{
				subscriber.ProcessChanges(logger, changeTable);
			});
			AssertSubsDeleted(SubEntity1, SubEntity2);
		}

		public override void TestFailedInsertReplication()
		{
			InitialiseTestFields();
			SetupPreparatoryData();
			SetupMasterEntityForInsertTest();
			SubscriberFactory.Save();
			AssertPreconditionsOnMasterAndSubEntitiesForInsertTest();

			var changeTable = GetTestDataTable();
			var insertRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.Insert, MasterEntity);

			changeTable.AcceptChanges();

			var subscriber = (BaseDtbMasterBookingInsertingReplicationSubscriber)NewDataChangeSubscriberUsingFactory(throwTestErrorOnSaveToFactory: true);
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			ReloadParentSubEntitiesFromCurrentFactory(subscriber);
			AssertSubsNotInsertedFromMaster(insertRow, SubParents);
			AuditTestHelper.AssertLog(logger.Logs, ExpectedInsertFailedLogs);
			AssertCorrectFailedInsertErrorNotesWereCreated();
			AssertFactoryWasRecreated(subscriber);
		}

		protected override void SetupPreparatoryData()
		{
			ClearPreparatoryData();

			SetupPreparatoryDataWithoutSubs();

			SetupSubParents();
		}

		protected void SetupPreparatoryDataWithoutSubs()
		{
			ClearPreparatoryData();

			SetupMiscellaneousPreparatoryData();

			SetupMasterParent();
		}

		protected abstract void ClearPreparatoryData();
		protected abstract void SetupSubParents();
		protected abstract void SetupMiscellaneousPreparatoryData();
		protected abstract void SetupMasterParent();
		protected abstract void SetupMasterEntityForInsertTest();
		protected abstract void AssertSubsNotInsertedFromMaster(DataRow insertRow, params BusinessObject[] subParents);
		protected abstract void AssertSubsInsertedFromMaster(DataRow insertRow, params BusinessObject[] subParents);
		protected abstract void AssertPreconditionsOnMasterAndSubEntitiesForInsertTest();
		protected abstract void ReloadParentSubEntitiesFromCurrentFactory(BaseDtbMasterBookingInsertingReplicationSubscriber subscriber);

		protected IDtbBooking MasterBooking { get; set; }
		protected IDtbBooking SubBooking1 { get; set; }
		protected IDtbBooking SubBooking2 { get; set; }

		protected abstract SchemaColumn SystemLastEditTimeUtcColumn { get; }
		protected abstract SchemaColumn SystemLastEditUserColumn { get; }

		protected abstract BusinessObject[] SubParents { get; }
	}
}
