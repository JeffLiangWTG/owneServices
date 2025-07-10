using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Client.JAS.Business.Invoicing.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Invoicing
{
	[TestedType(typeof(JASARCreditNote))]
	class JASARCreditNoteTest : ARCreditNoteTest
	{
		public void TestTypeDecidingTheCorrectType()
		{
			Factory.Save();
			BusinessObject newCreditNote = new BusinessObjectFactory().Load(typeof(InvoicingBase), CreditNote.PK);
			AssertEquals("If this fails, check the base class. It has to be subclassed from AR invoice", typeof(JASARCreditNote), newCreditNote.GetType());
		}

		public void TestOnSaving()
		{
			JASARCreditNoteForTest creditNote = Factory.NewWithValidTestData<JASARCreditNoteForTest>();
			Assert("Pre-condition", !creditNote.Helper.OnSavingCalled);
			Factory.Save();
			Assert("Should be called", creditNote.Helper.OnSavingCalled);
		}

		public void TestOnSaved()
		{
			JASARCreditNoteForTest creditNote = Factory.New<JASARCreditNoteForTest>();
			creditNote.OnSavedHander += CreditNote_OnSavedHander;
			Assert("Pre-condition", !OnSavedHandlerCalled);
			AssertNull("Pre-condition", creditNote.Helper.LastSaveSucceeded);
			creditNote.OnSaved(true);
			Assert("base.OnSaved() should be called", OnSavedHandlerCalled);
			Assert("Should be passed in", creditNote.Helper.LastSaveSucceeded.Value);
		}

		public void TestBaseFinancialMessageExportHelper()
		{
			JASARCreditNoteForTest creditNote = Factory.New<JASARCreditNoteForTest>();
			FinancialMessageExportHelper exportHelper = creditNote.BaseGetNewFinancialMessageExportHelper();
			AssertEquals(typeof(FinancialMessageExportHelper), exportHelper.GetType());
			AssertEquals(creditNote, exportHelper.Invoice);
		}

		public void TestNoteTypes()
		{
			AssertCollectionContains(JASPredefinedNoteTypes.Instance.JXCExportLog, CreditNote.NoteTypes);
		}

		public void TestCreditNoteOrInvoice()
		{
			AssertEquals(TransactionTypes.CreditNote, ((IJASInvoicingBase)CreditNote).CreditNoteOrInvoice);
		}

		public void TestInvoicingBase()
		{
			AssertEquals(CreditNote, ((IJASInvoicingBase)CreditNote).InvoicingBase);
		}

		[TestedType(typeof(JASARCreditNote))]
		public class JASARCreditNoteMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<JASARCreditNote>();
			}
		}

		#region Implementation
		void CreditNote_OnSavedHander(bool saveSuccessful)
		{
			OnSavedHandlerCalled = true;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JASARCreditNote>();
		}

		bool OnSavedHandlerCalled;
		#region class JASARCreditNoteForTest
		class JASARCreditNoteForTest : JASARCreditNote
		{
			public JASARCreditNoteForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override FinancialMessageExportHelper GetNewFinancialMessageExportHelper()
			{
				return Helper;
			}

			public FinancialMessageExportHelper BaseGetNewFinancialMessageExportHelper()
			{
				return base.GetNewFinancialMessageExportHelper();
			}

			public FinancialMessageExportHelperForTest Helper
			{
				get
				{
					if (fHelper == null)
					{
						fHelper = new FinancialMessageExportHelperForTest(this);
					}

					return fHelper;
				}
			}

			FinancialMessageExportHelperForTest fHelper;
		}
		#endregion
		#endregion
	}
}
