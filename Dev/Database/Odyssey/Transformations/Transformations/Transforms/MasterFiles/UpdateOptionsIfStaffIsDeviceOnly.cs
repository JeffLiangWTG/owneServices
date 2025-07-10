using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	public class UpdateOptionsIfStaffIsDeviceOnly : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update some columns to false if staff is device only.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @$"
BEGIN TRY
	UPDATE dbo.GlbStaff SET GS_IsController = 0, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = '~BP' WHERE GS_IsDevice = 1;
	DELETE FROM dbo.GlbGroupLink WHERE GK_GG IN ('a99e7f0e-8379-4f50-8560-9b4bb804c0de', '6f0eb310-fc5c-4696-9594-f8ce156542c6', '208068b6-3383-44bf-8e0d-dbd827f9d675') AND GK_GS IN (SELECT GS_PK FROM dbo.GlbStaff WHERE GS_IsDevice = 1);
END TRY    
BEGIN CATCH                
	THROW
END CATCH
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(GlbStaffSchema.Instance)
					.Key(GlbStaffSchema.Constants.GS_IsDevice)
					.Include(GlbStaffSchema.Constants.PK, GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc, GlbStaffSchema.Constants.GS_SystemLastEditUser)
					.Where("[GS_IsDevice]=(1)")
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
