using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EBookingAirCarrierConfigurationRegistryItemEditor))]
	sealed class EBookingAirCarrierConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;

		protected override RegistryItemEditor GetEditor()
		{
			return new EBookingAirCarrierConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((EBookingAirCarrierConfigurationControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(EBookingAirCarrierConfigurationControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EBookingAirCarrierConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new EBookingCarrierConfiguration());
		}

		protected override object[] GetValidRegistryValues() => new object[] { new EBookingCarrierConfiguration() };
	}
}
