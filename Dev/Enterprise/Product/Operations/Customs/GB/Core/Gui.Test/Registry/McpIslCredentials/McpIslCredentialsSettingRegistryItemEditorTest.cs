using System;
using System.Windows.Forms;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(McpIslCredentialsSettingRegistryItemEditor))]
	class McpIslCredentialsSettingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new McpIslCredentialsSettingRegistryItemEditor(new McpIslCredentialsSettingRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((McpIslCredentialsSettingControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(McpIslCredentialsSettingControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new McpIslCredentialsSettingCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues()
		{
			var credentials = new McpIslCredentialsSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

			var credential1 = credentials.AddNew();
			credential1.McpIslCompanyCode = "WIS";
			credential1.McpIslUsername = "test";
			credential1.McpIslDevice = "CAW1";
			credential1.McpIslPassword = "abc";

			var credential2 = credentials.AddNew();
			credential2.McpIslCompanyCode = "GLO";
			credential2.McpIslUsername = "test2";
			credential2.McpIslDevice = "CAW2";
			credential2.McpIslPassword = "abcd";

			return new object[] { credentials };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
