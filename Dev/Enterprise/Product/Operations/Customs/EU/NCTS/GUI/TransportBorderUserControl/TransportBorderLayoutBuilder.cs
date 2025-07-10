using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class TransportBorderLayoutBuilder<T> : ColumnLayoutBuilder<T, TransportBorderControlBag> where T : Business.NctsDepartureMovementHeader
	{
		public override TransportBorderControlBag CommonBag => TransportBorderControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
