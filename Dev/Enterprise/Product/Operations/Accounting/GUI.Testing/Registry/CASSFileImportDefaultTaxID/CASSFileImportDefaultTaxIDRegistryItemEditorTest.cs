using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CASSFileImportDefaultTaxIDRegistryItemEditor))]
	public class CASSFileImportDefaultTaxIDRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CASSFileImportDefaultTaxIDRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CASSFileImportDefaultTaxIDControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CASSFileImportDefaultTaxIDControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CASSFileImportDefaultTaxIDRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { new CASSFileImportDefaultTaxID() };
		}
	}
}
