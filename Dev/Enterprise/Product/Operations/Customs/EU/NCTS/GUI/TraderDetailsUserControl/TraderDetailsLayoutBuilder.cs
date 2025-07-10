using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class TraderDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, TraderDetailsControlBag> where T : Business.NctsHeader
	{
		public override TraderDetailsControlBag CommonBag => TraderDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
