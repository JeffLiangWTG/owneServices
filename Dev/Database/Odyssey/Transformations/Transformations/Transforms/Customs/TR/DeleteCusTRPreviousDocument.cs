using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TR
{
	internal class DeleteCusTRPreviousDocument : DataTransformation
	{
		public override string UserDescription => "Delete CusTRPreviousDocument dirty data,  table link has changed";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, "CusTRPreviousDocument"))
			{
				var sql = FormattableString.Invariant($@"
BEGIN TRY
    DELETE FROM dbo.CusTRPreviousDocumentItem;
    DELETE FROM dbo.CusTRPreviousDocument; 
END TRY
BEGIN CATCH 
THROW;
END CATCH;
");
				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
