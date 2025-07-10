using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings
{
	public class RemovePackageDivotsOnSubInstructions : DataTransformation
	{
		public override string UserDescription => "Remove DtbBookingInstructionPkgDivots on sub DtbBookingInstructions, after removing links to them from DtbBookingConfirmation records";
		const string LastProcessedChunkPKName = "RemovePackageDivotsOnSubInstructions.KN_KM_BookingMovement.LastProcessedChunkPKName";
		const int BatchSize = 3000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var chunks = GuidChunker.GenerateChunks(BatchSize, DataUtils.GetApproximateRowCountForTable(Db.Connection, DtbBookingInstructionSchema.Constants.TableName), lastProcessedPK);

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

		void ProcessChunk(Guid fromBookingPK, Guid toBookingPK)
		{
			using (var command = Db.Connection.Command(DeleteSubBookingInstructionDivotsChunkSQL))
			{
				command.AddParameter("@fromBookingPK", SqlDbType.UniqueIdentifier, fromBookingPK);
				command.AddParameter("@toBookingPK", SqlDbType.UniqueIdentifier, toBookingPK);
				command.ExecuteNonQuery();
			}
		}

		const string DeleteSubBookingInstructionDivotsChunkSQL = @"
	BEGIN TRY
		BEGIN TRANSACTION;
		
		UPDATE conf
			SET KK_KD_BookingInstructionPkgDivot = NULL,
				KK_SystemLastEditTimeUtc = GETUTCDATE(),
				KK_SystemLastEditUser = '~BP'
			FROM dbo.DtbBookingConfirmation conf
			JOIN dbo.DtbBookingInstruction inst ON conf.KK_KN_BookingInstruction = inst.KN_PK
			JOIN dbo.DtbBookingInstructionPkgDivot divot ON divot.KD_KN_BookingInstruction = inst.KN_PK
			WHERE inst.KN_KM_BookingMovement BETWEEN @fromBookingPK AND @toBookingPK
			AND inst.KN_KN_MasterBookingInstruction IS NOT NULL;

		DELETE divot
			FROM dbo.DtbBookingInstruction inst
			JOIN dbo.DtbBookingInstructionPkgDivot divot ON divot.KD_KN_BookingInstruction = inst.KN_PK
			WHERE inst.KN_KM_BookingMovement BETWEEN @fromBookingPK AND @toBookingPK
			AND inst.KN_KN_MasterBookingInstruction IS NOT NULL;

		COMMIT;
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK TRANSACTION;
		END;
		THROW;
	END CATCH
";
	}
}
