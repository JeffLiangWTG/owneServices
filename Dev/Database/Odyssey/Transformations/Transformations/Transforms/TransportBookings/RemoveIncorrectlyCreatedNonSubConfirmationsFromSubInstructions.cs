using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings
{
	public class RemoveIncorrectlyCreatedNonSubConfirmationsFromSubInstructions : DataTransformation
	{
		public override string UserDescription => "Remove incorrectly created non-sub DtbBookingConfirmation records on sub DtbBookingInstructions";
		const string LastProcessedChunkPKName = "RemoveIncorrectlyCreatedNonSubConfirmationsFromSubInstructions.KK_KN_BookingInstruction.LastProcessedChunkPKName";
		const int BatchSize = 10000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var chunks = GuidChunker.GenerateChunks(BatchSize, DataUtils.GetApproximateRowCountForTable(Db.Connection, DtbBookingConfirmationSchema.Constants.TableName), lastProcessedPK);

			foreach (var chunk in chunks)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				ProcessChunk(chunk.LowerBound, chunk.UpperBound);

				ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, chunk.UpperBound.ToString());
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		void ProcessChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(DeleteIncorrectConfirmationChunkSQL))
			{
				command.AddParameter("@fromInstructionPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@toInstructionPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		const string DeleteIncorrectConfirmationChunkSQL = @"
DELETE conf
FROM dbo.DtbBookingConfirmation conf
JOIN dbo.DtbBookingInstruction inst
	ON conf.KK_KN_BookingInstruction = inst.KN_PK
WHERE conf.KK_KN_BookingInstruction BETWEEN @fromInstructionPK AND @toInstructionPK
	AND conf.KK_KK_MasterBookingConfirmation IS NULL
	AND inst.KN_KN_MasterBookingInstruction IS NOT NULL;
";
	}
}
