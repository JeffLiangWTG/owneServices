using Enterprise.Customs.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public sealed class MenuBuilder : EU.H7.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm)
			: base(header, mainForm)
		{
		}

		protected override EU.H7.GUI.MessageSendingForm GetMessageSendingForm(BaseMessageSendingObjectParent messageSendingParent)
		{
			return new MessageSendingForm(messageSendingParent);
		}

		protected override EU.H7.GUI.UploadDocumentsForm GetUploadDocumentsForm(BaseMessageSendingObjectParent sendingObjectParent)
		{
			return new UploadDocumentsForm(sendingObjectParent as UploadDocumentsSendingActionParent);
		}
	}
}
