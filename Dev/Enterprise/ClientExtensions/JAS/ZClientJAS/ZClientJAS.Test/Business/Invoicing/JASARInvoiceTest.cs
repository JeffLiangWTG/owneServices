using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Invoicing.Testing
{
	[TestedType(typeof(JASARInvoice))]
	internal class JASARInvoiceTest : ARInvoiceTest
	{
		public void TestTypeDecidingTheCorrectType()
		{
			JASARInvoice invoice = Factory.NewWithValidTestData<JASARInvoice>();
			Factory.Save();
			BusinessObject newInvoice = new BusinessObjectFactory().Load(typeof(InvoicingBase), invoice.PK);
			AssertEquals("If this fails, check the base class. It has to be subclassed from AR invoice", typeof(JASARInvoice), newInvoice.GetType());
		}

		public void TestOnSaving()
		{
			JASARInvoiceForTest invoice = Factory.NewWithValidTestData<JASARInvoiceForTest>();
			Assert("Pre-condition", !invoice.Helper.OnSavingCalled);
			Factory.Save();
			Assert("Should be called", invoice.Helper.OnSavingCalled);
		}

		public void TestOnSaved()
		{
			JASARInvoiceForTest invoice = Factory.New<JASARInvoiceForTest>();
			invoice.OnSavedHander += new InvoicingBase.SaveEventHandler(Invoice_OnSavedHander);
			Assert("Pre-condition", !OnSavedHandlerCalled);
			AssertNull("Pre-condition", invoice.Helper.LastSaveSucceeded);
			invoice.OnSaved(true);
			Assert("base.OnSaved() should be called", OnSavedHandlerCalled);
			Assert("Should be passed in", invoice.Helper.LastSaveSucceeded.Value);
		}

		public void TestBaseFinancialMessageExportHelper()
		{
			JASARInvoiceForTest invoice = Factory.New<JASARInvoiceForTest>();
			FinancialMessageExportHelper exportHelper = invoice.BaseGetNewFinancialMessageExportHelper();
			AssertEquals(typeof(FinancialMessageExportHelper), exportHelper.GetType());
			AssertEquals(invoice, exportHelper.Invoice);
		}

		public void TestNoteTypes()
		{
			JASARInvoice invoice = Factory.New<JASARInvoice>();
			AssertCollectionContains(JASPredefinedNoteTypes.Instance.JXCExportLog, invoice.NoteTypes);
		}

		public void TestCreditNoteOrInvoice()
		{
			JASARInvoice invoice = Factory.New<JASARInvoice>();
			AssertEquals(ZArchitecture.Core.TransactionTypes.Invoice, ((IJASInvoicingBase)invoice).CreditNoteOrInvoice);
		}

		public void TestInvoicingBase()
		{
			JASARInvoice invoice = Factory.New<JASARInvoice>();
			AssertEquals(invoice, ((IJASInvoicingBase)invoice).InvoicingBase);
		}

		[TestedType(typeof(JASARInvoice))]
		public class JASARInvoiceMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<JASARInvoice>();
			}
		}

		#region Implementation
		void Invoice_OnSavedHander(bool saveSuccessful)
		{
			OnSavedHandlerCalled = true;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JASARInvoice>();
		}

		protected override Type TypeOfValidation
		{
			get
			{
				return typeof(InvoiceValidation);
			}
		}

		bool OnSavedHandlerCalled;
		#region class JASARInvoiceForTest
		class JASARInvoiceForTest : JASARInvoice
		{
			public JASARInvoiceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
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
