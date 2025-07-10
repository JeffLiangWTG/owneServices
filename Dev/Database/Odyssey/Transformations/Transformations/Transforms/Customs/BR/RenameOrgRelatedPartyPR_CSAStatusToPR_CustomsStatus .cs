using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR
{
	public class RenameOrgRelatedPartyPR_CSAStatusToPR_CustomsStatus : RenameColumnTransformation
	{
		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get
			{
				yield return new RenameColumnTransformationInfo(OrgRelatedPartySchema.Constants.TableName, "PR_CSAStatus", OrgRelatedPartySchema.Constants.PR_CustomsStatus);
			}
		}
	}
}
