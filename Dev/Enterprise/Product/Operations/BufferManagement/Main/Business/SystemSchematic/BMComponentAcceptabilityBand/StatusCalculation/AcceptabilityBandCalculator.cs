using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	class AcceptabilityBandCalculator
	{
		public AcceptabilityBandResultCollection CalculateStatus(BMComponentAcceptabilityBand acceptabilityBand, AcceptabilityBandDataProvider provider, AcceptabilityBandSqlBuilderParameters parameters)
		{
			var stopwatch = Stopwatch.StartNew();
			var results = GetResults(acceptabilityBand, parameters, provider);
			stopwatch.Stop();

			foreach (var result in results)
			{
				if (result.Status != ComponentAcceptabilityStatus.Timeout)
				{
					result.SetStatus(parameters.BoundaryValues);
				}

				result.CalculationDuration = stopwatch.Elapsed;
			}

			return results;
		}

		static AcceptabilityBandResultCollection GetResults(BMComponentAcceptabilityBand acceptabilityBand, AcceptabilityBandSqlBuilderParameters parameters, AcceptabilityBandDataProvider provider)
		{
			var results = new AcceptabilityBandResultCollection();

			if (!acceptabilityBand.BAB_IsActive || !parameters.AreValid)
			{
				AddEmptyResult(acceptabilityBand, results);
				return results;
			}

			var connection = provider.ConnectionWrapper.Connection;

			if (TryGetResultFromMENT(acceptabilityBand, parameters, connection) is AcceptabilityBandResult result)
			{
				results.Add(result);
				return results;
			}

			var isUsingAdditionalAggregator = false;
			DbCommand command = null;

			try
			{
				command = acceptabilityBand.GetAcceptabilityBandSqlCommand(parameters, connection);
			}
			catch (InvalidFilterConfigurationException)
			{
				// this one is okay. It's likely caused by a country-specific filter configured on the band, and the user is in another country.
				// They can report it to their admins if they notice a blank value. We don't want to bother anyone with an email or error report.
			}

			if (command != null)
			{
				using (command)
				{
					try
					{
						ReadResults(acceptabilityBand, command, ref isUsingAdditionalAggregator, results, provider);
					}
					catch (SqlException sqlException)
					{
						if (ZExceptionExtensions.IsInfrastructureDbError(sqlException) || sqlException.IsSpecifiedError(DbErrorType.LockTimeoutExpired) || sqlException.IsSpecifiedError(DbErrorType.TimeoutExpired))
						{
							if (sqlException.IsSpecifiedError(DbErrorType.TimeoutExpired))
							{
								var timeoutAB = new AcceptabilityBandResult(false, null, null, acceptabilityBand.PK);
								timeoutAB.Status = ComponentAcceptabilityStatus.Timeout;
								results.Add(timeoutAB);
							}
							// Om nom. We're probably already closing the application and/or reporting exceptions elsewhere.
						}
						else
						{
							var subject = Res.GetString("0DD61903-442C-4772-8E44-21D897A0B106", "Acceptability Band SQL Error");
							var body = Res.GetString("8E10FF4F-863A-44BA-AADB-5C61F23CDA90",
								"Acceptability Band SQL is invalid. The status calculation produced an error.\r\nThe acceptability band: {0}\r\nRelease Group PK: {1}\r\nSQL: {2}\r\n\r\n{3}",
								/*0*/acceptabilityBand.BAB_Name,
								/*1*/parameters.ReleaseGroupPK,
								/*2*/command.CommandText,
								/*3*/sqlException.ToString());

							body = DbCommand.SanitizeExecuteAsReaderFlags(body);
							new BMSEmailDef(subject, body).Send();
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						// Not sure if we're gobbling exceptions here that we/users should be aware of, so let's report them for now.
						var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"The acceptability band {0} caused an exception.\r\nreleaseGroupPK: {1}\r\n\r\nSQL: {2}", // Developer exception
							/*0*/ acceptabilityBand.BAB_Name,
							/*1*/ parameters.ReleaseGroupPK,
							/*2*/ command.CommandText);

						ErrorReporter.ReportOnce("8fd794b7-eae7-49f3-90aa-bba6ec485832", message, ex);
					}
				}
			}

			if (!isUsingAdditionalAggregator && !results.Any())
			{
				AddEmptyResult(acceptabilityBand, results);
			}

			return results;
		}

		static AcceptabilityBandResult TryGetResultFromMENT(BMComponentAcceptabilityBand acceptabilityBand, AcceptabilityBandSqlBuilderParameters parameters, DbConnection connection)
		{
			if (parameters.ShouldFilterBySection ||
				BMComponentAcceptabilityBand.IsAdditionalAggregatorColumnPresent(acceptabilityBand.BAB_SqlText.ToString()) ||
				parameters.Tag != null ||
				parameters.RunAsGoldenRule ||
				!BMSRegistry.Instance.AcceptabilityBandUseMENT.Value)
			{
				return null;
			}

			AcceptabilityBandResult result = null;

			var shouldFilterByReleaseGroup = parameters.ShouldFilterByReleaseGroup && parameters.ReleaseGroupPK.IsValid;
			var shouldFilterByComponent = acceptabilityBand.BAB_FC_Component.IsValid;

			try
			{
				var releaseGroupClause = shouldFilterByReleaseGroup ? $"AND MAS_GG_ReleaseGroup = @ReleaseGroup_PK" : string.Empty;
				var componentClause = shouldFilterByComponent ? $"AND MAS_FC_Component = @Component_PK" : string.Empty;

				var queryResult = connection.ExecuteScalar($@"
-- Acceptability Band Calculation Using MENT
-- Band name: [{acceptabilityBand.BAB_Name}], Type: [{acceptabilityBand.BAB_Type}]
WITH MaxDate AS
(
	SELECT MAX(MAS_TimeRecordedUtc) AS date
	FROM dbo.MENTAgedScoreMetric
	WHERE MAS_BAB_AcceptabilityBand = @BAB_PK {releaseGroupClause} {componentClause}
	AND MAS_TimeRecordedUtc > @MaxRecordedDateUtc
)
SELECT SUM(MAS_AgedScoreValue)
FROM dbo.MENTAgedScoreMetric
INNER JOIN MaxDate ON MAS_TimeRecordedUtc = MaxDate.date
WHERE MAS_BAB_AcceptabilityBand = @BAB_PK {releaseGroupClause} {componentClause}", dbParams =>
				{
					dbParams.AddParameter("@BAB_PK", System.Data.SqlDbType.UniqueIdentifier, acceptabilityBand.PK.ToGuid());
					var maxRecordedDateUtc = ZDateTime.UtcNow.AddMinutes(-BMSRegistry.Instance.AcceptabilityBandUseMentExpirationMinutes.Value).ToDateTime();
					dbParams.AddParameter("@MaxRecordedDateUtc", System.Data.SqlDbType.DateTime, maxRecordedDateUtc);

					if (shouldFilterByReleaseGroup)
					{
						dbParams.AddParameter("@ReleaseGroup_PK", System.Data.SqlDbType.UniqueIdentifier, parameters.ReleaseGroupPK.ToGuid());
					}

					if (shouldFilterByComponent)
					{
						dbParams.AddParameter("@Component_PK", System.Data.SqlDbType.UniqueIdentifier, acceptabilityBand.BAB_FC_Component.ToGuid());
					}
				});

				if (queryResult != null && queryResult != DBNull.Value)
				{
					result = new AcceptabilityBandResult(true, (decimal)queryResult, null, acceptabilityBand.PK);
				}
			}
			catch (SqlException sqlException)
			{
				if (ZExceptionExtensions.IsInfrastructureDbError(sqlException) || sqlException.IsSpecifiedError(DbErrorType.LockTimeoutExpired) || sqlException.IsSpecifiedError(DbErrorType.TimeoutExpired))
				{
					//if that happens it all doomed already!
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error when trying get AB result using MENT", $"Band name: [{acceptabilityBand.BAB_Name}], Type: [{acceptabilityBand.BAB_Type}]", ex);
			}

			return result;
		}

		static void AddEmptyResult(BMComponentAcceptabilityBand acceptabilityBand, AcceptabilityBandResultCollection results)
		{
			results.Add(new AcceptabilityBandResult(false, null, null, acceptabilityBand.PK));
		}

		static void ReadResults(BMComponentAcceptabilityBand acceptabilityBand, DbCommand command, ref bool isUsingAdditionalAggregator, AcceptabilityBandResultCollection results, AcceptabilityBandDataProvider provider)
		{
#if DEBUG
			provider.OnBeforeReaderCommandExecuted_ForTest();
#endif
			using (var reader = command.ExecuteReader())
			{
				var valueColumnIndex = -1;
				var isValueColumnSet = false;
				var additionalAggregatorColumnIndex = -1;
				var isAdditionalAggregatorColumnSet = false;

				while (reader.Read())
				{
					if (!isValueColumnSet)
					{
						valueColumnIndex = reader.GetOrdinal(BMComponentAcceptabilityBand.ValueColumnName);
						isValueColumnSet = true;
					}

					if (!isAdditionalAggregatorColumnSet)
					{
						try
						{
							additionalAggregatorColumnIndex = reader.GetOrdinal(BMComponentAcceptabilityBand.AdditionalAggregatorColumnName);
							isUsingAdditionalAggregator = true;
						}
						catch (IndexOutOfRangeException)
						{
						}

						isAdditionalAggregatorColumnSet = true;
					}

					var dbResult = reader.GetValue(valueColumnIndex);
					var decimalResult = dbResult != DBNull.Value ? new decimal?(Convert.ToDecimal(dbResult, CultureInfo.InvariantCulture)) : null;
					string aggregatedLabel = null;

					if (isUsingAdditionalAggregator)
					{
						aggregatedLabel = reader.GetString(additionalAggregatorColumnIndex);
					}

					results.Add(new AcceptabilityBandResult(true, decimalResult, aggregatedLabel, acceptabilityBand.PK));
				}
			}
		}
	}
}
