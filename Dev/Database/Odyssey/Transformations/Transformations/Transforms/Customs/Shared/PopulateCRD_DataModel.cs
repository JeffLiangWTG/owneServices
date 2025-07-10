using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class PopulateCRD_DataModel : DataTransformation
	{
		public override string UserDescription => "Populate new column CRD_DataModel";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
UPDATE dbo.CusReconDeclaration
SET
CRD_DataModel = GC_RN_NKCountryCode,
CRD_SystemLastEditUser = '~BP',
CRD_SystemLastEditTimeUtc = GetUtcDate()
FROM dbo.CusReconDeclaration
INNER JOIN GlbBranch on GB_PK= CRD_GB_Branch
INNER JOIN GlbCompany on GC_PK = GB_GC");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
