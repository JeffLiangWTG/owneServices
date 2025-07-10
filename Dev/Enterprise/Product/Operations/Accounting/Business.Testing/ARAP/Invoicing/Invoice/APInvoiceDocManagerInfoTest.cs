using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceDocManagerInfo))]
	public class APInvoiceDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestRelatedObjectsForAPInvoiceWhenBaseHasDraftPK()
		{
			var draftInvoiceHeader = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.DraftInvoiceHeaderPK = draftInvoiceHeader.PK;

			Factory.Save();

			AssertEquals(1, apInvoice.DocManagerInfo.RelatedObjects.Length);
			AssertEquals(typeof(AccDraftInvoiceHeader), apInvoice.DocManagerInfo.RelatedObjects[0].GetType());
		}

		public void TestRelatedObjectsForAPInvoiceWhenDraftIsPosted()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var draftInvoiceHeader = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoiceHeader.AIH_AH_PostedTransactionHeader = apInvoice.PK;

			Factory.Save();

			AssertEquals(1, apInvoice.DocManagerInfo.RelatedObjects.Length);
			AssertEquals(typeof(AccDraftInvoiceHeader), apInvoice.DocManagerInfo.RelatedObjects[0].GetType());
		}

		public void TestRelatedObjectsForAPCreditNoteWhenBaseHasDraftPK()
		{
			var draftInvoiceHeader = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			creditNote.DraftInvoiceHeaderPK = draftInvoiceHeader.PK;

			Factory.Save();

			AssertEquals(1, creditNote.DocManagerInfo.RelatedObjects.Length);
			AssertEquals(typeof(AccDraftInvoiceHeader), creditNote.DocManagerInfo.RelatedObjects[0].GetType());
		}

		public void TestRelatedObjectsForCreditNoteWhenDraftIsPosted()
		{
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();
			var draftInvoiceHeader = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoiceHeader.AIH_AH_PostedTransactionHeader = creditNote.PK;

			Factory.Save();

			AssertEquals(1, creditNote.DocManagerInfo.RelatedObjects.Length);
			AssertEquals(typeof(AccDraftInvoiceHeader), creditNote.DocManagerInfo.RelatedObjects[0].GetType());
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New(typeof(APInvoice));
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var draftInvoiceHeader = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.DraftInvoiceHeaderPK = draftInvoiceHeader.PK;

			return apInvoice;
		}
	}
}
