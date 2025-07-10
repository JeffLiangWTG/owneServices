using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.JP
{
	sealed class MoveJE_ValueTypeToCEI_ValueType : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Move JE_ValueType To CEI_ValueType";

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				if (DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName) && Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'JP'"))
				{
					indexProvider.New(JobDeclarationSchema.Instance)
					.Key(JobDeclarationSchema.Constants.JE_DataModel)
					.Where($"[JE_DataModel]='JP'")
					.GetInfo();
				}
				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'JP'"))
			{
				using (DataTransformationHelper.SuspendTriggerIfExists("TG_CusEntryInstruction_UpdateAutoVersion", CusEntryInstructionSchema.Constants.TableName))
				{
					UpdateCusEntryInstruction();
					InsertCusEntryInstruction();
				}
			}
		}

		void UpdateCusEntryInstruction()
		{
			var sql = $@"
UPDATE
	dbo.CusEntryInstruction
SET
	CEI_AddInfo =
		CASE WHEN LEN(CEI_AddInfo) > 0 THEN
			CONCAT(CEI_AddInfo, '*', 'ValueType=', DeclarationValueType.Value)
		ELSE
			CONCAT('ValueType=', DeclarationValueType.Value)
		END,
	CEI_SystemLastEditTimeUtc = GETUTCDATE(),
	CEI_SystemLastEditUser = '~BP',
	CEI_AutoVersion = (CEI_AutoVersion + 1) % 32768
FROM
	dbo.Jobdeclaration INNER JOIN dbo.CusEntryInstruction on JE_ClusterKey = CEI_ClusterKey AND JE_PK = CEI_JE
CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'ValueType') as DeclarationValueType
CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(CEI_AddInfo, 'ValueType') as ValueType
WHERE
	JE_dataModel = 'JP' AND
	CEI_DataModel = 'JP' AND
	LEN(DeclarationValueType.Value) > 0 AND LEN(ValueType.Value) = 0;
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		void InsertCusEntryInstruction()
		{
			var sql = $@"
INSERT INTO dbo.CusEntryInstruction
(
	CEI_PK,
	CEI_DataModel,
	CEI_ClusterKey,
	CEI_JE,
	CEI_AddInfo,
	CEI_DisplaySequence,
	CEI_SystemCreateTimeUtc,
	CEI_SystemCreateUser,
	CEI_SystemLastEditTimeUtc,
	CEI_SystemLastEditUser
)
SELECT
	NEWID(),
	'JP' ,
	JE_ClusterKey,
	JE_PK,
	CONCAT('ValueType=', DeclarationValueType.Value),
	1,
	GETUTCDATE(),
	'~BP',
	GETUTCDATE(),
	'~BP'
FROM
	dbo.Jobdeclaration
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'ValueType') as DeclarationValueType
WHERE
	JE_dataModel = 'JP' AND LEN(DeclarationValueType.Value) > 0 AND NOT EXISTS(SELECT 1 FROM CusEntryInstruction WHERE JE_ClusterKey = CEI_ClusterKey AND JE_PK = CEI_JE);
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
