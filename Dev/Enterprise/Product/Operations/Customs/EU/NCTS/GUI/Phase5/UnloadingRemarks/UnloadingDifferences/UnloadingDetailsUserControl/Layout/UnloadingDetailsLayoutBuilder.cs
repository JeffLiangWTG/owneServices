using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class UnloadingDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, UnloadingDetailsControlBag> where T : Business.NctsArrivalMovementHeader
	{
		public override UnloadingDetailsControlBag CommonBag => UnloadingDetailsControlBag.Instance;

		protected override int MaxColumns => 1;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
