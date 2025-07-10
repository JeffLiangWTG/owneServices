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
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.TransportBooking.Test
{
	[TestedType(typeof(DtbBookingInstructionMasterBookingReplicationSubscriber))]
	class DtbBookingInstructionMasterBookingReplicationSubscriberTest : BaseDtbMasterBookingInsertAndUpdateReplicationSubscriberBaseTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new DtbBookingInstructionMasterBookingReplicationSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals("Should include master booking instructions", 1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			row[DtbBookingInstructionSchema.PK.Name] = Guid.NewGuid();
			if (shouldBeFiltered)
			{
				row[DtbBookingInstructionSchema.KN_IsMaster.Name] = false;
			}
			else
			{
				row[DtbBookingInstructionSchema.KN_IsMaster.Name] = true;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override void SetupMasterParent()
		{
			SetupMasterBooking();
		}

		protected override void SetupMiscellaneousPreparatoryData()
		{
			Equipment1 = SubscriberFactory.NewWithValidTestData<RefEquipment>();
			var transportProvider = (BusinessObject)SubscriberFactory.New<IRateTransportProvider>();
			transportProvider.FillWithValidTestData();
			Zone1 = (BusinessObject)SubscriberFactory.New<IRateTransportZone>();
			Zone1[RateTransportZonesSchema.TZ_TP] = transportProvider.PK;
		}

		void SetupMasterBooking()
		{
			MasterBooking = Helper.CreateBooking();
			MasterBooking.KM_IsMaster = true;
			MasterBooking.KM_JobID = "M0001";
		}

		protected override void SetupSubParents()
		{
			SetupSubBookings();
		}

		void SetupSubBookings()
		{
			SubBooking1 = Helper.CreateBooking();
			SubBooking1.KM_JobID = "S0001";
			SubBooking2 = Helper.CreateBooking();
			SubBooking2.KM_JobID = "S0002";
			SubBooking1.KM_KM_MasterBooking = SubBooking2.KM_KM_MasterBooking = MasterBooking.PK;
			SubBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = SubBooking2.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = MasterBooking.ConsolidationSingleJob.PK;
		}

		protected override void ClearPreparatoryData()
		{
			MasterBooking = SubBooking1 = SubBooking2 = null;
			Equipment1 = null;
			Zone1 = null;
		}

		protected override void SetupMasterAndSubEntities()
		{
			ClearEntities();

			MasterEntity = (BusinessObject)Helper.CreateInstruction(MasterBooking, "PIC");

			SubEntity1 = (BusinessObject)Helper.CreateInstruction(SubBooking1, "PIC");
			SubEntity2 = (BusinessObject)Helper.CreateInstruction(SubBooking2, "PIC");
			SubEntity1[DtbBookingInstructionSchema.KN_KN_MasterBookingInstruction] = SubEntity2[DtbBookingInstructionSchema.KN_KN_MasterBookingInstruction] = MasterEntity.PK;

			UpdateEntities(
				new BusinessObject[] { MasterEntity, SubEntity1, SubEntity2 },
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_Sequence, 1),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_InstructionType, "PIC"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_DropMode, "ANY"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_ServiceInstruction, "Initial Service Instruction"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_Status, "AVL"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_RQ_Equipment, ZGuid.Empty),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsContainerRateable, false),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsLooseRateable, false),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_TZ_DomesticZone, ZGuid.Empty),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsAuthorisedToLeave, false),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_SystemLastEditUser, "ME"));
		}

		protected override void UpdateMasterEntity()
		{
			UpdateEntities(
				new BusinessObject[] { MasterEntity, },
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_Sequence, 2),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_InstructionType, "DLV"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_DropMode, "ALL"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_ServiceInstruction, "Modified Service Instruction"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_Status, "PIC"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_RQ_Equipment, Equipment1.PK),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsContainerRateable, true),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsLooseRateable, true),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_TZ_DomesticZone, Zone1.PK),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsAuthorisedToLeave, true),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_SystemLastEditUser, "ME"));
		}

		protected override void SetupMasterEntityForInsertTest()
		{
			ClearEntities();

			MasterEntity = (BusinessObject)Helper.CreateInstruction(MasterBooking, "PIC");

			UpdateEntities(
				new BusinessObject[] { MasterEntity },
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_Sequence, 1),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_InstructionType, "PIC"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_DropMode, "ANY"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_ServiceInstruction, "Initial Service Instruction"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_Status, "AVL"),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_RQ_Equipment, ZGuid.Empty),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsContainerRateable, false),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsLooseRateable, false),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_TZ_DomesticZone, ZGuid.Empty),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_IsAuthorisedToLeave, false),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingInstructionSchema.KN_SystemLastEditUser, "ME"));
		}

		protected override BaseDtbMasterBookingReplicationSubscriber NewDataChangeSubscriberUsingFactory(bool throwTestErrorOnSaveToFactory = false, bool onlyThrowTestErrorOnSaveToFactoryOnce = false)
		{
			var subscriber = new DtbBookingInstructionMasterBookingReplicationSubscriber(SubscriberFactory);
			subscriber.ThrowTestErrorOnSaveToFactory = throwTestErrorOnSaveToFactory;
			subscriber.OnlyThrowTestErrorOnSaveToFactoryOnce = onlyThrowTestErrorOnSaveToFactoryOnce;
			return subscriber;
		}

		protected override IEnumerable<SchemaColumn> DataTableColumns => new SchemaColumn[]
		{
			DtbBookingInstructionSchema.PK,
			DtbBookingInstructionSchema.KN_IsMaster,
			DtbBookingInstructionSchema.KN_MasterBookingVersion,
			DtbBookingInstructionSchema.KN_KM_BookingMovement,
			DtbBookingInstructionSchema.KN_Sequence,
			DtbBookingInstructionSchema.KN_InstructionType,
			DtbBookingInstructionSchema.KN_DropMode,
			DtbBookingInstructionSchema.KN_ServiceInstruction,
			DtbBookingInstructionSchema.KN_Status,
			DtbBookingInstructionSchema.KN_RQ_Equipment,
			DtbBookingInstructionSchema.KN_IsContainerRateable,
			DtbBookingInstructionSchema.KN_IsLooseRateable,
			DtbBookingInstructionSchema.KN_TZ_DomesticZone,
			DtbBookingInstructionSchema.KN_IsAuthorisedToLeave,
			DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc,
		};

		protected override IEnumerable<SchemaColumn> ColumnsToCheckForNonReplication => Array.Empty<SchemaColumn>();

		protected override ITableSchema ExpectedSubscriberTable => DtbBookingInstructionSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSubscriberSpecificColumns => new SchemaColumn[]
		{
			DtbBookingInstructionSchema.PK,
			DtbBookingInstructionSchema.KN_IsMaster,
			DtbBookingInstructionSchema.KN_MasterBookingVersion,
			DtbBookingInstructionSchema.KN_KM_BookingMovement,
			DtbBookingInstructionSchema.KN_Sequence,
			DtbBookingInstructionSchema.KN_InstructionType,
			DtbBookingInstructionSchema.KN_DropMode,
			DtbBookingInstructionSchema.KN_ServiceInstruction,
			DtbBookingInstructionSchema.KN_Status,
			DtbBookingInstructionSchema.KN_RQ_Equipment,
			DtbBookingInstructionSchema.KN_IsContainerRateable,
			DtbBookingInstructionSchema.KN_IsLooseRateable,
			DtbBookingInstructionSchema.KN_TZ_DomesticZone,
			DtbBookingInstructionSchema.KN_IsAuthorisedToLeave,
			DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc,
		};

		protected override string ExpectedSubscriberCode => "MBI";

		protected override string ExpectedDescription => "DtbBookingInstruction Master Booking Replication Change Subscriber";

		protected override bool ExpectedIsBaseTableDescendentOfDtbBooking => true;

		protected override void AssertPreconditionsOnMasterAndSubEntities()
		{
			CombineAssertions("Precondition: Should have initialised MasterBookingVersion correctly on master and subs", () =>
			{
				AssertEquals("Precondition: master booking instruction should be on MasterBookingVersion 1", (short)1, MasterEntity[DtbBookingInstructionSchema.KN_MasterBookingVersion]);
				AssertEquals("Precondition: sub booking 1 instruction should be on MasterBookingVersion 1", (short)1, SubEntity1[DtbBookingInstructionSchema.KN_MasterBookingVersion]);
				AssertEquals("Precondition: sub booking 2 instruction should be on MasterBookingVersion 1", (short)1, SubEntity2[DtbBookingInstructionSchema.KN_MasterBookingVersion]);
			});
		}

		protected override void AssertPreconditionsAfterUpdateOnMasterEntity()
		{
			AssertEquals("Precondition: master booking instruction should be on MasterBookingVersion 2", (short)2, MasterEntity[DtbBookingInstructionSchema.KN_MasterBookingVersion]);
		}

		protected override void AssertPreconditionsOnMasterAndSubEntitiesForInsertTest()
		{
			CombineAssertions("Precondition: Set up successfully for CDC Insert or CDC Update testing insert on missing sub instructions", () =>
			{
				AssertEquals("Precondition: master booking instruction should be on MasterBookingVersion 1", (short)1, MasterEntity[DtbBookingInstructionSchema.KN_MasterBookingVersion]);
				Assert("Precondition: sub booking 1 has no sub instructions linked to master booking instruction", !SubBooking1.Instructions.Any(i => i.KN_KN_MasterBookingInstruction == MasterEntity.PK));
				Assert("Precondition: sub booking 2 has no sub instructions linked to master booking instruction", !SubBooking2.Instructions.Any(i => i.KN_KN_MasterBookingInstruction == MasterEntity.PK));
			});
		}

		protected override void AssertSubsInsertedFromMaster(DataRow insertRow, params BusinessObject[] subParents)
		{
			AssertSubsInsertedFromMaster(insertRow, subParents.Cast<IDtbBooking>().ToArray());
		}

		void AssertSubsInsertedFromMaster(DataRow insertRow, params IDtbBooking[] subBookings)
		{
			CombineAssertions("Check that master booking instruction inserted on subs and relevant fields are updated on subs (no non-replicated fields to check for instruction)", () =>
			{
				for (var subNo = 0; subNo < subBookings.Length; subNo++)
				{
					var subBooking = subBookings[subNo];
					var subInstruction = AssertSubInstructionInsertedForMaster(subBooking, subNo);

					if (subInstruction != null)
					{
						foreach (var replicatingColumn in ColumnsToCheckForReplication)
						{
							AssertSubHasMatchingField(insertRow, "insert", replicatingColumn, subInstruction, subNo);
						}
					}
				}
			});
		}

		protected override void AssertSubsNotInsertedFromMaster(DataRow insertRow, params BusinessObject[] subParents)
		{
			AssertSubsNotInsertedFromMaster(insertRow, subParents.Cast<IDtbBooking>().ToArray());
		}

		void AssertSubsNotInsertedFromMaster(DataRow insertRow, params IDtbBooking[] subBookings)
		{
			CombineAssertions("Check that master booking instruction inserted on subs and relevant fields are updated on subs (no non-replicated fields to check for instruction)", () =>
			{
				for (var subNo = 0; subNo < subBookings.Length; subNo++)
				{
					var subBooking = subBookings[subNo];
					var subInstruction = AssertSubInstructionNotInsertedForMaster(subBooking, subNo);
				}
			});
		}

		BusinessObject AssertSubInstructionInsertedForMaster(IDtbBooking subBooking, int subNo)
		{
			var subInstruction = (BusinessObject)subBooking.Instructions.Where(i => i.KN_KN_MasterBookingInstruction == MasterEntity.PK).SingleOrDefault();
			AssertNotNull("Sub Booking #" + subNo + " should now have sub instruction", subInstruction);

			return subInstruction;
		}

		BusinessObject AssertSubInstructionNotInsertedForMaster(IDtbBooking subBooking, int subNo)
		{
			var subInstruction = (BusinessObject)subBooking.Instructions.Where(i => i.KN_KN_MasterBookingInstruction == MasterEntity.PK).SingleOrDefault();
			AssertNull("Sub Booking #" + subNo + " should not have sub instruction", subInstruction);

			return subInstruction;
		}

		protected override BetterLogForTest[] ExpectedUpdateLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Updated sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 2 subs successfully, 0 failed for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedInsertLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Inserted sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
			new BetterLogForTest(LogType.Information, "Inserted sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
			new BetterLogForTest(LogType.Information, "Inserted 2 subs successfully, 0 failed for master Booking Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
		};

		protected override BetterLogForTest[] ExpectedUpdateFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Update of sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Error, "Update of sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Updated 0 subs successfully, 2 failed for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedInsertFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Insert of Booking Instruction of type PIC sequence 1 on sub Booking Job ID S0001 for master Booking Job ID M0001 on MasterBookingVersion 1 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Error, "Insert of Booking Instruction of type PIC sequence 1 on sub Booking Job ID S0002 for master Booking Job ID M0001 on MasterBookingVersion 1 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Inserted 0 subs successfully, 2 failed for master Booking Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
		};

		protected override BetterLogForTest[] ExpectedFailureThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Update of sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Updated sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 1 subs successfully, 1 failed for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedFailureWithFailedErrorNoteProcessingThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Updated sub Booking Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 1 subs successfully, 1 failed for master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override void AssertCorrectFailedInsertErrorNotesWereCreated()
		{
			AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking1, GetExpectedInsertReplicationErrorNoteText(MasterEntity, SubDtbBooking1));
			AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking2, GetExpectedInsertReplicationErrorNoteText(MasterEntity, SubDtbBooking2));
		}

		protected override string GetExpectedUpdateReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity)
		{
			return FormattableString.Invariant($"Update of sub Booking Instruction of type PIC sequence 1 on Booking Job ID {((BusinessObject)subEntity["Booking"])[DtbBookingSchema.KM_JobID]} for master Booking Instruction of type DLV sequence 2 on Booking Job ID {((BusinessObject)masterEntity["Booking"])[DtbBookingSchema.KM_JobID]} on Master Booking Version 2 failed with exception Test Replication Error");
		}

		protected override string GetExpectedInsertReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subDtbBooking)
		{
			return FormattableString.Invariant($"Insert of Booking Instruction of type PIC sequence 1 on sub Booking Job ID {subDtbBooking[DtbBookingSchema.KM_JobID]} for master Booking Job ID {((BusinessObject)masterEntity["Booking"])[DtbBookingSchema.KM_JobID]} on Master Booking Version 1 failed with exception Test Replication Error");
		}

		protected override string ExpectedFailedErrorNoteProcessingErrorReportMessage =>
			"Error while handling error replicating master Booking Instruction of type DLV sequence 2 on Booking Job ID M0001 on MasterBookingVersion 2";

		protected override void ReloadParentSubEntitiesFromCurrentFactory(BaseDtbMasterBookingInsertingReplicationSubscriber subscriber)
		{
			SubBooking1 = (IDtbBooking)subscriber.ReloadParentEntityFromCurrentFactory(SubDtbBooking1);
			SubBooking2 = (IDtbBooking)subscriber.ReloadParentEntityFromCurrentFactory(SubDtbBooking2);
		}

		RefEquipment Equipment1 { get; set; }
		BusinessObject Zone1 { get; set; }

		protected override BusinessObject MasterDtbBooking => (BusinessObject)MasterBooking;
		protected override BusinessObject SubDtbBooking1 => (BusinessObject)SubBooking1;
		protected override BusinessObject SubDtbBooking2 => (BusinessObject)SubBooking2;

		protected override BusinessObject[] SubParents => new BusinessObject[] { SubDtbBooking1, SubDtbBooking2 };

		protected override SchemaColumn SystemLastEditTimeUtcColumn => DtbBookingInstructionSchema.KN_SystemLastEditTimeUtc;
		protected override SchemaColumn SystemLastEditUserColumn => DtbBookingInstructionSchema.KN_SystemLastEditUser;
	}
}
