using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class VehicleCollectionProvider : CollectionProvider
	{
		public VehicleCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new RefEquipmentCollection(BusinessObjectFactory, AdditionalQuery);
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new RefEquipmentCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefEquipment))) { AdditionalFilter = AdditionalQuery };
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefEquipment;

		ZQuery AdditionalQuery
		{
			get
			{
				if (additionalQuery == null)
				{
					additionalQuery = new ZQuery();
					additionalQuery.AddToFilter(JoinCondition.And, RefEquipmentSchema.RQ_IsVehicle, ZBool.True);
				}
				return additionalQuery;
			}
		}
		ZQuery additionalQuery;
	}
}
