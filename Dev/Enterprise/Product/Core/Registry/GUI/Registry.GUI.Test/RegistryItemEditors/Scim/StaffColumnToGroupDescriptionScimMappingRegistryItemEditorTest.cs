using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StaffColumnToGroupDescriptionScimMappingRegistryItemEditor))]
	public class StaffColumnToGroupDescriptionScimMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new StaffColumnToGroupDescriptionScimMappingRegistryItemEditor(new StaffColumnToGroupDescriptionScimMappingRegistryDataType(),
				new ZArchitecture.Environment.FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((StaffColumnToGroupDescriptionScimMappingControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(StaffColumnToGroupDescriptionScimMappingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StaffColumnToGroupDescriptionScimMappingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[]
			{
				new StaffColumnToGroupDescriptionScimMappingCollection()
				{
					new StaffColumnToGroupDescriptionScimMapping()
					{
						GroupDescriptionMapping = "Test01", StaffColumnName = "GS_CanLogin"
					}
				}
			};
		}

		public void TestReadonly()
		{
			var editor = GetEditor() as StaffColumnToGroupDescriptionScimMappingRegistryItemEditor;
			using var pane = editor.NewWinFormsEditorPane();
			AssertEquals(true, pane.Enabled);
			AssertEquals(true, GetEditorPaneEnabledState(pane));

			editor.EnableEditorPane(pane, false);

			AssertEquals(false, GetEditorPaneEnabledState(pane));
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
