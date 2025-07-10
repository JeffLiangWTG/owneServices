using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RadioButtonRegistryItemEditor))]
	sealed class RadioButtonRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				Editor.SetEditorPaneLayout(editorPane, ControlDpiScalingHelper.ScaleToCurrentDpiX(150), ControlDpiScalingHelper.ScaleToCurrentDpiY(85));
				AssertEquals("editorPane.Size", new Size(ControlDpiScalingHelper.ScaleToCurrentDpiX(150), ControlDpiScalingHelper.ScaleToCurrentDpiY(85)), editorPane.Size);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new RadioButtonRegistryItemEditor(new BooleanRegistryEditorInfo(), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RadioButtonControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BooleanRegistryItem("", null, null, null, RegistryStorageFlags.System, false);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { true, false };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
