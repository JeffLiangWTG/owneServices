using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePackedItemDetailsBuilder<T> : ColumnLayoutBuilder<T, UCC6TemporaryStoragePackedItemDetailsControlBag>
		where T : TemporaryStorageHeader
	{
		public override UCC6TemporaryStoragePackedItemDetailsControlBag CommonBag => UCC6TemporaryStoragePackedItemDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 1;
	}
}
