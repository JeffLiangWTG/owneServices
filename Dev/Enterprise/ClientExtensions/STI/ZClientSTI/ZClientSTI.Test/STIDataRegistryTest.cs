using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.STI.Testing
{
	[TestedType(typeof(STIDataRegistry))]
	public class STIDataRegistryTest : RegistryItemSetTestCase<STIDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Should be 9 Items in the Registry", 9, AllItems.Count);
			AssertVisible(ItemSet.OrganisationExportDirectoryRaw);
			AssertVisible(ItemSet.InvoiceHeaderExportDirectoryRaw);
			AssertVisible(ItemSet.InvoiceLinesExportDirectoryRaw);
			AssertVisible(ItemSet.ShipmentExportDirectoryRaw);
			AssertVisible(ItemSet.DataExportHighWaterMarkRaw);
			AssertVisible(ItemSet.ExportingOrganisationsForTheFirstTimeRaw, true);
			AssertVisible(ItemSet.DisableOrganisationExportRaw);
			AssertVisible(ItemSet.DisableShipmentExportRaw);
			AssertVisible(ItemSet.DisableInvoiceExportRaw);
		}

		public void TestOrganisationExportDirectory()
		{
			ItemSet.OrganisationExportDirectory = "C Drive";
			AssertEquals("Organisation Export Directory", "C Drive", ItemSet.OrganisationExportDirectory);
		}

		public void TestInvoiceHeaderExportDirectory()
		{
			ItemSet.InvoiceHeaderExportDirectory = "F Drive";
			AssertEquals("Invoice Header Export Directory", "F Drive", ItemSet.InvoiceHeaderExportDirectory);
		}

		public void TestInvoiceLinesExportDirectory()
		{
			ItemSet.InvoiceLinesExportDirectory = "X Drive";
			AssertEquals("Invoice Lines Export Directory", "X Drive", ItemSet.InvoiceLinesExportDirectory);
		}

		public void TestShipmentExportDirectory()
		{
			ItemSet.ShipmentExportDirectory = "G Drive";
			AssertEquals("Invoice Lines Export Directory", "G Drive", ItemSet.ShipmentExportDirectory);
		}

		public void TestDataExportHighWaterMark()
		{
			ZDateTime lastRun = new ZDateTime(2006, 2, 16, 12, 51, 48);
			ItemSet.DataExportHighWaterMark = lastRun;
			AssertEquals("Data Export High Water Mark", lastRun, ItemSet.DataExportHighWaterMark);
			ZDateTime systemHighWaterMark = (SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value);
			ItemSet.DataExportHighWaterMark = ZDateTime.Invalid;
			AssertEquals("Data Export High Water Mark should be SystemHighWaterMark", systemHighWaterMark, ItemSet.DataExportHighWaterMark);
		}

		public void TestExportingOrganisationsForTheFirstTime()
		{
			AssertEquals("Default Value should be true", true, ItemSet.ExportingOrganisationsForTheFirstTimeRaw.DefaultValue);
			ItemSet.ExportingOrganisationsForTheFirstTime = false;
			AssertEquals("Export orgs for the first time", false, ItemSet.ExportingOrganisationsForTheFirstTime);
			ItemSet.ExportingOrganisationsForTheFirstTime = true;
			AssertEquals("Export orgs for the first time", true, ItemSet.ExportingOrganisationsForTheFirstTime);
		}

		public void TestDisableOrganisationExport()
		{
			AssertEquals("Default Value should be false", false, ItemSet.DisableOrganisationExportRaw.DefaultValue);
			ItemSet.DisableOrganisationExport = true;
			AssertEquals("Disable Organisation Export", true, ItemSet.DisableOrganisationExport);
			ItemSet.DisableOrganisationExport = false;
			AssertEquals("Disable Organisation Export", false, ItemSet.DisableOrganisationExport);
		}

		public void TestDisableShipmentExport()
		{
			AssertEquals("Default Value should be false", false, ItemSet.DisableShipmentExportRaw.DefaultValue);
			ItemSet.DisableShipmentExport = true;
			AssertEquals("Disable Shipment Export", true, ItemSet.DisableShipmentExport);
			ItemSet.DisableShipmentExport = false;
			AssertEquals("Disable Shipment Export", false, ItemSet.DisableShipmentExport);
		}

		public void TestDisableInvoiceExport()
		{
			AssertEquals("Default Value should be false", false, ItemSet.DisableInvoiceExportRaw.DefaultValue);
			ItemSet.DisableInvoiceExport = true;
			AssertEquals("Disable Invoice Export", true, ItemSet.DisableInvoiceExport);
			ItemSet.DisableInvoiceExport = false;
			AssertEquals("Disable Invoice Export", false, ItemSet.DisableInvoiceExport);
		}
	}
}
