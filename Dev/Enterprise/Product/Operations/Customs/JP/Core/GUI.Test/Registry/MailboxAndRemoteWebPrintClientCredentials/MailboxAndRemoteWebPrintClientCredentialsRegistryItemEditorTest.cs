using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(MailboxAndRemoteWebPrintClientCredentialsRegistryItemEditor))]
	sealed class MailboxAndRemoteWebPrintClientCredentialsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new MailboxAndRemoteWebPrintClientCredentialsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new MailboxAndRemoteWebPrintClientCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var mailboxAndRemoteWebPrintClientCredentials = new MailboxAndRemoteWebPrintClientCredentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			mailboxAndRemoteWebPrintClientCredentials.LocalComputerAlias = "JP";
			mailboxAndRemoteWebPrintClientCredentials.DomainName = "JP";
			mailboxAndRemoteWebPrintClientCredentials.ReceivingInterval = 6;
			mailboxAndRemoteWebPrintClientCredentials.FailureNotificationGroup = group.GG_Code;
			mailboxAndRemoteWebPrintClientCredentials.DownTimeStart = ZDateTime.Today;
			mailboxAndRemoteWebPrintClientCredentials.DownTimeEnd = ZDateTime.Today.AddDays(1);
			Factory.Save();

			return new object[] { mailboxAndRemoteWebPrintClientCredentials };
		}
	}
}
