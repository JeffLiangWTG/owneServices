using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ
{
	sealed class UpdateContainerModeForNZAirJobDeclaration : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update JE_ContainerMode For NZ Air JobDeclaration";

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				if (DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName) && Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'NZ'"))
				{
					indexProvider.New("dbo", "JobDeclaration")
					.Key(JobDeclarationSchema.Constants.JE_JS, JobDeclarationSchema.Constants.JE_TransportMode, JobDeclarationSchema.Constants.JE_ContainerMode, JobDeclarationSchema.Constants.JE_DataModel)
					.Include(JobDeclarationSchema.Constants.JE_SystemLastEditTimeUtc, JobDeclarationSchema.Constants.JE_SystemLastEditUser)
					.Where("[JE_JS] IS NULL AND [JE_TransportMode]='AIR' AND [JE_ContainerMode]<>'' AND [JE_DataModel]='NZ'")
					.GetInfo();
				}
				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'NZ'"))
			{
				var script = @"
				UPDATE
					dbo.JobDeclaration
				SET
					JE_ContainerMode = '',
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE
					JE_JS IS NULL
					AND JE_TransportMode = 'AIR'
					AND JE_ContainerMode <> ''
					AND JE_DataModel = 'NZ'
				";

				Db.Connection.ExecuteNonQuery(script);
			}
		}
	}
}
