using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageBillPartiesBuilder : ColumnLayoutBuilder<TemporaryStorageHeader, UCC6TemporaryStorageBillPartiesControlBag>
	{
		public override UCC6TemporaryStorageBillPartiesControlBag CommonBag => UCC6TemporaryStorageBillPartiesControlBag.Instance;

		protected override int MaxColumns => 3;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
