using System.Threading;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterData
{
	public class RemovePatternMatchingDataForConstraints : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraints on column ParentTableCode of PatternMatching table'";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var sql = @$"
DELETE FROM dbo.[PatternMatchingAddress]
WHERE PMA_ParentTableCode NOT IN (
	'GS', 'HA', 'OA', 'PER'
)

DELETE FROM dbo.[PatternMatchingDomain]
WHERE PMD_ParentTableCode NOT IN (
	'HA', 'OA', 'OC', 'PU'
)

DELETE FROM dbo.[PatternMatchingEmail]
WHERE PME_ParentTableCode NOT IN (
	'GS', 'HA', 'OA', 'OC', 'OI', 'PER'
)

DELETE FROM dbo.[PatternMatchingName]
WHERE PMN_ParentTableCode NOT IN (
	'GS', 'HA', 'OA', 'OC', 'OH', 'P1', 'PER'
)

DELETE FROM dbo.[PatternMatchingPhone]
WHERE PMP_ParentTableCode NOT IN (
	'GS', 'HA', 'OA', 'OC', 'OI', 'PER'
)

DELETE FROM dbo.[PatternMatchingRegCode]
WHERE PMR_ParentTableCode NOT IN (
	'GS', 'HA', 'OC', 'OK', 'PER', 'XZ'
)
";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
