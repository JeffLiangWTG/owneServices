using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public class ApportionmentForGatewayPlugin : ApportionmentPlugin
	{
		public ApportionmentForGatewayPlugin(IBusiness hostEntity)
			: base(hostEntity)
		{ }

		protected override void RefreshGatewayElementsCore(bool isPlugInEnabled)
		{
			// no need to setup menu, just need to enable the tab if isPlugInEnabled is true.
			Enabled = isPlugInEnabled;
		}

		public override bool CheckIsUsedForGatewayApportionments()
		{
			return true;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			// We don't need a top level menu which targets the GW sell apportionment tab. i.e. the SellApportionmentForGateway plugin.
			return null;
		}

		public override string Name
		{
			get { return ResString.GetMultilingualString("6B418B14-3586-4E4F-AEA9-34FBF25D16EB", "Gateway Sell Apportionment"); }
		}
	}
}
