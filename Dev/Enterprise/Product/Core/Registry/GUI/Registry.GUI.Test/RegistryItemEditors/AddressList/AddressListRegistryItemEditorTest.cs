using System;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AddressListRegistryItemEditor))]
	sealed class AddressListRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestSetValueFromEditorPane()
		{
			using (ZForm form = new ZForm())
			{
				var editorPane = (AddressListControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				var collection = new AddressListCollection();

				var parent1 = collection.AddNew();
				var parent2 = collection.AddNew();

				parent1.AddressType = "ConsignorPickupDeliveryAddress";
				parent2.ControllerName = "JobShipment";

				Editor.SetValueFromEditorPane(editorPane, collection);

				var getCollection = (AddressListCollection)((IDataBoundControl)editorPane).DataSource;

				CombineAssertions(() =>
				{
					AssertEquals("ConsignorPickupDeliveryAddress", getCollection[0].AddressType);
					AssertEquals("JobShipment", getCollection[1].ControllerName);
				});
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor() => new AddressListRegistryItemEditor(RegistryItem.DataType, null, Factory);

		protected override Type GetExpectedEditorPaneType() => typeof(AddressListControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new AddressListRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new AddressListCollection());

		protected override object[] GetValidRegistryValues()
		{
			var collection = new AddressListCollection();
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((AddressListControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
