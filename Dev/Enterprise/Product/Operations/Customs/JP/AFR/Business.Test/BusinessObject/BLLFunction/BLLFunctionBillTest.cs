using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BLLFunctionBill))]
	class BLLFunctionBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRefJPAFRBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew("vic");
			var bllBill = new BLLFunctionBill(bill);
			AssertEquals(bill, bllBill.AFRBill);
		}

		public void TestJPM_BillOfLadingNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew("vic");
			var bllBill = new BLLFunctionBill(bill);
			AssertEquals("vic", bllBill.JPM_BillOfLadingNumber);
		}

		public void TestICodeDescription()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew("vic");
			var bllBill = new BLLFunctionBill(bill);
			AssertEquals("vic", ((ICodeDescription)bllBill).Code);
			AssertEquals("vic", ((ICodeDescription)bllBill).Description);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew("vic");
			return new BLLFunctionBill(bill);
		}
	}
}
