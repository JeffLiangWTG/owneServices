using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business.ServiceTasks
{
	public class AgedScoreInserter
	{
		static string tempTablePrefix => (NoResString)"##tempmentscore"; // this is a temp table prefix

		public QueryResult RunInsertSqlAndLogErrors(MENTAgedScoreQuery query)
		{
			Argument.NotNull(query, nameof(query));

			var result = new QueryResult();
			var tempTableName = FormattableString.Invariant($"{tempTablePrefix}{query.MAQ_Code}"); // this is a temp table name

			result.Messages += FormattableString.Invariant($"Starting Insert of {query.MAQ_Code} into MENTAgedScoreMetric.\r\n"); // Service task logging

			IDataCollectionStrategy queryableCollectionStrategy = query.GetQueryable().CollectionStrategy;

			DropGlobalTempTable(tempTableName);

			using (var readerConnect = Db.NewExtraConnectionToMainDbWithReaderCredentials())
			{
				readerConnect.DefaultCommandTimeOutInSeconds = MENTConstants.LongRunningQueryTimeoutSeconds;

				var systemToGlobalSql = FormattableString.Invariant($@"WITH AgedScoreMetricToInsert AS ({queryableCollectionStrategy.QueryText})
				SELECT '{query.MAQ_Code}' AS code,
				{MENTConstants.AgedScoreValueColumn}, 
				GETUTCDATE() as date, 
				CAST({MENTConstants.ReleaseGroupColumn} AS UNIQUEIDENTIFIER) AS {MENTConstants.ReleaseGroupColumn}, 
				CAST({MENTConstants.ComponentColumn} AS UNIQUEIDENTIFIER) AS {MENTConstants.ComponentColumn},
				{MENTConstants.AttributeValueColumn},
				{MENTConstants.StaffColumn}
				INTO {tempTableName} 
				FROM AgedScoreMetricToInsert");  // this is a direct sql

				var relatedAcceptabilityBandPK = query.MAQ_BAB_RelatedAcceptabilityBand.IsValid ? $"'{query.MAQ_BAB_RelatedAcceptabilityBand}'" : "NULL";
				var globalIntoMENTSql = FormattableString.Invariant($@"INSERT INTO {MENTAgedScoreMetricSchema.Constants.SqlSchemaName}.{MENTAgedScoreMetricSchema.Constants.TableName}
				(
					{MENTAgedScoreMetricSchema.Constants.MAS_MAQ_NKCode},
					{MENTAgedScoreMetricSchema.Constants.MAS_AgedScoreValue},
					{MENTAgedScoreMetricSchema.Constants.MAS_TimeRecordedUtc},
					{MENTAgedScoreMetricSchema.Constants.MAS_GG_ReleaseGroup},
					{MENTAgedScoreMetricSchema.Constants.MAS_FC_Component},
					{MENTAgedScoreMetricSchema.Constants.MAS_AttributeValue},
					{MENTAgedScoreMetricSchema.Constants.MAS_GS_NKStaffCode},
					{MENTAgedScoreMetricSchema.Constants.MAS_BAB_AcceptabilityBand}
				)
				SELECT code, {MENTConstants.AgedScoreValueColumn}, date, {MENTConstants.ReleaseGroupColumn}, {MENTConstants.ComponentColumn}, {MENTConstants.AttributeValueColumn}, {MENTConstants.StaffColumn}, {relatedAcceptabilityBandPK}
				FROM {tempTableName}");  // this is a direct sql

				try
				{
					queryableCollectionStrategy.PerformPreQueryOperation(readerConnect);
					readerConnect.ExecuteScalar(systemToGlobalSql); // Running insert into ##global

					Db.Connection.ExecuteScalar(globalIntoMENTSql); // Running the insert from ##global back into MENTAgedScoreMetric
					result.Result = InserterResult.Success;
				}
				catch (InvalidOperationException ex)
				{
					result.Result = InserterResult.Error;
					result.Messages += ex.Message + System.Environment.NewLine;
				}
				catch (Exception ex) when (DoesSqlExceptionExist(ref ex, out var sqlException))
				{
					var messageBuilder = new ZStringBuilder();

					foreach (var e in ex.FlattenInnerExceptions())
					{
						messageBuilder.Append(e.Message);
					}
					var errorMessage = messageBuilder.ToStringWithNewLineBetweenAppends();

					var note = query.Notes.AddNew();
					note.ST_Description = Res.GetString("049f29d0-7a41-4aab-bed4-8e35324c35b8", "Error With Query");
					note.ST_NoteDataAsText = errorMessage;

					if (ZExceptionExtensions.IsInfrastructureDbError(sqlException))
					{
						result.Messages += FormattableString.Invariant($"The following error occured with the Database/Infrastructure. Please contact your administrator\r\n{errorMessage}\r\n"); // Service task logging
						result.Result = InserterResult.InfrastructureFailure;
					}
					else
					{
						result.Messages += FormattableString.Invariant($"User Defined Statement is Invalid.\r\n{errorMessage}\r\n"); // Service task logging
						result.Result = InserterResult.Error;
					}
				}
				finally
				{
					queryableCollectionStrategy.PerformPostQueryOperation(readerConnect);
					DropGlobalTempTable(tempTableName);
					if (result.Result == InserterResult.Success)
					{
						result.Messages += FormattableString.Invariant($"Finished Insert of {query.MAQ_Code} into MENTAgedScoreMetric.\r\n"); // Service task logging
					}
					else
					{
						var band = query.RelatedAcceptabilityBand;

						if (band != null)
						{
							result.Messages += FormattableString.Invariant($"Error Inserting {query.MAQ_Code} into MENTAgedScoreMetric.\r\nRelated Acceptability Band: {band.BAB_Name}.\r\nThe Acceptability Band Sql being run was: {band.BAB_SqlText}\r\n"); // Service Task Logging
						}
						else
						{
							result.Messages += FormattableString.Invariant($"Error Inserting {query.MAQ_Code} into MENTAgedScoreMetric.\r\nThe Sql being run was: {query.MAQ_SqlText}\r\n"); // Service Task Logging
						}
					}
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Needed in order to test this functionality for the specific case...")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "and needs to be done this way because it's used in the catch statement.")]
		protected virtual bool DoesSqlExceptionExist(ref Exception ex, out System.Data.Common.DbException sqlException)
		{
			sqlException = ex.Find<System.Data.Common.DbException>();

			return sqlException != null;
		}

		static void DropGlobalTempTable(string tempTableName)
		{
			var sql = FormattableString.Invariant($@"DECLARE @sql NVARCHAR(MAX) = N'DROP TABLE IF EXISTS {tempTableName}';
 EXEC sp_executesql @sql;");
			Db.Connection.ExecuteNonQuery(sql); // dropping a temptable
		}
	}
}
