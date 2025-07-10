using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Integration.Licensing;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	public class SsasInfoPopulator
	{
		public SsasInfoPopulator(DbConnection biConnection)
		{
			this.biConnection = biConnection;
		}
		readonly DbConnection biConnection;

		protected virtual bool DeployToServer
		{
			get
			{
				if (deployToServer == null)
				{
					using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.DatabaseName))
					{
						var registration = ObjectFactory.Get<IProductRegistration>();
						deployToServer = registration.IsWiseTechGlobalInternalSystem();
					}
#if DEBUG
					deployToServer = true;
#endif
				}
				return deployToServer.Value;
			}
		}
		bool? deployToServer;
		public void PopulateSsasInfo()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.EdwDatabaseName))
			{
				foreach (var ssasCube in ConfigData.SsasCubes)
				{
					UpdateSsasCubeInformation(ssasCube);
					InsertOrUpdateSsasTables(ssasCube);
					DeleteUnusedSsasTables(ssasCube);
				}
				DeleteUnusedSsasCubes();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateSsasCubeInformation(BiAutomationConfigDataSet.SsasCubesRow ssasCube)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
UPDATE [{0}].[SsasCube]
	SET DeployToServer = @DeployToServer
WHERE DeployToServer IS NULL

IF NOT EXISTS (SELECT NULL FROM [{0}].[SsasCube] WHERE SsasModelFileName = @SsasModelFileName)
	INSERT INTO [{0}].[SsasCube] (SsasModelFileName, SsasModelLogicalName, SsasModelVersion, LastSourceLsnDateTimeUTC, LastProcessingStartDateTimeUTC, LastProcessingFinishDateTimeUTC, IsCubeProcessing, DeployToServer)
		VALUES(@SsasModelFileName, @SsasModelLogicalName, NULL, NULL, NULL, NULL, 0, @DeployToServer);
ELSE IF NOT EXISTS (SELECT NULL FROM [{0}].[SsasCube] WHERE SsasModelFileName = @SsasModelFileName AND SsasModelVersion = @SsasModelVersion AND SsasModelLogicalName = @SsasModelLogicalName)
	UPDATE [{0}].[SsasCube]
	SET SsasModelVersion = @SsasModelVersion,
		SsasModelLogicalName = @SsasModelLogicalName
	WHERE SsasModelFileName = @SsasModelFileName AND DeployToServer = 1",
				BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@DeployToServer", SqlDbType.Bit, DeployToServer);
				cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, ssasCube.SsasModelFileName);
				cmd.AddParameter("@SsasModelLogicalName", SqlDbType.VarChar, 128, ssasCube.SsasModelLogicalName);

				VersionLabel ssasVersion;
				if (SsasProjectVersion.ModelVersions.TryGetValue(ssasCube.SsasModelFileName, out ssasVersion))
				{
					cmd.AddParameter("@SsasModelVersion", SqlDbType.VarChar, 128, ssasVersion.ToString());
				}
				else
				{
					cmd.AddParameter("@SsasModelVersion", SqlDbType.VarChar, 128, "");
				}
				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void InsertOrUpdateSsasTables(BiAutomationConfigDataSet.SsasCubesRow ssasCube)
		{
			string ssasTableQuery = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS (
	SELECT NULL
	FROM
		[{0}].[SsasCube] c
		INNER JOIN [{0}].[SsasTable] t ON c.SsasCubeID = t.SsasCubeID
	WHERE
		c.SsasModelFileName = @SsasModelFileName
		AND t.TableName = @TableName
)
BEGIN
	INSERT INTO [{0}].[SsasTable] (SsasCubeID, TableName, PartitionQuery, PartitionKey)
		SELECT TOP 1 c.SsasCubeID, @TableName, @PartitionQuery, @PartitionKey 
		FROM [{0}].[SsasCube] c WHERE c.SsasModelFileName = @SsasModelFileName;
END
ELSE
BEGIN
	UPDATE [{0}].[SsasTable]
		SET
			PartitionQuery = @PartitionQuery,
			PartitionKey = @PartitionKey
		FROM
			[{0}].[SsasTable] t
			INNER JOIN [{0}].[SsasCube] c ON c.SsasCubeID = t.SsasCubeID
		WHERE
			t.TableName = @TableName
			AND c.SsasModelFileName = @SsasModelFileName;
END",
		BiConstants.BiAdminSchemaName);

			foreach (var ssasTable in ssasCube.GetSsasTablesRows().Where(t => !t.IsCalculated))
			{
				using (var cmd = biConnection.Command(ssasTableQuery))
				{
					cmd.AddParameter("@PartitionQuery", SqlDbType.NVarChar, -1, !ssasTable.IsCalculated ? ssasTable.Query ?? "" : "");
					cmd.AddParameter("@PartitionKey", SqlDbType.NVarChar, 128, ssasTable.PartitionKeyName ?? "");
					cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, ssasCube.SsasModelFileName);
					cmd.AddParameter("@TableName", SqlDbType.VarChar, 128, ssasTable.TableName);

					cmd.ExecuteNonQuery();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteUnusedSsasTables(BiAutomationConfigDataSet.SsasCubesRow ssasCube)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
@"
DELETE p
	FROM
		[{0}].[SsasPartition] p
		INNER JOIN [{0}].[SsasTable] t ON p.SsasTableID = t.SsasTableID
		INNER JOIN [{0}].[SsasCube] c ON t.SsasCubeID = c.SsasCubeID
	WHERE
		c.SsasModelFileName = @SsasModelFileName
		AND ISNULL(t.TableName, '') NOT IN ('{1}')

DELETE t
	FROM
		[{0}].[SsasTable] t
		INNER JOIN [{0}].[SsasCube] c ON t.SsasCubeID = c.SsasCubeID
	WHERE
		c.SsasModelFileName = @SsasModelFileName
		AND ISNULL(t.TableName, '') NOT IN ('{1}')",
				BiConstants.BiAdminSchemaName,
				string.Join("', '", ssasCube.GetSsasTablesRows().Where(t => !t.IsCalculated).Select(t => t.TableName)) // No user defined TVPs in EDW database
			);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@SsasModelFileName", SqlDbType.VarChar, 128, ssasCube.SsasModelFileName);
				cmd.ExecuteNonQuery();
			}
		}

		void DeleteUnusedSsasCubes()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
@"
DELETE FROM [{1}].SsasPartition WHERE SsasTableID IN
(SELECT SsasTableID
 FROM [{1}].SsasTable t
 INNER JOIN [{1}].SsasCube c
	ON t.SsasCubeId = c.SsasCubeID
 WHERE ISNULL(c.SsasModelFileName, '') NOT IN ('{0}'))

DELETE FROM [{1}].SsasTable WHERE SsasCubeID IN
(SELECT SsasCubeID
 FROM [{1}].SsasCube
 WHERE ISNULL(SsasModelFileName, '') NOT IN ('{0}'))

DELETE FROM [{1}].SsasCube
WHERE ISNULL(SsasModelFileName, '') NOT IN ('{0}')",
				string.Join("', '", ConfigData.SsasCubes.Select(c => c.SsasModelFileName)),
				BiConstants.BiAdminSchemaName);
			biConnection.ExecuteNonQuery(sqlText);
		}

		BiConfigurationData ConfigData
		{
			get
			{
				return configData ?? (configData = BiAutomationConfigLoader.Instance.ConfigData);
			}
		}
		BiConfigurationData configData;
	}
}
