using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE007IncidentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE007IncidentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Incident missing", () => new IE007IncidentProvider(null));
		}

		public void TestCode()
		{
			AssertEquals("1", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("Incident Information 1", Provider.Text);
		}

		public void TestEndorsement()
		{
			AssertSame("Endorsement", Provider, Provider.Endorsement);
		}

		public void TestLocationOfGoods()
		{
			incident.GoodsLocation.CGL_Qualifier = "X";
			AssertEquals("LocationOfGoods", "X", Provider.LocationOfGoods.QualifierOfIdentification);
		}

		public void TestHasContainer()
		{
			incident.IncidentContainers.AddNew().BC_Mode = "CNT";
			Assert("HasContainer", Provider.HasContainer);
		}

		public void TestTransportEquipments()
		{
			incident.BN_IncidentCode = "2";
			var container = incident.IncidentContainers.AddNew();
			AssertType<ITransportEquipmentWithSeals[]>("TransportEquipments", Provider.TransportEquipments);
			AssertEquals("Empty as no Container", 0, Provider.TransportEquipments.Count);

			container.BC_Mode = "CNT";
			var provider = GetProvider();
			AssertEquals("1 Record", 1, provider.TransportEquipments.Count);

			incident.BN_IncidentCode = "1";
			provider = GetProvider();
			AssertEquals("Empty as Code = 1", 0, provider.TransportEquipments.Count);

			incident.BN_IncidentCode = "5";
			provider = GetProvider();
			AssertEquals("Empty as Code = 5", 0, provider.TransportEquipments.Count);
		}

		public void TestTranshipment()
		{
			incident.BN_IncidentCode = "3";
			incident.IncidentContainers.AddNew().BC_Mode = "CNT";
			Assert("HasContainer", Provider.Transhipment.HasContainer);

			incident.BN_IncidentCode = "1";
			var provider = GetProvider();
			AssertNull("Null as Code = 1", provider.Transhipment);

			incident.BN_IncidentCode = "2";
			provider = GetProvider();
			AssertNull("Null as Code = 2", provider.Transhipment);

			incident.BN_IncidentCode = "4";
			provider = GetProvider();
			AssertNull("Null as Code = 4", provider.Transhipment);

			incident.BN_IncidentCode = "5";
			provider = GetProvider();
			AssertNull("Null as Code = 5", provider.Transhipment);
		}

		#region IE007Endorsement Members Test
		public void TestDate()
		{
			AssertEquals(ZDateTime.BrettsBirthday, Provider.Date);
		}

		public void TestAuthority()
		{
			AssertEquals("AUTH 01", Provider.Authority);
		}

		public void TestPlace()
		{
			AssertEquals("DUBLIN", Provider.Place);
		}

		public void TestCountry()
		{
			AssertEquals("IE", Provider.Country);
		}
		#endregion

		protected override IE007IncidentProvider GetProvider() => new IE007IncidentProvider(incident);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = nctsHeader.EnRouteIncidents.AddNew();
			incident.BN_IncidentCode = "1";
			incident.BN_Information = "Incident Information 1";
			incident.BN_EndorsementDate = ZDateTime.BrettsBirthday;
			incident.BN_EndorsementAuthority = "AUTH 01";
			incident.BN_EndorsementPlace = "DUBLIN";
			incident.BN_EndorsementCountryCode = "IE";
		}

		EnRouteIncident incident;
		NctsHeader nctsHeader;
	}
}
