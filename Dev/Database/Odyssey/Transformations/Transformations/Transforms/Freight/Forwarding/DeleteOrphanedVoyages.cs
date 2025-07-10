using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class DeleteOrphanedVoyages : DataTransformation
	{
		public override string UserDescription => "Delete orphaned voyages.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DROP TABLE IF EXISTS #OrphanedJobVoyage
CREATE TABLE #OrphanedJobVoyage
(
		PK UNIQUEIDENTIFIER NOT NULL
)

INSERT INTO #OrphanedJobVoyage
SELECT JV_PK FROM dbo.JobVoyage
LEFT OUTER JOIN dbo.JobContainerMove ON JV_PK = E9_JV
LEFT OUTER JOIN dbo.JobVoyAccount ON JV_PK = NA_JV
WHERE E9_PK IS NULL AND NA_PK IS NULL AND NOT EXISTS (
	SELECT 1 FROM dbo.JobSailing
	INNER JOIN dbo.JobVoyOrigin ON JX_JA = JA_PK
	INNER JOIN dbo.JobVoyDestination ON JX_JB = JB_PK
	WHERE JA_JV = JV_PK OR JB_JV = JV_PK )

DELETE FROM dbo.JobTradeLaneVoyage WHERE NB_JV IN (SELECT PK FROM #OrphanedJobVoyage)
DELETE FROM dbo.JobVoyageExRate WHERE E8_JV IN (SELECT PK FROM #OrphanedJobVoyage)
DELETE FROM dbo.JobVoyCountry WHERE J0_JV IN (SELECT PK FROM #OrphanedJobVoyage)
DELETE FROM dbo.JobVoyOrigin WHERE JA_JV IN (SELECT PK FROM #OrphanedJobVoyage)
DELETE FROM dbo.JobVoyDestination WHERE JB_JV IN (SELECT PK FROM #OrphanedJobVoyage)
DELETE FROM dbo.JobVoyage WHERE JV_PK IN (SELECT PK FROM #OrphanedJobVoyage)

DROP TABLE #OrphanedJobVoyage";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
