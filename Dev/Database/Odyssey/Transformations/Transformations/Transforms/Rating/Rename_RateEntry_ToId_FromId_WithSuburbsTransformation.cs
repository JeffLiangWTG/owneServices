using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating
{
	public class Rename_RateEntry_ToId_FromId_WithSuburbsTransformation : RenameColumnTransformation
	{
		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get
			{
				yield return new RenameColumnTransformationInfo(RateEntrySchema.Constants.TableName, "TI_FromID", RateEntrySchema.Constants.TI_R9_FromSuburb);
				yield return new RenameColumnTransformationInfo(RateEntrySchema.Constants.TableName, "TI_ToId", RateEntrySchema.Constants.TI_R9_ToSuburb);
			}
		}
	}
}
