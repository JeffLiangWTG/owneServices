using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	 abstract class ExporterDocumentsBaseTest : BaseRunDocumentsTest
	{
		public void TestBankDraft()
		{
			RunDocument("Bank Draft");
		}

		public void TestDocumentaryCollectionForm()
		{
			RunDocument("Documentary Collection Form");
		}

		public virtual void TestShippersLetterOfInstruction()
		{
			RunDocument("Shippers Letter Of Instruction");
		}

		public void TestExportLetter()
		{
			RunDocument("Export Letter");
		}

		public void TestBeneficiaryCertificate()
		{
			RunDocument("Beneficiary Certificate");
		}

		public void TestCertificateOfInsurance()
		{
			RunDocument("Certificate Of Insurance");
		}

		public void TestCertificateofOrigin()
		{
			RunDocument("Certificate of Origin");
		}

		public void TestCombinedInvoiceandCertifOfOrigin()
		{
			RunDocument("Combined Invoice and Certif. of Origin");
		}

		public void TestCommercialInvoice()
		{
			RunDocument("Commercial Invoice");
		}

		public void TestContainerList()
		{
			RunDocument("Container List");
		}

		public void TestExportPackingList()
		{
			RunDocument("Export Packing List");
		}

		public void TestSummaryCommercialInvoice()
		{
			RunDocument("Summary Commercial Invoice");
		}

		protected override void RunDocument(string menuItemName)
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuItemName);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Exporter Doc");
			RunDocument(filter);
		}

		ZString countryToStartWith;
		protected override void SetUp()
		{
			base.SetUp();
			countryToStartWith = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("AI");
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(countryToStartWith);
		}
	}
}
