using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CountryDefaultLanguageRegistryItemEditor))]
	sealed class CountryDefaultLanguageRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CountryDefaultLanguageRegistryItemEditor(new CountryDefaultLanguageRegistryDataType(), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CountryDefaultLanguageControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CountryDefaultLanguageControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CountryDefaultLanguageBusinessObjectCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CountryDefaultLanguageBusinessObjectCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CountryDefaultLanguageBusinessObjectCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
