using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.JP
{
	sealed class MigrateJobDeclarationFieldsToCusEntryInstruction : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => $"Migrate JobDeclaration Fields To CusEntryInstruction For JP";

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
				using (DisableTriggers())
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
	CEI_Style = Trim(JE_MessageSubType),
	CEI_SubStyle = CASE WHEN LEN(DeclarationSubType.Value) = 1 THEN DeclarationSubType.Value ELSE CEI_SubStyle END,
	CEI_AddInfo =
	CASE WHEN LEN(CargoType.Value) > 0 AND LEN(DeclarationCargoType.Value) = 0 THEN
		CASE WHEN LEN(CEI_AddInfo) > 0 THEN
			CONCAT(CEI_AddInfo, '*', 'DeclarationCargoType=', CargoType.Value)
		ELSE 
			CONCAT('DeclarationCargoType=', CargoType.Value)
		END
	ELSE
		CEI_AddInfo
	END,
	CEI_SystemLastEditTimeUtc = GETDATE(),
	CEI_SystemLastEditUser = '~BP',
	CEI_AutoVersion = (CEI_AutoVersion + 1) % 32768
FROM
	dbo.Jobdeclaration INNER JOIN dbo.CusEntryInstruction on JE_ClusterKey = CEI_ClusterKey AND JE_PK = CEI_JE
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'CargoType') as CargoType
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'DeclarationSubType') as DeclarationSubType
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(CEI_AddInfo, 'DeclarationCargoType') as DeclarationCargoType
WHERE
	JE_dataModel = 'JP';
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
	CEI_Style,
	CEI_SubStyle,
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
	Trim(JE_MessageSubType),
	CASE WHEN LEN(DeclarationSubType.Value) = 1 THEN DeclarationSubType.Value ELSE '' END,
	CASE WHEN LEN(CargoType.Value) > 0 THEN CONCAT('DeclarationCargoType=', CargoType.Value) ELSE '' END,
	1,
	GETUTCDATE(),
	'~BP',
	GETUTCDATE(),
	'~BP'
FROM
	dbo.Jobdeclaration
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'CargoType') as CargoType
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'DeclarationSubType') as DeclarationSubType
WHERE
	JE_dataModel = 'JP' AND NOT EXISTS(SELECT 1 FROM CusEntryInstruction WHERE JE_ClusterKey = CEI_ClusterKey AND JE_PK = CEI_JE);
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		IDisposable DisableTriggers()
		{
			var triggerName = "TG_CusEntryInstruction_UpdateAutoVersion";
			var tableName = CusEntryInstructionSchema.Constants.TableName;

			var connection = Db.Connection;
			Db.Connection.ExecuteNonQuery($@"
			IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{triggerName}')
			BEGIN
				DISABLE TRIGGER {triggerName} ON dbo.{tableName}
			END");

			return new DisposableAction(() =>
			{
				Db.Connection.ExecuteNonQuery($@"
				IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{triggerName}')
				BEGIN
					ENABLE TRIGGER {triggerName} ON dbo.{tableName}
				END");
			});
		}
	}
}
