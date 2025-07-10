using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	sealed class SeedUniversalClusterKeyNumberFountain : DataTransformation
	{
		public override string UserDescription => "Initialize universal cluster key number fountain";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DECLARE @clusterKeyName VARCHAR(256) = 'ClusterKeyNumber'

IF NOT EXISTS(SELECT NULL FROM dbo.StmNums WHERE SN_Name = @clusterKeyName)
	BEGIN TRY
		DECLARE @minValue BIGINT = 1
		DECLARE @maxValue BIGINT = 2147483647
		DECLARE @canRollover BIT = 0
		DECLARE @sequence SMALLINT = 0

		DECLARE @seed BIGINT =
			(
				SELECT MAX(ClusterKeyNumbers.SN_Value) FROM
					(
						SELECT SN_Value FROM dbo.StmNums
						WHERE SN_Name IN
								(
									'AsycudaManifestHeaderClusterKey',
									'CusExitHeaderClusterKey',
									'CusIntrastatGroupClusterKey',
									'CusIntrastatHeaderClusterKey',
									'CustomsDeclarationClusterKey',
									'CusUSLVClearanceClusterKey'
								)
						UNION SELECT @minValue AS SN_Value
					) AS ClusterKeyNumbers
			)

		INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
		VALUES (@clusterKeyName, @seed, @minValue, @maxValue, @canRollover, @sequence, GETUTCDATE())

		DELETE FROM dbo.StmNums
		WHERE SN_Name IN
				(
					'AsycudaManifestHeaderClusterKey',
					'CusExitHeaderClusterKey',
					'CusIntrastatGroupClusterKey',
					'CusIntrastatHeaderClusterKey',
					'CustomsDeclarationClusterKey',
					'CusUSLVClearanceClusterKey'
				)
	END TRY
	BEGIN CATCH
		THROW
	END CATCH";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
