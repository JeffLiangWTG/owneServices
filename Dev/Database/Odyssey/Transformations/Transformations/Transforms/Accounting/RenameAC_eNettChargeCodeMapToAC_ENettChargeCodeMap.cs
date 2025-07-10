using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class RenameAC_eNettChargeCodeMapToAC_ENettChargeCodeMap : RenameColumnTransformation
	{
		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get
			{
				yield return new RenameColumnTransformationInfo(AccChargeCodeSchema.Constants.TableName, "AC_eNettChargeCodeMap", AccChargeCodeSchema.Constants.AC_ENettChargeCodeMap);
			}
		}
	}
}
