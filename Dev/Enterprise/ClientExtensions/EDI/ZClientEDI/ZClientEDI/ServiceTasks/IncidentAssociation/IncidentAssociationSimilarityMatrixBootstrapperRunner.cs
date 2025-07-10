using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI
{
	public class IncidentAssociationSimilarityMatrixBootstrapperRunner
	{
		public const string StmFieldName = "IncidentAssociationMatrixBootstrapRequest";
		public const int StrideLength = 1000;

		readonly BusinessObjectFactory factory;
		readonly ILogger logger;

		public IncidentAssociationSimilarityMatrixBootstrapperRunner(
			ILogger logger,
			BusinessObjectFactory factory)
		{
			this.logger = logger;
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public bool Run(int strideLength)
		{
			var stmRequest = factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, StmFieldName));

			if (stmRequest == null)
			{
				logger?.Debug("No request to rebuild similarity matrix at this time");
				return false;
			}

			var newVersion = BitConverter.ToInt32(stmRequest.SD_BinaryValue, 0);
			logger?.Debug(string.Format(CultureInfo.InvariantCulture, "A request exists to bootstrap similarity matrix version {0}", newVersion));

			var minimumSimilarity = (double)EDIDataRegistry.Instance.RelatedIncidentsMinimumSimilarity.Value;
			var maxToStore = EDIDataRegistry.Instance.RelatedIncidentsMaxTopToStore.Value;

			try
			{
				var allDone = new SimilarityMatrixBootstrapper(logger)
					.BootstrapSimilarityMatrix(newVersion, maxToStore, minimumSimilarity, strideLength);

				if (allDone)
				{
					try
					{
						SimilarIncidentRepository.SetLatestVersion(newVersion);
					}
					catch (ZSaveConcurrencyException)
					{
						logger?.Debug("A ZSaveConcurrencyException was thrown while trying to set the latest version");
					}

					try
					{
						IncidentAssociationSystemStatus.SetStatus(IncidentAssociationSystemStatus.ReadyForNewIncidents);
					}
					catch (ZSaveConcurrencyException)
					{
						logger?.Debug("A ZSaveConcurrencyException was thrown while trying to set the status");
					}

					SqlApplicationLock appLock;

					try
					{
						stmRequest.Delete();
						factory.Save();

						// Bootstrapping is finished, but might have taken a really long time.
						// So, now process any new incidents that were added/modified during bootstrapping:
						if (Db.Connection.TryGetLock(IncidentAssociationNewIncidentsServiceTask.DbAppLockCode, out appLock))
						{
							logger?.Information("Checking for new and updated incidents during bootstrapping");
							using (appLock)
							{
								var runner = new IncidentAssociationNewIncidentsRunner(logger);
								runner.ProcessNewIncidentVectors();
								runner.ProcessNewIncidentSimilarities(maxToStore);
							}
						}
					}
					catch (ZSaveConcurrencyException)
					{
						logger?.Debug("A ZSaveConcurrencyException was thrown while trying to delete the bootstrapping request");
					}

					if (Db.Connection.TryGetLock(IncidentAssociationNewIncidentsServiceTask.DbAppLockCode, out appLock))
					{
						using (appLock)
						{
							var item = factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, IncidentAssociationDatasetBuilderRunner.LastDateStmDataName));
							if (item == null)
							{
								item = factory.New<StmData>();
								item.SD_Name = IncidentAssociationDatasetBuilderRunner.LastDateStmDataName;
							}

							var utcNow = ZDateTime.UtcNow.ToDateTime();
							logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Setting last bootstrap time to {0}", utcNow));
							item.SD_BinaryValue = IncidentAssociationDatasetBuilderRunner.BlobFromDateTime(utcNow);
							factory.Save();
						}
					}
				}

				logger?.Information("Done.");
				return true;
			}
			catch (OperationCanceledException oce)
			{
				logger?.Warning(string.Format(CultureInfo.InvariantCulture, "Cancellation requested before `{0}` completed", oce.TargetSite));
				return false;
			}
		}
	}
}
