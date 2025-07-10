using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	abstract public class OffloadingTagRuleRunStrategy : TagRuleRunStrategyBase
	{
		internal OffloadingTagRuleRunStrategy(IConnectionProvider connectionProvider, ILogger logger)
			: base(connectionProvider, logger)
		{
		}

		protected long Process(TagRule rule)
		{
			var rowsProcessed = 0L;
			using (var connectionWrapper = ConnectionProvider.GetNewConnectionWrapper())
			using (var tempTableWorker = GetWorkflowTempTableWorker(rule, connectionWrapper))
			{
				string tempTableNameWhichContainsWorkflowsToTag = null;
				try
				{
					tempTableNameWhichContainsWorkflowsToTag = tempTableWorker.TempTableName;
					var maxDopNumber = GetMaxDopNumber(rule);

					var performanceTimestampStart = ZDateTime.UtcNow;
					var numberOfWorkflowsFound = tempTableWorker.InsertWorkflowsToTempTable(GetWorkflowsToInsertQueryTextAndParameters(rule, tempTableNameWhichContainsWorkflowsToTag, maxDopNumber)); // Most expensive part of this operation occurs if possible on secondary connection
					var firstRun = GetFirstRunTime(rule, performanceTimestampStart);

					if (numberOfWorkflowsFound > 0)
					{
						using (CreateWorkflowsProcessedTempTable(PrimaryServerWorkflowBatchTempTableName))
						{
							foreach (var batch in GenerateBatches(rule, numberOfWorkflowsFound, tempTableNameWhichContainsWorkflowsToTag, connectionWrapper))
							{
								rowsProcessed += PerformTaggingOnMainServer(rule, batch);
							}
						}
					}

					RunVerification(rule, tempTableWorker, tempTableNameWhichContainsWorkflowsToTag, maxDopNumber, firstRun);
				}
				catch (NoResultQueryException)
				{
					Logger.Error(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} was not processed as the query it was about to generate was not going to produce any results.", rule.TGR_Name)); // This is a log message
				}
				catch (SqlException ex)
				{
					if (!ThrowDroppedDbExceptionIfMessageMatches(ex, PrimaryServerWorkflowBatchTempTableName, tempTableNameWhichContainsWorkflowsToTag))
					{
						throw;
					}
				}
			}
			return rowsProcessed;
		}

		void RunVerification(TagRule rule, ITempTableWorker tempTableWorker, string tempTableNameWhichContainsWorkflowsToTag, int maxDopNumber, double firstRun)
		{
			if (rule.ShouldRunPerformanceVerification)
			{
				var performanceTimestampStart = ZDateTime.UtcNow;
				bool bitToggled = false;

				tempTableWorker.InsertWorkflowsToTempTable(GetWorkflowsToInsertQueryTextAndParameters(rule, tempTableNameWhichContainsWorkflowsToTag, 1 - maxDopNumber)); // Most expensive part of this operation occurs if possible on secondary connection
				var secondRun = (ZDateTime.UtcNow - performanceTimestampStart).TotalMilliseconds;
				if (secondRun * (double)BMSRegistry.Instance.ThreadedQuerySlownessThresholdFactor.Value < firstRun)
				{
					ToggleSingleThreadedBit(rule);
					bitToggled = true;
				}
				if (rule.TGR_ActionType != TagRuleActionTypeList.Codes.AddAndRemoveTag)
				{
					rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
				}
				WriteVerificationLog(rule, bitToggled, 1 - maxDopNumber);
			}
		}

		protected abstract int GetMaxDopNumber(TagRule rule);

		protected abstract ITempTableWorker GetWorkflowTempTableWorker(TagRule rule, IDbConnectionForReportingWrapper secondaryConnectionWrapper);

		protected abstract IDisposable CreateWorkflowsProcessedTempTable(string nextWorkflowBatchTempTableName);

		IEnumerable<ITagDto[]> GenerateBatches(TagRule rule, int countOfWorkflowsFound, string tempTableNameWhichContainsWorkflowsToTag, IDbConnectionForReportingWrapper connectionWrapper)
		{
			var batchesRequired = (countOfWorkflowsFound + WorkflowsToProcessPerBatch - 1) / WorkflowsToProcessPerBatch;

			for (var i = 0; i < batchesRequired; i++)
			{
				var whereClause = GenerateWhereClause(i, WorkflowsToProcessPerBatch);

				yield return RowsToProcess(WorkflowsToProcessPerBatch, whereClause, tempTableNameWhichContainsWorkflowsToTag, connectionWrapper);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ITagDto[] RowsToProcess(int workflowsToProcessPerBatch, QueryTextAndParameters whereClause, string tempTableNameWhichContainsWorkflowsToTag, IDbConnectionForReportingWrapper connectionWrapper)
		{
			var queryTextAndParameters = RowProcessSql(tempTableNameWhichContainsWorkflowsToTag, whereClause);
			var command = connectionWrapper.Connection.Command(queryTextAndParameters.QueryText);
			command.AddParameters(queryTextAndParameters.Parameters);

			var results = new ITagDto[workflowsToProcessPerBatch];

			using (var result = command.ExecuteReader())
			{
				var currentRowToRead = 0;
				while (result.Read())
				{
					ITagDto row;
					if (TryReadRow(result, out row))
					{
						results[currentRowToRead] = row;
					}

					currentRowToRead++;
				}
			}

			return results;
		}

		protected abstract bool TryReadRow(IDataReader result, out ITagDto row);

		protected abstract QueryTextAndParameters RowProcessSql(string tempTableNameWhichContainsWorkflowsToTag, QueryTextAndParameters whereClause);

		QueryTextAndParameters GenerateWhereClause(int batchNumber, int workflowsToProcessThisBatch)
		{
			var batchLowNumber = 1 + (batchNumber * workflowsToProcessThisBatch);
			var batchHighNumber = 1 + ((batchNumber + 1) * workflowsToProcessThisBatch);

			var sqlParameterCollection = new ZSqlParameterCollection();
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.BatchLowNumber, batchLowNumber, Schema.GenericIntSchemaColumn);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.BatchHighNumber, batchHighNumber, Schema.GenericIntSchemaColumn);

			return new QueryTextAndParameters(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} >= {1} AND {0} < {2}",
				RowNumber,
				TagRuleRunStrategyBaseConstants.ParameterNames.BatchLowNumber,
				TagRuleRunStrategyBaseConstants.ParameterNames.BatchHighNumber),
				sqlParameterCollection);
		}

		protected abstract QueryTextAndParameters GetResultsQueryTextAndParameters(TagRule rule, string batchTempTable);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual int PerformTaggingOnMainServer(TagRule rule, ITagDto[] headersToTag)
		{
			var primaryTempTableName = PrimaryServerWorkflowBatchTempTableName;
			PopulatePrimaryServerTempTable(headersToTag, primaryTempTableName);

			var textAndParameters = GetResultsQueryTextAndParameters(rule, primaryTempTableName);
			var command = Db.Connection.Command(textAndParameters.QueryText); // Running Sql is quicker than bizos

			command.AddParameters(textAndParameters.Parameters);

			var rowsProcessed = (int)command.ExecuteScalar(); // Running Sql is quicker than bizos

			return rowsProcessed;
		}

		IEnumerable<ITagDto> Skip<ITagDto>(ITagDto[] source, int count)
		{
			for (int i = count; i < source.Length; i++)
			{
				yield return source[i];
			}
		}

		void PopulatePrimaryServerTempTable(ITagDto[] headersToTag, string tempTableName)
		{
			var originalInsertBatch = headersToTag.WhereNotNull().ToArray();

			var maxNumberPerBatch = BMConstants.SqlServerMaxValuesClausesCount;

			var maxBatches = (originalInsertBatch.Length + maxNumberPerBatch - 1) / maxNumberPerBatch;

			for (int i = 0; i < maxBatches; i++)
			{
				var valuesToInsert = Skip(originalInsertBatch, i * maxNumberPerBatch).Take(maxNumberPerBatch).ToArray();
				var insertQuery = CreateInsertQueryForPrimaryServer(valuesToInsert, tempTableName);
				Db.Connection.ExecuteNonQuery(insertQuery); // Inserting Rows directly into the temp table
			}
		}

		protected abstract string CreateInsertQueryForPrimaryServer(ITagDto[] valuesToInsert, string tempTableName);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "These are temp table names")]
		protected const string PrimaryServerWorkflowBatchTempTableName = "#ProcessHeaderBatch";

		protected abstract QueryTextAndParameters GetWorkflowsToInsertQueryTextAndParameters(TagRule rule, string tempTableName, int maxDopNumber);

		#region WorkflowsToManipulate

		protected abstract class WorkflowTempTableWorker : ITempTableWorker
		{
			readonly IDbConnectionForReportingWrapper connectionWrapper;
			readonly TagRule rule;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
			internal WorkflowTempTableWorker(IDbConnectionForReportingWrapper connectionWrapper, TagRule rule)
			{
				this.connectionWrapper = connectionWrapper;
				this.rule = rule;

				CreateTempTable();
			}

			internal TagRule TagRule
			{
				get { return rule; }
			}

			string CreateTempTable()
			{
				var tempTableCreateSql = CreateTempTableCore();

				connectionWrapper.Connection.ExecuteNonQuery(tempTableCreateSql); // Working with temp table

				return ((ITempTableWorker)this).TempTableName;
			}

			protected abstract string CreateTempTableCore();

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			int ITempTableWorker.InsertWorkflowsToTempTable(QueryTextAndParameters queryTextAndParameters)
			{
				var command = connectionWrapper.Connection.Command(queryTextAndParameters.QueryText); // listen here you little shit
				command.AddParameters(queryTextAndParameters.Parameters);

				return (int)command.ExecuteScalar(); // Running Sql is quicker than bizo's
			}

			string ITempTableWorker.TempTableName
			{
				get { return TempTableNameCore; }
			}

			protected abstract string TempTableNameCore { get; }

			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					DropTempTable(((ITempTableWorker)this).TempTableName, connectionWrapper.Connection);
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		public interface ITempTableWorker : IDisposable
		{
			int InsertWorkflowsToTempTable(QueryTextAndParameters queryTextAndParameters);
			string TempTableName { get; }
		}

		#endregion

		#region HeaderDTO

		protected abstract class HeaderToTag : Tuple<ZGuid, ZGuid>, ITagDto
		{
			protected HeaderToTag(ZGuid headerPk, ZGuid workflowParentID)
				: base(headerPk, workflowParentID)
			{ }

			public ZGuid HeaderPK
			{
				get { return Item1; }
			}

			public ZGuid WorkflowParentID
			{
				get { return Item2; }
			}
		}

		public interface ITagDto
		{
			ZGuid HeaderPK { get; }
			ZGuid WorkflowParentID { get; }
		}

		#endregion

		#region CommonSql

		protected const string RowNumber = "RowNumber"; // This is a temp table column name

		#endregion

		protected int WorkflowsToProcessPerBatch
		{
			get { return BMSRegistry.Instance.TagRuleRunnerPrimarySecondaryServerTransferBatchSize.Value; }
		}
	}
}
