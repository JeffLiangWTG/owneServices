using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(AuthorizationModeAndSettingsEditor))]
	public class AuthorizationModeAndSettingsEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new AuthorizationModeAndSettingsEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AuthorizationModeAndSettingsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AuthorizationModeAndSettingsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AuthorizationModeAndSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var result = new AuthorizationModeAndSettings();
			result.AuthorizationMode = "DEF";
			var setting11 = result.AuthorisationSettings.AddNew();
			setting11.Amount = 100;
			setting11.AuthorisationRequirement = AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting11.Range = "Up to";

			var setting12 = result.AuthorisationSettings.AddNew();
			setting12.Amount = 100;
			setting12.Range = "Above";
			setting12.AuthorisationRequirement = AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			return new object[] { result };
		}
	}
}
