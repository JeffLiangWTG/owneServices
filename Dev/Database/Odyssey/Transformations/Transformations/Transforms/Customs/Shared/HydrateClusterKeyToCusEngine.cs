using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

public class HydrateClusterKeyToCusEngine : DataTransformation
{
	public override string UserDescription => "Hydrates ClusterKey to existing CusEngine records";

	bool CusVehicleClusterKeyColumnExists()
	{
		return DbObjectCreator.TableExists(Db.Connection, CusVehicleSchema.Constants.TableName) &&
				DbObjectCreator.ColumnExists(Db.Connection, CusVehicleSchema.Constants.TableName,
					CusVehicleSchema.Constants.CVH_ClusterKey);
	}

	bool JobComInvoiceLineClusterKeyColumnExists()
	{
		return DbObjectCreator.TableExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName) &&
				DbObjectCreator.ColumnExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName,
					JobComInvoiceLineSchema.Constants.JI_ClusterKey);
	}

	protected override void OnlinePreUpgradeTransform()
	{
		const int batchSize = 1000;
		int rowsAffected = 0;
		var cusVehicleClusterKeyColumnExists = CusVehicleClusterKeyColumnExists();
		var jobComInvoiceLineClusterKeyColumnExists = JobComInvoiceLineClusterKeyColumnExists();

		do
		{
			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				try
				{
					rowsAffected = 0;

					if (cusVehicleClusterKeyColumnExists)
					{
						rowsAffected += Db.Connection.ExecuteNonQuery(@$"
UPDATE TOP ({batchSize}) CEG
SET CEG_ClusterKey = CVH_ClusterKey, CEG_SystemLastEditTimeUtc = GETUTCDATE(), CEG_SystemLastEditUser = '~BP'
FROM CusEngine CEG
	INNER JOIN CusVehicle ON CVH_PK = CEG_ParentID
WHERE CEG_ParentTableCode = 'CVH' AND CVH_ClusterKey <> 0 AND CEG_ClusterKey = 0
");
					}

					if (jobComInvoiceLineClusterKeyColumnExists)
					{
						rowsAffected += Db.Connection.ExecuteNonQuery(@$"
UPDATE TOP ({batchSize}) CEG
SET CEG_ClusterKey = JI_ClusterKey, CEG_SystemLastEditTimeUtc = GETUTCDATE(), CEG_SystemLastEditUser = '~BP'
FROM CusEngine CEG
	INNER JOIN JobComInvoiceLine ON JI_PK = CEG_ParentID
WHERE CEG_ParentTableCode = 'JI' AND JI_ClusterKey <> 0 AND CEG_ClusterKey = 0
");
					}

					manager.CommitTransaction();
				}
				catch (Exception)
				{
					manager.RollbackTransaction();
					throw;
				}
			}
		} while (rowsAffected > 0);
	}

	protected override void OfflinePreUpgradeTransform()
	{
		if (CusVehicleClusterKeyColumnExists())
		{
			Db.Connection.ExecuteNonQuery(@"
UPDATE CEG
SET CEG_ClusterKey = CVH_ClusterKey, CEG_SystemLastEditTimeUtc = GETUTCDATE(), CEG_SystemLastEditUser = '~BP'
FROM CusEngine CEG
	INNER JOIN CusVehicle ON CVH_PK = CEG_ParentID
WHERE CEG_ParentTableCode = 'CVH' AND CVH_ClusterKey <> 0 AND CEG_ClusterKey = 0");
		}

		if (JobComInvoiceLineClusterKeyColumnExists())
		{
			Db.Connection.ExecuteNonQuery(@"
UPDATE CEG
SET CEG_ClusterKey = JI_ClusterKey, CEG_SystemLastEditTimeUtc = GETUTCDATE(), CEG_SystemLastEditUser = '~BP'
FROM CusEngine CEG
	INNER JOIN JobComInvoiceLine ON JI_PK = CEG_ParentID
WHERE CEG_ParentTableCode = 'JI' AND JI_ClusterKey <> 0 AND CEG_ClusterKey = 0");
		}
	}
}
