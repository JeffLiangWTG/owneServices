using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	sealed class UpdateGoodsLocationCustomsOfficeWhereQualifierIsV : DataTransformation
	{
		public override string UserDescription => "Update CGL_CustomsOffice With values from CGL_AdditionalIdentifier when CGL_Qualifier = V.";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
			BEGIN TRY
				UPDATE dbo.CusGoodsLocation
				SET 
					CGL_CustomsOffice = LEFT(CGL_AdditionalIdentifier, 10),
					CGL_AdditionalIdentifier = '',
					CGL_SystemLastEditUser = '~BP',
					CGL_SystemLastEditTimeUtc = GetUtcDate()
				FROM dbo.CusGoodsLocation
				WHERE CGL_Qualifier ='V' AND CGL_CustomsOffice = '' AND CGL_AdditionalIdentifier <> '';
			END TRY
			BEGIN CATCH
				THROW;
			END CATCH";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
