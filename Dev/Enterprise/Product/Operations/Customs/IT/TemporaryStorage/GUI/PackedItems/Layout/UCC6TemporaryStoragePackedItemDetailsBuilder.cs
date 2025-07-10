using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class UCC6TemporaryStoragePackedItemDetailsBuilder : ColumnLayoutBuilder<TemporaryStorageHeader, UCC6TemporaryStoragePackedItemDetailsControlBag>
{
	public override UCC6TemporaryStoragePackedItemDetailsControlBag CommonBag => UCC6TemporaryStoragePackedItemDetailsControlBag.Instance;

	protected override int MaxColumns => 2;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}
