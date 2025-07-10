using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Client.JAS.Business.Invoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Invoicing
{
	[TestedType(typeof(JASARAdjustmentNote))]
	class JASARAdjustmentNoteTest : ARAdjustmentNoteTest
	{
		public void TestTypeDecidingTheCorrectType()
		{
			JASARAdjustmentNote adjustmentNote = Factory.NewWithValidTestData<JASARAdjustmentNote>();
			Factory.Save();
			BusinessObject newAdjNote = new BusinessObjectFactory().Load(typeof(InvoicingBase), adjustmentNote.PK);
			AssertEquals("If this fails, check the base class. It has to be subclassed from AR adjustment note", typeof(JASARAdjustmentNote), newAdjNote.GetType());
		}

		public void TestOnSaving()
		{
			JASARAdjustmentNoteForTest adjustmentNote = Factory.NewWithValidTestData<JASARAdjustmentNoteForTest>();
			Assert("Pre-condition", !adjustmentNote.Helper.OnSavingCalled);
			Factory.Save();
			Assert("Should be called", adjustmentNote.Helper.OnSavingCalled);
		}

		public void TestOnSaved()
		{
			JASARAdjustmentNoteForTest adjustmentNote = Factory.New<JASARAdjustmentNoteForTest>();
			adjustmentNote.OnSavedHander += new InvoicingBase.SaveEventHandler(AdjustmentNote_OnSavedHander);
			Assert("Pre-condition", !OnSavedHandlerCalled);
			AssertNull("Pre-condition", adjustmentNote.Helper.LastSaveSucceeded);
			adjustmentNote.OnSaved(true);
			Assert("base.OnSaved() should be called", OnSavedHandlerCalled);
			Assert("Should be passed in", adjustmentNote.Helper.LastSaveSucceeded.Value);
		}

		public void TestBaseFinancialMessageExportHelper()
		{
			JASARAdjustmentNoteForTest adjustmentNote = Factory.New<JASARAdjustmentNoteForTest>();
			FinancialMessageExportHelper exportHelper = adjustmentNote.BaseGetNewFinancialMessageExportHelper();
			AssertEquals(typeof(FinancialMessageExportHelper), exportHelper.GetType());
			AssertEquals(adjustmentNote, exportHelper.Invoice);
		}

		public void TestNoteTypes()
		{
			JASARAdjustmentNote adjustmentNote = Factory.New<JASARAdjustmentNote>();
			AssertCollectionContains(JASPredefinedNoteTypes.Instance.JXCExportLog, adjustmentNote.NoteTypes);
		}

		public void TestCreditNoteOrInvoice()
		{
			JASARAdjustmentNote adjustmentNote = Factory.New<JASARAdjustmentNote>();
			adjustmentNote.AH_OSTotal = -20m;
			AssertEquals(ZArchitecture.Core.TransactionTypes.CreditNote, ((IJASInvoicingBase)adjustmentNote).CreditNoteOrInvoice);
			adjustmentNote.AH_OSTotal = 20m;
			AssertEquals(ZArchitecture.Core.TransactionTypes.Invoice, ((IJASInvoicingBase)adjustmentNote).CreditNoteOrInvoice);
		}

		public void TestInvoicingBase()
		{
			JASARAdjustmentNote adjustmentNote = Factory.New<JASARAdjustmentNote>();
			AssertEquals(adjustmentNote, ((IJASInvoicingBase)adjustmentNote).InvoicingBase);
		}

		[TestedType(typeof(JASARAdjustmentNote))]
		public class JASARAdjustmentNoteMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<JASARAdjustmentNote>();
			}
		}

		#region Implementation
		void AdjustmentNote_OnSavedHander(bool saveSuccessful)
		{
			OnSavedHandlerCalled = true;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JASARAdjustmentNote>();
		}

		protected override Type TypeOfValidation
		{
			get
			{
				return typeof(AdjustmentNoteValidation);
			}
		}

		bool OnSavedHandlerCalled;
		#region class JASARAdjustmentNoteForTest
		class JASARAdjustmentNoteForTest : JASARAdjustmentNote
		{
			public JASARAdjustmentNoteForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
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
