using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.ES
{
	sealed class PopulateCVH_BrandNameAndCVH_ModelName : DataTransformation
	{
		public override string UserDescription => "Change the field JobComInvoiceLine.JI_BrandName to CusVehicle.CVH_BrandName and JobComInvoiceLine.JI_Model to CusVehicle.CVH_ModelName.";

		protected override void OfflinePostUpgradeTransform()
		{
			var anyCompanyES = Db.Connection.ExecuteScalar<int>("select count(*) from dbo.GlbCompany where GC_RN_NKCountryCode = 'ES';");
			if (anyCompanyES > 0)
			{
				const string sql = @"
				BEGIN TRY
					UPDATE dbo.CusVehicle
					SET
						CVH_BrandName = Li.JI_BrandName,
						CVH_ModelName = Li.JI_Model,
						CVH_SystemLastEditUser = '~BP',
						CVH_SystemLastEditTimeUtc = GetUtcDate()
					FROM dbo.CusVehicle
					INNER JOIN dbo.JobComInvoiceLine Li WITH (FORCESEEK)
					ON CVH_ParentTableCode = 'JI' and Li.JI_PK = CVH_ParentID and Li.JI_DataModel = 'ES' and (Li.JI_BrandName != '' or Li.JI_Model != '');
				END TRY
				BEGIN CATCH
					THROW;
				END CATCH";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
