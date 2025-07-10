using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class SimilarityMatrixBootstrapperQueryExecutor : IncidentAssociationQuery, ISimilarityMatrixBootstrapperQueryExecutor
	{
		public SimilarityMatrixBootstrapperQueryExecutor() : base(Db.Connection) { }
		public SimilarityMatrixBootstrapperQueryExecutor(DbConnection dbConnection) : base(dbConnection) { }

		public void Commit(IEnumerable<SimilarityMatrixDataTransferObject> similarityMatrixData, ZGuid tfidfGuid, int batchSize = 1000)
		{
			using (var transactionManager = dbConnection.BeginTransactionWithManager())
			{
				foreach (var batch in similarityMatrixData.Chunk(batchSize))
				{
					var sqlValuesToInsert = batch.Select(
						x => string.Format(
							CultureInfo.InvariantCulture,
							"('{0}','{1}','{2}',{3},{4})",
							x.PK.ToString(),
							x.Incident1.ToString(),
							x.Incident2.ToString(),
							x.Version,
							x.Similarity)
						).ToList();
					AddIncidentSimilarityMatrix(sqlValuesToInsert);
				}
				CreateStmAlogs(tfidfGuid.ToGuid(), ZDateTime.UtcNow);
				UpdateStatus(tfidfGuid.ToGuid());

				transactionManager.CommitTransaction();
			}
		}

		public int DeleteExistingRows(Guid incidentGuid, int newVersion)
		{
			using (var transactionManager = dbConnection.BeginTransactionWithManager())
			{
				var affectedRowsCount = NonQuery(
					string.Format(
						CultureInfo.InvariantCulture,
						"DELETE FROM {0} WHERE {1} = @version AND ({2} = @guid1 OR {3} = @guid2)",
						AutoIncidentSimilarityMatrix.Schema.TableName,
						AutoIncidentSimilarityMatrix.Schema.ISM_Version,
						AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
						AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2
					),
					new ZSqlParameterCollection
					{
						ZSqlParameter.New("@version", newVersion, IncidentSimilarityMatrixSchema.ISM_Version),
						ZSqlParameter.New("@guid1", incidentGuid, IncidentSimilarityMatrixSchema.ISM_IM_Incident1),
						ZSqlParameter.New("@guid2", incidentGuid, IncidentSimilarityMatrixSchema.ISM_IM_Incident2)
					});

				affectedRowsCount += NonQuery(
					string.Format(
						CultureInfo.InvariantCulture,
						"DELETE FROM {0} WHERE {1} = @version AND {2} = @guid",
						AutoIncidentSimilarityTfIdf.Schema.TableName,
						AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
						AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident),
					new ZSqlParameterCollection
					{
						ZSqlParameter.New("@version", newVersion, IncidentSimilarityTfIdfSchema.ISV_Version),
						ZSqlParameter.New("@guid", incidentGuid, IncidentSimilarityTfIdfSchema.ISV_IM_Incident)
					});
				transactionManager.CommitTransaction();
				return affectedRowsCount;
			}
		}

		void AddIncidentSimilarityMatrix(IEnumerable<string> sqlValuesToInsert)
		{
			var sqlInsert = string.Format(
				CultureInfo.InvariantCulture,
				"INSERT INTO [{0}] ({1},{2},{3},{4},{5}) VALUES {6} OPTION(RECOMPILE)",
				AutoIncidentSimilarityMatrix.Schema.TableName,
				AutoIncidentSimilarityMatrix.Schema.PK,
				AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
				AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2,
				AutoIncidentSimilarityMatrix.Schema.ISM_Version,
				AutoIncidentSimilarityMatrix.Schema.ISM_Similarity,
				string.Join(",", sqlValuesToInsert));
			NonQuery(sqlInsert);
		}

		void CreateStmAlogs(Guid tfidfGuid, ZDateTime time)
		{
			var sqlInsert = string.Format(
				CultureInfo.InvariantCulture,
				"INSERT INTO {0} ({1},{2},{3},{4},{5},{6}) " +
				"VALUES ('{7}','{8}','{9}','{10}','{11}','{12}')",
				AutoStmALog.Schema.TableName,
				AutoStmALog.Schema.SL_Table,
				AutoStmALog.Schema.SL_Parent,
				AutoStmALog.Schema.SL_Reference,
				AutoStmALog.Schema.SL_PostedTimeUtc,
				AutoStmALog.Schema.SL_EventTime,
				AutoStmALog.Schema.SL_SE_NKEvent,
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				tfidfGuid.ToString(),
				"",
				time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				"ADD"
			);
			NonQuery(sqlInsert);
		}

		void UpdateStatus(Guid tfidfGuid)
		{
			var sql = string.Format(
				CultureInfo.InvariantCulture,
				"UPDATE {0} SET {1} = '{2}' WHERE {3} = @guid",
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.ISV_Status,
				IncidentSimilarityTfIdf.Status.MatrixComputed,
				AutoIncidentSimilarityTfIdf.Schema.PK);
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@guid", tfidfGuid, IncidentSimilarityTfIdfSchema.PK)
			};
			NonQuery(sql, parameters);
		}
	}

	public class SimilarityMatrixDataTransferObject
	{
		public readonly ZGuid PK;
		public readonly ZGuid Incident1;
		public readonly ZGuid Incident2;
		public readonly int Version;
		public readonly double Similarity;

		public SimilarityMatrixDataTransferObject(ZGuid pk, ZGuid incident1, ZGuid incident2, int version, double similarity)
		{
			PK = pk;
			Incident1 = incident1;
			Incident2 = incident2;
			Version = version;
			Similarity = similarity;
		}
	}
}
