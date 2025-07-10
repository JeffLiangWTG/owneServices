using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	class RemoveRefCountryRecordsDoNotMatchCodeConstraint : DataTransformation
	{
		public override string UserDescription => "Remove RefCountry records that do not match RN_Code constraint and any referenced records from RefLocoMap";

		const string TransformationQuery = @"
			IF NOT EXISTS (SELECT * FROM sys.tables WHERE [name] = 'RefCountry')
			BEGIN
				RETURN
			END

			DROP TABLE IF EXISTS #RefCountryPksToDelete;

			SELECT RN_PK
			INTO	#RefCountryPksToDelete
			FROM [dbo].[RefCountry]
			WHERE LEN([RN_Code]) <> 2;

			IF NOT EXISTS (SELECT * FROM #RefCountryPksToDelete)
			BEGIN
				RETURN
			END

			IF EXISTS (SELECT * FROM sys.tables WHERE [name] = 'RefLocoMap')
			BEGIN
				DELETE [dbo].[RefLocoMap]
				WHERE RY_RN IN (SELECT RN_PK
									FROM #RefCountryPksToDelete);
			END

			DELETE [dbo].[RefCountry]
			WHERE RN_PK IN (SELECT RN_PK
								FROM #RefCountryPksToDelete);";

		protected override void OfflinePreUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(TransformationQuery);
		}
	}
}
