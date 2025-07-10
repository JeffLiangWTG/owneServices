using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSDataExporterTest : TestCaseWithFactory
	{
		public void TestFilterProvider_CMSLastDateExportIsInvalid()
		{
			AssertFilterProvider(Exporter.FilterProvider);
		}

		void AssertFilterProvider(TransactionExportFilterProvider filterProvider)
		{
			AssertEquals("IncludeAccrualsPosting", false, filterProvider.IncludeAccrualsPosting);
			AssertEquals("IncludeAccrualsReversing", false, filterProvider.IncludeAccrualsReversing);
			AssertEquals("IncludeWIPsPosting", false, filterProvider.IncludeWIPsPosting);
			AssertEquals("IncludeWIPsReversing", false, filterProvider.IncludeWIPsReversing);
			AssertEquals("IncludeAPAdjustmentNotes", false, filterProvider.IncludeAPAdjustmentNotes);
			AssertEquals("IncludeAPCreditNotes", false, filterProvider.IncludeAPCreditNotes);
			AssertEquals("IncludeAPInvoices", false, filterProvider.IncludeAPInvoices);
			AssertEquals("IncludeARAdjustmentNotes", true, filterProvider.IncludeARAdjustmentNotes);
			AssertEquals("IncludeARCreditNotes", true, filterProvider.IncludeARCreditNotes);
			AssertEquals("IncludeARInvoices", true, filterProvider.IncludeARInvoices);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAP", false, Exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAR", false, Exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);
		}

		public void TestFormat()
		{
			AssertEquals("IFlatFileFormat implementation was not of the expected type", typeof(CMSFlatFileFormat), Exporter.Format.GetType());
		}

#region SetUp
		protected override void SetUp()
		{
			base.SetUp();
			Exporter = new CMSDataExporterTestClass(0, Factory);
		}

		CMSDataExporterTestClass Exporter;
		class CMSDataExporterTestClass : CMSDataExporter
		{
			public CMSDataExporterTestClass(int batchNumber, BusinessObjectFactory factory) : base(batchNumber, factory)
			{
			}

			protected override AccountingFlatFileConverter Converter
			{
				get
				{
					return null;
				}
			}

			public new FlatFileFormat Format
			{
				get
				{
					return base.Format;
				}
			}
		}
#endregion
	}
}
