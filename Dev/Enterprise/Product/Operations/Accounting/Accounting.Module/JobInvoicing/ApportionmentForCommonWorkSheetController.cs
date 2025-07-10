using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ApportionmentForCommonWorkSheetController : ApportionmentController
	{
		public ApportionmentForCommonWorkSheetController()
		{ }

		public override ControllerID ID
		{
			get { return ControllerIDs.ApportionmentForCommonWorkSheet; }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ApportionmentForCommonWorkSheetPlugin(businessEntity);
		}
	}
}
