using System;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrdersModuleOverride : OrdersModule
	{
		public WoolworthsOrdersModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("Order Customs Declarations", new EventHandler(OnImportCustomsDeclGenerator_Click));
		}

		#region Button Clicks		

		void OnImportCustomsDeclGenerator_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new ContainerCustomsDeclGeneratorForm());
		}

		#endregion

		#region temporary, must delete after problem is resolved in production

		#endregion

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WoolworthsOrdersFilterBusinessObject();
		}
	}
}
