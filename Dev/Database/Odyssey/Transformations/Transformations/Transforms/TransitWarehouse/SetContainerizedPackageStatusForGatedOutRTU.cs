using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class SetContainerizedPackageStatusForGatedOutRTU : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Set WPS_Status of containerized package to DEP if its RTU is gated out";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE dbo.WhsItemPackageState
SET WPS_Status = 'DEP', WPS_SystemLastEditTimeUtc = GETDATE(), WPS_SystemLastEditUser = '~BP'
FROM dbo.WhsItemPackageState
JOIN dbo.PkgPackageExtension ON KPN_KP_Package = WPS_KP_Package
JOIN dbo.WhsItemReceiveTransportationUnit ON WRH_PK = KPN_ParentID
WHERE WRH_GateOutTime IS NOT NULL AND WPS_Status NOT IN ('DEP', 'FIN') AND KPN_ParentTableCode = 'WRH'
";

			Db.Connection.ExecuteNonQuery(sql);
		}

		#region ITransformationIndexProvider Members

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(WhsItemReceiveTransportationUnitSchema.Instance)
					.Key(WhsItemReceiveTransportationUnitSchema.Constants.WRH_GateOutTime)
					.Include(WhsItemReceiveTransportationUnitSchema.Constants.PK)
					.GetInfo();
				indexProvider.New(PkgPackageExtensionSchema.Instance)
					.Key(PkgPackageExtensionSchema.Constants.KPN_ParentTableCode)
					.Include(PkgPackageExtensionSchema.Constants.KPN_KP_Package)
					.Include(PkgPackageExtensionSchema.Constants.KPN_ParentID)
					.GetInfo();
				return indexProvider;
			}
		}

		#endregion
	}
}
