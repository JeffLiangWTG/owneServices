using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class UpdateEmptyCompanyNameToPlaceholderValueAndMakeInactive : DataTransformation
	{
		public override string UserDescription => "Update empty company name to placeholder value and make inactive";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName)
				 && DbObjectCreator.ColumnExists(Db.Connection, GlbCompanySchema.Constants.TableName, GlbCompanySchema.Constants.GC_Name))
			{
				var sql = FormattableString.Invariant($@"
UPDATE dbo.GlbCompany
SET
	GC_Name = '** Name not provided **',
	GC_IsActive = 0,
	GC_SystemLastEditTimeUtc = GetUtcDate(),
	GC_SystemLastEditUser = '~BP'
WHERE GC_Name = '';
			");

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
