using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(MessagesTabUserControl))]
sealed class MessagesTabUserControl_ResponseUploaderTest : ResponseFileManualUploaderGridContextMenuItemComponentTest<NctsHeader, MessagesTabUserControl>
{
	protected override ITEDIMessage AddNewMessage(NctsHeader bizObj) => bizObj.Messages.AddNew();

	protected override ZGrid GetMessagesGrid(MessagesTabUserControl messageUserControl) => messageUserControl.FindSingle<ZGrid>("MessageGrid");

	protected override MessagesTabUserControl GetMessageUserControl(NctsHeader header)
	{
		var control = new MessagesTabUserControl();
		control.SetDataBinding(header, "Messages");
		return control;
	}

	protected override NctsHeader GetTopLevelBizObj() => Factory.NewDepartureNctsHeader();
}
