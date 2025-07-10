using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class InvoiceFlatFileImporterTest : TestCaseWithFactory
	{
		public void TestImportInvoices()
		{
			var numberOfCommercialInvoices = Factory.GetDatabaseCount(typeof(BaseJobComInvoiceHeader));

			var importer = new InvoiceFlatFileImporter(Constants.Groups.AllPK);
			var info = new FileInfo(CommercialInvoiceCsvPath);
			var buffer = new NotificationBuffer(new NotificationBuffer());
			importer.ImportFlatFile(info, buffer);

			var numberOfCommercialInvoicesAfterImport = Factory.GetDatabaseCount(typeof(BaseJobComInvoiceHeader));
			AssertEquals("1 Commercial Invoices were created in the import", numberOfCommercialInvoices + 1, numberOfCommercialInvoicesAfterImport);
		}

		public void TestEventLogWhenImporting()
		{
			var importer = new InvoiceFlatFileImporter(Constants.Groups.AllPK);
			var info = new FileInfo(CommercialInvoiceCsvPath);
			var buffer = new NotificationBuffer(new NotificationBuffer());
			importer.ImportFlatFile(info, buffer);

			AssertEquals("Processing records should have been notified", true, buffer.AsString.IndexOf("records processed") > -1);
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

		string commercialInvoiceCsvPath;
		string CommercialInvoiceCsvPath
		{
			get
			{
				if (string.IsNullOrEmpty(commercialInvoiceCsvPath))
				{
					commercialInvoiceCsvPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.CommercialInvoice.csv", "CommercialInvoice.csv");
				}
				return commercialInvoiceCsvPath;
			}
		}
	}
}
