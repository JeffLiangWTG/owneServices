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
	[TestedType(typeof(CreditCardFeeRegistryItemEditor))]
	class CreditCardFeeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CreditCardFeeRegistryItemEditor(new CreditCardFeeRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CreditCardFeeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CreditCardFeeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CreditCardFeeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CreditCardFeeCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
