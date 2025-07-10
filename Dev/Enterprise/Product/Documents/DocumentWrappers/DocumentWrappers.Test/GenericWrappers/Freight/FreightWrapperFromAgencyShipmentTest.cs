using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromAgencyShipment))]
	sealed class FreightWrapperFromAgencyShipmentTest : FreightWrapperTest
	{
		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			return ((AgencyShipment)parent).BookedContainers.AddNew();
		}

		public void TestExportReceivingCTOAddress()
		{
			#region Setup

			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			JobSailing sailing = Factory.New<JobSailing>();
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "HKHKG";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			agencyShipment.JS_JX = sailing.PK;

			#endregion

			FreightWrapperFromAgencyShipment wrapper = new FreightWrapperFromAgencyShipment(agencyShipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			OrgAddress originDepartureCTOAddress = Factory.New<OrgAddress>();
			originDepartureCTOAddress.OA_Address1 = "ORIGIN DEPARTURECTO ADDRESS";
			originDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			origin.JA_OA_DepartureCTOAddress = originDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromAgencyShipment(agencyShipment, Factory);
			AssertEquals("Should return AddressWrapper with AgencyShipmentBO.Sailing.Origin.DepartureCTOAddress", "ORIGIN DEPARTURECTO ADDRESS\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
		}

		public void TestExportReceivalAddress()
		{
			#region Setup

			AgencyShipment agencyShipment = Factory.New<AgencyShipment>();
			JobSailing sailing = Factory.New<JobSailing>();
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "HKHKG";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			agencyShipment.JS_JX = sailing.PK;

			OrgAddress originDepartureCTOAddress = Factory.New<OrgAddress>();
			originDepartureCTOAddress.OA_Address1 = "ORIGIN DEPARTURECTO ADDRESS";
			originDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			origin.JA_OA_DepartureCTOAddress = originDepartureCTOAddress.PK;

			#endregion

			agencyShipment.JS_PackingMode = Constants.ContainerModes.LCL;

			FreightWrapperFromAgencyShipment wrapper = new FreightWrapperFromAgencyShipment(agencyShipment, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			agencyShipment.JS_PackingMode = Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromAgencyShipment(agencyShipment, Factory);
			AssertEquals("Should return AddressWrapper with AgencyShipmentBO.Sailing.Origin.DepartureCTOAddress", "ORIGIN DEPARTURECTO ADDRESS\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("wrapper.GoodsDescription", ZString.Empty, wrapper.GoodsDescription);

			string shortGoodsDescription = "I LOVE CHIPS WITH SAUCE";
			shipment.JS_GoodsDescription = shortGoodsDescription;
			AssertEquals("wrapper.GoodsDescription", shortGoodsDescription, wrapper.GoodsDescription);

			string longGoodsDescription = @"I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, 
I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I REALLY DO!!!!!";
			shipment.DetailedGoodsDescriptionNoteText = longGoodsDescription;
			AssertEquals("wrapper.GoodsDescription", shortGoodsDescription, wrapper.GoodsDescription);
		}

		public void TestMasterBill()
		{
			BillOfLading billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_HouseBill = "BOL 1";

			FreightWrapperFromAgencyShipment wrapper = new FreightWrapperFromAgencyShipment(billOfLading, Factory);
			AssertEquals("wrapper.MasterBill", "BOL 1", wrapper.MasterBill);

			AgencyBooking booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_HouseBill = "BOOK 1";

			wrapper = new FreightWrapperFromAgencyShipment(booking, Factory);
			AssertEquals("wrapper.MasterBill", ZString.Empty, wrapper.MasterBill);
		}

		public void TestGetExchangeRates()
		{
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "HKHKG";

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			VoyageExRate exRate1 = voyage.ExRates.AddNew();
			exRate1.E8_RX_NKExCurrency = "USD";
			exRate1.E8_VoyageExchangeRate = 5.0m;

			VoyageExRate exRate2 = voyage.ExRates.AddNew();
			exRate2.E8_RX_NKExCurrency = "UAH";
			exRate2.E8_VoyageExchangeRate = 7.0m;

			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			FreightWrapperFromAgencyShipment fullWrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			AssertEquals("fullWrapper.ExchangeRates.Count", 2, fullWrapper.ExchangeRates.Count);
			AssertEquals("fullWrapper.ExchangeRates[0].Currency.Code", "USD", fullWrapper.ExchangeRates[0].Currency.Code);
			AssertEquals("fullWrapper.ExchangeRates[0].SellRate", 5.0m, fullWrapper.ExchangeRates[0].SellRate);
			AssertEquals("fullWrapper.ExchangeRates[1].Currency.Code", "UAH", fullWrapper.ExchangeRates[1].Currency.Code);
			AssertEquals("fullWrapper.ExchangeRates[1].SellRate", 7.0m, fullWrapper.ExchangeRates[1].SellRate);
		}

		public void TestGetExchangeRates_DifferentCompanies()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = MasterFilesTestHelper.GetRandomString(newBranch.GB_CodeInfo.MaxLength);
			newBranch.SetCountry("CN");

			var newCompany = newBranch.Company;
			newCompany.GC_Code = MasterFilesTestHelper.GetRandomString(newCompany.GC_CodeInfo.MaxLength);
			newCompany.SetCountry("CN");

			Factory.Save();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "HKHKG";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			var exRate1 = voyage.ExRates.AddNew();
			exRate1.E8_RX_NKExCurrency = "CNY";
			exRate1.E8_VoyageExchangeRate = 15.0m;
			exRate1.E8_GC = GlbCompany.CurrentCompany.PK;

			var exRate2 = voyage.ExRates.AddNew();
			exRate2.E8_RX_NKExCurrency = "EUR";
			exRate2.E8_VoyageExchangeRate = 12.0m;
			exRate2.E8_GC = newCompany.PK;

			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			var fullWrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);

			AssertEquals("fullWrapper.ExchangeRates.Count", 1, fullWrapper.ExchangeRates.Count);
			AssertEquals("fullWrapper.ExchangeRates[0].Currency.Code", "CNY", fullWrapper.ExchangeRates[0].Currency.Code);
			AssertEquals("fullWrapper.ExchangeRates[0].SellRate", 15.0m, fullWrapper.ExchangeRates[0].SellRate);

			using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				fullWrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);

				AssertEquals("fullWrapper.ExchangeRates.Count", 1, fullWrapper.ExchangeRates.Count);
				AssertEquals("fullWrapper.ExchangeRates[0].Currency.Code", "EUR", fullWrapper.ExchangeRates[0].Currency.Code);
				AssertEquals("fullWrapper.ExchangeRates[0].SellRate", 12.0m, fullWrapper.ExchangeRates[0].SellRate);
			}
		}

		public void TestGetPackages()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var container1 = shipment.ShippingContainers.AddNew();
			var container2 = shipment.ShippingContainers.AddNew();

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			var wrappedObjects = wrapper.Packages.Cast<PackageWrapper>().Select(w => w.WrappedObject);

			AssertContainsExactElementsInAnyOrder(
			new[]
			{
				packline1,
				packline2
			},
			wrappedObjects);

			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				shipment.JS_PackingMode = mode;

				AssertEquals("containers have been removed when changing cargo type", 0, shipment.ShippingContainers.Count);
				AssertEquals("pack lines have been removed when changing cargo type", 0, shipment.OuterPackLines.Count);

				container1 = shipment.ShippingContainers.AddNew();
				container2 = shipment.ShippingContainers.AddNew();

				packline1 = shipment.OuterPackLines.AddNew();
				packline2 = shipment.OuterPackLines.AddNew();

				wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
				wrappedObjects = wrapper.Packages.Cast<PackageWrapper>().Select(w => w.WrappedObject);

				AssertContainsExactElementsInAnyOrder(
				new[]
					{
						container1,
						container2
					},
				wrappedObjects.OfType<AgencyShipmentPackLineAdapter>().Select(adapter => adapter.Adaptee));
			}
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFull()
		{
			#region Setup

			var bookedShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			bookedShippingLine.OH_FullName = "SHIPPINGLINE";

			var receivingForwarderDocAddress = Factory.New<JobDocAddress>();
			receivingForwarderDocAddress.E2_CompanyName = "McLaren";
			receivingForwarderDocAddress.DocAddressType = DocAddressType.ReceivingForwarderAddress;

			var sendingForwarderDocAddress = Factory.New<JobDocAddress>();
			sendingForwarderDocAddress.E2_CompanyName = "Manchester United";
			sendingForwarderDocAddress.DocAddressType = DocAddressType.SendingForwarderAddress;

			var principal = Factory.NewWithValidTestData<OrgHeader>();

			var exportAgent = GetOrgHeader("McLaren");
			var exportPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			exportPort.O5_PortOrCountry = "USDNV";
			exportPort.O5_OA_AgentOfficeAddress = exportAgent.MainAddress.PK;

			var importAgent = GetOrgHeader("Manchester United");
			var importPort = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			importPort.O5_PortOrCountry = "ERXXX";
			importPort.O5_OA_AgentOfficeAddress = importAgent.MainAddress.PK;

			var agencyShipment = Factory.New<AgencyShipment>();
			agencyShipment.JS_UniqueConsignRef = "S000234567";
			agencyShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			agencyShipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			agencyShipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			agencyShipment.JS_OH_DeliveryAgent = principal.PK;
			agencyShipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			agencyShipment.JS_RS_NKServiceLevel = "SLV";
			agencyShipment.JS_ShipmentStatus = "BKD";
			agencyShipment.JS_RL_NKOrigin = "USDNV";
			agencyShipment.JS_E_DEP = new ZDateTime(2006, 5, 5);
			agencyShipment.JS_RL_NKDestination = "ERXXX";
			agencyShipment.JS_E_ARV = new ZDateTime(2006, 5, 8);

			agencyShipment.JS_HouseBill = "HOUSEME";

			agencyShipment.JS_HouseBillIssueDate = new ZDateTime(2005, 4, 8);
			agencyShipment.JS_GoodsDescription = "JILTED LOVERS";
			agencyShipment.JS_MarksAndNumbers = "BROKEN HEARTS";
			agencyShipment.JS_InspectionTypeCode = "UNK";

			agencyShipment.DocAddresses.Add(receivingForwarderDocAddress);
			agencyShipment.DocAddresses.Add(sendingForwarderDocAddress);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "HKHKG";
			origin.JA_E_DEP = new ZDateTime(2006, 5, 5);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_ArrivalReference = "ARRIVAL REFERENCE";
			destination.JB_E_ARV = new ZDateTime(2006, 5, 8);
			destination.JB_Berth = "BOOTH A23";

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			agencyShipment.JS_JX = sailing.PK;
			agencyShipment.JS_OA_BookedShippingLineAddress = bookedShippingLine.MainAddress.PK;

			var container = agencyShipment.BookedContainers.AddNew();
			container.JC_ContainerNum = "OOCL0000026";

			var packLine1 = agencyShipment.OuterPackLines.Count == 0 ? agencyShipment.OuterPackLines.AddNew() : agencyShipment.OuterPackLines[0];
			packLine1.JL_JC = container.PK;
			packLine1.JL_PackageCount = 11;
			packLine1.JL_F3_NKPackType = "PK";

			var packLine2 = agencyShipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.JL_PackageCount = 22;
			packLine2.JL_F3_NKPackType = "PK";

			var packLine3 = agencyShipment.OuterPackLines.AddNew();
			packLine3.JL_JC = container.PK;
			packLine3.JL_PackageCount = 33;
			packLine3.JL_F3_NKPackType = "PK";

			agencyShipment.JS_OuterPacks = 400;
			agencyShipment.JS_F3_NKPackType = "PKG";
			agencyShipment.JS_TotalPackageCount = 354;
			agencyShipment.JS_F3_NKTotalCountPackType = "BOX";
			agencyShipment.JS_ActualWeight = 55.4;
			agencyShipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			agencyShipment.JS_ActualVolume = 32.45;
			agencyShipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			agencyShipment.JS_GoodsValue = 665.33m;
			agencyShipment.JS_RX_NKGoodsValueCurr = "HKD";
			agencyShipment.JS_ShippedOnBoard = "SOB";
			agencyShipment.JS_ShippedOnBoardDate = new ZDateTime(2006, 12, 25);
			agencyShipment.JS_UnitFreightRate = 123.45m;
			agencyShipment.JS_RX_NKFrtRateCurrency = "AUD";
			agencyShipment.JS_ReleaseType = "NXS";
			agencyShipment.JS_NoCopyBills = 5;
			agencyShipment.JS_NoOriginalBills = 6;
			agencyShipment.JS_ConsolReference = "55555";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = agencyShipment.PK;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Constants.DomesticPaymentTerms.Collect;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "1ONE1";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "2TWO2";

			agencyShipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;
			agencyShipment.NotifyParty2DocumentaryAddress.E2_OA_Address = GetOrgHeader("ALSONOTIFYME").MainAddress.PK;
			agencyShipment.NotifyParty3DocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYMEASWELL").MainAddress.PK;

			agencyShipment.DocsAndCartage.RequiredDocuments.AddNew();

			#endregion

			var fullWrapper = new FreightWrapperFromAgencyShipment(agencyShipment, Factory);
			AssertEquals("fullWrapper.ConsolContainerMode.Code", ZString.Empty, fullWrapper.ConsolContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", Constants.ContainerModes.FCL, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ConsolTransportMode.Code", ZString.Empty, fullWrapper.ConsolTransportMode.Code);
			AssertEquals("fullWrapper.OrderTransportMode.Code", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.ConsolType.Code", ZString.Empty, fullWrapper.ConsolType.Code);
			AssertEquals("fullWrapper.ShipmentType.Code", "IMP", fullWrapper.ShipmentType.Code);
			AssertEquals("fullWrapper.IncoTerm.Code", Constants.DomesticPaymentTerms.Collect, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.ServiceLevel.Code", "SLV", fullWrapper.ServiceLevel.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "BKD", fullWrapper.ShipmentStatus.Code);
			AssertEquals("fullWrapper.MasterBillHeading", "Bill Of Lading", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBillHeading", ZString.Empty, fullWrapper.HouseBillHeading);

			AssertEquals("fullWrapper.Consignee.CompanyName", "IMPORTER", fullWrapper.Consignee.CompanyName);
			AssertEquals("fullWrapper.Consignor.CompanyName", "SUPPLIER", fullWrapper.Consignor.CompanyName);
			AssertEquals("fullWrapper.Carrier.CompanyName", "SHIPPINGLINE", fullWrapper.Carrier.CompanyName);
			AssertEquals("fullWrapper.PickupAgent.CompanyName", ZString.Empty, fullWrapper.PickupAgent.CompanyName);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", ZString.Empty, fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", ZString.Empty, fullWrapper.DeliveryAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "Manchester United", fullWrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "McLaren", fullWrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportBroker.CompanyName", ZString.Empty, fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.ExportBroker.CompanyName", ZString.Empty, fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.LocalForwarder.CompanyName", ZString.Empty, fullWrapper.LocalForwarder.CompanyName);
			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);

			AssertEquals("fullWrapper.NotifyParty", "NOTIFYME", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.NotifyParty2", "ALSONOTIFYME", fullWrapper.NotifyParty2.CompanyName);
			AssertEquals("fullWrapper.NotifyParty3", "NOTIFYMEASWELL", fullWrapper.NotifyParty3.CompanyName);
			AssertEquals("fullWrapper.ConsolCreditor", ZString.Empty, fullWrapper.ConsolCreditor.CompanyName);

			AssertEquals("fullWrapper.DeliveryAddress.CompanyName", "IMPORTER", fullWrapper.DeliveryAddress.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "SUPPLIER", fullWrapper.PickupAddress.CompanyName);

			AssertEquals("Shipment Routes", 1, fullWrapper.ShipmentRoutes.Count);
			AssertEquals("Consol Routes", 0, fullWrapper.ConsolRoutes.Count);

			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USDNV", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "ERXXX", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Destination.ActualDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.ActualDate);

			AssertEquals("fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero", "400 PKG", fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Volume.ValueAndUnitCodeBlankIfZero", "32.450 M3", fullWrapper.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Weight.ValueAndUnitCodeBlankIfZero", "55.400 KG", fullWrapper.Weight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero", "32.450 M3", fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero", "354 BOX", fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero);

			AssertEquals("fullWrapper.ShippedOnBoardType.Code", "SOB", fullWrapper.ShippedOnBoardType.Code);
			AssertEquals("fullWrapper.ReleaseType.Code", "NXS", fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.FreightRate.AmountAndCurrencyCode", "123.45 AUD", fullWrapper.FreightRate.AmountAndCurrencyCode);

			var dateTimeCreated = agencyShipment.Logs.CreatedDateUtc;
			AssertEquals("fullWrapper.ConsolDateCreated", dateTimeCreated, fullWrapper.ConsolDateCreated);
			AssertEquals("fullWrapper.ShipmentDateCreated", dateTimeCreated, fullWrapper.ShipmentDateCreated);

			AssertEquals("fullWrapper.MasterBill", "", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.HouseBill", ZString.Empty, fullWrapper.HouseBill);
			AssertEquals("fullWrapper.HBLIssueDate", ZDate.Empty, fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.GoodsDescription", "JILTED LOVERS", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.MarksAndNumbers", "BROKEN HEARTS", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.NoCopyBills", 5, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.NoOriginalBills", 6, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2006, 12, 25), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.LocalForwarderReference", "S000234567", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.ExportAgentsReference", "S000234567", fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", "S000234567", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.BookingReference", ZString.Empty, fullWrapper.BookingReference);
			AssertEquals("fullWrapper.ConsolPaymentType", ZString.Empty, fullWrapper.ConsolPaymentType);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.FCL, fullWrapper.HBLContainerMode);

			AssertEquals("fullWrapper.DeliveryCartageAdvised", ZDateTime.Empty, fullWrapper.DeliveryCartageAdvised);
			AssertEquals("fullWrapper.DeliveryFrom", ZDateTime.Empty, fullWrapper.DeliveryFrom);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", ZDateTime.Empty, fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.DeliveryRequiredBy", ZDateTime.Empty, fullWrapper.DeliveryRequiredBy);
			AssertEquals("fullWrapper.PickupCartageAdvised", ZDateTime.Empty, fullWrapper.PickupCartageAdvised);
			AssertEquals("fullWrapper.PickupFrom", ZDateTime.Empty, fullWrapper.PickupFrom);
			AssertEquals("fullWrapper.PickupGoodsPickedup", ZDateTime.Empty, fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.PickupRequiredBy", ZDateTime.Empty, fullWrapper.PickupRequiredBy);
			AssertEquals("fullWrapper.PickupDateOfReceipt", ZDateTime.Empty, fullWrapper.PickupDateOfReceipt);

			AssertEquals("fullWrapper.CustomAttribute1", ZString.Empty, fullWrapper.CustomAttribute1);
			AssertEquals("fullWrapper.CustomAttribute2", ZString.Empty, fullWrapper.CustomAttribute2);
			AssertEquals("fullWrapper.CustomDate1", ZDateTime.Empty, fullWrapper.CustomDate1);
			AssertEquals("fullWrapper.CustomDate2", ZDateTime.Empty, fullWrapper.CustomDate2);
			AssertEquals("fullWrapper.CustomDecimal1", 0m, fullWrapper.CustomDecimal1);
			AssertEquals("fullWrapper.CustomDecimal2", 0m, fullWrapper.CustomDecimal2);
			AssertEquals("fullWrapper.CustomFlag1", false, fullWrapper.CustomFlag1);
			AssertEquals("fullWrapper.CustomFlag2", false, fullWrapper.CustomFlag2);

			AssertEquals("fullWrapper.CommercialInvoices.Count", 0, fullWrapper.CommercialInvoices.Count);
			AssertEquals("fullWrapper.CommercialInvoiceLines.Count", 0, fullWrapper.CommercialInvoiceLines.Count);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", "665.33 HKD", fullWrapper.GoodsValue.AmountAndCurrencyCode);

			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.Packages.Count", 3, fullWrapper.Packages.Count);

			AssertEquals("fullWrapper.CustomsEntries.Count", 0, fullWrapper.CustomsEntries.Count);
			AssertEquals("fullWrapper.Orders.Count", 0, fullWrapper.Orders.Count);

			AssertEquals("fullWrapper.FreightJobs.Count", 0, fullWrapper.FreightJobs.Count);

			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);

			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);

			AssertEquals("fullWrapper.ConsolNumber", ZString.Empty, fullWrapper.ConsolNumber);
			AssertEquals("fullWrapper.JobNumber", "S000234567", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.SecondaryHeading", ZString.Empty, fullWrapper.SecondaryHeading);
			AssertEquals("fullWrapper.SecondaryNumber", ZString.Empty, fullWrapper.SecondaryNumber);
			AssertEquals("fullWrapper.ArrivalReference", "ARRIVAL REFERENCE", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "BOOTH A23", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.UnAllocatedWeight", 0m, fullWrapper.UnAllocatedWeight);
			AssertEquals("fullWrapper.UnAllocatedVolume", 0m, fullWrapper.UnAllocatedVolume);
			AssertEquals("fullWrapper.UnAllocatedPackages", 0, fullWrapper.UnAllocatedPackages);
			AssertEquals("fullWrapper.ConsolReference", "55555", fullWrapper.ConsolReference);

			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);

			AssertEquals("fullWrapper.PickupLocation", "USDNV", fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", "ERXXX", fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);

			AssertEquals("fullWrapper.ReceivingForwarder.CompanyName", "McLaren", fullWrapper.ReceivingForwarder.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder.CompanyName", "Manchester United", fullWrapper.SendingForwarder.CompanyName);

			AssertOrgWrappersReturnRightTypes(fullWrapper);
		}

		public override void TestBuyer()
		{
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", ZString.Empty, wrapper.Buyer.CompanyNameAndAddress);

			JobDocAddress buyerAddress = shipment.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 1";
			buyerAddress.E2_City = "CITY";
			buyerAddress.E2_State = "STATE";
			buyerAddress.E2_Postcode = "PCODE";

			wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", "FRED'S FLAT TYRES\nADDRESS 1\nCITY STATE PCODE\nERITREA", wrapper.Buyer.CompanyNameAndAddress);
		}

		public void TestAgencyShipmentDocumentWrapperNotNull()
		{
			AssertNotNull("Wrapper.AgencyShipment should not be null", Wrapper.AgencyShipment);
		}

		public override void TestInsuranceRelatedAddressFields()
		{
			AssertEquals("wrapper.InsuredBy.CompanyName", ZString.Empty, wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", ZString.Empty, wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", ZString.Empty, wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", ZString.Empty, wrapper.SurveyReportParty.CompanyName);

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY THING";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY THING";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY THING";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY THING";

			wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY THING", wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY THING", wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY THING", wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY THING", wrapper.SurveyReportParty.CompanyName);
		}

		public override void TestWrapperNotes()
		{
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "TEST HANDLING INFO");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "TEST CERTIFICATE OF ORIGIN NOTE INCEST");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO");

			FreightWrapperFromAgencyShipment noteWrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			AssertEquals("fullWrapper.DangerousGoodsAdditionalHandlingInformation", "TEST HANDLING INFO", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("fullWrapper.CertificateOfOriginNotes", "TEST CERTIFICATE OF ORIGIN NOTE INCEST", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("fullWrapper.PreAlertArrivalNoticeRemarks", "TEST PRE-ALERT ARRIVAL NOTICE REMARKS BOO", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		public void TestPrincipal()
		{
			AssertEquals("wrapper.Principal.CompanyNameAndAddress", ZString.Empty, wrapper.Principal.CompanyNameAndAddress);

			OrgHeader header = GetOrgHeader("AO MMM");
			OrgAddress principalAddress = header.MainAddress;
			principalAddress.OA_Address1 = "ADDRESS1";
			principalAddress.OA_City = "CITY";
			principalAddress.OA_State = "STATE";
			principalAddress.OA_PostCode = "PCODE";
			principalAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OH_DeliveryAgent = header.PK;

			wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			AssertEquals("wrapper.Principal.CompanyNameAndAddress", "AO MMM\nADDRESS1\nCITY STATE PCODE\nAUSTRALIA", wrapper.Principal.CompanyNameAndAddress);
		}

		public void TestAlternativeBranding()
		{
			var shipment = Factory.New<ShipmentForTest>();
			var wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			var image = new Bitmap(1, 1);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, image);
			AssertEquals("Default company logo without branding.", image.Size, wrapper.CompanyLogo.Size);
			AssertEquals("Default brand name without branding.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), wrapper.BrandName);
			AssertEquals("Default brand email without branding.", GlbStaff.CurrentUser.GS_EmailAddress, wrapper.BrandEmailAddress);

			var branding = new PrincipalBranding();
			branding.Code = "PB1";
			branding.BrandName = "Brand 1";
			branding.BrandEmailAddress = "generic@brand1.com";
			branding.Image = new Bitmap(2, 2);

			shipment.AlternativeBrandingForTest = branding;

			AssertNotEquals("Precondition: alternate image size does not equal to default image size.", image.Size, branding.Image.Size);
			AssertNotEquals("Precondition: alternate brand name does not equal to default brand name.", GlbCompany.CurrentCompany.GC_Name.ToUpper(), branding.BrandName);
			AssertNotEquals("Precondition: alternate brand email does not equal to default brand email.", GlbStaff.CurrentUser.GS_EmailAddress, branding.BrandEmailAddress);

			AssertEquals("Alternative company logo.", branding.Image.Size, wrapper.CompanyLogo.Size);
			AssertEquals("Alternative brand name.", branding.BrandName.ToUpper(), wrapper.BrandName);
			AssertEquals("Alternative brand email.", branding.BrandEmailAddress, wrapper.BrandEmailAddress);
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Shipping);
			var defaultNumberOfDecimals_Weight = collection.AddNew();
			defaultNumberOfDecimals_Weight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_Weight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Weight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_Volume = collection.AddNew();
			defaultNumberOfDecimals_Volume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_Volume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Volume.RoundingMode = RoundingModes.Down;

			AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 55.441m;
			shipment.JS_ActualVolume = 32.369m;

			var wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);

			AssertEquals(55.45m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("55.45 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(32.36m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("32.36 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(32.36m, wrapper.ChargeableWeight.Value);
			AssertEquals("M3", wrapper.ChargeableWeight.Unit.Code);
			AssertEquals("32.36 M3", wrapper.ChargeableWeight.ValueAndUnitCode);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader bookedShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			bookedShippingLine.OH_FullName = "SHIPPINGLINE";
			bookedShippingLine.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			shipment.JS_OH_DeliveryAgent = GetOrgHeader("PRINCIPAL").PK;
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			shipment.JS_RS_NKServiceLevel = "SLV";
			shipment.JS_ShipmentStatus = "MSA";
			shipment.JS_RL_NKOrigin = "USDNV";
			shipment.JS_RL_NKDestination = "ERXXX";
			shipment.JS_OA_BookedShippingLineAddress = bookedShippingLine.MainAddress.PK;

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;

			shipment.JS_OH_ImportBroker = GetOrgHeader("IMP_BROKER").PK;
			shipment.JS_OH_ExportBroker = GetOrgHeader("EXP_BROKER").PK;

			shipment.JS_OuterPacks = 400;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_TotalPackageCount = 354;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_ActualWeight = 55.4;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 32.45;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_GoodsValue = 665.33m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_ShippedOnBoard = "SOB";
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_ConsolReference = "ConsolRef";

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY THING";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY THING";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY THING";

			JobDocAddress buyerAddress = shipment.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 1";
			buyerAddress.E2_City = "CITY";
			buyerAddress.E2_State = "STATE";
			buyerAddress.E2_Postcode = "PCODE";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY THING";

			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			pickupdepotAddress.OA_Address1 = "TEST 1 ADDRESS";
			shipment.JS_A_RCV = new ZDateTime(2017, 9, 12);
			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			unpackdepotAddress.OA_Address1 = "TEST 2 ADDRESS";

			Transport shipmentTransport = shipment.Transports.AddNew();
			shipmentTransport.JW_LegOrder = 2;
			shipmentTransport.JW_RL_NKDiscPort = "ERXXX";
			shipmentTransport.JW_OA_ArrivalLocation = unpackdepotAddress.PK;
			shipmentTransport.JW_OA_DepartureLocation = pickupdepotAddress.PK;

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = GetOrgHeader("ALSONOTIFYME").MainAddress.PK;
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYMEASWELL").MainAddress.PK;

			return new FreightWrapperFromAgencyShipment(shipment, Factory);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "ActualReceive", "12-Sep-17 00:00:00" },
					{ "ConsolReference", "ConsolRef" },
					{ "HBLContainerMode",  "FCL" },
					{ "JobNumberHeading", "Shipment" },
					{ "MasterBillHeading", "Bill Of Lading" },
					{ "NoCopyBills", "3" },
					{ "NoOriginalBills", "3" },
				};
			}
		}

		public override void TestTrackingBusinessObjectPK()
		{
			shipment = Factory.New<AgencyShipment>();
			var wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
			AssertEquals("TrackingBusinessObjectPK", shipment.PK, wrapper.TrackingBusinessObjectPK);
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
AssuredParty : ASSUREDPARTY THING\nERITREA
Buyer : FRED'S FLAT TYRES\nADDRESS 1\nCITY STATE PCODE\nERITREA
Carrier : SHIPPINGLINE\n#1\nAUSTRALIA
CarrierAccount :  is null
CarrierServiceLevel : 
CartageInfo : (No Default Field Value Available on CartageInfo)
ChargeableWeight : 32.450 M3
ClaimsPayableBy : CLAIMSPAYABLEBY THING\nERITREA
Consignee : IMPORTER\nAUSTRALIA
Consignor : SUPPLIER\nAUSTRALIA
DeliveryAddress : IMPORTER\nAUSTRALIA
DeliveryLocation : ERXXX
Destination : ERXXX
FreightRate : 123.45 AUD
GoodsAvailableAt : TEST 2 ADDRESS\nAUSTRALIA
GoodsValue : 665.33 HKD
IncoTerm : CLT - Collect
InspectionType : UNK - Unknown - No Security Measures Taken
InsuredBy : INSUREDBY THING\nERITREA
NotifyParty : NOTIFYME\nAUSTRALIA
NotifyParty2 : ALSONOTIFYME\nAUSTRALIA
NotifyParty3 : NOTIFYMEASWELL\nAUSTRALIA
Origin : USDNV - Dunnville
PickupAddress : SUPPLIER\nAUSTRALIA
PickupCFSAddress : TEST 1 ADDRESS\nAUSTRALIA
PickupLocation : USDNV - Dunnville
Principal : PRINCIPAL\nAUSTRALIA
ReleaseType : NXS
ServiceLevel : SLV
ShipmentContainerMode : FCL - Full Container Load
ShipmentInnerPacksQty : 354 BOX
ShipmentOuterPacksQty : 400 PKG
ShipmentStatus : MSA
ShipmentTransportMode : SEA - Sea Freight
ShipmentType : IMP - Import
ShippedOnBoardType : SOB
SurveyReportParty : SURVEYREPORTPARTY THING\nERITREA
Volume : 32.450 M3
Weight : 55.400 KG";
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<AgencyShipment>();
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((AgencyShipment)WrappedBO).JS_OA_BookedShippingLineAddress = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;
			return new FreightWrapperFromAgencyShipment((AgencyShipment)WrappedBO, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<AgencyShipment>();
			wrapper = new FreightWrapperFromAgencyShipment(shipment, Factory);
		}
		FreightWrapperFromAgencyShipment wrapper;
		AgencyShipment shipment;

		#endregion

		#region Test Classes

		class ShipmentForTest : AgencyShipment, IDocumentSupportable
		{
			public ShipmentForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get { return new DocumentSupporterForTest(this); }
			}

			public DocumentBrandingBusinessObject AlternativeBrandingForTest { get; set; }
		}

		class DocumentSupporterForTest : DocumentSupporter
		{
			public DocumentSupporterForTest(ShipmentForTest bizObj)
				: base(bizObj)
			{
			}

			public override ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
			{
				return ((ShipmentForTest)BusinessObject).AlternativeBrandingForTest;
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.INVALID; }
			}

			#region Not Implemented

			public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { throw new NotImplementedException(); }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				throw new NotImplementedException();
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				throw new NotImplementedException();
			}

			#endregion //Not Implemented
		}

		#endregion
	}
}
