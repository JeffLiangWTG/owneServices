using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(DepartureCusTransportMeansFilteredCollection))]
	public class DepartureCusTransportMeansFilteredCollectionTest : SubsetBusinessObjectCollectionTestCase<DepartureCusTransportMeansFilteredCollection, DepartureCusTransportMeans>
	{
		public void TestFilterData()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			bill.DepartureTransportInfos.AddNew();
			var departureTransportInfos = bill.DepartureTransportInfos;
			var transportDepartureAdditionalWagonNumbers = bill.TransportDepartureAdditionalWagonNumbers;
			CombineAssertions(() =>
			{
				AssertEquals("DepartureTransportInfos initial count", 1, departureTransportInfos.Count);
				AssertEquals("No Additional wagons", 0, transportDepartureAdditionalWagonNumbers.Count);

				bill.TransportDepartureAdditionalWagonNumbers.AddNew();
				AssertEquals("Additional wagon is in parent collection", 2, departureTransportInfos.Count);
				AssertEquals("1 Additional wagon", 1, transportDepartureAdditionalWagonNumbers.Count);
			});
		}

		public void TestAllowNew()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("AllowNew false as no DepartuteTransportInfos", false, bill.TransportDepartureAdditionalWagonNumbers.AllowNew);

				bill.DepartureTransportInfos.AddNew();

				AssertEquals("AllowNew true as DepartuteTransportInfos.Count >= 1", true, bill.TransportDepartureAdditionalWagonNumbers.AllowNew);
			});
		}

		public void TestGetEnumerator()
		{
			AssertNotNull(AdditionalWagons.GetEnumerator());
		}

		public void TestIBusinessObjectCollectionAddNew()
		{
			(AdditionalWagons as IBusinessObjectCollection<IAdditionalWagonProvider>).AddNew();
			AssertEquals(1, AdditionalWagons.Count);
		}

		protected override DepartureCusTransportMeansFilteredCollection GetCollectionToTest() => AdditionalWagons;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<DepartureCusTransportMeans>();

		DepartureCusTransportMeansFilteredCollection AdditionalWagons
		{
			get
			{
				if (additionalWagons == null)
				{
					var nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					var bill = nctsHeader.Bills.AddNew();
					additionalWagons = new DepartureCusTransportMeansFilteredCollection(bill);
				}
				return additionalWagons;
			}
		}
		DepartureCusTransportMeansFilteredCollection additionalWagons;
	}
}
