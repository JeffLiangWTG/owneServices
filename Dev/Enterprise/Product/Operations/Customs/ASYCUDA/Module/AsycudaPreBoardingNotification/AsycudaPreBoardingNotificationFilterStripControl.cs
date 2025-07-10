using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public partial class AsycudaPreBoardingNotificationFilterStripControl : ZFilterStripControl
	{
		public AsycudaPreBoardingNotificationFilterStripControl()
		{
			InitializeComponent();
		}

		public AsycudaPreBoardingNotificationFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
