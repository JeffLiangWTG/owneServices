using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.LandTransport
{
	class RemoveEquipmentItemsWithInvalidParentTableCode : DataTransformation
	{
		public override string UserDescription => "Removing EquipmentItem With Invalid Parent Table Code.";

		protected override void OnlinePreUpgradeTransform()
		{
			base.OnlinePreUpgradeTransform();

			if (!DbObjectCreator.TableExists(Db.Connection, "DtbEquipmentItem"))
			{
				return;
			}

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.DtbEquipmentItem WHERE LTE_ParentTableCode <> 'KG';");
		}
	}
}
