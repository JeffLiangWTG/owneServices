using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings.Testing
{
	[TestedType(typeof(RemovePackageDivotsOnSubInstructions))]
	public class RemovePackageDivotsOnSubInstructionsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemovePackageDivotsOnSubInstructions();
		}

		protected override void PrepareTestData()
		{
			ClearAllPKLists();
			CreateMasterWithTwoSubsWithPackageDivotsAndLinkedConfirmations("1");
			CreateMasterWithTwoSubsWithPackageDivotsAndLinkedConfirmations("2");

			CreateMasterWithTwoSubsWithPackageDivotsButNoLinkedConfirmations("3");
			CreateMasterWithTwoSubsWithPackageDivotsButNoLinkedConfirmations("4");

			CreateNonMasterWithPackageDivotsAndSomeLinkedConfirmations("5");
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions("Check that correct records were deleted and others were not touched, and sub confirmation links to package divots were erased", () =>
			{
				AssertAllRecordsExistence(DtbBookingConsolidationSchema.Constants.TableName, DtbBookingConsolidationSchema.Constants.Prefix, "Booking Consolidations (master/sub/neither)", masterBookingConsolidationPKs.Concat(subBookingConsolidationPKs).Concat(nonMasterBookingConsolidationPKs), expectedToExist: true);
				AssertAllRecordsExistence(DtbBookingSchema.Constants.TableName, DtbBookingSchema.Constants.Prefix, "Bookings (master/sub/neither)", masterBookingPKs.Concat(subBookingPKs).Concat(nonMasterBookingPKs), expectedToExist: true);
				AssertAllRecordsExistence(DtbBookingInstructionSchema.Constants.TableName, DtbBookingInstructionSchema.Constants.Prefix, "Booking Instructions (master/sub/neither)", masterBookingInstructionPKs.Concat(subBookingInstructionPKs).Concat(nonMasterBookingInstructionPKs), expectedToExist: true);
				AssertAllRecordsExistence(
					DtbBookingConfirmationSchema.Constants.TableName,
					DtbBookingConfirmationSchema.Constants.Prefix,
					"Booking Confirmations (master/sub/neither)",
					masterBookingConfirmationPKsWithoutDivotLink
					.Concat(masterBookingConfirmationPKsWithDivotLink)
					.Concat(subBookingConfirmationPKsWithoutDivotLink)
					.Concat(subBookingConfirmationPKsWithDivotLink)
					.Concat(nonMasterBookingConfirmationPKsWithoutDivotLink)
					.Concat(nonMasterBookingConfirmationPKsWithDivotLink),
					expectedToExist: true);
				AssertAllRecordsExistence(
					DtbBookingInstructionPkgDivotSchema.Constants.TableName,
					DtbBookingInstructionPkgDivotSchema.Constants.Prefix,
					"Booking Instruction Package Divots (master/non-master)",
					masterBookingInstructionPkgDivotPKs
					.Concat(nonMasterBookingInstructionPkgDivotPKs),
					expectedToExist: true);
				AssertAllRecordsExistence(
					DtbBookingInstructionPkgDivotSchema.Constants.TableName,
					DtbBookingInstructionPkgDivotSchema.Constants.Prefix,
					"Sub Booking Instruction Package Divots",
					subBookingInstructionPkgDivotPKs,
					expectedToExist: false);

				AssertConfirmationDivotLinks(
					confirmationsDescription: "Master and non-Master Booking Confirmations with instruction package divot links",
					confirmationPKsToCheck: masterBookingConfirmationPKsWithDivotLink.Concat(nonMasterBookingConfirmationPKsWithDivotLink),
					expectedToBeBlank: false);
				AssertConfirmationDivotLinks(
					confirmationsDescription: "Sub Booking Confirmations previously with instruction package divot links",
					confirmationPKsToCheck: subBookingConfirmationPKsWithDivotLink,
					expectedToBeBlank: true);
			});
		}

		void CreateMasterWithTwoSubsWithPackageDivotsAndLinkedConfirmations(string jobIDSuffix)
		{
			var masterBookingConsolidationPK = CreateMasterBookingConsolidation("BKG", "PIC", FormattableString.Invariant($"MC{jobIDSuffix}"));
			var masterBookingPK = CreateMasterBooking(masterBookingConsolidationPK, FormattableString.Invariant($"MTB{jobIDSuffix}"), "ORG");
			var masterInstructionPicPK = CreateMasterInstruction(masterBookingPK, 1, "PIC");
			var masterInstructionDlvPK = CreateMasterInstruction(masterBookingPK, 2, "DLV");

			var subBookingConsolidationAPK = CreateSubBookingConsolidation(masterBookingConsolidationPK, "BKG", "PIC", FormattableString.Invariant($"SC{jobIDSuffix}A"));
			var subBookingAPK = CreateSubBooking(masterBookingPK, subBookingConsolidationAPK, FormattableString.Invariant($"STB{jobIDSuffix}A"), "ORG");
			var subInstructionAPicPK = CreateSubInstruction(masterInstructionPicPK, subBookingAPK, 1, "PIC");
			var subInstructionADlvPK = CreateSubInstruction(masterInstructionDlvPK, subBookingAPK, 2, "DLV");
			var subPackageJobAPK = CreatePackageJob(subBookingConsolidationAPK, FormattableString.Invariant($"SKJ{jobIDSuffix}A"));
			var subPackageA1PK = CreatePackage(subPackageJobAPK, 1);
			var subPackageA2PK = CreatePackage(subPackageJobAPK, 2);

			var subBookingConsolidationBPK = CreateSubBookingConsolidation(masterBookingConsolidationPK, "BKG", "PIC", FormattableString.Invariant($"SC{jobIDSuffix}B"));
			var subBookingBPK = CreateSubBooking(masterBookingPK, subBookingConsolidationBPK, FormattableString.Invariant($"STB{jobIDSuffix}B"), "ORG");
			var subInstructionBPicPK = CreateSubInstruction(masterInstructionPicPK, subBookingBPK, 1, "PIC");
			var subInstructionBDlvPK = CreateSubInstruction(masterInstructionDlvPK, subBookingBPK, 2, "DLV");
			var subPackageJobBPK = CreatePackageJob(subBookingConsolidationBPK, FormattableString.Invariant($"SKJ{jobIDSuffix}B"));
			var subPackageB1PK = CreatePackage(subPackageJobBPK, 1);
			var subPackageB2PK = CreatePackage(subPackageJobBPK, 2);

			var masterDivotPicA1 = CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageA1PK);
			var masterDivotPicA2 = CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageA2PK);
			var masterDivotPicB1 = CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageB1PK);
			var masterDivotPicB2 = CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageB2PK);
			var masterDivotDlvA1 = CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageA1PK);
			var masterDivotDlvA2 = CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageA2PK);
			var masterDivotDlvB1 = CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageB1PK);
			var masterDivotDlvB2 = CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageB2PK);

			var subDivotPicA1 = CreateDivot(MasterBookingType.Sub, subInstructionAPicPK, subPackageA1PK);
			var subDivotPicA2 = CreateDivot(MasterBookingType.Sub, subInstructionAPicPK, subPackageA2PK);
			var subDivotDlvA1 = CreateDivot(MasterBookingType.Sub, subInstructionADlvPK, subPackageA1PK);
			var subDivotDlvA2 = CreateDivot(MasterBookingType.Sub, subInstructionADlvPK, subPackageA2PK);
			var subDivotPicB1 = CreateDivot(MasterBookingType.Sub, subInstructionBPicPK, subPackageB1PK);
			var subDivotPicB2 = CreateDivot(MasterBookingType.Sub, subInstructionBPicPK, subPackageB2PK);
			var subDivotDlvB1 = CreateDivot(MasterBookingType.Sub, subInstructionBDlvPK, subPackageB1PK);
			var subDivotDlvB2 = CreateDivot(MasterBookingType.Sub, subInstructionBDlvPK, subPackageB2PK);

			var masterConfirmationPicA1PK = CreateMasterConfirmationWithDivotLink(masterInstructionPicPK, masterDivotPicA1);
			var masterConfirmationPicA2PK = CreateMasterConfirmationWithDivotLink(masterInstructionPicPK, masterDivotPicA2);
			var masterConfirmationPicB1PK = CreateMasterConfirmationWithDivotLink(masterInstructionPicPK, masterDivotPicB1);
			var masterConfirmationPicB2PK = CreateMasterConfirmationWithDivotLink(masterInstructionPicPK, masterDivotPicB2);
			var masterConfirmationDlvA1PK = CreateMasterConfirmationWithDivotLink(masterInstructionDlvPK, masterDivotDlvA1);
			var masterConfirmationDlvA2PK = CreateMasterConfirmationWithDivotLink(masterInstructionDlvPK, masterDivotDlvA2);
			var masterConfirmationDlvB1PK = CreateMasterConfirmationWithDivotLink(masterInstructionDlvPK, masterDivotDlvB1);
			var masterConfirmationDlvB2PK = CreateMasterConfirmationWithDivotLink(masterInstructionDlvPK, masterDivotDlvB2);

			CreateSubConfirmationWithDivotLink(masterConfirmationPicA1PK, subInstructionAPicPK, subDivotPicA1);
			CreateSubConfirmationWithDivotLink(masterConfirmationPicA2PK, subInstructionAPicPK, subDivotPicA2);
			CreateSubConfirmationWithDivotLink(masterConfirmationPicB1PK, subInstructionBPicPK, subDivotPicB1);
			CreateSubConfirmationWithDivotLink(masterConfirmationPicB2PK, subInstructionBPicPK, subDivotPicB2);
			CreateSubConfirmationWithDivotLink(masterConfirmationDlvA1PK, subInstructionADlvPK, subDivotDlvA1);
			CreateSubConfirmationWithDivotLink(masterConfirmationDlvA2PK, subInstructionADlvPK, subDivotDlvA2);
			CreateSubConfirmationWithDivotLink(masterConfirmationDlvB1PK, subInstructionBDlvPK, subDivotDlvB1);
			CreateSubConfirmationWithDivotLink(masterConfirmationDlvB2PK, subInstructionBDlvPK, subDivotDlvB2);
		}

		void CreateMasterWithTwoSubsWithPackageDivotsButNoLinkedConfirmations(string jobIDSuffix)
		{
			var masterBookingConsolidationPK = CreateMasterBookingConsolidation("BKG", "PIC", FormattableString.Invariant($"MC{jobIDSuffix}"));
			var masterBookingPK = CreateMasterBooking(masterBookingConsolidationPK, FormattableString.Invariant($"MTB{jobIDSuffix}"), "ORG");
			var masterInstructionPicPK = CreateMasterInstruction(masterBookingPK, 1, "PIC");
			var masterInstructionDlvPK = CreateMasterInstruction(masterBookingPK, 2, "DLV");

			var subBookingConsolidationAPK = CreateSubBookingConsolidation(masterBookingConsolidationPK, "BKG", "PIC", FormattableString.Invariant($"SC{jobIDSuffix}A"));
			var subBookingAPK = CreateSubBooking(masterBookingPK, subBookingConsolidationAPK, FormattableString.Invariant($"STB{jobIDSuffix}A"), "ORG");
			var subInstructionAPicPK = CreateSubInstruction(masterInstructionPicPK, subBookingAPK, 1, "PIC");
			var subInstructionADlvPK = CreateSubInstruction(masterInstructionDlvPK, subBookingAPK, 2, "DLV");
			var subPackageJobAPK = CreatePackageJob(subBookingConsolidationAPK, FormattableString.Invariant($"SKJ{jobIDSuffix}A"));
			var subPackageA1PK = CreatePackage(subPackageJobAPK, 1);
			var subPackageA2PK = CreatePackage(subPackageJobAPK, 2);

			var subBookingConsolidationBPK = CreateSubBookingConsolidation(masterBookingConsolidationPK, "BKG", "PIC", FormattableString.Invariant($"SC{jobIDSuffix}B"));
			var subBookingBPK = CreateSubBooking(masterBookingPK, subBookingConsolidationBPK, FormattableString.Invariant($"STB{jobIDSuffix}B"), "ORG");
			var subInstructionBPicPK = CreateSubInstruction(masterInstructionPicPK, subBookingBPK, 1, "PIC");
			var subInstructionBDlvPK = CreateSubInstruction(masterInstructionDlvPK, subBookingBPK, 2, "DLV");
			var subPackageJobBPK = CreatePackageJob(subBookingConsolidationBPK, FormattableString.Invariant($"SKJ{jobIDSuffix}B"));
			var subPackageB1PK = CreatePackage(subPackageJobBPK, 1);
			var subPackageB2PK = CreatePackage(subPackageJobBPK, 2);

			CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageA1PK);
			CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageA2PK);
			CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageB1PK);
			CreateDivot(MasterBookingType.Master, masterInstructionPicPK, subPackageB2PK);
			CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageA1PK);
			CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageA2PK);
			CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageB1PK);
			CreateDivot(MasterBookingType.Master, masterInstructionDlvPK, subPackageB2PK);

			CreateDivot(MasterBookingType.Sub, subInstructionAPicPK, subPackageA1PK);
			CreateDivot(MasterBookingType.Sub, subInstructionAPicPK, subPackageA2PK);
			CreateDivot(MasterBookingType.Sub, subInstructionADlvPK, subPackageA1PK);
			CreateDivot(MasterBookingType.Sub, subInstructionADlvPK, subPackageA2PK);
			CreateDivot(MasterBookingType.Sub, subInstructionBPicPK, subPackageB1PK);
			CreateDivot(MasterBookingType.Sub, subInstructionBPicPK, subPackageB2PK);
			CreateDivot(MasterBookingType.Sub, subInstructionBDlvPK, subPackageB1PK);
			CreateDivot(MasterBookingType.Sub, subInstructionBDlvPK, subPackageB2PK);

			var masterConfirmationPicPK = CreateMasterConfirmationWithoutDivotLink(masterInstructionPicPK);
			var masterConfirmationDlvPK = CreateMasterConfirmationWithoutDivotLink(masterInstructionDlvPK);

			CreateSubConfirmationWithoutDivotLink(masterConfirmationPicPK, subInstructionAPicPK);
			CreateSubConfirmationWithoutDivotLink(masterConfirmationPicPK, subInstructionBPicPK);
			CreateSubConfirmationWithoutDivotLink(masterConfirmationDlvPK, subInstructionAPicPK);
			CreateSubConfirmationWithoutDivotLink(masterConfirmationDlvPK, subInstructionBPicPK);
		}

		void CreateNonMasterWithPackageDivotsAndSomeLinkedConfirmations(string jobIDSuffix)
		{
			var nonMasterBookingConsolidationPK = CreateNonMasterBookingConsolidation("BKG", "PIC", FormattableString.Invariant($"C{jobIDSuffix}"));
			var nonMasterBookingPK = CreateNonMasterBooking(nonMasterBookingConsolidationPK, FormattableString.Invariant($"TB{jobIDSuffix}"), "ORG");
			var nonMasterInstructionPicPK = CreateNonMasterInstruction(nonMasterBookingPK, 1, "PIC");
			var nonMasterInstructionDlvPK = CreateNonMasterInstruction(nonMasterBookingPK, 2, "DLV");

			var packageJobPK = CreatePackageJob(nonMasterBookingConsolidationPK, FormattableString.Invariant($"SKJ{jobIDSuffix}B"));
			var package1PK = CreatePackage(packageJobPK, 1);
			var package2PK = CreatePackage(packageJobPK, 2);

			var divotPic1PK = CreateDivot(MasterBookingType.NonMaster, nonMasterInstructionPicPK, package1PK);
			var divotPic2PK = CreateDivot(MasterBookingType.NonMaster, nonMasterInstructionPicPK, package2PK);
			CreateDivot(MasterBookingType.NonMaster, nonMasterInstructionDlvPK, package1PK);
			CreateDivot(MasterBookingType.NonMaster, nonMasterInstructionDlvPK, package2PK);

			CreateNonMasterConfirmationWithDivotLink(nonMasterInstructionPicPK, divotPic1PK);
			CreateNonMasterConfirmationWithDivotLink(nonMasterInstructionPicPK, divotPic2PK);
			CreateNonMasterConfirmationWithoutDivotLink(nonMasterInstructionDlvPK);
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

		Guid CreateMasterConfirmationWithoutDivotLink(Guid instructionPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_IsMaster, KK_MasterBookingVersion, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', 1, 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			masterBookingConfirmationPKsWithoutDivotLink.Add(confirmationPK);

			return confirmationPK;
		}

		Guid CreateMasterConfirmationWithDivotLink(Guid instructionPK, Guid instructionPkgDivotPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_KD_BookingInstructionPkgDivot, KK_IsMaster, KK_MasterBookingVersion, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', '{instructionPkgDivotPK}', 1, 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			masterBookingConfirmationPKsWithDivotLink.Add(confirmationPK);

			return confirmationPK;
		}

		void CreateSubConfirmationWithoutDivotLink(Guid masterConfirmationPK, Guid instructionPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_KK_MasterBookingConfirmation, KK_MasterBookingVersion, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', '{masterConfirmationPK}', 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			subBookingConfirmationPKsWithoutDivotLink.Add(confirmationPK);
		}

		void CreateSubConfirmationWithDivotLink(Guid masterConfirmationPK, Guid instructionPK, Guid instructionPkgDivotPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_KD_BookingInstructionPkgDivot, KK_KK_MasterBookingConfirmation, KK_MasterBookingVersion, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', '{instructionPkgDivotPK}', '{masterConfirmationPK}', 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			subBookingConfirmationPKsWithDivotLink.Add(confirmationPK);
		}

		void CreateNonMasterConfirmationWithoutDivotLink(Guid instructionPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			nonMasterBookingConfirmationPKsWithoutDivotLink.Add(confirmationPK);
		}

		void CreateNonMasterConfirmationWithDivotLink(Guid instructionPK, Guid instructionPkgDivotPK)
		{
			var confirmationPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingConfirmation (KK_PK, KK_KN_BookingInstruction, KK_KD_BookingInstructionPkgDivot, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser) VALUES
						('{confirmationPK}', '{instructionPK}', '{instructionPkgDivotPK}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			nonMasterBookingConfirmationPKsWithDivotLink.Add(confirmationPK);
		}

		Guid CreateDivot(MasterBookingType masterBookingType, Guid instructionPK, Guid packagePK)
		{
			var divotPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.DtbBookingInstructionPkgDivot (KD_PK, KD_KN_BookingInstruction, KD_KP_Package, KD_Quantity, KD_SystemCreateTimeUtc, KD_SystemCreateUser, KD_SystemLastEditTimeUtc, KD_SystemLastEditUser) VALUES
						('{divotPK}', '{instructionPK}', '{packagePK}', 1, GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			switch (masterBookingType)
			{
				case MasterBookingType.Master:
					masterBookingInstructionPkgDivotPKs.Add(divotPK);
					break;

				case MasterBookingType.Sub:
					subBookingInstructionPkgDivotPKs.Add(divotPK);
					break;

				case MasterBookingType.NonMaster:
					nonMasterBookingInstructionPkgDivotPKs.Add(divotPK);
					break;
			}

			return divotPK;
		}

		Guid CreatePackageJob(Guid bookingConsolidationPK, string packageJobID)
		{
			var packageJobPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.PkgPackageJob (KJ_PK, KJ_JobID, KJ_ParentID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES
						('{packageJobPK}', '{packageJobID}', '{bookingConsolidationPK}', '{DtbBookingConsolidationSchema.Constants.Prefix}', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			packageJobPKs.Add(packageJobPK);

			return packageJobPK;
		}

		Guid CreatePackage(Guid packageJobPK, short packageSequence)
		{
			var packagePK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.PkgPackage (KP_PK, KP_KJ_ParentPackageJob, KP_F3_NKPackType, KP_PackageQty, KP_Weight, KP_WeightUQ, KP_Volume, KP_VolumeUQ, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES
						('{packagePK}', '{packageJobPK}', 'PLT', 1, 1, 'KG', 1, 'M3', GETDATE(), 'XXX', GETDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();
			packagePKs.Add(packageJobPK);

			return packagePK;
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

		void AssertConfirmationDivotLinks(string confirmationsDescription, IEnumerable<Guid> confirmationPKsToCheck, bool expectedToBeBlank)
		{
			var confirmationPKsToCheckString = string.Join(",", confirmationPKsToCheck.Select(pk => FormattableString.Invariant($"'{pk}'")).ToArray());
			var blankOrNotBlankCheckString = expectedToBeBlank ? "IS NULL" : "IS NOT NULL";
			var query = FormattableString.Invariant($@"SELECT COUNT(*) AS cnt FROM dbo.DtbBookingConfirmation WHERE KK_PK IN ({confirmationPKsToCheckString}) AND KK_KD_BookingInstructionPkgDivot {blankOrNotBlankCheckString}");
			var count = TestConnection.ExecuteScalar(query);

			var assertMessage = expectedToBeBlank ?
				FormattableString.Invariant($"{confirmationsDescription} should have had their link to instruction package divot removed") :
				FormattableString.Invariant($"{confirmationsDescription} should have had their link to instruction package divot left unchanged");
			var expectedCount = confirmationPKsToCheck.Count();
			AssertEquals(assertMessage, expectedCount, count);
		}

		void ClearAllPKLists()
		{
			masterBookingConsolidationPKs.Clear();
			masterBookingPKs.Clear();
			masterBookingInstructionPKs.Clear();
			masterBookingInstructionPkgDivotPKs.Clear();
			masterBookingConfirmationPKsWithDivotLink.Clear();
			masterBookingConfirmationPKsWithoutDivotLink.Clear();

			subBookingConsolidationPKs.Clear();
			subBookingPKs.Clear();
			subBookingInstructionPKs.Clear();
			subBookingInstructionPkgDivotPKs.Clear();
			subBookingConfirmationPKsWithDivotLink.Clear();
			subBookingConfirmationPKsWithoutDivotLink.Clear();

			nonMasterBookingConsolidationPKs.Clear();
			nonMasterBookingPKs.Clear();
			nonMasterBookingInstructionPKs.Clear();
			nonMasterBookingInstructionPkgDivotPKs.Clear();
			nonMasterBookingConfirmationPKsWithDivotLink.Clear();
			nonMasterBookingConfirmationPKsWithoutDivotLink.Clear();

			packageJobPKs.Clear();
			packagePKs.Clear();
		}

		readonly List<Guid> masterBookingConsolidationPKs = new List<Guid>();
		readonly List<Guid> masterBookingPKs = new List<Guid>();
		readonly List<Guid> masterBookingInstructionPKs = new List<Guid>();
		readonly List<Guid> masterBookingConfirmationPKsWithoutDivotLink = new List<Guid>();
		readonly List<Guid> masterBookingConfirmationPKsWithDivotLink = new List<Guid>();
		readonly List<Guid> masterBookingInstructionPkgDivotPKs = new List<Guid>();

		readonly List<Guid> subBookingConsolidationPKs = new List<Guid>();
		readonly List<Guid> subBookingPKs = new List<Guid>();
		readonly List<Guid> subBookingInstructionPKs = new List<Guid>();
		readonly List<Guid> subBookingConfirmationPKsWithoutDivotLink = new List<Guid>();
		readonly List<Guid> subBookingConfirmationPKsWithDivotLink = new List<Guid>();
		readonly List<Guid> subBookingInstructionPkgDivotPKs = new List<Guid>();

		readonly List<Guid> nonMasterBookingConsolidationPKs = new List<Guid>();
		readonly List<Guid> nonMasterBookingPKs = new List<Guid>();
		readonly List<Guid> nonMasterBookingInstructionPKs = new List<Guid>();
		readonly List<Guid> nonMasterBookingConfirmationPKsWithoutDivotLink = new List<Guid>();
		readonly List<Guid> nonMasterBookingConfirmationPKsWithDivotLink = new List<Guid>();
		readonly List<Guid> nonMasterBookingInstructionPkgDivotPKs = new List<Guid>();

		readonly List<Guid> packageJobPKs = new List<Guid>();
		readonly List<Guid> packagePKs = new List<Guid>();

		enum MasterBookingType { Master, Sub, NonMaster }
	}
}
