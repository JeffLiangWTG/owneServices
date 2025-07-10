using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GmailOAuth2JsonFileUserRegistryItemEditor))]
	sealed class GmailOAuth2JsonFileUserRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestReportOnceOAuth2JsonFileUserRegistryItemEditor()
		{
			try
			{
				_ = new GmailOAuth2JsonFileUserRegistryItemEditor(null);
				AssertEquals("GmailOAuth2JsonFileUserRegistryItemEditor", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestEditorPane()
		{
			var control = Editor.GetType().GetMethod("NewWinFormsEditorPaneCore", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Editor, null) as GmailOAuth2JsonFileUserControl;
			using (control)
			{
				AssertNotNull(control);
			}
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GmailOAuth2JsonFileRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override bool CanNotHaveReferenceEquality => false;

		protected override RegistryItemEditor GetEditor()
		{
			return new GmailOAuth2JsonFileUserRegistryItemEditor(GetRegistryItemWithSystemStorageLevel());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GmailOAuth2JsonFileUserControl);
		}

		protected override object[] GetValidRegistryValues() => new object[] { new GmailOAuth2JsonFile() };

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var chooseButton = (ZButton)editorPane.GetType().GetField("btnChoose", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editorPane);
			return !chooseButton.ReadOnly;
		}
	}
}
