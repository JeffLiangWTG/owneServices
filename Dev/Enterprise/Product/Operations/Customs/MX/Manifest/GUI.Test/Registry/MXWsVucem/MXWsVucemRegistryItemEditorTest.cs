using System;
using System.Windows.Forms;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	[TestedType(typeof(MXWsVucemRegistryItemEditor))]
	class MXWsVucemRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new MXWsVucemRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((MXWsVucemRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(MXWsVucemRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new MXWsVucemRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, CreateNewValue());

		protected override object[] GetValidRegistryValues() => new[] { CreateNewValue() };

		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 436, 160);
				AssertEquals("should have the correct width", 436, control.Width);
				AssertEquals("should have the correct height", 160, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, control.Anchor);
			}
		}

		MXWsVucem CreateNewValue()
		{
			return new MXWsVucem { SeaModeWSResponse = "http://127.0.0.1/", SeaModeWSUsername = "ADMINVUCEM1", SeaModeWSPassword = "9974567891", AirModeWSResponse = "http://127.0.0.1/", AirModeWSUsername = "ADMINVUCEM2", AirModeWSPassword = "9974567890" };
		}
	}
}
