using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;
using TransportDocumentTypes = Enterprise.Customs.ASYCUDA.Business.TransportDocumentTypes;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestBillPacksDeletedOnDeleteForDataRefresh()
		{
			var forwardingFactory = new BusinessObjectFactory();
			var forwardingConsol = forwardingFactory.NewWithValidTestData<ForwardingConsol>();
			var forwardingShipment = forwardingConsol.Shipments.AddNew();
			forwardingShipment.OuterPackLines.RemoveAndDeleteAll();
			forwardingShipment.OuterPackLines.AddNew();
			forwardingShipment.OuterPackLines.AddNew();
			forwardingFactory.Save();

			var wrapper = new ManifestHeadersWrapper(forwardingConsol);
			var header = wrapper.CreateCountry("EU", "ENS");
			forwardingFactory.Save();
			var billPk = header.Bills[0].PK;
			var newFactory = new BusinessObjectFactory();

			var headerNewFactory = newFactory.Load<AsycudaManifestHeader>(header.PK);
			headerNewFactory.HasChanges = true;
			headerNewFactory.Bills.RemoveAndDeleteAll();
			headerNewFactory.Factory.Save();
			AssertEquals(0, forwardingFactory.Load<AsycudaPack>(new ZQuery(AsycudaPackSchema.APA_ABL_Bill, billPk)).Length);
			AssertNoExceptionThrown(() => forwardingConsol.Factory.Save());
		}

		public void TestCanDeleteWhenNotDefaulting()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(true, bill.CanDelete);
			header.SetParent(Factory.New<ForwardingConsol>());
			AssertEquals(false, bill.CanDelete);
			AssertEquals("Cannot delete Bills when defaulting values from Consol.", bill.ReasonForNotAbleToDelete);

			header.AMA_OverrideFreightDefaults = true;
			AssertEquals(true, bill.CanDelete);

			AssertEquals(ZString.Empty, bill.ReasonForNotAbleToDelete);
		}

		public void TestRegNoTypes_BillParties()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertContainsExactElementsInAnyOrder("Shipper", new[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori }, bill.ShipperRegNoTypes());
			AssertContainsExactElementsInAnyOrder("Buyer", new[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori }, bill.BuyerRegNoTypes());
			AssertContainsExactElementsInAnyOrder("Seller", new[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori }, bill.SellerRegNoTypes());
			AssertContainsExactElementsInAnyOrder("Consignee", new[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori }, bill.ConsigneeRegNoTypes());
			AssertContainsExactElementsInAnyOrder("NotifyParty", new[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori }, bill.NotifyPartyRegNoTypes());
		}

		public void TestRegNoAndRegNoType_BillParties()
		{
			TestHelper.PrepareCL010(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("pre-condition Shipper", ZString.Empty, bill.ABL_ShipperRegNo);
			AssertEquals("pre-condition Buyer", ZString.Empty, bill.ABL_BuyerRegNo);
			AssertEquals("pre-condition Seller", ZString.Empty, bill.ABL_SellerRegNo);
			AssertEquals("pre-condition Consignee", ZString.Empty, bill.ABL_ConsigneeRegNo);
			AssertEquals("pre-condition NotifyParty", ZString.Empty, bill.ABL_NotifyPartyRegNo);

			CombineAssertions(() =>
			{
				var orgHeaderWithEuEoriNumberAndGbEoriNumber = GetOrgHeaderWithEuEoriAndGbEori(Factory, "TESTORG", "12345", "DE", "FR");
				bill.ABL_OA_Shipper = orgHeaderWithEuEoriNumberAndGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_Buyer = orgHeaderWithEuEoriNumberAndGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_Seller = orgHeaderWithEuEoriNumberAndGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_Consignee = orgHeaderWithEuEoriNumberAndGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_NotifyParty = orgHeaderWithEuEoriNumberAndGbEoriNumber.MainAddress.PK;
				AssertEquals("Shipper with EORI: RegNo", "DE12345", bill.ABL_ShipperRegNo);
				AssertEquals("Shipper with EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_ShipperRegNoType);
				AssertEquals("Buyer with EORI: RegNo", "DE12345", bill.ABL_BuyerRegNo);
				AssertEquals("Buyer with EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_BuyerRegNoType);
				AssertEquals("Seller with EORI: RegNo", "DE12345", bill.ABL_SellerRegNo);
				AssertEquals("Seller with EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_SellerRegNoType);
				AssertEquals("Consignee with EORI: RegNo", "DE12345", bill.ABL_ConsigneeRegNo);
				AssertEquals("Consignee with EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_ConsigneeRegNoType);
				AssertEquals("NotifyParty with EORI: RegNo", "DE12345", bill.ABL_NotifyPartyRegNo);
				AssertEquals("NotifyParty with EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_NotifyPartyRegNoType);

				var orgHeaderWithGbEoriNumber = GetOrUpdateOrgHeaderWithEori(Factory, "TESTORG2", "12345", Core.Constants.CountryCodes.UnitedKingdom, mainAddressCoutryCode: Core.Constants.CountryCodes.Germany);
				bill.ABL_OA_Shipper = orgHeaderWithGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_Buyer = orgHeaderWithGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_Seller = orgHeaderWithGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_Consignee = orgHeaderWithGbEoriNumber.MainAddress.PK;
				bill.ABL_OA_NotifyParty = orgHeaderWithGbEoriNumber.MainAddress.PK;
				AssertEquals("Shipper without EORI: RegNo", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertEquals("Shipper without EORI: RegNoType", ZString.Empty, bill.ABL_ShipperRegNoType);
				AssertEquals("Buyer without EORI: RegNo", ZString.Empty, bill.ABL_BuyerRegNo);
				AssertEquals("Buyer without EORI: RegNoType", ZString.Empty, bill.ABL_BuyerRegNoType);
				AssertEquals("Seller without EORI: RegNo", ZString.Empty, bill.ABL_SellerRegNo);
				AssertEquals("Seller without EORI: RegNoType", ZString.Empty, bill.ABL_SellerRegNoType);
				AssertEquals("Consignee without EORI: RegNo", ZString.Empty, bill.ABL_ConsigneeRegNo);
				AssertEquals("Consignee without EORI: RegNoType", ZString.Empty, bill.ABL_ConsigneeRegNoType);
				AssertEquals("NotifyParty without EORI: RegNo", ZString.Empty, bill.ABL_NotifyPartyRegNo);
				AssertEquals("NotifyParty without EORI: RegNoType", ZString.Empty, bill.ABL_NotifyPartyRegNoType);

				var orgWithoutEoriNumber = Factory.NewWithValidTestData<OrgHeader>();
				bill.ABL_OA_Shipper = orgWithoutEoriNumber.MainAddress.PK;
				bill.ABL_OA_Buyer = orgWithoutEoriNumber.MainAddress.PK;
				bill.ABL_OA_Seller = orgWithoutEoriNumber.MainAddress.PK;
				bill.ABL_OA_Consignee = orgWithoutEoriNumber.MainAddress.PK;
				bill.ABL_OA_NotifyParty = orgWithoutEoriNumber.MainAddress.PK;
				AssertEquals("Shipper without EORI: RegNo", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertEquals("Shipper without EORI: RegNoType", ZString.Empty, bill.ABL_ShipperRegNoType);
				AssertEquals("Buyer without EORI: RegNo", ZString.Empty, bill.ABL_BuyerRegNo);
				AssertEquals("Buyer without EORI: RegNoType", ZString.Empty, bill.ABL_BuyerRegNoType);
				AssertEquals("Seller without EORI: RegNo", ZString.Empty, bill.ABL_SellerRegNo);
				AssertEquals("Seller without EORI: RegNoType", ZString.Empty, bill.ABL_SellerRegNoType);
				AssertEquals("Consignee without EORI: RegNo", ZString.Empty, bill.ABL_ConsigneeRegNo);
				AssertEquals("Consignee without EORI: RegNoType", ZString.Empty, bill.ABL_ConsigneeRegNoType);
				AssertEquals("NotifyParty without EORI: RegNo", ZString.Empty, bill.ABL_NotifyPartyRegNo);
				AssertEquals("NotifyParty without EORI: RegNoType", ZString.Empty, bill.ABL_NotifyPartyRegNoType);
			});
		}

		public void TestRegNoAndRegNoType_BillParties_OnMultiple()
		{
			TestHelper.PrepareCL010(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var orgHeaderWithMultipleMatchingEoriNumber = GetOrUpdateOrgHeaderWithEori(Factory, "TESTORG3", "12345", CountryCodes.Germany, CountryCodes.Germany);
			GetOrUpdateOrgHeaderWithEori(Factory, "TESTORG4", "12345", CountryCodes.Germany, orgHeader: orgHeaderWithMultipleMatchingEoriNumber);
			bill.ABL_OA_Shipper = orgHeaderWithMultipleMatchingEoriNumber.MainAddress.PK;
			bill.ABL_OA_Buyer = orgHeaderWithMultipleMatchingEoriNumber.MainAddress.PK;
			bill.ABL_OA_Seller = orgHeaderWithMultipleMatchingEoriNumber.MainAddress.PK;
			bill.ABL_OA_Consignee = orgHeaderWithMultipleMatchingEoriNumber.MainAddress.PK;
			bill.ABL_OA_NotifyParty = orgHeaderWithMultipleMatchingEoriNumber.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Shipper with multiple EORI: RegNo", "* multiple found", bill.ABL_ShipperRegNo);
				AssertEquals("Shipper with multiple EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_ShipperRegNoType);
				AssertEquals("Buyer with multiple EORI: RegNo", "* multiple found", bill.ABL_BuyerRegNo);
				AssertEquals("Buyer v EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_BuyerRegNoType);
				AssertEquals("Seller with multiple EORI: RegNo", "* multiple found", bill.ABL_SellerRegNo);
				AssertEquals("Seller with multiple EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_SellerRegNoType);
				AssertEquals("Consignee with multiple EORI: RegNo", "* multiple found", bill.ABL_ConsigneeRegNo);
				AssertEquals("Consignee with multiple EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_ConsigneeRegNoType);
				AssertEquals("NotifyParty with multiple EORI: RegNo", "* multiple found", bill.ABL_NotifyPartyRegNo);
				AssertEquals("NotifyParty with multiple EORI: RegNoType", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, bill.ABL_NotifyPartyRegNoType);
			});
		}

		public void TestCodeAndDescriptionProperty()
		{
			var bill = Factory.New<AsycudaBill>();
			var code = "";
			var description = "";
			AssertNoExceptionThrown(() =>
			{
				code = CodePropertyAttribute.CodePropertyNameFromType(typeof(AsycudaBill));
				description = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(AsycudaBill));
			});
			AssertEquals(AsycudaBill.Schema.ABL_BillNumber, code);
			AssertEquals(AsycudaBill.Schema.ABL_BillNumber, description);
			AssertEquals(bill.ABL_BillNumber, CodePropertyAttribute.CodeFromBusinessObject(bill));
			AssertEquals(bill.ABL_BillNumber, DescriptionPropertyAttribute.DescriptionFromBusinessObject(bill));
		}

		public void TestSpecificCircumstanceCompatibilityAtBillLevel()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var supportingDocument = bill.SupportingDocuments.AddNew();

			var applicableSpecificCircumstanceIndicatorList = new string[] {
				EUICS2SpecificCircumstanceList.Codes.F10,
				EUICS2SpecificCircumstanceList.Codes.F11,
				EUICS2SpecificCircumstanceList.Codes.F12,
				EUICS2SpecificCircumstanceList.Codes.F13,
				EUICS2SpecificCircumstanceList.Codes.F14,
				EUICS2SpecificCircumstanceList.Codes.F15,
				EUICS2SpecificCircumstanceList.Codes.F20,
				EUICS2SpecificCircumstanceList.Codes.F22,
				EUICS2SpecificCircumstanceList.Codes.F26,
				EUICS2SpecificCircumstanceList.Codes.F27,
				EUICS2SpecificCircumstanceList.Codes.F30,
				EUICS2SpecificCircumstanceList.Codes.F32,
				EUICS2SpecificCircumstanceList.Codes.F43,
				EUICS2SpecificCircumstanceList.Codes.F50,
				EUICS2SpecificCircumstanceList.Codes.F51
			};

			foreach (var item in new EUICS2SpecificCircumstanceList().GetAllCodes())
			{
				manifestHeader.SpecificCircumstanceIndicator = item;
				supportingDocument.RunPreSaveValidation();

				if (applicableSpecificCircumstanceIndicatorList.Contains(item))
				{
					AssertNoRowWarnings(supportingDocument);
				}
				else
				{
					AssertHasRowWarning("Has row warning as specific circumstance is not applicable.", supportingDocument, "The Specific Circumstance does not support Supporting Document details at this level. These will not be sent in the ICS2 message.");
				}
			}
		}

		public void TestABL_PrepaidCollect()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(AsycudaBill.Schema.PrepaidCollectMaxLength, bill.ABL_PrepaidCollectInfo.MaxLength);
		}

		public void TestBillScreenings()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0, bill.BillScreenings.Count);
			bill.BillScreenings.AddNew();
			AssertEquals(1, bill.BillScreenings.Count);
			Factory.Save();
			var billReloaded = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals(1, billReloaded.BillScreenings.Count);
		}

		public void TestGrossWeightInKG()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.ABL_GrossWeight = 123.4567894m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("BillWeightInKG should be rounded to 6 decimals", 0.123457m, bill.GrossWeightInKG);
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			AssertType<AsycudaBillValidationForMasterChild>(masterBill.Validation);
		}

		public void TestShipmentTypeDefault()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals("Shipment Type for Bill is defaulted to Import", ShipmentTypeList.Codes.Import23, bill.ABL_ShipmentType);
		}

		public void TestPersonTypeMaxLength()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			CombineAssertions("Max length", () =>
			{
				AssertEquals(1, bill.ShipperPersonTypeInfo.MaxLength);
				AssertEquals(1, bill.ConsigneePersonTypeInfo.MaxLength);
				AssertEquals(1, bill.NotifyPartyPersonTypeInfo.MaxLength);
			});
		}

		public void TestPersonTypeDefaultingLogic()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			CombineAssertions("Preconditions: all of the person types should be empty by default", () =>
			{
				AssertNullOrEmpty(bill.ShipperPersonType);
				AssertNullOrEmpty(bill.ConsigneePersonType);
				AssertNullOrEmpty(bill.NotifyPartyPersonType);
			});

			bill.ABL_OA_Shipper = orgAddress.PK;
			bill.ABL_OA_Consignee = orgAddress.PK;
			bill.ABL_OA_NotifyParty = orgAddress.PK;

			CombineAssertions("Preconditions: all of the person types should be changed to 3 when organisation is specified", () =>
			{
				AssertEquals(EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2, bill.ShipperPersonType);
				AssertEquals(EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2, bill.ConsigneePersonType);
				AssertEquals(EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2, bill.NotifyPartyPersonType);
			});
		}

		public void TestDefaultTransportDocumentType_DefaultDataMapping()
		{
			var defaultTransportDocumentTypeTestDataMapping = new List<(string transport, string specificCircumstanceIndicator, string agentType, bool isMasterBill, string expectedTransportDocumentType)>()
			{
				(string.Empty, EUICS2SpecificCircumstanceList.Codes.F20, AgentType.Direct, true, string.Empty),
				(TransportModes.Air, string.Empty, AgentType.Direct, true, string.Empty),
				(TransportModes.Air, EUICS2SpecificCircumstanceList.Codes.F20, AgentType.Direct, true, TransportDocumentTypes.Codes.CL754_N740),
				(TransportModes.Air, EUICS2SpecificCircumstanceList.Codes.F20, string.Empty, true, TransportDocumentTypes.Codes.CL754_N741),
				(TransportModes.Air, EUICS2SpecificCircumstanceList.Codes.F20, string.Empty, false, TransportDocumentTypes.Codes.CL754_N703),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F10, string.Empty, true, TransportDocumentTypes.Codes.CL754_N705),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F13, string.Empty, true, TransportDocumentTypes.Codes.CL754_N705),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F17, string.Empty, true, TransportDocumentTypes.Codes.CL754_N705),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F11, string.Empty, true, TransportDocumentTypes.Codes.CL754_N704),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F12, string.Empty, true, TransportDocumentTypes.Codes.CL754_N704),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F14, string.Empty, true, TransportDocumentTypes.Codes.CL754_N704),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F15, string.Empty, true, TransportDocumentTypes.Codes.CL754_N704),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F16, string.Empty, true, TransportDocumentTypes.Codes.CL754_N704),
				(TransportModes.Sea, EUICS2SpecificCircumstanceList.Codes.F10, string.Empty, false, TransportDocumentTypes.Codes.CL754_N714),
				(TransportModes.Road, EUICS2SpecificCircumstanceList.Codes.F50, string.Empty, true, TransportDocumentTypes.Codes.CL754_N722),
				(TransportModes.Road, EUICS2SpecificCircumstanceList.Codes.F50, string.Empty, false, TransportDocumentTypes.Codes.CL754_N730),
				(TransportModes.InlandWaterwayTransport, EUICS2SpecificCircumstanceList.Codes.F10, string.Empty, true, TransportDocumentTypes.Codes.CL754_C625),
				(TransportModes.Mail, EUICS2SpecificCircumstanceList.Codes.F40, string.Empty, false, TransportDocumentTypes.Codes.CL754_N750),
			};

			foreach (var item in defaultTransportDocumentTypeTestDataMapping)
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				if (item.isMasterBill)
				{
					bill.ABL_BolType = "BOL";
				}

				if (item.agentType == AgentType.Direct)
				{
					bill.DefaultTransportDocumentType(item.transport, item.specificCircumstanceIndicator, item.agentType);
					AssertEquals(item.expectedTransportDocumentType, bill.TransportDocumentType);
				}
				else
				{
					var agentTypeListExcludeDirect = header.Lookups.AgentTypeList.GetAllCodes().Where(n => n != AgentType.Direct);
					foreach (var agentType in agentTypeListExcludeDirect)
					{
						bill.DefaultTransportDocumentType(item.transport, item.specificCircumstanceIndicator, agentType);
						AssertEquals(item.expectedTransportDocumentType, bill.TransportDocumentType);
					}
				}
			}
		}

		public void TestSupportedCusCodeDataTypes()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			var supportedCusCodeDataTypes = bill.GetCusCodeDataTypes();
			AssertContainsExactElementsInAnyOrder(new[] { CusCodeDataTypeList.Codes.EUICS2SupplementaryDeclarant, CusCodeDataTypeList.Codes.EUICS2Receptacle }, supportedCusCodeDataTypes.Keys);
		}

		public void TestBuyerCaptions()
		{
			CombineAssertions(() =>
			{
				var bill = (AsycudaBill)GetNewBusinessObject();

				AssertCaption(bill.ABL_BuyerNameInfo, "Buyer Name");
				AssertCaption(bill.ABL_BuyerStreet1Info, "Buyer Street 1");
				AssertCaption(bill.ABL_BuyerStreet2Info, "Buyer Street 2");
				AssertCaption(bill.ABL_BuyerCityInfo, "Buyer City");
				AssertCaption(bill.ABL_BuyerStateInfo, "Buyer State");
				AssertCaption(bill.ABL_BuyerPostcodeInfo, "Buyer Postcode");
				AssertCaption(bill.ABL_BuyerPhoneInfo, "Buyer Phone");
				AssertCaption(bill.ABL_RN_NKBuyerCountryInfo, "Buyer Country / Region");
				AssertCaption(bill.BuyerPersonTypeInfo, "Person Type");
				AssertCaption(bill.BuyerOrgPKInfo, "Buyer");
				AssertCaption(bill.ABL_OA_BuyerInfo, "Buyer");
			});
		}

		public void TestBuyerPersonTypeDefaulting()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();

			AssertNullOrEmpty(bill.BuyerPersonType);

			bill.ABL_OA_Buyer = Factory.New<OrgAddress>().PK;

			AssertEquals(EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2, bill.BuyerPersonType);
		}

		public void TestCanConvertSellerToOrganization()
		{
			CombineAssertions(() =>
			{
				var bill = (AsycudaBill)GetNewBusinessObject();

				AssertEquals("No address field set, we have no details with which to create an org.", false, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_SellerName = "Test";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_SellerStreet1 = "Test";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_SellerStreet2 = "Test";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_SellerCity = "Test";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_SellerState = "Test";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_SellerPostcode = "12345";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_RN_NKSellerCountry = "DE";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_SellerPhone = "09876543";
				AssertEquals(true, bill.CanConvertSellerToOrganization);

				bill.ABL_OA_Seller = Factory.New<OrgAddress>().PK;
				AssertEquals("Seller Address is already set", false, bill.CanConvertSellerToOrganization);
			});
		}

		public void TestSellerCaptions()
		{
			CombineAssertions(() =>
			{
				var bill = (AsycudaBill)GetNewBusinessObject();

				AssertCaption(bill.ABL_SellerNameInfo, "Seller Name");
				AssertCaption(bill.ABL_SellerStreet1Info, "Seller Street 1");
				AssertCaption(bill.ABL_SellerStreet2Info, "Seller Street 2");
				AssertCaption(bill.ABL_SellerCityInfo, "Seller City");
				AssertCaption(bill.ABL_SellerStateInfo, "Seller State");
				AssertCaption(bill.ABL_SellerPostcodeInfo, "Seller Postcode");
				AssertCaption(bill.ABL_SellerPhoneInfo, "Seller Phone");
				AssertCaption(bill.ABL_RN_NKSellerCountryInfo, "Seller Country / Region");
				AssertCaption(bill.SellerOrgPKInfo, "Seller");
				AssertCaption(bill.SellerPersonTypeInfo, "Person Type");
				AssertCaption(bill.ABL_OA_SellerInfo, "Seller");
			});
		}

		public void TestSellerUseRealOrg()
		{
			CombineAssertions(() =>
			{
				var bill = (AsycudaBill)GetNewBusinessObject();

				AssertEquals("No Seller is set", false, bill.SellerUseRealOrg);

				bill.ABL_OA_Seller = Factory.New<OrgAddress>().PK;

				AssertEquals("A Seller is set", true, bill.SellerUseRealOrg);
			});
		}

		public void TestSellerPersonTypeDefaulting()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();

			AssertNullOrEmpty(bill.SellerPersonType);

			bill.ABL_OA_Seller = Factory.New<OrgAddress>().PK;

			AssertEquals(EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2, bill.SellerPersonType);
		}

		public void TestDefaultBuyerSellerOrgAddressDetails()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.OH_FullName = "FullName";
			org.OA_Address1 = "Address1";
			org.OA_Address2 = "Address2";
			org.OA_City = "SIN";
			org.OA_State = "STATE";
			org.OA_PostCode = "0001";
			org.OA_Phone = "+00123456888";
			org.OA_RN_NKCountryCode = "SG";

			bill.ABL_OA_Seller = org.PK;
			AssertEquals("Seller", "FullName", bill.ABL_SellerName);
			AssertEquals("Seller", "Address1", bill.ABL_SellerStreet1);
			AssertEquals("Seller", "Address2", bill.ABL_SellerStreet2);
			AssertEquals("Seller", "SIN", bill.ABL_SellerCity);
			AssertEquals("Seller", "STATE", bill.ABL_SellerState);
			AssertEquals("Seller", "0001", bill.ABL_SellerPostcode);
			AssertEquals("Seller", "+00123456888", bill.ABL_SellerPhone);
			AssertEquals("Seller", "SG", bill.ABL_RN_NKSellerCountry);

			bill.ABL_OA_Seller = ZGuid.Empty;
			org.OA_CompanyNameOverride = "Override Company Name";
			bill.ABL_OA_Seller = org.PK;

			AssertEquals("Seller", "Override Company Name", bill.ABL_SellerName);
		}

		public void TestConsigneePersonType_PreSaveValidation_WhenShipperPersonTypeIsSet()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			bill.ShipperPersonType = "3";
			bill.RunPreSaveValidation();
			AssertNoErrors(bill.ShipperPersonTypeInfo);

			bill.ConsigneePersonType = "1";
			bill.RunPreSaveValidation();
			AssertNoErrors(bill.ConsigneePersonTypeInfo);
		}

		public void TestABL_Freight()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();

			var propertyInfo = bill.ABL_FreightValueInfo;

			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(AsycudaBill), nameof(AsycudaBill.ABL_FreightValue), false, x => x.DecimalPlaces == 2);
				AssertHasCustomAttribute<DecimalPrecisionAttribute>(typeof(AsycudaBill), nameof(AsycudaBill.ABL_FreightValue), false, x => x.DecimalPrecision == 16);
				AssertCaption(propertyInfo, "Postal Charges");
			});
		}

		public void TestABL_RX_NKFreightValueCurrency()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();

			AssertCaption(bill.ABL_RX_NKFreightValueCurrencyInfo, "Currency");
		}

		public void TestPersonTypeValidation_PropertyCannotFind()
		{
			ErrorReporter.Clear();
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.ABL_OA_Buyer = ZGuid.NewZGuid();
			bill.RunPreSaveValidation();
			bill.ABL_OA_Seller = ZGuid.NewZGuid();
			bill.RunPreSaveValidation();

			AssertEquals("ZCustomTypeDescriptor.properties should be refreshed to add new property '__SELLERPERSONTYPE__prop__ZString'", 0, ErrorReporter.LastExceptionsReported().Count);
		}

		public void TestReceptacleId()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();

			bill.ReceptacleId = "342516";

			AssertNotNull(bill.Receptacle);
			AssertEquals("CY_Data should populate from ReceptacleId", "342516", bill.Receptacle.CY_Data);
		}

		public void TestReceptacleIdProperties()
		{
			AssertResourceStringDataAttribute(
				typeof(AsycudaBill), nameof(AsycudaBill.ReceptacleId),
				shortCaption: "Rec. ID",
				caption: "Receptacle ID"
			);

			var info = Factory.New<AsycudaBill>().ReceptacleIdInfo;
			AssertEquals("MaxLength", 35, info.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestIsReceptacleEnabled()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			AssertEquals(false, bill.IsReceptacleEnabled);

			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifestHeader.AMA_TransportMode = TransportModes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			AssertEquals(true, bill.IsReceptacleEnabled);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
			AssertEquals(false, bill.IsReceptacleEnabled);

			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			AssertEquals(false, bill.IsReceptacleEnabled);
		}

		public void TestFactorySave_ShouldCleanReceptacle_WhenIsReceptacleEnabledIsFalse()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifestHeader.AMA_TransportMode = TransportModes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;

			bill.ReceptacleId = "342516";
			Assert("IsReceptacleEnabled is false", !bill.IsReceptacleEnabled);
			AssertNotNull("Precondition: Receptacle is not null", bill.Receptacle);
			Factory.Save();
			AssertNull("Receptacle is cleaned up", bill.Receptacle);
		}

		public void TestCanDelete()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var bill = manifestHeader.Bills.AddNew();
			Assert("Can Delete, as not submitted: RegistrationStatus is empty", bill.CanDelete);
			manifestHeader.RegistrationStatus = "SNT";
			Assert("Can not Delete, as submitted: RegistrationStatus is not empty", !bill.CanDelete);
			manifestHeader.RegistrationStatus = ZString.Empty;

			Assert("Can Delete, as not submitted: MessageStatus is empty", bill.CanDelete);
			manifestHeader.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.NotSent;
			Assert("Can Delete, as not submitted: MessageStatus == 'NOT'", bill.CanDelete);
			manifestHeader.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;
			Assert("Can not Delete, as submitted: MessageStatus == 'SNT'", !bill.CanDelete);
			manifestHeader.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Error;
			Assert("Can Delete, as not submitted: MessageStatus == 'ERR'", bill.CanDelete);
		}

		public void TestMessageStatusProvider()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var bill = manifestHeader.Bills.AddNew();
			AssertType<MessageStatusProvider>(bill.MessageStatusProvider);
		}

		public void TestSynchronisePaymentType()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			Assert(bill.SynchronisePaymentType);
		}

		public void TestBillNumber_ShouldBeReadOnly_WhenManifestIsRegisteredAndF14F15F16()
		{
			var testCases = new[]
			{
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isRegistered: true, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isRegistered: false, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isRegistered: true, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isRegistered: false, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isRegistered: true, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isRegistered: false, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F17, isRegistered: true, expectedReadonly: false),
			};

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			foreach (var (specificCircumstanceIndicator, isRegistered, expectedReadonly) in testCases)
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				manifestHeader.RegistrationNumber = isRegistered ? "123" : string.Empty;

				AssertEquals($"specificCircumstanceIndicator: {specificCircumstanceIndicator}, isRegistered: {isRegistered}", expectedReadonly, bill.ABL_BillNumberInfo.ReadOnly);
			}
		}

		public void TestBillNumber_ShouldNotBeReadOnly_WhenCustomsStatusIsCancelledAndF14F15F16()
		{
			var testCases = new[]
			{
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isCancelled: true, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, isCancelled: false, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isCancelled: true, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, isCancelled: false, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isCancelled: true, expectedReadonly: false),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, isCancelled: false, expectedReadonly: true),
				(specificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F17, isCancelled: true, expectedReadonly: false),
			};

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.RegistrationNumber = "123";
			var bill = manifestHeader.Bills.AddNew();

			foreach (var (specificCircumstanceIndicator, isCancelled, expectedReadonly) in testCases)
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				manifestHeader.RegistrationStatus = isCancelled ? EUICS2CustomsStatusList.Codes.CAN : EUICS2CustomsStatusList.Codes.REG;

				AssertEquals($"specificCircumstanceIndicator: {specificCircumstanceIndicator}, isCancelled: {isCancelled}", expectedReadonly, bill.ABL_BillNumberInfo.ReadOnly);
			}
		}

		void AssertResourceStringDataAttribute(
			Type typeToCheck, string propertyName,
			string shortCaption = null,
			string mediumCaption = null,
			string caption = null,
			string fullDescription = null,
			string multipleKey = null
		)
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeToCheck, propertyName, false,
				x => x.ShortCaption == shortCaption &&
					 x.MediumCaption == mediumCaption &&
					 x.Caption == caption &&
					 x.FullDescription == fullDescription &&
					 x.MultipleKey == multipleKey
			);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		void AssertCaption(ZPropertyInfo info, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} - Caption", caption, propertyData.Caption);
		}

		static OrgHeader GetOrUpdateOrgHeaderWithEori(BusinessObjectFactory factory, string orgCode, string eoriNumber, string eoriNumberCountryCode, string mainAddressCoutryCode = null, OrgHeader orgHeader = null)
		{
			if (orgHeader == null)
			{
				orgHeader = factory.New<OrgHeader>();
				orgHeader.OH_Code = orgCode;
				orgHeader.MainAddress.OA_RN_NKCountryCode = mainAddressCoutryCode;
			}
			var eori = factory.New<OrgCusCode>();
			eori.OK_OH = orgHeader.PK;
			eori.OK_RN_NKCodeCountry = eoriNumberCountryCode;
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_CustomsRegNo = eoriNumber;
			orgHeader.CustomsCodes.Add(eori);
			return orgHeader;
		}

		static OrgHeader GetOrgHeaderWithEuEoriAndGbEori(BusinessObjectFactory factory, string orgCode, string eoriNumber, string euEoriNumberCountryCode, string mainAddressCoutryCode)
		{
			var orgHeader = GetOrUpdateOrgHeaderWithEori(factory, orgCode, eoriNumber, euEoriNumberCountryCode, mainAddressCoutryCode);
			GetOrUpdateOrgHeaderWithEori(factory, orgCode, eoriNumber, Core.Constants.CountryCodes.UnitedKingdom, orgHeader: orgHeader);
			return orgHeader;
		}
	}
}
