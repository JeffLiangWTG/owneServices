using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageSupportingDocumentsDetailsLayoutBuilder : ColumnLayoutBuilder<TemporaryStorageBill, UCC6TemporaryStorageSupportingDocumentsDetailsControlBag>
	{
		public override UCC6TemporaryStorageSupportingDocumentsDetailsControlBag CommonBag => UCC6TemporaryStorageSupportingDocumentsDetailsControlBag.Instance;
		protected override int MaxColumns => 1;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
