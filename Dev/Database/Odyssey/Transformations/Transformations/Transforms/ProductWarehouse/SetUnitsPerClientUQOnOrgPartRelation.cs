using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	class SetUnitsPerClientUQOnOrgPartRelation : DataTransformation
	{
		public override string UserDescription => "Set OU_UnitsPerClientUQ on all OrgPartRelation";

		const string StoredFromPKName = "SetUnitsPerClientUQOnOrgPartRelation.FromPK";
		const string StoredTotalUpdatedCount = "SetUnitsPerClientUQOnOrgPartRelation.TotalUpdatedCount";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			// No-op transform to allow old builds to complete the old (slow and defective) transform immediately, so they can upgrade to a new build with the improved transform.
			// Deleting the transform entirely will block the upgrade to the new version until the removed transform finishes.
			// The new transform is in SetUnitsPerClientUQOnOrgPartRelationV2.cs, makes sense to clear the original transform's ext properties here.
			ExtProperty.Database.Delete(Db.Connection, StoredFromPKName);
			ExtProperty.Database.Delete(Db.Connection, StoredTotalUpdatedCount);
		}
	}
}
