using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class UpdateOrgMiscServNegativeNumericValuesToZero : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set negative values to zero for non-negative OrgMiscServ Client Intel columns";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CICapitalEmployed)
					.Where("[OM_CICapitalEmployed]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CIEstimatedStaffThisCountry)
					.Where("[OM_CIEstimatedStaffThisCountry]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CIEstimatedStaffThisLocation)
					.Where("[OM_CIEstimatedStaffThisLocation]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CITurnover)
					.Where("[OM_CITurnover]<(0)")
					.GetInfo();

				indexProvider.New(OrgMiscServSchema.Instance)
					.Key(OrgMiscServSchema.Constants.OM_CIProfit)
					.Where("[OM_CIProfit]<(0)")
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var updateOrgMiscServNegativeNumericValues = @"
UPDATE dbo.OrgMiscServ
SET OM_CICapitalEmployed = 0,
OM_SystemLastEditUser = '~BP',
OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CICapitalEmployed < 0;
							
UPDATE dbo.OrgMiscServ
SET OM_CIEstimatedStaffThisCountry = 0,
OM_SystemLastEditUser = '~BP',
OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CIEstimatedStaffThisCountry < 0;

UPDATE dbo.OrgMiscServ
SET OM_CIEstimatedStaffThisLocation = 0,
OM_SystemLastEditUser = '~BP',
OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CIEstimatedStaffThisLocation < 0;

UPDATE dbo.OrgMiscServ
SET OM_CITurnover = 0,
OM_SystemLastEditUser = '~BP',
OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CITurnover < 0;

UPDATE dbo.OrgMiscServ
SET OM_CIProfit = 0,
OM_SystemLastEditUser = '~BP',
OM_SystemLastEditTimeUtc = GetUtcDate() 
WHERE OM_CIProfit < 0;
";
			Db.Connection.ExecuteNonQuery(updateOrgMiscServNegativeNumericValues);
		}
	}
}
