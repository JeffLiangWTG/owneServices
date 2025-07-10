using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Integration;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	#region SuppressResourceStringsCheckRegion

	public class SsasProjectUpgrader : IDisposable
	{
		protected readonly string analysisServer;

		public SsasProjectUpgrader(ILogger logger = null)
		{
			analysisServer = BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection);
			this.logger = logger;
		}
		readonly ILogger logger;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (deployer != null)
				{
					deployer.Dispose();
					deployer = null;
				}
			}
		}

		public void Upgrade()
		{
			Deployer.DeploySsasModels();
		}

		public void PartitionSsasModels()
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(dwServer, Db.EdwDatabaseName))
			{
				var partitionManager = new PartitionManager(biConnection, analysisServer, logger);
				partitionManager.PartitionCubes();
			}
		}

		public void ProcessSsasModels()
		{
			Deployer.ProcessSsasModels();
		}

		#region Upgrade Required

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		public bool CheckRequirements(out string unsatisfiedRequirementMessage)
		{
			if (string.IsNullOrWhiteSpace(analysisServer))
			{
				unsatisfiedRequirementMessage = "Analysis Server registry is empty. SSAS cubes cannot be deployed.";
			}
			else
			{
				try
				{
					if (!SsasRequirementChecker.Instance.IsSsasMeetRequirementWithTabularServerMode(analysisServer))
					{
						unsatisfiedRequirementMessage = string.Format(CultureInfo.InvariantCulture, "SSAS server [{0}] does not exist, its version is below 2016 or it is not Tabular mode.", analysisServer);
					}
					else
					{
						unsatisfiedRequirementMessage = null;
					}
				}
				catch (SsasException ex)
				{
					unsatisfiedRequirementMessage = ex.Message;
				}
			}

			return (unsatisfiedRequirementMessage == null);
		}

		protected virtual ReadOnlyDictionary<string, VersionLabel> ModelVersions
		{
			get
			{
				return SsasProjectVersion.ModelVersions;
			}
		}

		protected virtual string GetClientName()
		{
			return Db.DatabaseName;
		}

		protected virtual SsasCubesDataTable GetRegisteredSsasCubes()
		{
			return BiAutomationConfigLoader.Instance.ConfigData.SsasCubes;
		}

		public virtual bool AreAllCubesDeployedAndUpdated(bool checkReaderRole)
		{
			var result = true;
			using (var server = SsasServer.New(analysisServer))
			{
				foreach (var ssasCube in GetRegisteredSsasCubes())
				{
					var ssasDatabaseName = GetClientName() + "_" + ssasCube.SsasModelLogicalName;

					if (Deployer.TabularModelsToRedeploy.Contains(ssasCube.SsasModelFileName))
					{
						result = false;
						break;
					}
					else if (!Deployer.TabularModelsToSkip.Contains(ssasCube.SsasModelFileName))
					{
						VersionLabel expectedVersion;
						if (ModelVersions.TryGetValue(ssasCube.SsasModelFileName, out expectedVersion))
						{
							var ssasVersion = server.GetDatabaseVersion(ssasDatabaseName);
							if (ssasVersion != expectedVersion.ToString() || (checkReaderRole && !Deployer.CheckReaderRoleHasCompleteMembers(ssasDatabaseName)))
							{
								result = false;
								break;
							}
						}
						else
						{
							throw new SsasException(string.Format(CultureInfo.InvariantCulture, "Version for analysis cube [{0}] was not found.", ssasCube.SsasModelFileName));
						}
					}
					else if (server.DatabaseExists(ssasDatabaseName))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		#endregion

#if DEBUG
		protected virtual
#endif
		SsasDeployer Deployer
		{
			get
			{
				return deployer ?? (deployer = new SsasDeployer(analysisServer, logger));
			}
		}
		SsasDeployer deployer;
	}

	#endregion
}
