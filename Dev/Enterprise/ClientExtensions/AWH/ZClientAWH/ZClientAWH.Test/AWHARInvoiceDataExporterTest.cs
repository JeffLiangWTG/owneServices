using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.AWH.Testing
{
	[TestedType(typeof(AWHARInvoiceDataExporter))]
	public class AWHARInvoiceDataExporterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2005, 10, 9, 10, 30, 20)]
		public void TestGenerateFileName()
		{
			AWHARInvoiceDataExporter exporter = new AWHARInvoiceDataExporter(Factory);
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			OrgCusCode lscCusCode = helper.Header.CustomsCodes.AddNew();
			lscCusCode.OK_RN_NKCodeCountry = Env.CurrentCompany.Country.Code;
			lscCusCode.OK_CodeType = "LSC";
			lscCusCode.OK_CustomsRegNo = "CusRegNo";
			exporter.FilterProvider.CurrentBatchNo = 89;
			string fileName = exporter.GenerateFileName();
			ZString currentBatchNumber = exporter.FilterProvider.CurrentBatchNo.ToString().PadLeft(4, '0');
			AssertEquals("Expected Filename", "PREFIX_" + currentBatchNumber + "_20051009103020.csv", fileName);
		}

		public void TestFilterProvider()
		{
			AWHARInvoiceDataExporter exporter = new AWHARInvoiceDataExporter(Factory);
			Assert(!exporter.FilterProvider.IncludeAccrualsPosting);
			Assert(!exporter.FilterProvider.IncludeAccrualsReversing);
			Assert(!exporter.FilterProvider.IncludeWIPsPosting);
			Assert(!exporter.FilterProvider.IncludeWIPsReversing);
			Assert(exporter.FilterProvider.IncludeARInvoices);
			Assert(exporter.FilterProvider.IncludeARCreditNotes);
			Assert(exporter.FilterProvider.IncludeARInvoices);
			Assert(exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);
			Assert(exporter.FilterProvider.ExcludeJobRelatedTransactionsForAP);
			Assert(!exporter.FilterProvider.ExcludeJobRelatedTransactionsForAR);
			Assert(!exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);
			AssertEquals("Current Batch Number is not zero", 0, exporter.FilterProvider.CurrentBatchNo);
		}

		public void TestFormatIsCorrect()
		{
			AssertEquals("expected IFlatFileFormat implementation was not of the expected type", typeof(CsvFlatFileFormat), Exporter.Format.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AWHARInvoiceDataExporter(Factory);
		}

#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			AWHDataRegistry.Instance.ExportFilePrefix = "PREFIX";
			Exporter = new AWHARInvoiceDataExporterForTest(Factory);
		}

		AWHARInvoiceDataExporterForTest Exporter;
		class AWHARInvoiceDataExporterForTest : AWHARInvoiceDataExporter
		{
			public AWHARInvoiceDataExporterForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new FlatFileConverter Converter
			{
				get
				{
					return base.Converter;
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
