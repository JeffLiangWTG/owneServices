using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class IncidentDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, IncidentDetailsControlBag> where T : Business.EnRouteIncident
	{
		public override IncidentDetailsControlBag CommonBag => IncidentDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
