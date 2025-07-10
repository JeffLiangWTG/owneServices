using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public sealed class EUH7PackDetailsControlLayoutBuilder<T> : ColumnLayoutBuilder<T, EUH7PackDetailsControlBag>
		where T : AsycudaPack
	{
		public override EUH7PackDetailsControlBag CommonBag => EUH7PackDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;
	}
}
