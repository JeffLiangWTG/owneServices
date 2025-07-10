using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BlanketVesselChange))]
	class BlanketVesselChangeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetJPM_BlanketChange()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<JPAFRHeader>();
				var blanketVesselChange = new BlanketVesselChange(header);
				blanketVesselChange.JPM_AreAllBillsChecked = ZBool.True;
				blanketVesselChange.JPM_BlanketChange = ZBool.True;
				AssertEquals("JPM_BlanketChange to True", ZBool.False, blanketVesselChange.JPM_AreAllBillsChecked);

				blanketVesselChange.JPM_AreAllBillsChecked = ZBool.True;
				blanketVesselChange.JPM_BlanketChange = ZBool.False;
				AssertEquals("JPM_BlanketChange to False", ZBool.True, blanketVesselChange.JPM_AreAllBillsChecked);
			});
		}

		public void TestIsShippingLineEntry()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header);
			AssertEquals(false, blanketVesselChange.IsShippingLineEntry);

			header.JPH_IsShippingLineEntry = ZBool.True;
			AssertEquals(true, blanketVesselChange.IsShippingLineEntry);
		}

		public void TestNewVesselDetails()
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

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032432";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.JPH_CarrierCode = "C123";
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_RadioCallSign = vessel.RV_RadioCallSign;
			header.JPH_RN_NKCountryOfReg = vessel.RV_RN_NKCountryOfReg;
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
			AssertEquals("C123", blanketVesselChange.JPM_CarrierCodeNew);
			AssertEquals(vessel.RV_Code, blanketVesselChange.JPM_VesselNameNew);
			AssertEquals("V123", blanketVesselChange.JPM_VoyageNumberNew);
			AssertEquals("O123", blanketVesselChange.JPM_OperatorVoyageNew);
			AssertEquals(portOfLoading.Code, blanketVesselChange.JPM_PortOfLoadingCodeNew);
			AssertEquals("1", blanketVesselChange.JPM_PortOfLoadingSuffixNew);
			AssertEquals(ZBool.True, blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNew);
			AssertEquals(new ZDateTime(2017, 5, 3), blanketVesselChange.JPM_ETDNew);
			AssertEquals(ZBool.False, blanketVesselChange.JPM_AreAllBillsChecked);

			AssertEquals(2, blanketVesselChange.BlanketVesselChangeBills.Count);
			var vesselChangeBill1 = blanketVesselChange.BlanketVesselChangeBills[0];
			var vesselChangeBill2 = blanketVesselChange.BlanketVesselChangeBills[1];
			AssertEquals(ZBool.False, vesselChangeBill1.JPM_Send);
			AssertEquals(ZBool.False, vesselChangeBill2.JPM_Send);

			blanketVesselChange.JPM_CarrierCodeNew = "C123";
			blanketVesselChange.JPM_VesselNameNew = vessel.RV_Code;
			AssertEquals(vessel.RV_RadioCallSign, blanketVesselChange.JPM_RadioCallSignNew);
			blanketVesselChange.JPM_VoyageNumberNew = "V123";
			blanketVesselChange.JPM_OperatorVoyageNew = "O123";
			blanketVesselChange.JPM_PortOfLoadingCodeNew = portOfLoading.Code;
			blanketVesselChange.JPM_PortOfLoadingSuffixNew = "1";
			blanketVesselChange.JPM_IsDepartureFromRelaxedAreaNew = ZBool.True;
			blanketVesselChange.JPM_ETDNew = new ZDateTime(2017, 5, 3);
			AssertEquals(vessel.PK, blanketVesselChange.Vessel.PK);
			AssertEquals(portOfLoading.PK, blanketVesselChange.Loading.PK);
			AssertEquals(false, blanketVesselChange.AreVesselDetailsDifferentFromHeader);
			blanketVesselChange.Factory.Save();

			AssertNotNull(header.NewVesselVoyage);
			AssertEquals(true, header.NewVesselVoyage.JP_BlanketChange);
			AssertEquals("C123", header.NewVesselVoyage.JP_CarrierCode);
			AssertEquals(vessel.RV_Code, header.NewVesselVoyage.JP_VesselName);
			AssertEquals(vessel.RV_RN_NKCountryOfReg, header.NewVesselVoyage.JP_VesselCountry);
			AssertEquals(vessel.RV_RadioCallSign, header.NewVesselVoyage.JP_VesselCallSign);
			AssertEquals("V123", header.NewVesselVoyage.JP_VoyageNumber);
			AssertEquals("O123", header.NewVesselVoyage.JP_OperationalCarrierVoyageNo);
			AssertEquals(portOfLoading.Code, header.NewVesselVoyage.JP_PortOfLoadingCode);
			AssertEquals(portOfLoading.RL_PortName, header.NewVesselVoyage.JP_PortOfLoadingName);
			AssertEquals("1", header.NewVesselVoyage.JP_PortOfLoadingSuffix);
			AssertEquals(ZBool.True, header.NewVesselVoyage.JP_IsDepartureFromRelaxedArea);
			AssertEquals(new ZDateTime(2017, 5, 3), header.NewVesselVoyage.JP_EstimatedDateTimeOfDeparture);

			vesselChangeBill1.JPM_Send = ZBool.False;
			AssertEquals(false, blanketVesselChange.JPM_AreAllBillsChecked);
			vesselChangeBill1.JPM_Send = ZBool.False;
			vesselChangeBill2.JPM_Send = ZBool.True;
			AssertEquals(false, blanketVesselChange.JPM_AreAllBillsChecked);
		}

		public void TestBlanketVesselChangeBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";

			var blanketVesselChange = new BlanketVesselChange(header);
			AssertEquals(2, blanketVesselChange.BlanketVesselChangeBills.Count);
			var vesselChangeBill1 = blanketVesselChange.BlanketVesselChangeBills[0];
			var vesselChangeBill = blanketVesselChange.BlanketVesselChangeBills[1];
			AssertEquals(bill1.PK, vesselChangeBill1.PK);
			AssertEquals(bill2.PK, vesselChangeBill.PK);
		}

		public void TestBillsToSend()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "MB3";
			var blanketVesselChange = new BlanketVesselChange(header);
			AssertEquals(3, blanketVesselChange.BlanketVesselChangeBills.Count);
			var changeBill1 = blanketVesselChange.BlanketVesselChangeBills[0];
			var changeBill2 = blanketVesselChange.BlanketVesselChangeBills[1];
			var changeBill3 = blanketVesselChange.BlanketVesselChangeBills[2];
			changeBill1.JPM_Send = ZBool.True;
			changeBill2.JPM_Send = ZBool.False;
			changeBill3.JPM_Send = ZBool.False;
			AssertEquals(1, blanketVesselChange.BillsToSend.Length);
			AssertEquals(changeBill1.JPM_BillOfLadingNumber, blanketVesselChange.BillsToSend[0].JPM_BillOfLadingNumber);
			changeBill3.JPM_Send = ZBool.True;
			AssertEquals(2, blanketVesselChange.BillsToSend.Length);
			AssertEquals(changeBill1.JPM_BillOfLadingNumber, blanketVesselChange.BillsToSend[0].JPM_BillOfLadingNumber);
			AssertEquals(changeBill3.JPM_BillOfLadingNumber, blanketVesselChange.BillsToSend[1].JPM_BillOfLadingNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new BlanketVesselChange(header);
		}
	}
}
