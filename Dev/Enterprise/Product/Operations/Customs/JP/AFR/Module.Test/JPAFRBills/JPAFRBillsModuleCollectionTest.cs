using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRBillsModuleCollection))]
	class JPAFRBillsModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JPAFRBillsModuleCollection(Factory);
		}

		#endregion

		public void TestGetJPAFRBills()
		{
			var testHeader1 = Factory.NewWithValidTestData<JPAFRHeader>();
			var testHeader2 = Factory.NewWithValidTestData<JPAFRHeader>();
			testHeader1.JPH_IsShippingLineEntry = false;
			testHeader2.JPH_IsShippingLineEntry = true;
			var testBill11 = testHeader1.Bills.AddNew();
			var testBill21 = testHeader2.Bills.AddNew();
			var testBill22 = testHeader2.Bills.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				var testCollection = GetCollectionToTest();
				testCollection.Load();
				AssertEquals(3, testCollection.Count);
				Assert(testCollection.Any(header => header.PK == testBill11.PK));
				Assert(testCollection.Any(header => header.PK == testBill21.PK));
				Assert(testCollection.Any(header => header.PK == testBill22.PK));
			});
		}
	}
}
