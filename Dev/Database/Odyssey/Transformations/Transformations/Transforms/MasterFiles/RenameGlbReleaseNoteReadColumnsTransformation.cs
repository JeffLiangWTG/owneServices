using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;

public class RenameGlbReleaseNoteReadColumnsTransformation : RenameColumnTransformation
{
	public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
	{
		get
		{
			yield return new RenameColumnTransformationInfo(GlbReleaseNoteReadSchema.Constants.TableName, "GR_GS", GlbReleaseNoteReadSchema.Constants.GR_GS_Staff);
			yield return new RenameColumnTransformationInfo(GlbReleaseNoteReadSchema.Constants.TableName, "GR_GF", GlbReleaseNoteReadSchema.Constants.GR_ReleaseNoteID);
		}
	}
}
