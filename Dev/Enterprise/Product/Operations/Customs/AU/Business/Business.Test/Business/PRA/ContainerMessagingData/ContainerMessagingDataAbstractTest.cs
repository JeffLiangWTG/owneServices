using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class ContainerMessagingDataAbstractTest : TestCaseWithFactory
	{
		public void TestSenderID()
		{
			GlbCompany.CurrentCompany.GC_Code = "BBB";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;

			ContainerMessagingData containerMessagingData;
			registrationKey.EnterpriseCodeForTest = "AAA";
			registrationKey.ServerCodeForTest = "CCC";
			containerMessagingData = GetDataLayerPassingNullIntoConstructor();
			AssertEquals("AAACCC", containerMessagingData.SenderID);

			registrationKey.EnterpriseCodeForTest = "";
			containerMessagingData = GetDataLayerPassingNullIntoConstructor();
			AssertEquals("BBBCCC", containerMessagingData.SenderID);
		}

		public void TestInvalidCommodity()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.Commodity1StopCode = "XXXX";
			AssertNoExceptionThrown(() => containerMessagingData.GetWarningText());
			Assert(containerMessagingData.GetErrorText().Contains("Commodity 1-Stop Code."));
		}

		public void TestCommodityCodeWarnings()
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "CONF";
			commodity.RH_Description = "CONFECTIONERY";
			commodity.RH_IsShipping = true;
			commodity.RH_IsForwarding = false;
			commodity.RH_IsLandTransport = false;
			commodity.RH_ContainerVentRequired = true;
			commodity.RH_ReeferMinTemperature = 8.0m;
			commodity.RH_ReeferMaxTemperature = 18.0m;

			commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AABT";
			commodity.RH_Description = "ANIMAL ANTIBIOTICS";
			commodity.RH_IsShipping = true;
			commodity.RH_ExpiryDate = new ZDateTime(2005, 3, 14, 0, 0, 0);

			Factory.Save();

			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.Commodity1StopCode = "CONF";
			containerMessagingData.Temperature = 0;
			containerMessagingData.AirVentSetting = 5;
			containerMessagingData.IsTempControlled = true;
			AssertEquals("Temperature warning expected", "Temperature must be between 8.0 and 18.0 for the commodity code CONF (CONFECTIONERY).\r\n", containerMessagingData.GetWarningText());
			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.Temperature = 10;
			AssertEquals("Should have no Warning", "", containerMessagingData.GetWarningText());
			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.AirVentSetting = 0;
			AssertEquals("Should have AirVentSetting Warning", "Commodity code CONF requires a container air vent setting to be entered.\r\n", containerMessagingData.GetWarningText());
			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.Commodity1StopCode = "AABT";
			AssertEquals("Should have Date Warning", "This Commodity code is marked as expired as of 14-Mar-05 00:00:00.\r\n", containerMessagingData.GetWarningText());
		}

		public void TestValidDataHasNoErrorsOrWarnings()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();

			AssertEquals("Should have no Errors here", "", containerMessagingData.GetErrorText());
			AssertEquals("Should have no Warnings here", "", containerMessagingData.GetWarningText());
		}

		public void TestGeneralISOCodeWithTempControlledContainerWarning()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.Commodity1StopCode = "GENL";
			containerMessagingData.TemperatureSettingFormatted = "05.0";
			containerMessagingData.IsTempControlled = true;

			string warning = "Commodity Code should not be 'GENL' for Temperature Controlled Containers";
			Assert("Should have Warning", containerMessagingData.GetWarningText().IndexOf(warning) != -1);
		}

		public void TestGeneralISOCodeWithHazardousGoodsWarning()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.Commodity1StopCode = "GENL";
			containerMessagingData.DangerousGoodsList.AddNew();

			string warning = "Commodity Code should not be 'GENL' for Containers containing Hazardous Goods";
			Assert("Should have Warning", containerMessagingData.GetWarningText().IndexOf(warning) != -1);
		}

		public void TestInvalidCarrierCode()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.ShippingLine1StopCode = "HELLO";
			string errorText = "Must be three characters";
			Assert("Should have Warning", containerMessagingData.GetErrorText().IndexOf(errorText) != -1);
		}

		public void TestGrossWeightUnderMinimumWarning()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.ContainerGrossWeight = 1100;

			string warning = "Gross Weight should not be less than 2300kg for a 20' Container";
			Assert("Should have Warning", containerMessagingData.GetWarningText().IndexOf(warning) != -1);
		}

		public void TestDepartureCTOError()
		{
			var containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.PortOfLoading = "AUMEL";
			containerMessagingData.LoadTerminal1StopCode = "VICTM";
			containerMessagingData.GrossWeightVerifiedType = PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;

			AssertEquals("Precondition: should have no errors here", "", containerMessagingData.GetErrorText());

			containerMessagingData.LoadTerminal1StopCode = "CONFR";

			var praErrors = containerMessagingData.GetErrorText();
			AssertContains("Should have error", "Verified Method WTA is only valid for VICT CTO. (Registration Numbers on the Config Tab in the CTO Master File)", praErrors);

			containerMessagingData.PortOfLoading = "AUSYD";
			containerMessagingData.LoadTerminal1StopCode = "";

			praErrors = containerMessagingData.GetErrorText();
			AssertContains("Should have error", "Verified Method WTA is only valid for VICT CTO. (Registration Numbers on the Config Tab in the CTO Master File)", praErrors);
			AssertContains("Should have error", "Verified Method WTA is not valid for Load Port other than AUMEL.", praErrors);
		}

		public void TestCommodityError()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "XXX";
			commodity1.RH_Description = "XXX Cargo";
			commodity1.RH_IsShipping = true;

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "OOG";
			commodity2.RH_Description = "Out Of Gauge";
			commodity2.RH_IsShipping = true;

			Factory.Save();

			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.PortOfLoading = "AUMEL";
			containerMessagingData.LoadTerminal1StopCode = "VICTM";
			containerMessagingData.GrossWeightVerifiedType = PRAConstants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			AssertEquals("Precondition: should have no errors here", "", containerMessagingData.GetErrorText());

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.Commodity1StopCode = "XXX";
			var praErrors = containerMessagingData.GetErrorText();
			AssertEquals("Should have no errors here", "", praErrors);

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.Commodity1StopCode = "OOG";
			praErrors = containerMessagingData.GetErrorText();
			AssertContains("Should have error", "Verified Method WTA is not valid for Commodity OOG", praErrors);
		}

		public void TestOverhangOnNonOverhangContainerTypeWarning()
		{
			string warning = "This container is being sent as 'Over Size' which is not normal for this ISO Container Type";

			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.ISOContainerType = "40G0";

			containerMessagingData.OverhangHeightInCM = 1;
			AssertContains("Should have Oversize ISO Type Warning", warning, containerMessagingData.GetWarningText());

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.OverhangHeightInCM = 0;
			containerMessagingData.OverhangFrontInCM = 1;
			AssertContains("Should have Oversize ISO Type Warning", warning, containerMessagingData.GetWarningText());

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.OverhangFrontInCM = 0;
			containerMessagingData.OverhangBackInCM = 1;
			AssertContains("Should have Oversize ISO Type Warning", warning, containerMessagingData.GetWarningText());

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.OverhangBackInCM = 0;
			containerMessagingData.OverhangLeftInCM = 1;
			AssertContains("Should have Oversize ISO Type Warning", warning, containerMessagingData.GetWarningText());

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.OverhangLeftInCM = 0;
			containerMessagingData.OverhangRightInCM = 1;
			AssertContains("Should have Oversize ISO Type Warning", warning, containerMessagingData.GetWarningText());
		}

		public void TestISOCode2000Warning()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.ISOContainerType = "2000";

			string warning = "1-Stop have advised us that there are no containers of ISO Type 2000 or 20G0";
			Assert("Should have 2000 ISO Type Warning", containerMessagingData.GetWarningText().IndexOf(warning) != -1);
		}

		public void TestNullContainerThrowsNoExceptions()
		{
			ContainerMessagingData containerMessagingData = GetDataLayerPassingNullIntoConstructor();
			AssertEquals("containerMessagingData.AirVentSetting", 0, containerMessagingData.AirVentSetting);
			AssertEquals("containerMessagingData.AirVentSettingUnit", "", containerMessagingData.AirVentSettingUnit);
			AssertEquals("containerMessagingData.ShippingLineBookingReference", "", containerMessagingData.ShippingLineBookingReference);
			AssertEquals("containerMessagingData.CartageBookingReference", "", containerMessagingData.CartageBookingReference);
			AssertEquals("containerMessagingData.CartageCompanyABN", "", containerMessagingData.CartageCompanyABN);
			AssertEquals("containerMessagingData.Commodity1StopCode", "", containerMessagingData.Commodity1StopCode);
			AssertNotNull("containerMessagingData.Consignor", containerMessagingData.ConsignorName);
			AssertEquals("containerMessagingData.ContainerGrossWeight", 0m, containerMessagingData.ContainerGrossWeight);
			AssertEquals("containerMessagingData.ContainerNetWeight", 0m, containerMessagingData.ContainerNetWeight);
			AssertEquals("containerMessagingData.ContainerNumber", "", containerMessagingData.ContainerNumber);
			AssertEquals("containerMessagingData.ContainerTareWeight", 0m, containerMessagingData.ContainerTareWeight);
			AssertNotNull("containerMessagingData.DateTimeStringForMessage", containerMessagingData.DateTimeStringForMessage);
			AssertEquals("containerMessagingData.ECNorCRN", "", containerMessagingData.ECNorCRN);
			AssertEquals("containerMessagingData.FlatRackID", "", containerMessagingData.FlatRackID);
			AssertEquals("containerMessagingData.GoodsDescription", "", containerMessagingData.GoodsDescription);
			AssertEquals("containerMessagingData.HasTynes", false, containerMessagingData.HasTynes);
			AssertEquals("containerMessagingData.IsEmptyContainer", false, containerMessagingData.IsEmptyContainer);
			AssertEquals("containerMessagingData.ArrivingAtCTOByRail", false, containerMessagingData.ArrivingAtCTOByRail);
			AssertEquals("containerMessagingData.HumidityPercentage", 0, containerMessagingData.HumidityPercentage);
			AssertEquals("containerMessagingData.ISOContainerType", "", containerMessagingData.ISOContainerType);
			AssertEquals("containerMessagingData.LloydsNumber", "", containerMessagingData.LloydsNumber);
			AssertEquals("containerMessagingData.LoadTerminal1StopCode", "", containerMessagingData.LoadTerminal1StopCode);
			AssertEquals("containerMessagingData.MessageReference.Substring(3, 2)", "--", containerMessagingData.MessageReference.Substring(3, 2));
			AssertEquals("containerMessagingData.OverhangFrontInCM", 0, containerMessagingData.OverhangFrontInCM);
			AssertEquals("containerMessagingData.OverhangBackInCM", 0, containerMessagingData.OverhangBackInCM);
			AssertEquals("containerMessagingData.OverhangLeftInCM", 0, containerMessagingData.OverhangLeftInCM);
			AssertEquals("containerMessagingData.OverhangRightInCM", 0, containerMessagingData.OverhangRightInCM);
			AssertEquals("containerMessagingData.OverhangHeightInCM", 0, containerMessagingData.OverhangHeightInCM);
			AssertEquals("containerMessagingData.PortOfDischarge", "", containerMessagingData.PortOfDischarge);
			AssertEquals("containerMessagingData.PortOfFinalDischarge", "", containerMessagingData.PortOfFinalDischarge);
			AssertEquals("containerMessagingData.PortOfLoading", "", containerMessagingData.PortOfLoading);
			AssertEquals("containerMessagingData.ReeferGeneratorID", "", containerMessagingData.ReeferGeneratorID);
			AssertEquals("containerMessagingData.RoadDest1StopCode", "", containerMessagingData.RoadDest1StopCode);
			AssertEquals("containerMessagingData.RoadOrig1StopCode", "", containerMessagingData.RoadOrig1StopCode);
			AssertEquals("containerMessagingData.RoadScheduledArrival", ZDateTime.Empty, containerMessagingData.RoadScheduledArrival);
			AssertEquals("containerMessagingData.RoadScheduledDeparture", ZDateTime.Empty, containerMessagingData.RoadScheduledDeparture);
			AssertEquals("containerMessagingData.SealNumber", "", containerMessagingData.SealNumber);
			AssertEquals("containerMessagingData.SenderContactName", GlbStaff.CurrentUser.GS_FullName, containerMessagingData.SenderContactName);
			AssertEquals("containerMessagingData.SenderFax", "", containerMessagingData.SenderFax);
			AssertNotNull("containerMessagingData.SenderID", containerMessagingData.SenderID);
			AssertEquals("containerMessagingData.SenderPhone", "", containerMessagingData.SenderPhone);
			AssertEquals("containerMessagingData.ShippingLine1StopCode", "", containerMessagingData.ShippingLine1StopCode);
			AssertEquals("containerMessagingData.TemperatureSettingFormatted", "00.0", containerMessagingData.TemperatureSettingFormatted);
			AssertEquals("containerMessagingData.TerminalVBSBooking", "", containerMessagingData.TerminalVBSBooking);
			AssertEquals("containerMessagingData.TruckRegoNumber", "", containerMessagingData.TruckRegoNumber);
			AssertEquals("containerMessagingData.VesselName", "", containerMessagingData.VesselName);
			AssertEquals("containerMessagingData.Voyage", "", containerMessagingData.Voyage);
		}

		public void TestSettingAllValues()
		{
			ContainerMessagingData containerMessagingData = GetDataLayerPassingNullIntoConstructor();
			containerMessagingData.AirVentSetting = 12;
			AssertEquals(12, containerMessagingData.AirVentSetting);
			containerMessagingData.AirVentSettingUnit = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.AirVentSettingUnit);
			containerMessagingData.ShippingLineBookingReference = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.ShippingLineBookingReference);
			containerMessagingData.CartageBookingReference = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.CartageBookingReference);
			containerMessagingData.CartageCompanyABN = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.CartageCompanyABN);
			containerMessagingData.Commodity1StopCode = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.Commodity1StopCode);
			containerMessagingData.ConsignorName = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.ConsignorName);
			containerMessagingData.ContainerGrossWeight = 4000m;
			AssertEquals(4000m, containerMessagingData.ContainerGrossWeight);
			containerMessagingData.ContainerNetWeight = 2300m;
			AssertEquals(2300m, containerMessagingData.ContainerNetWeight);
			containerMessagingData.ContainerNumber = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.ContainerNumber);
			containerMessagingData.ContainerTareWeight = 1200m;
			AssertEquals(1200m, containerMessagingData.ContainerTareWeight);
			containerMessagingData.DateTimeStringForMessage = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.DateTimeStringForMessage);
			containerMessagingData.FlatRackID = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.FlatRackID);
			containerMessagingData.ECNorCRN = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.ECNorCRN);
			containerMessagingData.GoodsDescription = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.GoodsDescription);
			containerMessagingData.HasTynes = true;
			Assert(containerMessagingData.HasTynes);
			containerMessagingData.ArrivingAtCTOByRail = true;
			Assert(containerMessagingData.ArrivingAtCTOByRail);
			containerMessagingData.IsEmptyContainer = true;
			Assert(containerMessagingData.IsEmptyContainer);
			containerMessagingData.HumidityPercentage = 23;
			AssertEquals(23, containerMessagingData.HumidityPercentage);
			containerMessagingData.ISOContainerType = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.ISOContainerType);
			containerMessagingData.LloydsNumber = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.LloydsNumber);
			containerMessagingData.LoadTerminal1StopCode = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.LoadTerminal1StopCode);
			containerMessagingData.MessageReference = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.MessageReference);
			containerMessagingData.OverhangFrontInCM = 20;
			AssertEquals(20, containerMessagingData.OverhangFrontInCM);
			containerMessagingData.OverhangBackInCM = 22;
			AssertEquals(22, containerMessagingData.OverhangBackInCM);
			containerMessagingData.OverhangLeftInCM = 30;
			AssertEquals(30, containerMessagingData.OverhangLeftInCM);
			containerMessagingData.OverhangRightInCM = 33;
			AssertEquals(33, containerMessagingData.OverhangRightInCM);
			containerMessagingData.OverhangHeightInCM = 40;
			AssertEquals(40, containerMessagingData.OverhangHeightInCM);
			containerMessagingData.PortOfDischarge = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.PortOfDischarge);
			containerMessagingData.PortOfFinalDischarge = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.PortOfFinalDischarge);
			containerMessagingData.PortOfLoading = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.PortOfLoading);
			containerMessagingData.ReeferGeneratorID = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.ReeferGeneratorID);
			containerMessagingData.RoadDest1StopCode = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.RoadDest1StopCode);
			containerMessagingData.RoadOrig1StopCode = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.RoadOrig1StopCode);
			ZDateTime testDate = new ZDateTime(2004, 1, 1, 1, 1, 1);
			containerMessagingData.RoadScheduledArrival = testDate;
			AssertEquals(testDate, containerMessagingData.RoadScheduledArrival);
			containerMessagingData.RoadScheduledDeparture = testDate;
			AssertEquals(testDate, containerMessagingData.RoadScheduledDeparture);
			containerMessagingData.SealNumber = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.SealNumber);
			containerMessagingData.SenderContactName = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.SenderContactName);
			containerMessagingData.SenderFax = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.SenderFax);
			containerMessagingData.SenderID = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.SenderID);
			containerMessagingData.SenderPhone = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.SenderPhone);
			containerMessagingData.ShippingLine1StopCode = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.ShippingLine1StopCode);
			containerMessagingData.TerminalVBSBooking = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.TerminalVBSBooking);
			containerMessagingData.TemperatureSettingFormatted = "0";
			AssertEquals("00.0", containerMessagingData.TemperatureSettingFormatted);
			containerMessagingData.TruckRegoNumber = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.TruckRegoNumber);
			containerMessagingData.VesselName = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.VesselName);
			containerMessagingData.Voyage = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.Voyage);
			containerMessagingData.GrossWeightVerifiedType = "ZZZ";
			AssertEquals("ZZZ", containerMessagingData.GrossWeightVerifiedType);
			containerMessagingData.GrossWeightVerifiedDeclarantContact = "ZZZ;ZZZ";
			AssertEquals("ZZZ;ZZZ", containerMessagingData.GrossWeightVerifiedDeclarantContact);
		}

		public void TestGetMissingDataErrorText()
		{
			ContainerMessagingData containerMessagingData = GetDataLayerPassingNullIntoConstructor();
			string result = "\r\n" + containerMessagingData.GetErrorText();
			AssertMultilineASCIIEquals("Should have all warnings here", ExpectedPRAErrors, result);
		}

		public void TestInvalidOneStopCodeFails()
		{
			string missing = "Departure CTO 1-Stop Code.";
			string invalid = "Departure CTO 1-Stop Code is Invalid.";

			ContainerMessagingData containerMessagingData = GetDataLayerPassingNullIntoConstructor();

			containerMessagingData.LoadTerminal1StopCode = "";
			Assert("Error Message Not Right", containerMessagingData.GetErrorText().IndexOf(missing) != -1);

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.LoadTerminal1StopCode = "asas";
			Assert("Error Message Not Right", containerMessagingData.GetErrorText().IndexOf(invalid) != -1);

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.LoadTerminal1StopCode = "ASLPB";
			Assert("Error Message Not Right", containerMessagingData.GetErrorText().IndexOf(missing) == -1 && containerMessagingData.GetErrorText().IndexOf(invalid) == -1);
		}

		public void TestEmptyContainerCanSendWithNoCAN()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			containerMessagingData.ECNorCRN = "";
			containerMessagingData.IsEmptyContainer = false;
			AssertEquals("Precondition: Should bork about CAN when container is not empty", ECNorCRNErrorText, containerMessagingData.GetErrorText());

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.Commodity1StopCode = "MT"; // MT = Empty Container
			AssertEquals("Should have no Errors here", "", containerMessagingData.GetErrorText());
			AssertEquals("Should have no Warnings here", "", containerMessagingData.GetWarningText());

			containerMessagingData.ResetCacheForTesting();
			containerMessagingData.Commodity1StopCode = "MTHZ"; // MTHZ = Empty Container (but still hazardous)
			AssertEquals("Should have no Errors here", "", containerMessagingData.GetErrorText());
			AssertEquals("Should have no Wardings here", "", containerMessagingData.GetWarningText());
		}

		public void TestFullContainerSendWithVerifiedMethodNON()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			AssertEquals("Precondition: Should have no error", "", containerMessagingData.GetErrorText());

			containerMessagingData.IsEmptyContainer = true;
			containerMessagingData.GrossWeightVerifiedType = "";
			AssertEquals("Should have no errors", "", containerMessagingData.GetErrorText());

			containerMessagingData.IsEmptyContainer = false;
			AssertEquals("Should have error", "Container Verified Weight Method must be CNT or PKG. (VGM Tab on the Containers Tab)\r\n", containerMessagingData.GetErrorText());
		}

		public void TestGetGrossWeightVerifiedDate()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			AssertEquals("Precondition: Should have no errors", "", containerMessagingData.GetErrorText());

			containerMessagingData.GrossWeightVerifiedDateTime = ZDateTime.Empty;
			AssertEquals("Should have error", "Container Verified Weight Date must be provided. (VGM Tab on the Containers Tab)\r\n", containerMessagingData.GetErrorText());

			containerMessagingData.GrossWeightVerifiedDateTime = new ZDateTime(2015, 5, 1);
			AssertEquals("Should have no errors", "", containerMessagingData.GetErrorText());
		}

		public void TestGetGrossWeightVerifiedDeclarantSignature()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			AssertEquals("Should have no error", "", containerMessagingData.GetErrorText());

			containerMessagingData.GrossWeightVerifiedDeclarantSignature = "";
			AssertEquals("Should have error", "Declarant contact must be provided. (Staff Details in the Staff Master File)\r\n", containerMessagingData.GetErrorText());
		}

		public void TestGetGrossWeightVerifiedDeclarantContact()
		{
			ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
			AssertEquals("Should have no error", "", containerMessagingData.GetErrorText());

			containerMessagingData.GrossWeightVerifiedDeclarantSignature = "";
			AssertEquals("Should have error", "Declarant contact must be provided. (Staff Details in the Staff Master File)\r\n", containerMessagingData.GetErrorText());
		}

		public void TestGetGrossWeightVerifiedDeclarantCompanyName()
		{
			var currentCompanyProxyName = GlbCompany.CurrentCompany.OrgProxy.OH_FullName;
			var currentBranchProxyName = GlbBranch.CurrentBranch.OrgProxy.OH_FullName;

			try
			{
				ContainerMessagingData containerMessagingData = GetMinimumValidDataLayer();
				AssertEquals("Should have no error", "", containerMessagingData.GetErrorText());

				GlbCompany.CurrentCompany.OrgProxy.OH_FullName = string.Empty;
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = string.Empty;
				containerMessagingData.GrossWeightDeclarantCompanyName = "";

				AssertEquals("Should have error", "Declarant company name must be provided. (Staff Details in the Staff Master File)\r\n", containerMessagingData.GetErrorText());
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_FullName = currentCompanyProxyName;
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = currentBranchProxyName;
			}
		}

		public void TestIHaveTheDateFormatRight()
		{
			AssertEquals("20050101192345", new ZDateTime(2005, 1, 1, 19, 23, 45).ToString("yyyyMMddHHmmss"));
		}

		public void TestSenderCompanyName()
		{
			var currentCompanyProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var currentBranchProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			try
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				var containerMessagingData = GetDataLayerPassingNullIntoConstructor();
				AssertEquals(string.Empty, containerMessagingData.SenderCompanyName);

				var companyOrgProxy = GlbCompany.CurrentCompany.Factory.New<OrgHeader>();
				companyOrgProxy.OH_Code = "COM";
				companyOrgProxy.OH_FullName = "Company OrgProxy";
				companyOrgProxy.MainAddress.OA_Address1 = "Address 1";
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
				GlbCompany.CurrentCompany.Factory.Save();

				containerMessagingData = GetDataLayerPassingNullIntoConstructor();
				AssertEquals("Company OrgProxy", containerMessagingData.SenderCompanyName);

				var branchOrgProxy = GlbBranch.CurrentBranch.Factory.New<OrgHeader>();
				branchOrgProxy.OH_Code = "BRH";
				branchOrgProxy.OH_FullName = "Branch OrgProxy";
				branchOrgProxy.MainAddress.OA_Address1 = "Address 1";
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
				GlbBranch.CurrentBranch.Factory.Save();

				containerMessagingData = GetDataLayerPassingNullIntoConstructor();
				AssertEquals("Company OrgProxy", containerMessagingData.SenderCompanyName);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = currentCompanyProxy;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = currentBranchProxy;
			}
		}

		public void TestContainerMessagingIsFilledIn()
		{
			ContainerMessagingData containerMessagingData = GetDataLayerPassingEmptyContainerIntoConstructor();
			EDIMessage message = containerMessagingData.GetEDIMessage();
			AssertNotNull("GetEDIMessage() should return a real object", message);
			AssertEquals("Type of GetEDIMessage()", typeof(PRAMessage).ToString(), message.GetType().ToString());
			AssertNotNull("GetDataLayerPassingEmptyContainerIntoConstructor() should always fill in the ContainerMessaging property on the concrete TestCase.", containerMessaging);
			AssertEquals("Parent Record Type", containerMessaging.TableName, message.EM_LinkTable);
		}

		public void TestLogUsagePerMessage()
		{
			var logFilter = new ZDBOnlyQuery(typeof(StmActivityLog));
			logFilter.AddToFilter(StmActivityLogSchema.S7_FormCaption, Env.Licence.PRAMessagingPerTransaction.Name);

			var beforSavingLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), logFilter);

			var dataLayer = GetReadyToSaveDataLayer();
			dataLayer.Save();

			var afterSavingLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), logFilter);
			AssertEquals(1, afterSavingLogCount - beforSavingLogCount);
		}

		public abstract void TestContainerIsWaitingForResponse();

		public abstract void TestGetMessageReferenceIsRightForImplementationInThisModule();

		public abstract void TestFieldMappingsAfterPassingContainerIntoConstructor();

		protected abstract string ExpectedPRAErrors { get; }

		protected abstract string ECNorCRNErrorText { get; }

		protected abstract ContainerMessagingData GetDataLayerPassingNullIntoConstructor();

		protected abstract ContainerMessagingData GetDataLayerPassingEmptyContainerIntoConstructor();

		protected abstract ContainerMessagingData GetReadyToSaveDataLayer();

		protected IPRAContainerMessaging containerMessaging;

		protected ContainerMessagingData GetMinimumValidDataLayer()
		{
			ContainerMessagingData containerMessagingData = GetDataLayerPassingNullIntoConstructor();

			containerMessagingData.MessageReference = "001";
			containerMessagingData.DateTimeStringForMessage = "20040402092831";
			containerMessagingData.SenderID = "EXPORTER";
			containerMessagingData.ConsignorName = "CONSIGNOR";
			containerMessagingData.ShippingLineBookingReference = "BOOKING123";
			containerMessagingData.ECNorCRN = "CUSTOMS ECN";
			containerMessagingData.ShippingLine1StopCode = "ANL";
			containerMessagingData.VesselName = "MSC MARTINA";
			containerMessagingData.Voyage = "123N";
			containerMessagingData.LloydsNumber = "9060637";
			containerMessagingData.PortOfLoading = "AUSYD";
			containerMessagingData.LoadTerminal1StopCode = "ASLPB";
			containerMessagingData.PortOfFinalDischarge = "IDJKT";
			containerMessagingData.ContainerNumber = "TESU1234567";
			containerMessagingData.ISOContainerType = "22G0";
			containerMessagingData.Commodity1StopCode = "GENL";
			containerMessagingData.ContainerGrossWeight = 11600m;
			containerMessagingData.SealNumber = "289184";
			containerMessagingData.GrossWeightVerifiedType = PRAConstants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			containerMessagingData.GrossWeightVerifiedDateTime = new ZDateTime(2016, 5, 1);
			containerMessagingData.GrossWeightVerifiedDeclarantContact = "DEVELOPER;DEMO ORGANISATION;(03) 2222 2222";
			containerMessagingData.GrossWeightVerifiedDeclarantSignature = "DEVELOPER";
			containerMessagingData.GrossWeightDeclarantCompanyName = "DEMO ORGANISATION";

			return containerMessagingData;
		}
	}
}
