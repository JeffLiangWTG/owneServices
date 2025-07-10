using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.H7.GUI
{
	public sealed class MenuBuilder : EU.H7.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm)
			: base(header, mainForm)
		{
		}

		protected override EU.H7.GUI.MessageSendingForm GetMessageSendingForm(BaseMessageSendingObjectParent messageSendingParent) => new MessageSendingForm(messageSendingParent);
	}
}
