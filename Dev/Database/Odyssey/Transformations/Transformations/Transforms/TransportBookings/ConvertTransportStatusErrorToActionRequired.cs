using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings
{
	public class ConvertTransportStatusErrorToActionRequired : DataTransformation
	{
		public override string UserDescription => "Replace Transport Bookings and Booking Consolidations Transport Status Error to Action Required";

#if DEBUG
		public
#endif
		const string LastProcessedKM_JobID = "UpdateTableDtbBookingColumnKM_JobID_LastProcessedVarChar";

#if DEBUG
		public
#endif
		const string HasProcessedAllKM_JobIDs = "UpdateTableDtbBookingColumnKM_JobID_HasProcessedAllJobIDs";

#if DEBUG
		public
#endif
		const string LastProcessedKB_JobID = "UpdateTableDtbBookingConsolidationColumnKB_JobID_LastProcessedVarChar";

		const string UpdateDtbBookingChunkSql = @"	UPDATE DtbBooking
													SET KM_Status = 'ACR', KM_SystemLastEditTimeUtc = GETUTCDATE(), KM_SystemLastEditUser = '~BP'
													WHERE KM_Status = 'ERR'
													AND KM_JobID >= @FromId AND KM_JobID <= @ToId;";

		const string UpdateDtbBookingConsolidationChunkSql = @"	UPDATE DtbBookingConsolidation
																SET KB_Status = 'ACR', KB_SystemLastEditTimeUtc = GETUTCDATE(), KB_SystemLastEditUser = '~BP'
																WHERE KB_Status = 'ERR'
																AND KB_JobID >= @FromId AND KB_JobID <= @ToId;";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var chunkSize = 10000;
#if DEBUG
			chunkSize = 10;
#endif
			var hasProcessedAllKM_JobIDs = bool.Parse(ExtProperty.Database.Select(Db.Connection, HasProcessedAllKM_JobIDs) ?? bool.FalseString);
			var stopwatch = Stopwatch.StartNew();
			if (!hasProcessedAllKM_JobIDs)
			{
				var lastProcessedKM_JobID = ExtProperty.Database.Select(Db.Connection, LastProcessedKM_JobID);

				foreach (var chunk in VarCharChunker.GenerateChunks(chunkSize, lastProcessedKM_JobID, DtbBookingSchema.KM_JobID))
				{
					using (var cmd = Db.Connection.Command(UpdateDtbBookingChunkSql))
					{
						cmd.AddParameterBasedOnDbColumn("@FromId", chunk.LowerBound, DtbBookingSchema.KM_JobID);
						cmd.AddParameterBasedOnDbColumn("@ToId", chunk.UpperBound, DtbBookingSchema.KM_JobID);

						cmd.ExecuteNonQuery();
					}

					if (token.IsCancellationRequested || stopwatch.Elapsed.TotalMinutes > 1)
					{
						ExtProperty.Database.Update(Db.Connection, LastProcessedKM_JobID, chunk.UpperBound);
						ExtProperty.Database.Update(Db.Connection, HasProcessedAllKM_JobIDs, chunk.RowCount < chunkSize ? bool.TrueString : bool.FalseString);
						manager.ShowInfoMessage($"Processed values up to {chunk.UpperBound} for DtbBooking.KM_JobID");
						token.ThrowIfCancellationRequested();
						stopwatch.Restart();
					}
				}
				ExtProperty.Database.Update(Db.Connection, HasProcessedAllKM_JobIDs, bool.TrueString);
			}

			var lastProcessedKB_JobID = ExtProperty.Database.Select(Db.Connection, LastProcessedKB_JobID);

			foreach (var chunk in VarCharChunker.GenerateChunks(chunkSize, lastProcessedKB_JobID, DtbBookingConsolidationSchema.KB_JobID))
			{
				using (var cmd = Db.Connection.Command(UpdateDtbBookingConsolidationChunkSql))
				{
					cmd.AddParameterBasedOnDbColumn("@FromId", chunk.LowerBound, DtbBookingConsolidationSchema.KB_JobID);
					cmd.AddParameterBasedOnDbColumn("@ToId", chunk.UpperBound, DtbBookingConsolidationSchema.KB_JobID);

					cmd.ExecuteNonQuery();
				}

				if (token.IsCancellationRequested || stopwatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, LastProcessedKB_JobID, chunk.UpperBound);
					manager.ShowInfoMessage($"Processed values up to {chunk.UpperBound} for DtbBookingConsolidation.KB_JobID");
					token.ThrowIfCancellationRequested();
					stopwatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedKM_JobID);
			ExtProperty.Database.Delete(Db.Connection, HasProcessedAllKM_JobIDs);
			ExtProperty.Database.Delete(Db.Connection, LastProcessedKB_JobID);
		}
	}
}
