using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.HEN.Nissan.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest("AU")]
	public class NissanInvoiceDataImporterTest : FlatFileDataImporterTestCase
	{
		public void TestImportInvoiceHeader_CrapFile()
		{
			AssertEquals(0, JobDec.Invoices.Count);
			GetDataImporter().ImportData(PathToCrapTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals(0, JobDec.Invoices.Count);
		}

		public void TestImportInvoiceHeader()
		{
			AssertEquals(0, JobDec.Invoices.Count);
			GetDataImporter().ImportData(PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals(1, JobDec.Invoices.Count);
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			return new NissanInvoiceDataImporter(JobDec);
		}

		protected override string PathToTestFile
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("ImportNissanInvoiceData.TestFiles.PROFORMA3.TXT");
			}
		}

		protected string PathToCrapTestFile
		{
			get
			{
				return resourceRetriever.SaveResourceToFile("ImportNissanInvoiceData.TestFiles.PROFORMA3__CRAP.TXT");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDec = Factory.NewWithValidTestData<JobDeclaration>();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		JobDeclaration JobDec;
		EmbeddedResourceRetriever resourceRetriever;
	}
}
