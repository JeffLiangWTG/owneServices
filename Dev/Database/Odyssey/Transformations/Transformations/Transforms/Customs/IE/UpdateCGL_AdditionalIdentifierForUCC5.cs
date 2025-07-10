using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.IE
{
	sealed class UpdateCGL_AdditionalIdentifierForUCC5 : DataTransformation
	{
		public override string UserDescription => "Move CGL_AdditionalIdentifier to CGL_CustomsOffice for UCC5 IE imports";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			const string sql = @"
;WITH CTE_NAME_JobDeclaration_UCC5Declarations as (
	SELECT JE_PK
	FROM dbo.JobDeclaration
	WHERE 1=1
	AND JE_ApplicationCode = 'V1'
	AND JE_MessageType = 'IMP'
)
, CTE_NAME_CusEntryInstruction_Ucc5Declarations as (
	SELECT CEI_PK
	FROM dbo.CusEntryInstruction
	WHERE CEI_JE IN (SELECT JE_PK FROM CTE_NAME_JobDeclaration_UCC5Declarations)
)
UPDATE dbo.CusGoodsLocation
SET
	CGL_CustomsOffice = CGL_AdditionalIdentifier
	,CGL_AdditionalIdentifier = ''
	,CGL_SystemLastEditTimeUtc = GETDATE()
	,CGL_SystemLastEditUser = '~BP'
WHERE 1 = 1
AND CGL_Qualifier = 'U'
AND CGL_AdditionalIdentifier <> ''
AND ( 1 = 0
  OR (CGL_ParentTableCode = 'CEI' AND CGL_ParentID IN (SELECT CEI_PK FROM CTE_NAME_CusEntryInstruction_Ucc5Declarations))
  OR (CGL_ParentTableCode = 'JE' AND CGL_ParentID IN (SELECT JE_PK FROM CTE_NAME_JobDeclaration_UCC5Declarations))
)";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
