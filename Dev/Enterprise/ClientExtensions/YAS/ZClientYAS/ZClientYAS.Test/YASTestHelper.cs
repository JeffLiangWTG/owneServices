using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.YAS.Testing
{
	public class YASTestHelper : SharedTestHelper
	{
		public YASTestHelper(BusinessObjectFactory factory) : base(factory) { }

		public YASTestHelper() : base() { }

		#region Set Factory Items

		internal ForwardingConsol Consol1
		{
			get { return consol1 ?? (consol1 = GetTestConsol()); }
		}
		ForwardingConsol consol1;

		ForwardingConsol GetTestConsol()
		{
			ForwardingConsol populatedConsol = Factory.New<ForwardingConsol>();

			populatedConsol = GetPopulatedConsolHeader(populatedConsol);

			var vessel = RefVessel.LookupVesselByName("QANTAS", Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "QANTAS";
			}

			//Add one more transport
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = "AIR";
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			voyage1.JV_VoyageFlight = "QF099";
			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUBNE";
			origin1.JA_E_DEP = new ZDateTime(2003, 05, 1);
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUMEL";
			destination1.JB_E_ARV = new ZDateTime(2003, 5, 10);
			voyage1.GenerateSailings();
			JobSailing sailing1 = voyage1.Sailings[0];

			Transport populatedTransport1 = populatedConsol.Transports.AddNew();
			populatedTransport1.JW_IsLinked = true;
			populatedTransport1.JW_JX = sailing1.PK;
			//Add one more transport end

			AppendAirWayBillAndShipmentsToConsol(populatedConsol);

			Factory.Save();

			return populatedConsol;
		}

		ForwardingConsol GetPopulatedConsolHeader(ForwardingConsol populatedConsol)
		{
			populatedConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			populatedConsol.JK_MasterBillNum = "MASTERBILL";
			populatedConsol.JK_UniqueConsignRef = "C00001002";
			populatedConsol.JK_RL_NKDischargePort = "NZAKL";
			populatedConsol.JK_MasterBillIssueDate = new ZDateTime(2003, 04, 1);

			var vessel = RefVessel.LookupVesselByName("QANTAS", Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "QANTAS";
			}

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "QF081";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2003, 04, 5);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = new ZDateTime(2003, 4, 30);
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			Transport populatedTransport = populatedConsol.Transports[0];
			populatedTransport.JW_JX = sailing.PK;

			return populatedConsol;
		}

		void AppendAirWayBillAndShipmentsToConsol(ForwardingConsol populatedConsol)
		{
			populatedConsol.AutomaticallyUpdatePackLineContainers = false;
			populatedConsol.SetDefaultReceivingForwarderAddress(LoadingAgent);
			populatedConsol.JK_PrepaidCollect = "PPD";
			populatedConsol.SetDefaultReceivingForwarderAddress(ReceivingAgent);
			populatedConsol.JK_OverrideWaybillDefaults = true;

			populatedConsol.PopulateAWB();
			ExportAWBHeader awbHeader = populatedConsol.AWBHeader;

			awbHeader.EH_Currency = Core.Constants.CurrencyCodes.Australia;

			//rateline
			ExportAWBRateLine rateLine = awbHeader.AWBRateLines.AddNew();
			rateLine.ER_NoOfPiecesOrRCP = "109";
			rateLine.ER_RateClass = "Q";
			rateLine.ER_CommodityItemNumber = "BEER";
			rateLine.ER_ChargeableWeight = 21m;
			rateLine.ER_WeightInLBsOrKGs = "K";
			rateLine.ER_RateChargeOrDiscount = 0.23m;
			rateLine.ER_GrossWeight = 2.45m;
			rateLine.ER_LineCount = 1;
			rateLine.ER_Total = 39.3m;

			shipment1 = populatedConsol.Shipments.AddNew();
			Shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment1.JS_UniqueConsignRef = "S00001110";
			Shipment1.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			Shipment1.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			Shipment1.JS_RL_NKOrigin = "AUSYD";
			Shipment1.JS_RL_NKDestination = "USLAX";
			Shipment1.JS_E_DEP = new ZDateTime(2005, 04, 01);
			Shipment1.JS_E_ARV = new ZDateTime(2005, 05, 01);

			Shipment1.JS_HouseBill = "11111111";
			Shipment1.JS_UnitFreightRate = new ZDecimal(0.74);
			Shipment1.JS_RX_NKFrtRateCurrency = Core.Constants.CurrencyCodes.Australia;
			Shipment1.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.Australia;
			Shipment1.NotifyPartyDocumentaryAddress.OrganisationPK = NotifyParty.PK;

			Shipment1.JS_GoodsDescription = "PENCIL";
			Shipment1.JS_MarksAndNumbers = "MARKSANDNUMBERS";
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			Shipment1.JS_TotalPackageCount = 22;
			Shipment1.JS_ActualVolume = 10.5m;
			Shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			Shipment1.JS_ActualWeight = 2.5m;
			Shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			Shipment1.PopulateAWB();
			ForwardingPackLine packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_ActualVolume = 10.5m;
			packLine.JL_ActualWeight = 2.5m;
			packLine.JL_UnitOfDimension = Core.Constants.Volume.CubicMetres;
			packLine.JL_JS = Shipment1.PK;

			//Shipment 2
			ForwardingShipment shipment2 = populatedConsol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00002220";
			shipment2.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_E_DEP = new ZDateTime(2005, 04, 01);
			shipment2.JS_E_ARV = new ZDateTime(2005, 05, 01);

			shipment2.JS_HouseBill = "22222222";
			shipment2.JS_UnitFreightRate = new ZDecimal(0.74);
			shipment2.JS_RX_NKFrtRateCurrency = Core.Constants.CurrencyCodes.Australia;
			shipment2.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.Australia;
			shipment2.NotifyPartyDocumentaryAddress.OrganisationPK = NotifyParty.PK;

			shipment2.JS_GoodsDescription = "LOLLIES";
			shipment2.JS_MarksAndNumbers = "MARKSANDNUMBERS3";
			shipment2.JS_OuterPacks = 10;
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			shipment2.JS_TotalPackageCount = 33;
			shipment2.JS_ActualVolume = 10.5m;
			shipment2.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment2.JS_ActualWeight = 2.5m;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment2.JS_OverrideWaybillDefaults = true;
			shipment2.JS_RS_NKServiceLevel = "TSP";
			shipment2.PopulateAWB();
			shipment2.AWBHeader.EH_Currency = Core.Constants.CurrencyCodes.Australia;

			ForwardingPackLine packLine2 = Factory.New<ForwardingPackLine>();
			packLine2.JL_ActualVolume = 10.5m;
			packLine2.JL_ActualWeight = 2.5m;
			packLine2.JL_UnitOfDimension = Core.Constants.Volume.CubicMetres;
			packLine2.JL_JS = shipment2.PK;
		}

		internal ForwardingShipment Shipment1
		{
			get { return shipment1 ?? (shipment1 = Factory.NewWithValidTestData<ForwardingShipment>()); }
		}
		ForwardingShipment shipment1;

		internal void SetupOrganisations()
		{
			Consignee = Factory.New<OrgHeader>();
			Consignee.OH_Code = "BIGBIRD";
			Consignee.OH_FullName = "BigBird Incorporated Consignee";
			OrgAddress consigneeAddress = Consignee.Addresses[0];
			consigneeAddress.OA_CompanyNameOverride = "BigBird Incorporated Consignee";
			consigneeAddress.OA_Address1 = "Level 3";
			consigneeAddress.OA_Address2 = "1 Sesame St";
			consigneeAddress.OA_City = "Sydney";
			consigneeAddress.OA_State = "NSW";
			consigneeAddress.OA_Phone = "33333333";
			consigneeAddress.OA_Fax = "44444444";
			consigneeAddress.OA_PostCode = "3333";

			Consignor = Factory.New<OrgHeader>();
			Consignor.OH_Code = "COOKIEMAN";
			Consignor.OH_FullName = "CookieMan Incorporated Consignor";
			OrgAddress consignorAddress = Consignor.Addresses[0];
			consignorAddress.OA_Address1 = "2 Sesame St";
			consignorAddress.OA_Address2 = "";
			consignorAddress.OA_City = "Los Angeles";
			consignorAddress.OA_State = "CA";
			consignorAddress.OA_Phone = "55555555";
			consignorAddress.OA_Fax = "66666666";
			consignorAddress.OA_PostCode = "5555";
			consignorAddress.OA_RN_NKCountryCode = ZString.Empty;

			NotifyParty = Factory.New<OrgHeader>();
			NotifyParty.OH_Code = "BERT";
			NotifyParty.OH_FullName = "Bert Incorporated Notify Party";
			OrgAddress notifyPartyAddress = NotifyParty.Addresses[0];
			notifyPartyAddress.OA_Address1 = "3 Sesame St";
			notifyPartyAddress.OA_Address2 = "";
			notifyPartyAddress.OA_City = "Melbourne";
			notifyPartyAddress.OA_State = "VIC";
			notifyPartyAddress.OA_Phone = "77777777";
			notifyPartyAddress.OA_Fax = "88888888";

			LoadingAgent = Factory.New<OrgHeader>();
			LoadingAgent.OH_Code = "ERNIE";
			LoadingAgent.OH_FullName = "Ernie Incorporated Delivery Agent";
			OrgAddress loadingAgentAddress = LoadingAgent.Addresses[0];
			loadingAgentAddress.OA_Address1 = "4 Sesame St";
			loadingAgentAddress.OA_Address2 = "";
			loadingAgentAddress.OA_City = "Sydney";
			loadingAgentAddress.OA_State = "NSW";
			loadingAgentAddress.OA_Phone = "99999999";
			loadingAgentAddress.OA_Fax = "00000000";

			ReceivingAgent = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			ReceivingAgent.OH_Code = "RECAGT";
			OrgAddress address = ReceivingAgent.Addresses.AddNew();
			address.OA_Address1 = "blah";

			OrgCusCode gTN = ReceivingAgent.CustomsCodes.AddNew();
			gTN.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gTN.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;
			gTN.OK_CustomsRegNo = "ECUAGENTCODE";

			Factory.Save();
		}

		internal OrgHeader Consignee, Consignor, LoadingAgent, NotifyParty, ReceivingAgent;

		internal void SetuupPODImport()
		{
			SetupOrganisations();
			GetTestConsol();
			Shipment1.JS_HouseBill = "HouseBill";
		}

		internal ForwardingShipment CreateForwardingShipment(ZString houseBill)
		{
			SetupOrganisations();
			GetTestConsol();
			Shipment1.JS_HouseBill = houseBill;
			return Shipment1;
		}

		#endregion

		#region Set Registry Items
		#region Set Valid Registry Items

		internal void SetValidRegistryProofOfDeliveryInterface()
		{
			SetValidRegistryProofOfDeliveryInterfaceErrorNotificationGroup();
			rego.PODImportFolder = AddTempFolder(Path.Combine(Env.TempPath, "PODImport"));
		}

		void SetValidRegistryProofOfDeliveryInterfaceErrorNotificationGroup()
		{
			rego.PODEmailNotificationGroup = DummyEmailGroup.PK;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal ZString ExpectedMessageShowedForAutoCodeMap = "Question Would you like to match the organization {0} ({1}) to this CargoWise One Code ({2})?";

		public ZQuery QueryToGetOrgPattern(ZString ownerCode, ZGuid orgPK)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, ownerCode);
			result.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Organisation);
			result.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, GlbCompany.CurrentCompany.OrgProxy.PK);
			result.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, orgPK);

			return result;
		}

		public GlbGroup DummyEmailGroup
		{
			get
			{
				if (dummyEmailGroup == null)
				{
					dummyEmailGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);

					if (dummyEmailGroup.Staff.Count == 0)
					{
						GlbStaff currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
						dummyEmailGroup.Staff.Add(currentStaff);
					}
					dummyEmailGroup.Staff[0].GS_EmailAddress = "teststaff@cargowise.com";
					Factory.Save();
				}
				return dummyEmailGroup;
			}
		}
		GlbGroup dummyEmailGroup;

		internal string AddTempFolder(string folderFullName)
		{
			TempTestFolders.Add(folderFullName);
			if (!Directory.Exists(folderFullName))
			{
				Directory.CreateDirectory(folderFullName);
			}
			return folderFullName;
		}

		#endregion

		static YASDataRegistry rego
		{
			get { return YASDataRegistry.Instance; }
		}

		#endregion

		public ForwardingShipment GetExportShipment(ZString transportMode)
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = "S1010101";
			shipment.JS_HouseBillOfLadingType = "ITP";
			shipment.ConsigneePK = Buyer.PK;
			shipment.ConsignorPK = Supplier.PK;
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2009, 12, 12);
			shipment.ConsignorPK = FindOrCreateOrgHeader("YASORGPRXY").PK;

			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_MasterBillNum = "MAWB101010";
			return shipment;
		}

		public ForwardingShipment ExportShipmentWithAWBHeader
		{
			get
			{
				if (exportShipmentWithAWBHeader == null)
				{
					exportShipmentWithAWBHeader = GetExportShipment(Core.Constants.TransportModes.Air);

					ForwardingConsol consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					consol.JK_AgentType = Core.Constants.AgentType.Direct;
					consol.JK_RL_NKLoadPort = "AUSYD";
					consol.JK_RL_NKDischargePort = "NZAKL";

					exportShipmentWithAWBHeader = consol.Shipments.AddNew();
					exportShipmentWithAWBHeader.JS_HouseBill = "S1010101";
					exportShipmentWithAWBHeader.JS_GoodsDescription = "pencil\n box";
					Transport tran = consol.MostInterestingTransportForBinding[0];
					tran.JW_ETD = new ZDateTime(2009, 03, 24);
					tran.JW_TransportMode = Core.Constants.TransportModes.Air;
					tran.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
					tran.JW_VoyageFlight = "CX123";
					consol.MasterBillMAWB = "10000001";
					tran.JW_DepotCutOff = new ZDateTime(2009, 03, 23);

					exportShipmentWithAWBHeader.JS_INCO = "FOB";

					PackLine packline = exportShipmentWithAWBHeader.OuterPackLines.AddNew();
					packline.JL_ActualVolume = 0.34m;
					packline.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
					packline.JL_ActualWeight = 34m;
					packline.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
					packline.JL_PackageCount = 190;
					packline.JL_RH_NKCommodityCode = "BEER";

					exportShipmentWithAWBHeader.PopulateAWB();

					ExportAWBHeader header = exportShipmentWithAWBHeader.AWBHeader;

					header.EH_ShippingLoadAndCount = 108;
					exportShipmentWithAWBHeader.JS_OverrideWaybillDefaults = true;

					//notify party
					header.EH_AlsoNotifyAddress = "Notify addr 1";
					header.EH_AlsoNotifyAddress2 = "Notify addr 2";
					header.EH_AlsoNotifyContactCode = "FX";
					header.EH_AlsoNotifyContactDetail = "13-3433";
					header.EH_AlsoNotifyCountryCode = "AU";
					header.EH_AlsoNotifyName = "notify company";
					header.EH_AlsoNotifyPostCode = "1023";
					header.EH_AlsoNotifyState = "NSW";
					header.EH_AWBIssuePlace = "Sydney";

					//shipper
					header.EH_ReferenceNumber = consol.JK_UniqueConsignRef;
					header.EH_IsShipperOverriden = true;
					header.EH_ShipperOverride1 = "shipper name";
					header.EH_ShipperOverride2 = "shipper street line 1";
					header.EH_ShipperOverride3 = "shipper street line 2";
					header.EH_ShipperOverride4 = "shipper street line 3";
					header.EH_ShipperOverride5 = "shipper town";

					//consignee
					header.EH_ConsigneeAccount = "Consignee";
					header.EH_ConsigneeName = "consignee name";
					header.EH_ConsigneeAddress = "consignee co street";
					header.EH_ConsigneeAddress2 = "consignee addr 2";
					header.EH_ConsigneeContactCode = "TE";
					header.EH_ConsigneeContactDetail = "3939-2";
					header.EH_ConsigneeCountryCode = "NZ";

					header.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;

					header.EH_OptionalShippingInformation = "shipping info1";
					header.EH_OptionalShippingInformation2 = "shipping info2";
					header.EH_InsuranceValue = 1000m;
					header.EH_SpecialHandlingCode = "T1";
					header.EH_NetRateCode = "HYA3004";
					header.EH_TaxesCOL = 100.3m;
					header.EH_TaxesPPD = 203.345m;
					header.EH_To1st = "ACC";
					header.EH_By1st = "CX";
					header.EH_To2nd = "CHC";
					header.EH_By2nd = "BA";
					header.EH_By3rd = "CX";
					header.EH_To3rd = "AKL";
					header.EH_Booking1stFlight = "CX";
					header.EH_Booking1stFlightDate = "12";
					header.EH_Booking2ndCarrier = "BA";
					header.EH_Booking2ndFlight = "234";
					header.EH_Booking2ndFlightDate = "12";
					header.EH_ValuationCOL = 45.23m;
					header.EH_WeightPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
					header.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
					header.EH_AirportOfDepartureAndRequestRouteText = "SYDNEY";
					header.EH_AirportOfDestinationCode = "AKL";
					header.EH_AirportOfDestinationText = "AUCKLAND";
					header.EH_DeclaredValue = 1933m;
					header.EH_HouseDeclaredValueCurrency = Core.Constants.CurrencyCodes.Australia;
					header.EH_HouseCustomsValueCurrency = Core.Constants.CurrencyCodes.Australia;
					header.EH_CustomsValue = 339.3m;
					header.EH_AWBOriginCode = "SYD";
					header.EH_ShippersSignature = "CARRIER AGENT NAME";
					header.EH_ExtraShipperInfoLine1 = "shipper extra1";
					header.EH_ExtraShipperInfoLine2 = "shipper extra2";
					header.EH_ExtraCarrierInfoLine2 = "carrier info";
					header.EH_AWBIssueDate = new ZDateTime(2009, 01, 20);
					header.EH_Currency = Core.Constants.CurrencyCodes.Australia;
					header.EH_ChargesCode = "CC";
					header.EH_HandlingInformation = "handling";
					header.EH_OtherPPDCOL = "COL";
					header.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;

					ExportAWBAccountingInformation acctInfo = header.AWBAccountingInformations.AddNew();
					acctInfo.EA_InformationID = "GEN";
					acctInfo.EA_Information = "akckc";

					//other charges
					ExportAWBOtherCharges otherCharge = header.AWBOtherCharges.AddNew();
					otherCharge.EO_ChargeCode = "AW";
					otherCharge.EO_EntitlementCode = "A";
					otherCharge.EO_Amount = 120M;

					otherCharge = header.AWBOtherCharges.AddNew();
					otherCharge.EO_ChargeCode = "XY";
					otherCharge.EO_EntitlementCode = "C";
					otherCharge.EO_Amount = 130M;

					otherCharge = header.AWBOtherCharges.AddNew();
					otherCharge.EO_ChargeCode = "AB";
					otherCharge.EO_EntitlementCode = "C";
					otherCharge.EO_Amount = 140M;

					otherCharge = header.AWBOtherCharges.AddNew();
					otherCharge.EO_ChargeCode = "QW";
					otherCharge.EO_EntitlementCode = "E";
					otherCharge.EO_Amount = 190.12M;
					otherCharge.EO_ChargeDescription = "QW Desc";
					otherCharge.EO_PPDCLT = "CLT";

					//rateline
					ExportAWBRateLine rateLine = header.AWBRateLines.AddNew();
					rateLine.ER_NoOfPiecesOrRCP = "109";
					rateLine.ER_RateClass = "Q";
					rateLine.ER_CommodityItemNumber = "BEER";
					rateLine.ER_ChargeableWeight = 2.45m;
					rateLine.ER_WeightInLBsOrKGs = "K";
					rateLine.ER_RateChargeOrDiscount = 0.23m;
					rateLine.ER_GrossWeight = 2.45m;
					rateLine.ER_LineCount = 1;
					rateLine.ER_Total = 39.3m;

					Factory.Save();
				}
				return exportShipmentWithAWBHeader;
			}
		}
		ForwardingShipment exportShipmentWithAWBHeader;

		internal void TidyUp()
		{
			foreach (string folderFullName in TempTestFolders)
			{
				foreach (string fileName in Directory.GetFiles(folderFullName))
				{
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
				}
				if (folderFullName != Env.TempPath && Directory.Exists(folderFullName))
				{
					Directory.Delete(folderFullName);
				}
			}
		}

		#region Test File Resources
		internal static class TestFiles
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
			internal static readonly string Folder = TestCase.BaseSourcePath + folder;
			const string folder = @"Enterprise\ClientExtensions\YAS\ZClientYAS\ZClientYAS.Test\";

			internal static class ProofOfDelivery
			{
				internal static readonly string Folder = TestFiles.Folder + folder;
				const string folder = @"Business\ProofOfDeliveryInterface\TestFiles\";

				internal static readonly string CorrectSample = ProofOfDelivery.Folder + correctSample;
				const string correctSample = "POD Test Sample2 (correct format).csv";
			}
		}
		#endregion

		internal List<String> TempTestFolders
		{
			get { return tempTestFolders ?? (tempTestFolders = new List<string>()); }
		}
		List<String> tempTestFolders;

		internal NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;
	}
}
