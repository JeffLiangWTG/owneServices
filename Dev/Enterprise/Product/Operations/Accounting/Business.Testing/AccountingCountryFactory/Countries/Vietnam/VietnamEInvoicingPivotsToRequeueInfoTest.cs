using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam.Testing
{
	class VietnamEInvoicingPivotsToRequeueInfoTest : TestCaseWithFactory
	{
		public void TestGetTransactionPivotsToRequeue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Discarded);
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: EInvoicingPivotState.Queued);
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice3, status: EInvoicingPivotState.Discarded);
				Factory.Save();

				var selectedTransactions = new List<TransactionHeader>() { invoice1, invoice2, invoice3 };
				var testObject = GetComplianceNumberResetStatusInstance();

				var transactionPivots = testObject.GetAdditionalTransactionPivotsToRequeue(Factory, selectedTransactions);

				AssertEquals(2, transactionPivots.Length);
			}
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		IEInvoicingRequeueProvider GetComplianceNumberResetStatusInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.VietNam) as IInstanceProvider<IEInvoicingRequeueProvider>).Get();
	}
}
