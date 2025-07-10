using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.AuditDataServices.TransportBooking.Subscribers;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.TransportBooking.Test
{
	[TestedType(typeof(DtbBookingConsolidationMasterBookingReplicationSubscriber))]
	class DtbBookingConsolidationMasterBookingReplicationSubscriberTest : BaseDtbMasterBookingUpdateOnlyReplicationSubscriberBaseTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new DtbBookingConsolidationMasterBookingReplicationSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals("Should include master booking consolidations", 1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			row[DtbBookingConsolidationSchema.PK.Name] = Guid.NewGuid();
			if (shouldBeFiltered)
			{
				row[DtbBookingConsolidationSchema.KB_IsMaster.Name] = false;
			}
			else
			{
				row[DtbBookingConsolidationSchema.KB_IsMaster.Name] = true;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override void SetupPreparatoryData()
		{
		}

		protected override void SetupMasterAndSubEntities()
		{
			MasterEntity = (BusinessObject)Helper.CreateConsolidation();
			MasterEntity[DtbBookingConsolidationSchema.KB_IsMaster] = true;
			MasterEntity[DtbBookingConsolidationSchema.KB_MasterBookingVersion] = (short)1;
			MasterEntity[DtbBookingConsolidationSchema.KB_JobDirection] = "PIC";
			MasterEntity[DtbBookingConsolidationSchema.KB_Status] = "ACR";
			var masterDtbBooking = (BusinessObject)Helper.CreateBooking((IDtbBookingConsolidation)MasterEntity);
			masterDtbBooking[DtbBookingSchema.KM_IsMaster] = true;
			masterDtbBooking[DtbBookingSchema.KM_MasterBookingVersion] = (short)1;

			SubEntity1 = (BusinessObject)Helper.CreateConsolidation();
			SubEntity2 = (BusinessObject)Helper.CreateConsolidation();
			SubEntity1[DtbBookingConsolidationSchema.KB_KB_MasterBookingConsolidation] = SubEntity2[DtbBookingConsolidationSchema.KB_KB_MasterBookingConsolidation] = MasterEntity.PK;
			var subDtbBooking1 = (BusinessObject)Helper.CreateBooking((IDtbBookingConsolidation)SubEntity1);
			subDtbBooking1[DtbBookingSchema.KM_MasterBookingVersion] = (short)32767;
			var subDtbBooking2 = (BusinessObject)Helper.CreateBooking((IDtbBookingConsolidation)SubEntity2);
			subDtbBooking2[DtbBookingSchema.KM_MasterBookingVersion] = (short)32767;
			subDtbBooking1[DtbBookingSchema.KM_KM_MasterBooking] = subDtbBooking2[DtbBookingSchema.KM_KM_MasterBooking] = masterDtbBooking.PK;
			masterDtbBooking[DtbBookingSchema.KM_Status] = "ACR";

			UpdateEntities(
				new BusinessObject[] { MasterEntity, SubEntity1, SubEntity2 },
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_JobDirection, "PIC"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_Status, "AVL"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_SystemLastEditUser, "ME"));
		}

		protected override void UpdateMasterEntity()
		{
			UpdateEntities(
				new BusinessObject[] { MasterEntity },
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_JobDirection, "DLV"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_JobID, "MC0001A"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_GoodsDescription, "New Goods Description"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_IsOverridden, true),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_SystemLastEditUser, "ME"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_Status, "SVM"));
		}

		protected override void UpdateInsertedMaster()
		{
			UpdateEntities(
				new BusinessObject[] { MasterEntity },
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_JobDirection, "DLV"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_GoodsDescription, "New Goods Description"),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_IsOverridden, true),
				new Tuple<SchemaColumn, object>(DtbBookingConsolidationSchema.KB_Status, "SVM"));
		}

		protected override BaseDtbMasterBookingReplicationSubscriber NewDataChangeSubscriberUsingFactory(bool throwTestErrorOnSaveToFactory = false, bool onlyThrowTestErrorOnSaveToFactoryOnce = false)
		{
			var subscriber = new DtbBookingConsolidationMasterBookingReplicationSubscriber(SubscriberFactory);
			subscriber.ThrowTestErrorOnSaveToFactory = throwTestErrorOnSaveToFactory;
			subscriber.OnlyThrowTestErrorOnSaveToFactoryOnce = onlyThrowTestErrorOnSaveToFactoryOnce;
			return subscriber;
		}

		protected override IEnumerable<SchemaColumn> DataTableColumns => new SchemaColumn[]
		{
			DtbBookingConsolidationSchema.PK,
			DtbBookingConsolidationSchema.KB_IsMaster,
			DtbBookingConsolidationSchema.KB_MasterBookingVersion,
			DtbBookingConsolidationSchema.KB_JobDirection,
			DtbBookingConsolidationSchema.KB_Status,
			DtbBookingConsolidationSchema.KB_JobID,
			DtbBookingConsolidationSchema.KB_GoodsDescription,
			DtbBookingConsolidationSchema.KB_IsOverridden,
			DtbBookingConsolidationSchema.KB_SystemLastEditTimeUtc,
		};

		protected override IEnumerable<SchemaColumn> ColumnsToCheckForNonReplication => new SchemaColumn[]
		{
			DtbBookingConsolidationSchema.KB_JobID,
			DtbBookingConsolidationSchema.KB_GoodsDescription,
			DtbBookingConsolidationSchema.KB_IsOverridden,
			DtbBookingConsolidationSchema.KB_Status,
		};

		protected override ITableSchema ExpectedSubscriberTable => DtbBookingConsolidationSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSubscriberSpecificColumns => new SchemaColumn[]
		{
			DtbBookingConsolidationSchema.PK,
			DtbBookingConsolidationSchema.KB_IsMaster,
			DtbBookingConsolidationSchema.KB_MasterBookingVersion,
			DtbBookingConsolidationSchema.KB_JobID,
			DtbBookingConsolidationSchema.KB_JobDirection,
			DtbBookingConsolidationSchema.KB_SystemLastEditTimeUtc,
		};

		protected override string ExpectedSubscriberCode => "MBC";

		protected override string ExpectedDescription => "DtbBookingConsolidation Master Booking Replication Change Subscriber";

		protected override bool ExpectedIsBaseTableDescendentOfDtbBooking => false;

		protected override void AssertPreconditionsOnMasterAndSubEntities()
		{
			CombineAssertions("Precondition: Should have initialised MasterBookingVersion correctly on master and subs", () =>
			{
				AssertEquals("Precondition: master booking consolidation should be on MasterBookingVersion 1", (short)1, MasterEntity[DtbBookingConsolidationSchema.KB_MasterBookingVersion]);
				AssertEquals("Precondition: sub booking consolidation 1 should be on MasterBookingVersion 1", (short)1, SubEntity1[DtbBookingConsolidationSchema.KB_MasterBookingVersion]);
				AssertEquals("Precondition: sub booking consolidation 2 should be on MasterBookingVersion 1", (short)1, SubEntity2[DtbBookingConsolidationSchema.KB_MasterBookingVersion]);
			});
		}

		protected override void AssertPreconditionsAfterUpdateOnMasterEntity()
		{
			AssertEquals("Precondition: master booking consolidation should be on MasterBookingVersion 2", (short)2, MasterEntity[DtbBookingConsolidationSchema.KB_MasterBookingVersion]);
		}

		protected override BetterLogForTest[] ExpectedUpdateLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated sub Booking Consolidation Job ID {SubEntity1[DtbBookingConsolidationSchema.KB_JobID]} for master Booking Consolidation Job ID MC0001A on MasterBookingVersion 2")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated sub Booking Consolidation Job ID {SubEntity1[DtbBookingConsolidationSchema.KB_JobID]} for master Booking Consolidation Job ID MC0001A on MasterBookingVersion 2")),
			new BetterLogForTest(LogType.Information, "Updated 2 subs successfully, 0 failed for master Booking Consolidation Job ID MC0001A on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedInsertLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated sub Booking Consolidation Job ID {SubEntity1[DtbBookingConsolidationSchema.KB_JobID]} for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 1")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated sub Booking Consolidation Job ID {SubEntity2[DtbBookingConsolidationSchema.KB_JobID]} for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 1")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated 2 subs successfully, 0 failed for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 1")),
		};

		protected override BetterLogForTest[] ExpectedUpdateFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, FormattableString.Invariant($"Update of Booking Consolidation for sub Consolidation Job ID {SubEntity1[DtbBookingConsolidationSchema.KB_JobID]} for master Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2 failed with exception Test Replication Error")),
			new BetterLogForTest(LogType.Error, FormattableString.Invariant($"Update of Booking Consolidation for sub Consolidation Job ID {SubEntity2[DtbBookingConsolidationSchema.KB_JobID]} for master Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2 failed with exception Test Replication Error")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated 0 subs successfully, 2 failed for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2")),
		};

		protected override BetterLogForTest[] ExpectedInsertFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, FormattableString.Invariant($"Update of Booking Consolidation for sub Consolidation Job ID {SubEntity1[DtbBookingConsolidationSchema.KB_JobID]} for master Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 1 failed with exception Test Replication Error")),
			new BetterLogForTest(LogType.Error, FormattableString.Invariant($"Update of Booking Consolidation for sub Consolidation Job ID {SubEntity2[DtbBookingConsolidationSchema.KB_JobID]} for master Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 1 failed with exception Test Replication Error")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated 0 subs successfully, 2 failed for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 1")),
		};

		protected override BetterLogForTest[] ExpectedFailureThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, FormattableString.Invariant($"Update of Booking Consolidation for sub Consolidation Job ID {SubEntity1[DtbBookingConsolidationSchema.KB_JobID]} for master Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2 failed with exception Test Replication Error")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated sub Booking Consolidation Job ID {SubEntity2[DtbBookingConsolidationSchema.KB_JobID]} for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated 1 subs successfully, 1 failed for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2")),
		};

		protected override BetterLogForTest[] ExpectedFailureWithFailedErrorNoteProcessingThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated sub Booking Consolidation Job ID {SubEntity2[DtbBookingConsolidationSchema.KB_JobID]} for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2")),
			new BetterLogForTest(LogType.Information, FormattableString.Invariant($"Updated 1 subs successfully, 1 failed for master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2")),
		};

		protected override string GetExpectedUpdateReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity)
		{
			return FormattableString.Invariant($"Update of Booking Consolidation for sub Consolidation Job ID {subEntity[DtbBookingConsolidationSchema.KB_JobID]} for master Consolidation Job ID {masterEntity[DtbBookingConsolidationSchema.KB_JobID]} on Master Booking Version 2 failed with exception Test Replication Error");
		}

		protected override string GetExpectedInsertReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity)
		{
			return FormattableString.Invariant($"Update of Booking Consolidation for sub Consolidation Job ID {subEntity[DtbBookingConsolidationSchema.KB_JobID]} for master Consolidation Job ID {masterEntity[DtbBookingConsolidationSchema.KB_JobID]} on Master Booking Version 1 failed with exception Test Replication Error");
		}

		protected override string ExpectedFailedErrorNoteProcessingErrorReportMessage =>
			FormattableString.Invariant($"Error while handling error replicating master Booking Consolidation Job ID {MasterEntity[DtbBookingConsolidationSchema.KB_JobID]} on MasterBookingVersion 2");

		protected override BusinessObject MasterDtbBooking => (BusinessObject)(MasterEntity as IDtbBookingConsolidation)?.Bookings.FirstOrDefault();
		protected override BusinessObject SubDtbBooking1 => (BusinessObject)(SubEntity1 as IDtbBookingConsolidation)?.Bookings.FirstOrDefault();
		protected override BusinessObject SubDtbBooking2 => (BusinessObject)(SubEntity2 as IDtbBookingConsolidation)?.Bookings.FirstOrDefault();
	}
}
