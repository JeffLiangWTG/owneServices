using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class UpdateOrgMiscServCMNegativeNumericValuesToZero : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set negative values to zero for non-negative OrgMiscServ CM columns";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMAcheivableClientRevenue)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMAcheivableClientRevenue]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMConsultingRevenue)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMConsultingRevenue]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMEstimatedProfit)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMEstimatedProfit]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMNoOfEmployees)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMNoOfEmployees]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMPaidUpCapital)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMPaidUpCapital]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMPercentage)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMPercentage]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMTotalClientRevenue)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMTotalClientRevenue]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CMWarehouseRevenue)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditUser)
					.Include(OrgMiscServSchema.Constants.OM_SystemLastEditTimeUtc)
					.Where("[OM_CMWarehouseRevenue]<(0)")
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var updateOrgMiscServCMNegativeNumericValues = $@"
BEGIN TRY
UPDATE dbo.OrgMiscServ
SET
	OM_CMAcheivableClientRevenue = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMAcheivableClientRevenue < 0;
							
UPDATE dbo.OrgMiscServ
SET
	OM_CMConsultingRevenue = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMConsultingRevenue < 0;

UPDATE dbo.OrgMiscServ
SET
	OM_CMEstimatedProfit = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMEstimatedProfit < 0;

UPDATE dbo.OrgMiscServ
SET
	OM_CMNoOfEmployees = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMNoOfEmployees < 0;

UPDATE dbo.OrgMiscServ
SET
	OM_CMPaidUpCapital = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMPaidUpCapital < 0;

UPDATE dbo.OrgMiscServ
SET
	OM_CMPercentage = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMPercentage < 0;

UPDATE dbo.OrgMiscServ
SET
	OM_CMTotalClientRevenue = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMTotalClientRevenue < 0;

UPDATE dbo.OrgMiscServ
SET
	OM_CMWarehouseRevenue = 0,
	OM_SystemLastEditUser = '~BP',
	OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CMWarehouseRevenue < 0;
END TRY
BEGIN CATCH
	THROW
END CATCH
";
			Db.Connection.ExecuteNonQuery(updateOrgMiscServCMNegativeNumericValues);
		}
	}
}
