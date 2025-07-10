namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.MasterFiles.Business;

	public abstract class BaseAPInvoiceCollectionForDocScanningTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_GC = GlbCompany.CurrentCompany.PK;
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_GC = GlbCompany.CurrentCompany.PK;
			APCreditNote apCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			apCreditNote.AH_GC = GlbCompany.CurrentCompany.PK;
			APAdjustmentNote apAdjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
			apAdjustmentNote.AH_GC = GlbCompany.CurrentCompany.PK;
			BaseAPInvoiceCollectionForDocScanning collection = GetNewCollection();
			collection.Load();
			AssertEquals("Should only have one transaction in the collection", 1, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GetNewCollection();
		}

		protected abstract BaseAPInvoiceCollectionForDocScanning GetNewCollection();
	}
}