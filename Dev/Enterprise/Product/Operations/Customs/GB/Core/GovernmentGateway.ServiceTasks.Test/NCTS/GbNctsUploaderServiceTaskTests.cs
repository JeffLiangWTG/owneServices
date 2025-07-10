using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	class GbNctsUploaderServiceTaskTests : TestCaseWithFactory
	{
		public void TestNctsMessageSenderExistForGB()
		{
			SetupNctsIE15ForTest(Factory);
			var chooser = new NctsMessageSenderChooser(ie15, new NctsMessageFunctionSet.DeclarationDataMessage());
			AssertNotNull(chooser);
			var messageSender = chooser.CountrySpecificChooser;
			AssertType<NctsMessageSender>(messageSender);
			AssertEquals("GB", chooser.NctsDomainCountryCode);
		}

		NctsDepartureCargoDesc AddGoodsItemForTest(ZString goodsDecription, ZDecimal grossMassInKilograms, ZDecimal netMassInKilograms)
		{
			var goodsItem = ie15.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = "123.456.789";
			goodsItem.BY_Type = "T2";
			goodsItem.BY_Description = goodsDecription;
			goodsItem.BY_GrossWeight = grossMassInKilograms;
			goodsItem.BY_GrossWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			goodsItem.BY_NetWeight = netMassInKilograms;
			goodsItem.BY_NetWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			goodsItem.BY_RN_NKCountryOfDispatch = "GB";
			goodsItem.BY_RN_NKCountryOfDestination = "IT";
			goodsItem.BY_TransportChargesMethodOfPayment = "B";
			goodsItem.BY_CommercialReferenceNumber = "HQDOV002";
			AddPreviousDocumentForTest(goodsItem, "PD1", "PD123");
			AddSupportingDocumentForTest(goodsItem, "316", "SD001", "Pre Entry Lin=9999");
			AddSpecialMentionForTest(goodsItem, "Special Mention", "XXXXX", true, "GB");
			AddSgiForTest(goodsItem, "SGIXX", 12.1m);
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			goodsItem.UNDGs.AddNew().LinkDefault(subs);
			CreateJobDocAddressForTest(goodsItem.Consignor, "CARGOW", "ITEM CONSIGNOR @ NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GB", "GB954131533000", "");
			CreateJobDocAddressForTest(goodsItem.Consignee, "CARGOW", "ITEM CONSIGNEE @ NCTS UK TEST LAB HMCE", "ITALIAN OFFICE", "IT99 1IT", "MILAN", "IT", "IT11ITALIANC11", "");
			CreateJobDocAddressForTest(goodsItem.SecurityConsignor, "CARGOW", "ITEM SECURITY CONSIGNOR @ NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GB", "GB954131533000", "");
			CreateJobDocAddressForTest(goodsItem.SecurityConsignee, "CARGOW", "ITEM SECURITY CONSIGNEE @ NCTS UK TEST LAB HMCE", "ITALIAN OFFICE", "IT99 1IT", "MILAN", "IT", "IT11ITALIANC11", "");
			return goodsItem;
		}

		void AddSupportingDocumentForTest(NctsCommonCargoDesc goodsItem, ZString typeCode, ZString reference, ZString moreInfo)
		{
			var sd = goodsItem.SupportingDocuments.AddNew();
			sd.CSI_Code = typeCode;
			sd.CSI_ReferenceNumber = reference;
			sd.CSI_Description = moreInfo;
		}

		void AddPreviousDocumentForTest(NctsDepartureCargoDesc goodsItem, ZString typeCode, ZString reference)
		{
			var pd = goodsItem.PreviousDocuments.AddNew();
			pd.CSI_SubType = PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS;
			pd.CSI_Code = typeCode;
			pd.CSI_ReferenceNumber = reference;
		}

		void AddSpecialMentionForTest(NctsCommonCargoDesc goodsItem, ZString additonalInfo, ZString additionalInfoType, ZBool exportFromEC, ZString exportFromCountry)
		{
			var sm = goodsItem.AdditionalInfos.AddNew();
			sm.CSI_Description = additonalInfo;
			sm.CSI_Code = additionalInfoType;
			sm.CSI_NctsExportFromEC = exportFromEC;
			sm.CSI_RN_NKCountryCode = exportFromCountry;
		}

		void AddSgiForTest(NctsCommonCargoDesc goodsItem, ZString sensitiveGoodsCode, ZDecimal sensitiveGoodsQuantity)
		{
			var sgi = goodsItem.AdditionalInfos.AddNew();
			sgi.CSI_Code = sensitiveGoodsCode;
			sgi.CSI_Description = sensitiveGoodsQuantity.ToString();
		}

		void AddPackageForTest(NctsCommonCargoDesc goodsItem, ZString marksAndNumbers, ZString packageType, ZLong packageCount)
		{
			var package = goodsItem.Packages.AddNew();
			package.B5_MarksAndNumbers = marksAndNumbers;
			package.B5_UnitType = packageType;
			package.B5_UnitCount = packageCount;
		}

		void AddGuaranteeForTest(ZString guaranteeType, ZString referenceNumber, ZString accessCode, ZString otherReference, ZString validityLimitation)
		{
			var guarantee1 = ie15.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondType = guaranteeType;
			guarantee1.PW_BondNumber = referenceNumber;
			guarantee1.PW_BondNumber2 = otherReference;
			guarantee1.PW_Password = accessCode;
			guarantee1.PW_ValidityLimitation = validityLimitation;
		}

		ZGuid CreateJobDocAddressForTest(JobDocAddress jda, ZString traderCode, ZString traderName, ZString address1, ZString postCode, ZString city, ZString countryCode, ZString traderTin, ZString relatedPortCode)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = traderCode;
			org.OH_FullName = traderName;
			var address = org.Addresses.AddNew();
			address.OA_Address1 = address1;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
			if (jda != null)
			{
				jda.E2_OA_Address = address.PK;
			}
			return org.PK;
		}

		NctsHeader SetupNctsIE15ForTest(BusinessObjectFactory factory)
		{
			ie15 = factory.New<NctsHeader>();
			ie15.SetMovementType(NctsMovementType.Codes.Departure);
			ie15.MovementHeader.BM_InBondEntryType = "T1";
			ie15.MovementHeader.BM_RL_NKDestinationPort = "IT";
			ie15.MovementHeader.BM_LocationOfGoods = "Pre-Lodged";
			ie15.MovementHeader.BM_LocationOfGoodsCode = "954131533-GB60DEP"; // TODO Port of presentation - GBLHRBAC
			ie15.MovementHeader.BM_RL_NKForeignDestPort = "GBDVR";
			ie15.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
			ie15.MovementHeader.BM_TOLCarrierID = "NC15REG";
			ie15.MovementHeader.BM_EntryDate = ZDateTime.Today;

			NCTSTestHelper.SetupContainersAndSealsForTest(ie15);
			CreateJobDocAddressForTest(ie15.Principal, "CARGOW", "PRINCIPAL @ NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GB", "GB954131533000", "");
			CreateJobDocAddressForTest(ie15.Consignor, "CARGOW", "CONSIGNOR @ NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GB", "GB954131533000", "");
			CreateJobDocAddressForTest(ie15.Consignee, "CARGOW", "CONSIGNEE @ NCTS UK TEST LAB HMCE", "ITALIAN OFFICE", "IT99 1IT", "MILAN", "IT", "IT11ITALIANC11", "");
			ie15.BH_OH_Carrier = CreateJobDocAddressForTest(null, "CARGOW", "CARRIER @ NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GB", "GB954131533000", "");
			CreateJobDocAddressForTest(ie15.SecurityConsignor, "CARGOW", "SECURITY CONSIGNOR @ NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GB", "GB954131533000", "");
			CreateJobDocAddressForTest(ie15.SecurityConsignee, "CARGOW", "SECURITY CONSIGNEE @ NCTS UK TEST LAB HMCE", "ITALIAN OFFICE", "IT99 1IT", "MILAN", "IT", "IT11ITALIANC11", "");

			var departureOffice = ie15.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			departureOffice.CY_Data = "GB000060";
			var destinationOffice = ie15.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			destinationOffice.CY_Data = "IT021300";

			ie15.MovementHeader.BM_GONumber = "A3";
			ie15.MovementHeader.BM_ExportDate = ZDate.Today.AddDays(8);

			var goodsItem = AddGoodsItemForTest("CARGOWISE, NINAD WAGLE, 0845 1255211", 1000, 960);
			NCTSTestHelper.SetContainerPivotForTest(goodsItem, 1);
			AddPackageForTest(goodsItem, "AB234", "BX", 10L);

			AddGuaranteeForTest("1", "09GB00000100000M0", "AC01", "OR02", "");

			var carrier = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "CIREQU");
			var carrierAddress = carrier.MainAddress;
			ie15.BH_OH_Carrier = carrierAddress.PK;
			ie15.MovementHeader.BM_AdditionalText = "HQDOV001";
			ie15.MovementHeader.BM_ConveyanceNumber = "REF1234";
			ie15.MovementHeader.BM_RL_NKDestinationPort = "IT";
			ie15.BH_RL_NKImportLoadPort = "AU";
			ie15.MovementHeader.BM_CustomsSubPlace = "DOVER ERTS";

			ie15.MovementHeader.BM_InlandTransportMode = "3";
			NCTSTestHelper.AddItineraryCountryForTest(ie15, "DE");
			NCTSTestHelper.AddItineraryCountryForTest(ie15, "FR");

			ie15.MovementHeader.BM_TransportAtDeparture = "NC15REG";
			ie15.MovementHeader.BM_RN_NKTransportAtDepartureCountry = "US";
			ie15.MovementHeader.BM_TOLCarrierID = "NC25REG";
			ie15.MovementHeader.BM_TOLCarrierCode = "AU";
			ie15.MovementHeader.BM_ExportTransportMode = "12";

			ie15.PlaceOfUnloadingCode = "ITBGO";
			ie15.MovementHeader.BM_RL_NKDestinationPort = "IT";
			SetupStaffForTest();
			ie15.MovementHeader.BM_GS_NKCusAgent = "A35";

			ie15.MovementHeader.BM_BTAIndicator = "1";

			ie15.MovementHeader.BM_MethodOfPayment = "A";
			ie15.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;

			return ie15;
		}

		void SetupStaffForTest()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "REPRESENTATIVE";
			staff.GS_Code = "A35";
		}

		NctsHeader ie15;
	}
}
