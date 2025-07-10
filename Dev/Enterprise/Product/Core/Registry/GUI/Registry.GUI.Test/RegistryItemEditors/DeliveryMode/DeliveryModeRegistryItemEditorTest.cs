using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DeliveryModeRegistryItemEditor))]
	sealed class DeliveryModeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DeliveryModeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DeliveryModeRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DeliveryModeRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DeliveryModeRegistryItem("ContainerDeliveryModeList", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System | RegistryStorageFlags.Company, DeliveryModeCollection.GetDefault());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { DeliveryModeCollection.GetDefault() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
