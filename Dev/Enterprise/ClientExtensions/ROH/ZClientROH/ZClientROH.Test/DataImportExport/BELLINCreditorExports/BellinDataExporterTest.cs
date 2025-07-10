using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.Bellin.Testing
{
	[TestedType(typeof(BellinDataExporter))]
	public class BellinDataExporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilterProvider()
		{
			BellinDataExporter exporter = new BellinDataExporter(Factory);
			AssertEquals("IncludeAccrualsPosting", false, exporter.FilterProvider.IncludeAccrualsPosting);
			AssertEquals("IncludeAccrualsReversing", false, exporter.FilterProvider.IncludeAccrualsReversing);
			AssertEquals("IncludeWIPsPosting", false, exporter.FilterProvider.IncludeWIPsPosting);
			AssertEquals("IncludeWIPsReversing", false, exporter.FilterProvider.IncludeWIPsReversing);
			AssertEquals("IncludeAPAdjustmentNotes", true, exporter.FilterProvider.IncludeAPAdjustmentNotes);
			AssertEquals("IncludeAPCreditNotes", true, exporter.FilterProvider.IncludeAPCreditNotes);
			AssertEquals("IncludeAPInvoices", true, exporter.FilterProvider.IncludeAPInvoices);
			AssertEquals("IncludeARAdjustmentNotes", false, exporter.FilterProvider.IncludeARAdjustmentNotes);
			AssertEquals("IncludeARCreditNotes", false, exporter.FilterProvider.IncludeARCreditNotes);
			AssertEquals("IncludeARInvoices", false, exporter.FilterProvider.IncludeARInvoices);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAP", false, exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAR", false, exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);
		}

		public void TestFormat()
		{
			BellinDataExporterTestClass exporter = new BellinDataExporterTestClass(Factory);
			FlatFileFormat format = exporter.Format;
			AssertSame("Format was not Lazy Loaded", format, exporter.Format);
			AssertEquals("Should be CSVFlatFileFormat", typeof(CsvFlatFileFormat), exporter.Format.GetType());
		}

		public void TestConverter()
		{
			BellinDataExporterTestClass exporter = new BellinDataExporterTestClass(Factory);
			AccountingFlatFileConverter converter = exporter.Converter;
			AssertSame("Converter was not Lazy Loaded", converter, exporter.Converter);
			AssertEquals("Should be BellinFlatFileConverter", typeof(BellinFlatFileConverter), exporter.Converter.GetType());
		}

		public void TestInvoiceBatchFilter()
		{
			BellinDataExporterTestClass exporter = new BellinDataExporterTestClass(Factory);
			FinancialInvoiceTransactionExportFilter filter = exporter.InvoiceBatchFilter;
			AssertSame("Invoice Batch Filter was not Lazy Loaded", filter, exporter.InvoiceBatchFilter);
			AssertEquals("Should be BellinInvoiceBatchFilter", typeof(BellinInvoiceBatchFilter), exporter.InvoiceBatchFilter.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BellinDataExporter(Factory);
		}

		#region Implementation
		class BellinDataExporterTestClass : BellinDataExporter
		{
			public BellinDataExporterTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new FlatFileFormat Format
			{
				get
				{
					return base.Format;
				}
			}

			public new AccountingFlatFileConverter Converter
			{
				get
				{
					return base.Converter;
				}
			}

			public new FinancialInvoiceTransactionExportFilter InvoiceBatchFilter
			{
				get
				{
					return base.InvoiceBatchFilter;
				}
			}
		}
		#endregion
	}
}
