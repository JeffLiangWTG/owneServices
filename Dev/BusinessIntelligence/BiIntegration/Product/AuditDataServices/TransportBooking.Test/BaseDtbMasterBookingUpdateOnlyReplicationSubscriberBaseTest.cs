using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;

namespace Enterprise.AuditDataServices.TransportBooking.Test
{
	abstract class BaseDtbMasterBookingUpdateOnlyReplicationSubscriberBaseTest : BaseDtbMasterBookingReplicationSubscriberBaseTest
	{
		public override void TestProcessChangesOnInsert()
		{
			InitialiseTestFields();
			ClearEntities();
			SetupPreparatoryData();
			SetupMasterAndSubEntities();
			UpdateInsertedMaster();
			SubscriberFactory.Save();
			UpdateEntities(new BusinessObject[] { MasterEntity }, new Tuple<SchemaColumn, object>(MasterBookingVersionColumn, (short)1));
			UpdateEntities(new BusinessObject[] { SubEntity1, SubEntity2 }, new Tuple<SchemaColumn, object>(MasterBookingVersionColumn, (short)32767));
			SubscriberFactory.Save();

			var changeTable = GetTestDataTable();
			var insertRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.Insert, MasterEntity);
			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory();
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);
			AssertSubsUpdatedRelevantFieldsFromMaster(insertRow, SubEntity1, SubEntity2);

			AuditTestHelper.AssertLog(logger.Logs, ExpectedInsertLogs);
		}

		public override void TestFailedInsertReplication()
		{
			InitialiseTestFields();
			ClearEntities();
			SetupPreparatoryData();
			SetupMasterAndSubEntities();
			UpdateInsertedMaster();
			SubscriberFactory.Save();
			UpdateEntities(new BusinessObject[] { MasterEntity }, new Tuple<SchemaColumn, object>(MasterBookingVersionColumn, (short)1));
			UpdateEntities(new BusinessObject[] { SubEntity1, SubEntity2 }, new Tuple<SchemaColumn, object>(MasterBookingVersionColumn, (short)32767));
			SubscriberFactory.Save();

			var changeTable = GetTestDataTable();
			var insertRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.Insert, MasterEntity);
			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory(throwTestErrorOnSaveToFactory: true);
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			ReloadSubEntitiesFromCurrentFactory(subscriber);
			AssertSubsDidNotUpdateReplicationFieldsFromMasterDueToError(insertRow, SubEntity1, SubEntity2);
			AuditTestHelper.AssertLog(logger.Logs, ExpectedInsertFailedLogs);
			AssertCorrectFailedInsertErrorNotesWereCreated();
			AssertNotEquals("Because of error subscriber.Factory should have been re-created", SubscriberFactory, subscriber.Factory);
		}

		protected abstract void UpdateInsertedMaster();
	}
}
