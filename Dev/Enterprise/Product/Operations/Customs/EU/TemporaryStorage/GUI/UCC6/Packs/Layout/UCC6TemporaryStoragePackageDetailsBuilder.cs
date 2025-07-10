using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePackageDetailsBuilder : ColumnLayoutBuilder<TemporaryStorageHeader, UCC6TemporaryStoragePackageDetailsControlBag>
	{
		public override UCC6TemporaryStoragePackageDetailsControlBag CommonBag => UCC6TemporaryStoragePackageDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;
	}
}
