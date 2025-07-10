using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings.Testing
{
	[TestedType(typeof(ConvertTransportStatusErrorToActionRequired))]
	public class ConvertTransportStatusErrorToActionRequiredTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new ConvertTransportStatusErrorToActionRequired();
		}

		protected override void PrepareTestData()
		{
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000001", "ERR");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000002", "ACR");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000003", "AVL");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000004", "HLD");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000005", "");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000006", "ALC");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000007", "BKD");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000008", "DLV");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000009", "DNR");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000010", "DLA");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000011", "INC");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000012", "PIC");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000013", "PCA");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000014", "PCM");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000015", "PCN");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000016", "QTE");
			CreateBookingAndBookingConsolidationWithTransportStatus("TB00000017", "SVM");
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions("Check that correct records were converted and others were not touched", () =>
			{
				AssertCountAndIdsOfBookingsTransportStatuses(0, "ERR", new List<string> { });
				AssertCountAndIdsOfBookingsTransportStatuses(2, "ACR", new List<string> { "TB00000001", "TB00000002" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "AVL", new List<string> { "TB00000003" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "HLD", new List<string> { "TB00000004" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "", new List<string> { "TB00000005" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "ALC", new List<string> { "TB00000006" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "BKD", new List<string> { "TB00000007" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "DLV", new List<string> { "TB00000008" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "DNR", new List<string> { "TB00000009" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "DLA", new List<string> { "TB00000010" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "INC", new List<string> { "TB00000011" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "PIC", new List<string> { "TB00000012" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "PCA", new List<string> { "TB00000013" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "PCM", new List<string> { "TB00000014" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "PCN", new List<string> { "TB00000015" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "QTE", new List<string> { "TB00000016" });
				AssertCountAndIdsOfBookingsTransportStatuses(1, "SVM", new List<string> { "TB00000017" });
			});
		}

		void CreateBookingAndBookingConsolidationWithTransportStatus(string jobId, string transportStatus)
		{
			var consolidationID = CreateBookingConsolidation(jobId, transportStatus);
			var bookingID = CreateDtbBooking(consolidationID, jobId, transportStatus);
		}

		public void TestCancellationTokenAndExtProperties()
		{
			PrepareTestData();
			var logger = new List<string>();
			AssertNull("ExtProperty.LastProcessedKM_JobID should be empty.", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKM_JobID));
			AssertNull("ExtProperty.HasProcessedAllKM_JobIDs should be empty.", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.HasProcessedAllKM_JobIDs));
			AssertNull("ExtProperty.LastProcessedKB_JobID should be empty.", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKB_JobID));

			var cancellationToken = new CancellationToken(true);
			var transformation = new ConvertTransportStatusErrorToActionRequired();

			AssertExceptionThrown<OperationCanceledException>("Run one chunk (first 10 rows of data for DtbBooking) when token is cancelled", () => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertEquals("ExtProperty.LastProcessedKM_JobID should be 'TB00000010' as transformation should have processed some booking data.", "TB00000010", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKM_JobID));
			AssertEquals("ExtProperty.HasProcessedAllKM_JobIDs should be false as transformation has not finished processing bookings.", bool.FalseString, GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.HasProcessedAllKM_JobIDs));
			AssertNull("ExtProperty.LastProcessedKB_JobID should still be empty as cancellation token was cancelled", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKB_JobID));

			cancellationToken = new CancellationToken(true);
			transformation = new ConvertTransportStatusErrorToActionRequired();

			AssertExceptionThrown<OperationCanceledException>("Run next chunk (remaining test data for DtbBooking) when token is cancelled", () => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertEquals("ExtProperty.LastProcessedKM_JobID should be 'TB00000017' as transformation should have processed all booking data.", "TB00000017", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKM_JobID));
			AssertEquals("ExtProperty.HasProcessedAllKM_JobIDs should be true as transformation has finished processing bookings.", bool.TrueString, GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.HasProcessedAllKM_JobIDs));
			AssertNull("ExtProperty.LastProcessedKB_JobID should still be empty as cancellation token was cancelled", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKB_JobID));

			cancellationToken = new CancellationToken(true);
			transformation = new ConvertTransportStatusErrorToActionRequired();
			AssertExceptionThrown<OperationCanceledException>("Run the next chunk (first 10 rows of data for DtbBookingConsolidation) when token is cancelled", () => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertEquals("ExtProperty.LastProcessedKM_JobID should be 'TB00000017' as transformation should have processed all booking data.", "TB00000017", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKM_JobID));
			AssertEquals("ExtProperty.HasProcessedAllKM_JobIDs should be true as transformation has finished processing bookings.", bool.TrueString, GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.HasProcessedAllKM_JobIDs));
			AssertEquals("ExtProperty.LastProcessedKB_JobID should be be 'TB00000010' as transformation should have processed some booking consolidation data.", "TB00000010", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKB_JobID));

			cancellationToken = new CancellationToken(true);
			transformation = new ConvertTransportStatusErrorToActionRequired();
			AssertExceptionThrown<OperationCanceledException>("Run the next chunk (remaining test data for DtbBookingConsolidation) when token is cancelled", () => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertEquals("ExtProperty.LastProcessedKM_JobID should be 'TB00000017' as transformation should have processed all booking data.", "TB00000017", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKM_JobID));
			AssertEquals("ExtProperty.HasProcessedAllKM_JobIDs should be true as transformation has finished processing bookings.", bool.TrueString, GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.HasProcessedAllKM_JobIDs));
			AssertEquals("ExtProperty.LastProcessedKB_JobID should be be 'TB00000017' as transformation should have processed all booking consolidation data.", "TB00000017", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKB_JobID));

			transformation = new ConvertTransportStatusErrorToActionRequired();
			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertNull("ExtProperty.LastProcessedKM_JobID should be cleared when the transformation runs successfully without a cancellation token.", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKM_JobID));
			AssertNull("ExtProperty.HasProcessedAllKM_JobIDs should be cleared when the transformation runs successfully without a cancellation token.", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.HasProcessedAllKM_JobIDs));
			AssertNull("ExtProperty.LastProcessedKB_JobID should be cleared when the transformation runs successfully without a cancellation token.", GetExtPropertyValue(ConvertTransportStatusErrorToActionRequired.LastProcessedKB_JobID));
		}

		public static string GetExtPropertyValue(string extPropertyName) => ExtProperty.Database.Select(Db.Connection, extPropertyName);

		Guid CreateBookingConsolidation(string jobId, string transportStatus)
		{
			var consolidationID = Guid.NewGuid();

			var sql = FormattableString.Invariant($@"
				INSERT INTO [dbo].[DtbBookingConsolidation] (KB_PK, KB_JobID, KB_Status, KB_SystemCreateTimeUtc, KB_SystemCreateUser, KB_SystemLastEditTimeUtc, KB_SystemLastEditUser)
				VALUES ('{consolidationID}', '{jobId}', '{transportStatus}', GETDATE(), 'XXX', GETDATE(), 'XXX')");
			_ = TestConnection.Command(sql).ExecuteNonQuery();

			return consolidationID;
		}

		Guid CreateDtbBooking(Guid consolidationID, string jobId, string transportStatus)
		{
			var bookingID = Guid.NewGuid();

			var sql = FormattableString.Invariant($@"
				INSERT INTO [dbo].[DtbBooking] (KM_PK, KM_KB_Booking, KM_JobID, KM_Status, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser)
				VALUES ('{bookingID}', '{consolidationID}', '{jobId}', '{transportStatus}', GETDATE(), 'XXX', GETDATE(), 'XXX')");
			_ = TestConnection.Command(sql).ExecuteNonQuery();

			return bookingID;
		}

		void AssertCountAndIdsOfBookingsTransportStatuses(int expectedCount, string transportStatus, List<string> expectedJobIds)
		{
			var sqlCount = FormattableString.Invariant($@"
        SELECT COUNT(*)
        FROM [dbo].[DtbBooking] b
        INNER JOIN [dbo].[DtbBookingConsolidation] c ON b.KM_KB_Booking = c.KB_PK
        WHERE c.KB_Status = '{transportStatus}'");

			var actualCount = Convert.ToInt32(TestConnection.Command(sqlCount).ExecuteScalar());

			AssertEquals($"Should have {expectedCount} bookings with status {transportStatus}", expectedCount, actualCount);

			foreach (var expectedJobId in expectedJobIds)
			{
				var sqlJobIdCheck = FormattableString.Invariant($@"
            SELECT COUNT(*)
            FROM [dbo].[DtbBooking] b
            INNER JOIN [dbo].[DtbBookingConsolidation] c ON b.KM_KB_Booking = c.KB_PK
            WHERE c.KB_Status = '{transportStatus}' AND b.KM_JobID = '{expectedJobId}'");

				var jobIdCount = Convert.ToInt32(TestConnection.Command(sqlJobIdCheck).ExecuteScalar());

				Assert($"{expectedJobId} should have status {transportStatus}", jobIdCount == 1);
			}
		}
	}
}
