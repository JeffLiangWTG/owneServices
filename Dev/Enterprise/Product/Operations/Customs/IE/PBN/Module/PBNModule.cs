using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.PBN.Module
{
	public class PBNModule : AsycudaPreBoardingNotificationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new PBNFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new PBNFilterStripControl(GridCollection, FilterBusinessObject);
	}
}
