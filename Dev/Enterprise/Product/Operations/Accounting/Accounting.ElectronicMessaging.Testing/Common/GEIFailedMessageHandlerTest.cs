using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GEIFailedMessageHandlerTest : TestCaseWithFactory
	{
		public void TestUpdateFailedMessage()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_LinkUniqueID = batch.PK;
			message.EM_LinkTable = AccEInvoicingBatchSchema.Constants.TableName;
			message.EM_EI = interchange.PK;

			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_AIB = batch.PK;
			Factory.Save();

			var handler = new GEIFailedMessageHandler();
			handler.UpdateFailedMessage(interchange);

			var pivotReloaded = new BusinessObjectFactory().Load<AccEInvoicingTransactionPivot>(pivot.PK);
			AssertEquals(Core.Constants.EInvoicingPivotState.Failed, pivotReloaded.AIP_Status);
			AssertEquals("Unexpected error when sending the Interchange. Please check the Notes 'eHub Server Error' on this EDI Interchange for more details.", pivotReloaded.AIP_ErrorDescription);
		}
	}
}
