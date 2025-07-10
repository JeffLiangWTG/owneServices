using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5TransportMeansLayoutBuilder<T> : ColumnLayoutBuilder<T, Phase5TransportMeansControlBag> where T : CusInBondEvent
	{
		public override Phase5TransportMeansControlBag CommonBag => Phase5TransportMeansControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
