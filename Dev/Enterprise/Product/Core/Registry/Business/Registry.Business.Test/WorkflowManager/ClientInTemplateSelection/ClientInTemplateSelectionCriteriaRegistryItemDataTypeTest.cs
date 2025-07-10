using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ClientInTemplateSelectionCriteriaCollectionRegistryDataType))]
	sealed class ClientInTemplateSelectionCriteriaRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ClientInTemplateSelectionCriteriaCollectionRegistryDataType>
	{
		public override void TestISDefaultImmutable()
		{
			var rego = new ClientInTemplateSelectionCriteriaCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new ClientInTemplateSelectionCriteriaCollection());
			var item = rego.Value;
			AssertEquals(item, rego.Value);
			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				var pair = item.AddNew();
				pair.ProcessTaskCode = "NX";
			});
			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				var pair = item.AddNew();
				pair.ProcessTaskCode = "NY";
			});
			var clone = item.Clone(item.CurrentFallbackLevel, item.Factory);
			AssertNoExceptionThrown(() => clone.AddNew());
		}

		public void TestDefaultCollectionValue_Count()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;
			AssertEquals("Should have four items", 4, collection.Count);
		}

		public void TestDefaultCollectionValue_Shipment()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;
			var item = collection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.Shipment);
			AssertType("Should be ClientInTemplateSelectionCriteria", typeof(ClientInTemplateSelectionCriteria), item);

			AssertEquals("Selected items should have 2 values", 2, item.SelectedItems.Count);
			AssertEquals("Available items should have 1 values", 1, item.AvailableItems.Count);

			AssertEquals("CON", item.SelectedItems[0].OrgTypeCode);
			AssertEquals("LOC", item.SelectedItems[1].OrgTypeCode);
			AssertEquals("CPY", item.AvailableItems[0].OrgTypeCode);

			AssertEquals("Consignee / Consignor", item.SelectedItems[0].OrgTypeDescription);
			AssertEquals("Local Client", item.SelectedItems[1].OrgTypeDescription);
			AssertEquals("Controlling Customer", item.AvailableItems[0].OrgTypeDescription);
		}

		public void TestDefaultCollectionValue_QuotedBooking()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;
			var item = collection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.QuotedBooking);
			AssertType("Should be ClientInTemplateSelectionCriteria", typeof(ClientInTemplateSelectionCriteria), item);

			AssertEquals("Selected items should have 1 values", 1, item.SelectedItems.Count);
			AssertEquals("Available items should have 1 values", 1, item.AvailableItems.Count);

			AssertEquals("CLI", item.SelectedItems[0].OrgTypeCode);
			AssertEquals("CPY", item.AvailableItems[0].OrgTypeCode);

			AssertEquals("Client", item.SelectedItems[0].OrgTypeDescription);
			AssertEquals("Controlling Customer", item.AvailableItems[0].OrgTypeDescription);
		}

		public void TestDefaultCollectionValue_ForwardingOrder()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;
			var item = collection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.ForwardingOrder);
			AssertType("Should be ClientInTemplateSelectionCriteria", typeof(ClientInTemplateSelectionCriteria), item);

			AssertEquals("Selected items should have 2 values", 2, item.SelectedItems.Count);
			AssertEquals("Available items should have 0 value", 0, item.AvailableItems.Count);

			AssertEquals(ClientInTemplateSelectionOrgTypeList.Codes.ControllingCustomer, item.SelectedItems[0].OrgTypeCode);
			AssertEquals(ClientInTemplateSelectionOrgTypeList.Codes.BuyerSupplier, item.SelectedItems[1].OrgTypeCode);

			AssertEquals("Controlling Customer", item.SelectedItems[0].OrgTypeDescription);
			AssertEquals("Buyer / Supplier", item.SelectedItems[1].OrgTypeDescription);
		}

		public void TestDefaultCollectionValue_LandTransportConsignment()
		{
			var collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;
			var item = collection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);
			AssertType("Should be ClientInTemplateSelectionCriteria", typeof(ClientInTemplateSelectionCriteria), item);

			var orgTypes = item.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().ToArray();

			AssertArrayEqualsByElements("We have a specific order that we want the default options to be listed in.", new[]
			{
				ClientInTemplateSelectionOrgTypeList.Codes.BookingParty,
				ClientInTemplateSelectionOrgTypeList.Codes.LocalClient,
				ClientInTemplateSelectionOrgTypeList.Codes.PickupAddressOrganization,
				ClientInTemplateSelectionOrgTypeList.Codes.DeliveryAddressOrganization,
			}, orgTypes.Select(x => x.OrgTypeCode.ToString()).ToArray());

			AssertArrayEqualsByElements(new[]
			{
				"Booking Party",
				"Local Client",
				"Pickup Address Organization",
				"Delivery Address Organization",
			}, orgTypes.Select(x => x.OrgTypeDescription.ToString()).ToArray());

			AssertContainsExactElementsInAnyOrder("All four current options should be selected, leaving the available options list empty.",
				Array.Empty<ClientInTemplateSelectionCriteriaOrgType>(), item.AvailableItems);
		}

		public void TestDefaultProcessTaskListRaceCondition()
		{
			for (int i = 0; i < 1000; i++)
			{
				var barrier = new Barrier(100);
				ClientInTemplateSelectionCriteriaCollectionRegistryItem.defaultProcessTaskList = null;

				var threads = Enumerable.Range(0, 100).Select(t => new Thread(() =>
				{
					barrier.SignalAndWait();
					Assert(object.ReferenceEquals(ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultProcessTaskList, ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultProcessTaskList));
				})).ToArray();

				threads.ForEach(t => t.Start());
				foreach (var thread in threads)
				{
					thread.Join();
				}
			}
			Assert(true);
		}

		public void TestGetDefaultValueByCode()
		{
			var item1 = ClientInTemplateSelectionCriteria.GetDefaultValueByCode("SHP");
			AssertNotNull(item1);

			var item2 = ClientInTemplateSelectionCriteria.GetDefaultValueByCode("XYZ");
			AssertNull(item2);

			var item3 = ClientInTemplateSelectionCriteria.GetDefaultValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.ForwardingOrder);
			AssertNotNull(item3);

			var item4 = ClientInTemplateSelectionCriteria.GetDefaultValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);
			AssertNotNull(item4);
		}

		public void TestNewProcessTypesAdded_WhenCustomerIsOverridingRegistryItem_ShouldIncludeDefaultsForNewProcessType()
		{
			var defaults = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;
			var clonedCollection = (ClientInTemplateSelectionCriteriaCollection)defaults
				.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), defaults.Factory);
			var shipmentCriteria = clonedCollection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.Shipment);

			AssertContainsExactElementsInAnyOrder(@"Verifying that shipment does, indeed, not include all 3 org types by default.
Apologies to future devs whose work to add new org types for Shipment may cause this test to fail.", new[]
			{
				ClientInTemplateSelectionOrgTypeList.Codes.ConsigneeConsignor,
				ClientInTemplateSelectionOrgTypeList.Codes.LocalClient,
			}, shipmentCriteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeCode.ToString()));

			var shipmentOrgTypeNotSelectedByDefault = shipmentCriteria.AvailableItems.First();
			shipmentCriteria.SelectedItems.Add(shipmentOrgTypeNotSelectedByDefault);
			shipmentCriteria.AvailableItems.Remove(shipmentOrgTypeNotSelectedByDefault);

			var landTransportCriteria = clonedCollection.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);
			clonedCollection.RemoveAndDelete(landTransportCriteria);

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clonedCollection);

			var registryItem = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			landTransportCriteria = registryItem.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);
			AssertNotNull("The settings for the new process type should be added from the defaults even though it wasn't saved in the existing serialization.", landTransportCriteria);

			shipmentCriteria = registryItem.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.Shipment);
			AssertContainsExactElementsInAnyOrder(@"The bytes added to the database include 3 selected org types for Shipment (only 2 are selected by default).
The existing values should still be stored correctly in the database.", new[]
			{
				ClientInTemplateSelectionOrgTypeList.Codes.ConsigneeConsignor,
				ClientInTemplateSelectionOrgTypeList.Codes.LocalClient,
				ClientInTemplateSelectionOrgTypeList.Codes.ControllingCustomer,
			}, shipmentCriteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeCode.ToString()));
		}

		#region Implementation

		protected override ClientInTemplateSelectionCriteriaCollectionRegistryDataType GetNewDataType()
		{
			return new ClientInTemplateSelectionCriteriaCollectionRegistryDataType(new ClientInTemplateSelectionCriteriaCollection());
		}

		protected override string ExpectedEditorName
		{
			get { return "ClientInTemplateSelectionRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ClientInTemplateSelectionCriteriaCollection collection = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue;

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,
				0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,
				0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,
				0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,
				0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,
				0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,62,0,60,0,80,0,114,0,111,0,99,0,101,0,115,0,115,0,84,0,97,0,115,0,107,0,67,0,111,0,100,0,101,
				0,62,0,79,0,82,0,68,0,60,0,47,0,80,0,114,0,111,0,99,0,101,0,115,0,115,0,84,0,97,0,115,0,107,0,67,0,111,0,100,0,101,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,
				0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,
				0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,
				0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,67,0,80,0,89,0,60,0,47,0,79,
				0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,
				0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,
				0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,
				0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,66,0,85,0,89,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,
				0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,
				0,121,0,112,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,
				0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,
				0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,
				0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,62,0,60,0,80,0,114,0,111,0,99,0,101,0,115,
				0,115,0,84,0,97,0,115,0,107,0,67,0,111,0,100,0,101,0,62,0,76,0,84,0,67,0,60,0,47,0,80,0,114,0,111,0,99,0,101,0,115,0,115,0,84,0,97,0,115,0,107,0,67,0,111,0,100,0,101,0,62,0,60,0,65,
				0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,
				0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,
				0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,
				0,100,0,101,0,62,0,66,0,80,0,84,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,
				0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,
				0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,
				0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,76,0,79,0,67,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,
				0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,
				0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,
				0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,
				0,101,0,62,0,80,0,65,0,79,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,
				0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,0,105,
				0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,
				0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,68,0,65,0,79,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,
				0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,
				0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,
				0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,67,0,108,
				0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,62,0,60,
				0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,
				0,62,0,60,0,80,0,114,0,111,0,99,0,101,0,115,0,115,0,84,0,97,0,115,0,107,0,67,0,111,0,100,0,101,0,62,0,81,0,66,0,75,0,60,0,47,0,80,0,114,0,111,0,99,0,101,0,115,0,115,0,84,0,97,0,115,
				0,107,0,67,0,111,0,100,0,101,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,
				0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,
				0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,
				0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,67,0,76,0,73,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,
				0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,
				0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,
				0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,
				0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,
				0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,62,0,60,0,80,0,114,0,111,0,99,0,101,
				0,115,0,115,0,84,0,97,0,115,0,107,0,67,0,111,0,100,0,101,0,62,0,83,0,72,0,80,0,60,0,47,0,80,0,114,0,111,0,99,0,101,0,115,0,115,0,84,0,97,0,115,0,107,0,67,0,111,0,100,0,101,0,62,0,60,
				0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,
				0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,
				0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,
				0,111,0,100,0,101,0,62,0,67,0,79,0,78,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,
				0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,67,
				0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,
				0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,0,111,0,100,0,101,0,62,0,76,0,79,0,67,0,60,0,47,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,67,
				0,111,0,100,0,101,0,62,0,60,0,47,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,
				0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,
				0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,79,0,114,0,103,0,84,0,121,0,112,0,101,0,62,0,60,0,47,
				0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,
				0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,108,0,105,0,101,0,110,0,116,0,73,0,110,0,84,0,101,0,109,0,112,0,108,0,97,0,116,0,101,0,83,0,101,0,108,0,101,0,99,0,116,0,105,
				0,111,0,110,0,67,0,114,0,105,0,116,0,101,0,114,0,105,0,97,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
