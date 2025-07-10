using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHHouseContainerPivotCollectionForContainer))]
	sealed class CusCAeMHHouseContainerPivotCollectionForContainerTest : ActiveBusinessObjectCollectionTestCase<CusCAeMHHouseContainerPivotCollectionForContainer>
	{
		protected override CusCAeMHHouseContainerPivotCollectionForContainer GetCollectionToTest()
		{
			return Container.Pivots;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Container.Pivots.AddNew();
		}

		CusCAeMHContainer Container
		{
			get { return fContainer ?? (fContainer = Factory.New<CusCAeMHMaster>().Containers.AddNew()); }
		}
		CusCAeMHContainer fContainer;
	}
}
