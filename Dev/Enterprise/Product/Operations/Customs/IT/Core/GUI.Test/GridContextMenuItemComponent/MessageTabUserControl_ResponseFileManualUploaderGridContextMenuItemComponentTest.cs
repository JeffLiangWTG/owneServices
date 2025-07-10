using System.Linq;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ResponseFileManualUploaderGridContextMenuItemComponent))]
sealed class MessageTabUserControl_ResponseFileManualUploaderGridContextMenuItemComponentTest : ResponseFileManualUploaderGridContextMenuItemComponentTest<JobDeclaration, MessageUserControl>
{
	protected override ITEDIMessage AddNewMessage(JobDeclaration bizObj)
	{
		var entryHeader = !bizObj.CustomsEntryHeaders.Any()
			? bizObj.CustomsEntryHeaders.AddNew()
			: bizObj.CustomsEntryHeaders[0];

		return entryHeader.Messages.AddNew();
	}

	protected override ZGrid GetMessagesGrid(MessageUserControl messageUserControl)
	{
		var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
		tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
		var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
		return userControlMessages.FindSingle<ZGrid>("MessagesGrid");
	}

	protected override MessageUserControl GetMessageUserControl(JobDeclaration decl) => new MessageUserControl();

	protected override JobDeclaration GetTopLevelBizObj() => Factory.New<JobDeclaration>();
}
