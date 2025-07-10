using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class TransportAndPackagingLayoutBuilder<T> : ColumnLayoutBuilder<T, TransportAndPackagingControlBag> where T : Business.NctsDepartureMovementHeader
	{
		public override TransportAndPackagingControlBag CommonBag => TransportAndPackagingControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
