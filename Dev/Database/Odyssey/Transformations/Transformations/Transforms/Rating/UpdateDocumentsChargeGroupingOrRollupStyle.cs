using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations;

public class UpdateDocumentsChargeGroupingOrRollupStyle : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Updating constraints for RCG_Style and updating values for ONF => O&F AND FND => F&D";

	protected override void OfflinePostUpgradeTransform()
	{
		var sqlText = @"
UPDATE dbo.RatingDocumentsChargeGroupingOrRollup
SET
	RCG_Style = 'O&F',
	RCG_SystemLastEditTimeUtc = GETUTCDATE(),
	RCG_SystemLastEditUser = '~BP'
WHERE RCG_Style = 'ONF'

UPDATE dbo.RatingDocumentsChargeGroupingOrRollup
SET
	RCG_Style = 'F&D',
	RCG_SystemLastEditTimeUtc = GETUTCDATE(),
	RCG_SystemLastEditUser = '~BP'
WHERE RCG_Style = 'FND'
";

		Db.Connection.ExecuteNonQuery(sqlText);
	}

	TransformationIndexProvider ITransformationIndexProvider.IndexProvider
	{
		get
		{
			var indexProvider = new TransformationIndexProvider(this);
			indexProvider.New(RatingDocumentsChargeGroupingOrRollupSchema.Instance)
				.Key(RatingDocumentsChargeGroupingOrRollupSchema.Constants.RCG_Style)
				.Where($"[{RatingDocumentsChargeGroupingOrRollupSchema.Constants.RCG_Style}] IN ('ONF', 'FND')")
				.GetInfo();
			return indexProvider;
		}
	}
}
