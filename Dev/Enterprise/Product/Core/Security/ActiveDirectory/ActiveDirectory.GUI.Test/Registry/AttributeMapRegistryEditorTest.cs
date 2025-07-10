using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.Security.ActiveDirectory.GUI.Registry;
using Enterprise.Security.ActiveDirectory.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(AttributeMapRegistryEditor))]
	class AttributeMapRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		public void TestEnableEditor_ShouldSetControlReadOnly()
		{
			var map = AttributeMap.DefaultMap;
			var registryItem = new AttributeMapRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, map);
			var editor = GetEditor();
			using (var control = new AttributeMapControl())
			{
				control.Value = map;

				editor.EnableEditorPane(control, false);
				Assert(!control.IsReadOnly);

				editor.EnableEditorPane(control, true);
				Assert(control.IsReadOnly);
			}
		}

		public void TestGridFillCorrectly()
		{
			using (var registryForm = GetNewRegistryFormForTest(RegistryItem))
			{
				registryForm.Show();
				registryForm.DisplayRegistryItem();
				Application.DoEvents();

				var attributeMapControl = registryForm.Controls.Find("attributeMapControl", true)[0];
				var attributeMapGrid = (ZGrid)attributeMapControl.Controls.Find("attributeMapGrid", true)[0];

				AssertEquals("The grid should use the available space", attributeMapControl.Width, attributeMapGrid.Width);

				var widthBefore = attributeMapControl.Width;
				registryForm.Width += 200;
				Application.DoEvents();
				var widthAfter = attributeMapControl.Width;

				AssertGreaterThan("The map should have gotten wider", widthAfter, widthBefore);
				AssertEquals("The grid should resize with the control", attributeMapControl.Width, attributeMapGrid.Width);
			}
		}

		protected override object[] GetValidRegistryValues() => new[] { AttributeMap.DefaultMap };

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AttributeMapRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, AttributeMap.DefaultMap);
		}

		protected override Type GetExpectedEditorPaneType() => typeof(AttributeMapControl);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((AttributeMapControl)editorPane).IsReadOnly;

		protected override RegistryItemEditor GetEditor() => new AttributeMapRegistryEditor(new AttributeMapRegistryDataType(AttributeMap.DefaultMap));
	}
}
