using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class SetDefaultCrmOpportunityFrequencyUnit : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set default opportunity overall Frequency unit to ('Y')";

		#region ITransformationIndexProvider.IndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CrmOpportunitySchema.Instance)
					.Key(CrmOpportunitySchema.Constants.COP_OverallFrequencyUnit)
					.Where("[COP_OverallFrequencyUnit]=''")
					.GetInfo();

				return indexProvider;
			}
		}

		#endregion

		protected override void OfflinePostUpgradeTransform()
		{
			var setDefaultFrequencyUnit = @"
UPDATE dbo.CrmOpportunity
SET COP_OverallFrequencyUnit = 'Y',
COP_SystemLastEditUser = '~BP',
COP_SystemLastEditTimeUtc = GetUtcDate() 
WHERE COP_OverallFrequencyUnit = '';
";
			Db.Connection.ExecuteNonQuery(setDefaultFrequencyUnit);
		}
	}
}
