using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	public class ApInvoiceRequisitionFormTest : TestCaseWithFactory
	{
		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestClickOkButtonToShowErrorMessageForConcurrencyErrors()
		{
			// Arrange
			Factory.RefreshEnabled = false;
			new AccountingPeriodTestHelper().SetupPeriods();
			APTransactionHeaderCollectionHolder transactionHeaderCollectionHolder = new APTransactionHeaderCollectionHolder(Factory, new APTransactionHeaderCollection(Factory));
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_RequisitionDate = ZDateTime.UtcNow;
			APInvoiceLine invoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
			invoiceLine.AL_OSExTaxAmount = 100M;
			transactionHeaderCollectionHolder.Collection.Add(invoice);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			APInvoice invoiceFromNewFactory = newFactory.Load<APInvoice>(invoice.PK);
			invoiceFromNewFactory.AH_RequisitionDate = invoiceFromNewFactory.AH_RequisitionDate.AddDays(1);
			newFactory.Save();
			// Act
			using (APInvoiceRequisitionForm apInvoiceRequisitionForm = new APInvoiceRequisitionForm(transactionHeaderCollectionHolder))
			{
				using (ZButton mockOkButton = new ZButton())
				{
					invoice.AH_RequisitionDate = invoice.AH_RequisitionDate.AddDays(2);
					apInvoiceRequisitionForm.OKButton_Click(mockOkButton, EventArgs.Empty);
				}
			}
			// Assert
			AssertEquals(
				$"The invoice whose transaction number is {invoice.AH_TransactionNum} cannot be saved because of another user has updated it beforehand. Please try this again later.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
