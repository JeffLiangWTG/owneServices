using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class MoveExRateTypeDataFromAccJobConfigToAccJobConfigPivot : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Copy the Exchange Rate Type from AccJobConfig to AccJobConfigPivot";

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New("dbo", "AccJobConfig")
					.Key("JCF_ConfigType")
					.Where("[JCF_ConfigType]='ERT'")
					.GetInfo();

				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = $@"
MERGE dbo.AccJobConfigPivot AS target
USING (
    SELECT JCF_PK, JCF_Code
    FROM dbo.AccJobConfig
    WHERE JCF_ConfigType = 'ERT'
) AS source
ON target.JCT_JCF_JobConfig = source.JCF_PK
WHEN MATCHED THEN
    UPDATE SET 
        target.JCT_ExRateType = source.JCF_Code,
        target.JCT_SystemLastEditTimeUtc = GETUTCDATE(),
        target.JCT_SystemLastEditUser = '~BP'
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
        JCT_PK, 
        JCT_JCF_JobConfig, 
        JCT_ExRateType, 
        JCT_Code, 
        JCT_ParentId, 
        JCT_ParentTableCode, 
        JCT_SystemCreateTimeUtc, 
        JCT_SystemLastEditTimeUtc, 
        JCT_SystemCreateUser, 
        JCT_SystemLastEditUser
    )
    VALUES (
        NEWID(),
        source.JCF_PK, 
        source.JCF_Code,
        '',
        NULL,
        '',
        GETUTCDATE(),  
        GETUTCDATE(),
        '~BP',
        '~BP'
    );
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
