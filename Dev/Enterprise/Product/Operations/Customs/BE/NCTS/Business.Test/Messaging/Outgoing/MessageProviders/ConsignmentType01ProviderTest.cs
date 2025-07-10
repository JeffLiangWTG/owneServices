using System;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(ConsignmentType01Provider))]
	sealed class ConsignmentType01ProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentType01Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentType01Provider(null));
		}

		public void TestIncidents()
		{
			var incident = header.EnRouteIncidents.AddNew();
			incident.IncidentContainers.AddNew();
			AssertNotNull(Provider.Incidents);
		}

		public void TestLocationOfGoods()
		{
			AssertNotNull(Provider.LocationOfGoods);
		}

		protected override ConsignmentType01Provider GetProvider() => provider;
		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			provider = new ConsignmentType01Provider(header);
		}

		NctsHeader header;
		ConsignmentType01Provider provider;
	}
}
