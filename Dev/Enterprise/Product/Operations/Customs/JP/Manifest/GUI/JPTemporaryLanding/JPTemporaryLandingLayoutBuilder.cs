using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public class JPTemporaryLandingLayoutBuilder<T> : ColumnLayoutBuilder<T, JPTemporaryLandingControlBag> where T : AsycudaBill
	{
		public override JPTemporaryLandingControlBag CommonBag { get; } = JPTemporaryLandingControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
