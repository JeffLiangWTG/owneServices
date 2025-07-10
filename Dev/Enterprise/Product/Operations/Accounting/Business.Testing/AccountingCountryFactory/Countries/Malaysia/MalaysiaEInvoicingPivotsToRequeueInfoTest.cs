using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Testing;

public class MalaysiaEInvoicingPivotsToRequeueInfoTest : TestCaseWithFactory
{
	public void TestGetPivotsToRequeueFilter()
	{
		TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

		var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
		var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
		invoice2.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
		var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
		var invoice4 = Factory.NewWithValidTestData<ARInvoice>();
		Factory.Save();

		var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: Constants.EInvoicingPivotState.BatchedWithError);
		var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: Constants.EInvoicingPivotState.Failed);
		var pivot3 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice3, status: Constants.EInvoicingPivotState.Failed);
		var pivot4 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice4, status: Constants.EInvoicingPivotState.Succeed);
		Factory.Save();

		var eInvoicingTransactionPivots = new List<AccEInvoicingTransactionPivot>() { pivot1, pivot2, pivot3, pivot4 };
		var testObject = GetInstance();
		var filterCondition = testObject.GetPivotsToRequeueFilter(Factory);
		var pivotsToRequeue = eInvoicingTransactionPivots.Where(filterCondition);

		var expectedPKs = new[] { pivot1.PK, pivot3.PK };
		AssertEquals(2, pivotsToRequeue.Count());
		Assert(pivotsToRequeue.All(x => expectedPKs.Contains(x.PK)));
	}

	public void TestGetPivotsToRequeueMessage()
	{
		AssertEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:
- 'FAL' - Fail and E-Reporting Government # is blank, or
- 'BER' - Batched with errors.", GetInstance().GetPivotsToRequeueMessage());
	}

	TestObjectCreator testObjectCreator;
	TestObjectCreator TestObjectCreator
	{
		get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
	}

	IEInvoicingPivotsToRequeueFilterProvider GetInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Malaysia) as IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>).Get();
}
