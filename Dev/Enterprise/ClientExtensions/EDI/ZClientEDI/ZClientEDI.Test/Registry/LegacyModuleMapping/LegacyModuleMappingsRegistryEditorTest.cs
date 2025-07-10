using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(LegacyModuleMappingsRegistryEditor))]
	public class LegacyModuleMappingsRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new LegacyModuleMappingsRegistryItem("", null, null, null, new LegacyModuleMappingsRegistryEditorInfo("Category"), RegistryStorageFlags.System, ModuleListType.MenuSection);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new LegacyModuleMappingsRegistryEditor(new LegacyModuleMappingsRegistryDataType(ModuleListType.MenuSection), new LegacyModuleMappingsRegistryEditorInfo("Category"));
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(LegacyModuleMappingsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			LegacyModuleMappingCollection collection = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			collection.AddNew("COR", "Core", "", MandatoryCustomerServiceMenuSectionList.Codes.Other);
			collection.AddNew("XXX", "XXX", "CR8", Cr8ModuleList.Codes.OtherComplianceIssue);
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((LegacyModuleMappingsControl)editorPane).ReadOnly;
		}
		#endregion
	}
}
