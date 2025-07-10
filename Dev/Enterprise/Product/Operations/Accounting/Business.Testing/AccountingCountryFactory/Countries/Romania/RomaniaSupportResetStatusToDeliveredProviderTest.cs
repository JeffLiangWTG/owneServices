using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class RomaniaSupportResetStatusToDeliveredProviderTest : TestCaseWithFactory
	{
		public void TestGetEligiblePivotToRequeue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Romania))
			{
				var pivot1 = CreatePivot(EInvoicingPivotState.Failed, "100001");
				var pivot2 = CreatePivot(EInvoicingPivotState.BatchedWithError, "100002");
				var pivot3 = CreatePivot(EInvoicingPivotState.Succeed, "100003");
				var pivot4 = CreatePivot(EInvoicingPivotState.Failed, ZString.Empty);

				var result = ProviderForTest.GetEligiblePivotToRequeue(new List<AccEInvoicingTransactionPivot> { pivot1, pivot2, pivot3, pivot4 });

				AssertContainsExactElementsInAnyOrder(new List<AccEInvoicingTransactionPivot> { pivot1, pivot2 }, result);
			}
		}

		AccEInvoicingTransactionPivot CreatePivot(string pivotState, string governmentAllocatedId)
		{
			var transactionHeader = Factory.NewWithValidTestData<APInvoice>();

			var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(transactionHeader);
			authRecord.AHF_Number = governmentAllocatedId;

			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_Status = pivotState;
			pivot.AIP_ParentID = transactionHeader.PK;

			return pivot;
		}

		ISupportResetStatusToDelivered ProviderForTest => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Romania) as IInstanceProvider<ISupportResetStatusToDelivered>).Get();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
