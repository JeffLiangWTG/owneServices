using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class IncidentsProviderTest : Customs.Business.Testing.DataProviderTestCase<IncidentsProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IncidentsProvider(null, 0));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestCode()
		{
			incident.BN_IncidentCode = "A";
			AssertEquals("A", Provider.Code);
		}

		public void TestText()
		{
			incident.BN_Information = "info";
			AssertEquals("info", Provider.Text);
		}

		public void TestEndorsement()
		{
			AssertNotNull(Provider.Endorsement);
		}

		public void TestLocation()
		{
			AssertNotNull(Provider.Location);
		}

		public void TestTransportEquipments_IncidentCode2()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._2;
			AssertNotNull(Provider.TransportEquipments);
		}

		public void TestTransportEquipments_IncidentCode3()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._3;
			AssertNotNull(Provider.TransportEquipments);
		}

		public void TestTransportEquipments_IncidentCode4()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._4;
			AssertNotNull(Provider.TransportEquipments);
		}

		public void TestTransportEquipments_IncidentCode6()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._6;
			AssertNotNull(Provider.TransportEquipments);
		}

		public void TestTransportEquipments_IncidentCodeNot2Or3Or4Or6()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._1;
			AssertNotNull(Provider.TransportEquipments);
		}

		public void TestTranshipment_IncidentCode3()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._3;
			AssertNotNull(Provider.Transhipment);
		}

		public void TestTranshipment_IncidentCode4()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._4;
			AssertNotNull(Provider.Transhipment);
		}

		public void TestTranshipment_IncidentCodeNot3Or4()
		{
			incident.BN_IncidentCode = IncidentCodeList.Codes._1;
			AssertNull(Provider.Transhipment);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = header.EnRouteIncidents.AddNew();
			incident.BN_IncidentCode = IncidentCodeList.Codes._2;

			provider = new IncidentsProvider(incident, 1);
		}

		protected override IncidentsProvider GetProvider() => provider;

		IncidentsProvider provider;
		EnRouteIncident incident;
	}
}
