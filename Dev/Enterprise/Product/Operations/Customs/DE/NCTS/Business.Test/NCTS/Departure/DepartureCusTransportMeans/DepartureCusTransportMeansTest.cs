using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DepartureCusTransportMeans))]
	sealed class DepartureCusTransportMeansTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return departureCusTransportMeans;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => departureCusTransportMeans;

		protected override BusinessObject GetNewBusinessObject() => departureCusTransportMeans;

		protected override void SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureCusTransportMeans = nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();
		}

		DepartureCusTransportMeans departureCusTransportMeans;
	}
}
