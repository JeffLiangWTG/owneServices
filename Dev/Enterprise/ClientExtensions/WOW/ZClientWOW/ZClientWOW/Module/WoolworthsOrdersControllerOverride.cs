using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Module;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrdersControllerOverride : OrdersController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WoolworthsOrder); }
		}

		protected override Enterprise.ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new WoolworthsOrdersForm((WoolworthsOrder)businessEntity);
		}
	}
}
