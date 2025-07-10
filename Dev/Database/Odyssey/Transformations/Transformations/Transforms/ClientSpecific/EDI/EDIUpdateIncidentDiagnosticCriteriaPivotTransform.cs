using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class EDIUpdateIncidentDiagnosticCriteriaPivotTransform : RenameColumnTransformation
	{
		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get {
				yield return new RenameColumnTransformationInfo("dbo", "IncidentDiagnosticCriteriaPivot", "IMV_IM_Incident", "IMV_ParentID");
			}
		}

		protected override void PostRenameTransformation()
		{
			base.PostRenameTransformation();
			if (DbObjectCreator.TableExists(Db.Connection, "IncidentDiagnosticCriteriaPivot"))
			{
				Db.Connection.ExecuteNonQuery(
					@"
IF COL_LENGTH('dbo.IncidentDiagnosticCriteriaPivot', 'IMV_ParentTableCode') IS NULL
	ALTER TABLE dbo.IncidentDiagnosticCriteriaPivot ADD IMV_ParentTableCode VARCHAR(3) NOT NULL DEFAULT 'IM';
ELSE
	EXECUTE('UPDATE dbo.IncidentDiagnosticCriteriaPivot
	SET
		IMV_ParentTableCode = ''IM'',
		IMV_SystemLastEditTimeUtc = GETUTCDATE(),
		IMV_SystemLastEditUser = ''E''
	WHERE
		IMV_ParentTableCode NOT IN (''IM'' , ''ING'')');
");
			}
		}
	}
}
