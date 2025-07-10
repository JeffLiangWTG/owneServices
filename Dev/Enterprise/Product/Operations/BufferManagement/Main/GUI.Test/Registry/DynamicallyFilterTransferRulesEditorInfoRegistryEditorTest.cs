using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(DynamicallyFilterTransferRulesEditorInfoRegistryEditor))]
	internal class DynamicallyFilterTransferRulesEditorInfoRegistryEditorTest : RegistryItemEditorTestCase
	{
		public void TestDisableEditorPane_ButtonShouldStillBeClickable()
		{
			var editor = GetEditor();

			using (var control = editor.NewWinFormsEditorPane())
			{
				var button = control.FindSingle<ZButton>();

				editor.EnableEditorPane(control, true);

				AssertEquals(false, button.ReadOnly);

				editor.EnableEditorPane(control, false);

				AssertEquals(false, button.ReadOnly);
			}
		}

		public void TestClickForceToRunAll_ShouldSetRegistryProcessAllTransferRulesLinksOnNextBMSRun_AndMakeItReadyOnly()
		{
			var editor = GetEditor();

			var previousShowDialogsInTest = ZFormModaliser.ShowDialogsInTest;
			ZFormModaliser.ShowDialogsInTest = true;

			AssertEquals(false, BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);

			using (var control = editor.NewWinFormsEditorPane())
			{
				var button = control.FindSingle<ZButton>();

				AssertEquals(false, button.ReadOnly);

				button.PerformClick();

				using (var dialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					var messageBox = (ZMessageBox)dialog;

					AssertEquals("Next time BMS service task runs it will process all transfer rules links.", messageBox.Message);
				}

				AssertEquals(true, button.ReadOnly);
			}

			AssertEquals(true, BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);

			ZFormModaliser.ShowDialogsInTest = previousShowDialogsInTest;
		}

		#region RegistryItemEditorTestCase Implementation

		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (Control control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 666, 333);
				AssertEquals("should have the correct width", 666, control.Width);
				AssertEquals("should have the correct height", 333, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
			}
		}

		protected override RegistryItemEditor GetEditor() => new DynamicallyFilterTransferRulesEditorInfoRegistryEditor(new DynamicallyFilterTransferRulesRegistryEditorInfo(), new BooleanRegistryDataType());

		protected override bool GetEditorPaneEnabledState(Control editorPane) => editorPane.FindAll<RadioButtonControl>().Single().Enabled;

		protected override Type GetExpectedEditorPaneType() => typeof(FlowLayoutPanel);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => BMSRegistry.Instance.DynamicallyFilterTransferRules;

		protected override object[] GetValidRegistryValues() => new object[] { true, false };

		#endregion
	}
}
