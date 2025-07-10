using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ
{
	class UpdateJEContainerModeForNZ : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update NZ declaration with wrong containerMode due to WI00799676.";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobDeclarationSchema.Instance)
					.Key(JobDeclarationSchema.JE_ContainerMode.Name)
					.Include(JobDeclarationSchema.PK.Name, JobDeclarationSchema.JE_JS.Name, JobDeclarationSchema.JE_OverrideFreightDefaults.Name, JobDeclarationSchema.JE_SystemLastEditUser.Name, JobDeclarationSchema.JE_SystemLastEditTimeUtc.Name)
					.Where("[JE_DataModel]='NZ'").GetInfo();

				return indexProvider;
			}
		}

		const string sql = @"
BEGIN TRY
	UPDATE
		Declaration
	SET
		JE_ContainerMode ='CNT',
		JE_SystemLastEditTimeUtc = GetUtcDate(),
		JE_SystemLastEditUser = '~BP'
	FROM
		dbo.JobDeclaration Declaration
	WHERE JE_DataModel = 'NZ'
	And (JE_JS IS NULL OR JE_OverrideFreightDefaults = 1)
	AND JE_PK IN (select CO_JE from dbo.CusContainer where CO_JE = JE_PK)
	AND JE_ContainerMode <> 'CNT'

	UPDATE
		Declaration
	SET
		JE_ContainerMode = '',
		JE_SystemLastEditTimeUtc = GetUtcDate(),
		JE_SystemLastEditUser = '~BP'
	FROM
		dbo.JobDeclaration Declaration
	WHERE JE_DataModel = 'NZ'
	And (JE_JS IS NULL OR JE_OverrideFreightDefaults = 1)
	AND JE_PK NOT IN (select CO_JE from dbo.CusContainer where CO_JE = JE_PK)
	AND JE_ContainerMode <> ''
END TRY
BEGIN CATCH
	THROW;
END CATCH";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
