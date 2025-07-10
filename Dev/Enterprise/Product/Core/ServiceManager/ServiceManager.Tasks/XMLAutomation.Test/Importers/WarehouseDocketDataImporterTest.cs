using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class WarehouseDocketDataImporterTest : TestCaseWithFactory
	{
		public void TestImportInvalidXmlSchema()
		{
			int receiveCountBefore = GetReceiveCount(Factory);
			int ordersCountBefore = GetOrdersCount(Factory);
			int wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));

			var whsDocketWithBadXmlSchemaPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WhsDocketWithBadXmlSchema.xml");
			using (var reader = new StreamReader(whsDocketWithBadXmlSchemaPath))
			{
				Importer.ImportData(reader, "WhsDocketWithBadXmlSchema.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
			AssertEquals("Should have no new warehouse Receive records", 0, GetReceiveCount(Factory) - receiveCountBefore);
			AssertEquals("Should have no new warehouse Order records", 0, GetOrdersCount(Factory) - ordersCountBefore);

			AssertEquals("Should have no new EDI Interchange", 0, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		}

		public void TestImportNoInterchange()
		{
			int receiveCountBefore = GetReceiveCount(Factory);
			int ordersCountBefore = GetOrdersCount(Factory);
			int wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));

			SetupEnvironment();

			var whsDocketNoInterchangeInfoPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WhsDocketNoInterchangeInfo.xml");
			using (var reader = new StreamReader(whsDocketNoInterchangeInfoPath))
			{
				Importer.ImportData(reader, "WhsDocketNoInterchangeInfo.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}

			AssertEquals("Should have 1 new warehouse Receive record", 1, GetReceiveCount(Factory) - receiveCountBefore);
			AssertEquals("Should have 1 new warehouse Order record", 1, GetOrdersCount(Factory) - ordersCountBefore);

			AssertEquals("Should have no new EDI Interchange", 0, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		}

		public void TestImportNoInterchangeNum()
		{
			int receiveCountBefore = GetReceiveCount(Factory);
			int ordersCountBefore = GetOrdersCount(Factory);
			int wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));

			SetupEnvironment();
			var whsDocketNoInterchangeNumberPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WhsDocketNoInterchangeNumber.xml");
			using (var reader = new StreamReader(whsDocketNoInterchangeNumberPath))
			{
				Importer.ImportData(reader, "WhsDocketNoInterchangeNumber.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
			AssertEquals("Should have 1 new warehouse Receive record", 1, GetReceiveCount(Factory) - receiveCountBefore);
			AssertEquals("Should have 1 new warehouse Order record", 1, GetOrdersCount(Factory) - ordersCountBefore);

			AssertEquals("Should have no new EDI Interchange", 0, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		}

		public void TestImportWhsOrderWithoutErrors()
		{
			int ordersCountBefore = GetOrdersCount(Factory);
			int wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));

			SetupEnvironment();

			using (var reader = new StreamReader(WhsOrderPath))
			{
				Importer.ImportData(reader, "WhsOrder.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
			AssertEquals("Should have 1 new warehouse order record", 1, GetOrdersCount(Factory) - ordersCountBefore);

			AssertEquals("Should have no new WDFEDIInterchange", 0, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		}

		public void TestImportWhsOrderWithErrors()
		{
			int ordersCountBefore = GetOrdersCount(Factory);
			int wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));

			using (var reader = new StreamReader(WhsOrderPath))
			{
				Importer.ImportData(reader, "WhsOrder.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
			AssertEquals("Should have no new warehouse order records", 0, GetOrdersCount(Factory) - ordersCountBefore);

			AssertEquals("Should have no new WDFEDIInterchange", 0, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		}

		public void TestImportWhsReceiveWithoutErrors()
		{
			int receiveCountBefore = GetReceiveCount(Factory);
			int wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));

			SetupEnvironment();

			using (var reader = new StreamReader(WhsReceivePath))
			{
				Importer.ImportData(reader, "WhsReceive.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
			AssertEquals("Should have 1 new warehouse Receive record", 1, GetReceiveCount(Factory) - receiveCountBefore);

			AssertEquals("Should have no new WDFEDIInterchange", 0, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		}

		public void TestImportWhsReceiveWithErrors()
		{
			int receiveCountBefore = GetReceiveCount(Factory);
			int wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));

			using (var reader = new StreamReader(WhsReceivePath))
			{
				Importer.ImportData(reader, "WhsReceive.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
			AssertEquals("Should have no new warehouse Receive records", 0, GetReceiveCount(Factory) - receiveCountBefore);

			AssertEquals("Should have no new WDFEDIInterchange", 0, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		}

		public void TestImportWhsDocketWithSameReference()
		{
			var whsDocketWithSameReferencePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WhsDocketWithSameReference.xml");
			using (var reader = new StreamReader(whsDocketWithSameReferencePath))
			{
				var buffer = new NotificationBuffer();
				AssertNoExceptionThrown(() => Importer.ImportData(reader, "WhsDocketWithSameReference.xml", buffer, SourceInfo.EmptySourceInfo));
			}
		}

		public void TestAdapter()
		{
			AssertNotNull(Importer.Adapter);
			AssertEquals(typeof(WhsDocketValueObjectDataUniversalAdapter), Importer.Adapter.GetType());
		}

		#region Implementation

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

		string whsOrderPath;
		string WhsOrderPath
		{
			get
			{
				if (string.IsNullOrEmpty(whsOrderPath))
				{
					whsOrderPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WhsOrder.xml");
				}
				return whsOrderPath;
			}
		}

		string whsReceivePath;
		string WhsReceivePath
		{
			get
			{
				if (string.IsNullOrEmpty(whsReceivePath))
				{
					whsReceivePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.WhsReceive.xml");
				}
				return whsReceivePath;
			}
		}

		int GetReceiveCount(BusinessObjectFactory factory)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(WhsReceive));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, SQLComparisonOperator.Equal, Warehouse.Transactions.CodeLists.DocketType.Codes.Receive);
			return factory.GetDatabaseCount(typeof(WhsReceive), query);
		}

		int GetOrdersCount(BusinessObjectFactory factory)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(WhsOrder));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, SQLComparisonOperator.Equal, Warehouse.Transactions.CodeLists.DocketType.Codes.Order);
			return factory.GetDatabaseCount(typeof(WhsOrder), query);
		}

		void SetupEnvironment()
		{
			var clientPK = Helper.CreateClient("DANPACWLG");
			var whs = Helper.CreateWarehouse("TWH");
			var product = (OrgSupplierPart)Helper.CreateProduct(clientPK, "PRD");
			product.OP_Desc = "Test Product";
			Factory.Save();
		}

		IWhsTransactionTestHelper Helper
		{
			get
			{
				return helper ?? (helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory));
			}
		}

		WarehouseDocketDataImporter Importer
		{
			get
			{
				if (importer == null)
				{
					importer = new WarehouseDocketDataImporter();
				}
				return importer;
			}
		}

		WarehouseDocketDataImporter importer;
		IWhsTransactionTestHelper helper;

		#endregion
	}
}
