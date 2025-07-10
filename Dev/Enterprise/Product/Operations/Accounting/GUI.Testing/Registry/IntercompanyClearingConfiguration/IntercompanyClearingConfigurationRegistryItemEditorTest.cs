using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(IntercompanyClearingConfigurationRegistryItemEditor))]
	public class IntercompanyClearingConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new IntercompanyClearingConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((IntercompanyClearingConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(IntercompanyClearingConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new IntercompanyClearingConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			IntercompanyClearingConfigurationCollection copy = new IntercompanyClearingConfigurationCollection();
			IntercompanyClearingConfiguration config = copy.AddNew();
			config.Company = "EDI";
			config.ClearingGLAccount = ZGuid.Empty;

			return new object[] { copy };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
