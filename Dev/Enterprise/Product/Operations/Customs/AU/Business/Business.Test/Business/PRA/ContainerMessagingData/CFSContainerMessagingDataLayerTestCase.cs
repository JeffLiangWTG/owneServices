using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CFSContainerMessagingDataLayerTestCase : FreightDataLayerTest
	{
		public void TestCFSSpecificProperties()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "CONZIGNOR GRIMO";

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_UniqueConsignRef = "Z00000001";
			consol.JK_RL_NKDischargePort = "USABQ";
			var transport = consol.Transports[0];
			transport.JW_JX = CreateSailing().PK;

			var shippingLine = Factory.New<OrgHeader>();
			var cusCode1 = shippingLine.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "BBBB2";
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST!@#";
			container.JC_SealNum = "HARPIC";
			container.JC_ExportDepotCustomsReference = "ABC2362324";
			var refContainer = Factory.New<RefContainer>();
			container.JC_RC = refContainer.PK;
			container.Container.RC_HasTynes = true;
			container.Container.RC_ISOType = "1099";
			container.Container.RC_GrossWeight = 5m;
			container.JC_GrossWeight = 5m;
			container.JC_TareWeight = 2m;

			container.JC_TotalLength = container.RefContainer.RC_Length + Core.Constants.Length.Convert(11m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
			container.JC_TotalWidth = container.RefContainer.RC_Width + Core.Constants.Length.Convert(22m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
			container.JC_TotalHeight = container.RefContainer.RC_Height + Core.Constants.Length.Convert(33m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);

			container.JC_OverhangBack = Core.Constants.Length.Convert(3m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
			container.JC_OverhangRight = Core.Constants.Length.Convert(4m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);

			container.JC_RefrigGeneratorID = "R101";
			container.JC_SetPointTemp = 12.0m;
			container.JC_DepartureSlotReference = "VBSBOOK";
			container.JC_AirVentFlow = 100.15m;
			container.JC_AirVentFlowRateUnit = "MQH";

			container.JC_DepartureDeliveryByRail = true;
			container.JC_HumidityPercent = 33;

			var code = Factory.New<RefCommodityCode>();
			code.RH_Description = "YOUGUESS";
			code.RH_Code = "XXXX";
			container.JC_RH_NKContainerCommodityCode = code.RH_Code;

			container.JC_DepartureTruckRegistration = "HECTIK";
			container.JC_DepartureTime = new ZDateTime(2006, 08, 23);

			var leg = container.OriginCFSDeparture;
			leg.EU_VehicleRegistration = "HECTIK";
			leg.EU_PickupDeliveryTime = new ZDateTime(2006, 08, 23);

			var cartageCo = Factory.New<OrgHeader>();
			cartageCo.OH_Code = "CartageCo";
			cartageCo.PrimaryRegistrationNumber.Number = "2362342342";
			consol.JK_OA_CartageCoAddress = cartageCo.MainAddress.PK;

			var cTO = Factory.New<OrgHeader>();
			cTO.OH_Code = "CTO";
			cTO.SetCustomsCode(OrgCusCode.CodeTypes.OneStopCode, GlbBranch.CurrentBranch.Country, "ASLPB");
			var cTOAddress = cTO.Addresses.AddNew();
			cTOAddress.OA_Address1 = "1 CuckooSqueaker Ln";
			consol.JK_OA_CTOAddress = cTOAddress.PK;
			consol.SetDefaultShippingLineAddress(shippingLine);

			var dataLayer = new CFSContainerMessagingDataLayer(container);
			AssertEquals("AARDVAARK", dataLayer.VesselName);
			AssertEquals("35234", dataLayer.Voyage);
			AssertEquals("3214235", dataLayer.LloydsNumber);
			AssertEquals("ABC2362324", dataLayer.ECNorCRN);
			AssertEquals("2362342342", dataLayer.CartageCompanyABN);
			AssertEquals("ASLPB", dataLayer.LoadTerminal1StopCode);
			AssertEquals(7m, dataLayer.ContainerGrossWeight);
			AssertEquals(2m, dataLayer.ContainerTareWeight);
			AssertEquals(5m, dataLayer.ContainerNetWeight);
			AssertEquals("HECTIK", dataLayer.TruckRegoNumber);
			AssertEquals(new ZDateTime(2006, 08, 23), dataLayer.RoadScheduledDeparture);

			AssertEquals("DataLayer.ContainerNumber", "TEST!@#", dataLayer.ContainerNumber);
			AssertEquals("DataLayer.AirVentSetting", 100, dataLayer.AirVentSetting);
			AssertEquals("DataLayer.AirVentSetting", "MQH", dataLayer.AirVentSettingUnit);
			AssertEquals("DataLayer.ArrivingAtCTOByRail", true, dataLayer.ArrivingAtCTOByRail);
			AssertEquals("DataLayer.Consignor", "CONZIGNOR GRIMO", dataLayer.ConsignorName);
			AssertEquals("DataLayer.Commodity1StopCode", "XXXX", dataLayer.Commodity1StopCode);
			AssertEquals("DataLayer.GoodsDescription", "YOUGUESS", dataLayer.GoodsDescription);
			AssertEquals("DataLayer.HasTynes", true, dataLayer.HasTynes);
			AssertEquals("DataLayer.HumidityPercentage", 33, dataLayer.HumidityPercentage);
			AssertEquals("DataLayer.ISOContainerType", "1099", dataLayer.ISOContainerType);
			AssertEquals("DataLayer.MessageReference", "CON-Z00000001-TEST!@#", dataLayer.MessageReference);
			AssertEquals("DataLayer.OverhangFrontInCM", 8, dataLayer.OverhangFrontInCM);
			AssertEquals("DataLayer.OverhangBackInCM", 3, dataLayer.OverhangBackInCM);
			AssertEquals("DataLayer.OverhangLeftInCM", 18, dataLayer.OverhangLeftInCM);
			AssertEquals("DataLayer.OverhangRightInCM", 4, dataLayer.OverhangRightInCM);
			AssertEquals("DataLayer.OverhangHeightInCM", 33, dataLayer.OverhangHeightInCM);
			AssertEquals("DataLayer.PortOfDischarge", "USLAX", dataLayer.PortOfDischarge);
			AssertEquals("DataLayer.PortOfFinalDischarge", "USABQ", dataLayer.PortOfFinalDischarge);
			AssertEquals("DataLayer.PortOfLoading", "AUSYD", dataLayer.PortOfLoading);
			AssertEquals("DataLayer.ReeferGeneratorID", "R101", dataLayer.ReeferGeneratorID);
			AssertEquals("DataLayer.SealNumber", "HARPIC", dataLayer.SealNumber);
			AssertEquals("DataLayer.SenderContactName", GlbStaff.CurrentUser.GS_FullName, dataLayer.SenderContactName);
			AssertEquals("DataLayer.SenderFax", GlbStaff.CurrentUser.GS_FaxNum, dataLayer.SenderFax);
			AssertEquals("DataLayer.SenderPhone", GlbStaff.CurrentUser.GS_WorkPhone, dataLayer.SenderPhone);
			AssertEquals("DataLayer.TemperatureSetting", "12.0", dataLayer.TemperatureSettingFormatted);
			AssertEquals("DataLayer.TerminalVBSBooking", "VBSBOOK", dataLayer.TerminalVBSBooking);
			AssertEquals("DataLayer.ShippingLine1StopCode", "BBBB2", dataLayer.ShippingLine1StopCode);

			//UnMapped Fields
			AssertEquals("dataLayer.ShippingLineBookingReference", "", dataLayer.ShippingLineBookingReference);
			AssertEquals("dataLayer.FlatRackID", "", dataLayer.FlatRackID);
			AssertEquals("dataLayer.RoadDest1StopCode", "", dataLayer.RoadDest1StopCode);
			AssertEquals("dataLayer.RoadOrig1StopCode", "", dataLayer.RoadOrig1StopCode);
			AssertEquals("dataLayer.CartageBookingReference", "", dataLayer.CartageBookingReference);
			AssertEquals("dataLayer.RoadScheduledArrival", ZDateTime.Empty, dataLayer.RoadScheduledArrival);
		}

		protected override ContainerMessagingData GetDataLayerPassingEmptyContainerIntoConstructor()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var container = consol.Containers.AddNew();
			containerMessaging = container;
			return new CFSContainerMessagingDataLayer(container);
		}

		protected override ContainerMessagingData GetDataLayerPassingNullIntoConstructor() => new CFSContainerMessagingDataLayer(Factory.New<CFSContainer>());

		protected override string ECNorCRNErrorText => "ECN/CRN on the container.\r\n";

		protected override string ExpectedPRAErrors
		{
			get
			{
				return @"
Shipping Line Booking Reference. (Carrier Ref on the load list)
ECN/CRN on the container.
Carrier 1-Stop Code. (Registration Numbers on the Config Tab in the Carrier Master File)
Vessel Name. (on the container or the main transport leg)
Voyage. (on the container or the main transport leg)
Lloyds Number. (Vessel Master File for selected vessel on the container or the main transport leg)
Port Of Loading. (on the load list)
Departure CTO 1-Stop Code. (Registration Numbers on the Config Tab in the CTO Master File)
Port Of Discharge. (on the load list)
Container Number. (Containers Tab in the Grid)
ISO Container Type. (Containers Tab in the Grid)
Commodity 1-Stop Code. (Container Details on the container)
Container Gross Weight. (Dimensions on the container)
Seal Number Not Entered. (Container Details on the container)
Container Verified Weight Method must be CNT or PKG. (VGM Tab on the Containers Tab)
";
			}
		}

		JobSailing CreateSailing()
		{
			var result = Factory.New<JobSailing>();
			var testVoyage = Factory.New<JobVoyage>();
			var voyOrigin = Factory.New<VoyageOrigin>();
			var voyDest = Factory.New<VoyageDestination>();
			voyOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			voyDest.JB_RL_NKPortOfDischarge = "USLAX";
			testVoyage.JV_VoyageFlight = "35234";
			var vessel = RefVessel.New(Factory);
			vessel.RV_LloydsNumber = "3214235";
			vessel.RV_Code = "AARDVAARK";
			testVoyage.JV_RV_NKVessel = vessel.RV_FK;
			voyOrigin.JA_JV = testVoyage.PK;
			voyDest.JB_JV = testVoyage.PK;
			result.JX_JA = voyOrigin.PK;
			result.JX_JB = voyDest.PK;
			return result;
		}
	}
}
