using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public partial class TemporaryStorageForm : EU.TemporaryStorage.GUI.TemporaryStorageForm
{
	public TemporaryStorageForm(TemporaryStorageHeader header) : base(header)
	{
	}

	protected override ZMenuItem GetNewMessagingMenu() => new TemporaryStorageMessagesMenu(this);
}
