using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoTestDataSetter
	{
		public SeaCargoTestDataSetter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public CusSCAOceanBill CreateOceanBill()
		{
			var consol = CreateConsol();
			return factory.LoadTop1<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK));
		}

		public CusSCAOceanBill CreateAmendableOceanBill(bool shouldSetUpCertificate)
		{
			var result = CreateOceanBill();
			result.HouseBills[0].CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			if (shouldSetUpCertificate)
			{
				new ZTestHelper(factory).SetupCertificates();
			}

			return result;
		}

		public CusSCAOceanBill CreateAmendableOceanBillWithChanges(bool shouldSetUpCertificate)
		{
			var result = CreateAmendableOceanBill(shouldSetUpCertificate);
			result.HouseBills[0].CA_ConsigneeName = "TRITEC PTY LTD changed";
			return result;
		}

		public CusSCAHouse CreateAmendableHouseBill(bool shouldSetUpCertificate)
		{
			var result = CreateAmendableOceanBill(shouldSetUpCertificate);
			return result.HouseBills[0];
		}

		public CusSCAHouse CreateAmendableHouseBillWithChanges(bool shouldSetUpCertificate)
		{
			var result = CreateAmendableOceanBillWithChanges(shouldSetUpCertificate);
			return result.HouseBills[0];
		}

		public ForwardingConsol CreateConsol()
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "AAA";
			CreateTestVessel(TestVesselName);
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = TestPortOfLoading;
			consol.JK_RL_NKDischargePort = TestPortOfDischarge;
			consol.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2005, 1, 1);
			var transport = consol.Transports[0];
			consol.JK_MasterBillNum = "OCEANTEST123";
			transport.JW_Vessel = TestVesselName;
			transport.JW_VoyageFlight = "23";
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "TESTHOUSE123";
			shipment.JS_RL_NKOrigin = TestPortOfLoading;
			shipment.JS_RL_NKDestination = TestPortOfDischarge;
			shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);
			var consignee = factory.New<OrgHeader>();
			consignee.OH_Code = "TESTIGNEE";
			consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
			shipment.ConsigneePK = consignee.PK;
			var consignor = factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
			shipment.ConsignorPK = consignor.PK;
			var oceanBill = factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "SUDU400014772110";
			oceanBill.CB_PrincipalID = "C065301902";
			oceanBill.CB_VesselName = TestVesselName;
			oceanBill.CB_Voyage = "442";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "49104160312018";
			houseBill.CA_RN_NKGoodsOrigin = "NZ";
			houseBill.CA_RL_NK_PortOfDestination = "AUBNE";
			houseBill.CA_RL_NK_PortOfOrigin = "NZAKL";
			houseBill.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			houseBill.CA_ConsigneeName = "TRITEC PTY LTD";
			houseBill.CA_ConsigneeAddress1 = "C O NEW CENTURY PACKING";
			houseBill.CA_ConsigneeAddress2 = "UNIT 5 370 NUDGEE ROAD";
			houseBill.CA_ConsigneeSuburb = "HENDRA QLD AUSTRALIA";
			houseBill.CA_ConsigneePostcode = "4011";
			houseBill.CA_ConsignorName = "HUHTAMAKI VAN LEER  NZ  LTD";
			houseBill.CA_ConsignorAddress1 = "FLEXIBLE PACKAGING DIVISION";
			houseBill.CA_ConsignorAddress2 = "PRIVATE BAG 93 002";
			houseBill.CA_ConsignorSuburb = "NEW LYNN AUCKLAND";
			houseBill.CA_NotifyName = "TRITEC PTY LTD";
			houseBill.CA_NotifyAddress1 = "C O NEW CENTURY PACKING";
			houseBill.CA_NotifyAddress2 = "UNIT 5 370 NUDGEE ROAD";
			houseBill.CA_NotifySuburb = "HENDRA QLD AUSTRALIA";
			houseBill.CA_NotifyPostcode = "4011";
			houseBill.CA_HouseBill = "49104160312018";
			houseBill.CA_JS = shipment.PK;
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "FSCU6400235";
			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.CN_RC_NKContainerType = "40GP";
			container.CN_ShipperOwnedContainer = false;
			container.CN_SealNumber = "89644";
			CusSCAPivot houseContainerPivot = houseBill.Pivot.AddNew();
			houseContainerPivot.CV_PackageCount = 40;
			houseContainerPivot.CV_PackageType = "BX";
			houseContainerPivot.CV_MarksAndNumbers = "FSCU6400235";
			houseContainerPivot.CV_GoodsDescription = "STC 40 BAGS OF PLASTIC REGRIND";
			houseContainerPivot.CV_WeightUQ = Core.Constants.Weight.Kilograms;
			houseContainerPivot.CV_Weight = 23900.00m;
			houseContainerPivot.CV_Volume = 68.000m;
			houseContainerPivot.CV_CN = container.PK;
			var container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerMode = "BBK";
			container2.CN_ContainerNumber = CusSCAPivot.BreakBulk;
			CusSCAPivot houseContainerPivot2 = houseBill.Pivot.AddNew();
			houseContainerPivot2.CV_CN = container2.PK;
			houseContainerPivot2.CV_PackageCount = 56;
			houseContainerPivot2.CV_PackageType = "BX";
			houseContainerPivot2.CV_MarksAndNumbers = "WOW THIS IS GREAT";
			houseContainerPivot2.CV_GoodsDescription = "WOMEN TIED UP";
			houseContainerPivot2.CV_WeightUQ = Core.Constants.Weight.Kilograms;
			houseContainerPivot2.CV_Weight = 23900.00m;
			houseContainerPivot2.CV_Volume = 68.000m;
			factory.Save();
			return consol;
		}

		void CreateTestVessel(string vesselName)
		{
			var vessel = RefVessel.New(factory);
			vessel.RV_Code = vesselName;
			vessel.RV_LloydsNumber = "1234567";
			var shippingLine = factory.New<OrgHeader>();
			shippingLine.OH_Code = "TESTSHIP";
			shippingLine.MainAddress.OA_Address1 = "TEST SHIP ADDRESS";
			shippingLine.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "C067764014");
			vessel.RV_OH = shippingLine.PK;
		}

		const string TestVesselName = "SCOTTSFLOATINGBROTHEL";
		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "AUSYD";
	}
}
