using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public sealed class EUH7ItemDetailsControlLayoutBuilder<T> : ColumnLayoutBuilder<T, EUH7ItemDetailsCommonControlBag>
		where T : AsycudaPackedItem
	{
		public override EUH7ItemDetailsCommonControlBag CommonBag => EUH7ItemDetailsCommonControlBag.Instance;

		protected override int MaxColumns => 2;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

		public override bool NarrowColumnForMediumControls => true;
	}
}
