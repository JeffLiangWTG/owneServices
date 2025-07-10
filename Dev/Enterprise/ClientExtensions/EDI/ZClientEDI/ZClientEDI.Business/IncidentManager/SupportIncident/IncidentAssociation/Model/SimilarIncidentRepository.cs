using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class SimilarIncidentRepository : ISimilarIncidentRepository
	{
		public const string LatestVersionFieldName = "IncidentSimilarity_LatestVersion";
		public const string IdfFieldName = "IncidentSimilarity_IDF_";

		public int RepositoryVersion { get; set; }

		IEnumerable<double> idfVector;
		readonly IDictionary<string, int> tokenPairs;

		public BusinessObjectFactory Factory { get; }
		readonly IIncidentAssociationQuery incidentAssociationQuery;

		public SimilarIncidentRepository() : this(new IncidentAssociationQuery(Db.Connection)) { }
		public SimilarIncidentRepository(IIncidentAssociationQuery incidentAssociationQuery) : this(incidentAssociationQuery, new BusinessObjectFactory()) { }

		public SimilarIncidentRepository(IIncidentAssociationQuery incidentAssociationQuery, BusinessObjectFactory factory)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
			RepositoryVersion = GetLatestVersion();
			this.incidentAssociationQuery = incidentAssociationQuery;
		}

		public SimilarIncidentRepository(int version) : this(new IncidentAssociationQuery(Db.Connection), version) { }
		public SimilarIncidentRepository(IIncidentAssociationQuery incidentAssociationQuery, int version) : this(incidentAssociationQuery, version, new BusinessObjectFactory()) { }
		public SimilarIncidentRepository(IIncidentAssociationQuery incidentAssociationQuery, int version, BusinessObjectFactory factory)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
			RepositoryVersion = version;
			this.incidentAssociationQuery = incidentAssociationQuery;
		}

		public static int GetLatestVersion(BusinessObjectFactory factory = null)
		{
			var query = new ZQuery(StmDataSchema.SD_Name, LatestVersionFieldName);
			var stmRow = (factory ?? new BusinessObjectFactory()).Load<StmData>(query).FirstOrDefault();

			if (stmRow == null)
			{
				return -1;
			}

			return BitConverter.ToInt32(stmRow.SD_BinaryValue, 0);
		}

		public int GetLatestVersion()
		{
			return GetLatestVersion(Factory);
		}
		public static ZGuid SetLatestVersion(int latestVersion, BusinessObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			var query = new ZQuery(StmDataSchema.SD_Name, LatestVersionFieldName);
			var stmRow = factory.Load<StmData>(query).FirstOrDefault();

			if (stmRow == null)
			{
				stmRow = factory.New<StmData>();
				stmRow.SD_Name = LatestVersionFieldName;
			}

			stmRow.SD_BinaryValue = BitConverter.GetBytes(latestVersion);
			factory.Save();

			return stmRow.PK;
		}

		public IEnumerable<ZGuid> GetRelatedIncidentGuidsFromMatrix(ZGuid sourceIncident)
		{
			var paramCollection = new ZSqlParameterCollection();
			var query = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0}, {1} FROM dbo.{2} WHERE {3} = @ver AND ({4} = @guid1 OR {5} = @guid2)",
				AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
				AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2,
				AutoIncidentSimilarityMatrix.Schema.TableName,
				AutoIncidentSimilarityMatrix.Schema.ISM_Version,
				AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
				AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2);
			paramCollection.Add(ZSqlParameter.New("@ver", RepositoryVersion, IncidentSimilarityMatrixSchema.ISM_Version));
			paramCollection.Add(ZSqlParameter.New("@guid1", sourceIncident, IncidentSimilarityMatrixSchema.ISM_IM_Incident1));
			paramCollection.Add(ZSqlParameter.New("@guid2", sourceIncident, IncidentSimilarityMatrixSchema.ISM_IM_Incident2));

			return incidentAssociationQuery.QueryObjects(query, paramCollection)
				.Select(dict => (Guid)dict[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1] == sourceIncident.ToGuid() ? (Guid)dict[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2] : (Guid)dict[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1])
				.Select(guid => new ZGuid(guid));
		}

		public IEnumerable<ZGuid> GetProcessedIncidentGuidsByDate(Guid sourceIncidentPK, DateTime? fromDate, DateTime? toDate)
		{
			var relatedIncidents = GetRelatedIncidentGuidsFromMatrix(sourceIncidentPK).ToList();
			if (!relatedIncidents.Any())
			{
				return Enumerable.Empty<ZGuid>();
			}

			var paramCollection = new ZSqlParameterCollection();
			var query = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0} FROM dbo.{1} WHERE {2} = @ver AND {3} = '{4}' AND {5} IN (SELECT Value FROM @relevantIncidents) ",
				AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident,
				AutoIncidentSimilarityTfIdf.Schema.TableName,
				AutoIncidentSimilarityTfIdf.Schema.ISV_Version,
				AutoIncidentSimilarityTfIdf.Schema.ISV_Status,
				IncidentSimilarityTfIdf.Status.MatrixComputed,
				AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident);
			paramCollection.Add(ZSqlParameter.New("@ver", RepositoryVersion, IncidentSimilarityTfIdfSchema.ISV_Version));
			paramCollection.Add(ZSqlParameter.New("@relevantIncidents", relatedIncidents, IncidentMainSchema.PK, true));

			if (fromDate.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} >= @fromDate ",
					AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified);
				paramCollection.Add(ZSqlParameter.New("@fromDate", fromDate.Value, IncidentSimilarityTfIdfSchema.ISV_IncidentLastModified));
			}

			if (toDate.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} <= @toDate ",
					AutoIncidentSimilarityTfIdf.Schema.ISV_IncidentLastModified);
				paramCollection.Add(ZSqlParameter.New("@toDate", toDate.Value, IncidentSimilarityTfIdfSchema.ISV_IncidentLastModified));
			}

			return incidentAssociationQuery
				.QueryObjects(query, paramCollection)
				.Select(dict => new ZGuid((Guid)dict[AutoIncidentSimilarityTfIdf.Schema.ISV_IM_Incident]));
		}

		public IEnumerable<ZGuid> GetProcessedIncidentGuidsByOptions(SimilarIncidentSearchOptions options)
		{
			var relatedIncidents = GetRelatedIncidentGuidsFromMatrix(options.SourceIncidentPK).ToList();
			if (!relatedIncidents.Any())
			{
				return Enumerable.Empty<ZGuid>();
			}

			var paramCollection = new ZSqlParameterCollection();
			var query = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT {0} FROM dbo.{1} WHERE {2} = 'INC' ",
				AutoIncidentMain.Schema.PK,
				AutoIncidentMain.Schema.TableName,
				AutoIncidentMain.Schema.IM_IncidentType);

			if (options.FromTime.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} >= @fromTime ",
					AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc);
				paramCollection.Add(ZSqlParameter.New("@fromTime", options.FromTime.Value, IncidentMainSchema.IM_SystemLastEditTimeUtc));
			}

			if (options.IncidentStatus == IncidentStatus.Open)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} IS NULL ",
					AutoIncidentMain.Schema.IM_CloseTimeUtc);
			}
			else if (options.IncidentStatus == IncidentStatus.Closed)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} IS NOT NULL ",
					AutoIncidentMain.Schema.IM_CloseTimeUtc);
			}

			if (options.ToTime.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} <= @toTime ",
					AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc);
				paramCollection.Add(ZSqlParameter.New("@toTime", options.ToTime.Value, IncidentMainSchema.IM_SystemLastEditTimeUtc));
			}

			if (options.CompanyGuid.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} = @clientId ",
					AutoIncidentMain.Schema.IM_OH_Client);
				paramCollection.Add(ZSqlParameter.New("@clientId", options.CompanyGuid.Value, IncidentMainSchema.IM_OH_Client));
			}

			if (options.Product.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} = @product ",
					AutoIncidentMain.Schema.IM_Product);
				paramCollection.Add(ZSqlParameter.New("@product", options.Product.Value, IncidentMainSchema.IM_Product));
			}

			if (options.ProductArea.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} = @productArea ",
					AutoIncidentMain.Schema.IM_ProgramArea);
				paramCollection.Add(ZSqlParameter.New("@productArea", options.ProductArea.Value, IncidentMainSchema.IM_ProgramArea));
			}

			query += string.Format(
					CultureInfo.InvariantCulture,
					"AND {0} IN (SELECT Value FROM @relevantIncidents)",
					AutoIncidentMain.Schema.PK);
			paramCollection.Add(ZSqlParameter.New("@relevantIncidents", relatedIncidents, IncidentMainSchema.PK, true));

			return incidentAssociationQuery
				.QueryObjects(query, paramCollection)
				.Select(dict => new ZGuid((Guid)dict[AutoIncidentMain.Schema.PK]));
		}

		public IEnumerable<IncidentSimilarityMatrix> SearchStoredIncidentSimilarities(SimilarIncidentSearchOptions options)
		{
			if (options.SourceIncidentPK == ZGuid.Empty)
			{
				throw new NoNullAllowedException($"'{nameof(options.SourceIncidentPK)}' property must be set in '{nameof(options)}' parameter.");
			}

			var paramCollection = new ZSqlParameterCollection();
			var query = string.Format(
				CultureInfo.InvariantCulture,
				@"SELECT ROW_NUMBER() OVER(ORDER BY {0} DESC) row_num, {1} FROM dbo.{2} ",
				AutoIncidentSimilarityMatrix.Schema.ISM_Similarity,
				AutoIncidentSimilarityMatrix.Schema.PK,
				AutoIncidentSimilarityMatrix.Schema.TableName);

			var restrictionType = options.GetRestriction();

			if (restrictionType == SimilarIncidentSearchOptionsRestriction.None)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					"WHERE ({0} = @sourcePk1 OR {1} = @sourcePk2)",
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2);

				paramCollection.Add(ZSqlParameter.New("@sourcePk1", options.SourceIncidentPK, IncidentSimilarityMatrixSchema.ISM_IM_Incident1));
				paramCollection.Add(ZSqlParameter.New("@sourcePk2", options.SourceIncidentPK, IncidentSimilarityMatrixSchema.ISM_IM_Incident2));
			}
			else if (restrictionType == SimilarIncidentSearchOptionsRestriction.Date)
			{
				var relevantIncidents = GetProcessedIncidentGuidsByDate(options.SourceIncidentPK.ToGuid(), options.FromTime, options.ToTime).Where(guid => guid != options.SourceIncidentPK).ToList();
				if (!relevantIncidents.Any())
				{
					return Enumerable.Empty<IncidentSimilarityMatrix>();
				}

				query += string.Format(
					CultureInfo.InvariantCulture,
					"WHERE ((({0} = @sourcePk1 AND {1} IN (SELECT Value FROM @relevantIncidents2)) OR ({2} = @sourcePk2 AND {3} IN (SELECT Value FROM @relevantIncidents1))))",
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2,
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2,
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1);

				paramCollection.Add(ZSqlParameter.New("@sourcePk1", options.SourceIncidentPK, IncidentSimilarityMatrixSchema.ISM_IM_Incident1));
				paramCollection.Add(ZSqlParameter.New("@sourcePk2", options.SourceIncidentPK, IncidentSimilarityMatrixSchema.ISM_IM_Incident2));
				paramCollection.Add(ZSqlParameter.New("@relevantIncidents1", relevantIncidents, IncidentSimilarityMatrixSchema.ISM_IM_Incident1, true));
				paramCollection.Add(ZSqlParameter.New("@relevantIncidents2", relevantIncidents, IncidentSimilarityMatrixSchema.ISM_IM_Incident2, true));
			}
			else if (restrictionType == SimilarIncidentSearchOptionsRestriction.Full)
			{
				var relevantIncidents = GetProcessedIncidentGuidsByOptions(options).Where(guid => guid != options.SourceIncidentPK).ToList();
				if (!relevantIncidents.Any())
				{
					return Enumerable.Empty<IncidentSimilarityMatrix>();
				}

				query += string.Format(
					CultureInfo.InvariantCulture,
					"WHERE ((({0} = @sourcePk1 AND {1} IN (SELECT Value FROM @relevantIncidents2)) OR ({2} = @sourcePk2 AND {3} IN (SELECT Value FROM @relevantIncidents1))))",
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2,
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2,
					AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1);

				paramCollection.Add(ZSqlParameter.New("@sourcePk1", options.SourceIncidentPK, IncidentSimilarityMatrixSchema.ISM_IM_Incident1));
				paramCollection.Add(ZSqlParameter.New("@sourcePk2", options.SourceIncidentPK, IncidentSimilarityMatrixSchema.ISM_IM_Incident2));
				paramCollection.Add(ZSqlParameter.New("@relevantIncidents1", relevantIncidents, IncidentSimilarityMatrixSchema.ISM_IM_Incident1, true));
				paramCollection.Add(ZSqlParameter.New("@relevantIncidents2", relevantIncidents, IncidentSimilarityMatrixSchema.ISM_IM_Incident2, true));
			}
			else
			{
				ErrorReporter.ReportOnce(
					"IncidentSimilarity",
					string.Format(
						CultureInfo.InvariantCulture,
						"The {0} value of {1} is not supported.", nameof(SimilarIncidentSearchOptionsRestriction), restrictionType));

				return Enumerable.Empty<IncidentSimilarityMatrix>();
			}

			if (options.MinimumSimilarity.HasValue)
			{
				query += string.Format(
					CultureInfo.InvariantCulture,
					" AND {0} >= @minimumSimilarity",
					AutoIncidentSimilarityMatrix.Schema.ISM_Similarity);
				paramCollection.Add(ZSqlParameter.New("@minimumSimilarity", new decimal(options.MinimumSimilarity.Value), IncidentSimilarityMatrixSchema.ISM_Similarity));
			}

			var compoundQuery = string.Format(
				CultureInfo.InvariantCulture,
				"WITH results AS ({0}) SELECT {1} FROM results WHERE row_num BETWEEN {2} AND {3}",
				query,
				AutoIncidentSimilarityMatrix.Schema.PK,
				options.StartIndex + 1,
				options.StartIndex + options.Count);

			return incidentAssociationQuery.QueryObjects(compoundQuery, paramCollection)
				.ToList()
				.Select(dict => new ZGuid((Guid)dict[AutoIncidentSimilarityMatrix.Schema.PK]))
				.Select(guid => Factory.Load<IncidentSimilarityMatrix>(guid));
		}

		public IEnumerable<double> GetIdfVector()
		{
			if (idfVector != null)
			{
				return idfVector;
			}

			var query = new ZQuery(StmDataSchema.SD_Name, string.Format(CultureInfo.InvariantCulture, "{0}{1}", IdfFieldName, RepositoryVersion));
			var stmRow = Factory.Load<StmData>(query).FirstOrDefault();

			if (stmRow == null)
			{
				return null;
			}

			var binaryArray = (byte[])stmRow.SD_BinaryValue;

			return idfVector = BytesToDoubles(binaryArray);
		}

		public ZGuid SetIdfVector(ICollection<double> idf)
		{
			var query = new ZQuery(StmDataSchema.SD_Name, string.Format(CultureInfo.InvariantCulture, "{0}{1}", IdfFieldName, RepositoryVersion));
			var row = Factory.Load<StmData>(query).FirstOrDefault();

			if (row == null)
			{
				row = Factory.New<StmData>();
				row.SD_Name = string.Format(CultureInfo.InvariantCulture, "{0}{1}", IdfFieldName, RepositoryVersion);
			}

			row.SD_BinaryValue = DoublesToBytes(idf).ToArray();
			Factory.Save();

			return row.PK;
		}

		public IDictionary<string, int> GetTokenPairs()
		{
			if (tokenPairs != null)
			{
				return tokenPairs;
			}

			var query = new ZQuery(IncidentSimilarityTokenSchema.IST_Version, RepositoryVersion);
			var queryResult = Factory.Load<IncidentSimilarityToken>(query);
			var resultDict = new Dictionary<string, int>();
			foreach (var tokenPair in queryResult)
			{
				if (!resultDict.ContainsKey(tokenPair.IST_InputToken.ToString()))
				{
					resultDict[tokenPair.IST_InputToken.ToString()] = (int)tokenPair.IST_OutputToken;
				}
			}

			return resultDict;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1725:Parameter names should match base declaration", Justification = "Conflicts with member field")]
		public void SetTokenPairs(IDictionary<string, int> newTokenPairs)
		{
			var query = new ZQuery(IncidentSimilarityTokenSchema.IST_Version, RepositoryVersion);
			var results = Factory.Load<IncidentSimilarityToken>(query);
			foreach (var result in results)
			{
				result.Delete();
			}
			Factory.Save();

			var newTokens = new List<IncidentSimilarityToken>();

			foreach (var pair in newTokenPairs)
			{
				var newToken = Factory.New<IncidentSimilarityToken>();
				newToken.IST_InputToken = pair.Key;
				newToken.IST_OutputToken = pair.Value;
				newToken.IST_Version = RepositoryVersion;

				newTokens.Add(newToken);
			}

			Factory.Save();
		}

		public void UpdateTfIdfTimestamps(IDictionary<Guid, DateTime> timestamps)
		{
			incidentAssociationQuery.UpdateTfIdfTimestamps(RepositoryVersion, timestamps);
		}

		public static IEnumerable<double> BytesToDoubles(ICollection<byte> bytes)
		{
			var numDoubles = bytes.Count / 8;

			using (var br = new BinaryReader(new MemoryStream(bytes.ToArray()), Encoding.ASCII, leaveOpen: false))
			{
				for (var i = 0; i < numDoubles; i++)
				{
					yield return br.ReadDouble();
				}
			}
		}

		public static IEnumerable<byte> DoublesToBytes(IEnumerable<double> doubles)
		{
			using (var ms = new MemoryStream())
			{
				using (var bw = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
				{
					foreach (var value in doubles)
					{
						bw.Write(value);
					}
				}

				return ms.ToArray();
			}
		}
	}
}
