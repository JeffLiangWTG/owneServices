using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class AddConstraintToRateEntryToAndFromSuburb : DataTransformation
	{
		public override string UserDescription => "Ensure columns TI_R9_FromSuburb and TI_R9_ToSuburb meet corresponding foreign key constraint";

		public override bool IsRequired => base.IsRequired &&
				(DbObjectCreator.ColumnExists(Db.Connection, RateEntrySchema.Constants.TableName, "TI_R9_FromSuburb") ||
				DbObjectCreator.ColumnExists(Db.Connection, RateEntrySchema.Constants.TableName, "TI_FromId")) &&
				(DbObjectCreator.ColumnExists(Db.Connection, RateEntrySchema.Constants.TableName, "TI_R9_ToSuburb") ||
				DbObjectCreator.ColumnExists(Db.Connection, RateEntrySchema.Constants.TableName, "TI_ToId"));

		protected override void OfflinePreUpgradeTransform()
		{
			string fromColumn = DbObjectCreator.ColumnExists(Db.Connection, RateEntrySchema.Constants.TableName, "TI_R9_FromSuburb") ? "TI_R9_FromSuburb" : "TI_FromId";
			string toColumn = DbObjectCreator.ColumnExists(Db.Connection, RateEntrySchema.Constants.TableName, "TI_R9_ToSuburb") ? "TI_R9_ToSuburb" : "TI_ToId";

			var query = $@"
IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TG_CheckNoRateEntryOverlaps')
BEGIN
    DISABLE TRIGGER TG_CheckNoRateEntryOverlaps ON dbo.RateEntry;
END

UPDATE dbo.RateEntry
SET 
	{fromColumn} = NULL,
	TI_SystemLastEditTimeUtc = GetUtcDate(),
	TI_SystemLastEditUser = '~BP'
WHERE 
	{fromColumn} IS NOT NULL AND 
	{fromColumn} NOT IN (SELECT R9_PK FROM RefCityTown);

UPDATE dbo.RateEntry
SET 
	{toColumn} = NULL,
	TI_SystemLastEditTimeUtc = GetUtcDate(),
	TI_SystemLastEditUser = '~BP'
WHERE 
	{toColumn} IS NOT NULL AND 
	{toColumn} NOT IN (SELECT R9_PK FROM RefCityTown);

IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TG_CheckNoRateEntryOverlaps')
BEGIN
    ENABLE TRIGGER TG_CheckNoRateEntryOverlaps ON dbo.RateEntry;
END
";

			Db.Connection.ExecuteNonQuery(query);
		}
	}
}
