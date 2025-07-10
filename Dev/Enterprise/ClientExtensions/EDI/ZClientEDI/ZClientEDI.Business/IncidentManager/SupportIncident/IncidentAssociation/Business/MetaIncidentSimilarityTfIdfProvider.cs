using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	using ISV = AutoIncidentSimilarityTfIdf.Schema;

	public class MetaIncidentSimilarityTfIdfProvider : IncidentAssociationQuery, IMetaIncidentSimilarityTfIdfProvider
	{
		public MetaIncidentSimilarityTfIdfProvider() : this(Db.Connection) { }
		public MetaIncidentSimilarityTfIdfProvider(DbConnection dbConnection) : base(dbConnection) { }

		public IMetaIncidentSimilarityTfIdf FindByIncidentGuid(int version, Guid incidentMainPkGuid)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@version", version, IncidentSimilarityTfIdfSchema.ISV_Version));
			parameters.Add(ZSqlParameter.New("@incidentGuid", incidentMainPkGuid, IncidentSimilarityTfIdfSchema.ISV_IM_Incident));

			return QueryObjects(
				string.Format(
					CultureInfo.InvariantCulture,
					"SELECT {0},{1},{2},{3},{4},{5},{6} FROM {7} WHERE {8} = @version AND {9} = @incidentGuid",
					AutoIncidentSimilarityTfIdf.Schema.PK,
					AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
					AutoIncidentSimilarityTfIdf.Schema.ISV_TF,
					AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Status,
					AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
					AutoIncidentSimilarityTfIdf.Schema.TableName,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
					AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident),
				parameters)
				.Select(ReadMetaIncidentSimilarityTfIdfFromRow)
				.FirstOrDefault();
		}

		public IEnumerable<IMetaIncidentSimilarityTfIdf> FindByVectorGuids(IEnumerable<Guid> tfIdfPkGuids)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@incidentGuids", tfIdfPkGuids.ToList(), IncidentSimilarityTfIdfSchema.ISV_IM_Incident, true));

			return QueryObjects(
				string.Format(
					CultureInfo.InvariantCulture,
					"SELECT {0},{1},{2},{3},{4},{5},{6} FROM {7} WHERE {8} IN (SELECT Value FROM @incidentGuids)",
					AutoIncidentSimilarityTfIdf.Schema.PK,
					AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
					AutoIncidentSimilarityTfIdf.Schema.ISV_TF,
					AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Status,
					AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
					AutoIncidentSimilarityTfIdf.Schema.TableName,
					AutoIncidentSimilarityTfIdf.Schema.PK),
				parameters)
				.ToList()
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.Select(ReadMetaIncidentSimilarityTfIdfFromRow)
				.OrderBy(msi => msi.IncidentGuid);
		}

		public IEnumerable<IMetaIncidentSimilarityTfIdf> Load(int version)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@version", version, IncidentSimilarityTfIdfSchema.ISV_Version));

			return QueryObjects(
				string.Format(
					CultureInfo.InvariantCulture,
					"SELECT {0},{1},{2},{3},{4},{5},{6} FROM {7} WHERE {8} = @version",
					AutoIncidentSimilarityTfIdf.Schema.PK,
					AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
					AutoIncidentSimilarityTfIdf.Schema.ISV_TF,
					AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Status,
					AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
					AutoIncidentSimilarityTfIdf.Schema.TableName,
					AutoIncidentSimilarityTfIdf.Schema.ISV_Version),
				parameters)
				.ToList()
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.Select(ReadMetaIncidentSimilarityTfIdfFromRow)
				.OrderBy(msi => msi.IncidentGuid);
		}

		public int CountNewOrUpdatedIncidentTfIdfs(int version)
		{
			var parameters = new ZSqlParameterCollection(
				ZSqlParameter.New("@version", version, IncidentSimilarityTfIdfSchema.ISV_Version),
				ZSqlParameter.New("@status", IncidentSimilarityTfIdf.Status.TfIdfComputed, IncidentSimilarityTfIdfSchema.ISV_Status)
			);

			var query = $"SELECT COUNT(*) c FROM {ISV.TableName} WHERE {ISV.ISV_Version} = @version AND {ISV.ISV_Status} = @status";
			foreach (var row in QueryObjects(query, parameters))
			{
				return Convert.ToInt32(row["c"]);
			}

			return 0;
		}

		public int CountProcesedIncidentTfIdfs(int version)
		{
			var parameters = new ZSqlParameterCollection(
				ZSqlParameter.New("@version", version, IncidentSimilarityTfIdfSchema.ISV_Version),
				ZSqlParameter.New("@status", IncidentSimilarityTfIdf.Status.MatrixComputed, IncidentSimilarityTfIdfSchema.ISV_Status)
			);

			var query = $"SELECT COUNT(*) c FROM {ISV.TableName} WHERE {ISV.ISV_Version} = @version AND {ISV.ISV_Status} = @status";
			foreach (var row in QueryObjects(query, parameters))
			{
				return Convert.ToInt32(row["c"]);
			}

			return 0;
		}

		public IEnumerable<IMetaIncidentSimilarityTfIdf> LoadTopNewOrUpdatedIncidentTfIdfs(int version, int topN)
		{
			var parameters = new ZSqlParameterCollection(
				ZSqlParameter.New("@version", version, IncidentSimilarityTfIdfSchema.ISV_Version),
				ZSqlParameter.New("@status", IncidentSimilarityTfIdf.Status.TfIdfComputed, IncidentSimilarityTfIdfSchema.ISV_Status)
			);

			var query =
@$" SELECT      TOP ({topN}) WITH TIES
                {ISV.PK},
                {ISV.ISV_IM_Incident},
                {ISV.ISV_Version},
                {ISV.ISV_TF},
                {ISV.ISV_TFIDF},
				{ISV.ISV_Status},
                {ISV.ISV_IncidentLastModified}
    FROM        {ISV.TableName} WITH (NOLOCK)
    WHERE       {ISV.ISV_Version} = @version
            AND {ISV.ISV_Status} = @status
    ORDER BY    {ISV.ISV_IncidentLastModified} ASC
    ";

			var incidentVectors = QueryObjects(query, parameters)
				.Select(ReadMetaIncidentSimilarityTfIdfFromRow)
				.ToList();

			FurtherSortIncidentVectorsByIncidentPk(incidentVectors);
			return incidentVectors;
		}

		public IEnumerable<List<IMetaIncidentSimilarityTfIdf>> LoadProcessedIncidentTfIdfsInChunks(int version, int chunkSize)
		{
			var versionParameter = ZSqlParameter.New("@version", version, IncidentSimilarityTfIdfSchema.ISV_Version);
			var statusParameter = ZSqlParameter.New("@status", IncidentSimilarityTfIdf.Status.MatrixComputed, IncidentSimilarityTfIdfSchema.ISV_Status);

			var query =
@$" SELECT      TOP ({chunkSize}) WITH TIES
                {ISV.PK},
                {ISV.ISV_IM_Incident},
                {ISV.ISV_Version},
                {ISV.ISV_TF},
                {ISV.ISV_TFIDF},
                {ISV.ISV_Status},
                {ISV.ISV_IncidentLastModified}
    FROM        {ISV.TableName} WITH (NOLOCK)
    WHERE       {ISV.ISV_Version} = @version
            AND {ISV.ISV_Status} = @status
		    AND {ISV.ISV_IncidentLastModified} > @lastModified
    ORDER BY    {ISV.ISV_IncidentLastModified} ASC
    ";

			var lastPK = Guid.Empty;
			var lastModified = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			while (true)
			{
				var parameters = new ZSqlParameterCollection(
					versionParameter,
					statusParameter,
					ZSqlParameter.New("@lastModified", lastModified, IncidentSimilarityTfIdfSchema.ISV_IncidentLastModified)
				);

				var chunk = QueryObjects(query, parameters)
					.Select(ReadMetaIncidentSimilarityTfIdfFromRow)
					.ToList();

				if (chunk.Count == 0)
				{
					break;
				}

				FurtherSortIncidentVectorsByIncidentPk(chunk);
				yield return chunk;
				var lastOfChunk = chunk.Last();
				lastPK = lastOfChunk.PK;
				lastModified = lastOfChunk.IncidentLastModified;
			}
		}

		static void FurtherSortIncidentVectorsByIncidentPk(List<IMetaIncidentSimilarityTfIdf> incidentVectors)
		{
			var compareByIncidentPk = Comparer<IMetaIncidentSimilarityTfIdf>.Create((a, b) => new SqlGuid(a.IncidentGuid).CompareTo(new SqlGuid(b.IncidentGuid)));
			var numVectors = incidentVectors.Count;
			for (var sortWindowBegin = 0; sortWindowBegin < numVectors - 1;)
			{
				var lastModifiedTime = incidentVectors[sortWindowBegin].IncidentLastModified;
				var sortWindowEnd = incidentVectors.FindIndex(sortWindowBegin + 1, x => x.IncidentLastModified > lastModifiedTime);
				sortWindowEnd = sortWindowEnd != -1 ? sortWindowEnd : numVectors;

				var sortWindowSize = sortWindowEnd - sortWindowBegin;
				if (sortWindowSize > 1)
				{
					incidentVectors.Sort(sortWindowBegin, sortWindowSize, compareByIncidentPk);
				}

				sortWindowBegin = sortWindowEnd;
			}
		}

		static IMetaIncidentSimilarityTfIdf ReadMetaIncidentSimilarityTfIdfFromRow(Dictionary<string, object> dbo) =>
			new MetaIncidentSimilarityTfIdf()
			{
				PK = (Guid)dbo[ISV.PK],
				IncidentGuid = (Guid)dbo[ISV.ISV_IM_Incident],
				Version = (int)dbo[ISV.ISV_Version],
				TermFrequency = dbo[ISV.ISV_TF] == DBNull.Value ? null : SimilarIncidentRepository.BytesToDoubles(Compressor.Uncompress((byte[])dbo[ISV.ISV_TF])).ToList(),
				TFIDF = dbo[ISV.ISV_TFIDF] == DBNull.Value ? null : SimilarIncidentRepository.BytesToDoubles(Compressor.Uncompress((byte[])dbo[ISV.ISV_TFIDF])).ToList(),
				Status = (string)dbo[ISV.ISV_Status],
				IncidentLastModified = (DateTime)dbo[ISV.ISV_IncidentLastModified]
			};

		public void Save(IEnumerable<IMetaIncidentSimilarityTfIdf> metaISVs, int batchSize = 1000)
		{
			var sqlValues = metaISVs
				.Select(metaISV => string.Format(
					CultureInfo.InvariantCulture,
					"('{0}','{1}',{2},{3},{4},'{5}','{6}')",
					(metaISV.PK = Guid.NewGuid()).ToString(),
					metaISV.IncidentGuid.ToString(),
					metaISV.Version,
					metaISV.TermFrequencyHex ?? "NULL",
					metaISV.TFIDFHex ?? "NULL",
					metaISV.Status,
					metaISV.IncidentLastModified.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)))
				.ToList();

			using (var transactionManager = dbConnection.BeginTransactionWithManager())
			{
				while (sqlValues.Count > 0)
				{
					var sqlValuesToInsert = sqlValues.Take(batchSize).ToList();
					sqlValues = sqlValues.Skip(batchSize).ToList();

					var sqlInsert = string.Format(
						CultureInfo.InvariantCulture,
						"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES {8}",
						AutoIncidentSimilarityTfIdf.Schema.TableName,
						AutoIncidentSimilarityTfIdf.Schema.PK,
						AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident,
						AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
						AutoIncidentSimilarityTfIdf.Schema.ISV_TF,
						AutoIncidentSimilarityTfIdf.Schema.ISV_TFIDF,
						AutoIncidentSimilarityTfIdf.Schema.ISV_Status,
						AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified,
						string.Join(",", sqlValuesToInsert));
					dbConnection.Command(sqlInsert).ExecuteNonQuery();
				}

				transactionManager.CommitTransaction();
			}
		}
	}
}
