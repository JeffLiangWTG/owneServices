using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

sealed class UCC6TemporaryStorageBillDetailBuilder : ColumnLayoutBuilder<TemporaryStorageHeader, UCC6TemporaryStorageBillDetailControlBag>
{
	public override UCC6TemporaryStorageBillDetailControlBag CommonBag => UCC6TemporaryStorageBillDetailControlBag.Instance;

	protected override int MaxColumns => 2;

	public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
}
