using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Moq;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders.Test
{
	public class TransactionReasonFormPresentationProviderTest : TestCaseWithFactory
	{
		public void TestIsAmendInFullVisible()
		{
			var mockITransactionReasonFormProvider = new Mock<ITransactionReasonFormProvider>();
			mockITransactionReasonFormProvider.Setup(o => o.ShouldShowAmendInFull()).Returns(true);

			var mockIGlobalAccountingCountryFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			mockIGlobalAccountingCountryFactory.Setup(o => o.GetFeatureInterface<ITransactionReasonFormProvider>(It.IsAny<ZString>())).Returns(mockITransactionReasonFormProvider.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				var apCreditNote = Factory.NewWithValidTestData<APCreditNote>();
				var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				var apInvoice = Factory.NewWithValidTestData<APInvoice>();

				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(arCreditNote));
				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(apCreditNote));
				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(arInvoice));
				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(apInvoice));

				var amendingTransaction = arInvoice.GenerateAmendingTransaction(typeof(ARCreditNote));
				Assert(TransactionReasonFormPresentationProvider.IsAmendInFullVisible(amendingTransaction));
			}

			mockITransactionReasonFormProvider.Setup(o => o.ShouldShowAmendInFull()).Returns(false);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				var apCreditNote = Factory.NewWithValidTestData<APCreditNote>();
				var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				var apInvoice = Factory.NewWithValidTestData<APInvoice>();

				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(arCreditNote));
				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(apCreditNote));
				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(arInvoice));
				Assert(!TransactionReasonFormPresentationProvider.IsAmendInFullVisible(apInvoice));
			}
		}

		ITransactionReasonFormPresentationProvider TransactionReasonFormPresentationProvider => transactionReasonFormPresentationProvider ?? (transactionReasonFormPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetTransactionReasonFormPresentationProvider());
		ITransactionReasonFormPresentationProvider transactionReasonFormPresentationProvider;
	}
}
