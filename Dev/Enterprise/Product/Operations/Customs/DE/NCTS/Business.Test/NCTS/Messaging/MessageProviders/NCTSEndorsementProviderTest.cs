using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSEndorsementProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSEndorsementProvider>
	{
		public void TestNewOrNull()
		{
			CombineAssertions(() =>
			{
				AssertNull("parameter null", NCTSEndorsementProvider.NewOrNull(null));

				SetRequiredInBondEventProperties();
				AssertNotNull("All properties specified", NCTSEndorsementProvider.NewOrNull(inBondEvent));

				SetRequiredInBondEventProperties();
				inBondEvent.BN_EndorsementDate = ZDateTime.Empty;
				AssertNull("Date missing", NCTSEndorsementProvider.NewOrNull(inBondEvent));

				SetRequiredInBondEventProperties();
				inBondEvent.BN_EndorsementPlace = ZString.Empty;
				AssertNull("Place missing", NCTSEndorsementProvider.NewOrNull(inBondEvent));

				SetRequiredInBondEventProperties();
				inBondEvent.BN_EndorsementAuthority = ZString.Empty;
				AssertNull("Authority missing", NCTSEndorsementProvider.NewOrNull(inBondEvent));

				SetRequiredInBondEventProperties();
				inBondEvent.BN_EndorsementCountryCode = ZString.Empty;
				AssertNull("Country missing", NCTSEndorsementProvider.NewOrNull(inBondEvent));
			});
		}

		public void TestPlace()
		{
			inBondEvent.BN_EndorsementPlace = "Place";
			AssertEquals("Entered", "Place", dataProvider.Place);
		}

		public void TestCountry()
		{
			inBondEvent.BN_EndorsementCountryCode = "DE";
			AssertEquals("Entered", "DE", dataProvider.Country);
		}

		public void TestDate()
		{
			inBondEvent.BN_EndorsementDate = new ZDateTime(2020, 5, 29, 1, 23, 1);
			AssertEquals("Entered", new DateTime(2020, 5, 29), dataProvider.Date);
		}

		public void TestAuthority()
		{
			inBondEvent.BN_EndorsementAuthority = "Authority";
			AssertEquals("Entered", "Authority", dataProvider.Authority);
		}

		void SetRequiredInBondEventProperties()
		{
			inBondEvent.BN_EndorsementDate = DateTime.Now;
			inBondEvent.BN_EndorsementPlace = "PLABC";
			inBondEvent.BN_EndorsementAuthority = "ABC";
			inBondEvent.BN_EndorsementCountryCode = "DE";
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			inBondEvent = header.EnRouteIncidents.AddNew();
			SetRequiredInBondEventProperties();
			dataProvider = NCTSEndorsementProvider.NewOrNull(inBondEvent);
		}
		CusInBondEvent inBondEvent;
		INCTSEndorsement dataProvider;

		protected override NCTSEndorsementProvider GetProvider() => (NCTSEndorsementProvider)dataProvider;
	}
}
