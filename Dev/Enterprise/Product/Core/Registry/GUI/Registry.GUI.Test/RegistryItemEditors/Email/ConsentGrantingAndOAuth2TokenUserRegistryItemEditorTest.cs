using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ConsentGrantingAndOAuth2TokenUserRegistryItemEditor))]
	sealed class ConsentGrantingAndOAuth2TokenUserRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestReportOnceConsentGrantingAndOAuth2TokenUserRegistryItemEditor()
		{
			try
			{
				_ = new ConsentGrantingAndOAuth2TokenUserRegistryItemEditor(null);
				AssertEquals("ConsentGrantingAndOAuth2TokenUserRegistryItemEditor", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestEditorPane()
		{
			var control = Editor.GetType().GetMethod("NewWinFormsEditorPaneCore", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Editor, null) as ConsentGrantingAndOAuth2TokenUserControl;
			using (control)
			{
				AssertNotNull(control);
			}
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new Ms365OAuth2TokenRegistryItem("", null, null, null, EmailType.Incoming, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override bool CanNotHaveReferenceEquality => false;

		protected override RegistryItemEditor GetEditor()
		{
			return new ConsentGrantingAndOAuth2TokenUserRegistryItemEditor(GetRegistryItemWithSystemStorageLevel());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ConsentGrantingAndOAuth2TokenUserControl);
		}

		protected override object[] GetValidRegistryValues() => new object[] { new Ms365OAuth2Token() };

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var grantButton = (ZButton)editorPane.GetType().GetField("btnGrant", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editorPane);
			return !grantButton.ReadOnly;
		}
	}
}
