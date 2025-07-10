using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CountryListRegistryItemEditor))]
	sealed class CountryListRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CountryListRegistryItemEditor(Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CountryListControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CountryListControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			GuidArrayRegistryItem result = new GuidArrayRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new CountryListRegistryEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new Guid[] { Guid.NewGuid(), Guid.Empty }, Array.Empty<Guid>() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
