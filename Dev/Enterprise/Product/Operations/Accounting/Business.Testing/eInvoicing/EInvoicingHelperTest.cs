using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.eInvoicing.Testing
{
	public class EInvoicingHelperTest : TestCaseWithFactory
	{
		public void TestEInvoicingHelper_HasActiveEInvoicingTransactionPivot()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var eInvoicingHelper = ObjectFactory.Get<IEInvoicingHelper>();
			AssertNotNull(eInvoicingHelper);

			var transaction = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();
			Assert("Transaction does not have pivot", !eInvoicingHelper.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived));

			var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived);
			Factory.Save();
			Assert("Transaction has pivot. Pivot status is Queued", eInvoicingHelper.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived));

			pivot.AIP_Status = EInvoicingPivotState.Succeed;
			Factory.Save();
			Assert("Transaction has pivot. Pivot status is Succeed", eInvoicingHelper.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived));

			pivot.AIP_Status = EInvoicingPivotState.Discarded;
			Factory.Save();
			Assert("Transaction has pivot. Pivot status is Discarded", !eInvoicingHelper.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived));

			pivot = testObjectCreator.CreateEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Sent);
			Factory.Save();
			Assert("Helper does not check pivots with not provided action type", !eInvoicingHelper.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived));

			pivot = testObjectCreator.CreateEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived, EInvoicingPivotState.Sent);
			Factory.Save();
			Assert("Helper checks pivots only for provided action type", eInvoicingHelper.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived));
		}
	}
}
