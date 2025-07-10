using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI
{
	public class IncidentAssociationDatasetBuilderRunner
	{
		public const string LastDateStmDataName = "IncidentAssociationLastDatasetBuildTime";
		public const int DaysBetweenRebuilds = 90;

		readonly ILogger logger;
		readonly BusinessObjectFactory factory;
		readonly IBootstrapperProvider bootstrapperProvider;
		readonly ISimilarIncidentRepository similarIncidentRepository;

		public IncidentAssociationDatasetBuilderRunner(ILogger logger,
			BusinessObjectFactory factory,
			IBootstrapperProvider bootstrapperProvider,
			ISimilarIncidentRepository similarIncidentRepository)
		{
			this.logger = logger;
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			this.bootstrapperProvider = bootstrapperProvider ?? throw new ArgumentNullException(nameof(bootstrapperProvider));
			this.similarIncidentRepository = similarIncidentRepository ?? throw new ArgumentNullException(nameof(similarIncidentRepository));
		}

		public static ZBlob BlobFromDateTime(DateTime dt)
		{
			return new ZBlob(BitConverter.GetBytes(dt.ToBinary()));
		}

		public static DateTime DateTimeFromBlob(ZBlob blob)
		{
			return DateTime.FromBinary(BitConverter.ToInt64(blob, 0));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public bool Run(int monthsToStore)
		{
			var stmRequest = factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, IncidentAssociationSimilarityMatrixBootstrapperRunner.StmFieldName));
			if (stmRequest != null)
			{
				logger?.Warning("Cannot bootstrap the incident association database because a request to bootstrap the similarity matrix is still pending");
				return false;
			}

			var incidentAssociationSystemStatus = IncidentAssociationSystemStatus.GetStatus(factory);
			if (!(incidentAssociationSystemStatus == IncidentAssociationSystemStatus.ReadyToBootstrap ||
				  incidentAssociationSystemStatus == IncidentAssociationSystemStatus.ReadyForNewIncidents))
			{
				logger?.Information("Not yet ready to bootstrap");
				return false;
			}

			var item = factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, LastDateStmDataName));
			var latestVersion = similarIncidentRepository.GetLatestVersion();

			DateTime lastUpdateTime;

			if (item == null || DateTime.UtcNow > (lastUpdateTime = DateTimeFromBlob(item.SD_BinaryValue)).AddDays(DaysBetweenRebuilds))
			{
				var newVersion = Math.Max(1, latestVersion + 1);
				var tokens = LoadTokens();

				if (BuildDataset(
					tokens,
					bootstrapperProvider.GetTokenBootstrapper(newVersion),
					bootstrapperProvider.GetTfIdfBootstrapper(newVersion, tokens),
					monthsToStore))
				{
					logger?.Information("IAD bootstrapping complete");
					return true;
				}
				else
				{
					logger?.Warning("IAD bootstrapping is incomplete");
					return false;
				}
			}
			else
			{
				logger?.Information(
					string.Format(CultureInfo.InvariantCulture, "Not bootstrapping because previous bootstrap was at {0}, which is more recently than the threshold of {1} days ago ({2})",
						lastUpdateTime,
						DaysBetweenRebuilds,
						DateTime.UtcNow.AddDays(-DaysBetweenRebuilds)));

				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public bool BuildDataset(
			IDictionary<string, int> tokens,
			ITokenBootstrapper tokenBootstrapper,
			ITfIdfBootstrapper tfIdfBootstrapper,
			int monthsToStore)
		{
			logger?.Debug("Incident Association Data set Builder Service Task");

			var currentVersion = similarIncidentRepository.GetLatestVersion();
			if (currentVersion < 0) { throw new ArgumentException("No initial version in similarity database"); }
			var newVersion = Math.Max(1, currentVersion + 1);

			var toDate = DateTime.UtcNow;
			var fromDate = toDate.AddMonths(-monthsToStore);

			logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Current version: {0}; New version: {1}", currentVersion, newVersion));
			try
			{
				logger?.Information(string.Format(
						CultureInfo.InvariantCulture,
						"Bootstrapping version {0} with incidents between {1} and {2}",
						newVersion,
						fromDate.ToString(CultureInfo.InvariantCulture),
						toDate.ToString(CultureInfo.InvariantCulture)));

				tokenBootstrapper.BootstrapTokens(tokens);
				tfIdfBootstrapper.BootstrapBetween(fromDate, toDate);

				var stmRequest = factory.New<StmData>();
				stmRequest.SD_Name = IncidentAssociationSimilarityMatrixBootstrapperRunner.StmFieldName;
				stmRequest.SD_BinaryValue = BitConverter.GetBytes(newVersion);
				factory.Save();
				return true;
			}
			catch (OperationCanceledException oce)
			{
				logger?.Warning(string.Format(CultureInfo.InvariantCulture, "Cancellation requested before `{0}` completed", oce.TargetSite));
				return false;
			}
		}

		IDictionary<string, int> LoadTokens()
		{
			var tokens = similarIncidentRepository.GetTokenPairs();
			var latestVersion = similarIncidentRepository.GetLatestVersion();
			logger?.Information(string.Format(CultureInfo.InvariantCulture, "Loaded {0} tokens from version {1}", tokens.Count, latestVersion));
			return tokens;
		}
	}
}
