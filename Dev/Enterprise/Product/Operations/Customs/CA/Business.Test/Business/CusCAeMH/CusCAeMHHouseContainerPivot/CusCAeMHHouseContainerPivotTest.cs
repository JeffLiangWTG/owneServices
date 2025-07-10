using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHHouseContainerPivot))]
	sealed class CusCAeMHHouseContainerPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBPA_Calc_ContainerNumber()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var container = master.Containers.AddNew();
			container.BQ_ContainerNumber = "12345";
			var house = master.HouseBills.AddNew();
			var pivot = house.Pivots.AddNew();
			pivot.BPA_BQ_Container = container.PK;
			AssertEquals("12345", pivot.BPA_Calc_ContainerNumber);
		}

		public void TestHouseBill()
		{
			var house = Factory.New<CusCAeMHMaster>().HouseBills.AddNew();
			var pivot = house.Pivots.AddNew();
			AssertEquals(house, pivot.HouseBill);
		}

		public void TestIHouseBillContainerMembers()
		{
			CusCAeMHHouseContainerPivot pivot = (CusCAeMHHouseContainerPivot)GetNewBusinessObject();
			var container = Factory.New<CusCAeMHContainer>();
			pivot.BPA_BQ_Container = container.PK;
			container.BQ_ContainerNumber = "CNUM";
			container.BQ_Seal1 = "SEAL1";
			container.BQ_Seal2 = "SEAL2";

			AssertEquals("CNUM", ((IHouseBillContainer)pivot).ContainerNumber);
			AssertEquals(2, ((IHouseBillContainer)pivot).Seals.Count());
			AssertEquals("SEAL1", ((IHouseBillContainer)pivot).Seals.First());
			AssertEquals("SEAL2", ((IHouseBillContainer)pivot).Seals.Last());
		}

		protected override BusinessObject GetNewBusinessObject()
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
