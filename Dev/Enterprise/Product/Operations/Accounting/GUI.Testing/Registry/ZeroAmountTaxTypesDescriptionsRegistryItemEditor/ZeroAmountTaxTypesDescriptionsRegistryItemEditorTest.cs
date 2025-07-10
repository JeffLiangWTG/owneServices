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
	[TestedType(typeof(ZeroAmountTaxTypesDescriptionsRegistryItemEditor))]
	public class ZeroAmountTaxTypesDescriptionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ZeroAmountTaxTypesDescriptionsRegistryItemEditor(new ZeroAmountTaxTypesDescriptionsDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ZeroAmountTaxTypesDescriptionsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ZeroAmountTaxTypesDescriptionsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ZeroAmountTaxTypesDescriptionsRegistryItem("", null, null, null, RegistryStorageFlags.System, new ZeroAmountTaxTypesDescriptionsCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ZeroAmountTaxTypesDescriptionsCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
