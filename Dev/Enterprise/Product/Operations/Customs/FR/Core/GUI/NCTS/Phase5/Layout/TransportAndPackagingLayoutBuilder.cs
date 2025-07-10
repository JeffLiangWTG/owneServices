using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class TransportAndPackagingLayoutBuilder<T> : ColumnLayoutBuilder<T, TransportAndPackagingControlBag> where T : Business.NCTS.NctsDepartureMovementHeader
	{
		public override TransportAndPackagingControlBag CommonBag => TransportAndPackagingControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
