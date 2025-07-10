using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Gate
{
	public class PopulateGteGateBranch : DataTransformation
	{
		public override string UserDescription => "Add and populate GTE_GB_Branch linking GteGate to GlbBranch";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, GteGateSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, GteGateSchema.Constants.TableName, "GTE_OA_Address")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, GteGateSchema.Constants.TableName, GteGateSchema.Constants.GTE_GB_Branch, "UNIQUEIDENTIFIER"))
			{
				var sql = @"
IF EXISTS (SELECT TOP 1 NULL FROM [dbo].[GteGate])
BEGIN
	DECLARE @GateBranchMapping TABLE (GatePK UNIQUEIDENTIFIER, UniqueBranchPK UNIQUEIDENTIFIER);
	DECLARE @UniqueGateCodeAndBranch TABLE (GateCode CHAR(3), BranchPK UNIQUEIDENTIFIER, IsUnique BIT);

	INSERT INTO @GateBranchMapping 
	SELECT 
		[GTE_PK],
		CASE
			WHEN COUNT(DISTINCT [WW_GB_RelatedCompanyBranch]) = 1
			THEN MAX([WW_GB_RelatedCompanyBranch])
			ELSE NULL
		END
	FROM [dbo].[GteGate]
	LEFT JOIN [dbo].[WhsWarehouse] ON [GTE_OA_Address] = [WW_OA_WarehouseAddress]
	GROUP BY [GTE_PK];

	UPDATE [dbo].[GteGate]
	SET
		[GTE_GB_Branch] = [UniqueBranchPK],
		[GTE_IsActive] = CASE WHEN [UniqueBranchPK] IS NOT NULL THEN 1 ELSE 0 END,
		[GTE_SystemLastEditTimeUtc] = GETUTCDATE(),
		[GTE_SystemLastEditUser] = '~BP'
	FROM [dbo].[GteGate]
	JOIN @GateBranchMapping ON [GTE_PK] = [GatePK];

	INSERT INTO @UniqueGateCodeAndBranch
	Select
		[GTE_Code],
		[GTE_GB_Branch],
		CASE
			WHEN COUNT(*) = 1
			THEN 1
			ELSE 0
		END
	FROM [dbo].[GteGate]
	GROUP BY [GTE_Code], [GTE_GB_Branch];

	UPDATE [dbo].[GteGate]
	SET
		[GTE_IsActive] = [IsUnique],
		[GTE_SystemLastEditTimeUtc] = GETUTCDATE(),
		[GTE_SystemLastEditUser] = '~BP'
	FROM [dbo].[GteGate]
	JOIN @UniqueGateCodeAndBranch
		ON [GTE_Code] = [GateCode]
		AND [GTE_GB_Branch] = [BranchPK]
	WHERE [GTE_IsActive] = 1;
END
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
