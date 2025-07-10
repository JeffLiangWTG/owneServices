using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class RenameAUP_UniversalChargeCodeNameColumnTransformation : RenameColumnTransformation
	{
		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get
			{
				yield return new RenameColumnTransformationInfo(AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, "AUP_UniversalChargeCode", AccChargeCodeUniversalCodeMappingSchema.Constants.AUP_Code);
			}
		}
	}
}
