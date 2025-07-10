using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Utility.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting.PaymentChequeNumberAllocatorBase;

namespace Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting.Testing
{
	[TestedType(typeof(PaymentChequeNumberAllocatorBase))]
	public class PaymentChequeNumberAllocatorBaseTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetFactoriesWithAllocationCodeToBeCalledOnSaving_Core()
		{
			var newFactory = new BusinessObjectFactory();
			var newPayment = newFactory.NewWithValidTestData<APPayment>();
			var allocator = new PaymentChequeNumberAllocatorBase(newPayment, newPayment.Factory);
			var factories = allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(newFactory);
			AssertEquals("Factories array should contain 1 elements", 1, factories.Length);
			AssertEquals("First element is main Factory", newFactory, factories[0]);
		}

		public void TestCanContinueWithAllocation()
		{
			var testAutoPrintChequeBook = TestHelper.GetAutoPrintChequeBook(1, 1, 3);
			var newPayment = TestHelper.GetAutoAllocateAPPayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			TestAllocator = new DummyPaymentChequeNumberAllocatorBase(newPayment, Factory);
			Assert("Allocation should be enabled by default", ((IChequeNumberAutoAllocation)newPayment).IsAutoAllocationEnabled);
			Assert("Allocation should be enabled by default", TestAllocator.CanContinueWithAllocation_ForTestOnly);
			Assert("Allocation should not be performed on payment yet", !((IChequeNumberAutoAllocation)newPayment).IsAllocationPerformed);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Allocation should be performed", 1, TestAllocator.AllocationCalled_Counter);
			Assert("Allocation should be performed on payment", ((IChequeNumberAutoAllocation)newPayment).IsAllocationPerformed);
			Assert("Allocation should be disabled now", !TestAllocator.CanContinueWithAllocation_ForTestOnly);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Another Factory.Save call won't start another auto allocation", 1, TestAllocator.AllocationCalled_Counter);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PaymentChequeNumberAllocatorBase(null, Factory);
		}

		TransactionAllocateTestHelper TestHelper
		{
			get
			{
				if (fTestHelper == null)
				{
					fTestHelper = new TransactionAllocateTestHelper(Factory);
				}
				return fTestHelper;
			}
		}
		TransactionAllocateTestHelper fTestHelper;

		DummyPaymentChequeNumberAllocatorBase TestAllocator;
	}
}
