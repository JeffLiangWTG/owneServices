using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	class TurkeyEInvoicingPivotActionTypeProviderTest : TestCaseWithFactory
	{
		const string CountryCode = CountryCodes.Turkey;

		IEInvoicingPivotActionTypeProvider GetEInvoicingPivotActionTypeProvider() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode) as IInstanceProvider<IEInvoicingPivotActionTypeProvider>).Get();

		public void TestIEInvoicingPivotActionTypeProvider()
		{
			var pivotActionTypeProvider = GetEInvoicingPivotActionTypeProvider();

			var tpaTransaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			AssertEquals(EInvoicingPivotActionType.ConfirmTransactionReceived, pivotActionTypeProvider.GetPivotActionType(tpaTransaction));

			((INeedRow)tpaTransaction).Row.AcceptChanges();
			AssertEquals("The test case that should never come true", EInvoicingPivotActionType.Submit, pivotActionTypeProvider.GetPivotActionType(tpaTransaction));

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			AssertEquals(EInvoicingPivotActionType.Submit, pivotActionTypeProvider.GetPivotActionType(arInvoice));

			arInvoice.GenerateReverseTransaction(true);
			AssertEquals(EInvoicingPivotActionType.Cancel, pivotActionTypeProvider.GetPivotActionType(arInvoice.ReverseTransaction as TransactionHeader));

			((INeedRow)arInvoice).Row.AcceptChanges();
			AssertEquals(EInvoicingPivotActionType.Cancel, pivotActionTypeProvider.GetPivotActionType(arInvoice.ReverseTransaction as TransactionHeader));

			var apCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			AssertEquals(EInvoicingPivotActionType.Submit, pivotActionTypeProvider.GetPivotActionType(apCreditNote));

			apCreditNote.GenerateReverseTransaction(true);
			AssertEquals(EInvoicingPivotActionType.Cancel, pivotActionTypeProvider.GetPivotActionType(apCreditNote.ReverseTransaction as TransactionHeader));

			((INeedRow)apCreditNote).Row.AcceptChanges();
			AssertEquals(EInvoicingPivotActionType.Cancel, pivotActionTypeProvider.GetPivotActionType(apCreditNote.ReverseTransaction as TransactionHeader));

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals("The test case that should never come true", EInvoicingPivotActionType.Submit, pivotActionTypeProvider.GetPivotActionType(apInvoice));

			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			AssertEquals("The test case that should never come true", EInvoicingPivotActionType.Submit, pivotActionTypeProvider.GetPivotActionType(arCreditNote));
		}
	}
}
