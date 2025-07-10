using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsContainerItemCollection))]
	sealed class NctsContainerItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			var container = header.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
			return new NctsContainerItemCollection(container);
		}
	}
}
