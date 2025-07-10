using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.AuditDataServices.TransportBooking.Subscribers;
using Enterprise.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.AuditDataServices.TransportBooking.Test
{
	abstract class BaseDtbMasterBookingReplicationSubscriberBaseTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestProperties()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();
			CombineAssertions("Properties for DtbBookingMasterBookingReplicationSubscriber should be set correctly", () =>
			{
				AssertStartsWith("DtbBookingMasterReplicationSubscribers should only apply to DtbBookingXXX tables", "DtbBooking", subscriber.Table.TableName);
				AssertEquals(FormattableString.Invariant($"Should be using the schema for {ExpectedSubscriberTable.TableName}"), ExpectedSubscriberTable, subscriber.Table);
				AssertContainsExactElementsInAnyOrder(
					"Should focus on specific replication columns and other columns relevant to master booking or to find parent records",
					ExpectedSubscriberSpecificColumns,
					subscriber.SpecificColumns);
				AssertEquals(FormattableString.Invariant($"Should {(ExpectedNotifyInsert ? string.Empty : "not ")}react to the creation of new master booking {ExpectedSubscriberTable} records"), ExpectedNotifyInsert, subscriber.NotifyInsert);
				AssertEquals(FormattableString.Invariant($"Should {(ExpectedNotifyUpdate ? string.Empty : "not ")}react to the updating of master booking {ExpectedSubscriberTable} records"), ExpectedNotifyUpdate, subscriber.NotifyUpdate);
				AssertEquals(FormattableString.Invariant($"Should {(ExpectedNotifyDelete ? string.Empty : "not ")}react to the deletion of master booking {ExpectedSubscriberTable} records"), ExpectedNotifyDelete, subscriber.NotifyDelete);
				AssertEquals("Should have a distinct code", ExpectedSubscriberCode, subscriber.Code);
				AssertEquals("Should have a meaningful description", ExpectedDescription, subscriber.Description);
				using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("IsRequired should be true when MasterBookingsEnabled registry setting is true", true, subscriber.IsRequired());
				}
				using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("IsRequired should be false when MasterBookingsEnabled registry setting is false", false, subscriber.IsRequired());
				}
			});
		}

		public abstract void TestProcessChangesOnInsert();

		public void TestProcessChangesOnUpdate()
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
			var afterUpdateRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.UpdateAfterWrapperFiltering, MasterEntity);

			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory();
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);
			AssertSubsUpdatedRelevantFieldsFromMaster(afterUpdateRow, NonReplicationColumnsToExcludeFromUpdateMismatchCheck, SubEntity1, SubEntity2);

			AuditTestHelper.AssertLog(logger.Logs, ExpectedUpdateLogs);
		}

		public void TestFailedUpdateReplication()
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
			var afterUpdateRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.UpdateAfterWrapperFiltering, MasterEntity);

			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory(throwTestErrorOnSaveToFactory: true);
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			ReloadSubEntitiesFromCurrentFactory(subscriber);
			AssertSubsDidNotUpdateReplicationFieldsFromMasterDueToError(afterUpdateRow, SubEntity1, SubEntity2);
			AuditTestHelper.AssertLog(logger.Logs, ExpectedUpdateFailedLogs);
			AssertCorrectFailedUpdateErrorNotesWereCreated(oneErrorOnly: false);
			AssertFactoryWasRecreated(subscriber);
		}

		public abstract void TestFailedInsertReplication();

		public void TestAfterReplicationErrorContinuesToReplicateOnRemainingRecords()
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
			var afterUpdateRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.UpdateAfterWrapperFiltering, MasterEntity);

			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory(throwTestErrorOnSaveToFactory: true, onlyThrowTestErrorOnSaveToFactoryOnce: true);
			var logger = new BetterLoggerForTest();
			subscriber.ProcessChanges(logger, changeTable);

			ReloadSubEntitiesFromCurrentFactory(subscriber);

			AssertSubsDidNotUpdateReplicationFieldsFromMasterDueToError(afterUpdateRow, SubEntity1);
			AssertSubsUpdatedRelevantFieldsFromMaster(afterUpdateRow, NonReplicationColumnsToExcludeFromPartiallyFailedUpdateMismatchCheck, SubEntity2);

			AuditTestHelper.AssertLog(logger.Logs, ExpectedFailureThenSuccessLogs);

			AssertCorrectFailedUpdateErrorNotesWereCreated(oneErrorOnly: true);

			AssertFactoryWasRecreated(subscriber);
		}

		public void TestAfterReplicationErrorWhileAddingNoteContinuesToReplicateOnRemainingRecords()
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
			var afterUpdateRow = AddNewRowToChangeTable(changeTable, CdcOperationCodes.UpdateAfterWrapperFiltering, MasterEntity);

			changeTable.AcceptChanges();

			var subscriber = NewDataChangeSubscriberUsingFactory(throwTestErrorOnSaveToFactory: true, onlyThrowTestErrorOnSaveToFactoryOnce: true);
			var logger = new BetterLoggerForTestThatThrowsOnFirstError();
			AssertNoExceptionThrown("Should not throw error but should send ErrorReport", () => subscriber.ProcessChanges(logger, changeTable));

			ReloadSubEntitiesFromCurrentFactory(subscriber);

			AssertSubsDidNotUpdateReplicationFieldsFromMasterDueToError(afterUpdateRow, SubEntity1);
			AssertSubsUpdatedRelevantFieldsFromMaster(afterUpdateRow, NonReplicationColumnsToExcludeFromPartiallyFailedUpdateMismatchCheck, SubEntity2);

			AuditTestHelper.AssertLog(logger.Logs, ExpectedFailureWithFailedErrorNoteProcessingThenSuccessLogs);

			AssertFactoryWasRecreated(subscriber);

			AssertCorrectErrorReportWasCreated();

			ErrorReporter.Clear();
		}

		protected void ClearEntities()
		{
			MasterEntity = SubEntity1 = SubEntity2 = null;
		}

		protected abstract void SetupPreparatoryData();
		protected abstract void SetupMasterAndSubEntities();
		protected abstract void UpdateMasterEntity();

		protected abstract void AssertPreconditionsOnMasterAndSubEntities();
		protected abstract void AssertPreconditionsAfterUpdateOnMasterEntity();

		protected void UpdateEntities(BusinessObject[] entities, params Tuple<SchemaColumn, object>[] changes)
		{
			foreach (var entity in entities)
			{
				foreach (var change in changes)
				{
					entity[change.Item1] = change.Item2;
				}
			}
		}

		protected void DeleteMasterEntity()
		{
			MasterEntity.Delete();
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			foreach (var column in DataTableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}

			return changeTable;
		}

		protected void AssertSubsUpdatedRelevantFieldsFromMaster(DataRow afterUpdateRow, params BusinessObject[] subEntities)
		{
			AssertSubsUpdatedRelevantFieldsFromMaster(afterUpdateRow, Array.Empty<SchemaColumn>(), subEntities);
		}

		protected void AssertSubsUpdatedRelevantFieldsFromMaster(DataRow afterUpdateRow, IEnumerable<SchemaColumn> nonReplicationColumnsToExcludeFromMismatchCheck, params BusinessObject[] subEntities)
		{
			CombineAssertions("Check that master booking relevant updated fields are updated on subs and non-relevant fields are not updated", () =>
			{
				for (var subNo = 0; subNo < subEntities.Length; subNo++)
				{
					var sub = subEntities[subNo];
					foreach (var replicatingColumn in ColumnsToCheckForReplication)
					{
						AssertSubHasMatchingField(afterUpdateRow, "update", replicatingColumn, sub, subNo);
					}
					foreach (var nonReplicatingColumn in ColumnsToCheckForNonReplication.Where(c => !nonReplicationColumnsToExcludeFromMismatchCheck.Contains(c)))
					{
						AssertSubHasNonMatchingField(afterUpdateRow, "update", nonReplicatingColumn, sub, subNo);
					}
				}
			});
		}

		protected void AssertSubsDidNotUpdateReplicationFieldsFromMasterDueToError(DataRow dataChangeRow, params BusinessObject[] subEntities)
		{
			CombineAssertions("Check that master booking relevant updated fields are updated on subs and non-relevant fields are not updated", () =>
			{
				for (var subNo = 0; subNo < subEntities.Length; subNo++)
				{
					var sub = subEntities[subNo];
					foreach (var replicatingColumn in ColumnsToCheckForReplication)
					{
						AssertSubHasNonMatchingField(dataChangeRow, "update", replicatingColumn, sub, subNo, dueToError: true);
					}
				}
			});
		}

		protected void AssertSubHasMatchingField(DataRow changeDataRow, string changeDataRowDescription, SchemaColumn column, BusinessObject sub, int subNo)
		{
			var changeDataRowValue = GetChangeDataRowValueToCompareBusinessObjectPropertyTo(changeDataRow, column, sub);
			AssertEquals(FormattableString.Invariant($"Sub #{subNo} {column.Name} should be as per the {changeDataRowDescription} row as it is a relevant replication field"), changeDataRowValue, sub[column]);
		}

		protected void AssertSubHasNonMatchingField(DataRow changeDataRow, string changeDataRowDescription, SchemaColumn column, BusinessObject sub, int subNo, bool dueToError = false)
		{
			var reason = dueToError ? "as replication failed due to an error" : "as it is not a relevant replication field";
			var changeDataRowValue = GetChangeDataRowValueToCompareBusinessObjectPropertyTo(changeDataRow, column, sub);
			AssertNotEquals(FormattableString.Invariant($"Sub #{subNo} {column.Name} should be different to the {changeDataRowDescription} row {reason}"), changeDataRowValue, sub[column]);
		}

		void AssertCorrectFailedUpdateErrorNotesWereCreated(bool oneErrorOnly)
		{
			AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking1, GetExpectedUpdateReplicationErrorNoteText(MasterEntity, SubEntity1));
			if (!oneErrorOnly)
			{
				AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking2, GetExpectedUpdateReplicationErrorNoteText(MasterEntity, SubEntity2));
			}
		}

		protected virtual void AssertCorrectFailedInsertErrorNotesWereCreated()
		{
			AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking1, GetExpectedInsertReplicationErrorNoteText(MasterEntity, SubEntity1));
			AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking2, GetExpectedInsertReplicationErrorNoteText(MasterEntity, SubEntity2));
		}

		void AssertCorrectErrorReportWasCreated()
		{
			AssertEquals("Should have reported an error for failed error note creation", 1, ErrorReporter.TotalErrorCount);
			AssertStartsWith("Error report message for failed error note creation should be correct", ExpectedFailedErrorNoteProcessingErrorReportMessage, ErrorReporter.LastMessageReported);
			var lastExceptionReported = ErrorReporter.LastExceptionReported;
			AssertEquals("Last exception should be AggregateException", typeof(AggregateException), lastExceptionReported.GetType());
			var lastExceptionReportedAggregate = (AggregateException)lastExceptionReported;
			AssertEquals("Last exception should be AggregateException with 2 exceptions", 2, lastExceptionReportedAggregate.InnerExceptions.Count);
			var exceptionFromMainProcessLoop = lastExceptionReportedAggregate.InnerExceptions[0];
			var exceptionFromNoteProcessing = lastExceptionReportedAggregate.InnerExceptions[1];
			AssertEquals("First exception (for main process loop) should have correct message", "Test Replication Error", exceptionFromMainProcessLoop.Message);
			AssertEquals("Second exception (for failed error note creation) should have correct message", "Test Logging Error", exceptionFromNoteProcessing.Message);
		}

		protected abstract string GetExpectedUpdateReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity);

		protected abstract string GetExpectedInsertReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity);

		protected void AssertHasCorrectMasterBookingReplicationErrorNote(BusinessObject dtbBooking, string expectedNoteText)
		{
			var note = (StmNote)dtbBooking.GetNotes().GetAllNotes().ToArray().SingleOrDefault();
			AssertNotNull("Should have created error note", note);
			CombineAssertions("Should have created correct type of error note", () =>
			{
				AssertEquals("Note should have correct description", ReplicationErrorNoteDescription, note.ST_Description);
				AssertEquals("Should be custom note", true, note.ST_IsCustomDescription);
				AssertEquals(expectedNoteText, note.ST_NoteDataAsText);
			});
		}

		protected void AssertFactoryWasRecreated(BaseDtbMasterBookingReplicationSubscriber subscriber)
		{
			AssertNotEquals("Because of error subscriber.Factory should have been re-created", SubscriberFactory._Instance, subscriber.Factory._Instance);
		}

		object GetChangeDataRowValueToCompareBusinessObjectPropertyTo(DataRow changeDataRow, SchemaColumn column, BusinessObject sub)
		{
			switch (sub[column])
			{
				case ZGuid:
					return (changeDataRow[column.Name] == DBNull.Value) ? ZGuid.Empty : new ZGuid(changeDataRow[column.Name]);

				case ZBlob:
					return (changeDataRow[column.Name] == DBNull.Value) ? ZBlob.Empty : new ZBlob(changeDataRow[column.Name]);

				case ZDateTime:
					return (changeDataRow[column.Name] == DBNull.Value) ? ZDateTime.Empty : new ZDateTime(changeDataRow[column.Name]);

				case ZDateTimeOffset:
					return (changeDataRow[column.Name] == DBNull.Value) ? ZDateTimeOffset.Empty : new ZDateTimeOffset(changeDataRow[column.Name]);

				default:
					return changeDataRow[column.Name];
			}
		}

		protected void AssertSubsDeleted(params BusinessObject[] subEntities)
		{
			CombineAssertions("Check that sub entities have been deleted", () =>
			{
				for (var subNo = 0; subNo < subEntities.Length; subNo++)
				{
					var sub = subEntities[subNo];
					Assert($"Sub {subNo} should be deleted", sub.IsDeleted);
				}
			});
		}

		protected void ReloadSubEntitiesFromCurrentFactory(BaseDtbMasterBookingReplicationSubscriber subscriber)
		{
			SubEntity1 = subscriber.ReloadEntityFromCurrentFactory(SubEntity1);
			SubEntity2 = subscriber.ReloadEntityFromCurrentFactory(SubEntity2);
		}

		protected abstract IEnumerable<SchemaColumn> DataTableColumns { get; }

		protected SchemaColumn MasterBookingVersionColumn
		{
			get
			{
				return ExpectedSubscriberTable.GetSchemaColumn(ExpectedSubscriberTable.PK.ColumnPrefix + "_MasterBookingVersion");
			}
		}

		protected IEnumerable<SchemaColumn> ColumnsToCheckForReplication => new SchemaColumn[] { MasterBookingVersionColumn }.Concat(DtbMasterBookingReplication.GetReplicatedColumnsForTable(ExpectedSubscriberTable.TableName));

		protected virtual IEnumerable<SchemaColumn> NonReplicationColumnsToExcludeFromPartiallyFailedUpdateMismatchCheck => Array.Empty<SchemaColumn>();

		protected virtual IEnumerable<SchemaColumn> NonReplicationColumnsToExcludeFromUpdateMismatchCheck => Array.Empty<SchemaColumn>();

		protected abstract IEnumerable<SchemaColumn> ColumnsToCheckForNonReplication { get; }

		protected abstract BaseDtbMasterBookingReplicationSubscriber NewDataChangeSubscriberUsingFactory(bool throwTestErrorOnSaveToFactory = false, bool onlyThrowTestErrorOnSaveToFactoryOnce = false);

		protected DataRow AddNewRowToChangeTable(DataTable changeTable, int operation, BusinessObject entity)
		{
			var changeRow = changeTable.NewRow();
			changeRow[AuditFieldNames.OperationFieldName] = operation;
			foreach (var column in DataTableColumns)
			{
				if (column.DotNetType == typeof(Guid))
				{
					var zguidValue = (ZGuid)entity[column];
					changeRow[column.Name] = zguidValue.IsEmpty ? DBNull.Value : zguidValue.ToGuid();
				}
				else if (column.DotNetType == typeof(bool))
				{
					changeRow[column.Name] = ((ZBool)entity[column.Name]).Equals(new ZBool(true));
				}
				else if (column.DotNetType == typeof(short))
				{
					changeRow[column.Name] = (short)(ZShort)entity[column.Name];
				}
				else if (column.DotNetType == typeof(int))
				{
					changeRow[column.Name] = (int)(ZInt)entity[column.Name];
				}
				else if (column.DotNetType == typeof(decimal))
				{
					changeRow[column.Name] = (decimal)(ZDecimal)entity[column.Name];
				}
				else if (column.DotNetType == typeof(DateTime))
				{
					changeRow[column.Name] = ((ZDateTime)entity[column.Name]).ToDateTime();
				}
				else if (column.DotNetType == typeof(byte[]))
				{
					changeRow[column.Name] = ((ZBlob)entity[column.Name]).XmlSerializedValue;
				}
				else
				{
					changeRow[column.Name] = entity[column.Name];
				}
			}
			changeTable.Rows.Add(changeRow);

			return changeRow;
		}

		bool ExpectedNotifyUpdate => true;

		bool ExpectedNotifyInsert => true;

		bool ExpectedNotifyDelete => false;

		protected abstract bool ExpectedIsBaseTableDescendentOfDtbBooking { get; }

		protected abstract ITableSchema ExpectedSubscriberTable { get; }

		protected abstract IEnumerable<SchemaColumn> ExpectedSubscriberSpecificColumns { get; }

		protected abstract string ExpectedSubscriberCode { get; }

		protected abstract string ExpectedDescription { get; }

		protected abstract BetterLogForTest[] ExpectedUpdateLogs { get; }

		protected abstract BetterLogForTest[] ExpectedInsertLogs { get; }

		protected abstract BetterLogForTest[] ExpectedUpdateFailedLogs { get; }

		protected abstract BetterLogForTest[] ExpectedInsertFailedLogs { get; }

		protected abstract BetterLogForTest[] ExpectedFailureThenSuccessLogs { get; }

		protected abstract BetterLogForTest[] ExpectedFailureWithFailedErrorNoteProcessingThenSuccessLogs { get; }

		protected abstract string ExpectedFailedErrorNoteProcessingErrorReportMessage { get; }

		protected void InitialiseTestFields()
		{
			SubscriberFactory = new BusinessObjectFactory();
			MasterEntity = SubEntity1 = SubEntity2 = null;
		}

		protected BusinessObjectFactory SubscriberFactory { get; set; }

		protected BusinessObject MasterEntity { get; set; }
		protected BusinessObject SubEntity1 { get; set; }
		protected BusinessObject SubEntity2 { get; set; }

		protected abstract BusinessObject MasterDtbBooking { get; }
		protected abstract BusinessObject SubDtbBooking1 { get; }
		protected abstract BusinessObject SubDtbBooking2 { get; }

		protected ITransportBookingTestHelper Helper => helper ?? (helper = ObjectFactory.New<ITransportBookingTestHelper>(SubscriberFactory));
		protected ITransportBookingTestHelper helper;

		protected const string ReplicationErrorNoteDescription = "Master Booking Replication Error";
	}

	class BetterLoggerForTestThatThrowsOnFirstError : ILogger
	{
		public BetterLoggerForTestThatThrowsOnFirstError()
		{
			InternalLogger = new BetterLoggerForTest();
		}

		public void Log(LogType type, string message, Exception ex)
		{
			if (type == LogType.Error)
			{
				Errors++;
				if (Errors == 1)
				{
					throw new Exception("Test Logging Error");
				}
			}
			InternalLogger.Log(type, message, ex);
		}

		public void Log(LogType type, string message)
		{
			Log(type, message, null);
		}

		public List<BetterLogForTest> Logs => InternalLogger.Logs;

		int Errors { get; set; }

		BetterLoggerForTest InternalLogger { get; }
	}
}
