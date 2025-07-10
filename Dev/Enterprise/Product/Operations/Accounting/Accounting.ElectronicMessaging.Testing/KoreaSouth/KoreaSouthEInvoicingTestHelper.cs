using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class KoreaSouthEInvoicingTestHelper
	{
		public KoreaSouthEInvoicingTestHelper(TestObjectCreator testObjectCreator)
		{
			TestObjectCreator = testObjectCreator;
		}

		public (AccEInvoicingBatch batchSUB, AccEInvoicingBatch batchSTA) SetUpQueryBatch()
		{
			var provider = new KoreaSouthEInvoicingActionProvider();

			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			provider.OnEvaluateEligibilityAndQueue(arInvoice1);
			provider.OnEvaluateEligibilityAndQueue(arInvoice2);
			Factory.Save();

			var pivotSUB1 = arInvoice1.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			var pivotSUB2 = arInvoice2.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			var batchSUB = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			pivotSUB1.AIP_AIB = batchSUB.PK;
			pivotSUB2.AIP_AIB = batchSUB.PK;
			Factory.Save();

			var pivotSTA1 = arInvoice1.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Delivered);
			var batchSTA = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			pivotSTA1.AIP_AIB = batchSTA.PK;
			Factory.Save();

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_LinkUniqueID = batchSTA.PK;
			message.EM_LinkTable = AccEInvoicingBatchSchema.Constants.TableName;
			message.EM_EI = interchange.PK;
			Factory.Save();

			return (batchSUB, batchSTA);
		}

		public AccEInvoicingBatch SetUpSubmitBatch()
		{
			var provider = new KoreaSouthEInvoicingActionProvider();
			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			provider.OnEvaluateEligibilityAndQueue(arInvoice1);
			provider.OnEvaluateEligibilityAndQueue(arInvoice2);
			Factory.Save();

			var pivotSUB1 = arInvoice1.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			var pivotSUB2 = arInvoice2.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Delivered);
			var batchSUB = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			pivotSUB1.AIP_AIB = batchSUB.PK;
			pivotSUB2.AIP_AIB = batchSUB.PK;
			Factory.Save();

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_LinkUniqueID = batchSUB.PK;
			message.EM_LinkTable = AccEInvoicingBatchSchema.Constants.TableName;
			message.EM_EI = interchange.PK;
			Factory.Save();
			return batchSUB;
		}

		BusinessObjectFactory Factory => TestObjectCreator.Factory;

		readonly TestObjectCreator TestObjectCreator;
	}
}
