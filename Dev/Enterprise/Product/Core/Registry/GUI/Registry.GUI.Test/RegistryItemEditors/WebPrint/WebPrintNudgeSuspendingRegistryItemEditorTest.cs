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
	[TestedType(typeof(WebPrintNudgeSuspendingRegistryItemEditor))]
	sealed class WebPrintNudgeSuspendingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestWebPrintNudgeRegistryItemEditor()
		{
			try
			{
				_ = new WebPrintNudgeSuspendingRegistryItemEditor(null, null);
				AssertEquals("WebPrintNudgeSuspendingRegistryItemEditor", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new WebPrintNudgeSuspendingRegistryItemEditor(new WebPrintNudgeSuspendingRegistryDataType(NudgeSuspendingForTest), new WebPrintNudgeSuspendingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, NudgeSuspendingForTest));
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var optionGroupBox = (KGroupBox)editorPane.GetType().GetField("OptionGroupBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editorPane);
			return optionGroupBox.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebPrintNudgeSuspendingUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebPrintNudgeSuspendingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, NudgeSuspendingForTest);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { NudgeSuspendingForTest };
		}

		protected override bool CanNotHaveReferenceEquality => false;

		WebPrintNudgeSuspending NudgeSuspendingForTest => new WebPrintNudgeSuspending();
	}
}
