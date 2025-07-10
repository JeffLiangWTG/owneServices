using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSTranshipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSTranshipmentProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", NCTSTranshipmentProvider.NewOrNull(null));
				AssertNotNull("Not NULL", dataProvider);
			});
		}

		public void TestTransportMeansIdentity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", null, dataProvider.TransportMeansIdentity);
				transshipment.BN_TransportID = "Transport ID";
				AssertEquals("Entered", "Transport ID", dataProvider.TransportMeansIdentity);
			});
		}

		public void TestTransportMeansNationality()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", null, dataProvider.TransportMeansNationality);
				transshipment.BN_TransportCountryCode = "DE";
				AssertEquals("Entered", "DE", dataProvider.TransportMeansNationality);
			});
		}

		public void TestEndorsement()
		{
			CombineAssertions(() =>
			{
				transshipment.BN_EndorsementDate = DateTime.Now;
				transshipment.BN_EndorsementPlace = "PLACE";
				transshipment.BN_EndorsementAuthority = "ABC";
				transshipment.BN_EndorsementCountryCode = "DE";
				var endorsement = dataProvider.Endorsement;
				AssertNotNull("Endorsement", endorsement);
				AssertSame("Cached", endorsement, dataProvider.Endorsement);
			});
		}

		public void TestContainerIdentificationNumbers()
		{
			CombineAssertions(() =>
			{
				var containerIdentificationNumbers = dataProvider.ContainerIdentificationNumbers;
				AssertEquals("Count", 0, containerIdentificationNumbers.Count);
				AssertSame("Cached", containerIdentificationNumbers, dataProvider.ContainerIdentificationNumbers);
			});
		}

		public void TestContainerIdentificationNumbers_Entered()
		{
			transshipment.Containers.AddNew().BC_ContainerNum = "CNT 1";
			CombineAssertions(() =>
			{
				var containerIdentificationNumbers = dataProvider.ContainerIdentificationNumbers;
				AssertEquals("Count", 1, containerIdentificationNumbers.Count);
				AssertEquals("Number", "CNT 1", containerIdentificationNumbers.Single());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			transshipment = Factory.New<EnRouteTransshipment>();
			dataProvider = NCTSTranshipmentProvider.NewOrNull(transshipment);
		}
		EnRouteTransshipment transshipment;
		INCTSTranshipment dataProvider;

		protected override NCTSTranshipmentProvider GetProvider() => (NCTSTranshipmentProvider)dataProvider;
	}
}
