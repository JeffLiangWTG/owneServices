using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(EnRouteIncidentNctsContainerCollection))]
	sealed class EnRouteIncidentNctsContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<EnRouteIncidentNctsContainerCollection>
	{
		public void TestMaster()
		{
			var collection = GetCollectionToTest();
			AssertType<EnRouteIncident>(collection.Master);
		}

		public void TestDefaultParentTableCode()
		{
			var collection = GetCollectionToTest();
			var container = collection.AddNew();
			AssertEquals(CusInBondEventSchema.Constants.Prefix, container.BC_ParentTableCode);
		}

		protected override EnRouteIncidentNctsContainerCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incident = header.EnRouteIncidents.AddNew();
			return incident.IncidentContainers;
		}
	}
}
