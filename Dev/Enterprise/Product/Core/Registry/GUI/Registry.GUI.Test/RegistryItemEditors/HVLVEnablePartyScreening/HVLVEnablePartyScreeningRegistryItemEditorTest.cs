using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HVLVEnablePartyScreeningRegistryItemEditor))]
	sealed class HVLVEnablePartyScreeningRegistryItemEditorTest
		: RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new HVLVEnablePartyScreeningRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((HVLVEnablePartyScreeningRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(HVLVEnablePartyScreeningRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new HVLVEnablePartyScreeningRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new HVLVEnablePartyScreening[] { new HVLVEnablePartyScreening() };
		}

		#endregion
	}
}
