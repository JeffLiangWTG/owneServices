using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Testing;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Client.UPE.Business.DocWrappers.Testing
{
	sealed class DocCalloutTest : TestCaseWithFactory
	{
		public void TestDeclarationCommercialInvoiceImage()
		{
			var finance = Factory.New<Callout>();
			var doc = DocCallout.New(finance, Factory);
			AssertNull(doc.CommercialInvoiceImage);

			var jobDec = Factory.New<UPEJobDeclaration>();
			finance.CS_JE_CustomsFormalEntry = jobDec.PK;
			AssertNull(doc.CommercialInvoiceImage);

			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var parent = docFactory.New<StorageMain>();
			parent.SM_ParentFK = jobDec.PK;

			var document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;

			jobDec.DocManagerInfo.Documents.Add(document);
			jobDec.DocManagerInfo.Save();
			AssertNotNull(doc.CommercialInvoiceImage);
		}

		public void TestCusHawbCommercialInvoiceImage()
		{
			var testHelper = new UPETestHelper(Factory);
			testHelper.SetHouseBillTestData();
			testHelper.HouseBill.CS_HAWB = "1234";

			var callout = Factory.Load<Callout>(testHelper.HouseBill.PK);
			var doc = DocCalloutForTest.New(callout, Factory);
			AssertNull(doc.CommercialInvoiceImage);

			var docFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var parent = docFactory.New<StorageMain>();
			parent.SM_ParentFK = testHelper.HouseBill.PK;

			var document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;

			doc.CusHawb.DocManagerInfo.Documents.Add(document);
			doc.CusHawb.DocManagerInfo.Save();
			AssertNotNull(doc.CommercialInvoiceImage);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		sealed class DocCalloutForTest : DocCallout
		{
			public DocCalloutForTest(Callout callout, BusinessObjectFactory factoryToWrap)
			: base(callout, factoryToWrap)
			{
			}

			internal new static DocCalloutForTest New(Callout callout, BusinessObjectFactory factoryToWrap)
			{
				return (callout == null) ? null : new DocCalloutForTest(callout, factoryToWrap);
			}

			public UPECusHAWB CusHawb => Callout;
		}
	}
}
