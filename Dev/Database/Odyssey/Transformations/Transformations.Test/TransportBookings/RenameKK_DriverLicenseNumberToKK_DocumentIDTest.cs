using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.TransportBookings
{
	[UseSnapshotProtection]
	[TestedType(typeof(RenameKK_DriverLicenseNumberToKK_DocumentID))]
	class RenameKK_DriverLicenseNumberToKK_DocumentIDTest : DataTransformationTestCase
	{
		const string OldColumnName = "KK_DriverLicenseNumber";
		const string TableName = DtbBookingConfirmationSchema.Constants.TableName;
		const string NewColumnName = DtbBookingConfirmationSchema.Constants.KK_DocumentID;

		protected override DataTransformation GetNewTestTransformationInstance() => new RenameKK_DriverLicenseNumberToKK_DocumentID();

		readonly string emptyDocumentID = string.Empty;
		readonly string documentIDWithNumbersOnly = "0123456789";
		readonly string documentIDWithLettersOnly = "ABCDEFG";
		readonly string documentIDWithMix = "ABC 123";

		readonly Random randomizer = new();

		protected override void PrepareTestData()
		{
			SetUpTableColumns();
			CreateBookingWithDocumentIDs(emptyDocumentID, documentIDWithNumbersOnly, documentIDWithLettersOnly, documentIDWithMix);
		}

		void SetUpTableColumns()
		{
			if (DbObjectCreator.ColumnExists(TestConnection, TableName, NewColumnName))
			{
				new DbColumnDependencyRemover(TableName, NewColumnName).DropRelateObjects(TestConnection);
				bool successfullyRenameColumnBackToOldName = DbObjectCreator.RenameColumn(TestConnection, TableName, NewColumnName, OldColumnName);
				Assert($"The old column: {OldColumnName} exists in the DB but the system failed to rename it to {NewColumnName}", successfullyRenameColumnBackToOldName);
			}

			_ = DbObjectCreator.CreateColumnIfNotExists(TestConnection, TableName, OldColumnName, "varchar(25)", string.Empty);
		}

		void CreateBookingWithDocumentIDs(params string[] documentIDs)
		{
			var consolidationID = CreateBookingConsolidation();
			var bookingID = CreateDtbBooking(consolidationID);
			var bookingInstructionId = CreateDtbBookingInstruction(bookingID);

			foreach (var documentID in documentIDs)
			{
				CreateDtbBookingConfirmation(bookingInstructionId, documentID);
			}
		}

		Guid CreateBookingConsolidation()
		{
			var consolidationID = Guid.NewGuid();
			var randomBookingJobID = randomizer.Next(0,1_000_000);

			var sql = FormattableString.Invariant($@"
				INSERT INTO [dbo].[DtbBookingConsolidation] (KB_PK, KB_JobID, KB_SystemCreateTimeUtc, KB_SystemCreateUser, KB_SystemLastEditTimeUtc, KB_SystemLastEditUser)
				VALUES ('{consolidationID}', '{randomBookingJobID}', GETDATE(), 'XXX', GETDATE(), 'XXX')");
			_ = TestConnection.Command(sql).ExecuteNonQuery();

			return consolidationID;
		}

		Guid CreateDtbBooking(Guid consolidationID)
		{
			var bookingID = Guid.NewGuid();
			var randomJobID = randomizer.Next(0, 1_000_000);

			var sql = FormattableString.Invariant($@"
				INSERT INTO [dbo].[DtbBooking] (KM_PK, KM_KB_Booking, KM_JobID, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser)
				VALUES ('{bookingID}', '{consolidationID}', '{randomJobID}', GETDATE(), 'XXX', GETDATE(), 'XXX')");
			_ = TestConnection.Command(sql).ExecuteNonQuery();

			return bookingID;
		}

		Guid CreateDtbBookingInstruction(Guid bookingID)
		{
			var bookingInstructionId = Guid.NewGuid();

			var sql = FormattableString.Invariant($@"
				INSERT INTO [dbo].[DtbBookingInstruction] (KN_PK, KN_KM_BookingMovement, KN_SystemCreateTimeUtc, KN_SystemCreateUser, KN_SystemLastEditTimeUtc, KN_SystemLastEditUser)
				VALUES ('{bookingInstructionId}', '{bookingID}', GETDATE(), 'XXX', GETDATE(), 'XXX' )");
			_ = TestConnection.Command(sql).ExecuteNonQuery();

			return bookingInstructionId;
		}

		void CreateDtbBookingConfirmation(Guid bookingInstructionID, string documentID)
		{
			var dtbBookingConfirmationID = Guid.NewGuid();

			var sql = FormattableString.Invariant($@"
				INSERT INTO [dbo].[DtbBookingConfirmation] (KK_PK, KK_KN_BookingInstruction, KK_DriverLicenseNumber, KK_SystemCreateTimeUtc, KK_SystemCreateUser, KK_SystemLastEditTimeUtc, KK_SystemLastEditUser)
				VALUES ('{dtbBookingConfirmationID}', '{bookingInstructionID}', '{documentID}', GETDATE(), 'XXX', GETDATE(), 'XXX' )");
			_ = TestConnection.Command(sql).ExecuteNonQuery();
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertEquals($"{OldColumnName} is still in the database", expected: false, DbObjectCreator.ColumnExists(TestConnection, TableName, OldColumnName));
				Assert($"{NewColumnName} is not in the database", DbObjectCreator.ColumnExists(TestConnection, TableName, NewColumnName));
				Assert($"Missing data: a DtbBookingConfirmation with document ID \"{emptyDocumentID}\" should exist in the DB", CheckDocumentIDExistInDB(emptyDocumentID));
				Assert($"Missing data: a DtbBookingConfirmation with document ID \"{documentIDWithNumbersOnly}\" should exist in the DB", CheckDocumentIDExistInDB(documentIDWithNumbersOnly));
				Assert($"Missing data: a DtbBookingConfirmation with document ID \"{documentIDWithLettersOnly}\" should exist in the DB", CheckDocumentIDExistInDB(documentIDWithLettersOnly));
				Assert($"Missing data: a DtbBookingConfirmation with document ID \"{documentIDWithMix}\" should exist in the DB", CheckDocumentIDExistInDB(documentIDWithMix));
			});
		}

		bool CheckDocumentIDExistInDB(string documentID)
		{
			var query = FormattableString.Invariant($@"
				SELECT COUNT(*) FROM [dbo].[DtbBookingConfirmation] WHERE KK_DocumentID = '{documentID}'");

			return TestConnection.ExecuteScalar<int>(query) > 0;
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
		}

		protected override sealed void TearDown()
		{
			disposableAdminConnection.Dispose();
			base.TearDown();
		}

		IDisposable disposableAdminConnection;
	}
}
