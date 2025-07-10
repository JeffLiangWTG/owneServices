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
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.TransportBooking.Test
{
	[TestedType(typeof(DtbBookingConfirmationMasterBookingReplicationSubscriber))]
	class DtbBookingConfirmationMasterBookingReplicationSubscriberTest : BaseDtbMasterBookingInsertAndUpdateReplicationSubscriberBaseTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new DtbBookingConfirmationMasterBookingReplicationSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals("Should include master booking confirmations", 1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			row[DtbBookingConfirmationSchema.PK.Name] = Guid.NewGuid();
			if (shouldBeFiltered)
			{
				row[DtbBookingConfirmationSchema.KK_IsMaster.Name] = false;
			}
			else
			{
				row[DtbBookingConfirmationSchema.KK_IsMaster.Name] = true;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override void SetupMasterParent()
		{
			SetupMasterBookingAndInstruction();
		}

		protected override void ClearPreparatoryData()
		{
			MasterBookingInstruction = SubBookingInstruction1 = SubBookingInstruction2 = null;
			MasterBooking = SubBooking1 = SubBooking2 = null;
			Driver1 = Driver2 = null;
			Divot = null;
		}

		protected override void SetupMiscellaneousPreparatoryData()
		{
			var org = SubscriberFactory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ANOTHERORG";
			Driver1 = SubscriberFactory.NewWithValidTestData<OrgContact>();
			Driver1.OC_OH = org.PK;
			Driver1.OC_ContactName = "Driver One";
			Driver2 = SubscriberFactory.NewWithValidTestData<OrgContact>();
			Driver2.OC_OH = org.PK;
			Driver2.OC_ContactName = "Driver Two";
		}

		void SetupMasterBookingAndInstruction()
		{
			MasterBooking = Helper.CreateBooking();
			MasterBooking.KM_IsMaster = true;
			MasterBooking.KM_JobID = "M0001";

			MasterBookingInstruction = Helper.CreateInstruction(MasterBooking, "PIC");
			MasterBookingInstruction.KN_Sequence = 1;
			MasterBookingInstruction.OrganisationType = "CNE";
		}

		protected override void SetupSubParents()
		{
			SetupSubBookingsAndInstructions();
		}

		void SetupSubBookingsAndInstructions()
		{
			SubBooking1 = Helper.CreateBooking();
			SubBooking1.KM_JobID = "S0001";
			SubBooking2 = Helper.CreateBooking();
			SubBooking2.KM_JobID = "S0002";
			SubBooking1.KM_KM_MasterBooking = SubBooking2.KM_KM_MasterBooking = MasterBooking.PK;
			SubBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = SubBooking2.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = MasterBooking.ConsolidationSingleJob.PK;

			SubBookingInstruction1 = Helper.CreateInstruction(SubBooking1, "PIC");
			SubBookingInstruction2 = Helper.CreateInstruction(SubBooking2, "PIC");
			SubBookingInstruction1.KN_Sequence = SubBookingInstruction2.KN_Sequence = 1;
			SubBookingInstruction1.KN_KN_MasterBookingInstruction = SubBookingInstruction2.KN_KN_MasterBookingInstruction = MasterBookingInstruction.PK;

			var package = ((PkgPackageJob)SubBooking1.PackageJob).Packages.AddNew("CNT", 2);
			package.Container.K0_RC_ContainerType = SubscriberFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Divot = MasterBookingInstruction.PackageDivots.AddNew();
			Divot.KD_KP_Package = package.PK;
			Divot.KD_Quantity = package.KP_PackageQty;
		}

		protected override void SetupMasterAndSubEntities()
		{
			MasterEntity = (BusinessObject)Helper.CreateConfirmation(MasterBookingInstruction, "PIC");

			SubEntity1 = (BusinessObject)Helper.CreateConfirmation(SubBookingInstruction1, "PIC");
			SubEntity2 = (BusinessObject)Helper.CreateConfirmation(SubBookingInstruction2, "PIC");
			SubEntity1[DtbBookingConfirmationSchema.KK_KK_MasterBookingConfirmation] = SubEntity2[DtbBookingConfirmationSchema.KK_KK_MasterBookingConfirmation] = MasterEntity.PK;

			var now = DateTime.Now;
			UpdateEntities(
				new BusinessObject[] { MasterEntity, SubEntity1, SubEntity2 },
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Estimated, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Actual, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_RequiredFrom, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_RequiredTo, now.AddDays(7)),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReferenceNum, "Initial Ref"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReceivedBy, "ME"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReceivedBySignature, ZBlob.FromAscii("I was here")),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SlotDateTime, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SlotReference, "Initial SlotRef"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_OC_Driver, Driver1.PK),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_VehicleRegistration, "REGO1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentID, "LICENSE1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentIssuer, "ISSUER1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentType, "DOCTYPE1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Quantity, 0),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SystemLastEditUser, "ME"));

			UpdateEntities(
				new BusinessObject[] { SubEntity1, SubEntity2 },
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot, Divot.PK));
		}

		protected override void UpdateMasterEntity()
		{
			((IDtbBookingConfirmation)MasterEntity).Instruction.OrganisationType = "CYD";
			var dateInSevenDays = ZDateTime.Now.AddDays(7);
			UpdateEntities(
				new BusinessObject[] { MasterEntity },
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ConfirmationType, "DLV"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Estimated, dateInSevenDays),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Actual, dateInSevenDays),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_RequiredFrom, dateInSevenDays),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_RequiredTo, dateInSevenDays.AddDays(7)),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReferenceNum, "Changed Ref"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReceivedBy, "YOU"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReceivedBySignature, ZBlob.FromAscii("You were there")),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SlotDateTime, dateInSevenDays),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SlotReference, "Changed SlotRef"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_OC_Driver, Driver2.PK),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_VehicleRegistration, "REGO2"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentID, "LICENSE2"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentIssuer, "ISSUER2"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentType, "DOCTYPE2"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Quantity, 1),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_IsEmptyContainer, true),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot, ZGuid.Empty),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SystemLastEditUser, "ME"));
		}

		protected override void SetupMasterEntityForInsertTest()
		{
			MasterEntity = (BusinessObject)Helper.CreateConfirmation(MasterBookingInstruction, "PIC");

			var now = DateTime.Now;
			UpdateEntities(
				new BusinessObject[] { MasterEntity },
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Estimated, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Actual, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_RequiredFrom, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_RequiredTo, now.AddDays(7)),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReferenceNum, "Initial Ref"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReceivedBy, "ME"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_ReceivedBySignature, ZBlob.FromAscii("I was here")),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SlotDateTime, now),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SlotReference, "Initial SlotRef"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_OC_Driver, Driver1.PK),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_VehicleRegistration, "REGO1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentID, "LICENSE1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentIssuer, "ISSUER1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_DocumentType, "DOCTYPE1"),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_Quantity, 1),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc, ZDateTime.UtcNow),
				new Tuple<SchemaColumn, object>(DtbBookingConfirmationSchema.KK_SystemLastEditUser, "ME"));
		}

		protected override BaseDtbMasterBookingReplicationSubscriber NewDataChangeSubscriberUsingFactory(bool throwTestErrorOnSaveToFactory = false, bool onlyThrowTestErrorOnSaveToFactoryOnce = false)
		{
			var subscriber = new DtbBookingConfirmationMasterBookingReplicationSubscriber(SubscriberFactory);
			subscriber.ThrowTestErrorOnSaveToFactory = throwTestErrorOnSaveToFactory;
			subscriber.OnlyThrowTestErrorOnSaveToFactoryOnce = onlyThrowTestErrorOnSaveToFactoryOnce;
			return subscriber;
		}

		protected override IEnumerable<SchemaColumn> DataTableColumns => new SchemaColumn[]
		{
			DtbBookingConfirmationSchema.PK,
			DtbBookingConfirmationSchema.KK_IsMaster,
			DtbBookingConfirmationSchema.KK_MasterBookingVersion,
			DtbBookingConfirmationSchema.KK_KN_BookingInstruction,
			DtbBookingConfirmationSchema.KK_ConfirmationType,
			DtbBookingConfirmationSchema.KK_Estimated,
			DtbBookingConfirmationSchema.KK_EstimatedUtc,
			DtbBookingConfirmationSchema.KK_Actual,
			DtbBookingConfirmationSchema.KK_RequiredFrom,
			DtbBookingConfirmationSchema.KK_RequiredFromUtc,
			DtbBookingConfirmationSchema.KK_RequiredTo,
			DtbBookingConfirmationSchema.KK_RequiredToUtc,
			DtbBookingConfirmationSchema.KK_ReferenceNum,
			DtbBookingConfirmationSchema.KK_ReceivedBy,
			DtbBookingConfirmationSchema.KK_ReceivedBySignature,
			DtbBookingConfirmationSchema.KK_SlotDateTime,
			DtbBookingConfirmationSchema.KK_SlotReference,
			DtbBookingConfirmationSchema.KK_OC_Driver,
			DtbBookingConfirmationSchema.KK_VehicleRegistration,
			DtbBookingConfirmationSchema.KK_DocumentID,
			DtbBookingConfirmationSchema.KK_DocumentIssuer,
			DtbBookingConfirmationSchema.KK_DocumentType,
			DtbBookingConfirmationSchema.KK_Quantity,
			DtbBookingConfirmationSchema.KK_IsEmptyContainer,
			DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot,
			DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc,
		};

		protected override IEnumerable<SchemaColumn> NonReplicationColumnsToExcludeFromPartiallyFailedUpdateMismatchCheck => new SchemaColumn[]
		{
			DtbBookingConfirmationSchema.KK_IsEmptyContainer, // because second confirmation being updated true means first confirmation is also updated with true
			DtbBookingConfirmationSchema.KK_Quantity, // because quantity updates from divot which is now shared between master and subs
		};

		protected override IEnumerable<SchemaColumn> NonReplicationColumnsToExcludeFromUpdateMismatchCheck => new SchemaColumn[]
		{
			DtbBookingConfirmationSchema.KK_Quantity, // because quantity updates from divot which is now shared between master and subs
		};

		protected override IEnumerable<SchemaColumn> ColumnsToCheckForNonReplication => new SchemaColumn[]
		{
			DtbBookingConfirmationSchema.KK_Quantity,
			DtbBookingConfirmationSchema.KK_IsEmptyContainer,
			DtbBookingConfirmationSchema.KK_KD_BookingInstructionPkgDivot,
		};

		protected override ITableSchema ExpectedSubscriberTable => DtbBookingConfirmationSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSubscriberSpecificColumns => new SchemaColumn[]
		{
			DtbBookingConfirmationSchema.PK,
			DtbBookingConfirmationSchema.KK_IsMaster,
			DtbBookingConfirmationSchema.KK_MasterBookingVersion,
			DtbBookingConfirmationSchema.KK_KN_BookingInstruction,
			DtbBookingConfirmationSchema.KK_ConfirmationType,
			DtbBookingConfirmationSchema.KK_Estimated,
			DtbBookingConfirmationSchema.KK_EstimatedUtc,
			DtbBookingConfirmationSchema.KK_Actual,
			DtbBookingConfirmationSchema.KK_RequiredFrom,
			DtbBookingConfirmationSchema.KK_RequiredFromUtc,
			DtbBookingConfirmationSchema.KK_RequiredTo,
			DtbBookingConfirmationSchema.KK_RequiredToUtc,
			DtbBookingConfirmationSchema.KK_ReferenceNum,
			DtbBookingConfirmationSchema.KK_ReceivedBy,
			DtbBookingConfirmationSchema.KK_ReceivedBySignature,
			DtbBookingConfirmationSchema.KK_SlotDateTime,
			DtbBookingConfirmationSchema.KK_SlotReference,
			DtbBookingConfirmationSchema.KK_OC_Driver,
			DtbBookingConfirmationSchema.KK_VehicleRegistration,
			DtbBookingConfirmationSchema.KK_DocumentID,
			DtbBookingConfirmationSchema.KK_DocumentIssuer,
			DtbBookingConfirmationSchema.KK_DocumentType,
			DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc,
		};

		protected override string ExpectedSubscriberCode => "MBK";

		protected override string ExpectedDescription => "DtbBookingConfirmation Master Booking Replication Change Subscriber";

		protected override bool ExpectedIsBaseTableDescendentOfDtbBooking => true;

		protected override BetterLogForTest[] ExpectedUpdateLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Updated sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 2 subs successfully, 0 failed for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedInsertLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Inserted sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
			new BetterLogForTest(LogType.Information, "Inserted sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
			new BetterLogForTest(LogType.Information, "Inserted 2 subs successfully, 0 failed for master Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
		};

		protected override BetterLogForTest[] ExpectedUpdateFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Update of sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Error, "Update of sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Updated 0 subs successfully, 2 failed for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedInsertFailedLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Insert of sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Error, "Insert of sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Inserted 0 subs successfully, 2 failed for master Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 1"),
		};

		protected override BetterLogForTest[] ExpectedFailureThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Error, "Update of sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0001 for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2 failed with exception Test Replication Error"),
			new BetterLogForTest(LogType.Information, "Updated sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 1 subs successfully, 1 failed for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override BetterLogForTest[] ExpectedFailureWithFailedErrorNoteProcessingThenSuccessLogs => new BetterLogForTest[]
		{
			new BetterLogForTest(LogType.Information, "Updated sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID S0002 for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
			new BetterLogForTest(LogType.Information, "Updated 1 subs successfully, 1 failed for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2"),
		};

		protected override void AssertCorrectFailedInsertErrorNotesWereCreated()
		{
			AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking1, GetExpectedInsertReplicationErrorNoteText(MasterEntity, SubDtbBooking1));
			AssertHasCorrectMasterBookingReplicationErrorNote(SubDtbBooking2, GetExpectedInsertReplicationErrorNoteText(MasterEntity, SubDtbBooking2));
		}

		protected override string GetExpectedUpdateReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subEntity)
		{
			return FormattableString.Invariant($"Update of sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID {((BusinessObject)subEntity["Booking"])[DtbBookingSchema.KM_JobID]} for master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID {((BusinessObject)masterEntity["Booking"])[DtbBookingSchema.KM_JobID]} on Master Booking Version 2 failed with exception Test Replication Error");
		}

		protected override string GetExpectedInsertReplicationErrorNoteText(BusinessObject masterEntity, BusinessObject subParent)
		{
			return FormattableString.Invariant($"Insert of sub Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID {subParent[DtbBookingSchema.KM_JobID]} for master Booking Confirmation of type PIC on Instruction of type PIC sequence 1 on Booking Job ID {((BusinessObject)masterEntity["Booking"])[DtbBookingSchema.KM_JobID]} on Master Booking Version 1 failed with exception Test Replication Error");
		}

		protected override string ExpectedFailedErrorNoteProcessingErrorReportMessage =>
			"Error while handling error replicating master Booking Confirmation of type DLV on Instruction of type PIC sequence 1 on Booking Job ID M0001 on MasterBookingVersion 2";

		protected override void ReloadParentSubEntitiesFromCurrentFactory(BaseDtbMasterBookingInsertingReplicationSubscriber subscriber)
		{
			SubBookingInstruction1 = (IDtbBookingInstruction)subscriber.ReloadParentEntityFromCurrentFactory((BusinessObject)SubBookingInstruction1);
			SubBookingInstruction2 = (IDtbBookingInstruction)subscriber.ReloadParentEntityFromCurrentFactory((BusinessObject)SubBookingInstruction2);
		}

		protected override void AssertPreconditionsOnMasterAndSubEntities()
		{
			{
				CombineAssertions("Precondition: Should have initialised MasterBookingVersion correctly on master and subs", () =>
				{
					AssertEquals("Precondition: master booking confirmation should be on MasterBookingVersion 1", (short)1, MasterEntity[DtbBookingConfirmationSchema.KK_MasterBookingVersion]);
					AssertEquals("Precondition: sub booking 1 confirmation should be on MasterBookingVersion 1", (short)1, SubEntity1[DtbBookingConfirmationSchema.KK_MasterBookingVersion]);
					AssertEquals("Precondition: sub booking 2 confirmation should be on MasterBookingVersion 1", (short)1, SubEntity2[DtbBookingConfirmationSchema.KK_MasterBookingVersion]);
				});
			}
		}

		protected override void AssertPreconditionsAfterUpdateOnMasterEntity()
		{
			AssertEquals("Precondition: master booking confirmation should be on MasterBookingVersion 2", (short)2, MasterEntity[DtbBookingConfirmationSchema.KK_MasterBookingVersion]);
		}

		protected override void AssertPreconditionsOnMasterAndSubEntitiesForInsertTest()
		{
			CombineAssertions("Precondition: Set up successfully for CDC Insert or CDC Update testing insert on missing sub confirmations", () =>
			{
				AssertEquals("Precondition: master booking confirmation should be on MasterBookingVersion 1", (short)1, MasterEntity[DtbBookingConfirmationSchema.KK_MasterBookingVersion]);
				AssertEquals("Precondition: sub booking 1 instruction is linked to master booking instruction", MasterBookingInstruction.PK, SubBookingInstruction1.KN_KN_MasterBookingInstruction);
				AssertEquals("Precondition: sub booking 2 instruction linked to master booking instruction", MasterBookingInstruction.PK, SubBookingInstruction2.KN_KN_MasterBookingInstruction);
				Assert("Precondition: sub booking 1 instruction does not have any sub confirmations linked to master confirmation", !SubBookingInstruction1.Confirmations.Any(c => c.KK_KK_MasterBookingConfirmation == MasterEntity.PK));
				Assert("Precondition: sub booking 2 instruction does not have any sub confirmations linked to master confirmation", !SubBookingInstruction2.Confirmations.Any(c => c.KK_KK_MasterBookingConfirmation == MasterEntity.PK));
			});
		}

		protected override void AssertSubsInsertedFromMaster(DataRow insertRow, params BusinessObject[] subParents)
		{
			AssertSubsInsertedFromMaster(insertRow, subInstructions: subParents.Cast<IDtbBookingInstruction>().ToArray());
		}

		void AssertSubsInsertedFromMaster(DataRow insertRow, params IDtbBookingInstruction[] subInstructions)
		{
			CombineAssertions("Check that master booking confirmation inserted on subs and relevant fields are updated on subs", () =>
			{
				for (var subNo = 0; subNo < subInstructions.Length; subNo++)
				{
					var subInstruction = subInstructions[subNo];
					var subConfirmation = (BusinessObject)AssertSubConfirmationInsertedForMaster(subInstruction, subNo);

					if (subInstruction != null)
					{
						foreach (var replicatingColumn in ColumnsToCheckForReplication)
						{
							AssertSubHasMatchingField(insertRow, "insert", replicatingColumn, subConfirmation, subNo);
						}
					}
				}
			});
		}

		protected override void AssertSubsNotInsertedFromMaster(DataRow insertRow, params BusinessObject[] subParents)
		{
			AssertSubsNotInsertedFromMaster(insertRow, subParents.Cast<IDtbBookingInstruction>().ToArray());
		}

		void AssertSubsNotInsertedFromMaster(DataRow insertRow, params IDtbBookingInstruction[] subInstructions)
		{
			CombineAssertions("Check that master booking confirmation inserted on subs and relevant fields are updated on subs", () =>
			{
				for (var subNo = 0; subNo < subInstructions.Length; subNo++)
				{
					var subInstruction = subInstructions[subNo];
					var subConfirmation = (BusinessObject)AssertSubConfirmationNotInsertedForMaster(subInstruction, subNo);
				}
			});
		}

		IDtbBookingConfirmation AssertSubConfirmationInsertedForMaster(IDtbBookingInstruction subInstruction, int subNo)
		{
			var subConfirmation = subInstruction.Confirmations.SingleOrDefault(c => c.KK_KK_MasterBookingConfirmation == MasterEntity.PK);
			AssertNotNull("Sub Booking Instruction #" + subNo + " should now have sub confirmation", subConfirmation);

			return subConfirmation;
		}

		IDtbBookingConfirmation AssertSubConfirmationNotInsertedForMaster(IDtbBookingInstruction subInstruction, int subNo)
		{
			var subConfirmation = subInstruction.Confirmations.SingleOrDefault(c => c.KK_KK_MasterBookingConfirmation == MasterEntity.PK);
			AssertNull("Sub Booking Instruction #" + subNo + " should not have sub confirmation", subConfirmation);

			return subConfirmation;
		}

		OrgContact Driver1 { get; set; }
		OrgContact Driver2 { get; set; }
		IDtbBookingInstructionPkgDivot Divot { get; set; }

		protected override BusinessObject MasterDtbBooking => (BusinessObject)MasterBooking;
		protected override BusinessObject SubDtbBooking1 => (BusinessObject)SubBooking1;
		protected override BusinessObject SubDtbBooking2 => (BusinessObject)SubBooking2;

		IDtbBookingInstruction MasterBookingInstruction { get; set; }
		IDtbBookingInstruction SubBookingInstruction1 { get; set; }
		IDtbBookingInstruction SubBookingInstruction2 { get; set; }

		protected override BusinessObject[] SubParents => new BusinessObject[] { (BusinessObject)SubBookingInstruction1, (BusinessObject)SubBookingInstruction2 };

		protected override SchemaColumn SystemLastEditTimeUtcColumn => DtbBookingConfirmationSchema.KK_SystemLastEditTimeUtc;
		protected override SchemaColumn SystemLastEditUserColumn => DtbBookingConfirmationSchema.KK_SystemLastEditUser;
	}
}
