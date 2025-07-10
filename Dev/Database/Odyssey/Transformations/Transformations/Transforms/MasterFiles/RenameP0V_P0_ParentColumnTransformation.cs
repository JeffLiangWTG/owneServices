using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;

public class RenameP0V_P0_ParentColumnTransformation : RenameColumnTransformation
{
	public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
	{
		get
		{
			yield return new RenameColumnTransformationInfo(ProcessTemplateValidationSchema.Constants.TableName, "P0V_P0_Parent", ProcessTemplateValidationSchema.Constants.P0V_P0_WorkflowTemplate);
		}
	}
}
