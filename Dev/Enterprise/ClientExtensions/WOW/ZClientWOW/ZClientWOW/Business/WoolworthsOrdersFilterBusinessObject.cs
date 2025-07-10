using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrdersFilterBusinessObject : OrdersFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = base.GetModuleFiltersCore();
			collection.AddNumberFilter(WowConstants.OrderNumberFilterTypes.BuyerName, JobOrderHeaderSchema.JD_FirstBuyerContact);

			return collection;
		}
	}
}
