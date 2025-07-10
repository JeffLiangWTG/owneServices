using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(AccountingTransactionExporter))]
	sealed class AccountingTransactionExporterTest : XmlAccountingTransactionExporterTest
	{
		public void TestARAPExportedInvoicesLists()
		{
			AssertEquals("ARInvoices List should be empty", 0, exporter.ExportedARInvoices.Count);
			AssertEquals("APInvoices List should be empty", 0, exporter.ExportedAPInvoices.Count);
			AssertEquals("TotalAPAmount should be 0.0", new ZDecimal(0), exporter.TotalAPAmount);
			AssertEquals("TotalARAmount should be 0.0", new ZDecimal(0), exporter.TotalARAmount);

			using (MemoryStream stream = new MemoryStream())
			{
				exporter.Export(stream);
			}

			AssertEquals("ARInvoices List should contain 1 invoice", 1, exporter.ExportedARInvoices.Count);
			AssertEquals("APInvoices List should contain 1 invoice", 1, exporter.ExportedAPInvoices.Count);
			AssertEquals("TotalAPAmount should be -148.5", new ZDecimal(-148.5), exporter.TotalAPAmount);
			AssertEquals("TotalARAmount should be 220.0", new ZDecimal(220), exporter.TotalARAmount);

			exporter.ClearExportedLists();
			AssertEquals("ARInvoices List should be empty", 0, exporter.ExportedARInvoices.Count);
			AssertEquals("APInvoices List should be empty", 0, exporter.ExportedAPInvoices.Count);
			AssertEquals("TotalAPAmount should be 0.0", new ZDecimal(0), exporter.TotalAPAmount);
			AssertEquals("TotalARAmount should be 0.0", new ZDecimal(0), exporter.TotalARAmount);
		}

		#region NonPersistenBusinessObjectTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccountingTransactionExporter(Factory);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			exporter = new AccountingTransactionExporter(Factory);
			exporter.FilterProvider.IncludeAPInvoices = true;
			exporter.FilterProvider.IncludeARInvoices = true;
			exporter.FilterProvider.CurrentBatchNo = 0;
			Factory.Save();
		}

		AccountingTransactionExporter exporter;
	}
}
