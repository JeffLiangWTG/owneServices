using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.AuditDataServices.TransportBooking.Subscribers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.TransportBooking.Test
{
	[TestedType(typeof(DtbBookingMasterBookingReplicationSubscriber))]
	class DtbBookingMasterBookingReplicationSubscriberTest : BaseDtbMasterBookingUpdateOnlyReplicationSubscriberBaseTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new DtbBookingMasterBookingReplicationSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals("Should include master bookings", 1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			row[DtbBookingSchema.PK.Name] = Guid.NewGuid();
			if (shouldBeFiltered)
			{
				row[DtbBookingSchema.KM_IsMaster.Name] = false;
			}
			else
			{
				row[DtbBookingSchema.KM_IsMaster.Name] = true;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override void SetupPreparatoryData()
		{
			ClearPreparatoryData();

			BookingRequestedDate1 = DateTime.Now.AddDays(-2);
			BookingRequestedDate2 = DateTime.Now.AddDays(-1);

			TransportCo = Helper.CreateOrganisation("TRANSCO1");

			CarrierAccount1 = SubscriberFactory.New<OrgCarrierAccount>();
			CarrierAccount1.OAN_AccountNumber = "CARRACC1";
			CarrierAccount1.OAN_OH_Carrier = TransportCo.PK;
			CarrierAccount2 = SubscriberFactory.New<OrgCarrierAccount>();
			CarrierAccount2.OAN_AccountNumber = "CARRACC2";
			CarrierAccount2.OAN_OH_Carrier = TransportCo.PK;

			Branch1 = SubscriberFactory.NewWithValidTestData<GlbBranch>();
			Branch1.GB_Code = "BR1";
			Branch1.GB_BranchName = "Branch1";
			Branch2 = SubscriberFactory.NewWithValidTestData<GlbBranch>();
			Branch2.GB_Code = "BR2";
			Branch2.GB_BranchName = "Branch2";
		}

		void ClearPreparatoryData()
		{
			BookingRequestedDate1 = default;
			BookingRequestedDate2 = default;
			TransportCo = default;
			CarrierAccount1 = default;
			CarrierAccount2 = default;
			Branch1 = default;
			Branch2 = default;
		}

		protected override void SetupMasterAndSubEntities()
		{
			MasterEntity = (BusinessObject)Helper.CreateBooking(TransportCo);
			MasterEntity[DtbBookingSchema.KM_IsMaster] = true;
			MasterEntity[DtbBookingSchema.KM_TransportReference] = "MASTER1";
			MasterEntity[DtbBookingSchema.KM_JobID] = "M0001";

			SubEntity1 = (BusinessObject)Helper.CreateBooking();
			SubEntity1[DtbBookingSchema.KM_JobID] = "S0001";
			SubEntity2 = (BusinessObject)Helper.CreateBooking();
			SubEntity2[DtbBookingSchema.KM_JobID] = "S0002";
			SubEntity1[DtbBookingSchema.KM_KM_MasterBooking] = SubEntity2[DtbBookingSchema.KM_KM_MasterBooking] = MasterEntity.PK;

			UpdateEntities(
				new BusinessObject[] { MasterEntity, SubEntity1, SubEntity2 },
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_KT_NKBookingTemplate, "IFUD"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Description, "Old description"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Direction, "PIC"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_IsActive, true),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Status, "AVL"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_RS_NKServiceLevel, "STD"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_PL_NKCarrierServiceLevel, string.Empty),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_RatingFreightMode, "BTH"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Distance, 1),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_DistanceUnit, "KM"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_OAN_CarrierAccount, CarrierAccount1.PK),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_GB_Branch, Branch1.PK),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_BookingOfTransportRequestedDate, BookingRequestedDate1),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_IsAgentBooking, false),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_SystemLastEditUser, "ME"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_TransportMode, "ROA"));
		}

		protected override void UpdateMasterEntity()
		{
			UpdateEntities(
				new BusinessObject[] { MasterEntity },
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_KT_NKBookingTemplate, "LC2C"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Description, "New description"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Direction, "DLV"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_TransportReference, "MASTER2"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_JobID, "M0001A"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_IsActive, false),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Status, "HLD"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_RS_NKServiceLevel, "EXP"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_PL_NKCarrierServiceLevel, "EXP"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_RatingFreightMode, "LSE"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_Distance, 2),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_DistanceUnit, "MI"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_OAN_CarrierAccount, CarrierAccount2.PK),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_GB_Branch, Branch2.PK),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_BookingOfTransportRequestedDate, BookingRequestedDate2),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_IsAgentBooking, true),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_SystemLastEditUser, "ME"),
				new Tuple<SchemaColumn, object>(DtbBookingSchema.KM_TransportMode, "RAI"));
		}

		protected override void UpdateInsertedMaster() => UpdateMasterEntity();

		protected override BaseDtbMasterBookingReplicationSubscriber NewDataChangeSubscriberUsingFactory(bool throwTestErrorOnSaveToFactory = false, bool onlyThrowTestErrorOnSaveToFactoryOnce = false)
		{
			var subscriber = new DtbBookingMasterBookingReplicationSubscriber(SubscriberFactory);
			subscriber.ThrowTestErrorOnSaveToFactory = throwTestErrorOnSaveToFactory;
			subscriber.OnlyThrowTestErrorOnSaveToFactoryOnce = onlyThrowTestErrorOnSaveToFactoryOnce;
			return subscriber;
		}

		protected override IEnumerable<SchemaColumn> DataTableColumns => new SchemaColumn[]
		{
			DtbBookingSchema.PK,
			DtbBookingSchema.KM_IsMaster,
			DtbBookingSchema.KM_TransportReference,
			DtbBookingSchema.KM_MasterBookingVersion,
			DtbBookingSchema.KM_JobID,
			DtbBookingSchema.KM_KT_NKBookingTemplate,
			DtbBookingSchema.KM_IsActive,
			DtbBookingSchema.KM_Status,
			DtbBookingSchema.KM_Direction,
			DtbBookingSchema.KM_Description,
			DtbBookingSchema.KM_RS_NKServiceLevel,
			DtbBookingSchema.KM_PL_NKCarrierServiceLevel,
			DtbBookingSchema.KM_RatingFreightMode,
			DtbBookingSchema.KM_Distance,
			DtbBookingSchema.KM_DistanceUnit,
			DtbBookingSchema.KM_OAN_CarrierAccount,
			DtbBookingSchema.KM_GB_Branch,
			DtbBookingSchema.KM_BookingOfTransportRequestedDate,
			DtbBookingSchema.KM_IsAgentBooking,
			DtbBookingSchema.KM_TransportMode,
		};

		protected override IEnumerable<SchemaColumn> ColumnsToCheckForNonReplication => new SchemaColumn[]
		{
			DtbBookingSchema.KM_JobID,
		};

		protected override ITableSchema ExpectedSubscriberTable => DtbBookingSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSubscriberSpecificColumns => new SchemaColumn[]
		{
			DtbBookingSchema.PK,
			DtbBookingSchema.KM_KB_Booking,
			DtbBookingSchema.KM_KB_BookingConsolidationMultiJob,
			DtbBookingSchema.KM_IsMaster,
			DtbBookingSchema.KM_TransportReference,
			DtbBookingSchema.KM_MasterBookingVersion,
			DtbBookingSchema.KM_KT_NKBookingTemplate,
			DtbBookingSchema.KM_IsActive,
			DtbBookingSchema.KM_Status,
			DtbBookingSchema.KM_Direction,
			DtbBookingSchema.KM_Description,
			DtbBookingSchema.KM_RS_NKServiceLevel,
			DtbBookingSchema.KM_PL_NKCarrierServiceLevel,
			DtbBookingSchema.KM_RatingFreightMode,
			DtbBookingSchema.KM_Distance,
			DtbBookingSchema.KM_DistanceUnit,
			DtbBookingSchema.KM_OAN_CarrierAccount,
			DtbBookingSchema.KM_GB_Branch,
			DtbBookingSchema.KM_BookingOfTransportRequestedDate,
			DtbBookingSchema.KM_IsAgentBooking,
			DtbBookingSchema.KM_JobID,
			DtbBookingSchema.KM_SystemLastEditTimeUtc,
			DtbBookingSchema.KM_TransportMode,
		};

		protected override string ExpectedSubscriberCode => "MBB";

		protected override string ExpectedDescription => "DtbBooking Master Booking Replication Change Subscriber";

		protected override bool ExpectedIsBaseTableDescendentOfDtbBooking => false;

		protected override void AssertPreconditionsOnMasterAndSubEntities()
		{
			CombineAssertions("Precondition: Should have initialised MasterBookingVersion correctly on master and subs", () =>
			{
				AssertEquals("Precondition: master booking should be on MasterBookingVersion 1", (short)1, MasterEntity[DtbBookingSchema.KM_MasterBookingVersion]);
				AssertEquals("Precondition: sub booking 1 should be on MasterBookingVersion 1", (short)1, SubEntity1[DtbBookingSchema.KM_MasterBookingVersion]);
				AssertEquals("Precondition: sub booking 2 should be on MasterBookingVersion 1", (short)1, SubEntity2[DtbBookingSchema.KM_MasterBookingVersion]);
			});
		}

		protected override void AssertPreconditionsAfterUpdateOnMasterEntity()
		{
			AssertEquals("Precondition: master booking should be on MasterBookingVersion 2", (short)2, MasterEntity[DtbBookingSchema.KM_MasterBookingVersion]);
		}

		protected override BetterLogForTest[] ExpectedUpdateLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Updated sub Booking Job ID S0001 for master Booking Job ID M0001A on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated sub Booking Job ID S0002 for master Booking Job ID M0001A on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 2 subs successfully, 0 failed for master Booking Job ID M0001A on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedInsertLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Updated sub Booking Job ID S0001 for master Booking Job ID M0001A on MasterBookingVersion 1"),
			new BetterLogForTest(LogType.Information, "Updated sub Booking Job ID S0002 for master Booking Job ID M0001A on MasterBookingVersion 1"),
			new BetterLogForTest(LogType.Information, "Updated 2 subs successfully, 0 failed for master Booking Job ID M0001A on MasterBookingVersion 1"),
		};

		protected override BetterLogForTest[] ExpectedUpdateFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Update of Booking for sub Booking Job ID S0001 for master Booking Job ID M0001A on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Error, "Update of Booking for sub Booking Job ID S0002 for master Booking Job ID M0001A on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Updated 0 subs successfully, 2 failed for master Booking Job ID M0001A on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedInsertFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Update of Booking for sub Booking Job ID S0001 for master Booking Job ID M0001A on MasterBookingVersion 1 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Error, "Update of Booking for sub Booking Job ID S0002 for master Booking Job ID M0001A on MasterBookingVersion 1 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Updated 0 subs successfully, 2 failed for master Booking Job ID M0001A on MasterBookingVersion 1"),
		};

		protected override BetterLogForTest[] ExpectedFailureThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Update of Booking for sub Booking Job ID S0001 for master Booking Job ID M0001A on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Updated sub Booking Job ID S0002 for master Booking Job ID M0001A on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 1 subs successfully, 1 failed for master Booking Job ID M0001A on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedFailureWithFailedErrorNoteProcessingThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Updated sub Booking Job ID S0002 for master Booking Job ID M0001A on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 1 subs successfully, 1 failed for master Booking Job ID M0001A on MasterBookingVersion 2"),
		};

		protected override string GetExpectedUpdateReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity)
		{
			return FormattableString.Invariant($"Update of Booking for sub Booking Job ID {subEntity[DtbBookingSchema.KM_JobID]} for master Booking Job ID {masterEntity[DtbBookingSchema.KM_JobID]} on Master Booking Version 2 failed with exception Test Replication Error");
		}

		protected override string GetExpectedInsertReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity)
		{
			return FormattableString.Invariant($"Update of Booking for sub Booking Job ID {subEntity[DtbBookingSchema.KM_JobID]} for master Booking Job ID {masterEntity[DtbBookingSchema.KM_JobID]} on Master Booking Version 1 failed with exception Test Replication Error");
		}

		protected override string ExpectedFailedErrorNoteProcessingErrorReportMessage =>
			"Error while handling error replicating master Booking Job ID M0001A on MasterBookingVersion 2";

		protected override BusinessObject MasterDtbBooking => MasterEntity;
		protected override BusinessObject SubDtbBooking1 => SubEntity1;
		protected override BusinessObject SubDtbBooking2 => SubEntity2;

		DateTime BookingRequestedDate1 { get; set; }
		DateTime BookingRequestedDate2 { get; set; }
		OrgHeader TransportCo { get; set; }
		OrgCarrierAccount CarrierAccount1 { get; set; }
		OrgCarrierAccount CarrierAccount2 { get; set; }
		GlbBranch Branch1 { get; set; }
		GlbBranch Branch2 { get; set; }
	}
}
