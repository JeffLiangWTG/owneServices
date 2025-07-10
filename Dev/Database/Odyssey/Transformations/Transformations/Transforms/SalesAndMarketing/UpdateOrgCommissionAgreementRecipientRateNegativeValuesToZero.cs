using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class UpdateOrgCommissionAgreementRecipientRateNegativeValuesToZero : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set negative values to zero for non-negative OrgCommissionAgreementRecipientRate columns";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(OrgCommissionAgreementRecipientRateSchema.Instance)
					.Key(OrgCommissionAgreementRecipientRateSchema.Constants.CAT_CommissionAmount)
					.Where("[CAT_CommissionAmount]<(0)")
					.GetInfo();

				indexProvider.New(OrgCommissionAgreementRecipientRateSchema.Instance)
					.Key(OrgCommissionAgreementRecipientRateSchema.Constants.CAT_CommissionPercentage)
					.Where("[CAT_CommissionPercentage]<(0)")
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var updateOrgCommissionAgreementRecipientRateNegativeValuesToZero = @"
UPDATE dbo.OrgCommissionAgreementRecipientRate
SET CAT_CommissionAmount = 0,
CAT_SystemLastEditUser = '~BP',
CAT_SystemLastEditTimeUtc = GetUtcDate() 
WHERE CAT_CommissionAmount < 0;
							
UPDATE dbo.OrgCommissionAgreementRecipientRate
SET CAT_CommissionPercentage = 0,
CAT_SystemLastEditUser = '~BP',
CAT_SystemLastEditTimeUtc = GetUtcDate() 
WHERE CAT_CommissionPercentage < 0;
";
			Db.Connection.ExecuteNonQuery(updateOrgCommissionAgreementRecipientRateNegativeValuesToZero);
		}
	}
}
