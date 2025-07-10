using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class EventLayoutBuilder<T> : ColumnLayoutBuilder<T, EventControlBag> where T : Business.NctsHeader
	{
		public override EventControlBag CommonBag => EventControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
