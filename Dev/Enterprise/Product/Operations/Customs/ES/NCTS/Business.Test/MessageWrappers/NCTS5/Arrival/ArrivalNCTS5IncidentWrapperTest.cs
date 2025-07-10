using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5IncidentWrapperTest : WrapperHelperTest<ArrivalNCTS5IncidentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if enRouteIncident is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "enRouteIncident"), () => GetWrapper(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "2", wrapper.SequenceNumber);
		}

		public void TestCode()
		{
			enRouteIncident.BN_IncidentCode = "1";
			wrapper = GetWrapper(enRouteIncident);
			AssertEquals("Expected filled BN_IncidentCode", "1", wrapper.Code);
		}

		public void TestText()
		{
			enRouteIncident.BN_Information = "AAAA";
			wrapper = GetWrapper(enRouteIncident);
			AssertEquals("Expected filled BN_Information", "AAAA", wrapper.Text);
		}

		public void TestEndorsement()
		{
			var endorsement = wrapper.Endorsement;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Endorsement", endorsement);
				AssertSame("Cached Endorsement", wrapper.Endorsement, endorsement);
			});
		}

		public void TestLocation()
		{
			wrapper = GetWrapper(enRouteIncident);
			var location = wrapper.Location;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Location", location);
				AssertSame("Cached Location", wrapper.Location, location);
			});
		}

		public void TestTransportEquipment()
		{
			wrapper = GetWrapper(enRouteIncident);
			var transportEquipment = wrapper.TransportEquipment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled TransportEquipment", transportEquipment);
				AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);

				var cont1 = enRouteIncident.IncidentContainers.AddNew();
				cont1.BC_Mode = Core.Constants.ContainerModes.Containerised;
				cont1.BC_ContainerNum = "CONT1";
				cont1.BC_Seal1 = "SEAL1";
				cont1.BC_Seal2 = "SEAL2";
				cont1.Seals.AddNew().BK_SealNumber = "SEAL3";

				var cont2 = enRouteIncident.IncidentContainers.AddNew();
				cont2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				cont2.BC_ContainerNum = "CONT2";
				cont1.BC_Seal1 = "SEAL5";

				var cont3 = enRouteIncident.IncidentContainers.AddNew();
				cont3.BC_Mode = Core.Constants.ContainerModes.Containerised;
				cont3.BC_ContainerNum = "CONT3";
				cont3.BC_Seal1 = "SEAL4";

				wrapper = GetWrapper(enRouteIncident);
				transportEquipment = wrapper.TransportEquipment;
				AssertEquals("Expected filled TransportEquipment with 3 elements", 3, transportEquipment.Count);
				AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);

				var transportEquipmentArray = transportEquipment.ToArray();
				AssertEquals("Expected filled TransportEquipment[0].SequenceNumber", "1", transportEquipmentArray[0].SequenceNumber);
				AssertEquals("Expected filled TransportEquipment[0].ContainerIdentificationNumber", "CONT1", transportEquipmentArray[0].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[0].NumberOfSeals", "3", transportEquipmentArray[0].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[1].SequenceNumber", "2", transportEquipmentArray[1].SequenceNumber);
				AssertEquals("Expected filled TransportEquipment[1].ContainerIdentificationNumber", "CONT3", transportEquipmentArray[1].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[1].NumberOfSeals", "1", transportEquipmentArray[1].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[2].SequenceNumber", "3", transportEquipmentArray[2].SequenceNumber);
				AssertEquals("Expected empty when NCT TransportEquipment[2].ContainerIdentificationNumber", ZString.Empty, transportEquipmentArray[2].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[2].NumberOfSeals", "0", transportEquipmentArray[2].NumberOfSeals);
			});
		}

		public void TestTranshipment()
		{
			wrapper = GetWrapper(enRouteIncident);

			CombineAssertions(() =>
			{
				enRouteIncident.BN_TransportAtDepartureType = ZString.Empty;
				AssertNull("Expected null Transhipment when Transport Means is empty", wrapper.Transhipment);

				enRouteIncident.BN_TransportAtDepartureType = "3";
				var transhipment = wrapper.Transhipment;
				AssertNotNull("Expected filled Transhipment", transhipment);
				AssertSame("Cached Transhipment", wrapper.Transhipment, transhipment);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
			enRouteIncident.IncidentContainers.AddNew();

			wrapper = GetWrapper(enRouteIncident, seqNum: 2);
		}
		ArrivalNCTS5IncidentWrapper wrapper;
		EnRouteIncident enRouteIncident;

		ArrivalNCTS5IncidentWrapper GetWrapper(EnRouteIncident enRouteIncident, int seqNum = 1) => new ArrivalNCTS5IncidentWrapper(enRouteIncident, (ZShort)seqNum);

		protected override ArrivalNCTS5IncidentWrapper GetProvider() => wrapper;
	}
}
