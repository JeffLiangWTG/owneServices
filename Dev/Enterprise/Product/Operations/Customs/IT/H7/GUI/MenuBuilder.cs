using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI;

public class MenuBuilder : EU.H7.GUI.MenuBuilder
{
	public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
	{
	}

	protected override EU.H7.GUI.MessageSendingForm GetMessageSendingForm(BaseMessageSendingObjectParent messageSendingParent)
	{
		return new MessageSendingForm(messageSendingParent);
	}
}
