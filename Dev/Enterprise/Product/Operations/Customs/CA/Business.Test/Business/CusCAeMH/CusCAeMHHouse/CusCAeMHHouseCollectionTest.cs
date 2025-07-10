using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHHouseCollection))]
	sealed class CusCAeMHHouseCollectionTest : ActiveBusinessObjectCollectionTestCase<CusCAeMHHouseCollection>
	{
		public void TestSetDefaultsForNewElement()
		{
			var container = Master.Containers.AddNew();
			container.BQ_ContainerNumber = "APLU123456";
			var house = Master.HouseBills.AddNew();
			var pivot = house.Pivots[0];
			AssertEquals(container.PK, pivot.BPA_BQ_Container);
		}

		protected override CusCAeMHHouseCollection GetCollectionToTest()
		{
			return new CusCAeMHHouseCollection(Master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Master.HouseBills.AddNew();
		}

		CusCAeMHMaster Master
		{
			get { return fMaster ?? (fMaster = Factory.New<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster fMaster;
	}
}
