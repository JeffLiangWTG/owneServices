using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

public class HydrateClusterKeyToCusVehicle : DataTransformation
{
	public override string UserDescription => "Hydrates ClusterKey to existing CusVehicle records";

	bool JobDeclarationClusterKeyColumnExists()
	{
		return DbObjectCreator.TableExists(Db.Connection, JobDeclarationSchema.Constants.TableName) &&
				DbObjectCreator.ColumnExists(Db.Connection,
					JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.Constants.JE_ClusterKey);
	}

	bool JobComInvoiceLineClusterKeyColumnExists()
	{
		return DbObjectCreator.TableExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName) &&
				DbObjectCreator.ColumnExists(Db.Connection,
					JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.JI_ClusterKey);
	}

	protected override void OnlinePreUpgradeTransform()
	{
		const int batchSize = 1000;
		var rowsAffected = 0;
		var jobDeclarationClusterKeyColumnExists = JobDeclarationClusterKeyColumnExists();
		var jobComInvoiceLineClusterKeyColumnExists = JobComInvoiceLineClusterKeyColumnExists();

		do
		{
			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				try
				{
					rowsAffected = 0;

					if (jobDeclarationClusterKeyColumnExists)
					{
						rowsAffected += Db.Connection.ExecuteNonQuery(@$"
UPDATE TOP ({batchSize}) CVH
SET CVH_ClusterKey = JE_ClusterKey, CVH_SystemLastEditTimeUtc = GETUTCDATE(), CVH_SystemLastEditUser = '~BP'
FROM CusVehicle CVH
	INNER JOIN JobDeclaration ON JE_PK = CVH_ParentID
WHERE CVH_ParentTableCode = 'JE' AND JE_ClusterKey <> 0 AND CVH_ClusterKey = 0
");
					}

					if (jobComInvoiceLineClusterKeyColumnExists)
					{
						rowsAffected += Db.Connection.ExecuteNonQuery(@$"
UPDATE TOP ({batchSize}) CVH
SET CVH_ClusterKey = JI_ClusterKey, CVH_SystemLastEditTimeUtc = GETUTCDATE(), CVH_SystemLastEditUser = '~BP'
FROM CusVehicle CVH
	INNER JOIN JobComInvoiceLine ON JI_PK = CVH_ParentID
WHERE CVH_ParentTableCode = 'JI' AND JI_ClusterKey <> 0 AND CVH_ClusterKey = 0
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
		if (JobDeclarationClusterKeyColumnExists())
		{
			Db.Connection.ExecuteNonQuery(@"
UPDATE CVH
SET CVH_ClusterKey = JE_ClusterKey, CVH_SystemLastEditTimeUtc = GETUTCDATE(), CVH_SystemLastEditUser = '~BP'
FROM CusVehicle CVH
	INNER JOIN JobDeclaration ON JE_PK = CVH_ParentID
WHERE CVH_ParentTableCode = 'JE' AND JE_ClusterKey <> 0 AND CVH_ClusterKey = 0");
		}

		if (JobComInvoiceLineClusterKeyColumnExists())
		{
			Db.Connection.ExecuteNonQuery(@"
UPDATE CVH
SET CVH_ClusterKey = JI_ClusterKey, CVH_SystemLastEditTimeUtc = GETUTCDATE(), CVH_SystemLastEditUser = '~BP'
FROM CusVehicle CVH
	INNER JOIN JobComInvoiceLine ON JI_PK = CVH_ParentID
WHERE CVH_ParentTableCode = 'JI' AND JI_ClusterKey <> 0 AND CVH_ClusterKey = 0");
		}
	}
}
