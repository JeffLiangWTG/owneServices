using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class JPAFRHeaderDataObjectCMVWriterTest : JPAFRBillsDataObjectWriterTest
	{
		public void TestPopulateAddInfoForNewVesselDetails()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_RadioCallSign = "12345";
			vessel.RV_RN_NKCountryOfReg = "CN";

			var portOfLoading = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfLoading.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfLoading.RL_PortName = "LN123";
			portOfLoading.Code = "L123";

			var portOfDischarge = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfDischarge.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfDischarge.RL_PortName = "DN123";
			portOfDischarge.Code = "D123";

			Factory.SaveForTesting();

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.JPH_CarrierCode = "C123";
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_Voyage = "V123";
			header.JPH_OperationalCarrierVoyageNo = "O123";
			header.JPH_RL_NKLoading = portOfLoading.Code;
			header.JPH_LoadingPortSuffix = "1";
			header.JPH_RelaxedAppId = ZBool.True;
			header.JPH_RL_NKDischarge = portOfDischarge.Code;
			header.JPH_DischargePortSuffix = "1";
			header.JPH_ETD = new ZDateTime(2017, 5, 3);
			header.JPH_ETA = new ZDateTime(2017, 5, 3);

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertEquals(2, header.Bills.Count);

			var blanketVesselChange = new BlanketVesselChange(header);
			AssertEquals(2, blanketVesselChange.BlanketVesselChangeBills.Count);

			blanketVesselChange.JPM_CarrierCodeNew = "C123";
			blanketVesselChange.JPM_VesselNameNew = vessel.RV_Code;
			AssertEquals(vessel.RV_RadioCallSign, blanketVesselChange.JPM_RadioCallSignNew);
			blanketVesselChange.JPM_VoyageNumberNew = "V123";
			blanketVesselChange.JPM_OperatorVoyageNew = "O123";
			blanketVesselChange.JPM_PortOfLoadingCodeNew = portOfLoading.Code;
			blanketVesselChange.JPM_PortOfLoadingSuffixNew = "1";
			blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNew = ZBool.True;
			blanketVesselChange.JPM_ETDNew = new ZDateTime(2017, 5, 3);
			blanketVesselChange.Factory.Save();

			var writer = new JPAFRHeaderDataObjectCMVWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AFR, header)));
			var headerData = writer.GetDataObject(header);
			CombineAssertions("VOCC", () =>
			{
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCodeNew)", "C123", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCodeNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselNameNew)", vessel.RV_Code, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselNameNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCallSignNew)", vessel.RV_RadioCallSign, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCallSignNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCountryNew)", vessel.RV_RN_NKCountryOfReg, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCountryNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VoyageNumberNew)", "V123", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VoyageNumberNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.OperationalCarrierVoyageNoNew)", "O123", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.OperationalCarrierVoyageNoNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingSuffixNew)", "1", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingSuffixNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingCodeNew)", portOfLoading.Code, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingCodeNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingNameNew)", portOfLoading.RL_PortName, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingNameNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.IsDepartureFromRelaxedAreaNew)", "Y", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.IsDepartureFromRelaxedAreaNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.EstimatedDateTimeOfDepartureNew)", new ZDateTime(2017, 5, 3).ToISO8601String(), headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.EstimatedDateTimeOfDepartureNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.BlanketChange)", "Y", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.BlanketChange));
			});

			header.JPH_IsShippingLineEntry = false;
			writer = new JPAFRHeaderDataObjectCMVWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AFR, header)));
			headerData = writer.GetDataObject(header);
			CombineAssertions("NVOCC", () =>
			{
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCodeNew)", "C123", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCodeNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselNameNew)", vessel.RV_Code, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselNameNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCallSignNew)", vessel.RV_RadioCallSign, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCallSignNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCountryNew)", vessel.RV_RN_NKCountryOfReg, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCountryNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VoyageNumberNew)", "V123", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.VoyageNumberNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.OperationalCarrierVoyageNoNew)", null, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.OperationalCarrierVoyageNoNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingSuffixNew)", "1", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingSuffixNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingCodeNew)", portOfLoading.Code, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingCodeNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingNameNew)", portOfLoading.RL_PortName, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingNameNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.IsDepartureFromRelaxedAreaNew)", "Y", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.IsDepartureFromRelaxedAreaNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.EstimatedDateTimeOfDepartureNew)", new ZDateTime(2017, 5, 3).ToISO8601String(), headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.EstimatedDateTimeOfDepartureNew));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.BlanketChange)", "Y", headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Header.BlanketChange));
			});
		}
	}
}
