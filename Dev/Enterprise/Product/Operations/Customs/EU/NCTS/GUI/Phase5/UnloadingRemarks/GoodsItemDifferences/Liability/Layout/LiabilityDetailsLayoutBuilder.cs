using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class LiabilityDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, LiabilityDetailsControlBag> where T : Business.NctsArrivalCargoDesc
	{
		public override LiabilityDetailsControlBag CommonBag => LiabilityDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
