
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLineFromOrderControllerOverride : OrderLineFromOrderController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WoolworthsOrderLineForm((WoolworthsOrderLine)businessEntity);
		}
	}
}
