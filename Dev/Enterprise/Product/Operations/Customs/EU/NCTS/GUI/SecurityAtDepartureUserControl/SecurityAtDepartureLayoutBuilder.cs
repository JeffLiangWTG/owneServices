using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class SecurityAtDepartureLayoutBuilder<T> : ColumnLayoutBuilder<T, SecurityAtDepartureControlBag> where T : Business.NctsHeader
	{
		public override SecurityAtDepartureControlBag CommonBag => SecurityAtDepartureControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
