using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class IncidentAssociationQuery : IIncidentAssociationQuery
	{
		protected readonly DbConnection dbConnection;

		public IncidentAssociationQuery() : this(Db.Connection) { }

		public IncidentAssociationQuery(DbConnection dbConnection)
		{
			this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
		}

		public static int MaxDelete { get; } = 10_000;
		public static int MaxInValuesClause { get; } = 1_000;

		public IEnumerable<Dictionary<string, object>> QueryObjects(string query, ZSqlParameterCollection parameters = null)
		{
			using var cmd = dbConnection.Command(query);

			if (parameters != null)
			{
				cmd.AddParameters(parameters);
			}

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return Enumerable.Range(0, reader.FieldCount)
						.ToDictionary(i => reader.GetName(i), i => reader.GetValue(i));
				}
			}
		}

		public int NonQuery(string query, ZSqlParameterCollection parameters = null)
		{
			using var cmd = dbConnection.Command(query);

			if (parameters != null)
			{
				cmd.AddParameters(parameters);
			}

			return cmd.ExecuteNonQuery();
		}

		public int GetLatestVersion()
		{
			var latestVersionFieldName = "IncidentSimilarity_LatestVersion";
			var query = string.Format(
				CultureInfo.InvariantCulture,
				@"SELECT {0},{1} FROM {2} Where {3} = '{4}'",
				AutoStmData.Schema.SD_Name,
				AutoStmData.Schema.SD_BinaryValue,
				AutoStmData.Schema.TableName,
				AutoStmData.Schema.SD_Name,
				latestVersionFieldName);
			var qo = QueryObjects(query);
			return BitConverter.ToInt32((byte[])qo.First()[AutoStmData.Schema.SD_BinaryValue], 0);
		}

		public IEnumerable<Guid> GetIncidentGuidsBetween(DateTime fromDate, DateTime toDate)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@fromDate", fromDate, IncidentMainSchema.IM_SystemLastEditTimeUtc),
				ZSqlParameter.New("@toDate", toDate, IncidentMainSchema.IM_SystemLastEditTimeUtc)
			};

			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0} FROM {1} WHERE {2} = 'INC' AND {3} BETWEEN @fromDate AND @toDate",
				AutoIncidentMain.Schema.PK,
				AutoIncidentMain.Schema.TableName,
				AutoIncidentMain.Schema.IM_IncidentType,
				AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc);

			return QueryObjects(sql, parameters)
				.Select(o => (Guid)o[AutoIncidentMain.Schema.PK])
				.OrderBy(guid => guid)
				.ToList();
		}

		public IEnumerable<Guid> GetNewAndUpdatedIncidentGuids(int version, DateTime fromDate, DateTime toDate)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@version", version, IncidentSimilarityTfIdfSchema.ISV_Version),
				ZSqlParameter.New("@fromDate", fromDate, IncidentMainSchema.IM_SystemLastEditTimeUtc),
				ZSqlParameter.New("@toDate", toDate, IncidentMainSchema.IM_SystemLastEditTimeUtc)
			};

			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0} FROM {1} LEFT JOIN {2} ON ({3} = {4} AND {5} = @version) "
				+ "LEFT JOIN {6} ON {7} = {8} "
				+ "WHERE ({9} IS NULL OR {10} > {11}) AND {12} BETWEEN @fromDate AND @toDate AND {13} = 'INC'"
				+ " AND ({14} IS NULL OR {15} > {16})",
				AutoIncidentMain.Schema.PK,
				AutoIncidentMain.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident,
				AutoIncidentMain.Schema.PK,
				AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
				AutoIncidentSimilarityExclusion.Schema.TableName,
				AutoIncidentMain.Schema.PK,
				AutoIncidentSimilarityExclusion.Schema.ISE_IM,
				AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
				AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc,
				AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
				AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc,
				AutoIncidentMain.Schema.IM_IncidentType,
				AutoIncidentSimilarityExclusion.Schema.ISE_IM,
				AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc,
				AutoIncidentSimilarityExclusion.Schema.ISE_LastUpdatedUtc
			);

			return QueryObjects(sql, parameters)
				.Select(dbo => ((Guid)dbo[AutoIncidentMain.Schema.PK]))
				.ToList();
		}

		public void UpdateTfIdfTimestamps(int version, IDictionary<Guid, DateTime> timestamps)
		{
			using (var transactionManager = dbConnection.BeginTransactionWithManager())
			{
				foreach (var kvp in timestamps)
				{
					var keyParam = "@tfidfKey";
					var timestampParam = "@tfidfTimestamp";
					var versionParam = "@version";

					var parameters = new ZSqlParameterCollection
					{
						ZSqlParameter.New(keyParam, kvp.Key, IncidentSimilarityTfIdfSchema.ISV_IM_Incident),
						ZSqlParameter.New(timestampParam, kvp.Value, IncidentSimilarityTfIdfSchema.ISV_IncidentLastModified),
						ZSqlParameter.New(versionParam, version, IncidentSimilarityTfIdfSchema.ISV_Version),
					};

					var sql = string.Format(
						CultureInfo.InvariantCulture,
						"UPDATE {0} SET {1} = {2} WHERE {3} = {4} AND {5} = {6}",
						AutoIncidentSimilarityTfIdf.Schema.TableName,
						AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
						timestampParam,
						AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident,
						keyParam,
						AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
						versionParam);

					NonQuery(sql, parameters);
				}

				transactionManager.CommitTransaction();
			}
		}

		public IEnumerable<Guid> DeleteOldIncidentsFromIncidentSimilarityTfIdf(DateTime fromDate)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@fromDate", fromDate, IncidentSimilarityTfIdfSchema.ISV_IncidentLastModified),
			};

			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"DELETE TOP ({0}) "
				+ "FROM {1} "
				+ "OUTPUT DELETED.{2} "
				+ "WHERE {3} < @fromDate",
				MaxDelete, // 0
				AutoIncidentSimilarityTfIdf.Schema.TableName, // 1
				AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident, // 2
				AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified); // 3

			return QueryObjects(sql, parameters)
				.Select(dbo => (Guid)dbo[AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident])
				.ToList();
		}

		public void DeleteIncidentsFromIncidentSimilarityMatix(IEnumerable<Guid> incidents)
		{
			foreach (var incidentsChunk in incidents.Chunk(MaxInValuesClause))
			{
				var sql = string.Format(
					CultureInfo.InvariantCulture,
					"WITH IncidentsCte AS ( "
						+ "SELECT * FROM (VALUES {0}) AS Inc(PK)"
					+ ") "
					+ "DELETE "
					+ "FROM {1} "
					+ "WHERE {2} IN (SELECT * FROM IncidentsCte) "
					+ "OR {3} IN (SELECT * FROM IncidentsCte)",
					string.Join(",", incidentsChunk.Select(inc => $"('{inc}')")), // 0
					AutoIncidentSimilarityMatrix.Schema.TableName, // 1
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1, // 2
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2); // 3

				NonQuery(sql);
			}
		}

		public void DeleteOldIncidentsFromIncidentSimilarityExclusion(DateTime fromDate)
		{
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@fromDate", fromDate, IncidentSimilarityExclusionSchema.ISE_LastUpdatedUtc),
			};

			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"DELETE TOP ({0}) "
				+ "FROM {1} "
				+ "WHERE {2} < @fromDate",
				MaxDelete, // 0
				AutoIncidentSimilarityExclusion.Schema.TableName, // 1
				AutoIncidentSimilarityExclusion.Schema.ISE_LastUpdatedUtc); // 2

			NonQuery(sql, parameters);
		}
	}
}
