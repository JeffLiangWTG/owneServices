using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ApportionmentForTransitTransportationUnitController : ApportionmentController
	{
		public ApportionmentForTransitTransportationUnitController()
		{
		}

		public override ControllerID ID => ControllerIDs.ApportionmentForTransitTransportationUnit;

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new ApportionmentForTransitTransportationUnitPlugin(businessEntity);
	}
}
