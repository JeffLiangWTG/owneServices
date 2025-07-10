using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CUSPRLCusTempStorageJobHeaderProviderTest : CusTempStorageJobHeaderProviderAbstractTest<CUSPRLCusTempStorageJobHeaderProvider>
	{
		public void TestBorderTransportMode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty transport mode", 0, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Sea Transport Mode", 1, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("Rail Transport Mode", 2, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("Road Transport Mode", 3, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Air Transport Mode", 4, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals("Mail Transport Mode", 5, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertEquals("Fixed Installation Mode", 7, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
				AssertEquals("Inland Waterway Transport Mode", 8, TempStorageHeaderWrapped.BorderTransportMode);
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				AssertEquals("Own Propulsion Transport Mode", 9, TempStorageHeaderWrapped.BorderTransportMode);
			});
		}

		public void TestConveyanceReferenceNumber()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "KOTA MEWAH 2";
			vessel.RV_LloydsNumber = "7909518";
			Factory.Save();

			CombineAssertions(() =>
			{
				tempStorageHeader.SJH_TransportRegNo = "QF309";
				tempStorageHeader.SJH_TransportMeansCode = ZString.Empty;
				AssertEquals("No transport Means no Reg No", ZString.Empty, TempStorageHeaderWrapped.ConveyanceReferenceNumber);
				var transportMeansNotRequiringTransportRegNo = new[] { TemporaryStorageTransportMeansList.Codes.Truck, TemporaryStorageTransportMeansList.Codes.Wagon, TemporaryStorageTransportMeansList.Codes.Car
					, TemporaryStorageTransportMeansList.Codes.Without, TemporaryStorageTransportMeansList.Codes.Other };
				foreach (var transportMeans in transportMeansNotRequiringTransportRegNo)
				{
					tempStorageHeader.SJH_TransportMeansCode = transportMeans;
					AssertEquals($"Transport Rego No not required for {transportMeans}", ZString.Empty, TempStorageHeaderWrapped.ConveyanceReferenceNumber);
				}
				tempStorageHeader.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Aircraft;
				AssertEquals("Flight Number", "QF309", TempStorageHeaderWrapped.ConveyanceReferenceNumber);
				tempStorageHeader.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Vessel;
				tempStorageHeader.SJH_TransportRegNo = "INVALID VESSEL";
				AssertEquals("Cannot load vessel", ZString.Empty, TempStorageHeaderWrapped.ConveyanceReferenceNumber);
				tempStorageHeader.SJH_TransportRegNo = "KOTA MEWAH 2";
				AssertEquals("Vessel Lloyds", "7909518", TempStorageHeaderWrapped.ConveyanceReferenceNumber);
			});
		}

		protected override ITempStorageHeader GetTempStorageHeaderWrapped(CusTempStorageJobHeader tempStorageHeader) => new CUSPRLCusTempStorageJobHeaderProvider(tempStorageHeader);

		new ICUSPRLTempStorageHeader TempStorageHeaderWrapped => (ICUSPRLTempStorageHeader)base.TempStorageHeaderWrapped;
	}
}
