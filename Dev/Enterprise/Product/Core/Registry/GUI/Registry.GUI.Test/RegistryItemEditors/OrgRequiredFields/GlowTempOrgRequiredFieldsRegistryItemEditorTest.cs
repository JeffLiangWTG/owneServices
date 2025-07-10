using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowTempOrgRequiredFieldsRegistryItemEditor))]
	sealed class GlowOrgRequiredFieldsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new GlowTempOrgRequiredFieldsRegistryItemEditor(new GlowTempOrgRegistryDataType(new GlowTempOrgRequiredFieldCollection()), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GlowTempOrgRequiredFieldsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GlowTempOrgRequiredFieldsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GlowTempOrgRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, new GlowTempOrgRequiredFieldCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var defaultValue = new GlowTempOrgRequiredFieldCollection(10);
			defaultValue.AddSystemDefined("Name", ResString.GetMultilingualString("52085C33-53AA-4F6B-8CBF-D36576E4AFC1", "Name"), true, true);
			defaultValue.AddSystemDefined("Address1", ResString.GetMultilingualString("a506de0a-af8f-4da1-a9e6-518b05b54e84", "Address 1"), false, false);
			defaultValue.AddSystemDefined("Address2", ResString.GetMultilingualString("c56f3a51-9ede-4045-8095-5fca138a39f2", "Address 2"), false, false);
			defaultValue.AddSystemDefined("Country", ResString.GetMultilingualString("da389b33-751e-458c-ae2a-fc08f3d68b85", "Country/Region"), false, false);
			defaultValue.AddSystemDefined("UNLOCO", ResString.GetMultilingualString("66e8b9cc-ccf5-45f7-99da-52222f2ef838", "UNLOCO"), false, false);
			defaultValue.AddSystemDefined("City", ResString.GetMultilingualString("a30ddcc4-8c21-4396-8278-989c808a654e", "City"), false, false);
			defaultValue.AddSystemDefined("Postcode", ResString.GetMultilingualString("aa527797-4ae0-4e77-b0cd-f62a640ecb7c", "Postcode"), false, false);
			defaultValue.AddSystemDefined("State", ResString.GetMultilingualString("7267acfc-f447-4be0-9450-c3879fbc9023", "State"), false, false);
			defaultValue.AddSystemDefined("Branch", ResString.GetMultilingualString("d06b17ff-d290-41a6-8813-a19bc015b0db", "Branch"), false, false);
			defaultValue.AddSystemDefined("Phone", ResString.GetMultilingualString("eefea28f-e9a0-4a76-ad11-7b486568a6a2", "Phone"), false, false);
			defaultValue.AddSystemDefined("Mobile", ResString.GetMultilingualString("0e6a1eaf-da3e-404a-a11c-0d11c28dba6b", "Mobile"), false, false);
			defaultValue.AddSystemDefined("Email", ResString.GetMultilingualString("dbca8d61-8a78-4c9e-81de-9dd5639df5e7", "Email"), false, false);
			defaultValue.AddSystemDefined("Fax", ResString.GetMultilingualString("5b5b0e3c-6aee-446f-b16a-eb15227f3ca4", "Fax"), false, false);
			defaultValue.AddSystemDefined("Web", ResString.GetMultilingualString("2df45a1b-ed4b-4a20-8e55-4c58ecde6299", "WebSite URL"), false, false);

			return new[] { defaultValue };
		}
	}
}

