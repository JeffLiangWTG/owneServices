using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.IO;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class TfIdfBootstrapperQueryExecutor : IncidentAssociationQuery, ITfIdfBootstrapperQueryExecutor
	{
		public TfIdfBootstrapperQueryExecutor() : this(Db.Connection) { }
		public TfIdfBootstrapperQueryExecutor(DbConnection dbConnection) : base(dbConnection) { }

		public void UpdateTfidfPairs(IEnumerable<TfidfPairDataTransferObject> updatePairs, int batchSize)
		{
			using (var transactionManager = dbConnection.BeginTransactionWithManager())
			{
				while (updatePairs.Any())
				{
					var batch = updatePairs.Take(batchSize).ToList();
					updatePairs = updatePairs.Skip(batchSize).ToList();

					UpdateTFIDF(batch
						.AsParallel()
						.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
						.Select(tup => $"('{tup.PK}',{tup.TFIDF})")
						.ToList());
				}
				transactionManager.CommitTransaction();
			}
		}

		void UpdateTFIDF(List<string> updatePairBatch)
		{
			var virtualTable = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0},{1} FROM (VALUES {2}) AS t({3},{4})",
				AutoIncidentSimilarityTfIdf.Schema.PK,
				AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF,
				string.Join(",", updatePairBatch),
				AutoIncidentSimilarityTfIdf.Schema.PK,
				AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF
			);

			var query = string.Format(
				CultureInfo.InvariantCulture,
				"UPDATE {0} SET {1}.{2} = t.{3}, {4}.{5} = '{6}' FROM ({7}) AS t WHERE {8}.{9} = t.{10}",
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF,
				AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF,
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.ISV_Status,
				IncidentSimilarityTfIdf.Status.TfIdfComputed,
				virtualTable,
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.PK,
				AutoIncidentSimilarityTfIdf.Schema.PK
			);

			NonQuery(query);
		}
	}

	public class TfidfPairDataTransferObject
	{
		public readonly Guid PK;
		public readonly string TFIDF;

		public TfidfPairDataTransferObject(Guid pk, IEnumerable<double> tfidf)
		{
			PK = pk;
			TFIDF = string.Format(
				CultureInfo.InvariantCulture,
				"0x{0}",
				BitConverter.ToString(
					Compressor.Compress(
						SimilarIncidentRepository.DoublesToBytes(
							tfidf.ToArray())
						.ToArray()))
				.Replace("-", ""));
		}

		public TfidfPairDataTransferObject(Guid pk, string tfidf)
		{
			PK = pk;
			TFIDF = tfidf;
		}
	}
}
