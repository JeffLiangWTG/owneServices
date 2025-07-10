using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	public sealed class ResetEDocStatusServiceTest : TestCaseWithFactory
	{
		public void TestResetEDocStatus()
		{
			var invoicingBase = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			var smFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var eDocMain = smFactory.NewWithValidTestData<StorageMain>();
			eDocMain.SM_ParentFK = invoicingBase.PK;
			var item1 = CreateEDocItem(eDocMain, eDocMain.PK, isPublished: true, isSysGenerated: false, Core.Constants.RefDocTypes.Invoice, InvoiceFileNameProvider.GetInvoiceFileName(invoicingBase));
			var item2 = CreateEDocItem(eDocMain, eDocMain.PK, isPublished: true, isSysGenerated: true, Core.Constants.RefDocTypes.AgentsInstruction, InvoiceFileNameProvider.GetInvoiceFileName(invoicingBase));
			var item3 = CreateEDocItem(eDocMain, eDocMain.PK, isPublished: true, isSysGenerated: true, Core.Constants.RefDocTypes.Invoice, "DummyName");
			var item4 = CreateEDocItem(eDocMain, eDocMain.PK, isPublished: true, isSysGenerated: true, Core.Constants.RefDocTypes.Invoice, InvoiceFileNameProvider.GetInvoiceFileName(invoicingBase));
			var item5 = CreateEDocItem(eDocMain, eDocMain.PK, isPublished: true, isSysGenerated: true, Core.Constants.RefDocTypes.Invoice, $"{InvoiceFileNameProvider.GetInvoiceFileName(invoicingBase)}[1]");
			smFactory.Save();

			var newFactory = new BusinessObjectFactory();
			invoicingBase = newFactory.Load<ARInvoice>(invoicingBase.PK);

			var item6 = CreateEDocItem(invoicingBase.DocManagerInfo.StorageMain as StorageMain, eDocMain.PK, isPublished: true, isSysGenerated: true, Core.Constants.RefDocTypes.Invoice, $"{InvoiceFileNameProvider.GetInvoiceFileName(invoicingBase)}[1]");
			var resetEDocStatusService = new ResetEDocStatusService();
			resetEDocStatusService.ResetEDocStatus(invoicingBase);
			AssertEquals(true, item6.SC_IsPublished);
			AssertEquals(false, item6.IsInDatabase);

			(invoicingBase as IDocManagerSupport).DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
			newFactory.Save();

			var numberedFactory = new DocumentFactoryProvider().GetFactory(Factory).GetFactory(eDocMain.SM_DB);
			item1 = numberedFactory.Load<StorageDocs>(item1.PK);
			item2 = numberedFactory.Load<StorageDocs>(item2.PK);
			item3 = numberedFactory.Load<StorageDocs>(item3.PK);
			item4 = numberedFactory.Load<StorageDocs>(item4.PK);
			item5 = numberedFactory.Load<StorageDocs>(item5.PK);
			item6 = numberedFactory.Load<StorageDocs>(item6.PK);
			AssertEquals(true, item1.SC_IsPublished);
			AssertEquals(true, item2.SC_IsPublished);
			AssertEquals(true, item3.SC_IsPublished);
			AssertEquals(false, item4.SC_IsPublished);
			AssertEquals(false, item5.SC_IsPublished);
			AssertEquals(true, item6.SC_IsPublished);
		}

		StorageDocs CreateEDocItem(StorageMain eDocMain, ZGuid eDocMainPK, bool isPublished, bool isSysGenerated, string docType, string fileName)
		{
			var eDocItem = eDocMain.Documents.AddNew();
			eDocItem.SC_SM = eDocMainPK;
			eDocItem.SC_IsPublished = isPublished;
			eDocItem.SC_IsSystemGenerated = isSysGenerated;
			eDocItem.SC_DocType = docType;
			eDocItem.SC_FileName = fileName;
			return eDocItem;
		}
	}
}
