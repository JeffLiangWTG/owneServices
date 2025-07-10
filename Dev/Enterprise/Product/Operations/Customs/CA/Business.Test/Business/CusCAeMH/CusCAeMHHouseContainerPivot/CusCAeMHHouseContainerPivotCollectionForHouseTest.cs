using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHHouseContainerPivotCollectionForHouse))]
	sealed class CusCAeMHHouseContainerPivotCollectionForHouseTest : ActiveBusinessObjectCollectionTestCase<CusCAeMHHouseContainerPivotCollectionForHouse>
	{
		protected override CusCAeMHHouseContainerPivotCollectionForHouse GetCollectionToTest()
		{
			((IBusinessObjectCollection)House.Pivots).ClearHasChangesIncludingChildren();
			return House.Pivots;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return House.Pivots.AddNew();
		}

		CusCAeMHHouse House
		{
			get { return fHouse ?? (fHouse = Factory.New<CusCAeMHMaster>().HouseBills.AddNew()); }
		}
		CusCAeMHHouse fHouse;
	}
}
