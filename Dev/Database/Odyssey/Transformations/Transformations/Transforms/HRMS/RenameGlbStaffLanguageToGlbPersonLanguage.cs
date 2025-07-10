using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.HRMS
{
	class RenameGlbStaffLanguageToGlbPersonLanguage : RenameTableTransformation
	{
		public override string UserDescription => "Replace GlbStaffLanguage table with GlbPersonLanguage";

		protected override IEnumerable<IRenameTableTransformationInfo> RenameTableInfoList => new[] { new RenameTableTransformationInfo("GlbStaffLanguage", "GlbPersonLanguage") };

		protected override void PostRenameTransformation()
		{
			if (DbObjectCreator.ColumnExists(Db.Connection, "GlbPersonLanguage", "G7_GS"))
			{
				new DbColumnDependencyRemover("GlbPersonLanguage", "G7_GS").DropRelateObjectsBeforeRenamingColumn(Db.Connection, "G7_PER_Person");
				_ = DbObjectCreator.RenameColumn(Db.Connection, "GlbPersonLanguage", "G7_GS", "G7_PER_Person");
				_ = Db.Connection.ExecuteNonQuery(ChangeFKFromStaffToPersonSql);
			}
		}

		string ChangeFKFromStaffToPersonSql => @$"
UPDATE
	dbo.GlbPersonLanguage
SET
	G7_PER_Person = s.GS_PER,
	G7_SystemLastEditTimeUtc = GETUTCDATE(),
	G7_SystemLastEditUser = '~BP'
FROM
	dbo.GlbPersonLanguage l
JOIN
	dbo.GlbStaff s
ON
	s.GS_PK = l.G7_PER_Person
;";
	}
}
