using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;

namespace Enterprise.Warehouse.Transit
{
	public interface IWhsItemConsignmentOrderReferenceProvider
	{
		IActiveBusinessObjectCollection<IWhsItemConsignmentOrderReference> WhsItemConsignmentOrderReferences { get; }
		ZGuid PK { get; }
		string TablePrefix { get; }
	}
}
