using System;
using System.Windows.Forms;
using Enterprise.Customs.JP.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItemEditor))]
	sealed class FTPSettingsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new FTPSettingsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((FTPSettingsRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(FTPSettingsRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new FTPSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var fTPSettings = new FTPSettings(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			fTPSettings.UserName = "TestUser";
			fTPSettings.Password = "TestPassword";
			fTPSettings.InFolder = "TestPath";
			fTPSettings.OutFolder = "TestPath";
			fTPSettings.Server = "localhost";
			fTPSettings.Port = 8888;
			fTPSettings.Passive = false;
			Factory.Save();

			return new object[] { fTPSettings };
		}
	}
}
