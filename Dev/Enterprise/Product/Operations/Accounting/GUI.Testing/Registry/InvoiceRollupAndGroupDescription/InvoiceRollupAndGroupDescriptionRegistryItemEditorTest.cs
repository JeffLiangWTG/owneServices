using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(InvoiceRollupAndGroupDescriptionRegistryItemEditor))]
	class InvoiceRollupAndGroupDescriptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new InvoiceRollupAndGroupDescriptionRegistryItemEditor(new InvoiceRollupAndGroupDescriptionRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InvoiceRollupAndGroupDescriptionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InvoiceRollupAndGroupDescriptionRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceRollupAndGroupDescriptionCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new InvoiceRollupAndGroupDescriptionCollection() };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InvoiceRollupAndGroupDescriptionControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
