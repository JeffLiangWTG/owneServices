using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierIncidentDataTest : TestCase
	{
		public void TestItemNumber() => AssertEquals(1, nctsPrettierIncidentData.SequenceNumber);
		public void TestUCRReference() => AssertEquals(3, nctsPrettierIncidentData.Code);
		public void TestDescription() => AssertEquals("Change of tractor unit", nctsPrettierIncidentData.Text);
		public void TestTranshipmentContainerIndicator() => AssertEquals(true, nctsPrettierIncidentData.Transhipment.ContainerIndicator);
		public void TestTranshipmentTransportMeansNationality() => AssertEquals("AF", nctsPrettierIncidentData.Transhipment.TransportMeansNationality);
		public void TestTranshipmentTransportMeansIdentificationNumber() => AssertEquals("0012", nctsPrettierIncidentData.Transhipment.TransportMeansIdentificationNumber);
		public void TestTranshipmentTransportMeansTypeOfIdentification() => AssertEquals("30", nctsPrettierIncidentData.Transhipment.TransportMeansTypeOfIdentification);
		public void TestIncidentDataLocationAddress() => AssertNull("NCTSPrettierIncidentData.LocationAddress", nctsPrettierIncidentData.LocationAddress);
		public void TestIncidentTransportEquipments() => AssertType<NCTSPrettierIncidentTransportEquipmentData>(nctsPrettierIncidentData.TransportEquipments.FirstOrDefault());
		public void TestTransportEquipmentsSequenceNumber() => AssertEquals(1, nctsPrettierIncidentData.TransportEquipments.FirstOrDefault().SequenceNumber);
		public void TestTransportEquipmentsNumberOfSeals() => AssertEquals(1, nctsPrettierIncidentData.TransportEquipments.FirstOrDefault().NumberOfSeals);
		public void TestTransportEquipmentsContainerIdentificationNumber() => AssertEquals("WGPCGR", nctsPrettierIncidentData.TransportEquipments.FirstOrDefault().ContainerIdentificationNumber);
		public void TestTransportEquipmentsSealsSequenceNumber() => AssertEquals(1, nctsPrettierIncidentData.TransportEquipments.FirstOrDefault().Seals.FirstOrDefault().SequenceNumber);
		public void TestTransportEquipmentsSealsIdentifier() => AssertEquals("1234", nctsPrettierIncidentData.TransportEquipments.FirstOrDefault().Seals.FirstOrDefault().Identifier);
		protected override void SetUp()
		{
			base.SetUp();

			var transportEquipments = new List<INCTSTransportEquipment>
			{
				new NCTSPrettierIncidentTransportEquipmentData(transportEquipmentSequenceNumber: "1",
					transportEquipmentContainerIdentificationNumber: "WGPCGR",
					transportEquipmentNumberOfSeals: "1",
					transportEquipmentSeals: new List<INCTSSeal> { new NCTSPrettierSealData(sequenceNumber: "1", identifier: "1234") },
					goodsReferences: Enumerable.Empty<INCTSGoodsReference>().ToList())
			};

			nctsPrettierIncidentData = new NCTSPrettierIncidentData(sequenceNumber: "1", code: "3", text: "Change of tractor unit",
				transhipment: new NCTSPrettierTranshipmentData("1", "AF", "0012", "30"), locationAddress: null, transportEquipments)
			{
				EndorsementDate = ZDateTime.Today.ToDateTime(),
				EndorsementAuthority = "XIHMRC",
				EndorsementPlace = "GBBEL",
				EndorsementCountry = "XI",
				LocationQualifierOfIdentification = "U",
				LocationUNLocode = "GBBEL",
				LocationCountry = "XI",
				LocationLatitude = ZString.Empty,
				LocationLongitude = ZString.Empty
			};
		}
		NCTSPrettierIncidentData nctsPrettierIncidentData;
	}
}
