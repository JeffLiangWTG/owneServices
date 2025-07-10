using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Client.UPE.Business.CommercialInvoice.Testing
{
	sealed class CommercialInvoiceDocManagerTest : TestCaseWithFactory
	{
		public void TestUPECusHawbCommercialInvoice()
		{
			var testHelper = new UPETestHelper();
			var factory = new DocumentFactoryProvider().GetFactory(Factory);

			var parent = factory.New<StorageMain>();
			parent.SM_ParentFK = testHelper.HouseBill.PK;

			Assert("No commercial invoice attached", !testHelper.HouseBill.HasCommercialInvoice);
			AssertNull("No commercial invoice imaged", testHelper.HouseBill.CommercialInvoiceImage);

			var document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;

			testHelper.HouseBill.DocManagerInfo.Documents.Add(document);
			testHelper.HouseBill.DocManagerInfo.Save();
			Assert("Commercial invoice attached", testHelper.HouseBill.HasCommercialInvoice);
			AssertNotNull("Should find the Commercial Invoice document from eDocs", testHelper.HouseBill.CommercialInvoiceImage);
		}

		public void TestJobDeclarationCommercialInvoiceImage()
		{
			var testHelper = new UPETestHelper();
			var factory = new DocumentFactoryProvider().GetFactory(Factory);

			var parent = factory.New<StorageMain>();
			parent.SM_ParentFK = testHelper.JobDeclaration.PK;

			Assert("No commercial invoice attached", !testHelper.JobDeclaration.HasCommercialInvoice);
			AssertNull("No commercial invoice imaged", testHelper.JobDeclaration.CommercialInvoiceImage);

			var document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;

			testHelper.JobDeclaration.DocManagerInfo.Documents.Add(document);
			testHelper.JobDeclaration.DocManagerInfo.Save();
			Assert("Commercial invoice attached", testHelper.JobDeclaration.HasCommercialInvoice);
			AssertNotNull("Should find the Commercial Invoice document from eDocs", testHelper.JobDeclaration.CommercialInvoiceImage);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
