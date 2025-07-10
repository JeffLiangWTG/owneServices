using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CustomsDataLayerTest : ContainerMessagingDataAbstractTest
	{
		public override void TestContainerIsWaitingForResponse()
		{
			CusContainer container = Factory.New<CusContainer>();
			CustomsDataLayer dataLayer = new CustomsDataLayer(container);
			AssertEquals(false, dataLayer.ContainerIsWaitingForResponse);

			EDIMessage message1 = container.Messages.AddNew();
			message1.EM_ReceiveTransmit = "TRX";
			AssertEquals(true, dataLayer.ContainerIsWaitingForResponse);

			EDIMessage message2 = container.Messages.AddNew();
			message2.EM_ReceiveTransmit = "RCV";
			AssertEquals(false, dataLayer.ContainerIsWaitingForResponse);
		}

		public override void TestFieldMappingsAfterPassingContainerIntoConstructor()
		{
			ZString saveCurrentBranchName = GlbBranch.CurrentBranch.OrgProxy.OH_FullName;
			ZString saveCurrentUserFullName = GlbStaff.CurrentUser.GS_FullName;
			ZString saveCurrentUserWorkPhone = GlbStaff.CurrentUser.GS_WorkPhone;
			ZString saveCurrentUserEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			try
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "CONZIGNOR GRIMO";
				GlbStaff.CurrentUser.GS_FullName = "Developer";
				GlbStaff.CurrentUser.GS_WorkPhone = "(03) 2222 3333";
				GlbStaff.CurrentUser.GS_EmailAddress = "developer@edi.com.au";

				OrgHeader shippingLine = Factory.New<OrgHeader>();
				OrgCusCode cusCode1 = Factory.New<OrgCusCode>();
				cusCode1.OK_OH = shippingLine.PK;
				cusCode1.OK_CustomsRegNo = "BBBB2";
				cusCode1.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				cusCode1.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;

				OrgAddress cTOAddress = Factory.New<OrgAddress>();
				OrgHeader cTO = Factory.New<OrgHeader>();
				cTOAddress.OA_OH = cTO.PK;
				OrgCusCode cusCode2 = Factory.New<OrgCusCode>();
				cusCode2.OK_CustomsRegNo = "YYYY2";
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				cusCode2.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
				cusCode2.OK_OA_PremisesAddress = cTOAddress.PK;
				cusCode2.OK_OH = cTO.PK;

				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_DeclarationReference = "Z00000001";
				declaration.JE_RL_NKPortOfLoading = "AUSYD";
				declaration.JE_RL_NKPortOfArrival = "USLAX";
				declaration.JE_RL_NKFinalDestination = "USABQ";
				declaration.JE_VoyageFlightNo = "V102";
				declaration.JE_OH_ShippingLine = shippingLine.PK;
				declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;
				declaration.DeclarationNumber = "12341234";
				OrgHeader cartageCompany = Factory.New<OrgHeader>();
				cartageCompany.PrimaryRegistrationNumber.Number = "ZZZZ2";
				declaration.DeliveryOrPickupCartageCoPK = cartageCompany.PK;

				RefVessel refVessel = Factory.New<RefVessel>();
				refVessel.RV_LloydsNumber = "0982432";
				refVessel.RV_Code = "VESSELNAME";
				declaration.JE_VesselName = refVessel.RV_Code;
				AssertEquals("declaration.JE_VesselName", "VESSELNAME", declaration.JE_VesselName);

				CusContainer container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "TEST!@#";
				container.CO_Seal = "HARPIC";
				container.CO_Weight = 555m;

				RefContainer refContainer = Factory.New<RefContainer>();
				container.CO_RC = refContainer.PK;
				refContainer.RC_TareWeight = 2000m;
				refContainer.RC_HasTynes = true;
				refContainer.RC_ISOType = "1099";

				container.AirVentFlow = 100m;
				container.AirVentFlowRateUnit = "MQH";

				container.DepartureDeliveryByRail = true;
				container.HumidityPercent = 33;
				container.TareWeight = 2000m;

				container.TotalLength = Core.Constants.Length.Convert(11m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.TotalWidth = Core.Constants.Length.Convert(22m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.TotalHeight = Core.Constants.Length.Convert(33m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.JobContainer.JC_OverhangBack = Core.Constants.Length.Convert(3m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.JobContainer.JC_OverhangRight = Core.Constants.Length.Convert(4m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.RefrigGeneratorID = "R101";
				container.SetPointTemp = 12.0m;
				container.IsControlledAtmosphere = true;
				container.DepartureSlotReference = "VBSBOOK";

				RefCommodityCode code = Factory.New<RefCommodityCode>();
				code.RH_Description = "YOUGUESS";
				code.RH_Code = "XXXX";
				container.RH_NKContainerCommodityCode = code.RH_Code;

				CustomsDataLayer dataLayer = new CustomsDataLayer(container);

				AssertEquals("dataLayer.ContainerNumber", "TEST!@#", dataLayer.ContainerNumber);
				AssertEquals("dataLayer.ECNorCRN", "12341234", dataLayer.ECNorCRN);
				AssertEquals("dataLayer.AirVentSetting", 100, dataLayer.AirVentSetting);
				AssertEquals("dataLayer.AirVentSetting", "MQH", dataLayer.AirVentSettingUnit);
				AssertEquals("dataLayer.ArrivingAtCTOByRail", true, dataLayer.ArrivingAtCTOByRail);
				AssertEquals("dataLayer.Consignor", "CONZIGNOR GRIMO", dataLayer.ConsignorName);
				AssertEquals("dataLayer.ContainerGrossWeight", 2555m, dataLayer.ContainerGrossWeight);
				AssertEquals("dataLayer.ContainerTareWeight", 2000m, dataLayer.ContainerTareWeight);
				AssertEquals("dataLayer.ContainerNetWeight", 555m, dataLayer.ContainerNetWeight);
				AssertEquals("dataLayer.Commodity1StopCode", "XXXX", dataLayer.Commodity1StopCode);
				AssertEquals("dataLayer.GoodsDescription", "YOUGUESS", dataLayer.GoodsDescription);
				AssertEquals("dataLayer.HasTynes", true, dataLayer.HasTynes);
				AssertEquals("dataLayer.HumidityPercentage", 33, dataLayer.HumidityPercentage);
				AssertEquals("dataLayer.ISOContainerType", "1099", dataLayer.ISOContainerType);
				AssertEquals("dataLayer.LloydsNumber", "0982432", dataLayer.LloydsNumber);
				AssertEquals("dataLayer.MessageReference", "CUS-Z00000001-TEST!@#", dataLayer.MessageReference);
				AssertEquals("DataLayer.OverhangFrontInCM", 8, dataLayer.OverhangFrontInCM);
				AssertEquals("DataLayer.OverhangBackInCM", 3, dataLayer.OverhangBackInCM);
				AssertEquals("DataLayer.OverhangLeftInCM", 18, dataLayer.OverhangLeftInCM);
				AssertEquals("DataLayer.OverhangRightInCM", 4, dataLayer.OverhangRightInCM);
				AssertEquals("dataLayer.OverhangHeightInCM", 33, dataLayer.OverhangHeightInCM);
				AssertEquals("dataLayer.PortOfDischarge", "USLAX", dataLayer.PortOfDischarge);
				AssertEquals("dataLayer.PortOfFinalDischarge", "USABQ", dataLayer.PortOfFinalDischarge);
				AssertEquals("dataLayer.PortOfLoading", "AUSYD", dataLayer.PortOfLoading);
				AssertEquals("dataLayer.ReeferGeneratorID", "R101", dataLayer.ReeferGeneratorID);
				AssertEquals("dataLayer.SealNumber", "HARPIC", dataLayer.SealNumber);
				AssertEquals("dataLayer.SenderContactName", GlbStaff.CurrentUser.GS_FullName, dataLayer.SenderContactName);
				AssertEquals("dataLayer.SenderFax", GlbStaff.CurrentUser.GS_FaxNum, dataLayer.SenderFax);
				AssertEquals("dataLayer.SenderPhone", GlbStaff.CurrentUser.GS_WorkPhone, dataLayer.SenderPhone);
				AssertEquals("dataLayer.TemperatureSetting", "12.0", dataLayer.TemperatureSettingFormatted);
				AssertEquals("dataLayer.TerminalVBSBooking", "VBSBOOK", dataLayer.TerminalVBSBooking);
				AssertEquals("dataLayer.VesselName", "VESSELNAME", dataLayer.VesselName);
				AssertEquals("dataLayer.Voyage", "V102", dataLayer.Voyage);
				AssertEquals("dataLayer.ShippingLine1StopCode", "BBBB2", dataLayer.ShippingLine1StopCode);
				AssertEquals("dataLayer.CartageCompanyABN", "ZZZZ2", dataLayer.CartageCompanyABN);
				AssertEquals("dataLayer.LoadTerminal1StopCode", "YYYY2", dataLayer.LoadTerminal1StopCode);
				AssertEquals("dataLayer.GrossWeightVerifiedDeclarantSignature", GlbStaff.CurrentUser.GS_FullName, dataLayer.GrossWeightVerifiedDeclarantSignature);
				AssertEquals("dataLayer.GrossWeightVerifiedDeclarantContact", "Developer;CONZIGNOR GRIMO;(03) 2222 3333;developer@edi.com.au", dataLayer.GrossWeightVerifiedDeclarantContact);

				//UnMapped Fields
				AssertEquals("dataLayer.ShippingLineBookingReference", "", dataLayer.ShippingLineBookingReference);
				AssertEquals("dataLayer.FlatRackID", "", dataLayer.FlatRackID);
				AssertEquals("dataLayer.TruckRegoNumber", "", dataLayer.TruckRegoNumber);
				AssertEquals("dataLayer.RoadDest1StopCode", "", dataLayer.RoadDest1StopCode);
				AssertEquals("dataLayer.RoadOrig1StopCode", "", dataLayer.RoadOrig1StopCode);
				AssertEquals("dataLayer.CartageBookingReference", "", dataLayer.CartageBookingReference);
				AssertEquals("dataLayer.RoadScheduledArrival", ZDateTime.Empty, dataLayer.RoadScheduledArrival);
				AssertEquals("dataLayer.RoadScheduledDeparture", ZDateTime.Empty, dataLayer.RoadScheduledDeparture);
			}
			finally
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = saveCurrentBranchName;
				GlbStaff.CurrentUser.GS_FullName = saveCurrentUserFullName;
				GlbStaff.CurrentUser.GS_EmailAddress = saveCurrentUserEmailAddress;
				GlbStaff.CurrentUser.GS_WorkPhone = saveCurrentUserWorkPhone;
			}
		}

		public void TestCartageCompany()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			OrgHeader cartageCompany = Factory.New<OrgHeader>();
			cartageCompany.PrimaryRegistrationNumber.Number = "SuperHappyABN";
			declaration.DeliveryOrPickupCartageCoPK = cartageCompany.PK;
			CustomsDataLayer dataLayer = new CustomsDataLayer(container);
			AssertEquals("Cartage company should be loaded", "SuperHappyABN", dataLayer.CartageCompanyABN);
		}

		public void TestSetPointTemperatureWithDecimalPlaces()
		{
			CusContainer container = Factory.New<CusContainer>();
			container.SetPointTemp = -15.4m;
			CustomsDataLayer data = new CustomsDataLayer(container);
			AssertEquals("Temperature with decimal point", "-15.4", data.TemperatureSettingFormatted);
		}

		public void TestSetPointTemperatureWithNoDecimalPlaces()
		{
			CusContainer container = Factory.New<CusContainer>();
			container.SetPointTemp = 11m;
			CustomsDataLayer data1 = new CustomsDataLayer(container);
			AssertEquals("Temperature with no decimal point", "11.0", data1.TemperatureSettingFormatted);
			CustomsDataLayer data2 = new CustomsDataLayer(container);
			container.SetPointTemp = 4m;
			AssertEquals("Temperature with no decimal point (1 long)", "04.0", data2.TemperatureSettingFormatted);
		}

		public void TestIsTempControlled()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			container.IsControlledAtmosphere = true;
			CustomsDataLayer dataLayer = new CustomsDataLayer(container);
			AssertEquals("With no container type selected, should be false", false, dataLayer.IsTempControlled);

			CustomsDataLayer dataLayer1 = new CustomsDataLayer(container);
			RefContainer ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.CO_RC = ref20GP.PK;
			AssertEquals("With a non reefer container, it should be false", false, dataLayer1.IsTempControlled);

			CustomsDataLayer dataLayer2 = new CustomsDataLayer(container);
			RefContainer ref20RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			ref20RE.RC_ContainerType = Enterprise.Core.Constants.ContainerTypes.Refrigerated;
			container.CO_RC = ref20RE.PK;
			AssertEquals("A good container is a reefer container", true, dataLayer2.IsTempControlled);
		}

		public override void TestGetMessageReferenceIsRightForImplementationInThisModule()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			CustomsDataLayer dataLayer = new CustomsDataLayer(container);
			declaration.JE_DeclarationReference = "123456";
			container.CO_ContainerNumber = "OOCL1234567";
			AssertEquals("Message Reference should be returned in specified format", "CUS-123456-OOCL1234567", dataLayer.MessageReference);
		}

		public void TestLoadFromNewCustomsContainer()
		{
			DangerousGoodsCollection collection = new DangerousGoodsCollection();
			CusContainer container = Factory.New<CusContainer>();
			CustomsDataLayer dataLayer = new CustomsDataLayer(container);
			dataLayer.LoadFromContainer(collection);
			AssertEquals(0, collection.Count);
		}

		public void TestLoad2LinesFromRealCustomsContainer()
		{
			var collection = new DangerousGoodsCollection();
			AssertEquals("Pre-Condition", 0, collection.Count);

			var container = Factory.New<CusContainer>();

			var pivot1 = container.InvoiceLinePivotCollection.AddNew();
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			pivot1.C2_JI = invoiceLine1.PK;

			var subs1 = Factory.New<UNDGSubstance>();
			subs1.DG_UNNO = "123";
			subs1.DG_Variant = "a";
			subs1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			invoiceLine1.UNDGs.AddNew().LinkDefault(subs1);

			var pivot2 = container.InvoiceLinePivotCollection.AddNew();
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			pivot2.C2_JI = invoiceLine2.PK;

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "123";
			subs2.DG_Variant = "b";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			invoiceLine2.UNDGs.AddNew().LinkDefault(subs2);

			var dataLayer = new CustomsDataLayer(container);
			dataLayer.LoadFromContainer(collection);
			AssertEquals("Should only be 2 InvoiceLines with DG on them", 2, collection.Count);
		}

		public void TestLoadFromRealCustomsContainer()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Class = "111";
			subs.DG_FlashPoint = "23";
			subs.DG_PG = "II";
			subs.DG_PSN = "PUPPY DOGS TAILS";
			subs.DG_UNNO = "2120";
			subs.DG_Variant = "2";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var collection = new DangerousGoodsCollection();
			AssertEquals("Pre-Condition", 0, collection.Count);

			var container = Factory.New<CusContainer>();

			var pivot = container.InvoiceLinePivotCollection.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			pivot.C2_JI = invoiceLine.PK;
			pivot.C2_CO = container.PK;

			var dgItem = invoiceLine.UNDGs.AddNew();
			dgItem.LinkDefault(subs);

			var uNDGContact = Factory.New<OrgContact>();
			dgItem.DI_OC_DGContact = uNDGContact.PK;

			uNDGContact.OC_ContactName = "Roger Roger";
			uNDGContact.OC_Phone = "1234567890";
			uNDGContact.OC_Fax = "0987654321";
			uNDGContact.OC_Email = "rogerroger@roger.com";

			invoiceLine.JI_Weight = 30m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Pounds;
			dgItem.DI_DGFlashPoint = 33.3m;

			var dataLayer = new CustomsDataLayer(container);
			dataLayer.LoadFromContainer(collection);

			AssertEquals("Count of ContainerMessagingDangerousGoods in Collection", 1, collection.Count);

			var goods = collection[0];

			AssertEquals("Goods.IMDGClass", dgItem.Substance.DG_Class, goods.IMDGClass);
			AssertEquals("Goods.IMDGCodePage", "", goods.IMDGCodePage);
			AssertEquals("Goods.IMDGCodeVersion", dgItem.Substance.DG_Variant, goods.IMDGCodeVersion);
			AssertEquals("Goods.UNDGNumber", dgItem.Substance.DG_UNNO, goods.UNDGNumber);
			AssertEquals("Goods.FlashpointTemperatureInCelcius", TemperatureFormatter.FormatTemperatureString(dgItem.DI_DGFlashPoint.ToString()), goods.FlashpointTemperatureInCelcius);
			AssertEquals("Goods.PackingGroup", dgItem.Substance.DG_PG, goods.PackingGroup);
			AssertEquals("Goods.TechnicalName", dgItem.Substance.DG_PSN, goods.TechnicalName);
			AssertEquals("Goods.Weight", Core.Constants.Weight.Convert(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ, Core.Constants.Weight.Kilograms), goods.Weight);

			AssertEquals("Goods.ContactName", uNDGContact.OC_ContactName, goods.ContactName);
			AssertEquals("Goods.ContactPhoneNumber", uNDGContact.OC_Phone, goods.ContactPhoneNumber);
			AssertEquals("Goods.ContactFaxNumber", uNDGContact.OC_Fax, goods.ContactFaxNumber);
			AssertEquals("Goods.ContactEmailAddress", uNDGContact.OC_Email, goods.ContactEmailAddress);
		}

		public void TestLoadFromRealCustomsContainer_IsCombustibleFlashPoint()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "111";
				subs.DG_FlashPoint = "23";
				subs.DG_UNNO = "2120";
				subs.DG_Variant = "2";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

				var collection = new DangerousGoodsCollection();
				AssertEquals("Pre-Condition", 0, collection.Count);

				var container = Factory.New<CusContainer>();
				var pivot = container.InvoiceLinePivotCollection.AddNew();
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				pivot.C2_JI = invoiceLine.PK;
				pivot.C2_CO = container.PK;

				var dgItem = invoiceLine.UNDGs.AddNew();
				dgItem.LinkDefault(subs);
				dgItem.DI_DGFlashPoint = 33.3m;
				dgItem.DI_IsCombustible = false;

				var dataLayer1 = new CustomsDataLayer(container);
				dataLayer1.LoadFromContainer(collection);
				AssertEquals("Goods.FlashpointTemperatureInCelcius", ZString.Empty, collection[0].FlashpointTemperatureInCelcius);

				dgItem.DI_IsCombustible = true;
				var dataLayer2 = new CustomsDataLayer(container);
				dataLayer2.LoadFromContainer(collection);
				AssertEquals("Goods.FlashpointTemperatureInCelcius", "33.3", collection[1].FlashpointTemperatureInCelcius);
			}
		}

		protected override ContainerMessagingData GetDataLayerPassingNullIntoConstructor() => new CustomsDataLayer(Factory.New<CusContainer>());

		protected override string ExpectedPRAErrors
		{
			get
			{
				return @"
Shipping Line Booking Reference. (Export Process Tab on the Containers Tab)
Entry Number. (Against Declaration)
Carrier 1-Stop Code. (Registration Numbers on the Config Tab in the Carrier Master File)
Vessel Name. (Declaration Tab)
Voyage. (Declaration Tab)
Lloyds Number. (Vessel Master File for selected vessel on Declaration Tab)
Port Of Loading. (Declaration Tab)
Departure CTO 1-Stop Code. (Registration Numbers on the Config Tab in the CTO Master File)
Port Of Discharge. (Declaration Tab)
Container Number. (Containers Tab in the Grid)
ISO Container Type. (Containers Tab in the Grid)
Commodity 1-Stop Code. (Containers Tab in the Grid)
Container Gross Weight. (Containers Tab in the Grid)
Seal Number Not Entered. (Containers Tab in the Grid)
Container Verified Weight Method must be CNT or PKG. (VGM Tab on the Containers Tab)
";
			}
		}

		protected override string ECNorCRNErrorText => "Entry Number. (Against Declaration)\r\n";

		protected override ContainerMessagingData GetDataLayerPassingEmptyContainerIntoConstructor()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			containerMessaging = container;
			return new CustomsDataLayer(container);
		}

		protected override ContainerMessagingData GetReadyToSaveDataLayer() => GetDataLayerPassingEmptyContainerIntoConstructor();
	}
}
