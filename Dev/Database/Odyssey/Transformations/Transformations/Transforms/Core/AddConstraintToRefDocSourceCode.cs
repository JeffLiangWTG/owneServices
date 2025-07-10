using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class AddConstraintToRefDocSourceCode : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraint on column RDS_Code";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
				DELETE FROM dbo.RefDocSource
				WHERE RDS_Code = '';
			");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
