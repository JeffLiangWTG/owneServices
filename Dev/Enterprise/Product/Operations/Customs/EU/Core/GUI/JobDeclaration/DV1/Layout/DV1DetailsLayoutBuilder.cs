using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class DV1DetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, DV1DetailsControlBag> where T : JobDeclaration
	{
		public override DV1DetailsControlBag CommonBag { get; } = DV1DetailsControlBag.Instance;

		protected override int MaxColumns => 2;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		public override bool NarrowColumnForMediumControls => true;
	}
}
