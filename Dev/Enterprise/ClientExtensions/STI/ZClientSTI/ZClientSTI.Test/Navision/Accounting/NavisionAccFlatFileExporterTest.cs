using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(NavisionAccFlatFileExporterTestClass))]
	public class NavisionAccFlatFileExporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilterProvider()
		{
			NavisionAccFlatFileExporterTestClass exporter = new NavisionAccFlatFileExporterTestClass(Factory);
			Assert("AR Invoice should be included", exporter.FilterProvider.IncludeARInvoices);
			Assert("AR Credit Notes should be included", exporter.FilterProvider.IncludeARCreditNotes);
			Assert("AR Adjustment Notes should be included", !exporter.FilterProvider.IncludeARAdjustmentNotes);
			Assert("AP Invoice should be included", !exporter.FilterProvider.IncludeAPInvoices);
			Assert("AP Credit Notes should be included", !exporter.FilterProvider.IncludeAPCreditNotes);
			Assert("AP Adjustment Notes should be included", !exporter.FilterProvider.IncludeAPAdjustmentNotes);
			Assert("Include WIPs Posting", !exporter.FilterProvider.IncludeWIPsPosting);
			Assert("Inlucde WIPs Reversed", !exporter.FilterProvider.IncludeWIPsReversing);
			Assert("Include Accruals Posting", !exporter.FilterProvider.IncludeAccrualsPosting);
			Assert("Include Accruals Reversed", !exporter.FilterProvider.IncludeAccrualsReversing);
			Assert("Exclude Job Related AP Transactions", !exporter.FilterProvider.ExcludeJobRelatedTransactionsForAP);
			Assert("Exclude Job Related AR Transactions", !exporter.FilterProvider.ExcludeJobRelatedTransactionsForAR);
			Assert("Exclude NON Job Related AP Transactions", !exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);
			Assert("Exclude NON Job Related AR Transactions", !exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);
			AssertEquals("Current Batch Number should 0", 0, exporter.FilterProvider.CurrentBatchNo);
		}

		public void TestFlatFileFormat()
		{
			NavisionAccFlatFileExporterTestClass exporter = new NavisionAccFlatFileExporterTestClass(Factory);
			FlatFileFormat format = exporter.Format;
			AssertSame("Format was not lazy loaded", format, exporter.Format);
			AssertEquals("Format should be of type 'CsvFlatFileFormat'", typeof(CsvFlatFileFormat), format.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NavisionAccFlatFileExporterTestClass(Factory);
		}

		class NavisionAccFlatFileExporterTestClass : NavisionAccFlatFileExporter
		{
			public NavisionAccFlatFileExporterTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new FlatFileFormat Format
			{
				get
				{
					return base.Format;
				}
			}

			protected override AccountingFlatFileConverter Converter
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}
		}
	}
}
