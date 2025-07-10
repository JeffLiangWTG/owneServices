using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionControl))]
	sealed class ClientInTemplateSelectionControlTest : Testing.RegistryZUserControlTestCase
	{
		public void TestAddAndRemoveButtons()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;

			using (Form dummyForm = new Form())
			using (ClientInTemplateSelectionControl control = new ClientInTemplateSelectionControl())
			{
				control.SetDataBinding(collection, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				SelectShipmentRow(collection, control);
				control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.SelectAllElements();
				control.AddOrgTypeButton_Click(this, new EventArgs());

				AssertEquals("3 items selected", 3, control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List.Count);
				AssertEquals("0 items available", 0, control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.List.Count);

				control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(0);
				control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(1);

				AssertEquals("2 items selected", 2, control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.SelectedElements.Length);

				control.RemoveOrgTypeButon_Click(this, new EventArgs());

				AssertEquals("2 items available", 2, control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.List.Count);
				AssertEquals("1 item selected", 1, control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.List.Count);

				AssertEquals("Consignee / Consignor", control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid[0, 0]);
				AssertEquals("Local Client", control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid[1, 0]);
				AssertEquals("Controlling Customer", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
			}
		}

		public void TestMoveButtons()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;

			using (Form dummyForm = new Form())
			using (ClientInTemplateSelectionControl control = new ClientInTemplateSelectionControl())
			{
				control.SetDataBinding(collection, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				SelectShipmentRow(collection, control);
				control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.SelectAllElements();
				control.AddOrgTypeButton_Click(this, new EventArgs());

				AssertEquals("Precondition 1", "Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Precondition 2", "Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Precondition 3", "Controlling Customer", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[2, 0]);

				control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(1);
				control.MoveDownButton_Click(this, new EventArgs());

				AssertEquals("Test 1.1", "Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Test 1.1", "Controlling Customer", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Test 1.1", "Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[2, 0]);

				control.MoveDownButton_Click(this, new EventArgs());
				AssertEquals("Test 2.1", "Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Test 2.2", "Controlling Customer", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Test 2.3 - Can't move down as already at the bottom", "Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[2, 0]);

				control.MoveUpButton_Click(this, new EventArgs());
				control.MoveUpButton_Click(this, new EventArgs());
				AssertEquals("Test 3.1", "Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Test 3.1", "Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Test 3.1", "Controlling Customer", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[2, 0]);

				control.MoveUpButton_Click(this, new EventArgs());
				AssertEquals("Test 4.1 - can't move up as already at the top", "Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Test 4.1", "Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Test 4.1", "Controlling Customer", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[2, 0]);
			}
		}

		public void TestResetButton()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;

			using (Form dummyForm = new Form())
			using (ClientInTemplateSelectionControl control = new ClientInTemplateSelectionControl())
			{
				control.SetDataBinding(collection, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				SelectShipmentRow(collection, control);
				control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid.SelectAllElements();
				control.AddOrgTypeButton_Click(this, new EventArgs());

				AssertEquals("Precondition 1", "Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Precondition 2", "Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Precondition 3", "Controlling Customer", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[2, 0]);

				control.ResetButton_Click(this, new EventArgs());

				AssertEquals("Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Controlling Customer", control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid[0, 0]);

				control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(1);
				control.MoveUpButton_Click(this, new EventArgs());
				control.ResetButton_Click(this, new EventArgs());

				AssertEquals("Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Controlling Customer", control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid[0, 0]);

				control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(0);
				control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid.Select(1);
				control.RemoveOrgTypeButon_Click(this, new EventArgs());
				control.ResetButton_Click(this, new EventArgs());

				AssertEquals("Consignee / Consignor", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[0, 0]);
				AssertEquals("Local Client", control.ClientInTemplateSelectionCriteriaSelectedOrgTypesGrid[1, 0]);
				AssertEquals("Controlling Customer", control.ClientInTemplateSelectionCriteriaAvailableOrgTypesGrid[0, 0]);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			var list = new ClientInTemplateSelectionCriteriaCollection();
			var item = list.AddNew();
			return list;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ClientInTemplateSelectionControl)control).ReadOnly;
		}

		static void SelectShipmentRow(ClientInTemplateSelectionCriteriaCollection collection, ClientInTemplateSelectionControl control)
		{
			var shipmentRow = collection.Cast<ClientInTemplateSelectionCriteria>()
				.Single(x => x.ProcessTaskCode == ClientInTemplateSelectionProcessTypeList.Codes.Shipment);
			control.ClientInTemplateSelectionCriteriaGrid.SelectSingleElement(shipmentRow);
		}

		#endregion
	}
}
