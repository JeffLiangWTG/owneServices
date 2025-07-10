using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class WarehouseOrderFlatFileImporterTest : TestCaseWithFactory
	{
		public void TestImportWarehouseOrderFlatFileWithValidData()
		{
			SetupData();

			var warehouseOrderValidDataPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WarehouseOrderValidData.csv");
			var fileInfo = new FileInfo(warehouseOrderValidDataPath);
			Assert("Precondition - Import File should exist.", fileInfo.Exists);

			var buffer = new NotificationBuffer();
			var importer = new WarehouseOrderFlatFileImporter(NotificationGroup);

			var intialOrderCount = Factory.GetDatabaseCount(typeof(WhsOrder));
			importer.ImportFlatFile(fileInfo, buffer);
			AssertEquals("Valid order was not imported.", intialOrderCount + 1, Factory.GetDatabaseCount(typeof(WhsOrder)));
		}

		public void TestImportWarehouseOrderFlatFileWithInvalidData()
		{
			SetupData();

			var warehouseOrderInvalidDataPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WarehouseOrderInvalidData.csv");
			var fileInfo = new FileInfo(warehouseOrderInvalidDataPath);
			AssertEquals("Precondition - Import File should exist.", true, fileInfo.Exists);

			var buffer = new NotificationBuffer();
			var importer = new WarehouseOrderFlatFileImporter(NotificationGroup);

			var intialOrderCount = Factory.GetDatabaseCount(typeof(WhsOrder));
			importer.ImportFlatFile(fileInfo, buffer);
			AssertEquals("ImportingDataError should have been logged.", true, buffer.ContainsNotificationType(ErrorType.ImportingDataError));
			AssertEquals("Invalid order was imported.", intialOrderCount, Factory.GetDatabaseCount(typeof(WhsOrder)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		void SetupData()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_FullName = "Alcan Packaging Danaflex";
			client.OH_RL_NKClosestPort = "NZWLG";
			client.Addresses.MainAddress.OA_Address1 = "101 Collins Ave";

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "X8434";
			part.OP_Desc = "SK10 PLAIN MARAFLEX 380MM TUBING";
			part.RelatedOrganisations.AddOrganisationIfNotExist(client.PK, OrgPartRelation.RelationshipTypes.Owner, false);

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = helper.CreateWarehouse("BNE");
			Factory.Save();
		}

		GuidRegistryItem NotificationGroup
		{
			get { return NotificationDataRegistry.Instance.WarehouseImportNotificationGroup; }
		}
	}
}
