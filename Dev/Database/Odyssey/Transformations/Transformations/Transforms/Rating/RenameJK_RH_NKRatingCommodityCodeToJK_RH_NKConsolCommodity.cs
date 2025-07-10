using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class RenameJK_RH_NKRatingCommodityCodeToJK_RH_NKConsolCommodity : RenameColumnTransformation
	{
		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get
			{
				yield return new RenameColumnTransformationInfo(JobConsolSchema.Constants.TableName, "JK_RH_NKRatingCommodityCode", JobConsolSchema.Constants.JK_RH_NKConsolCommodity);
			}
		}
	}
}
