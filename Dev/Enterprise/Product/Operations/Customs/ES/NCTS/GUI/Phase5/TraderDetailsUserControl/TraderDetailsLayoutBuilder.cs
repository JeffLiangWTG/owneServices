using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class TraderDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, TraderDetailsControlBag> where T : Business.NctsHeader
	{
		public override TraderDetailsControlBag CommonBag => TraderDetailsControlBag.Instance;

		protected override int MaxColumns => 1;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => (ColumnLayoutBuilderCaptionWidthSize)CaptionWidthSize.Small;

		public enum CaptionWidthSize
		{
			Small = 60,
		}
	}
}
