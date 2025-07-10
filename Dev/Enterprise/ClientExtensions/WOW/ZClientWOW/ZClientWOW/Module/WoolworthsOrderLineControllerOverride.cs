
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Module;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrderLineControllerOverride : OrderLineController
	{
		protected override Enterprise.ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			WoolworthsOrderLine line = (WoolworthsOrderLine)businessEntity;
			if (line.Order == null)
			{
				WoolworthsOrder order = line.Factory.New<WoolworthsOrder>();
				line.JO_JD = order.PK;
			}
			return new WoolworthsOrderLineForm((WoolworthsOrderLine)businessEntity);
		}
	}
}
