using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class AISDocumentsUploadAddInfoGridLayoutBuilder<T> : ColumnLayoutBuilder<T, AISDocumentsUploadAddInfoGridControlBag> where T : UploadDocumentsSendingAction
	{
		public override AISDocumentsUploadAddInfoGridControlBag CommonBag => AISDocumentsUploadAddInfoGridControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.AddInfosGrid, x => !Show483Grid(x.MessageType), x => x.MessageTypeInfo);
			SetVisibility(CommonBag.AddInfosIM483Grid, x => Show483Grid(x.MessageType), x => x.MessageTypeInfo);
		}

		bool Show483Grid(string messageType)
		{
			return messageType.Equals(AISUploadDocumentsMessageTypeList.Codes.IM483, System.StringComparison.InvariantCultureIgnoreCase) ||
				messageType.Equals(AISUploadDocumentsMessageTypeList.Codes.IM446, System.StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
