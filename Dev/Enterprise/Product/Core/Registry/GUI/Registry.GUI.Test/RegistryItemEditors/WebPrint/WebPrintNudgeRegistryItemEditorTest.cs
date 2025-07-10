using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebPrintNudgeRegistryItemEditor))]
	sealed class WebPrintNudgeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestWebPrintNudgeRegistryItemEditor()
		{
			try
			{
				_ = new WebPrintNudgeRegistryItemEditor(null, null);
				AssertEquals("WebPrintNudgeRegistryItemEditor", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new WebPrintNudgeRegistryItemEditor(new WebPrintNudgeRegistryDataType(NudgeForTest), new WebPrintNudgeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, NudgeForTest));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var optionGroupBox = (KGroupBox)editorPane.GetType().GetField("OptionGroupBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editorPane);
			return optionGroupBox.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebPrintNudgeUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebPrintNudgeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, NudgeForTest);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { NudgeForTest };
		}

		protected override bool CanNotHaveReferenceEquality => false;

		WebPrintNudge NudgeForTest => new WebPrintNudge { EnableIPAddress = true };
	}
}
