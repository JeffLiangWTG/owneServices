using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings.Testing
{
	[TestedType(typeof(RemoveIncorrectlyCreatedNonSubConfirmationsFromSubInstructions))]
	public class RemoveIncorrectlyCreatedNonSubConfirmationsFromSubInstructionsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveIncorrectlyCreatedNonSubConfirmationsFromSubInstructions();
		}

		protected override void PrepareTestData()
		{
			ClearAllPKLists();

			CreateMasterAndTwoSubBookingsWithInstructionsAndConfirmations(jobIDSuffix: "1", confirmationTypesToCreateIncorrectly: ConfirmationTypes.All);
			CreateMasterAndTwoSubBookingsWithInstructionsAndConfirmations(jobIDSuffix: "2", confirmationTypesToCreateIncorrectly: ConfirmationTypes.Pic);
			CreateMasterAndTwoSubBookingsWithInstructionsAndConfirmations(jobIDSuffix: "3", confirmationTypesToCreateIncorrectly: ConfirmationTypes.Dlv);
			CreateMasterAndTwoSubBookingsWithInstructionsAndConfirmations(jobIDSuffix: "4", confirmationTypesToCreateIncorrectly: ConfirmationTypes.None);
			CreateNonMasterBookingWithInstructionsAndConfirmations(jobIDSuffix: "5");
			CreateNonMasterBookingWithInstructionsAndConfirmations(jobIDSuffix: "6");
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions("Check that correct records were deleted and others were not touched", () =>
			{
				AssertAllRecordsExistence(DtbBookingConsolidationSchema.Constants.TableName, DtbBookingConsolidationSchema.Constants.Prefix, "Booking Consolidations (master/sub/neither)", masterBookingConsolidationPKs.Concat(subBookingConsolidationPKs).Concat(nonMasterBookingConsolidationPKs), expectedToExist: true);
				AssertAllRecordsExistence(DtbBookingSchema.Constants.TableName, DtbBookingSchema.Constants.Prefix, "Bookings (master/sub/neither)", masterBookingPKs.Concat(subBookingPKs).Concat(nonMasterBookingPKs), expectedToExist: true);
				AssertAllRecordsExistence(DtbBookingInstructionSchema.Constants.TableName, DtbBookingInstructionSchema.Constants.Prefix, "Booking Instructions (master/sub/neither)", masterBookingInstructionPKs.Concat(subBookingInstructionPKs).Concat(nonMasterBookingInstructionPKs), expectedToExist: true);
				AssertAllRecordsExistence(DtbBookingConfirmationSchema.Constants.TableName, DtbBookingConfirmationSchema.Constants.Prefix, "Master and Sub Booking Confirmations and non-Sub Booking Confirmations under non-Sub Booking Instructions)", masterBookingConfirmationPKs.Concat(subBookingConfirmationPKs).Concat(nonMasterBookingConfirmationPKs), expectedToExist: true);
				AssertAllRecordsExistence(DtbBookingConfirmationSchema.Constants.TableName, DtbBookingConfirmationSchema.Constants.Prefix, "Non-Sub Booking Confirmations under Sub Booking Instructions", nonSubBookingConfirmationPKsUnderSubBookingInstructions, expectedToExist: false);
			});
		}

		void CreateMasterAndTwoSubBookingsWithInstructionsAndConfirmations(string jobIDSuffix, ConfirmationTypes confirmationTypesToCreateIncorrectly)
		{
			var masterBookingConsolidationPK = CreateMasterBookingConsolidation("BKG", "PIC", FormattableString.Invariant($"MC{jobIDSuffix}"));
			var masterBookingPK = CreateMasterBooking(masterBookingConsolidationPK, FormattableString.Invariant($"MTB{jobIDSuffix}"), "ORG");
			var masterInstructionPicPK = CreateMasterInstruction(masterBookingPK, 1, "PIC");
			var masterInstructionDlvPK = CreateMasterInstruction(masterBookingPK, 2, "DLV");
			var masterConfirmationPicPK = CreateMasterConfirmation(masterInstructionPicPK);
			var masterConfirmationDlvPK = CreateMasterConfirmation(masterInstructionDlvPK);

			var subBookingConsolidationAPK = CreateSubBookingConsolidation(masterBookingConsolidationPK, "BKG", "PIC", FormattableString.Invariant($"SC{jobIDSuffix}A"));
			var subBookingAPK = CreateSubBooking(masterBookingPK, subBookingConsolidationAPK, FormattableString.Invariant($"STB{jobIDSuffix}A"), "ORG");
			var subInstructionAPicPK = CreateSubInstruction(masterInstructionPicPK, subBookingAPK, 1, "PIC");
			var subInstructionADlvPK = CreateSubInstruction(masterInstructionDlvPK, subBookingAPK, 2, "DLV");
			CreateSubConfirmation(masterConfirmationPicPK, subInstructionAPicPK);
			CreateSubConfirmation(masterConfirmationDlvPK, subInstructionADlvPK);

			var subBookingConsolidationBPK = CreateSubBookingConsolidation(masterBookingConsolidationPK, "BKG", "PIC", FormattableString.Invariant($"SC{jobIDSuffix}B"));
			var subBookingBPK = CreateSubBooking(masterBookingPK, subBookingConsolidationBPK, FormattableString.Invariant($"STB{jobIDSuffix}B"), "ORG");
			var subInstructionBPicPK = CreateSubInstruction(masterInstructionPicPK, subBookingBPK, 1, "PIC");
			var subInstructionBDlvPK = CreateSubInstruction(masterInstructionDlvPK, subBookingBPK, 2, "DLV");
			CreateSubConfirmation(masterConfirmationPicPK, subInstructionBPicPK);
			CreateSubConfirmation(masterConfirmationDlvPK, subInstructionBDlvPK);

			if (new[] { ConfirmationTypes.All, ConfirmationTypes.Pic }.Contains(confirmationTypesToCreateIncorrectly))
			{
				CreateNonSubConfirmationFromSubInstruction(subInstructionAPicPK);
				CreateNonSubConfirmationFromSubInstruction(subInstructionBPicPK);
			}

			if (new[] { ConfirmationTypes.All, ConfirmationTypes.Dlv }.Contains(confirmationTypesToCreateIncorrectly))
			{
				CreateNonSubConfirmationFromSubInstruction(subInstructionADlvPK);
				CreateNonSubConfirmationFromSubInstruction(subInstructionBDlvPK);
			}
		}

		void CreateNonMasterBookingWithInstructionsAndConfirmations(string jobIDSuffix)
		{
			var nonMasterBookingConsolidationPK = CreateNonMasterBookingConsolidation("BKG", "PIC", FormattableString.Invariant($"C{jobIDSuffix}"));
			var nonMasterBookingPK = CreateNonMasterBooking(nonMasterBookingConsolidationPK, FormattableString.Invariant($"TB{jobIDSuffix}"), "ORG");
			var nonMasterInstructionPicPK = CreateNonMasterInstruction(nonMasterBookingPK, 1, "PIC");
			var nonMasterInstructionDlvPK = CreateNonMasterInstruction(nonMasterBookingPK, 2, "DLV");
			CreateNonMasterConfirmation(nonMasterInstructionPicPK);
			CreateNonMasterConfirmation(nonMasterInstructionDlvPK);
		}

		Guid CreateMasterBookingConsolidation(string jobType, string jobDirection, string jobID)
		{
			var bookingConsolidationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConsolidation (KB_PK, KB_JobType, KB_JobDirection, KB_JobID, KB_IsMaster, KB_MasterBookingVersion, KB_SystemCreateTimeUtc, KB_SystemCreateUser, KB_SystemLastEditTimeUtc, KB_SystemLastEditUser) VALUES
					('{bookingConsolidationPK}', '{jobType}', '{jobDirection}', '{jobID}', 1, 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			masterBookingConsolidationPKs.Add(bookingConsolidationPK);

			return bookingConsolidationPK;
		}

		Guid CreateSubBookingConsolidation(Guid masterBookingConsolidationPK, string jobType, string jobDirection, string jobID)
		{
			var bookingConsolidationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConsolidation (KB_PK, KB_JobType, KB_JobDirection, KB_JobID, KB_KB_MasterBookingConsolidation, KB_MasterBookingVersion, KB_SystemCreateTimeUtc, KB_SystemCreateUser, KB_SystemLastEditTimeUtc, KB_SystemLastEditUser) VALUES
					('{bookingConsolidationPK}', '{jobType}', '{jobDirection}', '{jobID}', '{masterBookingConsolidationPK}', 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			subBookingConsolidationPKs.Add(bookingConsolidationPK);

			return bookingConsolidationPK;
		}

		Guid CreateNonMasterBookingConsolidation(string jobType, string jobDirection, string jobID)
		{
			var bookingConsolidationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConsolidation (KB_PK, KB_JobType, KB_JobDirection, KB_JobID, KB_SystemCreateTimeUtc, KB_SystemCreateUser, KB_SystemLastEditTimeUtc, KB_SystemLastEditUser) VALUES
					('{bookingConsolidationPK}', '{jobType}', '{jobDirection}', '{jobID}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			nonMasterBookingConsolidationPKs.Add(bookingConsolidationPK);

			return bookingConsolidationPK;
		}

		Guid CreateMasterBooking(Guid bookingConsolidationPK, string jobID, string direction)
		{
			var bookingPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBooking (KM_PK, KM_KB_Booking, KM_JobID, KM_Direction, KM_IsMaster, KM_MasterBookingVersion, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser) VALUES
						('{bookingPK}', '{bookingConsolidationPK}', '{jobID}', '{direction}', 1, 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			masterBookingPKs.Add(bookingPK);

			return bookingPK;
		}

		Guid CreateSubBooking(Guid masterBookingPK, Guid bookingConsolidationPK, string jobID, string direction)
		{
			var bookingPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBooking (KM_PK, KM_KB_Booking, KM_JobID, KM_Direction, KM_KM_MasterBooking, KM_MasterBookingVersion, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser) VALUES
						('{bookingPK}', '{bookingConsolidationPK}', '{jobID}', '{direction}', '{masterBookingPK}', 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			subBookingPKs.Add(bookingPK);

			return bookingPK;
		}

		Guid CreateNonMasterBooking(Guid bookingConsolidationPK, string jobID, string direction)
		{
			var bookingPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBooking (KM_PK, KM_KB_Booking, KM_JobID, KM_Direction, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser) VALUES
						('{bookingPK}', '{bookingConsolidationPK}', '{jobID}', '{direction}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			nonMasterBookingPKs.Add(bookingPK);

			return bookingPK;
		}

		Guid CreateMasterInstruction(Guid bookingPK, int sequence, string instructionType)
		{
			var instructionPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingInstruction (KN_PK, KN_KM_BookingMovement, KN_Sequence, KN_InstructionType, KN_IsMaster, KN_MasterBookingVersion, KN_SystemCreateTimeUtc, KN_SystemCreateUser, KN_SystemLastEditTimeUtc, KN_SystemLastEditUser) VALUES
						('{instructionPK}', '{bookingPK}', {sequence}, '{instructionType}', 1, 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			masterBookingInstructionPKs.Add(instructionPK);

			return instructionPK;
		}

		Guid CreateSubInstruction(Guid masterInstructionPK, Guid bookingPK, int sequence, string instructionType)
		{
			var instructionPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingInstruction (KN_PK, KN_KM_BookingMovement, KN_Sequence, KN_InstructionType, KN_KN_MasterBookingInstruction, KN_MasterBookingVersion, KN_SystemCreateTimeUtc, KN_SystemCreateUser, KN_SystemLastEditTimeUtc, KN_SystemLastEditUser) VALUES
						('{instructionPK}', '{bookingPK}', {sequence}, '{instructionType}', '{masterInstructionPK}', 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			subBookingInstructionPKs.Add(instructionPK);

			return instructionPK;
		}

		Guid CreateNonMasterInstruction(Guid bookingPK, int sequence, string instructionType)
		{
			var instructionPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingInstruction (KN_PK, KN_KM_BookingMovement, KN_Sequence, KN_InstructionType, KN_SystemCreateTimeUtc, KN_SystemCreateUser, KN_SystemLastEditTimeUtc, KN_SystemLastEditUser) VALUES
						('{instructionPK}', '{bookingPK}', {sequence}, '{instructionType}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			nonMasterBookingInstructionPKs.Add(instructionPK);

			return instructionPK;
		}

		Guid CreateMasterConfirmation(Guid instructionPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_IsMaster, KK_MasterBookingVersion, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', 1, 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			masterBookingConfirmationPKs.Add(confirmationPK);

			return confirmationPK;
		}

		void CreateSubConfirmation(Guid masterConfirmationPK, Guid instructionPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_KK_MasterBookingConfirmation, KK_MasterBookingVersion, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', '{masterConfirmationPK}', 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			subBookingConfirmationPKs.Add(confirmationPK);
		}

		void CreateNonSubConfirmationFromSubInstruction(Guid subInstructionPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{subInstructionPK}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			nonSubBookingConfirmationPKsUnderSubBookingInstructions.Add(confirmationPK);
		}

		void CreateNonMasterConfirmation(Guid instructionPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			nonMasterBookingConfirmationPKs.Add(confirmationPK);
		}

		void AssertAllRecordsExistence(string tableName, string tablePrefix, string recordsToCheckDescription, IEnumerable<Guid> pksToCheck, bool expectedToExist)
		{
			var pksToCheckString = string.Join(",", pksToCheck.Select(pk => FormattableString.Invariant($"'{pk}'")).ToArray());
			var query = FormattableString.Invariant($@"SELECT COUNT(*) AS cnt FROM dbo.{tableName} WHERE {tablePrefix}_PK IN ({pksToCheckString})");
			var count = TestConnection.ExecuteScalar(query);

			var assertMessage = expectedToExist ?
				FormattableString.Invariant($"No {recordsToCheckDescription} should have been deleted") :
				FormattableString.Invariant($"All {recordsToCheckDescription} should have been deleted");
			var expectedCount = expectedToExist ? pksToCheck.Count() : 0;
			AssertEquals(assertMessage, expectedCount, count);
		}

		void ClearAllPKLists()
		{
			masterBookingConsolidationPKs.Clear();
			masterBookingPKs.Clear();
			masterBookingInstructionPKs.Clear();
			masterBookingConfirmationPKs.Clear();

			subBookingConsolidationPKs.Clear();
			subBookingPKs.Clear();
			subBookingInstructionPKs.Clear();
			subBookingConfirmationPKs.Clear();

			nonSubBookingConfirmationPKsUnderSubBookingInstructions.Clear();

			nonMasterBookingConsolidationPKs.Clear();
			nonMasterBookingPKs.Clear();
			nonMasterBookingInstructionPKs.Clear();
			nonMasterBookingConfirmationPKs.Clear();
		}

		readonly List<Guid> masterBookingConsolidationPKs = new List<Guid>();
		readonly List<Guid> masterBookingPKs = new List<Guid>();
		readonly List<Guid> masterBookingInstructionPKs = new List<Guid>();
		readonly List<Guid> masterBookingConfirmationPKs = new List<Guid>();

		readonly List<Guid> subBookingConsolidationPKs = new List<Guid>();
		readonly List<Guid> subBookingPKs = new List<Guid>();
		readonly List<Guid> subBookingInstructionPKs = new List<Guid>();
		readonly List<Guid> subBookingConfirmationPKs = new List<Guid>();

		readonly List<Guid> nonSubBookingConfirmationPKsUnderSubBookingInstructions = new List<Guid>();

		readonly List<Guid> nonMasterBookingConsolidationPKs = new List<Guid>();
		readonly List<Guid> nonMasterBookingPKs = new List<Guid>();
		readonly List<Guid> nonMasterBookingInstructionPKs = new List<Guid>();
		readonly List<Guid> nonMasterBookingConfirmationPKs = new List<Guid>();

		enum ConfirmationTypes
		{
			All,
			Pic,
			Dlv,
			None
		}
	}
}
