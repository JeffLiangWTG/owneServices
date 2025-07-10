using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Customs.Universal.Constants;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineItem = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItem;
using CusTempStorageRegLineItemPivot = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot;
using CusTempStorageRegLineTransaction = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction;
using CusTempStorageRegLineTransactionStatusList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList;
using CusTempStorageRegLineTransactionTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeader))]
sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestAllowMixedCaseAuthorisationNumbers()
	{
		configurationTestContext.DisableConfiguration(x => x.AllowMixedCaseAuthorisationNumbers);
		AssertEquals("AllowMixedCaseAuthorisationNumbers", false, departureMovement.AllowMixedCaseAuthorisationNumbers);

		configurationTestContext.EnableConfiguration(x => x.AllowMixedCaseAuthorisationNumbers);
		AssertEquals("AllowMixedCaseAuthorisationNumbers", true, departureMovement.AllowMixedCaseAuthorisationNumbers);
	}

	public void TestMessages()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var messages = departureMovement.Messages;

		AssertEquals(departureMovement, messages.Master);
		AssertSame(messages, departureMovement.Messages);
	}

	public void TestDisplayMessages()
	{
		departureMovement.Messages.AddNew();
		departureMovement.Header.Messages.AddNew();

		AssertEquals("When there are messages on both header and movementheader level, they should both be in the collection", 2, departureMovement.MessagesForDisplay.Count);
	}

	public void TestIsSeaInlandTransport()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals("When BM_ExportTransportMode = AIR, IsSeaInlandTransport", false, departureMovement.IsSeaInlandTransport);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals("When BM_ExportTransportMode = SEA, IsSeaInlandTransport", true, departureMovement.IsSeaInlandTransport);
		});
	}

	public void TestIsRailInlandTransport()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals("When BM_ExportTransportMode = AIR, IsRailInlandTransport", false, departureMovement.IsRailInlandTransport);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals("When BM_ExportTransportMode = RAIL, IsRailInlandTransport", true, departureMovement.IsRailInlandTransport);
		});
	}

	public void TestIsRoadInlandTransport()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals("When BM_ExportTransportMode = AIR, IsRoadInlandTransport", false, departureMovement.IsRoadInlandTransport);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("When BM_ExportTransportMode = ROAD, IsRoadInlandTransport", true, departureMovement.IsRoadInlandTransport);
		});
	}

	public void TestIsAirInlandTransport()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("When BM_ExportTransportMode = ROAD, IsAirInlandTransport", false, departureMovement.IsAirInlandTransport);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals("When BM_ExportTransportMode = AIR, IsAirInlandTransport", true, departureMovement.IsAirInlandTransport);
		});
	}

	public void TestIsPostalConsignmentInlandTransport()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("When BM_ExportTransportMode = ROAD, IsAirInlandTransport", false, departureMovement.IsPostalConsignmentInlandTransport);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
			AssertEquals("When BM_ExportTransportMode = POSTAL, IsPostalConsignmentInlandTransport", true, departureMovement.IsPostalConsignmentInlandTransport);
		});
	}

	public void TestIsFixedTransportInstallationsInlandTransport()
	{
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		AssertEquals("When BM_ExportTransportMode = ROAD, IsOwnPropulsionInlandTransport", false, departureMovement.IsFixedTransportInstallationsInlandTransport);
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		AssertEquals("When BM_ExportTransportMode = FIXEDTRANSPORTINSTALLATIONS, IsOwnPropulsionInlandTransport", true, departureMovement.IsFixedTransportInstallationsInlandTransport);
	}

	public void TestIsInlandWaterwayInlandTransport()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("When BM_ExportTransportMode = ROAD, IsAirInlandTransport", false, departureMovement.IsInlandWaterwayInlandTransport);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals("When BM_ExportTransportMode = INLANDWATERWAY, IsAirInlandTransport", true, departureMovement.IsInlandWaterwayInlandTransport);
		});
	}

	public void TestIsOwnPropulsionInlandTransport()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("When BM_ExportTransportMode = ROAD, IsOwnPropulsionInlandTransport", false, departureMovement.IsOwnPropulsionInlandTransport);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals("When BM_ExportTransportMode = OWNPROPULSION, IsOwnPropulsionInlandTransport", true, departureMovement.IsOwnPropulsionInlandTransport);
		});
	}

	public void TestIsSecurityTypeENTOrNON()
	{
		departureMovement.BM_TypeOfSecurity = "ENT";
		Assert("ENT", departureMovement.IsSecurityTypeENTOrNON);
		departureMovement.BM_TypeOfSecurity = "NON";
		Assert("NON", departureMovement.IsSecurityTypeENTOrNON);
		departureMovement.BM_TypeOfSecurity = "BTH";
		Assert("BTH", !departureMovement.IsSecurityTypeENTOrNON);
	}

	public void TestIsSecurityTypeENTOrBTH()
	{
		departureMovement.BM_TypeOfSecurity = "ENT";
		Assert("ENT", departureMovement.IsSecurityTypeENTOrBTH);
		departureMovement.BM_TypeOfSecurity = "BTH";
		Assert("BTH", departureMovement.IsSecurityTypeENTOrBTH);
		departureMovement.BM_TypeOfSecurity = "NON";
		Assert("NON", !departureMovement.IsSecurityTypeENTOrBTH);
	}

	public void TestIsSecurityTypeBTHOrEXI()
	{
		departureMovement.BM_TypeOfSecurity = "BTH";
		Assert("BTH", departureMovement.IsSecurityTypeBTHOrEXI);
		departureMovement.BM_TypeOfSecurity = "EXI";
		Assert("EXI", departureMovement.IsSecurityTypeBTHOrEXI);
		departureMovement.BM_TypeOfSecurity = "NON";
		Assert("NON", !departureMovement.IsSecurityTypeBTHOrEXI);
		departureMovement.BM_TypeOfSecurity = "ENT";
		Assert("ENT", !departureMovement.IsSecurityTypeBTHOrEXI);
	}

	public void TestIsSecurityTypeENTOrBTHOrEXI()
	{
		CombineAssertions("For different values of BM_TypeOfSecurity", () =>
		{
			departureMovement.BM_TypeOfSecurity = "ENT";
			AssertEquals("ENT", true, departureMovement.IsSecurityTypeENTOrBTHOrEXI);

			departureMovement.BM_TypeOfSecurity = "BTH";
			AssertEquals("BTH", true, departureMovement.IsSecurityTypeENTOrBTHOrEXI);

			departureMovement.BM_TypeOfSecurity = "EXI";
			AssertEquals("EXI", true, departureMovement.IsSecurityTypeENTOrBTHOrEXI);

			departureMovement.BM_TypeOfSecurity = "NON";
			AssertEquals("NON", false, departureMovement.IsSecurityTypeENTOrBTHOrEXI);
		});
	}

	public void TestIsSecurityTypeNONOrENT()
	{
		CombineAssertions("For different values of BM_TypeOfSecurity", () =>
		{
			departureMovement.BM_TypeOfSecurity = "ENT";
			AssertEquals("ENT", true, departureMovement.IsSecurityTypeNONOrENT);

			departureMovement.BM_TypeOfSecurity = "BTH";
			AssertEquals("BTH", false, departureMovement.IsSecurityTypeNONOrENT);

			departureMovement.BM_TypeOfSecurity = "EXI";
			AssertEquals("EXI", false, departureMovement.IsSecurityTypeNONOrENT);

			departureMovement.BM_TypeOfSecurity = "NON";
			AssertEquals("NON", true, departureMovement.IsSecurityTypeNONOrENT);
		});
	}

	public void TestBM_RN_NKTOLCarrierNationality_Caption()
	{
		AssertEquals("Nationality", DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_RN_NKTOLCarrierNationalityInfo).Caption);
	}

	public void TestInlandTransportList()
	{
		AssertType<InlandTransportCollection>(departureMovement.InlandTransportList);
	}

	public void TestICusCodeDataTypeSupporter()
	{
		ICusCodeDataTypeSupporter supporter = departureMovement;
		AssertEquals(typeof(InlandTransport), supporter.GetCusCodeDataTypes()[NctsConstants.CusCodeDataTypes.TransportInland]);
	}

	public void TestBM_RL_NKDestinationPortReadOnly()
	{
		AssertEquals(false, departureMovement.BM_RL_NKDestinationPortInfo.ReadOnly);

		var goodItem = departureMovement.GoodsItems.AddNew();
		AssertEquals(false, departureMovement.BM_RL_NKDestinationPortInfo.ReadOnly);

		goodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
		AssertEquals(true, departureMovement.BM_RL_NKDestinationPortInfo.ReadOnly);

		departureMovement.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Taiwan;
		AssertEquals(false, departureMovement.BM_RL_NKDestinationPortInfo.ReadOnly);
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(NctsMoveHeaderType.Codes.Departure, departureMovement.BM_SubApplicationCode);
		AssertEquals(Weight.Kilograms, departureMovement.BM_GrossWeightUQ);
	}

	public void TestCustomsValueCaption()
	{
		AssertEquals("Value in EUR", departureMovement.CustomsValueCaption);
	}

	public void TestGoodsItems()
	{
		AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(departureMovement.GoodsItems);
	}

	public void TestLookups_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsDepartureMovementHeaderPhase4Lookups>(departureMovement.Lookups);
	}

	public void TestLookups_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsDepartureMovementHeaderPhase5Lookups>(departureMovement.Lookups);
	}

	public void TestValidation_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		AssertType<NctsDepartureMovementHeaderPhase4Validation>(departureMovement.Validation);
	}

	public void TestValidation_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		AssertType<NctsDepartureMovementHeaderPhase5Validation>(departureMovement.Validation);
	}

	public void TestBM_CustomsStatus()
	{
		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly", true, departureMovement.BM_CustomsStatusInfo.ReadOnly);
			AssertEquals("Max Length", 3, departureMovement.BM_CustomsStatusInfo.MaxLength);
		});
	}

	public void TestBM_TransportAtDeparture_MaxLength()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Max Length (NCTS4)", 27, departureMovement.BM_TransportAtDepartureInfo.MaxLength);

			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			string[] differntBM_InlandTransportMode = { ModeOfTransportList.Codes._2_RailTransport, ModeOfTransportList.Codes._3_RoadTransport, ModeOfTransportList.Codes._4_AirTransport, ModeOfTransportList.Codes._7_FixedTransportInstallations, ModeOfTransportList.Codes._9_OwnPropulsion, ModeOfTransportList.Codes._1_SeaTransport };
			UniversalValidationHelperTest.AssertMaxLengthE1103(nctsHeader, departureMovement.BM_TransportAtDepartureInfo, true, differntBM_InlandTransportMode);
		});
	}

	public void TestBM_ForeignDestPortKCode_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_PlaceOfUnloadingInfo, NctsHeader.Phase4CaptionKey, "Place of Unloading Code", string.Empty, "Unloading", "Place of Unloading (code)");
	}

	public void TestBM_ForeignDestPortKCode_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_PlaceOfUnloadingInfo, NctsHeader.Phase5CaptionKey, "Place of Unloading", string.Empty, "Unloading");
	}

	public void TestBM_TransportAtDepartureTrailer1RegNo_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Trailer 1 ID", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Trailer 1", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "TRLR 1", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_TransportAtDepartureTrailer1RegNo_MaxLength()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Max Length (NCTS4)", 35, departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo.MaxLength);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			string[] differntBM_InlandTransportMode = { ModeOfTransportList.Codes._2_RailTransport, ModeOfTransportList.Codes._3_RoadTransport, ModeOfTransportList.Codes._1_SeaTransport };
			UniversalValidationHelperTest.AssertMaxLengthE1103(nctsHeader, departureMovement.BM_TransportAtDepartureTrailer1RegNoInfo, true, differntBM_InlandTransportMode);
		});
	}

	public void TestBM_AdditionalDeclarationType_Caption()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_AdditionalDeclarationTypeInfo
																	, NctsHeader.Phase5CaptionKey
																	, "Additional Declaration type"
																	, "Add. Decl. Type"
																	, "Add. Declaration Type"
																	, "11 02 001 000 Additional Declaration type");
	}

	public void TestBM_PresentationDateTime_Caption()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_PresentationDateTimeInfo
																	, NctsHeader.Phase5CaptionKey
																	, "Presentation Date and Time"
																	, "Presentation"
																	, "Presentation Date&Time"
																	, "15 08 000 000 Presentation Date and Time");
	}

	public void TestBM_RN_NKTransportAtDepartureTrailer1Nationality_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Trailer 1 Nationality", captionResourceString.Caption);
			AssertEquals("MediumCaption", "TRLR 1 Nationality", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Nationality", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_TransportAtDepartureTrailer2RegNo_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_TransportAtDepartureTrailer2RegNoInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Trailer 2 ID", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Trailer 2", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "TRLR 2", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_TransportAtDepartureTrailer2RegNo_MaxLength()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Max Length (NCTS4)", 35, departureMovement.BM_TransportAtDepartureTrailer2RegNoInfo.MaxLength);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			string[] differntBM_InlandTransportMode = { ModeOfTransportList.Codes._3_RoadTransport, ModeOfTransportList.Codes._1_SeaTransport };
			UniversalValidationHelperTest.AssertMaxLengthE1103(nctsHeader, departureMovement.BM_TransportAtDepartureTrailer2RegNoInfo, true, differntBM_InlandTransportMode);
		});
	}

	public void TestBM_RN_NKTransportAtDepartureTrailer2Nationality_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Trailer 2 Nationality", captionResourceString.Caption);
			AssertEquals("MediumCaption", "TRLR 2 Nationality", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Nationality", captionResourceString.ShortCaption);
		});
	}

	public void TestVesselNameAtDeparture()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_TransportAtDeparture = "XYZ";
			AssertEquals("Value from BM_TransportAtDeparture", "XYZ", departureMovement.VesselNameAtDeparture);

			departureMovement.VesselNameAtDeparture = "ABC";
			AssertEquals("Value set to BM_TransportAtDeparture", "ABC", departureMovement.BM_TransportAtDeparture);
		});
	}

	public void TestVesselNameAtDeparture_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.VesselNameAtDepartureInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Vessel Name", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Vessel", captionResourceString.ShortCaption);
		});
	}

	public void TestCheckVesselNameAtDeparture_MaxLength()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Max Length (NCTS4)", 35, departureMovement.VesselNameAtDepartureInfo.MaxLength);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			string[] differntBM_InlandTransportMode = { ModeOfTransportList.Codes._1_SeaTransport, ModeOfTransportList.Codes._2_RailTransport };
			UniversalValidationHelperTest.AssertMaxLengthE1103(nctsHeader, departureMovement.VesselNameAtDepartureInfo, true, differntBM_InlandTransportMode);
		});
	}

	public void TestVesselCountryAtDeparture()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_RN_NKTransportAtDepartureCountry = Core.Constants.CountryCodes.Germany;
			AssertEquals("Value from BM_RN_NKTransportAtDepartureCountry", Core.Constants.CountryCodes.Germany, departureMovement.VesselCountryAtDeparture);

			departureMovement.VesselCountryAtDeparture = Core.Constants.CountryCodes.Greece;
			AssertEquals("Value set to BM_RN_NKTransportAtDepartureCountry", Core.Constants.CountryCodes.Greece, departureMovement.BM_RN_NKTransportAtDepartureCountry);
		});
	}

	public void TestVesselCountryAtDeparture_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.VesselCountryAtDepartureInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Vessel Nationality", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Nationality", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_TransportAtDepartureType_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_TransportAtDepartureTypeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Type of Identification", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Type of ID", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_AircraftIDAtDeparture_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(departureMovement.BM_AircraftIDAtDepartureInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Aircraft Identification", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Aircraft ID", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_AircraftIDAtDeparture_MaxLength()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("Max Length (NCTS4)", 35, departureMovement.BM_AircraftIDAtDepartureInfo.MaxLength);

			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			string[] differntBM_InlandTransportMode = { ModeOfTransportList.Codes._4_AirTransport, ModeOfTransportList.Codes._1_SeaTransport };
			UniversalValidationHelperTest.AssertMaxLengthE1103(nctsHeader, departureMovement.BM_AircraftIDAtDepartureInfo, true, differntBM_InlandTransportMode);
		});
	}

	public void TestBM_CustomsStatus_Phase4Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_CustomsStatusInfo, NctsHeader.Phase4CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Departure Status", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Dep. St.", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_CustomsStatus_Phase5Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_CustomsStatusInfo, NctsHeader.Phase5CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Departure Status", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Dep. Status", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Dep. Stat.", captionResourceString.ShortCaption);
		});
	}

	public void TestCustomsStatusDescription()
	{
		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
		AssertEquals(NctsTransitStatusList.Descriptions.DeclarationGuaranteesNotValid, departureMovement.CustomsStatusDescription);
	}

	public void TestMessageStatusDescription()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
		AssertEquals(LogicalStatusList.Descriptions.Accepted, departureMovement.MessageStatusDescription);
	}

	public void TestMessageStatus_ReadOnly()
	{
		AssertEquals(true, departureMovement.BM_MessageStatusInfo.ReadOnly);
	}

	public void TestBM_LocationOfGoodsCode()
	{
		AssertEquals("Max Length", 17, departureMovement.BM_LocationOfGoodsCodeInfo.MaxLength);
	}

	public void TestBM_LocationOfGoodsCode_Phase5Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_LocationOfGoodsCodeInfo, NctsHeader.Phase5CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Location Code", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Loc.", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_LocationOfGoodsCode_Phase4Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_LocationOfGoodsCodeInfo, NctsHeader.Phase4CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Location Code", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Loc. Code", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Location of goods (code) (agreed or authorized)", captionResourceString.FullDescription);
		});
	}

	public void TestBM_UniqueConsignmentReference_Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_UniqueConsignmentReferenceInfo, "Reference Number / UCR", "Reference No. / UCR", "Ref. No. / UCR");
	}

	public void TestBM_UniqueConsignmentReference_Phase5DepartureCaption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_UniqueConsignmentReferenceInfo, NctsHeader.Phase5DepartureCaptionKey, "Reference Number / UCR", string.Empty, "Ref. No. / UCR", "Indicate the Reference Number / Unique Consignment Reference (UCR)");
	}

	public void TestBM_ExportDate_Phase5Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_ExportDateInfo, NctsHeader.Phase5CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Date Limit", captionResourceString.Caption);
			AssertEquals("ShortCaption", "Limit", captionResourceString.ShortCaption);
		});
	}

	public void TestBM_ExportDate_Phase4Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.BM_ExportDateInfo, NctsHeader.Phase4CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Control Result Date", captionResourceString.Caption);
			AssertEquals("FullDescription", "Date Limit of Control Result", captionResourceString.FullDescription);
		});
	}

	public void TestIsTIRDeclaration()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("Is TirDeclaration", true, departureMovement.IsTIRDeclaration);
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			AssertEquals("Not TirDeclaration", false, departureMovement.IsTIRDeclaration);
		});
	}

	public void TestIsMixedConsignment_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4;
			AssertEquals("Is MixedConsignment", true, departureMovement.IsMixedConsignment);
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("Not MixedConsignment", false, departureMovement.IsMixedConsignment);
		});
	}

	public void TestIsMixedConsignment_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
			AssertEquals("Is MixedConsignment", true, departureMovement.IsMixedConsignment);
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("Not MixedConsignment", false, departureMovement.IsMixedConsignment);
		});
	}

	public void TestSetMixedConsignment_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		departureMovement.SetMixedConsignment();
		AssertEquals(NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4, departureMovement.BM_InBondEntryType);
	}

	public void TestSetMixedConsignment_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.SetMixedConsignment();
		AssertEquals(NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5, departureMovement.BM_InBondEntryType);
	}

	public void TestIsInternalTransitProcedure()
	{
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			AssertEquals("Is IsInternalTransitProcedure", true, departureMovement.IsInternalTransitProcedure);
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
			AssertEquals("Not IsInternalTransitProcedure", false, departureMovement.IsInternalTransitProcedure);
		});
	}

	public void TestPreLodgedForAgreedLocationOfGoodsCode()
	{
		CombineAssertions(() =>
		{
			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = true;
			AssertEquals("Location Of Goods Code Set", "PRE-LODGED", departureMovement.BM_LocationOfGoodsCode);

			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = false;
			AssertEquals("Location Of Goods Code Cleared", ZString.Empty, departureMovement.BM_LocationOfGoodsCode);

			departureMovement.BM_LocationOfGoodsCode = "DANIEL";
			AssertEquals("String Not PRE-LODGED", false, departureMovement.PreLodgedForAgreedLocationOfGoodsCode);

			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = true;
			AssertEquals("PRE-LODGED string returned", "PRE-LODGED", departureMovement.BM_LocationOfGoodsCode);

			departureMovement.PreLodgedForAgreedLocationOfGoodsCode = false;
			AssertEquals("Returns field value", "DANIEL", departureMovement.BM_LocationOfGoodsCode);
		});
	}

	public void TestBM_LocationOfGoods()
	{
		AssertEquals("Max Length", 35, departureMovement.BM_LocationOfGoodsInfo.MaxLength);
	}

	public void TestBM_CustomsSubPlace()
	{
		AssertEquals("Max Length", 17, departureMovement.BM_CustomsSubPlaceInfo.MaxLength);
	}

	public void TestPlaceOfLoading()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Place Of Loading Code", ZString.Empty, departureMovement.PlaceOfLoading);
			departureMovement.BM_RL_NKForeignDestPort = "GBSVL";
			AssertEquals("No Place Of Loading Code", "Staunton in the V", departureMovement.PlaceOfLoading);
		});
	}

	public void TestBM_RL_NKForeignDestPort_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_RL_NKForeignDestPortInfo, NctsHeader.Phase5CaptionKey, "Place of Loading", string.Empty, "Loading");
	}

	public void TestBM_RL_NKForeignDestPort_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_RL_NKForeignDestPortInfo, NctsHeader.Phase4CaptionKey, "[27] Place of Loading Code", string.Empty, "Loading", "Place of Loading (code)");
	}

	public void TestBM_InlandTransportMode()
	{
		AssertEquals("Max Length", 2, departureMovement.BM_InlandTransportModeInfo.MaxLength);
	}

	public void TestBM_InlandTransportMode_ClearInlandTransportModeRelatedProperties_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		departureMovement.BM_TransportAtDeparture = "AA";
		departureMovement.BM_RN_NKTransportAtDepartureCountry = Core.Constants.CountryCodes.Germany;
		departureMovement.BM_TransportAtDepartureTrailer1RegNo = "BB";
		departureMovement.BM_RN_NKTransportAtDepartureTrailer1Nationality = Core.Constants.CountryCodes.Australia;
		departureMovement.BM_TransportAtDepartureTrailer2RegNo = "CC";
		departureMovement.BM_RN_NKTransportAtDepartureTrailer2Nationality = Core.Constants.CountryCodes.Italy;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._10;
		departureMovement.BM_AircraftIDAtDeparture = "DD";

		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals("BM_TransportAtDeparture not cleared", "AA", departureMovement.BM_TransportAtDeparture);
			AssertEquals("BM_RN_NKTransportAtDepartureCountry not cleared", Core.Constants.CountryCodes.Germany, departureMovement.BM_RN_NKTransportAtDepartureCountry);
			AssertEquals("BM_TransportAtDepartureTrailer1RegNo not cleared", "BB", departureMovement.BM_TransportAtDepartureTrailer1RegNo);
			AssertEquals("BM_RN_NKTransportAtDepartureTrailer1Nationality not cleared", Core.Constants.CountryCodes.Australia, departureMovement.BM_RN_NKTransportAtDepartureTrailer1Nationality);
			AssertEquals("BM_TransportAtDepartureTrailer2RegNo not cleared", "CC", departureMovement.BM_TransportAtDepartureTrailer2RegNo);
			AssertEquals("BM_RN_NKTransportAtDepartureTrailer2Nationality not cleared", Core.Constants.CountryCodes.Italy, departureMovement.BM_RN_NKTransportAtDepartureTrailer2Nationality);
			AssertEquals("BM_TransportAtDepartureType not cleared", NctsTransportTypeOfIdList.Codes._10, departureMovement.BM_TransportAtDepartureType);
			AssertEquals("BM_AircraftIDAtDeparture not cleared", "DD", departureMovement.BM_AircraftIDAtDeparture);
		});
	}

	public void TestBM_InlandTransportMode_ClearInlandTransportModeRelatedProperties_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		departureMovement.BM_TransportAtDeparture = "AA";
		departureMovement.BM_RN_NKTransportAtDepartureCountry = Core.Constants.CountryCodes.Germany;
		departureMovement.BM_TransportAtDepartureTrailer1RegNo = "BB";
		departureMovement.BM_RN_NKTransportAtDepartureTrailer1Nationality = Core.Constants.CountryCodes.Australia;
		departureMovement.BM_TransportAtDepartureTrailer2RegNo = "CC";
		departureMovement.BM_RN_NKTransportAtDepartureTrailer2Nationality = Core.Constants.CountryCodes.Italy;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._10;
		departureMovement.BM_AircraftIDAtDeparture = "DD";
		departureMovement.AdditionalWagons.AddNew();

		CombineAssertions(() =>
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals("BM_TransportAtDeparture cleared", ZString.Empty, departureMovement.BM_TransportAtDeparture);
			AssertEquals("BM_RN_NKTransportAtDepartureCountry cleared", ZString.Empty, departureMovement.BM_RN_NKTransportAtDepartureCountry);
			AssertEquals("BM_TransportAtDepartureTrailer1RegNo cleared", ZString.Empty, departureMovement.BM_TransportAtDepartureTrailer1RegNo);
			AssertEquals("BM_RN_NKTransportAtDepartureTrailer1Nationality cleared", ZString.Empty, departureMovement.BM_RN_NKTransportAtDepartureTrailer1Nationality);
			AssertEquals("BM_TransportAtDepartureTrailer2RegNo cleared", ZString.Empty, departureMovement.BM_TransportAtDepartureTrailer2RegNo);
			AssertEquals("BM_RN_NKTransportAtDepartureTrailer2Nationality cleared", ZString.Empty, departureMovement.BM_RN_NKTransportAtDepartureTrailer2Nationality);
			AssertEquals("BM_TransportAtDepartureType cleared", ZString.Empty, departureMovement.BM_TransportAtDepartureType);
			AssertEquals("BM_AircraftIDAtDeparture cleared", ZString.Empty, departureMovement.BM_AircraftIDAtDeparture);
			AssertEquals("AdditionalWagons cleared", 0, departureMovement.AdditionalWagons.Count);
		});
	}

	public void TestBM_InlandTransportMode_ClearInlandTransportModeRelatedProperties_AdditionalWagons()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_TransportAtDeparture = "AA";
		departureMovement.BM_RN_NKTransportAtDepartureCountry = Core.Constants.CountryCodes.Germany;
		departureMovement.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._21;
		departureMovement.AdditionalWagons.AddNew();
		departureMovement.AdditionalWagons.AddNew();

		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		AssertEquals("AdditionalWagons cleared", 0, departureMovement.AdditionalWagons.Count);
	}

	public void TestSaveB9_SeqNoInDetails()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);

		var bill1 = header.Bills.AddNew();
		bill1.B0_Weight = 1m;
		bill1.B0_WeightUQ = "LT";

		var bill2 = header.Bills.AddNew();
		bill2.B0_Weight = 2m;
		bill2.B0_WeightUQ = "LT";

		var bill3 = header.Bills.AddNew();
		bill3.B0_Weight = 3m;
		bill3.B0_WeightUQ = "LT";
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, SequenceNumber)",
												new (ZShort, ZString, ZDecimal, ZString)[]
												{
													(1, "1", 1m, "LT"),
													(2, "2", 2m, "LT"),
													(3, "3", 3m, "LT"),
												}, header.Bills.Select(x => (x.SequenceNumber, x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ)));
			bill2.Delete();
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, SequenceNumber)",
												new (ZShort, ZString, ZDecimal, ZString)[]
												{
													(1, "1", 1m, "LT"),
													(2, "2", 3m, "LT"),
												}, header.Bills.Select(x => (x.SequenceNumber, x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ)));
		});
	}

	public void TestBM_InlandTransportMode_RefreshBinding()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

		var bill = nctsHeader.Bills.AddNew();
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		CombineAssertions(() =>
		{
			AssertEquals("Bill InlandTransportModeDeparture", ModeOfTransportList.Codes._3_RoadTransport, bill.InlandTransportModeAtDeparture);

			var inlandTransportModeDepartureUpdated = false;
			bill.InlandTransportModeAtDepartureInfo.ValueChanged += (_, __) =>
			{
				inlandTransportModeDepartureUpdated = true;
			};

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals("Bill InlandTransportModeDeparture updated", true, inlandTransportModeDepartureUpdated);
		});
	}

	public void TestBM_InlandTransportMode_ClearBillValues()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

		var bill = nctsHeader.Bills.AddNew();
		var departureTransportInfos = bill.DepartureTransportInfos;
		var firstTransportMeans = departureTransportInfos.AddNew();
		firstTransportMeans.TPM_IdentificationNumber = "Transport ID";
		firstTransportMeans.TPM_RN_NKTransportNationality = "DE";
		departureTransportInfos.AddNew();
		departureTransportInfos.AddNew();
		departureTransportInfos.AddNew();
		var transportDepartureAdditionalWagonNumbers = bill.TransportDepartureAdditionalWagonNumbers;
		CombineAssertions(() =>
		{
			AssertEquals("Initial DepartureTransportInfos count", 4, departureTransportInfos.Count);
			AssertEquals("Initial TransportDepartureAdditionalWagonNumbers count", 3, transportDepartureAdditionalWagonNumbers.Count);
			AssertEquals("Initial FirstTransport ID", "Transport ID", bill.FirstDepartureTransportMeansID);
			AssertEquals("Initial Transport Nationality", "DE", bill.FirstDepartureTransportMeansNationality);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals("Same InlandTransportMode: DepartureTransportInfos count", 4, departureTransportInfos.Count);
			AssertEquals("Same InlandTransportMode: TransportDepartureAdditionalWagonNumbers count", 3, transportDepartureAdditionalWagonNumbers.Count);
			AssertEquals("Same InlandTransportMode: FirstTransport ID", "Transport ID", bill.FirstDepartureTransportMeansID);
			AssertEquals("Same InlandTransportMode: Transport Nationality", "DE", bill.FirstDepartureTransportMeansNationality);

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("InlandTransportMode changed: DepartureTransportInfos is empty", 0, departureTransportInfos.Count);
			AssertEquals("InlandTransportMode changed: TransportDepartureAdditionalWagonNumbers is empty", 0, transportDepartureAdditionalWagonNumbers.Count);
			AssertEquals("InlandTransportMode changed: First Transport ID empty", ZString.Empty, bill.FirstDepartureTransportMeansID);
			AssertEquals("InlandTransportMode changed: First Transport Nationality empty", ZString.Empty, bill.FirstDepartureTransportMeansNationality);
		});
	}

	public void TestBM_InlandTransportMode_Defaults_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_InlandTransportMode = "AB";
		AssertNullOrEmpty("DefaultCode Empty", departureMovement.BM_TransportAtDepartureType);
		departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		AssertEquals("DefaultCode XY", "10", departureMovement.BM_TransportAtDepartureType);
	}

	public void TestInlandTransportModeAtDeparture_Caption_Phase5()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.InlandTransportModeAtDepartureInfo
																	, NctsHeader.Phase5CaptionKey
																	, "Departure Transport Mode (Inland)"
																	, shortCaption: "Inland M.O.T."
																	, fullDescription: "[19 04 001 000] Inland Mode of Transport (Code)");
	}

	public void TestBM_ExportTransportMode()
	{
		AssertEquals("Max Length", 2, departureMovement.BM_ExportTransportModeInfo.MaxLength);
	}

	public void TestBM_ExportTransportMode_ClearExportTransportModeRelatedProperties_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
		departureMovement.BM_TOLCarrierID = "AA";
		departureMovement.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Germany;
		departureMovement.BM_ConveyanceNumber = "123";
		departureMovement.BM_CustomsOfficeAtBorder = "DE001";

		CombineAssertions(() =>
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals("BM_ActiveBorderIdentificationType not cleared", NctsTransportTypeOfIdList.Codes._10, departureMovement.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID not cleared", "AA", departureMovement.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality not cleared", Core.Constants.CountryCodes.Germany, departureMovement.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber not cleared", "123", departureMovement.BM_ConveyanceNumber);
			AssertEquals("BM_CustomsOfficeAtBorder not cleared", "DE001", departureMovement.BM_CustomsOfficeAtBorder);
		});
	}

	public void TestBM_ExportTransportMode_ClearExportTransportModeRelatedProperties_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
		departureMovement.BM_TOLCarrierID = "AA";
		departureMovement.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Germany;
		departureMovement.BM_ConveyanceNumber = "123";
		departureMovement.BM_CustomsOfficeAtBorder = "DE001";

		var additionalItem = departureMovement.AdditionalTransportAtBorderList.AddNew();
		additionalItem.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;

		CombineAssertions(() =>
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals("BM_ActiveBorderIdentificationType cleared", ZString.Empty, departureMovement.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID cleared", ZString.Empty, departureMovement.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality cleared", ZString.Empty, departureMovement.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber cleared", ZString.Empty, departureMovement.BM_ConveyanceNumber);
			AssertEquals("BM_CustomsOfficeAtBorder cleared", ZString.Empty, departureMovement.BM_CustomsOfficeAtBorder);
			AssertEquals("AdditionalTransportAtBorderList items deleted", 0, departureMovement.AdditionalTransportAtBorderList.Count);
			AssertEquals("AdditionalItem is deleted", true, additionalItem.IsDeleted);
		});
	}

	public void TestAdditionalTransportAtBorderListCount()
	{
		CombineAssertions(() =>
		{
			AssertEquals(0, departureMovement.AdditionalTransportAtBorderListCount);

			departureMovement.AdditionalTransportAtBorderList.AddNew();
			AssertEquals(1, departureMovement.AdditionalTransportAtBorderListCount);

			departureMovement.AdditionalTransportAtBorderList.AddNew();
			AssertEquals(2, departureMovement.AdditionalTransportAtBorderListCount);
		});
	}

	public void TestAdditionalTransportAtBorderListCountCaption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(
			departureMovement.AdditionalTransportAtBorderListCountInfo,
			expectedCaption: "Additional Border Num.",
			expectedMediumCaption: "",
			expectedShortCaption: "Add. Border Num.",
			expectedFullDescription: "Number of Additional Border MOT");
	}

	public void TestIAdditionalTransportMeansProvider_AdditionalTransportAtBorderList()
	{
		var provider = (IAdditionalTransportMeansProvider)departureMovement;
		AssertSame("AdditionalTransportAtBorderList", departureMovement.AdditionalTransportAtBorderList, provider.AdditionalTransportAtBorderList);
	}

	public void TestIAdditionalTransportMeansProvider_ValidateAdditionalTransportAtBorderListCount()
	{
		var provider = (IAdditionalTransportMeansProvider)departureMovement;
		var additionalTransportAtBorder = departureMovement.AdditionalTransportAtBorderList.AddNew();
		additionalTransportAtBorder.TPM_TypeOfIdentification = "10";
		additionalTransportAtBorder.Validation.ValidateAll();
		CombineAssertions("PRE-CONDITIONS", () =>
		{
			AssertEquals("AdditionalTransportAtBorder HasNotifications?", true, additionalTransportAtBorder.HasNotifications());
			AssertEquals("AdditionalTransportAtBorderListCountInfo HasNotifications?", false, departureMovement.AdditionalTransportAtBorderListCountInfo.HasNotifications());
		});

		departureMovement.Header.BH_ApplicationCode = "NCT";
		provider.ValidateAdditionalTransportAtBorderListCount();
		AssertEquals("POST-CONDITION [Phase4]: AdditionalTransportAtBorderListCountInfo HasNotifications?", false, departureMovement.AdditionalTransportAtBorderListCountInfo.HasNotifications());

		departureMovement.Header.BH_ApplicationCode = "NC5";
		provider.ValidateAdditionalTransportAtBorderListCount();
		AssertEquals("POST-CONDITION [Phase5]: AdditionalTransportAtBorderListCountInfo HasNotifications?", true, departureMovement.AdditionalTransportAtBorderListCountInfo.HasNotifications());
	}

	public void TestBM_TOLCarrierID_MaxLength()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Max Length (NCTS4)", 27, departureMovement.BM_TOLCarrierIDInfo.MaxLength);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Max Length (NCTS5)", 35, departureMovement.BM_TOLCarrierIDInfo.MaxLength);
		});
	}

	public void TestBM_TOLCarrierID_VesselPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.SeaVesselPhase5SeaExportTransportModeCaptionKey, caption: "Vessel Name", shortCaption: "Vessel");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._11;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Vessel Name", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_LloydsPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.LloydsPhase5SeaExportTransportModeCaptionKey, caption: "Lloyds Number", shortCaption: "Lloyds No.");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Lloyds Number", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_WagonPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.WagonPhase5SeaExportTransportModeCaptionKey, caption: "Wagon Number", shortCaption: "Wagon No.");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._20;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Wagon Number", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_TrainPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.TrainPhase5SeaExportTransportModeCaptionKey, caption: "Train Number", shortCaption: "Train No.");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Train Number", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_FlightPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.FlightPhase5SeaExportTransportModeCaptionKey, caption: "Flight Number", shortCaption: "Flight No.");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._40;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Flight Number", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_RegistrationPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.RegistrationPhase5SeaExportTransportModeCaptionKey, caption: "Registration Number", shortCaption: "Registration No.");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._41;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Registration Number", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_ENIPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.ENIPhase5SeaExportTransportModeCaptionKey, caption: "ENI Code");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._80;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "ENI Code", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_InlandWaterwayVesselPhase5SeaExportTransportModeCaptionKey()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_TOLCarrierIDInfo, multipleResourceKey: NctsDepartureMovementHeader.InlandWaterwayVesselPhase5SeaExportTransportModeCaptionKey, caption: "Vessel Name", shortCaption: "Vessel");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._81;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Vessel Name", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_TOLCarrierIDInfo, NctsHeader.Phase5CaptionKey, "Transport Identification", "Transport ID", "Transp. ID");
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "Transport Identification", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierID_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_TOLCarrierIDInfo, NctsHeader.Phase4CaptionKey, "[21] Transport ID (Frontier)", string.Empty, "Frontier ID", "Frontier Transport ID");
		AssertEquals("departureMovement.BM_TOLCarrierIDInfo.Description", "[21] Transport ID (Frontier)", departureMovement.BM_TOLCarrierIDInfo.Description);
	}

	public void TestBM_TOLCarrierCode()
	{
		AssertEquals("Max Length", 2, departureMovement.BM_TOLCarrierCodeInfo.MaxLength);
	}

	public void TestBM_BTAIndicator()
	{
		AssertEquals("Max Length", 1, departureMovement.BM_BTAIndicatorInfo.MaxLength);
	}

	public void TestBM_MethodOfPayment()
	{
		AssertEquals("Max Length", 1, departureMovement.BM_MethodOfPaymentInfo.MaxLength);
	}

	public void TestBM_MethodOfPayment_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_MethodOfPaymentInfo, NctsHeader.Phase4CaptionKey, "Transport Charges / Method of Payment", "", "MoP.", "Method of Payment of Transport Charges");
	}

	public void TestBM_MethodOfPayment_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_MethodOfPaymentInfo, NctsHeader.Phase5CaptionKey, "Transport Method of Payment", "Transport MoP", "Transp. MoP", "Method of Payment of Transport Charges");
	}

	public void TestBM_MethodOfPayment_ReadOnly_Phase4()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		CombineAssertions(() =>
		{
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals($"Type Of Security is ENT.", false, departureMovement.BM_MethodOfPaymentInfo.ReadOnly);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals($"Type Of Security is NON.", false, departureMovement.BM_MethodOfPaymentInfo.ReadOnly);
		});
	}

	public void TestBM_MethodOfPayment_ReadOnly_WhenC0186Active()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				AssertEquals($"Type Of Security is ENT.", false, departureMovement.BM_MethodOfPaymentInfo.ReadOnly);

				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals($"Type Of Security is NON.", true, departureMovement.BM_MethodOfPaymentInfo.ReadOnly);
			});
		}
	}

	public void TestBM_MethodOfPayment_ReadOnly_WhenC0186NotActive()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
		{
			AssertEquals(false, departureMovement.BM_MethodOfPaymentInfo.ReadOnly);
		}
	}

	public void TestBM_AdditionalText()
	{
		AssertEquals("Max Length", 70, departureMovement.BM_AdditionalTextInfo.MaxLength);
	}

	public void TestBM_ConveyanceNumber_Phase4MaxLength()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		AssertEquals("Max Length", 35, departureMovement.BM_ConveyanceNumberInfo.MaxLength);
	}

	public void TestBM_ConveyanceNumber_Phase5MaxLength()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		AssertEquals("Max Length", 17, departureMovement.BM_ConveyanceNumberInfo.MaxLength);
	}

	public void TestBM_ConveyanceNumber_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_ConveyanceNumberInfo, NctsHeader.Phase4CaptionKey, "Conveyance Reference No.", string.Empty, "Conv. Ref. No.", "Conveyance Reference Number");
	}

	public void TestBM_ConveyanceNumber_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_ConveyanceNumberInfo, NctsHeader.Phase5CaptionKey, "Conveyance Number", "Conveyance No.", "Conv. No.", "Conveyance Reference Number");
	}

	public void TestBM_CustomsOfficeAtBorder_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_CustomsOfficeAtBorderInfo, NctsHeader.Phase4CaptionKey, "Customs Office at Border", string.Empty, "Office at Border");
	}

	public void TestBM_CustomsOfficeAtBorder_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_CustomsOfficeAtBorderInfo, NctsHeader.Phase5CaptionKey, "Customs Office", string.Empty, "Office");
	}

	public void TestIsSimplifiedNctsProcedure_Phase5Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.IsSimplifiedNctsProcedureInfo, NctsHeader.Phase5CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Simplified Procedure", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Simplified Proc.", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Procedure", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Simplified Procedure for the Authorized Consignor", captionResourceString.FullDescription);
		});
	}

	public void TestIsSimplifiedNctsProcedure_Phase4Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.IsSimplifiedNctsProcedureInfo, NctsHeader.Phase4CaptionKey);
		AssertEquals("Caption", "Simplified Procedure (Authorized Consignor)", captionResourceString.Caption);
	}

	public void TestBM_GS_NKCusAgent()
	{
		AssertEquals("Max Length", 3, departureMovement.BM_GS_NKCusAgentInfo.MaxLength);
	}

	public void TestRepresentativeName()
	{
		CombineAssertions(() =>
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TEST STAFF USER";
			AssertEquals("Current User Fallback", "CargoWise Support", departureMovement.RepresentativeName);
			departureMovement.BM_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("Staff Name", "TEST STAFF USER", departureMovement.RepresentativeName);
		});
	}

	public void TestRepresentativeWhenRuleR0850Active()
	{
		const string errorMessage = "[R0850] Representative has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers / Codes";

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var representativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		representativeOrgHeader.CustomsCodes.RemoveAndDeleteAll();

		using (ValidationDeciderTestHelper.ActiveForDepartureMovementHeaderPhase5(Factory, v => v.IsRuleR0850Active))
		{
			departureMovement.Representative.OrganisationPK = representativeOrgHeader.PK;
			AssertHasMessageError("Does not have EORI or TCU code", departureMovement.Representative.OrganisationPKInfo, errorMessage);

			representativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A1234");
			departureMovement.Representative.Validation.ValidateOrganisationPK();
			AssertNoMessageError("Has EORI code", departureMovement.Representative.OrganisationPKInfo, errorMessage);

			representativeOrgHeader.CustomsCodes.RemoveAndDeleteAll();
			representativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "B12345");
			departureMovement.Representative.Validation.ValidateOrganisationPK();
			AssertNoMessageError("Has TCU code", departureMovement.Representative.OrganisationPKInfo, errorMessage);
		}
	}

	[ExpectNoExceptions]
	public void TestRepresentativeWhenRuleR0850Inactive()
	{
		const string errorMessage = "[R0850] Representative has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers / Codes";

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var representativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		departureMovement.Representative.OrganisationPK = representativeOrgHeader.PK;

		using (var context = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory).ClearCachedValidationDecider(departureMovement))
		{
			context.AssertNoNotifications(departureMovement.Representative.OrganisationPKInfo, errorMessage, v => v.IsRuleR0850Active, false, () =>
			{
				representativeOrgHeader.CustomsCodes.RemoveAndDeleteAll();
				departureMovement.Representative.Validation.ValidateOrganisationPK();
			});
		}
	}

	public void TestCheckRepresentative_NR0070()
	{
		const string messageError = "[NR0070] Representative must have EORI number if Additional Reference Code Y025 is used.";
		using (var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory))
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;

			deciderTestContext.ClearCachedValidationDecider(movementHeader);
			deciderTestContext.EnableRule(c => c.IsRuleNR0070Active);

			var orgWithEori = Factory.New<OrgHeader>();
			orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
			var orgWithoutEori = Factory.New<OrgHeader>();
			orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", CountryCodes.Italy);
			var representative = nctsHeader.MovementHeader.Representative;
			var propertyInfo = representative.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				representative.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError("No Y025, No Representative", propertyInfo, messageError);

				representative.OrganisationPK = orgWithoutEori.PK;
				AssertNoMessageError("No Y025, Has Representative (no eori)", propertyInfo, messageError);

				var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
				additionalReference.CSI_SubType = "REF";
				additionalReference.CSI_Code = "Y025";
				representative.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError("Has Y025, No Representative", propertyInfo, messageError);

				representative.OrganisationPK = orgWithoutEori.PK;
				AssertHasMessageError("Has Y025, Has Representative (no eori)", propertyInfo, messageError);

				representative.OrganisationPK = orgWithEori.PK;
				AssertNoMessageError("Has Y025, Has Representative (with eori)", propertyInfo, messageError);

				representative.OrganisationPK = orgWithoutEori.PK;
				deciderTestContext.DisableRule(c => c.IsRuleNR0070Active);
				representative.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Has Y025, Has Representative (no eori), Rule is not active", propertyInfo, messageError);
			});
		}
	}

	public void TestCheckCarrier_NR0072()
	{
		const string messageError = "[NR0072] Carrier must have EORI number if Additional Reference Code Y028 is used.";
		using (var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory))
		{
			deciderTestContext.EnableRule(c => c.IsRuleNR0072Active);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;

			deciderTestContext.ClearCachedValidationDecider(movementHeader);
			deciderTestContext.EnableRule(c => c.IsRuleNR0072Active);

			var orgWithEori = Factory.New<OrgHeader>();
			orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
			var orgWithoutEori = Factory.New<OrgHeader>();
			orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", CountryCodes.Italy);
			var carrier = nctsHeader.MovementHeader.Carrier;
			var propertyInfo = carrier.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				carrier.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError("No Y028, No Carrier", propertyInfo, messageError);

				carrier.OrganisationPK = orgWithoutEori.PK;
				AssertNoMessageError("No Y028, Has Carrier (no eori)", propertyInfo, messageError);

				var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
				additionalReference.CSI_SubType = "REF";
				additionalReference.CSI_Code = "Y028";
				carrier.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError("Has Y028, No Carrier", propertyInfo, messageError);

				carrier.OrganisationPK = orgWithoutEori.PK;
				AssertHasMessageError("Has Y028, Has Carrier (no eori)", propertyInfo, messageError);

				carrier.OrganisationPK = orgWithEori.PK;
				AssertNoMessageError("Has Y028, Has Carrier (with eori)", propertyInfo, messageError);

				carrier.OrganisationPK = orgWithoutEori.PK;
				deciderTestContext.DisableRule(c => c.IsRuleNR0072Active);
				carrier.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Has Y028, Has Carrier (no eori), Rule is not active", propertyInfo, messageError);
			});
		}
	}

	public void TestBM_GONumber()
	{
		AssertEquals("Max Length", 2, departureMovement.BM_GONumberInfo.MaxLength);
	}

	public void TestIsContainerised()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Goods Items", false, departureMovement.IsContainerised);
			var goodsItem = departureMovement.GoodsItems.AddNew();
			AssertEquals("No Containers", false, departureMovement.IsContainerised);
			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			container.BC_Seal1 = "SEAL1";
			container.BC_Seal2 = "SEAL2";
			AssertEquals("Container Not linked to Goods Item", false, departureMovement.IsContainerised);
			goodsItem.ContainersPivots[0].ContainerSelected = true;
			AssertEquals("Linked no Container Number", false, departureMovement.IsContainerised);
			container.BC_ContainerNum = "CONTAINER1";
			container.BC_Mode = "CNT";
			AssertEquals("Is Containerised", true, departureMovement.IsContainerised);
		});
	}

	public void TestIsContainerised_NotLinked()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			container.BC_Mode = "CNT";
			AssertEquals("Is Containerised", true, departureMovement.IsContainerised);
		});
	}

	public void TestIsContainerised_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		CombineAssertions(() =>
		{
			AssertEquals("No Goods Items", false, departureMovement.IsContainerised);
			var goodsItem = departureMovement.GoodsItems.AddNew();
			AssertEquals("No Containers", false, departureMovement.IsContainerised);
			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			container.BC_Seal1 = "SEAL1";
			container.BC_Seal2 = "SEAL2";
			AssertEquals("Container Not linked to Goods Item", false, departureMovement.IsContainerised);
			goodsItem.ContainersPivots[0].ContainerSelected = true;
			AssertEquals("Linked no Container Number", false, departureMovement.IsContainerised);
			container.BC_ContainerNum = "CONTAINER1";
			AssertEquals("Is Containerised", true, departureMovement.IsContainerised);
		});
	}

	public void TestTotalGrossMassInKilograms_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

		var bill1 = departureMovement.Header.Bills.AddNew();
		var bill2 = departureMovement.Header.Bills.AddNew();

		var goodsItem1 = bill1.GoodsItems.AddNew();
		goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
		var goodsItem2 = bill1.GoodsItems.AddNew();
		goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
		var goodsItem3 = bill1.GoodsItems.AddNew();
		goodsItem3.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
		var goodsItem4 = bill1.GoodsItems.AddNew();
		goodsItem3.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

		goodsItem1.BY_GrossWeight = 1001;
		goodsItem2.BY_GrossWeight = 1002;
		goodsItem3.BY_GrossWeight = 1003;
		goodsItem4.BY_GrossWeight = 1004;

		AssertEquals(1004461.045962m, departureMovement.TotalGrossMassInKilograms);
	}

	public void TestTotalGrossMassInKilograms_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;

		var goodsItem1 = departureMovement.GoodsItems.AddNew();
		var goodsItem2 = departureMovement.GoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 1001;
		goodsItem2.BY_GrossWeight = 1002;

		AssertEquals(2003m, departureMovement.TotalGrossMassInKilograms);
	}

	public void TestTotalNettMassInKilograms()
	{
		var goodsItem1 = departureMovement.GoodsItems.AddNew();
		goodsItem1.BY_NetWeight = 28;
		goodsItem1.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
		var goodsItem2 = departureMovement.GoodsItems.AddNew();
		goodsItem2.BY_NetWeight = 32;
		goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
		AssertEquals(60m, departureMovement.TotalNettMassInKilograms);
	}

	public void TestTirCarnetNumber_MaxLength_Phase4()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		AssertEquals(10, departureMovement.TirCarnetNumberInfo.MaxLength);
	}

	public void TestTirCarnetNumber_MaxLength_Phase5()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		AssertEquals(AutoCusEntryNum.Schema.CE_EntryNumMaxLength, departureMovement.TirCarnetNumberInfo.MaxLength);
	}

	public void TestTirCarnetNumber_Phase4Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.TirCarnetNumberInfo, NctsHeader.Phase4CaptionKey);
		AssertEquals("Caption", "TIR Carnet Num.", captionResourceString.Caption);
	}

	public void TestTirCarnetNumber_Phase5Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(departureMovement.TirCarnetNumberInfo, NctsHeader.Phase5CaptionKey);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "TIR Carnet Num.", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Carnet Num.", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Carnet", captionResourceString.ShortCaption);
		});
	}

	public void TestTirCarnetNumber_AddTirSupportingDocIfNeeded()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default count", 0, departureMovement.GoodsItems.Count);

			departureMovement.TirCarnetNumber = "GB12345678";
			var goodsItem = departureMovement.GoodsItems.Single();
			var tirDocument = goodsItem.SupportingDocuments.Single(x => x.CSI_Code == NctsHeaderValidationHelper.TirCarnetDocumentCode);
			AssertEquals("TIR document is added.", "GB12345678", tirDocument.CSI_ReferenceNumber);

			departureMovement.TirCarnetNumber = "FR12345678";
			AssertEquals("When TirCarnetNumber changes, the old TIR document is deleted.", true, tirDocument.IsDeleted);

			var tirDocument2 = goodsItem.SupportingDocuments.Single(x => x.CSI_Code == NctsHeaderValidationHelper.TirCarnetDocumentCode);
			AssertEquals("A new TIR document is added.", "FR12345678", tirDocument2.CSI_ReferenceNumber);
		});
	}

	public void TestTirCarnetNumber_ShouldAlwaysAddTirSupportingDocToTheFirstGoodsItem()
	{
		var goodsItem1 = departureMovement.GoodsItems.AddNew();
		goodsItem1.BY_InvoiceQuantity = 1m;

		var goodsItem2 = departureMovement.GoodsItems.AddNew();
		goodsItem2.BY_InvoiceQuantity = 10m;

		var goodsItem3 = departureMovement.GoodsItems.AddNew();
		goodsItem3.BY_InvoiceQuantity = 100m;

		departureMovement.TirCarnetNumber = "GB12345678";
		AssertEquals("GB12345678", goodsItem1.SupportingDocuments.Single(x => x.CSI_Code == NctsHeaderValidationHelper.TirCarnetDocumentCode).CSI_ReferenceNumber);

		departureMovement.GoodsItems.ApplySort(nameof(CusInBondCargoDesc.BY_InvoiceQuantity), ListSortDirection.Descending);
		departureMovement.TirCarnetNumber = "FR12345678";
		AssertEquals("TIR supporting doc is still added to the first (minimum BY_LineNo) goods item.", "FR12345678", goodsItem1.SupportingDocuments.Single(x => x.CSI_Code == NctsHeaderValidationHelper.TirCarnetDocumentCode).CSI_ReferenceNumber);
	}

	public void TestTIRCarnetSupportingDocumentCode()
	{
		var departureMovementHeaderForTest = Factory.New<NctsDepartureMovementHeaderForTest>();
		AssertEquals("TIRCarnetSupportingDocumentCode", NctsHeaderValidationHelper.TirCarnetDocumentCode, departureMovementHeaderForTest.TIRCarnetSupportingDocumentCodeExposed);
	}

	public void TestTirCarnetNumber_AddGoodsItem_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			AssertEquals("Default count", 0, departureMovement.GoodsItems.Count);

			departureMovement.TirCarnetNumber = "GB12345678";
			AssertEquals("Not add GoodsItem", 0, departureMovement.GoodsItems.Count);
		});
	}

	public void TestBM_SealType_SealsClearedWhenSealTypeContainer()
	{
		departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
		var seal = nctsHeader.Seals.AddNew();
		nctsHeader.Seals.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Has Records", 2, nctsHeader.Seals.Count);
			departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;
			AssertEquals("Cleared", 0, nctsHeader.Seals.Count);
			AssertEquals("Deleted", true, seal.IsDeleted);
		});
	}

	public void TestBM_SealType_CusSealsClearedWhenSealTypeContainer()
	{
		departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
		var cusSeal = nctsHeader.CusSeals.AddNew();
		nctsHeader.CusSeals.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Has Records", 2, nctsHeader.CusSeals.Count);
			departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;
			AssertEquals("Cleared", 0, nctsHeader.CusSeals.Count);
			AssertEquals("Deleted", true, cusSeal.IsDeleted);
		});
	}

	public void TestBM_SealType_ContainersClearedWhenSealTypePack()
	{
		departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;
		var container = nctsHeader.DepartureHeaderContainers.AddNew();
		nctsHeader.DepartureHeaderContainers.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Has Records", 2, nctsHeader.DepartureHeaderContainers.Count);
			departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
			AssertEquals("Cleared", 0, nctsHeader.DepartureHeaderContainers.Count);
			AssertEquals("Deleted", true, container.IsDeleted);
		});
	}

	public void TestBM_SealType_AllSealsClearedWhenSealTypeIsCleared()
	{
		departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;
		nctsHeader.DepartureHeaderContainers.AddNew();
		nctsHeader.DepartureHeaderContainers.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Has Containers Records", 2, nctsHeader.DepartureHeaderContainers.Count);
			departureMovement.BM_SealType = ZString.Empty;
			AssertEquals("Containers Cleared", 0, nctsHeader.DepartureHeaderContainers.Count);

			departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
			nctsHeader.CusSeals.AddNew();
			nctsHeader.Seals.AddNew();
			AssertEquals("Has CusSeals Records", 1, nctsHeader.CusSeals.Count);
			AssertEquals("Has Seals. Records", 1, nctsHeader.Seals.Count);
			departureMovement.BM_SealType = ZString.Empty;
			AssertEquals("CusSeals Cleared", 0, nctsHeader.CusSeals.Count);
			AssertEquals("Seals Cleared", 0, nctsHeader.Seals.Count);
		});
	}

	public void TestBM_SealType_Cancellation()
	{
		departureMovement.BM_SealType = ZString.Empty;
		departureMovement.OnBM_SealTypeAboutToChange += canceledFunction;
		CombineAssertions(() =>
		{
			departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;
			AssertEquals("BM_SealType changed when initial empty", SealTypeList.Codes.ContainerSeal, departureMovement.BM_SealType);
			nctsHeader.DepartureHeaderContainers.AddNew();
			nctsHeader.DepartureHeaderContainers.AddNew();

			departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
			AssertEquals("BM_SealType not changed", SealTypeList.Codes.ContainerSeal, departureMovement.BM_SealType);
			AssertEquals("Containers not cleared", 2, nctsHeader.DepartureHeaderContainers.Count);

			departureMovement.OnBM_SealTypeAboutToChange -= canceledFunction;
			departureMovement.OnBM_SealTypeAboutToChange += notCanceledFunction;

			departureMovement.BM_SealType = SealTypeList.Codes.PackageSeal;
			AssertEquals("BM_SealType changed", SealTypeList.Codes.PackageSeal, departureMovement.BM_SealType);
			AssertEquals("Containers cleared", 0, nctsHeader.DepartureHeaderContainers.Count);

			departureMovement.OnBM_SealTypeAboutToChange -= notCanceledFunction;
			departureMovement.BM_SealType = SealTypeList.Codes.ContainerSeal;
			AssertEquals("OnBM_SealTypeAboutToChange null, BM_SealType changed", SealTypeList.Codes.ContainerSeal, departureMovement.BM_SealType);
		});

		void canceledFunction(object sender, CancelEventArgs args) => args.Cancel = true;
		void notCanceledFunction(object sender, CancelEventArgs args) => args.Cancel = false;
	}

	public void TestWipeBM_AdditionalText()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		header.BH_FTZMove = true;
		var departureMovement = header.MovementHeader;

		departureMovement.BM_AdditionalText = "12345";
		AssertEquals("Check Commercial Reference Number", "12345", departureMovement.BM_AdditionalText);

		departureMovement.WipeAdditionalText();
		AssertEquals("Commercial Reference Number should have been wiped", string.Empty, departureMovement.BM_AdditionalText);
	}

	public void TestCarrier()
	{
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", departureMovement.Carrier, "1", traderTir: "GBR/022/1234567");
		NCTSTestHelper.AssertJobDocAddress(departureMovement.Carrier, DocAddressType.Carrier, "1", traderTir: "GBR/022/1234567");

		var oldCarrier = departureMovement.Carrier;
		oldCarrier.Delete();

		CombineAssertions(() =>
		{
			var carrier = departureMovement.Carrier;
			AssertNotEquals("New Carrier created", oldCarrier.PK, carrier.PK);
			AssertSame("Cached", carrier, departureMovement.Carrier);

			carrier.Delete();
			var carrier2 = departureMovement.DocAddresses.CreateWithRequirement(departureMovement.CarrierJobDocAddressRequirement);
			AssertEquals("Carrier from DocAddresses", carrier2.PK, departureMovement.Carrier.PK);
		});
	}

	public void TestCarrierAdditionalValidation()
	{
		AssertNull("Carrier AdditionalValidation", departureMovement.Carrier.AdditionalValidation);
	}

	public void TestCarrierJobDocAddressRequirement()
	{
		AssertNotNull("CarrierJobDocAddressRequirement", departureMovement.CarrierJobDocAddressRequirement);
		var carrierJobDocAddressRequirement = departureMovement.CarrierJobDocAddressRequirement;
		AssertSame("CarrierJobDocAddressRequirement Cached", carrierJobDocAddressRequirement, departureMovement.CarrierJobDocAddressRequirement);
	}

	public void TestCheckCarrierWhenRuleTR0064Active()
	{
		const string errorMessage = "[TR0064] Carrier has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers / Codes";

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var carrierOrgHeader = Factory.NewWithValidTestData<OrgHeader>();

		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleTR0064Active)))
		{
			CombineAssertions("When RuleTR0064 is Active", () =>
			{
				departureMovement.Carrier.OrganisationPK = carrierOrgHeader.PK;
				AssertHasMessageError("When Carrier is present without any custom code",
									departureMovement.Carrier.OrganisationPKInfo, errorMessage);

				carrierOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "123456789", Core.Constants.CountryCodes.Italy);
				carrierOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "132547698", Core.Constants.CountryCodes.Italy);

				departureMovement.Carrier.Validation.ValidateOrganisationPK();
				AssertHasMessageError("When Carrier is present and has [AEO, TEN] custom code",
									departureMovement.Carrier.OrganisationPKInfo, errorMessage);

				carrierOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);

				departureMovement.Carrier.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When Carrier is present and [Eori] added",
									departureMovement.Carrier.OrganisationPKInfo, errorMessage);

				carrierOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

				departureMovement.Carrier.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When Carrier is present and [TCU] added",
									departureMovement.Carrier.OrganisationPKInfo, errorMessage);

				carrierOrgHeader.CustomsCodes.RemoveAndDeleteAll();
				carrierOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

				departureMovement.Carrier.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When Carrier is present, removed all previous codes and [TCU] added",
									departureMovement.Carrier.OrganisationPKInfo, errorMessage);

				departureMovement.Carrier.E2_OA_Address = ZGuid.Empty;
				AssertNoMessageError("When Carrier is not filled",
									departureMovement.Carrier.OrganisationPKInfo, errorMessage);
			});
		}
	}

	public void TestCheckCarrierWhenTR0064Inactive()
	{
		const string errorMessage = "[TR0064] Carrier has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers / Codes";

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var carrierOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		departureMovement.Carrier.OrganisationPK = carrierOrgHeader.PK;

		ValidationRuleConfigurationTestHelper.AssertNoNotificationsWithInactiveRule(
			Factory,
			departureMovement.Carrier.OrganisationPKInfo,
			errorMessage,
			nameof(ValidationRuleConfiguration.IsRuleTR0064Active),
			() =>
			{
				carrierOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "123456789", Core.Constants.CountryCodes.Italy);
				carrierOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "132547698", Core.Constants.CountryCodes.Italy);
				departureMovement.Carrier.Validation.ValidateOrganisationPK();
			});
	}

	public void TestIDocAddressesMembers()
	{
		var docAddresses = departureMovement as IDocAddresses;
		AssertNotNull("DepartureMovement as IDocAddresses", docAddresses);

		CombineAssertions("Test properties", () =>
		{
			var jobDocAddresses = Factory.New<JobDocAddress>();

			AssertType<NctsJobDocAddressValidation>(nameof(IDocAddresses.PiggyBackedDocAddressValidation), docAddresses.PiggyBackedDocAddressValidation(jobDocAddresses));
			AssertEquals(nameof(IDocAddresses.GetCanOverrideCheckpoint), Environment.Env.Security.None, docAddresses.GetCanOverrideCheckpoint(jobDocAddresses));
			AssertEquals(nameof(IDocAddresses.CanDeleteAddress), true, docAddresses.CanDeleteAddress(jobDocAddresses));
			AssertType<OrgHeaderCollection>(nameof(IDocAddresses.GetOrgHeaderList), docAddresses.GetOrgHeaderList(DocAddressType.Representative));
			AssertSequencesEqual(nameof(IDocAddresses.SupportedAddressTypes), new DocAddressType[] { DocAddressType.Representative, DocAddressType.Carrier }, docAddresses.SupportedAddressTypes);
		});
	}

	public void TestBM_GrossWeightUQ_ReadOnly_Phase4()
	{
		Assert(departureMovement.BM_GrossWeightUQInfo.ReadOnly);
	}

	public void TestBM_GrossWeightUQ_ReadOnly_Phase5()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		Assert(departureMovement.BM_GrossWeightUQInfo.ReadOnly);
	}

	public void TestBM_GrossWeightUQ_ReadOnly_Phase5_FeatureFlagOn()
	{
		nctsHeader = Factory.New<NctsHeaderPhase5ForTest>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
		Assert(!departureMovement.BM_GrossWeightUQInfo.ReadOnly);
	}

	public void TestBM_SealType_Caption() => NCTSTestHelper.AssertCaptions(departureMovement.BM_SealTypeInfo, "Seal Type", "Type", string.Empty);

	public void TestBM_SealQty_Caption() => NCTSTestHelper.AssertCaptions(departureMovement.BM_SealQtyInfo, "Seal Quantity", "Quantity", string.Empty);

	public void TestBM_GrossWeight_Caption() => NCTSTestHelper.AssertCaptions(departureMovement.BM_GrossWeightInfo, "Gross Weight", "Gross Weight", string.Empty);

	public void TestBM_GrossWeightUQ_Caption() => NCTSTestHelper.AssertCaptions(departureMovement.BM_GrossWeightUQInfo, "Gross Weight Unit", "Gross Weight Unit", string.Empty);

	public void TestBM_GrossWeightUQ_List() => AssertHasCustomAttribute<ListAttribute>(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.BM_GrossWeightUQ), false, attr => attr.ListDataSourceMember == (nameof(NctsDepartureMovementHeader.Lookups) + "." + nameof(NctsDepartureMovementHeader.Lookups.WeightUnitList)));

	public void TestBM_TypeOfSecurity_Caption() => NCTSTestHelper.AssertCaptions(departureMovement.BM_TypeOfSecurityInfo, "Security", string.Empty, string.Empty);

	public void TestBM_TypeOfSecurity_Empty_BM_MethodOfPayment_Phase4()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		CombineAssertions(() =>
		{
			departureMovement.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals($"Type Of Security is ENT.", TransportChargesModeOfPayment.Codes.Cash, departureMovement.BM_MethodOfPayment);

			departureMovement.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals($"Type Of Security is NON.", TransportChargesModeOfPayment.Codes.Cash, departureMovement.BM_MethodOfPayment);
		});
	}

	public void TestBM_TypeOfSecurity_Empty_BM_MethodOfPayment_Phase5_WhenC0186Active()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				AssertEquals($"Type Of Security is ENT.", TransportChargesModeOfPayment.Codes.Cash, departureMovement.BM_MethodOfPayment);

				departureMovement.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals($"Type Of Security is NON.", ZString.Empty, departureMovement.BM_MethodOfPayment);
			});
		}
	}

	public void TestBM_TypeOfSecurity_Empty_BM_MethodOfPayment_Phase5_WhenC0186NotActive()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
		{
			departureMovement.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals(TransportChargesModeOfPayment.Codes.Cash, departureMovement.BM_MethodOfPayment);
		}
	}

	public void TestBM_TypeOfSecurity_Empty_Bill_B0_TransportPaymentMethod_Phase4()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		var bill = departureMovement.Header.Bills.AddNew();

		CombineAssertions(() =>
		{
			bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals($"Type Of Security is ENT.", TransportChargesModeOfPayment.Codes.Cash, bill.B0_TransportPaymentMethod);

			bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals($"Type Of Security is NON.", TransportChargesModeOfPayment.Codes.Cash, bill.B0_TransportPaymentMethod);
		});
	}

	public void TestBM_TypeOfSecurity_Empty_Bill_B0_TransportPaymentMethod_Phase5_WhenC0186Active()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill = departureMovement.Header.Bills.AddNew();
		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
		{
			CombineAssertions(() =>
			{
				bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				AssertEquals($"Type Of Security is ENT.", TransportChargesModeOfPayment.Codes.Cash, bill.B0_TransportPaymentMethod);

				bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals($"Type Of Security is NON.", ZString.Empty, bill.B0_TransportPaymentMethod);
			});
		}
	}

	public void TestBM_TypeOfSecurity_Empty_Bill_B0_TransportPaymentMethod_Phase5_WhenC0186NotActive()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill = departureMovement.Header.Bills.AddNew();
		using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
		{
			bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals($"Type Of Security is ENT.", TransportChargesModeOfPayment.Codes.Cash, bill.B0_TransportPaymentMethod);
		}
	}

	public void TestCheckBM_RL_NKDestinationPortReadOnly()
	{
		AssertEquals("Destination Country readonly default", false, departureMovement.BM_RL_NKDestinationPortReadOnly);

		var goodsItem = departureMovement.GoodsItems.AddNew();
		AssertEquals("[PRE-CONDITION] Destination Country readonly", false, departureMovement.BM_RL_NKDestinationPortReadOnly);

		departureMovement.BM_RL_NKDestinationPort = ZString.Empty;
		goodsItem.BY_RN_NKCountryOfDestination = "IT";
		AssertEquals("When BM_RL_NKDestinationPort is empty and BY_RN_NKCountryOfDestination not empty, Destination Country readonly", true, departureMovement.BM_RL_NKDestinationPortReadOnly);

		departureMovement.BM_RL_NKDestinationPort = "DE";
		AssertEquals("When BM_RL_NKDestinationPort is not empty and BY_RN_NKCountryOfDestination not empty, Destination Country readonly", false, departureMovement.BM_RL_NKDestinationPortReadOnly);

		goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
		AssertEquals("When BM_RL_NKDestinationPort is not empty and set BY_RN_NKCountryOfDestination empty, Destination Country readonly", false, departureMovement.BM_RL_NKDestinationPortReadOnly);
	}

	public void TestCheckIsAirExportTransportMode()
	{
		departureMovement.BM_ExportTransportMode = "1";
		AssertEquals("When BM_ExportTransportMode = SEA, IsAirExportTransportMode", false, departureMovement.IsAirExportTransportMode);

		departureMovement.BM_ExportTransportMode = "4";
		AssertEquals("When BM_ExportTransportMode = AIR, IsAirExportTransportMode", true, departureMovement.IsAirExportTransportMode);
	}

	public void TestPayInfoCollection()
	{
		AssertType<NctsDeparturePayInfoCollection>("PayInfoCollection Type", departureMovement.PayInfoCollection);
	}

	public void TestPayInfoType()
	{
		AssertEquals("PayInfoType", typeof(NctsDeparturePayInfo), departureMovement.PayInfoType);
	}

	public void TestBM_EntryDate_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_EntryDateInfo, NctsHeader.Phase5CaptionKey, "Acceptance Date", string.Empty, "Accept. Date");
	}

	public void TestBM_EntryDater_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_EntryDateInfo, NctsHeader.Phase4CaptionKey, "Declaration Date", string.Empty, "Dec. Date");
	}

	public void TestBM_BTAIndicator_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_BTAIndicatorInfo, NctsHeader.Phase5CaptionKey, "Specific Circumstance Indicator", "Specific Circumstance", "Circumstance");
	}

	public void TestBM_BTAIndicator_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_BTAIndicatorInfo, NctsHeader.Phase4CaptionKey, "Specific Circumstance Indicator", string.Empty, "Spec. Circ.");
	}

	public void TestBM_AdditionalText_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_AdditionalTextInfo, NctsHeader.Phase5CaptionKey, "Unique Consignment Reference", "Reference", "UCR");
	}

	public void TestBM_AdditionalText_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_AdditionalTextInfo, NctsHeader.Phase4CaptionKey, "[7] Commercial Reference No.", string.Empty, "Comm. Ref. No.", "Commercial Reference Number");
	}

	public void TestBM_ExportTransportMode_Phase5Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_ExportTransportModeInfo, NctsHeader.Phase5CaptionKey, "Border Method of Transport", "Border M.O.T.", "M.O.T.");
	}

	public void TestBM_ExportTransportMode_Phase4Caption()
	{
		NCTSTestHelper.AssertCaptionsAndFullDescription(departureMovement.BM_ExportTransportModeInfo, NctsHeader.Phase4CaptionKey, "[21] Transport Mode (Frontier)", string.Empty, "Frontier Transp. Mode", "Frontier Mode of Transport (Code)");
	}

	public void TestBM_ExportTransportTypeOfId_Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_ActiveBorderIdentificationTypeInfo, NctsHeader.Phase5CaptionKey, "Type of Identification", "Type of ID", "Type");
	}

	public void TestTotalWeightOfBills() => CombineAssertions(() =>
	{
		AssertEquals("BM_GrossWeightUQ: kg", new ZWeight(0, Weight.Kilograms), departureMovement.TotalWeightOfBills);

		var bill1 = departureMovement.Header.Bills.AddNew();
		bill1.B0_Weight = 11.111m;
		var bill2 = departureMovement.Header.Bills.AddNew();
		bill2.B0_Weight = 12.222m;
		AssertEquals("BM_GrossWeightUQ: kg, B0_WeightUQ #1: kg, B0_WeightUQ #2: kg", new ZWeight(23.333m, Weight.Kilograms), departureMovement.TotalWeightOfBills);

		bill2.B0_Weight = 10001m;
		bill2.B0_WeightUQ = Weight.Grams;
		AssertEquals("BM_GrossWeightUQ: kg, B0_WeightUQ #1: kg, B0_WeightUQ #2: g", new ZWeight(21.112m, Weight.Kilograms), departureMovement.TotalWeightOfBills);

		departureMovement.BM_GrossWeightUQ = Weight.Hectograms;
		AssertEquals("BM_GrossWeightUQ: hg, B0_WeightUQ #1: kg, B0_WeightUQ #2: g", new ZWeight(211.12m, Weight.Hectograms), departureMovement.TotalWeightOfBills);

		departureMovement.BM_GrossWeightUQ = Weight.Tonnes;
		AssertEquals("BM_GrossWeightUQ: t, B0_WeightUQ #1: kg, B0_WeightUQ #2: g", new ZWeight(0.021112m, Weight.Tonnes), departureMovement.TotalWeightOfBills);

		bill2.B0_WeightUQ = ZString.Empty;
		AssertEquals("BM_GrossWeightUQ: t, B0_WeightUQ #1: kg, B0_WeightUQ #2: <empty>", new ZWeight(0.011111m, Weight.Tonnes), departureMovement.TotalWeightOfBills);

		bill1.B0_WeightUQ = ZString.Empty;
		AssertEquals("BM_GrossWeightUQ: t, B0_WeightUQ #1: <empty>, B0_WeightUQ #2: <empty>", new ZWeight(0m, Weight.Tonnes), departureMovement.TotalWeightOfBills);

		departureMovement.BM_GrossWeightUQ = ZString.Empty;
		bill1.B0_Weight = 11.111m;
		bill1.B0_WeightUQ = Weight.Kilograms;
		Assert("BM_GrossWeightUQ: <empty>, B0_WeightUQ #1: kg, B0_WeightUQ #2: <empty>", !departureMovement.TotalWeightOfBills.IsValid);
	});

	public void TestIsGrossWeightValid()
	{
		CombineAssertions(() =>
		{
			var bill = nctsHeader.Bills.AddNew();
			departureMovement.BM_GrossWeight = 23.333m;
			departureMovement.BM_GrossWeightUQ = Weight.Kilograms;
			bill.B0_Weight = 0;
			bill.B0_WeightUQ = Weight.Kilograms;
			AssertEquals("BM_GrossWeight > TotalWeightOfBills (kg)", true, departureMovement.IsGrossWeightValid);

			bill.B0_Weight = 23.333m;
			AssertEquals("BM_GrossWeight = TotalWeightOfBills (kg)", true, departureMovement.IsGrossWeightValid);

			bill.B0_Weight = 24.444m;
			AssertEquals("BM_GrossWeight < TotalWeightOfBills (kg)", false, departureMovement.IsGrossWeightValid);

			bill.B0_WeightUQ = Weight.Grams;
			AssertEquals("BM_GrossWeight > TotalWeightOfBills (g)", true, departureMovement.IsGrossWeightValid);
		});
	}

	public void TestBM_RN_NKCountryOfDispatch_Caption()
	{
		this.AssertDataBoundResourceStringsWithMultipleResourceKey(departureMovement.BM_RN_NKCountryOfDispatchInfo, multipleResourceKey: null, "Country/Region of Dispatch", "Disp. Ctry./Rgn.", "Dispatch Ctry./Rgn.");
	}

	public void TestGoodsLocation_Create()
	{
		var goodsLocation = departureMovement.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertEquals("CGL_ParentID", departureMovement.PK, goodsLocation.CGL_ParentID);
			AssertEquals("CGL_ParentTableCode", CusInBondMoveHeaderSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
			AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.Departure, goodsLocation.CGL_LocationUse);
			AssertSame("Cached", goodsLocation, departureMovement.GoodsLocation);
			AssertEquals("IsRegisteredEditableChildObject", true, departureMovement.IsRegisteredEditableChildObject(goodsLocation));
			AssertType<CusGoodsLocation>(goodsLocation);
		});
	}

	public void TestGoodsLocation_Load()
	{
		var goodsLocation = Factory.New<CusGoodsLocation>();
		goodsLocation.Parent = departureMovement;
		goodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
		AssertSame(goodsLocation, departureMovement.GoodsLocation);
	}

	public void TestGoodsLocationDescription()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		CombineAssertions(() =>
		{
			AssertNull("GoodsLocation doesn't exist", CusGoodsLocation.Load<EU.Business.CusGoodsLocation>(departureMovement, CusGoodsLocationUseList.Codes.Departure));
			AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, departureMovement.GoodsLocationDescription);

			var goodsLocation = departureMovement.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			departureMovement.ValidateGoodsLocationDescription();
			AssertHasMessageError(departureMovement.GoodsLocationDescriptionInfo, "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.");

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			AssertEquals("GoodsLocationDescription when there's a GoodsLocation", "Z;B", departureMovement.GoodsLocationDescription);
		});
	}

	public void TestICusGoodsLocationProvider_ProviderKey()
	{
		AssertEquals("LVNCTS", (departureMovement as ICusGoodsLocationProvider).ProviderKey);
	}

	public void TestBM_PaperlessInbondNum_ReadOnly()
	{
		CombineAssertions(() =>
		{
			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("Enabled for Phase4 - registry = false (default)", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);
				AssertEquals("Enabled for Phase4 - registry = false (default)", false, departureMovement.ShouldGenerateLocalReferenceNumberOnSaving);

				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Disabled for Phase5 - registry = false (default)", true, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);
				AssertEquals("Disabled for Phase5 - registry = false (default)", true, departureMovement.ShouldGenerateLocalReferenceNumberOnSaving);
			}

			using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("Enabled for Phase4 - registry = true", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);
				AssertEquals("Enabled for Phase4 - registry = true", false, departureMovement.ShouldGenerateLocalReferenceNumberOnSaving);

				departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("Enabled for Phase5 - registry = true", false, departureMovement.BM_PaperlessInbondNumInfo.ReadOnly);
				AssertEquals("Enabled for Phase5 - registry = true", false, departureMovement.ShouldGenerateLocalReferenceNumberOnSaving);
			}
		});
	}

	[TestDate(2022, 10, 22)]
	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedWhenEmpty()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.HeaderBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");

		CombineAssertions(() =>
		{
			AssertEquals("BM_PaperlessInbondNum empty before saved", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
			Factory.Save();
			AssertEquals("BM_PaperlessInbondNum automatically generated after saved", "2212307896540000000001", departureMovement.BM_PaperlessInbondNum);
		});
	}

	[TestDate(2022, 10, 22)]
	public void TestBM_PaperlessInbondNum_AutomaticallyGeneratedWhenForced()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		configurationTestContext.EnableConfiguration(x => x.UseLocalReferenceNumberIgnoreInDatabaseCheck);
		CombineAssertions(() =>
		{
			Factory.Save();
			AssertEquals("BM_PaperlessInbondNum empty after saved", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
			departureMovement.HeaderBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");
			Factory.Save();
			AssertEquals("BM_PaperlessInbondNum empty after eori added & saved", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
			departureMovement.BM_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(1);
			Factory.Save();
			AssertEquals("BM_PaperlessInbondNum automatically generated when forced", "2212307896540000000001", departureMovement.BM_PaperlessInbondNum);
		});
	}

	[TestDate(2022, 10, 22)]
	public void TestBM_PaperlessInbondNum_NotAutomaticallyGeneratedWhenCustomerReferenceIsNotEmpty()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");

		CombineAssertions(() =>
		{
			departureMovement.BM_PaperlessInbondNum = "2011111111110000000001";
			Factory.Save();
			AssertEquals("BM_PaperlessInbondNum not updated when it's not empty", "2011111111110000000001", departureMovement.BM_PaperlessInbondNum);
		});
	}

	public void TestBM_PaperlessInbondNum_NotAutomaticallyGeneratedWhenManualCustomerReferenceIsEnabled()
	{
		using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("BM_PaperlessInbondNum empty before saved", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
			Factory.Save();
			AssertEquals("BM_PaperlessInbondNum is still empty after saved", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
		}
	}

	public void TestResetMovementForRetransmission()
	{
		departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
		configurationTestContext.EnableConfiguration(x => x.IsDepartureRetransmissionSupported);

		CombineAssertions(() =>
		{
			var result = departureMovement.ResetMovementForRetransmission();
			AssertEquals("Retransmission not allowed", "Retransmission is not allowed currently because of the customs status.", result.Message);
			Assert(!result.IsUpdated);

			departureMovement.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;
			result = departureMovement.ResetMovementForRetransmission();
			Assert(result.IsUpdated);
			AssertEquals("Retransmission allowed", "LRN and status reset. Please save, close and reopen this declaration to edit and retransmit.", result.Message);
			AssertEquals("BM_CustomsStatus after retransmission", NctsTransitStatusList.Codes.Unknown, departureMovement.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus after retransmission", NctsTransitStatusList.Codes.Unknown, departureMovement.BM_MessageStatus);
			AssertEquals("LocalReferenceNumber after retransmission", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
		});
	}

	public void TestIsDepartureRetransmissionAllowed()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus empty", false, departureMovement.IsDepartureRetransmissionAllowed);

			configurationTestContext.EnableConfiguration(x => x.IsDepartureRetransmissionSupported);

			departureMovement.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;
			AssertEquals($"BM_CustomsStatus '{NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit}'", true, departureMovement.IsDepartureRetransmissionAllowed);
		});
	}

	public void TestIsDepartureRetransmissionSupported_Disabled()
	{
		AssertIsDepartureRetransmissionSupported(false);
	}

	public void TestIsDepartureRetransmissionSupported_Enabled()
	{
		AssertIsDepartureRetransmissionSupported(true);
	}

	public void AssertIsDepartureRetransmissionSupported(bool isDepartureRetransmissionSupportedCore)
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

		configurationTestContext.SetConfiguration<ZBool>(x => x.IsDepartureRetransmissionSupported, isDepartureRetransmissionSupportedCore);
		AssertEquals($"Config setting expected {isDepartureRetransmissionSupportedCore}", departureMovement.IsDepartureRetransmissionAllowed, isDepartureRetransmissionSupportedCore);
	}

	public void TestBM_InBondEntryType_ChangeToTIR_Phase5()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_ReducedDatasetIndicator = true;
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("IsSimplifiedNctsProcedure", false, departureMovement.IsSimplifiedNctsProcedure);
			AssertEquals("ReducedDatasetIndicator", false, departureMovement.BM_ReducedDatasetIndicator);
		});
	}

	public void TestBM_InBondEntryType_ChangeToTIR_Phase4()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		departureMovement.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_ReducedDatasetIndicator = true;
		CombineAssertions(() =>
		{
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("IsSimplifiedNctsProcedure", true, departureMovement.IsSimplifiedNctsProcedure);
			AssertEquals("ReducedDatasetIndicator", true, departureMovement.BM_ReducedDatasetIndicator);
		});
	}

	public void TestBM_ReducedDatasetIndicator_ShouldAddTRDAuthorization_WhenAuthorizationValid()
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization);
		departureMovement.Header.Principal.OrganisationPK = organization.PK;

		AssertCollectionNotContains("Precondition", departureMovement.CusAuthorizationUsages, cau => cau.AGC_Code == CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset);

		// Act
		departureMovement.BM_ReducedDatasetIndicator = true;

		// Assert
		AssertCollectionContains(departureMovement.CusAuthorizationUsages,
								cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
									cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
									cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
	}

	public void TestBM_ReducedDatasetIndicator_ShouldNotAddTRDAuthorization_WhenMoreThanOneValidAuthorization()
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader1 = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization);
		var cusAuthorizationHeader2 = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization);
		departureMovement.Header.Principal.OrganisationPK = organization.PK;

		// Act
		departureMovement.BM_ReducedDatasetIndicator = true;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader1.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader1.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader1.CPH_OH_PermitHolder);
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader2.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader2.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader2.CPH_OH_PermitHolder);
	}

	public void TestBM_ReducedDatasetIndicator_ShouldNotAddTRDAuthorization_WhenAuthorizationInvalid()
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization);
		cusAuthorizationHeader.CPH_StartDate = ZDate.Today.AddDays(1);
		cusAuthorizationHeader.CPH_EndDate = ZDate.Today.AddDays(2);
		departureMovement.Header.Principal.OrganisationPK = organization.PK;

		// Act
		departureMovement.BM_ReducedDatasetIndicator = true;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
	}

	public void TestBM_ReducedDatasetIndicator_ShouldNotAddTRDAuthorization_WhenPrincipalNotSet()
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader = CreateCusAuthorizationHeader(CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, organization);
		departureMovement.Header.Principal.OrganisationPK = ZGuid.Empty;

		// Act
		departureMovement.BM_ReducedDatasetIndicator = true;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
	}

	public void TestBM_ReducedDatasetIndicator_ShouldDeleteTRDAuthorization()
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.BM_ReducedDatasetIndicator = true;
		var cusAuthorizationHeader = departureMovement.CusAuthorizationUsages.AddNew();
		cusAuthorizationHeader.AGC_Code = CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset;

		// Act
		departureMovement.BM_ReducedDatasetIndicator = false;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages, cusAuthorizationHeader);
	}

	public void TestBM_InBondEntryType_ChangeToNonTIR_Phase5()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.TirCarnetNumber = "Test";
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AssertEquals(ZString.Empty, departureMovement.TirCarnetNumber);
	}

	public void TestBM_InBondEntryType_ChangeToNonTIR_Phase4()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		departureMovement.TirCarnetNumber = "Test";
		departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4;
		AssertEquals("Test", departureMovement.TirCarnetNumber);
	}

	public void TestIsSimplifiedNctsProcedure_Phase5()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_ExportDate = new ZDateTime(2022, 11, 18);
		departureMovement.IsSimplifiedNctsProcedure = false;
		AssertEquals(ZDateTime.Empty, departureMovement.BM_ExportDate);
	}

	public void TestIsSimplifiedNctsProcedure_Phase4()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		departureMovement.IsSimplifiedNctsProcedure = true;
		departureMovement.BM_ExportDate = new ZDateTime(2022, 11, 18);
		departureMovement.IsSimplifiedNctsProcedure = false;
		AssertEquals(new ZDateTime(2022, 11, 18), departureMovement.BM_ExportDate);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldAddACRAuthorization_WhenAuthorizationValid()
	{
		AssertIsSimplifiedNctsProcedure_ShouldAddAuthorization_WhenAuthorizationValid(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldAddSSEAuthorization_WhenAuthorizationValid()
	{
		AssertIsSimplifiedNctsProcedure_ShouldAddAuthorization_WhenAuthorizationValid(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldNotAddACRAuthorization_WhenMoreThanOneValidAuthorization()
	{
		AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenMoreThanOneValidAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldNotAddSSEAuthorization_WhenMoreThanOneValidAuthorization()
	{
		AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenMoreThanOneValidAuthorization(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldNotAddACRAuthorization_WhenAuthorizationInvalid()
	{
		AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenAuthorizationInvalid(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldNotAddSSEAuthorization_WhenAuthorizationInvalid()
	{
		AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenAuthorizationInvalid(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldNotAddACRAuthorization_WhenPrincipalNotSet()
	{
		AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenPrincipalNotSet(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldNotAddSSEAuthorization_WhenPrincipalNotSet()
	{
		AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenPrincipalNotSet(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldDeleteACRAuthorization()
	{
		AssertIsSimplifiedNctsProcedure_ShouldDeleteAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit);
	}

	public void TestIsSimplifiedNctsProcedure_ShouldDeleteSSEAuthorization()
	{
		AssertIsSimplifiedNctsProcedure_ShouldDeleteAuthorization(CusAuthorizationHeaderTypeList.Codes.SpecialSeals);
	}

	[TestDate(2022, 12, 31)]
	public void TestRollbackBM_PaperlessInbondNumOnSavingFailed()
	{
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.Header.BH_OA_Importer = ZGuid.NewZGuid();

		using (Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			departureMovement.Header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Germany);

			var referenceNumber = ZString.Empty;
			departureMovement.BM_PaperlessInbondNumInfo.ValueChanged += (o, e) =>
			{
				if (!departureMovement.BM_PaperlessInbondNum.IsEmpty)
				{
					referenceNumber = departureMovement.BM_PaperlessInbondNum;
				}
			};

			AssertExceptionThrown<ZSaveException>("Saving failed", Factory.Save);
			AssertEquals("local reference number had been set on saving", "2212345000000000000001", referenceNumber);
			AssertEquals("reset local reference number", ZString.Empty, departureMovement.BM_PaperlessInbondNum);
		}
	}

	public void TestBM_SpecificCircumstance_Caption()
	{
		NCTSTestHelper.AssertCaptions(departureMovement.BM_SpecificCircumstanceInfo, "Specific Circumstance Indicator", "Specific Circumstance", "Circumstance");
	}

	public void TestBM_PlaceOfUnloadingDefaultValue_UNLOCO()
	{
		var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco1.RL_NameWithDiacriticals = "UNLOCO Name 1";
		var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco2.RL_NameWithDiacriticals = "UNLOCO Name 2";

		departureMovement.BM_ForeignDestPortKCode = unloco1.Code;
		AssertEquals("UNLOCO Name 1", departureMovement.BM_PlaceOfUnloading);

		departureMovement.BM_ForeignDestPortKCode = unloco2.Code;
		AssertEquals("UNLOCO Name 2", departureMovement.BM_PlaceOfUnloading);

		departureMovement.BM_ForeignDestPortKCode = string.Empty;
		AssertEquals(string.Empty, departureMovement.BM_PlaceOfUnloading);
	}

	public void TestBM_PlaceOfUnloadingDefaultValueNotSet_CountryCode()
	{
		departureMovement.BM_ForeignDestPortKCode = "DE";
		AssertEquals(ZString.Empty, departureMovement.BM_PlaceOfUnloading);
	}

	public void TestBM_PlaceOfUnloadingDefaultValueNotSetIfNotEmpty()
	{
		var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco.RL_NameWithDiacriticals = "UNLOCO Name 123";
		departureMovement.BM_PlaceOfUnloading = "Unloading";

		departureMovement.BM_ForeignDestPortKCode = unloco.Code;
		AssertEquals("Unloading", departureMovement.BM_PlaceOfUnloading);
	}

	public void TestBM_ForeignDestPortKCode_MaxLength()
	{
		AssertEquals(5, departureMovement.BM_ForeignDestPortKCodeInfo.MaxLength);
	}

	public void TestBM_PlaceOfLoadingDefaultValue_UNLOCO()
	{
		var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco1.RL_NameWithDiacriticals = "UNLOCO Name 1";
		var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco2.RL_NameWithDiacriticals = "UNLOCO Name 2";

		departureMovement.BM_PortOfPresentationCode = unloco1.Code;
		AssertEquals("UNLOCO Name 1", departureMovement.BM_PlaceOfLoading);

		departureMovement.BM_PortOfPresentationCode = unloco2.Code;
		AssertEquals("UNLOCO Name 2", departureMovement.BM_PlaceOfLoading);

		departureMovement.BM_PortOfPresentationCode = string.Empty;
		AssertEquals(string.Empty, departureMovement.BM_PlaceOfLoading);
	}

	public void TestBM_PlaceOfLoadingDefaultValueNotSet_CountryCode()
	{
		departureMovement.BM_PortOfPresentationCode = "DE";
		AssertEquals(ZString.Empty, departureMovement.BM_PlaceOfLoading);
	}

	public void TestBM_PlaceOfLoadingDefaultValueNotSetIfNotEmpty()
	{
		var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
		unloco.RL_NameWithDiacriticals = "UNLOCO Name 123";
		departureMovement.BM_PlaceOfLoading = "Loading";

		departureMovement.BM_PortOfPresentationCode = unloco.Code;
		AssertEquals("Loading", departureMovement.BM_PlaceOfLoading);
	}

	public void TestBM_PortOfPresentationCode_MaxLength()
	{
		AssertEquals(5, departureMovement.BM_PortOfPresentationCodeInfo.MaxLength);
	}

	public void TestBM_PlaceOfLoading_MaxLength()
	{
		AssertEquals(35, departureMovement.BM_PlaceOfLoadingInfo.MaxLength);
	}

	public void TestBM_PlaceOfUnloading_MaxLength_InTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		{
			AssertEquals(17, departureMovement.BM_PlaceOfUnloadingInfo.MaxLength);
		}
	}

	public void TestBM_PlaceOfUnloading_MaxLength_OutsideTransitionPeriod()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
		{
			AssertEquals(35, departureMovement.BM_PlaceOfUnloadingInfo.MaxLength);
		}
	}

	public void TestDefaultBM_TypeOfSecurity_Phase5()
	{
		var nctsHeader2 = Factory.New<NctsHeader>();
		nctsHeader2.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		var departureMovement = NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeader>(nctsHeader2, NctsMoveHeaderType.Codes.Departure);
		AssertEquals("BM_TypeOfSecurity set to NON", NctsTypeOfSecurityList.Codes.NON, departureMovement.BM_TypeOfSecurity);
	}

	public void TestUseLocalReferenceNumberIgnoreInDatabaseCheck_Disabled()
	{
		AssertUseLocalReferenceNumberIgnoreInDatabaseCheck(false);
	}

	public void TestUseLocalReferenceNumberIgnoreInDatabaseCheck_Enabled()
	{
		AssertUseLocalReferenceNumberIgnoreInDatabaseCheck(true);
	}

	public void TestRequireTransportAtDepartureUpperCase()
	{
		AssertEquals("FALSE, as RuleB1811Active", false, departureMovement.RequireTransportAtDepartureUpperCase);

		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleB1811Active)))
		{
			AssertEquals("TRUE, as NOT RuleB1811Active", false, departureMovement.RequireTransportAtDepartureUpperCase);
		}
	}

	public void TestGuarantees()
	{
		SetPhase5DepartureMovementHeader();
		departureMovement.Guarantees.AddNew();
		departureMovement.Guarantees.AddNew();
		CombineAssertions(() =>
		{
			var result = departureMovement.Guarantees;
			AssertEquals("count", 2, result.Count);
			AssertEquals("Parent", departureMovement.PK, result.First().PW_ParentID);
		});
	}

	public void TestClone_Phase5()
	{
		SetPhase5DepartureMovementHeader();
		departureMovement.Guarantees.AddNew().PW_BondNumber = "BondNr. 1";
		departureMovement.Guarantees.AddNew().PW_BondNumber = "BondNr. 2";
		var clone = (NctsDepartureMovementHeader)(new NctsDeepCloneStrategy(departureMovement, ZGuid.Empty).Clone());
		AssertContainsExactElementsInAnyOrder(new[] { "BondNr. 1", "BondNr. 2" }, clone.Guarantees.Select(x => x.PW_BondNumber));
	}

	public void TestClone_Phase4()
	{
		departureMovement.Guarantees.AddNew().PW_BondNumber = "BondNr. 1";
		departureMovement.Guarantees.AddNew().PW_BondNumber = "BondNr. 2";
		var clone = (NctsDepartureMovementHeader)(new NctsDeepCloneStrategy(departureMovement, ZGuid.Empty).Clone());
		AssertEquals(0, clone.Guarantees.Count);
	}

	void AssertUseLocalReferenceNumberIgnoreInDatabaseCheck(bool useLocalReferenceNumberIgnoreInDatabaseCheckValue)
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		configurationTestContext.SetConfiguration<ZBool>(x => x.UseLocalReferenceNumberIgnoreInDatabaseCheck, useLocalReferenceNumberIgnoreInDatabaseCheckValue);

		AssertEquals($"Config setting expected {useLocalReferenceNumberIgnoreInDatabaseCheckValue}", departureMovement.GenerateLocalReferenceNumberIgnoreInDatabaseCheck, useLocalReferenceNumberIgnoreInDatabaseCheckValue);
	}

	public void TestDepartureCustomerReferenceNumberFountain()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureMovement = Factory.New<NctsDepartureMovementHeaderForTest>();
		nctsHeader.MovementHeaders.Add(departureMovement);

		AssertEquals("Departure Number Fountain", "EULocalReferenceNumber", (departureMovement as ILRNGenerator).LrnNumberFountain.Name);
	}

	public void TestCustomsOfficesForDeparture()
	{
		var customsOfficesForDeparture = departureMovement.CustomsOfficesForDeparture;
		CombineAssertions(() =>
		{
			AssertType<NctsEuOfficeCodeCollectionForDepartureGrid>(customsOfficesForDeparture);
			AssertEquals("IsRegisteredEditableChildObject", true, departureMovement.IsRegisteredEditableChildObject(customsOfficesForDeparture));
			AssertSame("Cached", customsOfficesForDeparture, departureMovement.CustomsOfficesForDeparture);
			AssertEquals("IsLoaded", true, customsOfficesForDeparture.IsLoaded);
		});
	}

	public void TestCusReferenceTypeSupporter()
	{
		ICusReferenceTypeSupporter supporter = departureMovement;
		AssertEquals(typeof(CusSupplyChainActorReference), supporter.GetCusReferenceTypes()[CusReferenceTypeList.Codes.SupplyChainActor]);
	}

	public void TestCusSupplyChainActors()
	{
		var actors = departureMovement.CusSupplyChainActors;
		CombineAssertions(() =>
		{
			AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(actors);
			AssertEquals("IsRegisteredEditableChildObject", true, departureMovement.IsRegisteredEditableChildObject(actors));
			AssertSame("Cached", actors, departureMovement.CusSupplyChainActors);
			AssertEquals("IsLoaded", true, actors.IsLoaded);
		});
	}

	public void TestOnFactorySavingAndGrossWeightInvalid_TotalWeightGreaterThanBM_GrossWeight()
	{
		departureMovement.OnFactorySavingAndGrossWeightInvalid += (sender, args) => departureMovement.UpdateBM_GrossWeightFromBills();

		departureMovement.BM_GrossWeight = 10.0m;
		departureMovement.BM_GrossWeightUQ = "KG";

		var bill = departureMovement.Header.Bills.AddNew();
		bill.B0_Weight = 12.0m;

		Factory.Save();

		AssertEquals(12.0m, departureMovement.BM_GrossWeight);
	}

	public void TestOnFactorySavingAndGrossWeightInvalid_TotalWeightLessThanBM_GrossWeight()
	{
		departureMovement.OnFactorySavingAndGrossWeightInvalid += (sender, args) => departureMovement.UpdateBM_GrossWeightFromBills();

		departureMovement.BM_GrossWeight = 13.0m;
		departureMovement.BM_GrossWeightUQ = "KG";

		var bill = departureMovement.Header.Bills.AddNew();
		bill.B0_Weight = 12.0m;

		Factory.Save();

		AssertEquals(13.0m, departureMovement.BM_GrossWeight);
	}

	#region ReserveTemporaryStorageGoods

	public void TestGetGoodsItemDataDeclaredForDepartureToReserveTSGoods()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();
		var goodsItem2 = bill1.GoodsItems.AddNew();
		var bill2 = nctsHeader.Bills.AddNew();
		var goodsItem3 = bill2.GoodsItems.AddNew();
		var goodsItem4 = bill2.GoodsItems.AddNew();
		var movementheader = nctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			var (listReturned, messageReturned) = movementheader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods();
			AssertEquals("Method returns empty enumerable when there are no documents in the declaration", 0, listReturned.Count());
			AssertEquals("Method returns empty string when there are no documents in the declaration", ZString.Empty, messageReturned);

			var previousDoc1 = goodsItem1.PreviousDocuments.AddNew();
			previousDoc1.CSI_ReferenceNumber = "Reference";
			previousDoc1.CSI_Code = "BBB";

			var previousDoc2 = goodsItem2.PreviousDocuments.AddNew();
			previousDoc2.CSI_ReferenceNumber = "Reference2";
			previousDoc2.CSI_Code = "SUM";

			var previousDoc3 = goodsItem3.PreviousDocuments.AddNew();
			previousDoc3.CSI_ReferenceNumber = "Reference3";
			previousDoc3.CSI_Code = "AAA";

			var previousDoc4 = goodsItem4.PreviousDocuments.AddNew();
			previousDoc4.CSI_ReferenceNumber = "Reference";
			previousDoc4.CSI_Code = "SUM";

			(listReturned, messageReturned) = movementheader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods();
			AssertEquals("Method returns empty enumerable even when there are documents in the declaration", 0, listReturned.Count());
			AssertEquals("Method returns empty string even when there are documents in the declaration", ZString.Empty, messageReturned);
		});
	}

	public void TestTemporaryStorageTransactionInternalReferenceNumber()
	{
		var headerDep = Factory.New<NctsHeader>();
		headerDep.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		headerDep.SetMovementType(NctsMovementType.Codes.Departure);
		var depMovementheader = headerDep.MovementHeader;
		depMovementheader.BM_PaperlessInbondNum = "AAA";

		CombineAssertions(() =>
		{
			AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to empty when departure", ZString.Empty, depMovementheader.TemporaryStorageTransactionInternalReferenceNumber);
			AssertEquals("TemporaryStorageTransactionInternalReferenceType is set to empty when departure", ZString.Empty, depMovementheader.TemporaryStorageTransactionInternalReferenceType);
		});
	}

	#endregion

	#region ConfirmTemporaryStorageGoodsConsumption

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (movementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				movementHeader.BM_CustomsStatus = "AAA";

				AssertEquals("BM_CustomsStatus is changed to AAA", "AAA", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_EmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInEntry: ZString.Empty);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				movementHeader.BM_CustomsStatus = "AAA";

				AssertEquals("BM_CustomsStatus is changed to AAA", "AAA", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_LocationNotManagedInPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInPremises: "9999000005");

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				movementHeader.BM_CustomsStatus = "AAA";

				AssertEquals("BM_CustomsStatus is changed to AAA", "AAA", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_FlagFalse()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldConfirm: false);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				movementHeader.BM_CustomsStatus = "AAA";

				AssertEquals("BM_CustomsStatus is changed to AAA", "AAA", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_CustomsStatusNotInLists()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				movementHeader.BM_CustomsStatus = "DDD";

				AssertEquals("BM_CustomsStatus is changed to DDD", "DDD", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToCancel()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				movementHeader.BM_CustomsStatus = "AAA";

				AssertEquals("BM_CustomsStatus is changed to AAA", "AAA", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToConfirm()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			SetUpRefData();
			Factory.Save();

			var oldComment = "Extra Old Comment";

			var orgHeader = SetUpOrgHeader();
			var movementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);

			var regHeader1 = SetUpTmpRegHeader();

			var regLine1 = (CusTempStorageRegLine)regHeader1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			regLine1.SRL_PackageType = "NE";
			var regLine2 = (CusTempStorageRegLine)regHeader1.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_PackageType = "VQ";

			var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 10, 6);
			regLineTransaction1.SRT_Comments = oldComment;
			var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2);

			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5);
			var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);
			var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 6);

			var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

			var regLine3 = (CusTempStorageRegLine)regHeader2.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 1;
			regLine3.SRL_PackageType = "AA";
			var regLine4 = (CusTempStorageRegLine)regHeader2.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 3;
			regLine4.SRL_PackageType = "VG";

			var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 5, 6);
			var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("BM_CustomsStatus is changed to BBB", "BBB", movementHeader.BM_CustomsStatus);

				AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, comment: ExpectedComment + " - " + oldComment);
				AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2);
				AssertEquals("regLineTransaction3 was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction4", regLineTransaction4);
				AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
				AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6);
				AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7);

				AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

				AssertEquals("regLine3 CustomsStatus is not changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
				AssertEquals("regLine4 CustomsStatus is not changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
				AssertEquals("regHeader2 Status is not changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_WriteOff()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var movementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, -3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 2, grossWeight: -2);
			var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 1, grossWeight: -1);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("2 Write off transactions in Guarantee are created", 2, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
				AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);

				var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", RegHeaderReference, 2.0m);
				AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", RegHeaderReference, 1.0m);
			});
		}

		void AssertWriteOffTransaction(BaseCusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZDecimal expectedTranValue)
		{
			AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
			AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
			AssertEquals(transactionName + "'s CPL_TransactionDate is Entry Release Date", releaseDate, transaction.CPL_TransactionDate);
			AssertEquals(transactionName + "'s CPL_Comment is Write-off + reference + MRN", string.Format("Write-off TS {0} {1}", expectedReference, MRNCode), transaction.CPL_Comment);
			AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
			AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_NoPendingAmount()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var movementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 0.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_PendingAmountPositive()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var orgHeader = SetUpOrgHeader();
			var movementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
			var regHeader = SetUpTmpRegHeader();
			var guarantee = SetUpGauranteeForTempStorage(regHeader, 3.0m, orgHeader);
			var regLine = SetUpRegLine(regHeader);

			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

			var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

			CombineAssertions(() =>
			{
				AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
				AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

				var expectedError = "|RES=Reference reference has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
				AssertEquals("New event in logs", expectedError, movementHeader.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			});
		}
	}

	#region ConfirmTemporaryStorageGoodsConsumptionAfterAddingPNDTransactions

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactionsWithCustomsStatusNotInLists_WithPNDTransaction()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldGetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "DDD";

				AssertEquals("BM_CustomsStatus is changed to DDD", "DDD", movementHeader.BM_CustomsStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactionsWithCustomsStatusNotInLists_WithCONTransaction()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldGetPreviousDocuments: true);
			regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "DDD";

				AssertEquals("BM_CustomsStatus is changed to DDD", "DDD", movementHeader.BM_CustomsStatus);
				AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusNotInLists()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "DDD";

				AssertEquals("BM_CustomsStatus is changed to DDD", "DDD", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusNotInLists_HavingFormatDocRef_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var docReference = "1234565";
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "DDD";

				AssertEquals("BM_CustomsStatus is changed to DDD", "DDD", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusNotInLists_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var docReference = "1234565789456123789";
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "DDD";

				AssertEquals("BM_CustomsStatus is changed to DDD", "DDD", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusNotInLists_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var docReference = "1234565789456123789";
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference + "AAA", docReference: docReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "DDD";

				AssertEquals("BM_CustomsStatus is changed to DDD", "DDD", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", ZString.Empty, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", ZString.Empty, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ZString.Empty, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusToConfirm()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("BM_CustomsStatus is changed to BBB", "BBB", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusToConfirm_HavingFormatDocRef_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var docReference = "1234565";
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("BM_CustomsStatus is changed to BBB", "BBB", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusToConfirm_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var docReference = "1234565789456123789";
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("BM_CustomsStatus is changed to BBB", "BBB", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldAddPNDTransactionsWithCustomsStatusToConfirm_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var docReference = "1234565789456123789";
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference + "AAA", docReference: docReference);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "BBB";

				AssertEquals("BM_CustomsStatus is changed to BBB", "BBB", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
				AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
				AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
				AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
				AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
			});
		}
	}

	public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldNotAddPNDTransactionsWithCustomsStatusInList()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (movementHeader, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: BM_CustomsStatus is empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				movementHeader.BM_CustomsStatus = "CCC";

				AssertEquals("BM_CustomsStatus is changed to CCC", "CCC", movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not created (status in list)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

				movementHeader.BM_CustomsStatus = ZString.Empty;
				AssertEquals("BM_CustomsStatus is changed to empty", ZString.Empty, movementHeader.BM_CustomsStatus);
				AssertEquals("regLineTransaction was not created (empty is status in list)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
			});
		}
	}

	#endregion

	const string RegHeaderReference = "reference";
	const string EntryReference = "ES00001";
	const string InternalReferenceType = "OTH";
	const string MRNCode = "20ES00999930006184";
	const string LocationInEntry = "9999000002";
	const string CommentPrefix = "Prefix";
	const string DeclarationReference = "B00000001";
	const string ExpectedComment = CommentPrefix + " " + DeclarationReference;
	readonly ZDateTime issueDate = new ZDateTime(2024, 06, 10, 11, 11, 11);
	readonly ZDateTime releaseDate = new ZDateTime(2024, 06, 14, 11, 11, 11);

	NctsDepartureMovementHeader SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(OrgAddress orgAddress, string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry
		, bool shouldConfirm = true, bool shouldGetPreviousDocuments = false, bool shouldFormatDocumentNumber = false, string docReference = RegHeaderReference)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.BH_JobReference = DeclarationReference;

		var mrnEntryNum = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Latvia);
		mrnEntryNum.CE_EntryNum = MRNCode;
		mrnEntryNum.CE_IssueDate = issueDate;

		var departureMovement = Factory.New<NctsDepartureMovementHeaderForTest>();
		nctsHeader.MovementHeaders.Add(departureMovement);

		departureMovement.BM_PaperlessInbondNum = EntryReference;
		departureMovement.ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption = shouldConfirm;
		departureMovement.ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore = EntryReference;
		departureMovement.ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore = InternalReferenceType;
		departureMovement.ManuallySet_TemporaryStorageTransactionCommentPrefix = CommentPrefix;
		departureMovement.ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions = new ZString[] { "AAA" };
		departureMovement.ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions = new ZString[] { "BBB" };
		departureMovement.ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions = new ZString[] { "CCC", ZString.Empty };
		departureMovement.ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation = shouldFormatDocumentNumber;
		departureMovement.ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods = "SUM";
		departureMovement.ShouldGetPreviousDocumentsDeclared = shouldGetPreviousDocuments;
		departureMovement.SetCodeForGetPreviousDocumentsDeclared = "SUM";
		departureMovement.SetReferenceForGetPreviousDocumentsDeclared = docReference;
		departureMovement.SetLineNoForGetPreviousDocumentsDeclared = 1;
		departureMovement.ManuallySet_ReleaseDateForTemporaryStorage = releaseDate;

		departureMovement.GoodsLocation.CGL_AdditionalIdentifier = locationInEntry;

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = locationInPremises;
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		return departureMovement;
	}

	(NctsDepartureMovementHeader movementHeader, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine) SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction
	(string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, bool shouldConfirm = true, bool createTransaction = true, bool shouldGetPreviousDocuments = true
		, bool shouldFormatDocumentNumber = false, string regHeaderReference = RegHeaderReference, string docReference = RegHeaderReference)
	{
		var orgHeader = SetUpOrgHeader();
		var movementHeader = SetUpMovementHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, locationInEntry: locationInEntry, locationInPremises: locationInPremises, shouldConfirm: shouldConfirm, shouldGetPreviousDocuments: shouldGetPreviousDocuments, shouldFormatDocumentNumber: shouldFormatDocumentNumber, docReference: docReference);

		var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_CustomsStatus = "OPN";
		regLine.SRL_PackageType = "VQ";
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction = (CusTempStorageRegLineTransaction)null;
		if (createTransaction)
		{
			regLineTransaction = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
			regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction.SRT_SRL = regLine.PK;
		}

		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot1 = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine.PK;
		Factory.Save();

		return (movementHeader, regLineTransaction, regLine);
	}

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString transactionStatus, int packageQty = 0, decimal grossWeight = 0, string transactionType = "TRN", decimal bondAmount = 0.0m)
	{
		var regLineTransaction = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = transactionType;
		regLineTransaction.SRT_TransactionStatus = transactionStatus;
		regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
		regLineTransaction.SRT_PackageQty = packageQty;
		regLineTransaction.SRT_GrossWeight = grossWeight;

		if (transactionType == "OBL")
		{
			regLineTransaction.SRT_BondAmount = bondAmount;
		}
		else
		{
			regLineTransaction.SRT_InternalReferenceNumber = EntryReference;
		}

		return regLineTransaction;
	}

	void AssertConfirmedTransaction(ZString transactionName, CusTempStorageRegLineTransaction transaction, string comment = ExpectedComment)
	{
		AssertEquals(transactionName + "'s SRT_TransactionStatus was changed", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
		AssertEquals(transactionName + "'s SRT_ReferenceType was changed", "MRN", transaction.SRT_ReferenceType);
		AssertEquals(transactionName + "'s SRT_Reference was changed", MRNCode, transaction.SRT_Reference);
		AssertEquals(transactionName + "'s SRT_Comments was changed", comment, transaction.SRT_Comments);
		AssertEquals(transactionName + "'s SRT_TransactionDate was changed", issueDate.ToOffset(), transaction.SRT_TransactionDate);
		AssertEquals(transactionName + "'s SRT_PhysicalInOutDate was changed", releaseDate.ToOffset(), transaction.SRT_PhysicalInOutDate);
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
											"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
											"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
											"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
											"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
								"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
	}

	CusTempStorageRegHeader SetUpTmpRegHeader(string appCode = "AAA", string reference = RegHeaderReference)
	{
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = appCode;
		regHeader.SRH_Reference = reference;

		return regHeader;
	}

	CusGuaranteeHeader SetUpGauranteeForTempStorage(CusTempStorageRegHeader regHeader, ZDecimal value, OrgHeader orgHeader)
	{
		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
		cusGuarantee.CPH_Balance = 1000.0m;

		var commonGuarantee = Factory.New<CommonGuarantee>();
		commonGuarantee.PW_BondNumber = "Test1";
		commonGuarantee.PW_ParentID = regHeader.PK;
		commonGuarantee.PW_ParentTableCode = CusBondDetailSchema.Constants.Prefix;
		commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		var guarantee = regHeader.Guarantee.CusGuarantee;
		var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
		guaranteeLineTransaction.CPL_Reference = RegHeaderReference;
		guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		guaranteeLineTransaction.CPL_TranValue = value;

		guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		return guarantee;
	}

	CusTempStorageRegLine SetUpRegLine(CusTempStorageRegHeader regHeader)
	{
		var regLine = (CusTempStorageRegLine)regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_PackageType = "BX";

		return regLine;
	}

	OrgHeader SetUpOrgHeader()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		return orgHeader;
	}

	#endregion

	public void TestWarehouseTransactionStatusDescription()
	{
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, departureMovement.WarehouseTransactionStatusDescription);
			departureMovement.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			AssertEquals(WarehouseTransactionStatusList.Descriptions.OutwardCreatedPending, departureMovement.WarehouseTransactionStatusDescription);
		});
	}

	public void TestWarehouseTransactionStatusDescription_Caption()
	{
		var captionData = DataBoundResourceStrings.GetDataForProperty(typeof(NctsDepartureMovementHeader), nameof(NctsDepartureMovementHeader.WarehouseTransactionStatusDescription));
		AssertEquals("Warehouse Transaction Status Description", captionData.Caption);
		AssertEquals("WHS Trans. Status Des.", captionData.ShortCaption);
	}

	public void TestUpdateBM_GrossWeightFromBills()
	{
		departureMovement.BM_GrossWeight = ZDecimal.Zero;

		var bill1 = nctsHeader.Bills.AddNew();
		bill1.B0_Weight = new(1m);
		bill1.B0_WeightUQ = Weight.Kilograms;

		var bill2 = nctsHeader.Bills.AddNew();
		bill2.B0_Weight = new(1400m);
		bill2.B0_WeightUQ = Weight.Grams;

		CombineAssertions(() =>
		{
			departureMovement.UpdateBM_GrossWeightFromBills();

			AssertEquals(new ZDecimal(2.4m), departureMovement.BM_GrossWeight);
			AssertEquals(new ZString(Weight.Kilograms), departureMovement.BM_GrossWeightUQ);

			departureMovement.BM_GrossWeightUQ = Weight.Grams;
			departureMovement.UpdateBM_GrossWeightFromBills();

			AssertEquals(new ZDecimal(2400m), departureMovement.BM_GrossWeight);
			AssertEquals(new ZString(Weight.Grams), departureMovement.BM_GrossWeightUQ);
		});
	}

	#region DefaultDepartureLocationCodeFromCusAuthorisationIfBlank

	public void TestDefaultDepartureLocationCodeFromCusAuthorisationIfBlank()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.Header.Principal.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
		departureMovement.IsSimplifiedNctsProcedure = true;

		var configuration = nctsHeader.Configuration.LocationOfGoodsFromAuthorisationDefaulterConfiguration;
		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Configuration IsDefaultingEnabled", expected: true, configuration.IsDefaultingEnabled);
			AssertEquals("Pre-requisite: Configuration QualifierCode", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, configuration.QualifierCode);
			AssertEquals("Pre-requisite: Configuration TypeCode", CusGoodsLocationTypeList.Codes.AuthorizedPlace, configuration.TypeCode);
		});

		var cusAuthorisationUsage = departureMovement.CusAuthorizationUsages.AddNew();
		cusAuthorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorisationUsage.AGC_Number = "AR10001";
		cusAuthorisationUsage.AGC_OH_Owner = GlbCompany.CurrentCompany.OrgProxy.PK;

		var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorisationHeader.CPH_Number = "AR10001";
		cusAuthorisationHeader.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;

		var goodsLocation = departureMovement.GoodsLocation;

		var rule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		rule.CPR_ValueFrom = "A000";
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, cusAuthorisationHeader.CPH_OH_PermitHolder, rule.CPR_ValueFrom);

		ClearDepartureGoodsLocation(goodsLocation);

		var rule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule1.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		rule1.CPR_ValueFrom = "A001";
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty);

		ClearDepartureGoodsLocation(goodsLocation);
		cusAuthorisationHeader.CusAuthorisationRules.DeleteAll();

		var rule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		rule2.CPR_ValueFrom = "A002";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule2, Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "PT000001");
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, cusAuthorisationHeader.CPH_OH_PermitHolder, rule2.CPR_ValueFrom);

		ClearDepartureGoodsLocation(goodsLocation);

		var rule3 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule3.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		rule3.CPR_ValueFrom = "A003";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule3, Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "PT000002");
		_ = departureMovement.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDeparture, "PT000002");
		AssertEquals("Departure Customs Office", "PT000002", departureMovement.DepartureCustomsOffice.OfficeCode);
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, cusAuthorisationHeader.CPH_OH_PermitHolder, rule3.CPR_ValueFrom);

		ClearDepartureGoodsLocation(goodsLocation);

		var rule4 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		rule4.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		rule4.CPR_ValueFrom = "A004";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule4, Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "PT000002");
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty);

		ClearDepartureGoodsLocation(goodsLocation);

		departureMovement.Header.Principal.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[1].PK;
		var cusAuthorisationHeader1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		cusAuthorisationHeader1.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorisationHeader1.CPH_Number = "AR10002";
		cusAuthorisationHeader1.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.OrgProxy.PK;
		cusAuthorisationHeader1.CPH_OA_AppliesTo = GlbCompany.CurrentCompany.OrgProxy.Addresses[1].PK;

		var rule5 = cusAuthorisationHeader1.CusAuthorisationRules.AddNew();
		rule5.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		rule5.CPR_ValueFrom = "A005";
		_ = NCTSTestHelper.CreateLinkedAuthorisationRuleForTest(rule5, Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "PT000002");
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, configuration.QualifierCode, configuration.TypeCode, cusAuthorisationHeader.CPH_OH_PermitHolder, rule5.CPR_ValueFrom);

		ClearDepartureGoodsLocation(goodsLocation);
		_ = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();
		AssertEquals("Principal Addresses Count", 3, GlbCompany.CurrentCompany.OrgProxy.Addresses.Count);
		departureMovement.Header.Principal.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.Addresses[2].PK;
		departureMovement.UpdateAuthorizationUsageFromPrincipal(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, departureMovement.IsSimplifiedNctsProcedure);
		AssertDepartureGoodsLocation(goodsLocation, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty);
	}

	void ClearDepartureGoodsLocation(CusGoodsLocation goodsLocation)
	{
		goodsLocation.CGL_Qualifier = ZString.Empty;
		goodsLocation.CGL_Type = ZString.Empty;
		goodsLocation.Address.IdentificationHolderPK = ZGuid.Empty;
		goodsLocation.Address.AuthorisationNumber = ZString.Empty;
	}

	void AssertDepartureGoodsLocation(CusGoodsLocation goodsLocation, ZString qualifier, ZString place, ZGuid holder, ZString number)
	{
		CombineAssertions(() =>
		{
			AssertEquals("Goods Location Qualifier", qualifier, goodsLocation.CGL_Qualifier);
			AssertEquals("Goods Location Authorized Place", place, goodsLocation.CGL_Type);
			AssertEquals("Goods Location Authorization Holder", holder, goodsLocation.Address.IdentificationHolderPK);
			AssertEquals("Goods Location Authorization Number", number, goodsLocation.Address.AuthorisationNumber);

			var displayText = new ZStringBuilder().AppendIfNotEmpty(qualifier).AppendIfNotEmpty(place).AppendIfNotEmpty(number).ToStringWithDelimiterBetweenAppends(";").TrimEnd(';');
			AssertEquals("Goods Location Display Text", displayText, goodsLocation.DisplayText);
		});
	}

	#endregion

	public void TestDocManagerInfo() => AssertType<DepartureMovementHeaderDocManagerInfo>(departureMovement.DocManagerInfo);

	public void TestGuaranteeTransactionCoordinator() => AssertType<GuaranteeTransactionCoordinator>(departureMovement.GuaranteeTransactionCoordinator);

	public void TestForbidDeletion()
	{
		var movementHeader = Factory.New<NctsDepartureMovementHeader>();
		CombineAssertions("Default CanDelete and ReasonForNotAbleToDelete behavior", () =>
		{
			AssertEquals("CanDelete", true, movementHeader.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "", movementHeader.ReasonForNotAbleToDelete);
		});

		movementHeader.ForbidDeletion();
		CombineAssertions("CanDelete and ReasonForNotAbleToDelete behavior once deletion has been disabled", () =>
		{
			AssertEquals("CanDelete", false, movementHeader.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "The main departure movement cannot be deleted.", movementHeader.ReasonForNotAbleToDelete);
		});
	}

	public void TestSupportingDocuments()
	{
		AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(departureMovement.SupportingDocuments);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = ((ICusSupportingInfoTypeSupporter)departureMovement).GetCusSupportingInfoTypes();
		CombineAssertions(() =>
		{
			AssertEquals("#CusSupportingInfoTypes", 1, cusSupportingInfoTypes.Count);
			AssertEquals("SUP", typeof(NctsSupportingDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		});
	}

	public void TestIsLocationManagedInPremises() => CombineAssertions(() =>
	{
		var adtPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
		adtPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		adtPremises.SRP_CustomsLocation = "C001";
		var lamPremises = Factory.New<Integration.Customs.EU.ICusTempStorageRegPremises>();
		lamPremises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
		lamPremises.SRP_CustomsLocation = "C002";

		departureMovement.GoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
		AssertEquals("CGL_AdditionalIdentifier is empty", false, departureMovement.IsLocationManagedInPremises);

		departureMovement.GoodsLocation.CGL_AdditionalIdentifier = "C001";
		AssertEquals("CGL_AdditionalIdentifier is known TemporaryStorageWarehouse", true, departureMovement.IsLocationManagedInPremises);

		departureMovement.GoodsLocation.CGL_AdditionalIdentifier = "C002";
		AssertEquals("CGL_AdditionalIdentifier not TemporaryStorageWarehouse", false, departureMovement.IsLocationManagedInPremises);

		departureMovement.GoodsLocation.CGL_AdditionalIdentifier = "C999";
		AssertEquals("CGL_AdditionalIdentifier unkown", false, departureMovement.IsLocationManagedInPremises);
	});

	public void TestMovementReferenceNumber()
	{
		var mrn = CusEntryNumber.LoadOrCreate(departureMovement, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
		mrn.CE_EntryNum = "MRN123";
		AssertEquals("MRN123", departureMovement.MovementReferenceNumber);
	}

	void AssertIsSimplifiedNctsProcedure_ShouldAddAuthorization_WhenAuthorizationValid(string authorizationCode)
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader = CreateCusAuthorizationHeader(authorizationCode, organization);

		departureMovement.Header.Principal.OrganisationPK = organization.PK;

		AssertCollectionNotContains("Precondition", departureMovement.CusAuthorizationUsages, cau => cau.AGC_Code == authorizationCode);

		// Act
		departureMovement.IsSimplifiedNctsProcedure = true;

		// Assert
		AssertCollectionContains(departureMovement.CusAuthorizationUsages,
								cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
									cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
									cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
	}

	void AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenMoreThanOneValidAuthorization(string authorizationCode)
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader1 = CreateCusAuthorizationHeader(authorizationCode, organization);
		var cusAuthorizationHeader2 = CreateCusAuthorizationHeader(authorizationCode, organization);
		departureMovement.Header.Principal.OrganisationPK = organization.PK;

		// Act
		departureMovement.IsSimplifiedNctsProcedure = true;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader1.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader1.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader1.CPH_OH_PermitHolder);
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader2.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader2.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader2.CPH_OH_PermitHolder);
	}

	void AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenAuthorizationInvalid(string authorizationCode)
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader = CreateCusAuthorizationHeader(authorizationCode, organization);
		cusAuthorizationHeader.CPH_StartDate = ZDate.Today.AddDays(1);
		cusAuthorizationHeader.CPH_EndDate = ZDate.Today.AddDays(2);
		departureMovement.Header.Principal.OrganisationPK = organization.PK;

		// Act
		departureMovement.IsSimplifiedNctsProcedure = true;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
	}

	void AssertIsSimplifiedNctsProcedure_ShouldNotAddAuthorization_WhenPrincipalNotSet(string authorizationCode)
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var organization = Factory.NewWithValidTestData<OrgHeader>();

		var cusAuthorizationHeader = CreateCusAuthorizationHeader(authorizationCode, organization);
		departureMovement.Header.Principal.OrganisationPK = ZGuid.Empty;

		// Act
		departureMovement.IsSimplifiedNctsProcedure = true;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages,
									cau => cau.AGC_Code == cusAuthorizationHeader.CPH_Type &&
										cau.AGC_Number == cusAuthorizationHeader.CPH_Number &&
										cau.AGC_OH_Owner == cusAuthorizationHeader.CPH_OH_PermitHolder);
	}

	void AssertIsSimplifiedNctsProcedure_ShouldDeleteAuthorization(string authorizationCode)
	{
		// Arrange
		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.IsSimplifiedNctsProcedure = true;
		var cusAuthorizationHeader = departureMovement.CusAuthorizationUsages.AddNew();
		cusAuthorizationHeader.AGC_Code = authorizationCode;

		// Act
		departureMovement.IsSimplifiedNctsProcedure = false;

		// Assert
		AssertCollectionNotContains(departureMovement.CusAuthorizationUsages, cusAuthorizationHeader);
	}

	CusAuthorisationHeader CreateCusAuthorizationHeader(string type, OrgHeader organization)
	{
		var cusAuthorisationHeader = Factory.New<CusAuthorisationHeader>();
		cusAuthorisationHeader.CPH_Type = type;
		cusAuthorisationHeader.CPH_OH_PermitHolder = organization.PK;
		cusAuthorisationHeader.CPH_Number = "123";
		cusAuthorisationHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusAuthorisationHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		return cusAuthorisationHeader;
	}

	protected override BusinessObject GetNewBusinessObject() => departureMovement;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}

	void SetPhase5DepartureMovementHeader()
	{
		var phase5Header = Factory.New<NctsHeader>();
		phase5Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		phase5Header.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader = phase5Header;
		departureMovement = phase5Header.MovementHeader;
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
		configurationTestContext = departureMovement.CreateConfigurationTestContext();
	}

	protected override void TearDown()
	{
		base.TearDown();
		configurationTestContext?.Dispose();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;
	NctsConfigurationTestContext configurationTestContext;

	#region Custom Fields Test
	[TestedType(typeof(NctsDepartureMovementHeader))]
	sealed class NctsDepartureMovementHeaderCustomFieldsTest : TestICustomFieldProvider
	{
	}
	#endregion

	#region NctsDepartureMovementHeaderForTest

	class NctsDepartureMovementHeaderForTest : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString TIRCarnetSupportingDocumentCodeExposed => TIRCarnetSupportingDocumentCode;

		protected override (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetGoodsItemDataDeclaredForDepartureToReserveTSGoodsCore() => ManuallySet_GetGoodsItemDataDeclaredForDepartureToReserveTSGoodsCore();
		public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) ManuallySet_GetGoodsItemDataDeclaredForDepartureToReserveTSGoodsCore()
		{
			if (ShouldGetPreviousDocumentsDeclared)
			{
				var doc = Factory.New<NctsPreviousDocument>();
				doc.CSI_Code = SetCodeForGetPreviousDocumentsDeclared;
				doc.CSI_ReferenceNumber = SetReferenceForGetPreviousDocumentsDeclared;
				doc.CSI_LineNo = SetLineNoForGetPreviousDocumentsDeclared;
				var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
				{
					new ()
					{
						Document = doc,
						Packages = new (ZString, ZInt, ZString, ZBool)[]
						{
							("FR", 1, "VIN1", false),
							("BX", 8, "", false),
							("VQ", 0, "", true),
						},
						TotalGrossWeight = 30.6m
					}
				};

				if (ShouldHaveSecondPreviousDocumentDeclared)
				{
					var doc2 = Factory.New<NctsPreviousDocument>();
					doc2.CSI_Code = SetCodeForGetPreviousDocumentsDeclared;
					doc2.CSI_ReferenceNumber = SetReferenceForGetPreviousDocumentsDeclared2;
					doc2.CSI_LineNo = SetLineNoForGetPreviousDocumentsDeclared;
					dataToReturn.Add(
						new DeclarationDataToReserveTSGoods()
						{
							Document = doc2,
							Packages = new (ZString, ZInt, ZString, ZBool)[]
							{
								("VQ", 1, "", true),
							},
							TotalGrossWeight = 20m,
							TotalGrossWeightForVINs = 0m,
						});
				}

				return (dataToReturn, ZString.Empty);
			}
			else
			{
				return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), ZString.Empty);
			}
		}

		public ZBool ShouldGetPreviousDocumentsDeclared { private get; set; }
		public ZBool ShouldHaveSecondPreviousDocumentDeclared { private get; set; }
		public ZString SetCodeForGetPreviousDocumentsDeclared { private get; set; }
		public ZString SetReferenceForGetPreviousDocumentsDeclared { private get; set; }
		public ZString SetReferenceForGetPreviousDocumentsDeclared2 { private get; set; }
		public ZInt SetLineNoForGetPreviousDocumentsDeclared { private get; set; }

		protected override ZBool ShouldConfirmTemporaryStorageGoodsConsumption => ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption;

		public bool ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption { private get; set; }

		protected override ZString TemporaryStorageTransactionInternalReferenceNumberCore => ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore;

		public ZString ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore { private get; set; }

		protected override ZString TemporaryStorageTransactionInternalReferenceTypeCore => ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore;

		public ZString ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore { private get; set; }

		protected override IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions;

		public ZString[] ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions { private get; set; }

		protected override IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions;

		public ZString[] ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions { private get; set; }

		protected override IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions;

		public ZString[] ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions { private get; set; }

		protected override ZString PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods;

		public ZString ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods { private get; set; }

		protected override ZString TemporaryStorageTransactionCommentPrefix => ManuallySet_TemporaryStorageTransactionCommentPrefix;

		public ZString ManuallySet_TemporaryStorageTransactionCommentPrefix { private get; set; }

		protected override ZDateTime ReleaseDateForTemporaryStorage => ManuallySet_ReleaseDateForTemporaryStorage;

		public ZDateTime ManuallySet_ReleaseDateForTemporaryStorage { private get; set; }

		protected override ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation;

		public ZBool ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation { private get; set; }

		protected override ZString GetDocumentNumberFormat(ZString dsdtMRN) => dsdtMRN + "AAA";
	}

	#endregion
}
