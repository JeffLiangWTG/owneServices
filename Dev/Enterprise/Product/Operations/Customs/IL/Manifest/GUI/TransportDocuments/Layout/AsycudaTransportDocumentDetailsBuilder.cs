using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class AsycudaTransportDocumentDetailsBuilder : ColumnLayoutBuilder<AsycudaAdditionalInfo, AsycudaTransportDocumentDetailsControlBag>
	{
		public override AsycudaTransportDocumentDetailsControlBag CommonBag => AsycudaTransportDocumentDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;
	}
}
