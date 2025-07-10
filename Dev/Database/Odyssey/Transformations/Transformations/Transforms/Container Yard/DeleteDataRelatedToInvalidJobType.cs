using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class DeleteDataRelatedToInvalidJobType : DataTransformation
	{
		public override string UserDescription => "Delete data with invalid Job Type";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
					DELETE FROM dbo.AccChargeBranchOverride WHERE YA_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccChargeComplianceDescription WHERE ADE_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccChargeCreditorOverride WHERE ACC_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccChargeGLPostingOverride WHERE Y1_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccChargeGovtChargeCodeOverride WHERE ACG_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccChargeRevRecOverride WHERE AE_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccChargeSupplyTypeOverride WHERE ACS_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccChargeTaxOverride WHERE AO_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.AccSurchargeApplication WHERE ASP_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.OrgARTerms WHERE PY_JobType IN ('CYI', 'CYO', 'CYS');

					DELETE FROM dbo.RatingDateConfig WHERE RDT_JobType IN ('CYI', 'CYO', 'CYS');";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
