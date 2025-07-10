using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder : ColumnLayoutBuilder<TemporaryStorageBill, UCC6TemporaryStorageAdditionalInformationDetailsControlBag>
	{
		public override UCC6TemporaryStorageAdditionalInformationDetailsControlBag CommonBag { get; } = UCC6TemporaryStorageAdditionalInformationDetailsControlBag.Instance;

		protected override int MaxColumns => 1;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
