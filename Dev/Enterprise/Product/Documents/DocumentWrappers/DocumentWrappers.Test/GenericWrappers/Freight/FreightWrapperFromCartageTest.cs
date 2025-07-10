using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCartage))]
	sealed class FreightWrapperFromCartageTest : FreightWrapperTest
	{
		#region TestBusinessObjectToLogAgainst

		public void TestBusinessObjectToLogAgainst()
		{
			var cartage = Factory.New<CommonCartage>();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var leg = looseMove.CartageLegs.AddNew();
			var cartageWrapper1 = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals(cartage, ((IBODocDataProvider)cartageWrapper1).BusinessObjectToLogAgainst);

			var cartageWrapper2 = FreightWrapper.New(cartage, leg, Factory)[0];
			AssertEquals(leg, ((IBODocDataProvider)cartageWrapper2).BusinessObjectToLogAgainst);
		}

		#endregion

		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			return ((CommonCartage)parent).ContainerBookedMoves.AddNew().Container;
		}

		#region TestExportReceivingDepotAddress

		public void TestExportReceivingDepotAddress()
		{
			FreightWrapperFromCartage wrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingDepotAddress.Address);

			TestCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;

			OrgAddress localCartageCFS = Factory.New<OrgAddress>();
			localCartageCFS.OA_Address1 = "CARTAGEBO LOCALCARTAGECFSADDRESS 33";
			localCartageCFS.OA_RN_NKCountryCode = "AU";
			TestCartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS).E2_OA_Address = localCartageCFS.PK;

			wrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("Should return AddressWrapper with CartageBO.LocalCartageCFS", "CARTAGEBO LOCALCARTAGECFSADDRESS 33\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);
		}

		#endregion

		#region TestExportReceivingCTOAddress

		public void TestExportReceivingCTOAddress()
		{
			FreightWrapperFromCartage wrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			TestCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;

			OrgAddress localCartageCTO = Factory.New<OrgAddress>();
			localCartageCTO.OA_Address1 = "CARTAGEBO LOCALCARTAGECTOADDRESS 22";
			localCartageCTO.OA_RN_NKCountryCode = "AU";
			TestCartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO).E2_OA_Address = localCartageCTO.PK;

			wrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("Should return AddressWrapper with CartageBO.LocalCartageCTO", "CARTAGEBO LOCALCARTAGECTOADDRESS 22\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
		}

		#endregion

		#region TestExportReceivalAddress

		public void TestExportReceivalAddress()
		{
			FreightWrapperFromCartage wrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			TestCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportToSHP;

			OrgAddress localCartageCTO = Factory.New<OrgAddress>();
			localCartageCTO.OA_Address1 = "CARTAGEBO LOCALCARTAGECTOADDRESS 22";
			localCartageCTO.OA_RN_NKCountryCode = "AU";
			TestCartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO).E2_OA_Address = localCartageCTO.PK;

			wrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("Should return AddressWrapper with CartageBO.LocalCartageCTO", "CARTAGEBO LOCALCARTAGECTOADDRESS 22\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			TestCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;

			OrgAddress localCartageCFS = Factory.New<OrgAddress>();
			localCartageCFS.OA_Address1 = "CARTAGEBO LOCALCARTAGECFSADDRESS 33";
			localCartageCFS.OA_RN_NKCountryCode = "AU";
			TestCartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS).E2_OA_Address = localCartageCFS.PK;

			wrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("Should return AddressWrapper with CartageBO.LocalCartageCFS", "CARTAGEBO LOCALCARTAGECFSADDRESS 33\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		#endregion

		#region TestInsuranceRelatedAddressFields

		public override void TestInsuranceRelatedAddressFields()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY SHIPMENT";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY SHIPMENT";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY SHIPMENT";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY SHIPMENT";

			FreightWrapperFromCartage shipmentWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY SHIPMENT", shipmentWrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY SHIPMENT", shipmentWrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY SHIPMENT", shipmentWrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY SHIPMENT", shipmentWrapper.SurveyReportParty.CompanyName);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			cartage.JJ_ParentID = declaration.PK;

			JobDocAddress declarationInsuredBy = declaration.InsuredByDocAddress;
			declarationInsuredBy.E2_AddressOverride = true;
			declarationInsuredBy.E2_CompanyName = "INSUREDBY DECLARATION";

			JobDocAddress declarationAssuredParty = declaration.AssuredPartyDocAddress;
			declarationAssuredParty.E2_AddressOverride = true;
			declarationAssuredParty.E2_CompanyName = "ASSUREDPARTY DECLARATION";

			JobDocAddress declarationClaimsPayableBy = declaration.ClaimsPayableByDocAddress;
			declarationClaimsPayableBy.E2_AddressOverride = true;
			declarationClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY DECLARATION";

			JobDocAddress declarationSurveyReportParty = declaration.SurveyReportPartyDocAddress;
			declarationSurveyReportParty.E2_AddressOverride = true;
			declarationSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY DECLARATION";

			FreightWrapperFromCartage declarationWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY DECLARATION", declarationWrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY DECLARATION", declarationWrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY DECLARATION", declarationWrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY DECLARATION", declarationWrapper.SurveyReportParty.CompanyName);
		}

		#endregion

		#region TestBuyer

		public override void TestBuyer()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			cartage.JJ_ParentID = declaration.PK;

			FreightWrapperFromCartage wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", ZString.Empty, wrapper.Buyer.CompanyNameAndAddress);

			JobDocAddress buyerAddress = declaration.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 1";
			buyerAddress.E2_City = "CITY";
			buyerAddress.E2_State = "STATE";
			buyerAddress.E2_Postcode = "PCODE";

			wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", "FRED'S FLAT TYRES\nADDRESS 1\nCITY STATE PCODE\nERITREA", wrapper.Buyer.CompanyNameAndAddress);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;

			wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", ZString.Empty, wrapper.Buyer.CompanyNameAndAddress);

			buyerAddress = shipment.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S NOT SO FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 2";
			buyerAddress.E2_City = "CITY2";
			buyerAddress.E2_State = "STATE2";
			buyerAddress.E2_Postcode = "PCODE2";

			wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", "FRED'S NOT SO FLAT TYRES\nADDRESS 2\nCITY2 STATE2 PCODE2\nERITREA", wrapper.Buyer.CompanyNameAndAddress);
		}

		#endregion

		#region TestWrapperMappingFullCartageJob

		public void TestWrapperMappingFullCartageJob()
		{
			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();
			var voyageDestination = Factory.New<VoyageDestination>();
			var voyageOrigin = Factory.New<VoyageOrigin>();
			voyageDestination.JB_ArrivalReference = "CARTAGE ARRIVAL REF";
			voyageDestination.JB_JV = voyage.PK;
			voyageOrigin.JA_JV = voyage.PK;
			sailing.JX_JB = voyageDestination.PK;
			sailing.JX_JA = voyageOrigin.PK;
			sailing.Destination.JB_AvailabilityDate = new ZDateTime(2007, 8, 9);
			sailing.JX_DepotAvailabilityDate = new ZDateTime(2006, 2, 5);
			sailing.Destination.JB_StorageDate = new ZDateTime(2005, 4, 23);
			sailing.JX_DepotStorageDate = new ZDateTime(2006, 8, 19);
			voyageOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			voyageDestination.JB_RL_NKPortOfDischarge = "NZAKL";
			voyageDestination.JB_E_ARV = new ZDateTime(2008, 5, 20);
			voyageDestination.JB_A_ARV = new ZDateTime(2008, 6, 21);
			voyageDestination.JB_Berth = "WHARF 666";
			voyageOrigin.JA_E_DEP = new ZDateTime(2008, 7, 22);
			voyageOrigin.JA_A_DEP = new ZDateTime(2008, 8, 23);

			TestCartage.JJ_ConsignmentID = "T0099923";
			TestCartage.JJ_OrderReferenceNumber = "ORDREF";
			TestCartage.JJ_JX_Sailing = sailing.PK;
			TestCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportToSHP;
			TestCartage.JJ_RS_NKServiceLevel = "SLV";
			TestCartage.JJ_Weight = 46.23m;
			TestCartage.JJ_WeightUQ = "T";
			TestCartage.JJ_Volume = 23.56m;
			TestCartage.JJ_VolumeUQ = "CF";
			TestCartage.JJ_GoodsDescription = "WORK ITEM SYSTEM IS CRAP";
			TestCartage.JJ_EstimatedDelivery = new ZDateTime(2004, 9, 27);
			TestCartage.JJ_EstimatedPickup = new ZDateTime(2005, 11, 13);
			TestCartage.FCLCutOff = new ZDateTime(2008, 12, 26);
			TestCartage.LCLCutOff = new ZDateTime(2007, 8, 17);
			TestCartage.JJ_WaybillNumber = "1A2B3C";
			TestCartage.JJ_QuoteNumber = "QUOTENUM";

			var ctoOrg = CartageHelper.CreateOrgHeader("CTO", "CTO");
			ctoOrg.MainAddress.OA_Address1 = "1 CTO STREET";
			ctoOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			TestCartage.FirstDocAddress.E2_OA_Address = ctoOrg.MainAddress.PK;
			var cfsOrg = CartageHelper.CreateOrgHeader("CFS", "CFS");
			cfsOrg.MainAddress.OA_Address1 = "2 CFS STREET";
			cfsOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			TestCartage.SecondDocAddress.E2_OA_Address = cfsOrg.MainAddress.PK;
			var cydOrg = CartageHelper.CreateOrgHeader("CYD", "CYD");
			cydOrg.MainAddress.OA_Address1 = "3 CYD STREET";
			cydOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			TestCartage.ThirdDocAddress.E2_OA_Address = cydOrg.MainAddress.PK;

			var container = TestCartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "OOCL0000026";
			container.JC_SealNum = "12345";

			var fullWrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("fullWrapper.MasterBillHeading", "Waybill", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.MasterBill", "1A2B3C", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.HouseBillHeading", string.Empty, fullWrapper.HouseBillHeading);
			AssertEquals("fullWrapper.ShipmentType.Code", "EXP", fullWrapper.ShipmentType.Code);
			AssertEquals("fullWrapper.ServiceLevel.Code", "SLV", fullWrapper.ServiceLevel.Code);
			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.Containers[0].ContainerNo", "OOCL0000026", fullWrapper.Containers[0].ContainerNo);
			AssertEquals("fullWrapper.Containers[0].SealNo", "12345", fullWrapper.Containers[0].SealNo);
			AssertEquals("fullWrapper.LocalForwarderReference", "T0099923", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.ExportAgentsReference", "T0099923", fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", ZString.Empty, fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.Weight.ValueAndUnitCodeBlankIfZero", "46.2 T", fullWrapper.Weight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Volume.ValueAndUnitCodeBlankIfZero", "23.560 CF", fullWrapper.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.GoodsDescription", "WORK ITEM SYSTEM IS CRAP", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.OwnerReference", "ORDREF", fullWrapper.OwnerReference);
			AssertEquals("fullWrapper.JobNumber", "T0099923", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.DeliveryFrom", new ZDateTime(2004, 9, 27), fullWrapper.DeliveryFrom);
			AssertEquals("fullWrapper.PickupFrom", new ZDateTime(2005, 11, 13), fullWrapper.PickupFrom);
			AssertEquals("fullWrapper.ArrivalReference", "CARTAGE ARRIVAL REF", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "WHARF 666", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.PortOfLoading.Location.UNLOCO", "AUSYD", fullWrapper.ConsolRoutes["MostInteresting"].Origin.UNLOCO);
			AssertEquals("fullWrapper.PortOfLoading.EstimatedDate", new ZDateTime(2008, 7, 22), fullWrapper.ConsolRoutes["MostInteresting"].EstimatedDeparture);
			AssertEquals("fullWrapper.PortOfLoading.ActualDate", ZDateTime.Empty, fullWrapper.ConsolRoutes["MostInteresting"].ActualDeparture);
			AssertEquals("fullWrapper.PortOfLoading.Location.UNLOCO", "NZAKL", fullWrapper.ConsolRoutes["MostInteresting"].Destination.UNLOCO);
			AssertEquals("fullWrapper.PortOfLoading.EstimatedDate", new ZDateTime(2008, 5, 20), fullWrapper.ConsolRoutes["MostInteresting"].EstimatedArrival);
			AssertEquals("fullWrapper.PortOfLoading.ActualDate", ZDateTime.Empty, fullWrapper.ConsolRoutes["MostInteresting"].ActualArrival);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);
			AssertEquals("fullWrapper.QuoteNumber.", "QUOTENUM", fullWrapper.QuoteNumber);
			AssertEquals("fullWrapper.TransportAddresses.Count", 4, fullWrapper.TransportAddresses.Count);
			AssertMultilineASCIIEquals("fullWrapper.TransportAddresses[0]", "1 CTO STREET\nSYDNEY\nAUSTRALIA", fullWrapper.TransportAddresses[0].Address);
			AssertMultilineASCIIEquals("fullWrapper.TransportAddresses[1]", "2 CFS STREET\nSYDNEY\nAUSTRALIA", fullWrapper.TransportAddresses[1].Address);
			AssertMultilineASCIIEquals("fullWrapper.TransportAddresses[2]", "3 CYD STREET\nSYDNEY\nAUSTRALIA", fullWrapper.TransportAddresses[2].Address);
			AssertEquals("fullWrapper.TransportAddresses[3]", AddressWrapper.Empty(Factory).Address, fullWrapper.TransportAddresses[3].Address);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill_ChecksAdditionalReferencesFirst()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "123456789";
			var shipment = consol.Shipments.AddNew();

			var cartage = CartageHelper.CreateInternalCartage(shipment);
			CartageHelper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.MasterBill, "Correct MasterBill Number");

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBill", "Correct MasterBill Number", cartageWrapper.MasterBill);
		}

		public void TestMasterBill_MapsFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "DEC25818";

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			cartage.JJ_ParentID = declaration.PK;

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBill", "DEC25818", cartageWrapper.MasterBill);
		}

		public void TestMasterBill_MapsFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "123456789";
			var shipment = consol.Shipments.AddNew();

			var cartage = CartageHelper.CreateInternalCartage(shipment);
			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBill", "123456789", cartageWrapper.MasterBill);
		}

		public void TestMasterBill_IsBlank_IfOnlyHouseBillHasValue()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HouseBill";

			var cartage = CartageHelper.CreateInternalCartage(shipment);
			cartage.JJ_WaybillNumber = "WayBillNumber";

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBill", string.Empty, cartageWrapper.MasterBill);
		}

		public void TestMasterBill_IsSetToWayBill_IfBothBillsAreEmpty()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_WaybillNumber = "WayBillNumber";
			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBill", "WayBillNumber", cartageWrapper.MasterBill);
		}

		public void TestMasterBill_IsEmpty_IfBothBillsAreTheSame()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "123456789";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "123456789";
			var cartage = CartageHelper.CreateInternalCartage(shipment);

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBill", "", cartageWrapper.MasterBill);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill_ChecksAdditionalReferencesFirst()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Don'tWantThisValue";

			var cartage = CartageHelper.CreateInternalCartage(shipment);
			CartageHelper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.HouseBill, "Correct HouseBill Number");

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.HouseBill", "Correct HouseBill Number", cartageWrapper.HouseBill);
		}

		public void TestHouseBill_MapsFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "DEC654645123";

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			cartage.JJ_ParentID = declaration.PK;

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.HouseBill", "DEC654645123", cartageWrapper.HouseBill);
		}

		public void TestHouseBill_MapsFromShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "654645123";

			var cartage = CartageHelper.CreateInternalCartage(shipment);
			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.HouseBill", "654645123", cartageWrapper.HouseBill);
		}

		public void TestHouseBill_IsBlank_IfShipmentHouseBillIsBlank()
		{
			var cartage = Factory.New<CommonCartage>();
			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.HouseBill", "", cartageWrapper.HouseBill);
		}

		#endregion

		#region TestBillHeadings

		public void TestBillHeadings_HasOnlyHouse()
		{
			var cartage = Factory.New<CommonCartage>();
			CartageHelper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.HouseBill, "HouseBillNumber");

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBillHeading", "Master Bill", cartageWrapper.MasterBillHeading);
			AssertEquals("cartageWrapper.HouseBillHeading", "House Bill", cartageWrapper.HouseBillHeading);
		}

		public void TestBillHeadings_HasOnlyMaster()
		{
			var cartage = Factory.New<CommonCartage>();
			CartageHelper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.MasterBill, "MasterBillNumber");

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBillHeading", "Master Bill", cartageWrapper.MasterBillHeading);
			AssertEquals("cartageWrapper.HouseBillHeading", "House Bill", cartageWrapper.HouseBillHeading);
		}

		public void TestBillHeadings_HasBoth()
		{
			var cartage = Factory.New<CommonCartage>();
			CartageHelper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.HouseBill, "HouseBillNumber");
			CartageHelper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.MasterBill, "MasterBillNumber");

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBillHeading", "Master Bill", cartageWrapper.MasterBillHeading);
			AssertEquals("cartageWrapper.HouseBillHeading", "House Bill", cartageWrapper.HouseBillHeading);
		}

		public void TestBillHeadings_HasNeither()
		{
			var cartage = Factory.New<CommonCartage>();
			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("cartageWrapper.MasterBillHeading", "Waybill", cartageWrapper.MasterBillHeading);
			AssertEquals("cartageWrapper.HouseBillHeading", "", cartageWrapper.HouseBillHeading);
		}

		#endregion

		#region TestWrapperMappingMasterBillIssueDate

		public void TestWrapperMappingMasterBillIssueDate()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillIssueDate = new ZDateTime(2005, 11, 7);
			ForwardingShipment shipment = consol.Shipments.AddNew();

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;

			FreightWrapperFromCartage fullWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("fullWrapper.MasterBillIssue", new ZDateTime(2005, 11, 7), fullWrapper.MasterBillIssue);
		}

		#endregion

		#region TestWrapperMappingFullShipment

		[TestDate(2007, 1, 1)]
		public void TestWrapperMappingFullShipment()
		{
			#region Setup

			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "TEST PICKUP DEPOT ADDRESS";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			var pickupdepotOrg = GetOrgHeader("Pickup Depot", "PICKDPOT");
			pickupdepotAddress.OA_OH = pickupdepotOrg.PK;

			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST UNPACK DEPOT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			var unpackdepotOrg = GetOrgHeader("Unpack Depot", "UNPKDPOT");
			unpackdepotAddress.OA_OH = unpackdepotOrg.PK;

			OrgHeader cTO = GetOrgHeader("I WANT TO DIE", "CTO");
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";
			cTO.FillWithValidTestData();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";

			OrgHeader testSF = GetOrgHeader("TEST SENDING FORWARDER", "TESTSF");
			testSF.MainAddress.OA_Address1 = "TEST MAIN SENDING ADDRESS";
			testSF.MainAddress.OA_RN_NKCountryCode = "AU";
			testSF.FillWithValidTestData();

			OrgAddress sfAddr2 = testSF.Addresses.AddNew();
			sfAddr2.OA_Address1 = "TEST SENDING ADDRESS";
			sfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_SendingForwarderAddress = sfAddr2.PK;

			OrgHeader testRF = GetOrgHeader("TEST IMPORT AGENT", "TESTRF");
			testRF.MainAddress.OA_Address1 = "TEST MAIN IMPORT ADDRESS";
			testRF.MainAddress.OA_RN_NKCountryCode = "AU";
			testRF.FillWithValidTestData();

			OrgAddress rfAddr2 = testRF.Addresses.AddNew();
			rfAddr2.OA_Address1 = "TEST IMPORT ADDRESS";
			rfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			OrgHeader testCarrier = GetOrgHeader("TEST CARRIER", "TESTCARR");
			testCarrier.MainAddress.OA_Address1 = "TEST MAIN CARRIER ADDRESS";
			testCarrier.MainAddress.OA_RN_NKCountryCode = "AU";
			testCarrier.FillWithValidTestData();
			OrgAddress carrierAddr2 = testCarrier.Addresses.AddNew();
			carrierAddr2.OA_Address1 = "TEST CARRIER ADDRESS";
			carrierAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrierAddr2.PK;

			consol.JK_MasterBillIssueDate = new ZDateTime(2005, 11, 7);
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;

			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0001234";

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageDestination voyageDestination = Factory.New<VoyageDestination>();
			VoyageOrigin voyageOrigin = Factory.New<VoyageOrigin>();
			voyageDestination.JB_ArrivalReference = "CONSOL ARRIVAL REF";
			voyageDestination.JB_JV = voyage.PK;
			voyageDestination.JB_Berth = "CRAP 234";
			voyageOrigin.JA_JV = voyage.PK;
			sailing.JX_JB = voyageDestination.PK;
			sailing.JX_JA = voyageOrigin.PK;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_JX = sailing.PK;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_RL_NKOrigin = "USLKJ";
			shipment.JS_E_DEP = new ZDateTime(2006, 5, 9);
			shipment.JS_RL_NKDestination = "AUZZZ";
			shipment.JS_E_ARV = new ZDateTime(2006, 5, 15);
			shipment.JS_OH_ExportBroker = GetOrgHeader("TEST EXPORT BROKER", "TESTEXP").PK;
			shipment.JS_OH_ImportBroker = GetOrgHeader("TEST IMPORT BROKER", "TESTIMP").PK;
			shipment.JS_TotalPackageCount = 568;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_OuterPacks = 87;
			shipment.JS_F3_NKPackType = "LEG";
			shipment.JS_ActualVolume = 45.89;
			shipment.JS_ActualWeight = 58.57;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			shipment.JS_GoodsValue = 723.98m;
			shipment.JS_RX_NKGoodsValueCurr = "AUD";
			shipment.JS_UnitFreightRate = 784.23m;
			shipment.JS_RX_NKFrtRateCurrency = "HKD";
			shipment.JS_MarksAndNumbers = "SOME TEST MARKS FOR THE WRAPPER";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2006, 11, 8);
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2006, 12, 25);
			shipment.JS_InterimReceipt = "123LKJH";
			shipment.JS_WarehouseLocation = "CAIRNS";
			shipment.JS_A_RCV = new ZDateTime(2003, 4, 23);
			shipment.JS_BookingReference = "SHIPPERS REFERENCE";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("TEST SUPPLIER", "TESTSUPP").PK;
			shipment.JS_NoOriginalBills = 9;
			shipment.JS_NoCopyBills = 3;
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;

			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERY CARTAGE COMPANY", "DELVCART").PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUP CARTAGE COMPANY", "PICKCART").PK;

			shipment.DocsAndCartage.JP_PickupLabourCharge = 15.20m;
			shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 34.30m;
			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "ABX";
			shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Now.AddHours(50);
			shipment.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.Now.AddHours(34);
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 56;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("TEST NOTIFY PARTY", "TESTNOTF").MainAddress.PK;

			ForwardingPackLine packLine = shipment.OuterPackLines[0];
			packLine.JL_JC = container.PK;
			packLine.JL_ActualWeight = 22.34m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualVolume = 10.78m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine.JL_PackageCount = 54;

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;

			shipment.DocsAndCartage.RequiredDocuments.AddNew();

			shipment.Services.AddNew();
			Factory.Save();

			#endregion

			FreightWrapperFromCartage fullWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", Core.Constants.ContainerModes.FCL, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.IncoTerm.Code", Core.Constants.IncoTerms.FreeOnBoard, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "CCX - Collect", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.ReleaseType.Code", "NXS", fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.ShippedOnBoardType.Code", "SHP", fullWrapper.ShippedOnBoardType.Code);
			AssertEquals("fullWrapper.CaratagePickupMode.Code", "ABX", fullWrapper.CaratagePickupMode.Code);

			AssertEquals("fullWrapper.ExportAgent.CompanyNameAndAddress", "TEST SENDING FORWARDER\r\nTEST SENDING ADDRESS\nAUSTRALIA", fullWrapper.ExportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.Carrier.CompanyNameAndAddress", "TEST CARRIER\nTEST CARRIER ADDRESS\nAUSTRALIA", fullWrapper.Carrier.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ImportAgent.CompanyNameAndAddress", "TEST IMPORT AGENT\nTEST IMPORT ADDRESS\nAUSTRALIA", fullWrapper.ImportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.LocalForwarder.CompanyNameAndAddress", "TEST IMPORT AGENT\r\nTEST IMPORT ADDRESS\nAUSTRALIA", fullWrapper.LocalForwarder.CompanyNameAndAddress);

			AssertEquals("fullWrapper.ExportBroker.CompanyName", "TEST EXPORT BROKER", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.ImportBroker.CompanyName", "TEST IMPORT BROKER", fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.NotifyParty.CompanyName", "TEST NOTIFY PARTY", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", "DELIVERY CARTAGE COMPANY", fullWrapper.DeliveryAgent.CompanyName);
			AssertEquals("fullWrapper.PickupAgent.CompanyName", "PICKUP CARTAGE COMPANY", fullWrapper.PickupAgent.CompanyName);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "I WANT TO DIE", fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "TEST SUPPLIER", fullWrapper.PickupAddress.CompanyName);
			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USLKJ", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2006, 5, 9), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate", new ZDateTime(2006, 5, 9), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "AUZZZ", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2006, 5, 15), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Destination.ActualDate", new ZDateTime(2006, 5, 15), fullWrapper.Destination.ActualDate);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero", "568 BOX", fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero", ZString.Empty, fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero", "216.6 KG", fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", "723.98 AUD", fullWrapper.GoodsValue.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.FreightRate.AmountAndCurrencyCode", "784.23 HKD", fullWrapper.FreightRate.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.MarksAndNumbers", "SOME TEST MARKS FOR THE WRAPPER", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.HouseBill", "HOUSE BILL", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.HBLIssueDate", new ZDateTime(2006, 11, 8), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.HBLContainerMode", Core.Constants.ContainerModes.FCL, fullWrapper.HBLContainerMode);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2006, 12, 25), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.NoOriginalBills", 9, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.NoCopyBills", 3, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.PickupInterimReceipt", "123LKJH", fullWrapper.PickupInterimReceipt);
			AssertEquals("fullWrapper.WarehouseLocation", "CAIRNS", fullWrapper.WarehouseLocation);
			AssertEquals("fullWrapper.ActualReceive", new ZDateTime(2003, 4, 23), fullWrapper.ActualReceive);
			AssertEquals("fullWrapper.PickupLabourCharge", 15.20m, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", "50:00", fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", 34.30m, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", "34:00", fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.ShippersReference", "SHIPPERS REFERENCE", fullWrapper.ShippersReference);
			AssertEquals("fullWrapper.StorageTime", "56 Hours", fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.PickupCFSAddress.Address", "TEST PICKUP DEPOT ADDRESS\nAUSTRALIA", fullWrapper.PickupCFSAddress.Address);
			AssertEquals("fullWrapper.UnpackCFSAddress.Address", "TEST UNPACK DEPOT ADDRESS\nAUSTRALIA", fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.GoodsAvailableAt", "TEST UNPACK DEPOT ADDRESS\nAUSTRALIA", fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.RequiredDocuments.Count", 0, fullWrapper.RequiredDocuments.Count);
			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);
			AssertEquals("fullWrapper.ArrivalReference", "CONSOL ARRIVAL REF", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "CRAP 234", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.UnAllocatedWeight", 36.23m, fullWrapper.UnAllocatedWeight);
			AssertEquals("fullWrapper.UnAllocatedVolume", 35.11m, fullWrapper.UnAllocatedVolume);
			AssertEquals("fullWrapper.UnAllocatedPackages", 33, fullWrapper.UnAllocatedPackages);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress\nAUSTRALIA", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress\nAUSTRALIA", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress\nAUSTRALIA", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.PickupLocation", "USLKJ", fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", "AUZZZ", fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);
			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder.CompanyNameAndAddress", "TEST SENDING FORWARDER\r\nTEST SENDING ADDRESS\nAUSTRALIA", fullWrapper.SendingForwarder.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ReceivingForwarder.CompanyNameAndAddress", "TEST IMPORT AGENT\nTEST IMPORT ADDRESS\nAUSTRALIA", fullWrapper.ReceivingForwarder.CompanyNameAndAddress);
		}

		#endregion

		#region TestWrapperMappingFullDeclaration

		[TestDate(2007, 1, 1)]
		public void TestWrapperMappingFullDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "MBL12345678";
			declaration.JE_OH_ShippingLine = GetOrgHeader("TEST SHIPPING LINE").PK;
			declaration.JE_OH_Forwarder = GetOrgHeader("TEST FORWARDER").PK;
			declaration.Branch.GB_OH_OrgProxy = GetOrgHeader("TEST ORG PROXY EXPORT BROKER").PK;
			declaration.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("TEST NOTIFY PARTY").MainAddress.PK;
			JobDocAddress declarationPickup = declaration.SupplierPickupAddress;
			declarationPickup.E2_AddressOverride = true;
			declarationPickup.E2_CompanyName = "TEST PICKUP COMPANY";
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "ABX";
			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 5, 5);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 5, 6);
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 5, 7);
			declaration.JE_RL_NKFinalDestination = "NZCHC";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 5, 8);
			declaration.JE_TotalNoOfPacks = 48;
			declaration.JE_TotalNoOfPacksPackType = "BG";
			declaration.JE_MarksAndNumbers = "TEST DECLARATION MARKS AND NUMBERS";
			declaration.JE_OwnerRef = "OWNERS REFERENCE";
			declaration.JE_HouseBill = "HBLTESTTHIS";
			declaration.HouseBillIssuedDate = new ZDateTime(2006, 5, 8);
			declaration.JE_DeliveryOrPickupLabourCharge = 18.00m;
			declaration.JE_DeliveryOrPickupLabourTime = ZDateTime.Now.AddHours(22).AddMinutes(10);
			declaration.JE_PickupOrDeliveryTruckWaitCharge = 34.78m;
			declaration.JE_PickupOrDeliveryTruckWaitTime = ZDateTime.Now.AddHours(33).AddMinutes(45);

			declaration.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 8;

			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeCarrier;

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			cartage.JJ_ParentID = declaration.PK;

			FreightWrapperFromCartage fullWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("fullWrapper.ShipmentContainerMode", Core.Constants.ContainerModes.FCL, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode", Core.Constants.TransportModes.Sea, fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.OrderTransportMode", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.IncoTerm", Core.Constants.IncoTerms.FreeCarrier, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "CCX - Collect", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.CaratagePickupMode", "ABX", fullWrapper.CaratagePickupMode.Code);
			AssertEquals("fullWrapper.MasterBill", "MBL12345678", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.Carrier.CompanyName", "TEST SHIPPING LINE", fullWrapper.Carrier.CompanyName);
			AssertEquals("fullWrapper.ExportAgent", "TEST FORWARDER", fullWrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportBroker", "TEST ORG PROXY EXPORT BROKER", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.ImportAgent", ZString.Empty, fullWrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportBroker", ZString.Empty, fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.LocalForwarder", "TEST FORWARDER", fullWrapper.LocalForwarder.CompanyName);
			AssertEquals("fullWrapper.NotifyParty", "TEST NOTIFY PARTY", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent", ZString.Empty, fullWrapper.DeliveryAgent.CompanyName);
			AssertEquals("fullWrapper.PickupAgent", ZString.Empty, fullWrapper.PickupAgent.CompanyName);
			AssertEquals("fullWrapper.PickupAddress", "TEST PICKUP COMPANY", fullWrapper.PickupAddress.CompanyName);
			AssertEquals("fullWrapper.ShipmentDateCreated", ZDateTime.Empty, fullWrapper.ShipmentDateCreated);
			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USDNV", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "NZCHC", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Destination.ActualDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.ActualDate);
			AssertEquals("fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero", ZString.Empty, fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.StorgeTime", "8 Hours", fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", ZString.Empty, fullWrapper.GoodsValue.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.MarksAndNumbers", "TEST DECLARATION MARKS AND NUMBERS", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.OwnerReference", "OWNERS REFERENCE", fullWrapper.OwnerReference);
			AssertEquals("fullWrapper.HouseBill", "HBLTESTTHIS", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.HBLIssueDate", new ZDateTime(2006, 5, 8), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.HBLContainerMode", Core.Constants.ContainerModes.LCL, fullWrapper.HBLContainerMode);
			AssertEquals("fullWrapper.PickupLabourCharge", 18.00m, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", "22:10", fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", 34.78m, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", "33:45", fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.ArrivalReference", ZString.Empty, fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", ZString.Empty, fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.GoodsAvailableAt", ZString.Empty, fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder", "TEST FORWARDER", fullWrapper.SendingForwarder.CompanyName);
			AssertEquals("fullWrapper.ReceivingForwarder", ZString.Empty, fullWrapper.ReceivingForwarder.CompanyName);

			CustomsIncoTermOverrideCollection collection = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.Value;
			collection.AddNew(Core.Constants.IncoTerms.FreeCarrier, Core.Constants.IncoTerms.FreeAlongsideShip);
			DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			fullWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("fullWrapper.IncoTerm", Core.Constants.IncoTerms.FreeAlongsideShip, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "CCX - Collect", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
		}

		#endregion

		#region TestWrapperMappingImportDeclaration

		public void TestWrapperMappingImportDeclaration()
		{
			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "WHY ME";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			OrgAddress depotAddress = Factory.New<OrgAddress>();
			depotAddress.OA_Address1 = "TEST 3 ADDRESS";
			depotAddress.OA_RN_NKCountryCode = "AU";
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTO.Addresses[0].PK;

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			cartage.JJ_ParentID = declaration.PK;

			FreightWrapperFromCartage fullWrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("fullWrapper.UnpackCFSAddress.Address", "TEST 3 ADDRESS\nAUSTRALIA", fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.GoodsAvailableAt", "TEST 3 ADDRESS\nAUSTRALIA", fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "WHY ME", fullWrapper.CTOArrival.CompanyName);
		}

		#endregion

		#region TestWrapperNotes

		public override void TestWrapperNotes()
		{
			TestCartage.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "TEST DANGEROUS GOODS ADDITIONAL HANDLING INFORMATION");
			TestCartage.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "TEST CERTIFICATE OF ORIGIN NOTE");
			TestCartage.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "TEST PRE-ALERT ARRIVAL NOTICE REMARKS");

			FreightWrapperFromCartage noteWrapper = new FreightWrapperFromCartage(TestCartage, Factory);
			AssertEquals("noteWrapper.DangerousGoodsAdditionalHandlingInformation", "TEST DANGEROUS GOODS ADDITIONAL HANDLING INFORMATION", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("noteWrapper.CertificateOfOriginNotes", "TEST CERTIFICATE OF ORIGIN NOTE", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("noteWrapper.PreAlertArrivalNoticeRemarks", "TEST PRE-ALERT ARRIVAL NOTICE REMARKS", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		#endregion

		#region TestOrderLines

		public void TestOrderLines()
		{
			var firstCartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(firstCartage, Factory);

			AssertEquals(0, wrapper.Orders.Count);
			AssertEquals(0, wrapper.OrderLines.Count);

			var order1 = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			var orderLine2 = order1.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;

			var order2 = Factory.NewWithValidTestData<Order>();
			var orderLine3 = order2.OrderLines.AddNew();
			orderLine3.JO_LineNo = 3;

			var order3 = Factory.NewWithValidTestData<Order>();
			var orderLine4 = order3.OrderLines.AddNew();
			orderLine4.JO_LineNo = 4;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.AttachedOrders.Add(order1);
			shipment.AttachedOrders.Add(order2);

			var secondCartage = Factory.New<CommonCartage>();
			secondCartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			secondCartage.JJ_ParentID = shipment.PK;

			wrapper = new FreightWrapperFromCartage(secondCartage, Factory);

			AssertEquals(2, wrapper.Orders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, wrapper.OrderLines.Cast<OrderLineWrapper>().Select(x => (int)x.LineNumber));

			shipment.AttachedOrders.RemoveAllFromRelationship();

			wrapper = new FreightWrapperFromCartage(secondCartage, Factory);
			AssertEquals(0, wrapper.Orders.Count);
			AssertEquals(0, wrapper.OrderLines.Count);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.AttachedOrders.Add(order3);

			secondCartage = Factory.New<CommonCartage>();
			secondCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			secondCartage.JJ_ParentID = declaration.PK;

			wrapper = new FreightWrapperFromCartage(secondCartage, Factory);
			AssertEquals(1, wrapper.Orders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 4 }, wrapper.OrderLines.Cast<OrderLineWrapper>().Select(x => (int)x.LineNumber));

			declaration.AttachedOrders.RemoveAllFromRelationship();

			wrapper = new FreightWrapperFromCartage(secondCartage, Factory);
			AssertEquals(0, wrapper.Orders.Count);
			AssertEquals(0, wrapper.OrderLines.Count);
		}

		#endregion

		#region TestTransportAddresses

		public void TestTransportAddresses()
		{
			CartageBehaviorStrategyProvider.SetProvider(Factory, (CartageBehaviorStrategyProvider)Activator.CreateInstance(ObjectFactory.GetType<ICartageBehaviorStrategyProvider>()));

			var cartage = CartageHelper.CreateCartage(Core.Constants.CartageJobType.NEW_StagingCTOtoCFStoCNEtoCYD, 2);
			cartage.FirstDocAddress.E2_OA_Address = CartageHelper.CreateOrgHeader("CTO", "addy").PK;
			cartage.SecondDocAddress.E2_OA_Address = CartageHelper.CreateOrgHeader("CFS", "addy").PK;
			cartage.ThirdDocAddress.E2_OA_Address = CartageHelper.CreateOrgHeader("CNE", "addy").PK;
			cartage.FourthDocAddress.E2_OA_Address = CartageHelper.CreateOrgHeader("CYD", "addy").PK;

			var miscAddress = CartageHelper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageMSC, "misc", "addy", "2000", "city", "AUSYD", true);
			cartage.GetBookedMoves(cartage.Containers.First())[0].CartageLegs[0].JU_E2WaitPointAddressID = miscAddress.PK;

			var cartageWrapper = FreightWrapper.New(cartage, Factory)[0];
			AssertContainsExactElementsInAnyOrder(new[] { cartage.FirstDocAddress, cartage.SecondDocAddress, cartage.ThirdDocAddress, cartage.FourthDocAddress }, cartageWrapper.TransportAddresses.Cast<AddressWrapper>().Select(a => a.WrappedObject));

			var containerCartageWrapper = FreightWrapper.New(cartage, cartage.Containers.First(), Factory)[0];
			AssertContainsExactElementsInAnyOrder(new[] { cartage.FirstDocAddress, miscAddress, cartage.SecondDocAddress, cartage.ThirdDocAddress, cartage.FourthDocAddress }, containerCartageWrapper.TransportAddresses.Cast<AddressWrapper>().Select(a => a.WrappedObject));
		}

		#endregion

		#region TestLocalTransportLegs

		public void TestLocalTransportLegs()
		{
			var commonCartage = Factory.New<CommonCartage>();
			commonCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.DomesticLooseDelivery;
			var move1 = commonCartage.BookedMovesCollection.AddNew();
			var legA = move1.CartageLegs.AddNew();
			var legB = move1.CartageLegs.AddNew();
			var legC = move1.CartageLegs.AddNew();

			legA.JU_DisplayOrder = 1;
			legB.JU_DisplayOrder = 3;
			legC.JU_DisplayOrder = 2;

			var wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			AssertEquals(legA, wrapper.LocalTransportLegs[0].LegBO);
			AssertEquals(legC, wrapper.LocalTransportLegs[1].LegBO);
			AssertEquals(legB, wrapper.LocalTransportLegs[2].LegBO);
		}

		#endregion

		#region TestILegsOverrider_SetLegs

		public void TestILegsOverrider_SetLegs()
		{
			var commonCartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			commonCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.DomesticContainerizedDelivery;
			var containerisedMove1 = commonCartage.BookedMovesCollection.AddNew();
			var containerisedMove2 = commonCartage.BookedMovesCollection.AddNew();

			var containerisedCartageLeg1 = containerisedMove1.CartageLegs.AddNew();
			var containerisedCartageLeg2 = containerisedMove1.CartageLegs.AddNew();
			var containerisedCartageLeg3 = containerisedMove1.CartageLegs.AddNew();
			var containerisedCartageLeg4 = containerisedMove1.CartageLegs.AddNew();

			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg3)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg4)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(containerisedMove1));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(containerisedMove2));

			var cartageWrapper = (ILegsOverrider)wrapper;
			cartageWrapper.SetLegs(null, true, true, false);
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg3)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg4)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(containerisedMove1));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(containerisedMove2));
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartage), wrapper.CartageInfo.WrappedObject.GetType());

			cartageWrapper.SetLegs(Array.Empty<CommonCartageLeg>(), true, true, false);
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg3)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg4)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(containerisedMove1));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(containerisedMove2));
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartage), wrapper.CartageInfo.WrappedObject.GetType());

			cartageWrapper.SetLegs(new CommonCartageLeg[] { containerisedCartageLeg2, containerisedCartageLeg3 }, true, true, false);
			AssertEquals(false, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg3)));
			AssertEquals(false, (wrapper.LocalTransportLegs.ContainsWrappedObject(containerisedCartageLeg4)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(containerisedMove1));
			AssertEquals(false, wrapper.Packages.ContainsWrappedObject(containerisedMove2));
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartageLeg), wrapper.CartageInfo.WrappedObject.GetType());

			commonCartage = Factory.New<CommonCartage>();
			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			commonCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLImport;
			var move1 = commonCartage.LooseBookedMoves.AddNew();
			var move2 = commonCartage.LooseBookedMoves.AddNew();
			var move3 = commonCartage.LooseBookedMoves.AddNew();

			var cartageLeg1 = move1.CartageLegs.AddNew();
			var cartageLeg2 = move1.CartageLegs.AddNew();
			var cartageLeg3 = move2.CartageLegs.AddNew();
			var cartageLeg4 = move2.CartageLegs.AddNew();
			var cartageLeg5 = move3.CartageLegs.AddNew();

			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg3)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg4)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg5)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move1));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move2));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move3));

			cartageWrapper = wrapper;
			cartageWrapper.SetLegs(null, true, true, false);
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg3)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg4)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg5)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move1));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move2));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move3));
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartage), wrapper.CartageInfo.WrappedObject.GetType());

			cartageWrapper.SetLegs(Array.Empty<CommonCartageLeg>(), true, true, false);
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg3)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg4)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg5)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move1));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move2));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move3));
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartage), wrapper.CartageInfo.WrappedObject.GetType());

			cartageWrapper.SetLegs(new CommonCartageLeg[] { cartageLeg2, cartageLeg3 }, true, true, false);
			AssertEquals(false, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg1)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg2)));
			AssertEquals(true, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg3)));
			AssertEquals(false, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg4)));
			AssertEquals(false, (wrapper.LocalTransportLegs.ContainsWrappedObject(cartageLeg5)));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move1));
			AssertEquals(true, wrapper.Packages.ContainsWrappedObject(move2));
			AssertEquals(false, wrapper.Packages.ContainsWrappedObject(move3));
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartageLeg), wrapper.CartageInfo.WrappedObject.GetType());
		}

		#endregion

		#region TestILegsOverrider_SetLooseLegs

		public void TestILegsOverrider_SetLooseLegs()
		{
			var commonCartage = CartageHelper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLPackLooseFromSHP, 2);
			var wrapper = new FreightWrapperFromCartage(commonCartage, Factory);

			var containerLeg1 = commonCartage.GetBookedMoves(commonCartage.Containers.ElementAt(0))[0].CartageLegs[0];
			var containerLeg2 = commonCartage.GetBookedMoves(commonCartage.Containers.ElementAt(1))[0].CartageLegs[0];
			var looseLeg1 = commonCartage.LooseBookedMoves[0].CartageLegs[0];
			var looseLeg2 = commonCartage.LooseBookedMoves[1].CartageLegs[0];

			var cartageWrapper = (ILegsOverrider)wrapper;
			cartageWrapper.SetLegs(null, false, true, true);
			AssertEquals(true, wrapper.LocalTransportLegs.ContainsWrappedObject(containerLeg1));
			AssertEquals(true, wrapper.LocalTransportLegs.ContainsWrappedObject(containerLeg2));
			AssertEquals(true, wrapper.LocalTransportLegs.ContainsWrappedObject(looseLeg1));
			AssertEquals(true, wrapper.LocalTransportLegs.ContainsWrappedObject(looseLeg2));
			AssertEquals(2, wrapper.Containers.Count);
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartage), wrapper.CartageInfo.WrappedObject.GetType());

			cartageWrapper.SetLegs(new[] { looseLeg1 }, false, true, true);
			AssertEquals(false, wrapper.LocalTransportLegs.ContainsWrappedObject(containerLeg1));
			AssertEquals(false, wrapper.LocalTransportLegs.ContainsWrappedObject(containerLeg2));
			AssertEquals(true, wrapper.LocalTransportLegs.ContainsWrappedObject(looseLeg1));
			AssertEquals(false, wrapper.LocalTransportLegs.ContainsWrappedObject(looseLeg2));
			AssertEquals(0, wrapper.Containers.Count);
			AssertEquals(typeof(CartageInfoWrapperFromIDocCartageAdvice), wrapper.CartageInfo.GetType());
			AssertEquals(typeof(DocCommonCartage), wrapper.CartageInfo.WrappedObject.GetType());
		}

		#endregion

		#region TestShipmentOuterPacksQty()

		public void TestShipmentOuterPacksQty()
		{
			var commonCartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			commonCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLImport;
			commonCartage.JJ_OuterPacks = 10;
			commonCartage.JJ_F3_NKPackType = Core.Constants.PkgUnit.Box;

			AssertEquals("Since we didn't have overridden legs packsqty should be retrieved from the common cartage.", new ZDecimal(10), wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("Since we didn't have overridden legs packsqty type should be retrieved from the common cartage.", Core.Constants.PkgUnit.Box, wrapper.ShipmentOuterPacksQty.Unit.Code);

			CommonBookedCtgMove move1 = commonCartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move2 = commonCartage.LooseBookedMoves.AddNew();

			CommonCartageLeg cartageLeg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg2 = move2.CartageLegs.AddNew();

			move1.EW_F3_NKPackType = Core.Constants.PkgUnit.Box;
			move1.EW_BookedPackCount = 2;
			move2.EW_F3_NKPackType = Core.Constants.PkgUnit.Box;
			move2.EW_BookedPackCount = 4;

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("If there are overridden legs, we should retrieve pack count from booked moves.", new ZDecimal(6), wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("If there are overridden legs, we should retrieve pack type from booked moves.", Core.Constants.PkgUnit.Box, wrapper.ShipmentOuterPacksQty.Unit.Code);

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			move2.EW_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("When the package types are different we should calculate pack count as pieces.", new ZDecimal(6), wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("When the package types are different we should calculate pack type as pieces.", Core.Constants.PkgUnit.Piece, wrapper.ShipmentOuterPacksQty.Unit.Code);
		}

		#endregion

		#region TestWeight

		public void TestWeight()
		{
			var commonCartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			commonCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLImport;
			commonCartage.JJ_Weight = 100;
			commonCartage.JJ_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("If there are no overridden legs, weight should be from common cartageleg.", new ZDecimal(100), wrapper.Weight.Value);
			AssertEquals("If there are no overridden legs, weight unit should be from common cartageleg.", Core.Constants.Weight.Kilograms, wrapper.Weight.Unit.Code);

			var move1 = commonCartage.LooseBookedMoves.AddNew();
			var move2 = commonCartage.LooseBookedMoves.AddNew();
			var cartageLeg1 = move1.CartageLegs.AddNew();
			var cartageLeg2 = move2.CartageLegs.AddNew();

			move1.EW_BookedWeight = 100;
			move1.EW_WeightUQ = Core.Constants.Weight.Grams;
			move2.EW_BookedWeight = 200;
			move2.EW_WeightUQ = Core.Constants.Weight.Grams;

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("Since there are overridden legs and weight units are same, total weight should be in the same units.", new ZDecimal(300), wrapper.Weight.Value);
			AssertEquals("Since there are overridden legs and wight units are same, total weight should be in the same units.", Core.Constants.Weight.Grams, wrapper.Weight.Unit.Code);

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			move2.EW_WeightUQ = Core.Constants.Weight.Kilograms;
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("Since weight units are different, total weight should be calculated after converting them to units in common cartage.", new ZDecimal(200.100), wrapper.Weight.Value);
			AssertEquals("Since weight units are different, total weight should be in the same units as in the common cartage.", Core.Constants.Weight.Kilograms, wrapper.Weight.Unit.Code);

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			move2.EW_WeightUQ = Core.Constants.Weight.Pounds;
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false); //200lbs=90.9kg
			AssertEquals("Since weight units are different, total weight should be calculated after converting them to units in common cartage.", new ZDecimal(90.8), wrapper.Weight.Value);
			AssertEquals("Since weight units are different, total weight should be calculated after converting them to units in common cartage.", Core.Constants.Weight.Kilograms, wrapper.Weight.Unit.Code);

			Env.Registry.FreightWeightUnit = Core.Constants.Weight.Pounds;
			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("Since weight units are different, total weight should be calculated after converting them to units in common cartage.", new ZDecimal(200.2), wrapper.Weight.Value);
			AssertEquals("Since weight units are different, total weight should be calculated after converting them to units in common cartage.", Core.Constants.Weight.Pounds, wrapper.Weight.Unit.Code);

			move2.EW_WeightUQ = Core.Constants.Weight.Grams;
			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("Since there are overridden legs and weight units are same, total weight should be in the same units.", new ZDecimal(300), wrapper.Weight.Value);
			AssertEquals("Since there are overridden legs and wight units are same, total weight should be in the same units.", Core.Constants.Weight.Grams, wrapper.Weight.Unit.Code);
		}

		#endregion

		#region TestPackageVolume

		public void TestPackageVolume()
		{
			var commonCartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			commonCartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLImport;
			commonCartage.JJ_Volume = 100;
			commonCartage.JJ_VolumeUQ = Core.Constants.Volume.Litre;
			AssertEquals("If there are no overridden legs, volume should be from common cartageleg.", new ZDecimal(100), wrapper.Volume.Value);
			AssertEquals("If there are no overridden legs, volume unit should be from common cartageleg.", Core.Constants.Volume.Litre, wrapper.Volume.Unit.Code);

			CommonBookedCtgMove move1 = commonCartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move2 = commonCartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg2 = move2.CartageLegs.AddNew();

			move1.EW_BookedVolume = 100;
			move1.EW_VolumeUQ = Core.Constants.Volume.CubicMetres;
			move2.EW_BookedVolume = 200;
			move2.EW_VolumeUQ = Core.Constants.Volume.CubicMetres;

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("Since there are overridden legs and volume units are same, total volume should be in the same units.", new ZDecimal(300), wrapper.Volume.Value);
			AssertEquals("Since there are overridden legs and volume units are same, total volume should be in the same units.", Core.Constants.Volume.CubicMetres, wrapper.Volume.Unit.Code);

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			move2.EW_VolumeUQ = Core.Constants.Volume.Litre;
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false);
			AssertEquals("Since weight units are different, total volume should be calculated after converting them to units in common cartage.", new ZDecimal(100200), wrapper.Volume.Value);
			AssertEquals("Since weight units are different, total volume should be in the same units as in the common cartage.", Core.Constants.Volume.Litre, wrapper.Volume.Unit.Code);

			wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			move2.EW_VolumeUQ = Core.Constants.Volume.CubicFeet;
			((ILegsOverrider)wrapper).SetLegs(new CommonCartageLeg[] { cartageLeg1, cartageLeg2 }, true, true, false); //200lbs=90.9kg
			AssertEquals("Since weight units are different, total volume should be calculated after converting them to units in common cartage.", new ZDecimal(105663.369), wrapper.Volume.Value);
			AssertEquals("Since weight units are different, total volume should be calculated after converting them to units in common cartage.", Core.Constants.Volume.Litre, wrapper.Volume.Unit.Code);
		}

		#endregion

		#region TestConsignor

		public void TestConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = consignor.PK;
			shipment.FillWithValidTestData();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			cartage.FillWithValidTestData();
			Factory.Save();

			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertNotNull("There should be a consignor for shipment.", shipment.Consignor);
			AssertNotNull("Cartage wrapper should return consignor from shipment.", wrapper.Consignor.Organisation);
			AssertEquals("Since there is a shipment for the cartage, consignor should be from shipment.", shipment.Consignor, wrapper.Consignor.Organisation);

			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertNotNull("There should be a consignor for shipment.", shipment.Consignor);
			AssertNotNull("Wrapper should return consignor from shipment.", wrapper.Consignor.Organisation);
			AssertEquals("Since there is a shipment for the cartage, consignor should be from shipment.", shipment.Consignor, wrapper.Consignor.Organisation);
		}

		#endregion

		#region TestConsignee

		public void TestConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.FillWithValidTestData();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			cartage.FillWithValidTestData();
			Factory.Save();

			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertNotNull("There should be a consignor for shipment.", shipment.Consignee);
			AssertNotNull("Cartage wrapper should return consignor from shipment.", wrapper.Consignee.Organisation);
			AssertEquals("Since there is a shipment for the cartage, consignor should be from shipment.", shipment.Consignee, wrapper.Consignee.Organisation);

			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertNotNull("There should be a consignor for shipment.", shipment.Consignee);
			AssertNotNull("Wrapper should return consignor from shipment.", wrapper.Consignee.Organisation);
			AssertEquals("Since there is a shipment for the cartage, consignor should be from shipment.", shipment.Consignee, wrapper.Consignee.Organisation);
		}

		#endregion

		#region TestDocTypeCode

		public void TestDocTypeCode()
		{
			var cartage = Factory.New<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);

			((IDocTypeCode)wrapper).DocTypeCode = "CAD";
			AssertEquals("CAD", ((IDocTypeCode)wrapper).DocTypeCode);

			((IDocTypeCode)wrapper).DocTypeCode = "CAR";
			AssertEquals("CAR", ((IDocTypeCode)wrapper).DocTypeCode);
		}

		#endregion

		#region TestBarcodeTextForFont

		protected override void SetJobNumberForBarcodeTesting(BusinessObject bizO)
		{
			var cartage = (CommonCartage)bizO;
			cartage.JJ_ConsignmentID = "T1";
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^TRN=T1;CAD;|KÊ";
		}

		#endregion

		#region TestShipmentInnerPackQtyForQuotedBooking

		public void TestShipmentInnerPackQtyForQuotedBooking()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var bookingForShipment = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			var commonCartage = Factory.New<CommonCartage>();
			commonCartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			commonCartage.JJ_ParentID = shipment.PK;
			var wrapper = new FreightWrapperFromCartage(commonCartage, Factory);
			AssertNoExceptionThrown(() => { var qty = wrapper.ShipmentInnerPacksQty; });
		}

		#endregion

		#region TestFullHandlingInstructions

		public void TestFullHandlingInstructions()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with no care");
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("Handle with no care", wrapper.FullHandlingInstructions);
		}

		#endregion

		#region TestFullCartageInstructions

		public void TestFullCartageInstructions()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Pick it up");
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			AssertEquals("Pick it up", wrapper.FullCartageInstructions);
		}

		#endregion

		#region TestCartageCustomsEntries

		public void TestCartageCustomsEntries()
		{
			var cartage = Factory.New<CommonCartage>();
			var additionalRef = cartage.AdditionalReferenceNumbers.AddNew();
			additionalRef.CE_EntryType = TransportAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			additionalRef.CE_EntryNum = "XXXTESTXXX";
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);

			AssertEquals(1, wrapper.CustomsEntries.Count);
			AssertEquals(TransportAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, wrapper.CustomsEntries[0].EntryType.Code);
			AssertEquals("XXXTESTXXX", wrapper.CustomsEntries[0].EntryNumber);
		}

		#endregion

		public void TestParentJobNoParent()
		{
			var cartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);

			AssertEquals("ParentJob should be a FreightWrapperFromShipment", typeof(FreightWrapperFromShipment), wrapper.ParentJob.GetType());
			AssertEquals("ParentJob should be a null wrapper", true, wrapper.ParentJob.BusinessObjectForPrintJob.IsNull);
			AssertEquals("SecondaryHeading should be empty string", string.Empty, wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber should be empty string", string.Empty, wrapper.SecondaryNumber);
		}

		public void TestParentJobTransportBooking()
		{
			var cartage = Factory.New<CommonCartage>();
			var dtbBooking = (BusinessObject)Factory.New<IDtbBooking>();
			dtbBooking.FillWithValidTestData();
			var dtbBookingConsolidation = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			dtbBookingConsolidation.FillWithValidTestData();
			dtbBooking[DtbBookingSchema.KM_KB_Booking] = dtbBookingConsolidation.PK;
			dtbBooking[DtbBookingSchema.KM_JobID] = "TB99999";
			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = dtbBooking.TablePrefix;
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			Factory.Save();

			AssertEquals("ParentJob should point to DtbBooking", dtbBooking.PK, wrapper.ParentJob.WrappedObjectPK);
			AssertEquals("SecondaryHeading should be 'Transport Booking'", "Transport Booking", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber should be booking job ID", dtbBooking[DtbBookingSchema.KM_JobID], wrapper.SecondaryNumber);
		}

		public void TestParentJobForwardingShipment()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.FillWithValidTestData();
			forwardingShipment.JS_IsForwardRegistered = true;
			forwardingShipment.JS_IsCFSRegistered = false;
			forwardingShipment.JS_IsCancelled = false;
			forwardingShipment.JS_UniqueConsignRef = "S99999";
			cartage.JJ_ParentID = forwardingShipment.PK;
			cartage.JJ_ParentTableCode = forwardingShipment.TablePrefix;
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			Factory.Save();

			AssertEquals("ParentJob should point to ForwardingShipment", forwardingShipment.PK, wrapper.ParentJob.WrappedObjectPK);
			AssertEquals("SecondaryHeading should be 'Shipment'", "Shipment", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber should be shipment unique consign ref", forwardingShipment.JS_UniqueConsignRef, wrapper.SecondaryNumber);
		}

		public void TestParentJobCFSShipment()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			var cfsLoadList = Factory.New<CFSLoadListConsol>();
			cfsLoadList.FillWithValidTestData();
			var cfsShipment = cfsLoadList.Shipments.AddNew();
			cfsShipment.FillWithValidTestData();
			cfsShipment.JS_IsForwardRegistered = false;
			cfsShipment.JS_IsCFSRegistered = true;
			cfsShipment.JS_IsCancelled = false;
			cfsShipment.JS_UniqueConsignRef = "C99999";
			cartage.JJ_ParentID = cfsShipment.PK;
			cartage.JJ_ParentTableCode = cfsShipment.TablePrefix;
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			Factory.Save();

			AssertEquals("ParentJob should point to CFSShipment", cfsShipment.PK, wrapper.ParentJob.WrappedObjectPK);
			AssertEquals("SecondaryHeading should be 'CFS Shipment'", "CFS Shipment", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber should be shipment unique consign ref", cfsShipment.JS_UniqueConsignRef, wrapper.SecondaryNumber);
		}

		public void TestParentJobCFSLoadListConsol()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			var cfsLoadList = Factory.New<CFSLoadListConsol>();
			cfsLoadList.FillWithValidTestData();
			cfsLoadList.JK_IsCFS = true;
			cfsLoadList.JK_IsForwarding = false;
			cfsLoadList.JK_IsCancelled = false;
			cfsLoadList.JK_UniqueConsignRef = "L99999";
			cartage.JJ_ParentID = cfsLoadList.PK;
			cartage.JJ_ParentTableCode = cfsLoadList.TablePrefix;
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			Factory.Save();

			AssertEquals("ParentJob should point to CFSLoadList", cfsLoadList.PK, wrapper.ParentJob.WrappedObjectPK);
			AssertEquals("SecondaryHeading should be 'Load List'", "Load List", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber should be load list unique consign ref", cfsLoadList.JK_UniqueConsignRef, wrapper.SecondaryNumber);
		}

		public void TestParentJobBaseJobDeclaration()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_DeclarationReference = "B99999";
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			Factory.Save();

			AssertEquals("ParentJob should point to JobDeclaration", declaration.PK, wrapper.ParentJob.WrappedObjectPK);
			AssertEquals("SecondaryHeading should be 'Brokerage'", "Brokerage", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber should be declaration reference", declaration.JE_DeclarationReference, wrapper.SecondaryNumber);
		}

		public void TestParentJobWhsOrder()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			var whsOrder = Factory.New<WhsOrder>();
			whsOrder.FillWithValidTestData();
			whsOrder.WD_DocketID = "O99999";
			cartage.JJ_ParentID = whsOrder.PK;
			cartage.JJ_ParentTableCode = whsOrder.TablePrefix;
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			Factory.Save();

			AssertEquals("ParentJob should point to WhsOrder", whsOrder.PK, wrapper.ParentJob.WrappedObjectPK);
			AssertEquals("SecondaryHeading should be 'Order Number'", "Order Number", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber should be order number", whsOrder.WD_DocketID, wrapper.SecondaryNumber);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "HBLContainerMode", "FCL" },
					{ "HouseBillHeading", string.Empty },
					{ "JobNumberHeading", "Port Transport" },
					{ "MasterBillHeading", "Waybill" },
					{ "NoCopyBills", "3" },
					{ "NoOriginalBills", "3" },
					{ "SecondaryHeading", "Shipment" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				// after much effort I found that as the CartageType.GetCartageType() now matches to the cartage even without JJ_ConsignmentID populated, this makes the cartage Import Address pick up the shipment's Import Address automatically, so I've adjusted what is expected here
				return @"
AssuredParty : ASSUREDPARTY SHIPMENT\nERITREA
Buyer : FRED'S NOT SO FLAT TYRES\nADDRESS 2\nCITY2 STATE2 PCODE2\nERITREA
CaratagePickupMode : ABX
Carrier : TEST SHIPPING LINE\n1 TEST SHIPPING LINE STREET\nAUSTRALIA
ChargeableWeight : 12.3 KG
ClaimsPayableBy : CLAIMSPAYABLEBY SHIPMENT\nERITREA
Consignor : TEST SUPPLIER\n1 TEST SUPPLIER STREET\nAUSTRALIA
ConsolContainerMode : LSE - Loose
ConsolCreditor : CREDITOR\n1 CREDITOR STREET\nAUSTRALIA
ConsolTransportMode : AIR - Air Freight
ConsolType : AGT - Agent
CTOArrival : I WANT TO DIE\n1 I WANT TO DIE STREET\nAUSTRALIA
DeliveryAgent : DELIVERY CARTAGE COMPANY\n1 DELIVERY CARTAGE COMPANY STREET\nAUSTRALIA
DeliveryLocation : AUZZZ
Destination : AUZZZ
ExportAgent : SENDING FORWARDER\n1 SENDING FORWARDER STREET\nAUSTRALIA
ExportBroker : TEST EXPORT BROKER\n1 TEST EXPORT BROKER STREET\nAUSTRALIA
FreightRate : 784.23 HKD
GoodsAvailableAt : UNPACK DEPOT\nTEST UNPACK DEPOT ADDRESS\nAUSTRALIA
GoodsValue : 723.98 AUD
ImportArrivalCTOAddress : I WANT TO DIE\n1 I WANT TO DIE STREET\nAUSTRALIA
ImportAgent : RECEIVING FORWARDER\n1 RECEIVING FORWARDER STREET\nAUSTRALIA
ImportBroker : TEST IMPORT BROKER\n1 TEST IMPORT BROKER STREET\nAUSTRALIA
IncoTerm : FOB - Free On Board
InspectionType : UNK - Unknown - No Security Measures Taken
InsuredBy : INSUREDBY SHIPMENT\nERITREA
LocalForwarder : RECEIVING FORWARDER\n1 RECEIVING FORWARDER STREET\nAUSTRALIA
NotifyParty : TEST NOTIFY PARTY\n1 TEST NOTIFY PARTY STREET\nAUSTRALIA
Origin : USLKJ - Lackland City
PickupAddress : TEST SUPPLIER\n1 TEST SUPPLIER STREET\nAUSTRALIA
PickupAgent : PICKUP CARTAGE COMPANY\n1 PICKUP CARTAGE COMPANY STREET\nAUSTRALIA
PickupCFSAddress : PICKUP DEPOT\nTEST PICKUP DEPOT ADDRESS\nAUSTRALIA
PickupLocation : USLKJ - Lackland City
ReceivingForwarder : RECEIVING FORWARDER\n1 RECEIVING FORWARDER STREET\nAUSTRALIA
ReleaseType : NXS
SendingForwarder : SENDING FORWARDER\n1 SENDING FORWARDER STREET\nAUSTRALIA
ServiceLevel : SLV
ShipmentContainerMode : FCL
ShipmentInnerPacksQty : 568 BOX
ShipmentOuterPacksQty : 87 LEG
ShipmentStatus : MSA
ShipmentTransportMode : SEA - Sea Freight
ShipmentType : IMP - Import
ShippedOnBoardType : SHP - Shipped
StorageTime : 134 Hours
SurveyReportParty : SURVEYREPORTPARTY SHIPMENT\nERITREA
UnpackCFSAddress : UNPACK DEPOT\nTEST UNPACK DEPOT ADDRESS\nAUSTRALIA
Volume : 23.560 CF
Weight : 46.2 T";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CommonCartage>();
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			var bo = (CommonCartage)WrappedBO;
			bo.JJ_ParentID = Factory.New<ForwardingShipment>().PK;
			bo.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var consol = ((ForwardingShipment)bo.CartageParent).Consols.AddNew();
			consol.JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			return new FreightWrapperFromCartage((CommonCartage)WrappedBO, Factory);
		}

		CommonCartage TestCartage
		{
			get
			{
				return fcartage ?? (fcartage = Factory.New<CommonCartage>());
			}
		}

		CommonCartage fcartage;

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader sendingForwarder = GetOrgHeader("SENDING FORWARDER", "SENDFORW", addMainAddressDetails: true);

			OrgHeader receivingForwarder = GetOrgHeader("RECEIVING FORWARDER", "RECVFORW", addMainAddressDetails: true);

			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "TEST PICKUP DEPOT ADDRESS";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			var pickupdepotOrg = GetOrgHeader("Pickup Depot", "PICKDPOT");
			pickupdepotAddress.OA_OH = pickupdepotOrg.PK;

			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST UNPACK DEPOT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			var unpackdepotOrg = GetOrgHeader("Unpack Depot", "UNPKDPOT");
			unpackdepotAddress.OA_OH = unpackdepotOrg.PK;

			OrgHeader creditor = GetOrgHeader("CREDITOR", "CREDITOR", addMainAddressDetails: true);

			OrgHeader cTO = GetOrgHeader("I WANT TO DIE", "CTO", addMainAddressDetails: true);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GetOrgHeader("TEST SENDING FORWARDER", "TESTSEND", addMainAddressDetails: true));
			consol.SetDefaultReceivingForwarderAddress(GetOrgHeader("TEST IMPORT AGENT", "TESTIMAG", addMainAddressDetails: true));
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;
			consol.SetDefaultShippingLineAddress(GetOrgHeader("TEST SHIPPING LINE", "TESTSHLN", addMainAddressDetails: true));
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);

			ForwardingShipment shipment = consol.Shipments.AddNew();

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY SHIPMENT";

			JobDocAddress buyerAddress = shipment.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S NOT SO FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 2";
			buyerAddress.E2_City = "CITY2";
			buyerAddress.E2_State = "STATE2";
			buyerAddress.E2_Postcode = "PCODE2";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY SHIPMENT";

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY SHIPMENT";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY SHIPMENT";

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_RL_NKOrigin = "USLKJ";
			shipment.JS_RL_NKDestination = "AUZZZ";
			shipment.JS_TotalPackageCount = 568;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.JS_OuterPacks = 87;
			shipment.JS_F3_NKPackType = "LEG";
			shipment.JS_GoodsValue = 723.98m;
			shipment.JS_RX_NKGoodsValueCurr = "AUD";
			shipment.JS_UnitFreightRate = 784.23m;
			shipment.JS_RX_NKFrtRateCurrency = "HKD";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("TEST SUPPLIER", "TESTSUPP", addMainAddressDetails: true).PK;
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;
			shipment.JS_ActualChargeable = 12.34m;
			shipment.JS_ShipmentStatus = "MSA";

			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 134;
			shipment.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERY CARTAGE COMPANY", "DELVCART", addMainAddressDetails: true).PK;
			shipment.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUP CARTAGE COMPANY", "PICKCART", addMainAddressDetails: true).PK;

			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "ABX";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("TEST NOTIFY PARTY", "TESTNOTF", addMainAddressDetails: true).MainAddress.PK;
			shipment.JS_OH_ExportBroker = GetOrgHeader("TEST EXPORT BROKER", "TESTEXP", addMainAddressDetails: true).PK;
			shipment.JS_OH_ImportBroker = GetOrgHeader("TEST IMPORT BROKER", "TESTIMP", addMainAddressDetails: true).PK;

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			cartage.JJ_RS_NKServiceLevel = "SLV";
			cartage.JJ_OuterPacks = 87;
			cartage.JJ_F3_NKPackType = "LEG";
			cartage.JJ_Weight = 46.23m;
			cartage.JJ_WeightUQ = "T";
			cartage.JJ_Volume = 23.56m;
			cartage.JJ_VolumeUQ = "CF";

			Factory.Save();

			// to get the empty values required from default tests
			cartage.JJ_ConsignmentID = ZString.Empty;
			shipment.JS_UniqueConsignRef = ZString.Empty;
			shipment.Logs.CancelAll();
			consol.JK_UniqueConsignRef = ZString.Empty;
			consol.Logs.CancelAll();

			return new FreightWrapperFromCartage(cartage, Factory);
		}

		LocalCartageTestHelper CartageHelper
		{
			get { return cartageHelper ?? (cartageHelper = new LocalCartageTestHelper(Factory)); }
		}

		LocalCartageTestHelper cartageHelper;

		#endregion
	}
}
