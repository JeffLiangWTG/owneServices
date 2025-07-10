using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ConsolCostDefaultApportionmentMethodRegistryItemEditor))]
	public class ConsolCostDefaultApportionmentMethodRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ConsolCostDefaultApportionmentMethodRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ConsolCostDefaultApportionmentMethodControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ConsolCostDefaultApportionmentMethodControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ConsolCostDefaultApportionmentMethodRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			ConsolCostDefaultApportionmentMethodConfiguration copy = new ConsolCostDefaultApportionmentMethodConfiguration();

			return new object[] { copy };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
