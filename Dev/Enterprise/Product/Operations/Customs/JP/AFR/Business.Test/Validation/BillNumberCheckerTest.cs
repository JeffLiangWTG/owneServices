using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class BillNumberCheckerTest : TestCaseWithFactory
	{
		public void TestIsMBOLNumDuplicate()
		{
			var header1 = Factory.NewWithValidTestData<JPAFRHeader>();
			header1.JPH_MasterBillNumber = "SPQAM001";
			Assert(!BillNumberChecker.IsMBOLNumDuplicate(header1));

			Factory.Save();
			var header2 = Factory.NewWithValidTestData<JPAFRHeader>();
			header2.JPH_MasterBillNumber = "SPQAM001";

			Assert(BillNumberChecker.IsMBOLNumDuplicate(header2));
		}

		public void TestIsHBOLNumDuplicateWithinThisHeader()
		{
			var header1 = Factory.NewWithValidTestData<JPAFRHeader>();
			header1.JPH_MasterBillNumber = "SPQAM001";
			var bill1 = Factory.NewWithValidTestData<JPAFRBills>();
			bill1.JPB_BillNumber = "J07JH001";
			header1.Bills.Add(bill1);

			var bill2 = Factory.NewWithValidTestData<JPAFRBills>();
			CombineAssertions(() =>
			{
				bill2.JPB_BillNumber = "J07JH001";
				Assert(!BillNumberChecker.IsHBOLNumDuplicateWithinThisHeader(bill2));
				header1.Bills.Add(bill2);
				Assert(BillNumberChecker.IsHBOLNumDuplicateWithinThisHeader(bill2));
			});
		}

		public void TestHasHBOLReachedMaxAllowedWithinThisHeader_ForNVOCC()
		{
			var header1 = Factory.NewWithValidTestData<JPAFRHeader>();
			header1.JPH_MasterBillNumber = "SPQAM001";
			for (int i = 0; i < 99; i++)
			{
				var bill = Factory.NewWithValidTestData<JPAFRBills>();
				bill.JPB_BillNumber = "J07JH" + i.ToString("000");
				header1.Bills.Add(bill);
			}
			Assert(!BillNumberChecker.HasHBOLReachedMaxAllowedWithinThisHeader(header1.Bills.Last()));

			var newbill = Factory.NewWithValidTestData<JPAFRBills>();
			newbill.JPB_BillNumber = "BrandNewBill";
			header1.Bills.Add(newbill);
			Assert(BillNumberChecker.HasHBOLReachedMaxAllowedWithinThisHeader(newbill));
		}

		public void TestHasHBOLReachedMaxAllowedWithinThisHeader_ForVOCC()
		{
			var header1 = Factory.NewWithValidTestData<JPAFRHeader>();
			header1.JPH_IsShippingLineEntry = true;

			for (int i = 0; i < 9999; i++)
			{
				header1.Bills.AddNew();
			}
			Assert("Not Yet Exceeding Limit", !BillNumberChecker.HasHBOLReachedMaxAllowedWithinThisHeader(header1.Bills.Last()));

			var newbill = Factory.NewWithValidTestData<JPAFRBills>();
			newbill.JPB_BillNumber = "BrandNewBill";
			header1.Bills.Add(newbill);
			Assert("Now Exceeding Limit", BillNumberChecker.HasHBOLReachedMaxAllowedWithinThisHeader(newbill));
		}
	}
}
