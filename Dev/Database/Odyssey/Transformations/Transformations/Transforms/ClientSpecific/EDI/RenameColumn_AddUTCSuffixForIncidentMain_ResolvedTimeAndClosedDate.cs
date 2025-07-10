using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class RenameColumn_AddUTCSuffixForIncidentMain_ResolvedTimeAndClosedDate : RenameColumnTransformation
	{
		const string tableName = "IncidentMain";

		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get
			{
				yield return new RenameColumnTransformationInfo(tableName, "IM_ResolvedTime", "IM_ResolveTimeUtc");
				yield return new RenameColumnTransformationInfo(tableName, "IM_CloseDate", "IM_CloseTimeUtc");
			}
		}
	}
}
