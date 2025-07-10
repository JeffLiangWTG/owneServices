using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainerTypes()
		{
			AssertType(typeof(RefContainerCollection), Container.Lookups.ContainerTypes);
		}

		CusCAeMHContainer Container
		{
			get { return fContainer ?? (fContainer = Factory.New<CusCAeMHMaster>().Containers.AddNew()); }
		}
		CusCAeMHContainer fContainer;
	}
}
