using System;
using System.Windows.Forms;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(CLSMSMessageSendingRegistryItemEditor))]
	sealed class CLSMSMessageSendingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 436, 326);
				AssertEquals("should have the correct width", 436, control.Width);
				AssertEquals("should have the correct height", 359, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, control.Anchor);
			}
		}

		protected override RegistryItemEditor GetEditor() => new CLSMSMessageSendingRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((CLSMSMessageSendingRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(CLSMSMessageSendingRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CLSMSMessageSendingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, CreateNewValue());

		protected override object[] GetValidRegistryValues() => new[] { CreateNewValue() };

		CLSMSMessageSending CreateNewValue() => new CLSMSMessageSending
		{
			MachineName = "Machine Name",
			ApplicationNodePassword = "1234",
			RunningIntervalInSeconds = 60,
			SendFolder = @"D:\Folders\SendFolder",
			UnknownFolder = @"D:\Folders\UnknownFolder",
			InvalidFolder = @"D:\Folders\InvalidFolder",
			RejectedFolder = @"D:\Folders\RejectedFolder",
			ReceiveFolder = @"D:\Folders\ReceiveFolder",
			AcceptedFolder = @"D:\Folders\AcceptedFolder"
		};
	}
}
