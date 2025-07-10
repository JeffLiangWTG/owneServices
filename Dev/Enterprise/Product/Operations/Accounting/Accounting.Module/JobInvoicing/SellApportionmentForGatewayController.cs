using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class SellApportionmentForGatewayController : ApportionmentController
	{
		public SellApportionmentForGatewayController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.SellApportionmentForGateway; }
		}

		#region Implementation

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("PlugInTabPage|SellApportionmentForGateway", "Gateway Sell Apportionment", "The Gateway Sell Apportionment tab."); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.JobInvoicing.ConsolCosting.ApportionmentForGatewayPlugin(businessEntity);
		}

		#endregion
	}
}
