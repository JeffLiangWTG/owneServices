using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(BillCollection))]
	sealed class BillCollectionTest : Customs.Business.Testing.BaseHouseBillCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return declaration.Bills;
		}

		public void TestDefaultBill()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var bills = declaration.Bills;
			AssertNull("Default Bill", bills.DefaultBill);
			var bill1 = bills.AddNew();
			AssertEquals("Default Bill", bill1.PK, bills.DefaultBill.PK);
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			var bill2 = bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertNull("Default Bill", bills.DefaultBill);
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("Default Bill", bill1.PK, bills.DefaultBill.PK);
			var bill3 = bills.AddNew();
			AssertNull("Default Bill", bills.DefaultBill);
		}
	}
}
