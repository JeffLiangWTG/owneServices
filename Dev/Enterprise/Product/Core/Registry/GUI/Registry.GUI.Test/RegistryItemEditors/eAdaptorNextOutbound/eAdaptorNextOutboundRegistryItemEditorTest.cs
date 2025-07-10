using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(eAdaptorNextOutboundRegistryItemEditor))]
	sealed class eAdaptorNextOutboundRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new eAdaptorNextOutboundRegistryItemEditor(new eAdaptorNextOutboundConfigRegistryDataType(eAdaptorNextOutboundConfig.DefaultValue));
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((eAdaptorNextOutboundRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(eAdaptorNextOutboundRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new eAdaptorNextOutboundConfigRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, eAdaptorNextOutboundConfig.DefaultValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { eAdaptorNextOutboundConfig.DefaultValue };
		}
		#endregion
	}
}
