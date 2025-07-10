using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public class JPManifestLayoutBuilder : ManifestLayoutBuilder<AsycudaManifestHeader>
	{
		protected override int MaxColumns => 3;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium + 30;
	}
}
