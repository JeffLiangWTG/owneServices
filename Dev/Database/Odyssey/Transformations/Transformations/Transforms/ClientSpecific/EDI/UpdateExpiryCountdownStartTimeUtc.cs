using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class UpdateExpiryCountdownStartTimeUtc : DataTransformation
	{
		public override string UserDescription => "Update ExpiryCountdownStartTimeUtc";

		const string LastProcessedIncidentNumberName = "UpdateExpiryCountdownStartTimeUtc.LastProcessedIncidentNumber";
		const int BatchSize = 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (!token.IsCancellationRequested && DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentRequest")
											   && DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMain"))
			{
				var startIncidentNumberString = ExtProperty.Database.Select(Db.Connection, LastProcessedIncidentNumberName);
				var startIncidentNumber = string.IsNullOrEmpty(startIncidentNumberString) ? "CS00000001" : startIncidentNumberString;

				var maxIncidentNumberQuery = "SELECT MAX(INC_IncidentNumber) FROM dbo.IncidentRequest WHERE INC_ExpiryCountdownStartTimeUtc IS NULL AND INC_IncidentNumber LIKE 'CS[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]';";
				string maxIncidentNumber;
				using (var cmd = Db.Connection.Command(maxIncidentNumberQuery))
				{
					var commandResult = cmd.ExecuteScalar();
					if (Convert.IsDBNull(commandResult))
					{
						return;
					}
					maxIncidentNumber = (string)commandResult;
				}

				var stopWatch = Stopwatch.StartNew();

				manager.ShowInfoMessage("Started updating ExpiryCountdownStartTimeUtc.");
				stopWatch.Start();

				while (string.Compare(startIncidentNumber, maxIncidentNumber) <= 0)
				{
					var endIncidentNumber = IncrementIncidentNumber(startIncidentNumber, BatchSize);
					UpdateChunk(startIncidentNumber, endIncidentNumber);

					startIncidentNumber = endIncidentNumber;

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						ExtProperty.Database.Update(Db.Connection, LastProcessedIncidentNumberName, startIncidentNumber);
						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}

				stopWatch.Stop();
				ExtProperty.Database.Delete(Db.Connection, LastProcessedIncidentNumberName);
				manager.ShowInfoMessage("Finished updating ExpiryCountdownStartTimeUtc.");
			}
		}

		void UpdateChunk(string startIncidentNumber, string endIncidentNumber)
		{
			var query = @"
				BEGIN TRY
					BEGIN TRANSACTION;

					WITH IncidentAwaitingResponseInfo AS (
						SELECT 
							ir.INC_PK AS IncidentPK,
							ir.INC_IncidentNumber,
							COALESCE(
								(
									SELECT TOP 1 al.SL_EventTimeUtc
									FROM StmALog al
									WHERE al.SL_Parent = im.IM_PK 
										AND im.IM_INC_Request = ir.INC_PK
										AND al.SL_SE_NKEvent = 'IWR'
										AND im.IM_ResolutionCode = 'CWR'
									ORDER BY al.SL_EventTimeUtc DESC
								),
								(
									SELECT TOP 1 al.SL_EventTimeUtc
									FROM StmALog al
									WHERE al.SL_Parent = im.IM_PK 
										AND im.IM_INC_Request = ir.INC_PK
										AND al.SL_Reference LIKE '%to CWR'
										AND im.IM_ResolutionCode = 'CWR'
									ORDER BY al.SL_EventTimeUtc DESC
								),
								(
									CASE 
										WHEN im.IM_ResolveTimeUtc IS NOT NULL THEN im.IM_ResolveTimeUtc
										ELSE NULL
									END
								)
							) AS ExpiryCountdownStartTimeUtc
						FROM IncidentRequest ir
						INNER JOIN IncidentMain im ON ir.INC_PK = im.IM_INC_Request
						WHERE ir.INC_IncidentNumber >= @startIncidentNumber AND ir.INC_IncidentNumber < @endIncidentNumber
						  AND ir.INC_ExpiryCountdownStartTimeUtc IS NULL
					)

					UPDATE ir
					SET ir.INC_ExpiryCountdownStartTimeUtc = IncidentAwaitingResponseInfo.ExpiryCountdownStartTimeUtc,
						ir.INC_SystemLastEditTimeUtc = GETUTCDATE(),
						ir.INC_SystemLastEditUser = '~BP'
					FROM IncidentRequest ir
					INNER JOIN IncidentAwaitingResponseInfo ON ir.INC_PK = IncidentAwaitingResponseInfo.IncidentPK;

					SELECT @@ROWCOUNT AS RowsUpdated;

					COMMIT TRANSACTION;
				END TRY
				BEGIN CATCH
					ROLLBACK TRANSACTION;
					THROW;
				END CATCH;
			";

			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameter("@startIncidentNumber", System.Data.SqlDbType.NVarChar, startIncidentNumber);
				cmd.AddParameter("@endIncidentNumber", System.Data.SqlDbType.NVarChar, endIncidentNumber);
				var rowsUpdated = cmd.ExecuteNonQuery();
				manager.ShowInfoMessage($"Batch Update: {rowsUpdated} rows updated. IncidentNumber range: {startIncidentNumber} to {endIncidentNumber}.");
			}
		}

		string IncrementIncidentNumber(string incidentNumber, int increment)
		{
			var numericPart = int.Parse(incidentNumber.Substring(2));
			var incrementedNumber = numericPart + increment;

			return $"CS{incrementedNumber:D8}";
		}
	}
}
