using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;

namespace Enterprise.Client.EDI
{
	public interface ITokenBootstrapper
	{
		void BootstrapTokens(IDictionary<string, int> tokens);
	}

	public class TokenBootstrapper : IncidentAssociationQuery, ITokenBootstrapper
	{
		readonly int newVersion;

		public TokenBootstrapper(int newVersion) : this(newVersion, Db.Connection) { }
		public TokenBootstrapper(int newVersion, DbConnection dbConnection) : base(dbConnection)
		{
			var latestVersion = GetLatestVersion();
			if (latestVersion > newVersion)
			{
				throw new ArgumentException(
					string.Format(
					CultureInfo.InvariantCulture,
					"The latest version ({0}) is older than version {1}",
					latestVersion,
					newVersion));
			}
			this.newVersion = newVersion;
		}

		public void BootstrapTokens(IDictionary<string, int> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException(nameof(tokens));
			}
			var incidentSimilarityTokenDataTransferObjects = tokens
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.Select(tok => new IncidentSimilarityTokenDataTransferObject(
					Guid.NewGuid(),
					newVersion,
					tok.Key,
					tok.Value))
				.ToList();

			AddNewTokenVersion(incidentSimilarityTokenDataTransferObjects);
		}

		void AddNewTokenVersion(IEnumerable<IncidentSimilarityTokenDataTransferObject> incidentSimilarityTokens, int batchSize = 1000)
		{
			using (var transactionManager = dbConnection.BeginTransactionWithManager())
			{
				DeleteVersionOfTokens(newVersion);
				while (incidentSimilarityTokens.Any())
				{
					var dataTransferObjectsBatch = incidentSimilarityTokens.Take(batchSize);
					incidentSimilarityTokens = incidentSimilarityTokens.Skip(batchSize);
					AddIncidentSimilarityTokens(dataTransferObjectsBatch);
				}

				transactionManager.CommitTransaction();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteVersionOfTokens(int version)
		{
			var command = dbConnection.Command(string.Format(
				CultureInfo.InvariantCulture,
				"DELETE FROM {0} WHERE {1} = @version",
				AutoIncidentSimilarityToken.Schema.TableName,
				AutoIncidentSimilarityToken.Schema.IST_Version));
			command.AddParameter("@version", SqlDbType.Int, version);
			command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AddIncidentSimilarityTokens(IEnumerable<IncidentSimilarityTokenDataTransferObject> dataTransferObjects)
		{
			var values = dataTransferObjects.Select(dto => string.Format(
					CultureInfo.InvariantCulture,
					"('{0}',{1},'{2}',{3})",
					dto.PK.ToString(),
					dto.Version,
					dto.InputToken,
					dto.OutputToken));

			var sqlInsert = string.Format(
					CultureInfo.InvariantCulture,
					"INSERT INTO {0} ({1},{2},{3},{4}) VALUES {5}",
					AutoIncidentSimilarityToken.Schema.TableName,
					AutoIncidentSimilarityToken.Schema.PK,
					AutoIncidentSimilarityToken.Schema.IST_Version,
					AutoIncidentSimilarityToken.Schema.IST_InputToken,
					AutoIncidentSimilarityToken.Schema.IST_OutputToken,
					string.Join(",", values));

			dbConnection.Command(sqlInsert).ExecuteNonQuery();
		}

		internal class IncidentSimilarityTokenDataTransferObject
		{
			public ZGuid PK { get; }
			public int Version { get; }
			public string InputToken { get; }
			public int OutputToken { get; }
			public IncidentSimilarityTokenDataTransferObject(ZGuid pk, int version, string inputToken, int outputToken)
			{
				PK = pk;
				Version = version;
				InputToken = inputToken;
				OutputToken = outputToken;
			}
		}
	}
}
