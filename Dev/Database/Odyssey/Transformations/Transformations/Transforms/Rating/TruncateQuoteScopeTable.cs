using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class TruncateQuoteScopeTable : DataTransformation
	{
		public override string UserDescription => "Truncate QuoteScope table to prepare for Unique Constraint";

		protected override void OfflinePreUpgradeTransform()
		{
			var query = $@"
            IF OBJECT_ID('dbo.QuoteScope', 'U') IS NOT NULL
            BEGIN
                TRUNCATE TABLE dbo.QuoteScope;
            END
            ";

			Db.Connection.ExecuteNonQuery(query);
		}
	}
}
