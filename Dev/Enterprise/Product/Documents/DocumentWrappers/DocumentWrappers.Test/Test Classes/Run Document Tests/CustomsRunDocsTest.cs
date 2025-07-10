using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.Customs
{
	public abstract class CustomsRunDocsTest : BaseRunDocumentsTest
	{
		public CustomsRunDocsTest() { }

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Customs; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestBankDraft()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bank Draft");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestBeneficiaryCertificate()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Beneficiary Certificate");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageAdvicewithReceipt()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice with Receipt");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCertificateOfInsurance()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Certificate of Insurance");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCertificateOfOrigin()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Certificate Of Origin");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestChargeSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Charge Sheet");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedCartageAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedCartageAdvicewithReceipt()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice with Receipt");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCommercialInvoice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Commercial Invoice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestContainerList()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Container List");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCoverSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDeclarationNotes()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Declaration Notes");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDisbursementNote()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Disbursement Note");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDocumentaryCollectionForm()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Documentary Collection Form");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestEFTRequest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "EFT Request");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestSummaryCommercialInvoice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Summary Commercial Invoice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportAuthority()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Authority");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportLetter()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Letter");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportPackingList()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Packing List");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestPreAlert()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pre-Alert");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDelayAlert()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delay Alert");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForMissingDocuments()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Missing Documents");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShippersLetterOfInstruction()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shippers Letter Of Instruction");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestWorkSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Work Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestInvoiceBatchReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice Batch Report");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipperDepartureNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipper Departure Notice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTimeSlotRequest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Time Slot");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Service");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAuthorisationForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Authorization for Service");
			RunDocument();
		}

		#region Implementation

		protected ZQuery fFilterForMenuItem;

		#endregion
	}
}
