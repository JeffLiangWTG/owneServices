using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBCollection))]
	sealed class CusHAWBCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetHouseBillsWithPrealertHeld()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWBCollection collection = masterBill.ChildBills;
			AssertEquals("No house bill held", 0, collection.GetHouseBillsWithPrealertHeld().Length);

			CusHAWB hAWB = collection.AddNew();
			AssertEquals("No house bill held", 0, collection.GetHouseBillsWithPrealertHeld().Length);

			hAWB.CS_IsPrealertHeldByUser = true;
			AssertEquals("One house bill held", 1, collection.GetHouseBillsWithPrealertHeld().Length);
		}

		public void TestConstructor()
		{
			CusHAWBCollection collection = new CusHAWBCollection(Factory.New<CusMAWB>(), Factory);
			AssertNotNull(collection);
		}

		public void TestNewdAddNew()
		{
			CusHAWBCollection collection = new CusHAWBCollection(Factory.New<CusMAWB>(), Factory);
			AssertEquals(typeof(CusHAWB), collection.AddNew().GetType());
		}

		public void TestNewdAddNew_WithBizOType()
		{
			CusHAWBCollection collection = new CusHAWBCollection(Factory.New<CusMAWB>(), Factory);
			AssertEquals(typeof(CusHAWBForTest), collection.AddNew(typeof(CusHAWBForTest)).GetType());
		}

		public void TestDefaultFromCompanyCurrencyForNewChild()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertEquals("Default to Company Currency", Enterprise.Customs.AU.Declaration.Business.JobDeclaration.LocalCurrencyConstantCode, houseBill.CS_RX_NKGoodsCurrency);
		}

		public void TestHasChangesAfterGoodsCurrencyDefaulted()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertEquals("Default to Company Currency", Enterprise.Customs.AU.Declaration.Business.JobDeclaration.LocalCurrencyConstantCode, houseBill.CS_RX_NKGoodsCurrency);
			AssertEquals("HasChanges is false", false, houseBill.HasChanges);
		}

		public void TestOriginDestinationFromLoadAndDischarge()
		{
			var masterBill = Factory.New<CusMAWB>();
			masterBill.CM_RL_NKLoadPort = "NZAKL";
			masterBill.CM_RL_NKDischargePort = "AUSYD";

			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertEquals("Origin defaulted", masterBill.CM_RL_NKLoadPort, houseBill.CS_RL_NKOrigin);
			AssertEquals("Destination defaulted", masterBill.CM_RL_NKDischargePort, houseBill.CS_RL_NKDestination);
		}

		public void TestCanDelete()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB hAWB1 = masterBill.ChildBills.AddNew();
			CusHAWB hAWB2 = masterBill.ChildBills.AddNew();
			Assert("Collection can be deleted", masterBill.ChildBills.CanDelete);
		}

		public void TestDefaultValues()
		{
			var masterBill = Factory.New<CusMAWB>();
			CusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertEquals("HouseBill weight uq defaulted", "KG", houseBill.CS_WeightUQ);
		}

		public void TestNumberOfPackages()
		{
			var masterBill = Factory.New<CusMAWB>();
			AssertEquals("Number of packages", 0, masterBill.ChildBills.NumberOfPackages);

			CusHAWB hAWB = masterBill.ChildBills.AddNew();
			hAWB.CS_PiecesManifested = 100;
			AssertEquals("Number of packages", 100, masterBill.ChildBills.NumberOfPackages);

			CusHAWB hAWB2 = masterBill.ChildBills.AddNew();
			hAWB2.CS_PiecesManifested = 100;
			AssertEquals("Number of packages", 200, masterBill.ChildBills.NumberOfPackages);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusHAWBCollection(Factory.New<CusMAWB>(), Factory);

		sealed class CusHAWBForTest : CusHAWB
		{
			public CusHAWBForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
