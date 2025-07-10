using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Factory.New<JobDeclaration>(), "CNJobDeclaration");
		}

		public void TestGetTemplateCopyStrategy()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			var stratety = declaration.GetTemplateCopyStrategyExposed(declaration.Factory, CloneType.TemplateCopy);
			AssertType<JobDeclarationDeepCloneStrategy>(stratety);
		}

		public void TestBoolPropertiesForTransitMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("IsCustomsTransit", !declaration.IsCustomsTransit);
			Assert("IsTransshipment", !declaration.IsTransshipment);
			Assert("IsDeclaringInAdvance", !declaration.IsDeclaringInAdvance);
			Assert("IsDirectTransition", !declaration.IsDirectTransition);
			declaration.JE_TransitMode = TransitModeList.Codes.Transshipment;
			Assert("IsCustomsTransit", declaration.IsCustomsTransit);
			Assert("IsTransshipment", declaration.IsTransshipment);
			Assert("IsDeclaringInAdvance", !declaration.IsDeclaringInAdvance);
			Assert("IsDirectTransition", !declaration.IsDirectTransition);
			declaration.JE_TransitMode = TransitModeList.Codes.DeclaringInAdvance;
			Assert("IsCustomsTransit", declaration.IsCustomsTransit);
			Assert("IsTransshipment", !declaration.IsTransshipment);
			Assert("IsDeclaringInAdvance", declaration.IsDeclaringInAdvance);
			Assert("IsDirectTransition", !declaration.IsDirectTransition);
			declaration.JE_TransitMode = TransitModeList.Codes.DirectTransition;
			Assert("IsCustomsTransit", declaration.IsCustomsTransit);
			Assert("IsTransshipment", !declaration.IsTransshipment);
			Assert("IsDeclaringInAdvance", !declaration.IsDeclaringInAdvance);
			Assert("IsDirectTransition", declaration.IsDirectTransition);
		}

		public void TestBoolPropertiesForTransportModeInland()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("IsInlandWaterwayTransport", !declaration.IsInlandWaterwayTransport);
			Assert("IsInlandRoadTransport", !declaration.IsInlandRoadTransport);
			Assert("IsInlandRailTransport", !declaration.IsInlandRailTransport);
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			Assert("IsInlandWaterwayTransport", declaration.IsInlandWaterwayTransport);
			Assert("IsInlandRoadTransport", !declaration.IsInlandRoadTransport);
			Assert("IsInlandRailTransport", !declaration.IsInlandRailTransport);
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
			Assert("IsInlandWaterwayTransport", !declaration.IsInlandWaterwayTransport);
			Assert("IsInlandRoadTransport", declaration.IsInlandRoadTransport);
			Assert("IsInlandRailTransport", !declaration.IsInlandRailTransport);
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Rail;
			Assert("IsInlandWaterwayTransport", !declaration.IsInlandWaterwayTransport);
			Assert("IsInlandRoadTransport", !declaration.IsInlandRoadTransport);
			Assert("IsInlandRailTransport", declaration.IsInlandRailTransport);
		}

		public void TestIsCarNumberApplicable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
			Assert("True when ROA", declaration.IsInlandCarNumberApplicable);
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			Assert("False when IWT", !declaration.IsInlandCarNumberApplicable);
			declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Rail;
			Assert("True when RAI", declaration.IsInlandCarNumberApplicable);
		}

		public void TestDefaultApplicationCodeForFakeDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(declaration.JE_MessageSubType != "BTH");
			var invoice = Factory.New<JobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			Assert(((JobDeclaration)fakeDeclaration.HeaderData).JE_MessageSubType == "BTH");
		}

		public void TestManufacturerDocumentaryAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var manufacturerAddress = declaration.ManufacturerDocumentaryAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.ManufacturerDocumentaryAddress)", manufacturerAddress);
			AssertEquals(DocAddressType.Manufacturer, manufacturerAddress.DocAddressType);
			AssertEquals(ContactType.Consignor, manufacturerAddress.DefaultContactType);
		}

		public void TestUpdateJE_CNTransportMode()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_RL_NKClosestPort = "CNBJS";
			var cusCode1 = orgHeader1.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = "CN";
			cusCode1.OK_CodeType = "CCD";
			cusCode1.OK_CustomsRegNo = "11114";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_RL_NKClosestPort = "CNSHA";
			var cusCode2 = orgHeader2.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "CN";
			cusCode2.OK_CodeType = "CCD";
			cusCode2.OK_CustomsRegNo = "11115";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfLoading = "CNBJS";
			declaration.JE_RL_NKPortOfArrival = "CNBJS";
			declaration.JE_CNTransportMode = CNTransportModeList.Codes.Others;
			declaration.JE_OfficeOfEntryExit = "5345";
			declaration.JE_RL_NKPortOfLoading = "USAAT";
			AssertEquals(ZString.Empty, declaration.JE_CNTransportMode);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_RL_NKPortOfLoading = "CNBJS";
			AssertEquals(CNTransportModeList.Codes.Others, declaration.JE_CNTransportMode);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.UpdateJE_CNTransportMode();
			AssertEquals(CNTransportModeList.Codes.CrossBorder, declaration.JE_CNTransportMode);
			declaration.JE_OfficeOfEntryExit = "";
			declaration.JE_OH_Importer = orgHeader1.PK;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			AssertEquals(CNTransportModeList.Codes.BondedArea, declaration.JE_CNTransportMode);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = orgHeader2.PK;
			AssertEquals(CNTransportModeList.Codes.ExportProcessing, declaration.JE_CNTransportMode);
			orgHeader2.OH_RL_NKClosestPort = "USAAT";
			cusCode2.OK_CustomsRegNo = "1114";
			declaration.JE_OH_Importer = orgHeader1.PK;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = orgHeader2.PK;
			AssertEquals(CNTransportModeList.Codes.Others, declaration.JE_CNTransportMode);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals(CNTransportModeList.Codes.Others, declaration.JE_CNTransportMode);
		}

		public void TestJE_OH_BuyerChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "JOHN SMITH";
			contact1.OC_IsActive = true;
			var allocation = contact1.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.CNCUS;
			var phone1 = contact1.PhoneContactItems.AddNew();
			phone1.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;
			phone1.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			phone1.OI_Address_Formatted = "1234567";
			var org2 = Factory.New<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "JOHN SMITH";
			contact2.OC_IsActive = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Buyer = org1.PK;
			AssertEquals(org1.PK, declaration.BuyerDocAddress.OrganisationPK);
			AssertEquals(contact1.PK, declaration.BuyerDocAddress.ContactPK);
			declaration.IsImportingData = true;
			declaration.JE_OH_Buyer = org2.PK;
			AssertEquals(org1.PK, declaration.BuyerDocAddress.OrganisationPK);
			AssertEquals(contact1.PK, declaration.BuyerDocAddress.ContactPK);
			declaration.IsImportingData = false;
			declaration.JE_OH_Buyer = org1.PK;
			declaration.JE_OH_Buyer = org2.PK;
			AssertEquals(org2.PK, declaration.BuyerDocAddress.OrganisationPK);
			AssertNotEquals(contact2.PK, declaration.BuyerDocAddress.ContactPK);
		}

		public void TestDefaultMessageTypeFromSupplierOrImporter()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_RL_NKClosestPort = "CNBJS";
			var cusCode1 = orgHeader1.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = "CN";
			cusCode1.OK_CodeType = "CCD";
			cusCode1.OK_CustomsRegNo = "11114";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_RL_NKClosestPort = "CNSHA";
			var cusCode2 = orgHeader2.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "CN";
			cusCode2.OK_CodeType = "CCD";
			cusCode2.OK_CustomsRegNo = "11110";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = orgHeader1.PK;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			AssertEquals(SharedJobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			declaration.JE_OH_Supplier = orgHeader2.PK;
			AssertEquals(SharedJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			declaration.JE_OH_Importer = orgHeader2.PK;
			AssertEquals(SharedJobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			declaration.JE_OH_Importer = orgHeader1.PK;
			AssertEquals(SharedJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			declaration.JE_OH_Importer = orgHeader1.PK;
			AssertEquals("AnyEntryHasBeenLodgedOrIsWaitingForResponse", SharedJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "CE001";
			declaration.JE_OH_Supplier = orgHeader1.PK;
			AssertEquals("AnyEntryHasBeenLodgedOrIsWaitingForResponse", SharedJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			entry.EntryNumber = ZString.Empty;
			declaration.JE_OH_Importer = orgHeader2.PK;
			AssertEquals("AnyEntryHasBeenLodgedOrIsWaitingForResponse", SharedJobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			entry.CH_Status = "AWO";
			declaration.JE_OH_Importer = orgHeader1.PK;
			AssertEquals("AnyEntryHasBeenLodgedOrIsWaitingForResponse", SharedJobMessageTypeList.Codes.Import, declaration.JE_MessageType);
		}

		public void TestAvoidFlushing()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP";
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "BUY";
			AssertNoExceptionThrown(() =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OH_Importer = importer.PK;
				declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
				Factory.Save();
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OH_Supplier = supplier.PK;
				declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
				Factory.Save();
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OH_Buyer = supplier.PK;
				declaration.BuyerDocAddress.E2_AddressOverride = true;
				Factory.Save();
			}

			);
		}

		public void TestDefaultOrgAddInfoDatas()
		{
			var importer = Factory.New<OrgHeader>();
			var importerAddInfo = CNOrgImpAddInfo.Get(importer);
			importerAddInfo.ZO_MessageSubType = "CUS";
			importerAddInfo.ZO_IsConsolidatedDutyCollection = true;
			importerAddInfo.ZO_IsAssuredInspectClearance = false;
			var supplier = Factory.New<OrgHeader>();
			var supplierAddInfo = CNOrgImpAddInfo.Get(supplier);
			supplierAddInfo.ZO_MessageSubType = "REC";
			supplierAddInfo.ZO_IsConsolidatedDutyCollection = false;
			supplierAddInfo.ZO_IsAssuredInspectClearance = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_OH_Importer = importer.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("MessageSubType should have been set on Importer change.", "CUS", declaration.JE_MessageSubType);
			var operationMatters = instruction.OperationMatters.Cast<OperationMatter>();
			Assert("ZO_IsConsolidatedDutyCollection", operationMatters.Count() == 1 && operationMatters.FirstOrDefault().CY_Code == OperationMatterList.Codes.ConsolidatedDutyCollection);
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_OH_Supplier = supplier.PK;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("MessageSubType should have been set on Supplier change.", "REC", declaration.JE_MessageSubType);
			var operationMatters2 = instruction2.OperationMatters.Cast<OperationMatter>();
			Assert("ZO_IsAssuredInspectClearance", operationMatters2.Count() == 1 && operationMatters2.FirstOrDefault().CY_Code == OperationMatterList.Codes.AssuredInspectClearance);
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "CE001";
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("AnyEntryHasBeenLodgedOrIsWaitingForResponse", "REC", declaration.JE_MessageSubType);
			entry.EntryNumber = ZString.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("AnyEntryHasBeenLodgedOrIsWaitingForResponse", "CUS", declaration.JE_MessageSubType);
			entry.CH_Status = "AWO";
			declaration.JE_MessageSubType = "REC";
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("AnyEntryHasBeenLodgedOrIsWaitingForResponse", "REC", declaration.JE_MessageSubType);
		}

		public override void TestMergeByDefaultsFromClientWhenClientChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableMessageTypeChangeOnSupplierChangeForTesting = true;
			declaration.JE_MessageType = DefaultExportMessageType;
			var importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = declaration.CountryCode + "XXX";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = MergeType1;
			var cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCD";
			cusCode.OK_RN_NKCodeCountry = "CN";
			cusCode.OK_CustomsRegNo = "12345";
			var supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = declaration.CountryCode + "XXX";
			supplier.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "DEF";
			AssertEquals("Declaration.JE_MergeBy", DefaultMergeType, declaration.JE_MergeBy);
			Env.Registry.SetCommercialInvoiceLineMergeMethod(GlbCompany.CurrentCompany.PK.ToGuid(), MergeType2);
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			//export so from supplier
			AssertEquals("Declaration.JE_MergeBy", MergeType2, declaration.JE_MergeBy);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_MessageType = DefaultImportMessageType;
			cusCode.OK_RN_NKCodeCountry = "US";
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Declaration.JE_MergeBy", MergeType1, declaration.JE_MergeBy);
		}

		public void TestJE_LastPortBeforeEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "DE222";
			AssertEquals("DE222", declaration.JE_LastPortBeforeEntry);
			var transport = declaration.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "CNBJS";
			transport.JW_RL_NKLoadPort = "GBLON";
			AssertEquals("GBLON", declaration.JE_LastPortBeforeEntry);
			transport.JW_RL_NKDiscPort = "GBLON";
			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertEquals("GBLON", declaration.JE_LastPortBeforeEntry);
			transport.JW_RL_NKDiscPort = "CNBJS";
			declaration.JE_RL_NKPortOfLoading = "DE222";
			AssertEquals("DE222", declaration.JE_LastPortBeforeEntry);
		}

		[TestDate(2018, 2, 25)]
		public void TestDateOfValuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			AssertEquals("DateOfValuation should be today", new ZDateTime(2018, 2, 25), declaration.DateOfValuation);
			declaration.JE_DateOfArrival = new ZDateTime(2018, 2, 24);
			AssertEquals("DateOfValuation should be today", new ZDateTime(2018, 2, 25), declaration.DateOfValuation);
			declaration.JE_DateOfArrival = new ZDateTime(2018, 2, 26);
			AssertEquals("DateOfValuation should be today", new ZDateTime(2018, 2, 25), declaration.DateOfValuation);
			declaration.JE_MessageType = "IMP";
			AssertEquals("DateOfValuation should be Date Of Arrival", new ZDateTime(2018, 2, 26), declaration.DateOfValuation);
			declaration.CustomsEntryInstructions.AddNew().CEI_DateForDuty = new ZDateTime(2018, 3, 1);
			declaration.CustomsEntryInstructions.AddNew().CEI_DateForDuty = new ZDateTime(2018, 3, 2);
			AssertEquals("DateOfValuation should be the earliest CEI_DateForDuty", new ZDateTime(2018, 3, 1), declaration.DateOfValuation);
		}

		public void TestDefaultJE_RN_NKCountryOfTrade()
		{
			var declaration = Factory.New<JobDeclaration>();
			var orgCN = Factory.New<OrgHeader>();
			orgCN.MainAddress.OA_RN_NKCountryCode = "CN";
			var orgAU = Factory.New<OrgHeader>();
			orgAU.MainAddress.OA_RN_NKCountryCode = "AU";
			var orgUS = Factory.New<OrgHeader>();
			orgUS.MainAddress.OA_RN_NKCountryCode = "US";
			var orgCA = Factory.New<OrgHeader>();
			orgCA.MainAddress.OA_RN_NKCountryCode = "CA";
			declaration.JE_OH_Supplier = orgCN.PK;
			declaration.JE_OH_Importer = orgAU.PK;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Country of trade should be AU", "AU", declaration.JE_RN_NKCountryOfTrade);
			declaration.JE_OH_Importer = Guid.Empty;
			AssertEquals("Country of trade should be empty", "", declaration.JE_RN_NKCountryOfTrade);
			declaration.JE_OH_Importer = orgUS.PK;
			AssertEquals("Country of trade should be US", "US", declaration.JE_RN_NKCountryOfTrade);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Country of trade should be CN", "CN", declaration.JE_RN_NKCountryOfTrade);
			declaration.JE_OH_Supplier = Guid.Empty;
			AssertEquals("Country of trade should be empty", "", declaration.JE_RN_NKCountryOfTrade);
			declaration.JE_OH_Supplier = orgCA.PK;
			AssertEquals("Country of trade should be CA", "CA", declaration.JE_RN_NKCountryOfTrade);
		}

		public void TestDocAddressWrappers()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Buyer = buyer.PK;
			declaration.JE_OH_Manufacturer = manufacturer.PK;
			AssertEquals("ImporterDocumentaryAddress", importer.PK, declaration.ImporterDocumentaryAddress.OrganisationPK);
			AssertEquals("SupplierDocumentaryAddress", supplier.PK, declaration.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("BuyerDocAddress", buyer.PK, declaration.BuyerDocAddress.OrganisationPK);
			AssertEquals("ManufacturerDocumentaryAddress", manufacturer.PK, declaration.ManufacturerDocumentaryAddress.OrganisationPK);
		}

		public void TestGetDefaultOriginDistrictCode()
		{
			var org1 = Factory.New<OrgHeader>();
			var org1_CCD = org1.CustomsCodes.AddNew();
			org1_CCD.OK_CodeType = "CCD";
			org1_CCD.OK_RN_NKCodeCountry = "CN";
			org1_CCD.OK_CustomsRegNo = "1111111111";
			var org2 = Factory.New<OrgHeader>();
			var org2_CCD = org2.CustomsCodes.AddNew();
			org2_CCD.OK_CodeType = "CCD";
			org2_CCD.OK_RN_NKCodeCountry = "CN";
			org2_CCD.OK_CustomsRegNo = "2222222222";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org1.PK;
			AssertEquals("11111", declaration.GetDefaultOriginDistrictCode());
			declaration.JE_OH_Manufacturer = org2.PK;
			AssertEquals("22222", declaration.GetDefaultOriginDistrictCode());
		}

		public void TestGetDefaultDestinationDistrictCode()
		{
			var org1 = Factory.New<OrgHeader>();
			var org1_CCD = org1.CustomsCodes.AddNew();
			org1_CCD.OK_CodeType = "CCD";
			org1_CCD.OK_RN_NKCodeCountry = "CN";
			org1_CCD.OK_CustomsRegNo = "1111111111";
			var org2 = Factory.New<OrgHeader>();
			var org2_CCD = org2.CustomsCodes.AddNew();
			org2_CCD.OK_CodeType = "CCD";
			org2_CCD.OK_RN_NKCodeCountry = "CN";
			org2_CCD.OK_CustomsRegNo = "2222222222";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("11111", declaration.GetDefaultDestinationDistrictCode());
			declaration.JE_OH_Buyer = org2.PK;
			AssertEquals("22222", declaration.GetDefaultDestinationDistrictCode());
		}

		public void TestDefaultDistrict()
		{
			var org1 = Factory.New<OrgHeader>();
			var org1_CCD = org1.CustomsCodes.AddNew();
			org1_CCD.OK_CodeType = "CCD";
			org1_CCD.OK_RN_NKCodeCountry = "CN";
			org1_CCD.OK_CustomsRegNo = "1111111111";
			AssertEquals(org1_CCD.OK_CustomsRegNo, org1.LocalCustomsClientCode);
			var org2 = Factory.New<OrgHeader>();
			var org2_CCD = org2.CustomsCodes.AddNew();
			org2_CCD.OK_CodeType = "CCD";
			org2_CCD.OK_RN_NKCodeCountry = "CN";
			org2_CCD.OK_CustomsRegNo = "2222222222";
			AssertEquals(org2_CCD.OK_CustomsRegNo, org2.LocalCustomsClientCode);
			var org3 = Factory.New<OrgHeader>();
			var org3_CCD = org3.CustomsCodes.AddNew();
			org3_CCD.OK_CodeType = "CCD";
			org3_CCD.OK_RN_NKCodeCountry = "CN";
			org3_CCD.OK_CustomsRegNo = "3333333333";
			AssertEquals(org3_CCD.OK_CustomsRegNo, org3.LocalCustomsClientCode);
			var org4 = Factory.New<OrgHeader>();
			var org4_CCD = org4.CustomsCodes.AddNew();
			org4_CCD.OK_CodeType = "CCD";
			org4_CCD.OK_RN_NKCodeCountry = "CN";
			org4_CCD.OK_CustomsRegNo = "4444444444";
			AssertEquals(org4_CCD.OK_CustomsRegNo, org4.LocalCustomsClientCode);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Buyer = org2.PK;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_OriginRegion = "111111";
			AssertEquals("11111", invoiceLine.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine.JI_DestinationDistrict);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			invoiceLine.JI_DestinationRegion = "222222";
			AssertEquals("11111", invoiceLine.JI_OriginDistrict);
			AssertEquals("22222", invoiceLine.JI_DestinationDistrict);
			AssertEquals("111111", invoiceLine.JI_OriginRegion);
			AssertEquals("222222", invoiceLine.JI_DestinationRegion);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals("11111", invoiceLine.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine.JI_DestinationDistrict);
			AssertEquals("111111", invoiceLine.JI_OriginRegion);
			AssertEquals(ZString.Empty, invoiceLine.JI_DestinationRegion);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("11111", invoiceLine.JI_OriginDistrict);
			AssertEquals("22222", invoiceLine.JI_DestinationDistrict);
			AssertEquals("111111", invoiceLine.JI_OriginRegion);
			AssertEquals(ZString.Empty, invoiceLine.JI_DestinationRegion);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals(ZString.Empty, invoiceLine.JI_OriginDistrict);
			AssertEquals("22222", invoiceLine.JI_DestinationDistrict);
			AssertEquals(ZString.Empty, invoiceLine.JI_OriginRegion);
			AssertEquals(ZString.Empty, invoiceLine.JI_DestinationRegion);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertEquals("11111", invoiceLine.JI_OriginDistrict);
			AssertEquals("22222", invoiceLine.JI_DestinationDistrict);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals("11111", invoiceLine.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine.JI_DestinationDistrict);
			invoiceLine.JI_OriginDistrict = ZString.Empty;
			declaration.JE_OH_Supplier = org3.PK;
			declaration.JE_OH_Buyer = org4.PK;
			AssertEquals("33333", invoiceLine.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine.JI_DestinationDistrict);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals(ZString.Empty, invoiceLine.JI_OriginDistrict);
			AssertEquals("44444", invoiceLine.JI_DestinationDistrict);
		}

		public void TestOnCNTransportModeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ContainerMode = "1";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("1", declaration.JE_ContainerMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
		}

		public void TestWillNotDefalutIfNotOverrideFreightDefaults()
		{
			CreateTestLocoMapping();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OverrideFreightDefaults = false;
			declaration.JE_JS = (Factory.New<ForwardingShipment>()).PK;
			AssertNotNull(declaration.Shipment);
			declaration.JE_CNPortOfOrigin = "0001";
			AssertEquals("JE_RL_NKPortOfLoading should be empty", "", declaration.JE_RL_NKPortOfLoading);
			declaration.JE_CNPortOfDestination = "0001";
			AssertEquals("JE_RL_NKPortOfArrival should be empty", "", declaration.JE_RL_NKPortOfArrival);
			declaration.JE_CNPortOfOrigin = "";
			declaration.JE_CNPortOfDestination = "";
			declaration.JE_RL_NKPortOfLoading = "CNS";
			AssertEquals("JE_CNPortOfOrigin should be empty", "", declaration.JE_CNPortOfOrigin);
			declaration.JE_RL_NKPortOfArrival = "CNS";
			AssertEquals("JE_CNPortOfDestination should be empty", "", declaration.JE_CNPortOfDestination);
		}

		public override void TestSetDefaultPackagesType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("default value for JE_TotalNoOfPacksPackType", "", declaration.JE_TotalNoOfPacksPackType);
		}

		public void TestOnJE_CNPortOfOriginChanged()
		{
			CreateTestLocoMapping();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CNPortOfOrigin = "0001";
			AssertEquals("JE_RL_NKPortOfLoading should be CNS", "CNS", declaration.JE_RL_NKPortOfLoading);
		}

		public void TestOnJE_CNPortOfDestinationChanged()
		{
			CreateTestLocoMapping();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CNPortOfDestination = "0001";
			AssertEquals("JE_RL_NKPortOfArrival should be CNS", "CNS", declaration.JE_RL_NKPortOfArrival);
		}

		public void TestDefaultValuesOfPorts()
		{
			CreateTestLocoMapping();
			var declaration = Factory.New<JobDeclaration>();
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "PORT", "USA264", "洛杉矶（美国）");
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "CNS";
			AssertEquals("JE_CNPortOfOrigin should be 0001", "0001", declaration.JE_CNPortOfOrigin);
			AssertEquals("JE_CNPortOfDestination should remain empty", ZString.Empty, declaration.JE_CNPortOfDestination);
			AssertEquals("JE_CNLastPortBeforeEntry should be CHN000", "CHN000", declaration.JE_CNLastPortBeforeEntry);
			declaration.JE_RL_NKPortOfLoading = "US111";
			AssertEquals("JE_CNPortOfOrigin should be 0001", "0001", declaration.JE_CNPortOfOrigin);
			AssertEquals("JE_CNLastPortBeforeEntry should be 0002", "0002", declaration.JE_CNLastPortBeforeEntry);
			declaration.JE_CNPortOfOrigin = declaration.JE_CNPortOfOrigin = ZString.Empty;
			declaration.JE_CNLastPortBeforeEntry = declaration.JE_CNPortOfOrigin = ZString.Empty;
			declaration.JE_CNPortOfDestination = declaration.JE_CNPortOfOrigin = ZString.Empty;
			declaration.JE_RL_NKFinalDestination = "CNS";
			AssertEquals("JE_CNPortOfOrigin should remain empty", ZString.Empty, declaration.JE_CNPortOfOrigin);
			AssertEquals("JE_CNPortOfDestination should be 0001", "0001", declaration.JE_CNPortOfDestination);
			declaration.JE_RL_NKOrigin = "CNS1";
			declaration.JE_RL_NKFinalDestination = "CNS1";
			AssertEquals("Default from country port: JE_CNPortOfOrigin should be CHN000", "CHN000", declaration.JE_CNPortOfOrigin);
			AssertEquals("Default from country port: JE_CNPortOfDestination should be CHN000", "CHN000", declaration.JE_CNPortOfDestination);
			declaration.JE_RL_NKOrigin = "XXXX";
			declaration.JE_RL_NKFinalDestination = "XXXX";
			AssertEquals("Default from country port: JE_CNPortOfOrigin should be ZZZ", "ZZZ000", declaration.JE_CNPortOfOrigin);
			AssertEquals("Default from country port: JE_CNPortOfDestination should be ZZZ", "ZZZ000", declaration.JE_CNPortOfDestination);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(true, dec.AreMultipleEntryInstructionsAllowed);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = bizO.Lookups;
			var secondLookup = bizO.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		public void TestTypeDecider()
		{
			Assert("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>().GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			var dec = GetJobDeclaration();
			AssertEquals(Core.Constants.CurrencyCodes.China, dec.LocalCurrencyCode);
		}

		public void TestReciprocalRates()
		{
			Assert(Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("CN Declaration should support EntryInstructions", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}

		public void TestNoMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Assert("NON, shoud be NoMerge", declaration.NoMerge);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Assert("TRF, shoud be not NOMerge", !declaration.NoMerge);
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_MessageSubType", DecTypeList.Codes.CustomsEntry, declaration.JE_MessageSubType);
		}

		public void TestXC_OfficeOfEntryExitDefaultValue()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_CustomsOffice = "OF1";
			AssertEquals("OF1", declaration1.JE_OfficeOfEntryExit);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_OfficeOfEntryExit = "OF2";
			declaration2.JE_CustomsOffice = "OF1";
			AssertEquals("OF2", declaration2.JE_OfficeOfEntryExit);
		}

		public override void TestMasterBillLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			var databoundBO = new DataBoundBusinessObject(declaration);
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Air Master Bill Label", "Master Bill", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Sea Master Bill Label", "Ocean Bill", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Raod Master Bill Label", "Trans. Batch No.", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Post Master Bill Label", "Parcel Number", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Rail Master Bill Label", "Rail Waybill number", resourceStringData.Caption);
		}

		public void TestWillGenerateCustomsEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("IMP+CUS", declaration.WillGenerateCustomsEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("IMP+REC", !declaration.WillGenerateCustomsEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("IMP+BTH", declaration.WillGenerateCustomsEntry);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("EXP+CUS", declaration.WillGenerateCustomsEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("EXP+REC", !declaration.WillGenerateCustomsEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("EXP+BTH", declaration.WillGenerateCustomsEntry);
		}

		public void TestWillGenerateRecordListing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("IMP+CUS", !declaration.WillGenerateRecordListing);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("IMP+REC", declaration.WillGenerateRecordListing);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("IMP+BTH", declaration.WillGenerateRecordListing);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("EXP+CUS", !declaration.WillGenerateRecordListing);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("EXP+REC", declaration.WillGenerateRecordListing);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("EXP+BTH", declaration.WillGenerateRecordListing);
		}

		public void TestWillGenerateBothEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("IMP+CUS", !declaration.WillGenerateBothEntries);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("IMP+REC", !declaration.WillGenerateBothEntries);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("IMP+BTH", declaration.WillGenerateBothEntries);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("EXP+CUS", !declaration.WillGenerateBothEntries);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("EXP+REC", !declaration.WillGenerateBothEntries);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("EXP+BTH", declaration.WillGenerateBothEntries);
		}

		public void TestSyncDocAddressContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OR1";
			var address = org.Addresses.AddNew();
			address.OA_OH = org.PK;
			var contact1 = org.Contacts.AddNew();
			contact1.OC_OH = org.PK;
			contact1.OC_ContactName = "Contact 1";
			var allocation1 = contact1.Allocations.AddNew();
			allocation1.PC_Type = "CNB";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_OH = org.PK;
			contact2.OC_ContactName = "Contact 2";
			var allocation2 = contact2.Allocations.AddNew();
			allocation2.PC_Type = "CNC";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = org.PK;
			AssertEquals("Contact 2", declaration.ImporterDocumentaryAddress.E2_Contact);
			AssertEquals(ZString.Empty, declaration.SupplierDocumentaryAddress.E2_Contact);
			declaration.ImporterDocumentaryAddress.E2_Contact = ZString.Empty;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = org.PK;
			AssertEquals("Contact 2", declaration.SupplierDocumentaryAddress.E2_Contact);
			AssertEquals(ZString.Empty, declaration.ImporterDocumentaryAddress.E2_Contact);
		}

		public void TestCusEntryInstructionBLNumberDisabled()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = "ROA";
			Assert(!declaration.TransportDataHelper.ShouldBillOfLadingKeepEmpty);
			declaration.JE_RL_NKPortOfLoading = "CN";
			declaration.JE_RL_NKPortOfArrival = "CN";
			Assert(declaration.TransportDataHelper.ShouldBillOfLadingKeepEmpty);
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";
			declaration.JE_TransportMode = "FIX";
			Assert(declaration.TransportDataHelper.ShouldBillOfLadingKeepEmpty);
			declaration.JE_TransportMode = "PHC";
			Assert(declaration.TransportDataHelper.ShouldBillOfLadingKeepEmpty);
		}

		public void TestUpdateBillOfLoading()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instrunction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "B1000001001";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("B1000001001", instrunction.BillOfLading);
			declaration.JE_CNTransportMode = CNTransportModeList.Codes.Air;
			declaration.JE_HouseBill = "H2000002002";
			AssertEquals("B1000001001_H2000002002", instrunction.BillOfLading);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals("B1000001001_00002002", instrunction.BillOfLading);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, instrunction.BillOfLading);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("B1000001001", instrunction.BillOfLading);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(ZString.Empty, instrunction.BillOfLading);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("B1000001001", instrunction.BillOfLading);
			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			AssertEquals(ZString.Empty, instrunction.BillOfLading);
			declaration.JE_TransportMode = Core.Constants.TransportModes.PassengerHandCarried;
			AssertEquals(ZString.Empty, instrunction.BillOfLading);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("B1000001001", instrunction.BillOfLading);
			declaration.JE_RL_NKPortOfLoading = "CN";
			declaration.JE_RL_NKPortOfArrival = "CN";
		}

		public void TestWillGenerateEnteringEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Assert(declaration.WillGenerateEnteringEntry);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert(declaration.WillGenerateEnteringEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert(!declaration.WillGenerateEnteringEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert(!declaration.WillGenerateEnteringEntry);
		}

		public void TestWillGenerateExitingEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			Assert(declaration.WillGenerateExitingEntry);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert(declaration.WillGenerateExitingEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert(!declaration.WillGenerateExitingEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert(!declaration.WillGenerateExitingEntry);
		}

		public void TestContainersRequiredAndShouldDeleteContainers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_ContainerMode = "";
			Assert(declaration.ContainersRequired);
			Assert(!declaration.ShouldDeleteContainers);
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_TransportMode = "AIR";
			Assert(declaration.ContainersRequired);
			Assert(!declaration.ShouldDeleteContainers);
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_TransportMode = "MAI";
			Assert(declaration.ContainersRequired);
			Assert(!declaration.ShouldDeleteContainers);
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
			declaration.JE_ContainerMode = "CNT";
			Assert(declaration.ContainersRequired);
			Assert(!declaration.ShouldDeleteContainers);
			declaration.JE_TransportMode = "XXX";
			declaration.JE_ContainerMode = "";
			Assert(!declaration.ContainersRequired);
			Assert(declaration.ShouldDeleteContainers);
		}

		public void TestSetDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Setting HasChanges should be suspended", !declaration.HasChanges);
			Assert("Validation should be suspended", !declaration.NotificationsIncludingChildren.HasNotifications());

			AssertEquals("JE_MessageSubType", DecTypeList.Codes.CustomsEntry, declaration.JE_MessageSubType);
			AssertEquals("JE_ClearanceMode", ClearanceModeList.Codes.Integrated, declaration.JE_ClearanceMode);
		}

		public void TestOfficeOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, declaration.OfficeOfDestination);
			AssertEquals("CY_Code", CustomsOfficeTypeList.Codes.DES, declaration.CustomsOffices[0].CY_Code);
			Assert("Add empty CustomsOffice should not set HasChanges", !declaration.HasChanges);
			declaration.OfficeOfDestination = "1111";
			AssertEquals(1, declaration.CustomsOffices.Count);
			AssertEquals(4, declaration.OfficeOfDestinationInfo.MaxLength);
			AssertEquals("CY_Code", CustomsOfficeTypeList.Codes.DES, declaration.CustomsOffices[0].CY_Code);
			AssertEquals("CY_Data", "1111", declaration.CustomsOffices[0].CY_Data);
			AssertHasMessageErrorContaining(declaration.OfficeOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			declaration.CustomsOffices[0].CY_Data = "2222";
			AssertEquals("2222", declaration.OfficeOfDestination);
		}

		public void TestAgentOrg()
		{
			var proxy1 = Factory.New<OrgHeader>();
			var proxy2 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			declaration.Branch.Company.GC_OH_OrgProxy = proxy1.PK;
			AssertSame("Should fall back to Branch.Company.OrgProxy", proxy1, declaration.Declarant);
			declaration = Factory.New<JobDeclaration>();
			declaration.Branch.Company.GC_OH_OrgProxy = ZGuid.Empty;
			declaration.Branch.GB_OH_OrgProxy = proxy2.PK;
			AssertSame("Should return Declaration.Branch.OrgProxy", proxy2, declaration.Declarant);
		}

		public void TestJE_CustomsOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_CustomsOffice = "1234";
			AssertEquals("1234", declaration.JE_OfficeOfEntryExit);
			declaration.JE_CustomsOffice = "4321";
			AssertEquals("1234", declaration.JE_OfficeOfEntryExit);
		}

		public override void TestContainersRequiredOnAir()
		{
			var dec = GetJobDeclaration();
			dec.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			Assert("Precondition - Should not show", !dec.ContainersRequired);
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			Assert("Should show", dec.ContainersRequired);
		}

		public void TestCIQRequires()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction3.PK;
			AssertEquals(false, instruction1.CEI_CIQRequires);
			AssertEquals(false, instruction2.CEI_CIQRequires);
			AssertEquals(false, instruction3.CEI_CIQRequires);
			AssertEquals(false, declaration.CIQRequires);
			invoiceLine1.JI_CIQTariff = "1111";
			instruction1.CEI_CIQRequires = true;
			AssertEquals(true, instruction1.CEI_CIQRequires);
			AssertEquals(false, instruction2.CEI_CIQRequires);
			AssertEquals(false, instruction3.CEI_CIQRequires);
			AssertEquals(true, declaration.CIQRequires);
		}

		public void TestMergingRules()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().FirstOrDefault(option => option.Code == "CIR").Selected = true;
			declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().FirstOrDefault(option => option.Code == "TUP").Selected = true;
			declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().FirstOrDefault(option => option.Code == "SPM").Selected = true;
			var selections = declaration.MergingRules.Cast<MergingRule>();
			AssertEquals(3, selections.Count());
			Assert(selections.Any(selection => selection.CY_Code == "CIR"));
			Assert(selections.Any(selection => selection.CY_Code == "TUP"));
			Assert(selections.Any(selection => selection.CY_Code == "SPM"));
		}

		public void TestRefreshSelectionCollectionOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().FirstOrDefault(option => option.Code == "CIR").Selected = true;
			declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().FirstOrDefault(option => option.Code == "TUP").Selected = true;
			var selections = declaration.MergingRules.Cast<MergingRule>();
			AssertEquals(2, selections.Count());
			Assert(selections.Any(selection => selection.CY_Code == "CIR"));
			Assert(selections.Any(selection => selection.CY_Code == "TUP"));
			declaration.MergingRuleOptions.Cast<CodeDescriptionOption>().FirstOrDefault(option => option.Code == "SPM").Selected = true;
			Assert(!selections.Any(selection => selection.CY_Code == "SPM"));
			Factory.Save();
			Assert(selections.Any(selection => selection.CY_Code == "SPM"));
		}

		public void TestClearCEI_CEI_ParentWhenMessageSubTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			var instruction4 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Precondition: CEI_CEI_Parent", instruction1.PK, instruction2.CEI_CEI_Parent);
			AssertEquals("Precondition: CEI_CEI_Parent", instruction3.PK, instruction4.CEI_CEI_Parent);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals("CEI_CEI_Parent should be cleared", ZGuid.Empty, instruction2.CEI_CEI_Parent);
			AssertEquals("CEI_CEI_Parent should be cleared", ZGuid.Empty, instruction4.CEI_CEI_Parent);
		}

		public void TestClearJI_PrimaryPreferenceWhenMessageTypeChangedToExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_PrimaryPreference = "MFN";
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_PrimaryPreference = "STANDARD";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				AssertEquals(ZString.Empty, invoiceLine.JI_PrimaryPreference);
			}
		}

		public void TestSyncingOrgsFromDodAddresses()
		{
			var importer = Factory.New<OrgHeader>();
			importer.Addresses.AddNew();
			var supplier = Factory.New<OrgHeader>();
			supplier.Addresses.AddNew();
			var buyer = Factory.New<OrgHeader>();
			buyer.Addresses.AddNew();
			var manufacturer = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.ImporterDocumentaryAddress.OrganisationPK = importer.PK;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			declaration.BuyerDocAddress.OrganisationPK = buyer.PK;
			declaration.ManufacturerDocumentaryAddress.OrganisationPK = manufacturer.PK;
			AssertEquals("JE_OH_Importer should have been set as ImporterDocumentaryAddress be set.", importer.PK, declaration.JE_OH_Importer);
			AssertEquals("JE_OH_Supplier should have been set as SupplierDocumentaryAddress be set.", supplier.PK, declaration.JE_OH_Supplier);
			AssertEquals("JE_OH_Buyer should have been set as BuyerDocAddress be set.", buyer.PK, declaration.JE_OH_Buyer);
			AssertEquals("JE_OH_Manufacturer should have been set as ManufacturerDocumentaryAddress be set.", manufacturer.PK, declaration.JE_OH_Manufacturer);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_OH_Manufacturer = manufacturer.PK;
			AssertEquals("ManufacturerDocumentaryAddress should have been set as JE_OH_Manufacturer be set.", manufacturer.PK, declaration.ManufacturerDocumentaryAddress.OrganisationPK);
		}

		public void TestJE_MessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.NeedToGetNewIncoTermAndChargeFactory = false;
			AssertEquals("NeedToGetNewIncoTermAndChargeFactory", false, declaration.NeedToGetNewIncoTermAndChargeFactory);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("NeedToGetNewIncoTermAndChargeFactory", true, declaration.NeedToGetNewIncoTermAndChargeFactory);
		}

		public void TestDefaultValuesFromSupplierImporterLinkTransportMode()
		{
			void ClearTransportModeDefaults(JobDeclaration declarationToClear)
			{
				declarationToClear.JE_CustomsOffice = declarationToClear.JE_OfficeOfEntryExit = declarationToClear.JE_CIQOfficeOfEntryExit = declarationToClear.OfficeOfDestination = ZString.Empty;
			}

			void AssertMode1Data(JobDeclaration assertee)
			{
				AssertEquals("ZO_CustomsOffice", "1100", assertee.JE_CustomsOffice);
				AssertEquals("ZO_OfficeOfEntryExit", "2100", assertee.JE_OfficeOfEntryExit);
				AssertEquals("ZO_CIQOfficeOfEntryExit", "3100", assertee.JE_CIQOfficeOfEntryExit);
				AssertEquals("OfficeOfDestination", "4100", assertee.OfficeOfDestination);
			}

			void AssertMode2Data(JobDeclaration assertee)
			{
				AssertEquals("ZO_CustomsOffice", "1200", assertee.JE_CustomsOffice);
				AssertEquals("ZO_OfficeOfEntryExit", "2200", assertee.JE_OfficeOfEntryExit);
				AssertEquals("ZO_CIQOfficeOfEntryExit", "3200", assertee.JE_CIQOfficeOfEntryExit);
				AssertEquals("CIQOfficeCodes", "4200", assertee.OfficeOfDestination);
			}

			var importer = Factory.New<OrgHeader>();
			var supplier = Factory.New<OrgHeader>();
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
			var mode1 = (OrgSupBuyLinkTrnMode)(link.OrgSupBuyLinkTrnModes.FirstOrDefault() ?? link.OrgSupBuyLinkTrnModes.AddNew());
			mode1.PF_TransportMode = Core.Constants.TransportModes.Sea;
			mode1.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			var addInfo1 = (OrgSupBuyLinkTrnModeAddInfo)mode1.AddInfo;
			var addInfo1BizObj = new OrgSupBuyLinkTrnModeAddInfoBizObj(addInfo1)
			{ ZO_CustomsOffice = "1100", ZO_OfficeOfEntryExit = "2100", ZO_CIQOfficeOfEntryExit = "3100" };
			addInfo1BizObj.OfficeOfDestination = "4100";
			var mode2 = link.OrgSupBuyLinkTrnModes.AddNew();
			mode2.PF_TransportMode = Core.Constants.TransportModes.Other;
			mode2.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			var addInfo2 = (OrgSupBuyLinkTrnModeAddInfo)mode2.AddInfo;
			var addInfo2BizObj = new OrgSupBuyLinkTrnModeAddInfoBizObj(addInfo2)
			{ ZO_CustomsOffice = "1200", ZO_OfficeOfEntryExit = "2200", ZO_CIQOfficeOfEntryExit = "3200" };
			addInfo2BizObj.OfficeOfDestination = "4200";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_ContainerMode = "FCL";
			AssertMode1Data(declaration);
			ClearTransportModeDefaults(declaration);
			declaration.JE_TransportMode = "FIX";
			AssertMode2Data(declaration);
			declaration.JE_OH_Importer = ZGuid.Empty;
			ClearTransportModeDefaults(declaration);
			declaration.JE_OH_Importer = importer.PK;
			AssertMode2Data(declaration);
			declaration.JE_OH_Importer = ZGuid.Empty;
			ClearTransportModeDefaults(declaration);
			declaration.JE_OH_Importer = importer.PK;
			AssertMode2Data(declaration);
			declaration.JE_ContainerMode = ZString.Empty;
			ClearTransportModeDefaults(declaration);
			declaration.JE_ContainerMode = "FCL";
			AssertMode2Data(declaration);
			ClearTransportModeDefaults(declaration);
			declaration.JE_TransportMode = "SEA";
			AssertMode1Data(declaration);
		}

		public void TestDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.OfficeOfDestination = "1";
			declaration.MergingRules.AddNew();
			Factory.Save();
			AssertEquals(2, Factory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, declaration.PK)).Length);
			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.Load<JobDeclaration>(declaration.PK).Delete();
			AssertEquals(0, anotherFactory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, declaration.PK)).Length);
		}

		public void TestDeclarationDeadline()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 3);
			AssertEquals(new ZDateTime(2020, 1, 17), declaration.DeclarationDeadline);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 4);
			AssertEquals(new ZDateTime(2020, 1, 18), declaration.DeclarationDeadline);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 5);
			AssertEquals(new ZDateTime(2020, 1, 19), declaration.DeclarationDeadline);
			declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 10);
			AssertEquals(new ZDateTime(2020, 1, 24), declaration.DeclarationDeadline);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals(ZDateTime.Empty, declaration.DeclarationDeadline);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertEquals(ZDateTime.Empty, declaration.DeclarationDeadline);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals(new ZDateTime(2020, 1, 24), declaration.DeclarationDeadline);
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, declaration.DeclarationDeadline);
		}

		public void TestRemainingDaysForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000001", ZDateTime.Empty);
			entryHeader2.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000002", ZDateTime.Empty);
			entryHeader3.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", ZDateTime.Empty);
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals(0, declaration.RemainingDaysForDeclaration);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals(15, declaration.RemainingDaysForDeclaration);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-15);
			AssertEquals(-1, declaration.RemainingDaysForDeclaration);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(10);
			AssertEquals(25, declaration.RemainingDaysForDeclaration);
			entryHeader1.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000001", ZDateTime.Today);
			AssertEquals(25, declaration.RemainingDaysForDeclaration);
			entryHeader2.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000002", ZDateTime.Today);
			AssertEquals(25, declaration.RemainingDaysForDeclaration);
			entryHeader3.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000003", ZDateTime.Today);
			AssertEquals(0, declaration.RemainingDaysForDeclaration);
		}

		public void TestDefaultClearanceMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ClearanceModeList.Codes.Integrated, declaration.JE_ClearanceMode);
		}

		public void TestAnyEntryHasBeenLodgedOrIsWaitingForResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Assert(!declaration.AnyEntryHasBeenLodgedOrIsWaitingForResponse);
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Assert(declaration.AnyEntryHasBeenLodgedOrIsWaitingForResponse);
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000001", ZDateTime.Empty);
			Assert(declaration.AnyEntryHasBeenLodgedOrIsWaitingForResponse);
		}

		public void TestIsTwoStepDeclarationApplicable()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(declaration.IsTwoStepDeclarationApplicable);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				Assert(!declaration.IsTwoStepDeclarationApplicable);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				Assert(!declaration.IsTwoStepDeclarationApplicable);
			}

			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(!declaration.IsTwoStepDeclarationApplicable);
			}
		}

		public void TestClearanceModeReadOnly()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(!declaration.ClearanceModeReadOnly);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				Assert(declaration.ClearanceModeReadOnly);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				Assert(declaration.ClearanceModeReadOnly);
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				Assert(declaration.ClearanceModeReadOnly);
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
				entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000001", ZDateTime.Empty);
				Assert(declaration.ClearanceModeReadOnly);
			}
		}

		public void TestIsTwoStepDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			Assert(declaration.IsTwoStepDeclaration);
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepManual;
			Assert(declaration.IsTwoStepDeclaration);
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepAuto;
			Assert(declaration.IsTwoStepDeclaration);
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			Assert(!declaration.IsTwoStepDeclaration);
		}

		public void TestClearanceModeDetailedDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			declaration.JE_LicenseInvolved = false;
			declaration.JE_InspectionInvolved = false;
			declaration.JE_TaxInvolved = false;
			AssertEquals("Two-step Declaration (Multiple entry), non Certification-Related, non Inspection-Related, non Tax-Related", declaration.ClearanceModeDetailedDescription);
			declaration.JE_LicenseInvolved = true;
			declaration.JE_InspectionInvolved = false;
			declaration.JE_TaxInvolved = false;
			AssertEquals("Two-step Declaration (Multiple entry), Certification-Related, non Inspection-Related, non Tax-Related", declaration.ClearanceModeDetailedDescription);
			declaration.JE_LicenseInvolved = false;
			declaration.JE_InspectionInvolved = true;
			declaration.JE_TaxInvolved = false;
			AssertEquals("Two-step Declaration (Multiple entry), non Certification-Related, Inspection-Related, non Tax-Related", declaration.ClearanceModeDetailedDescription);
			declaration.JE_LicenseInvolved = false;
			declaration.JE_InspectionInvolved = false;
			declaration.JE_TaxInvolved = true;
			AssertEquals("Two-step Declaration (Multiple entry), non Certification-Related, non Inspection-Related, Tax-Related", declaration.ClearanceModeDetailedDescription);
			declaration.JE_LicenseInvolved = true;
			declaration.JE_InspectionInvolved = true;
			declaration.JE_TaxInvolved = false;
			AssertEquals("Two-step Declaration (Multiple entry), Certification-Related, Inspection-Related, non Tax-Related", declaration.ClearanceModeDetailedDescription);
			declaration.JE_LicenseInvolved = true;
			declaration.JE_InspectionInvolved = false;
			declaration.JE_TaxInvolved = true;
			AssertEquals("Two-step Declaration (Multiple entry), Certification-Related, non Inspection-Related, Tax-Related", declaration.ClearanceModeDetailedDescription);
			declaration.JE_LicenseInvolved = false;
			declaration.JE_InspectionInvolved = true;
			declaration.JE_TaxInvolved = true;
			AssertEquals("Two-step Declaration (Multiple entry), non Certification-Related, Inspection-Related, Tax-Related", declaration.ClearanceModeDetailedDescription);
			declaration.JE_LicenseInvolved = true;
			declaration.JE_InspectionInvolved = true;
			declaration.JE_TaxInvolved = true;
			AssertEquals("Two-step Declaration (Multiple entry), Certification-Related, Inspection-Related, Tax-Related", declaration.ClearanceModeDetailedDescription);
		}

		public void TestResetClearanceMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			SetTwoStepClearanceValues(declaration);
			AssertTwoStepClearanceValues(ClearanceModeList.Codes.TwoStep, true, declaration);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertTwoStepClearanceValues(ClearanceModeList.Codes.Integrated, false, declaration);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			SetTwoStepClearanceValues(declaration);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertTwoStepClearanceValues(ClearanceModeList.Codes.Integrated, false, declaration);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			SetTwoStepClearanceValues(declaration);
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			AssertTwoStepClearanceValues(ClearanceModeList.Codes.Integrated, false, declaration);
		}

		public void SetTwoStepClearanceValues(JobDeclaration jobDeclaration)
		{
			jobDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			jobDeclaration.JE_LicenseInvolved = true;
			jobDeclaration.JE_InspectionInvolved = true;
			jobDeclaration.JE_TaxInvolved = true;
		}

		public void AssertTwoStepClearanceValues(string clearanceMode, bool target, JobDeclaration jobDeclaration)
		{
			AssertEquals(clearanceMode, jobDeclaration.JE_ClearanceMode);
			AssertEquals(target, jobDeclaration.JE_LicenseInvolved);
			AssertEquals(target, jobDeclaration.JE_InspectionInvolved);
			AssertEquals(target, jobDeclaration.JE_TaxInvolved);
		}

		public void TestFullValidationReadOnly()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var declaration = Factory.New<JobDeclaration>();
				Assert(declaration.FullValidationReadOnly);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(!declaration.FullValidationReadOnly);
				declaration.JE_ClearanceMode = "";
				Assert(declaration.FullValidationReadOnly);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				Assert(declaration.FullValidationReadOnly);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert(declaration.FullValidationReadOnly);
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				Assert(declaration.FullValidationReadOnly);
			}

			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), false))
			{
				var declaration = Factory.New<JobDeclaration>();
				Assert(declaration.FullValidationReadOnly);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(declaration.FullValidationReadOnly);
				declaration.JE_ClearanceMode = "";
				Assert(declaration.FullValidationReadOnly);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				Assert(declaration.FullValidationReadOnly);
			}
		}

		public void TestFullValidation()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				Assert(declaration.FullValidation);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				Assert(!declaration.FullValidation);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				Assert(declaration.FullValidation);
				declaration.ValidationMode = ValidationModes.Preliminary;
				Assert(!declaration.FullValidation);
				declaration.ValidationMode = ValidationModes.Full;
				Assert(declaration.FullValidation);

				declaration.FullValidation = ZBool.True;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.FullValidation = ZBool.False;
				AssertEquals(ValidationModes.Preliminary, declaration.ValidationMode);
			}
		}

		public void TestDefaultValidationMode()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertEquals(ValidationModes.Preliminary, declaration.ValidationMode);
				declaration.ValidationMode = ValidationModes.Full;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.FullValidation = ZBool.True;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.FullValidation = ZBool.False;
				AssertEquals(ValidationModes.Preliminary, declaration.ValidationMode);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertEquals(ValidationModes.Preliminary, declaration.ValidationMode);
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.JE_ClearanceMode = "";
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);

				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				declaration.ValidationMode = ValidationModes.Preliminary;
				declaration.OnLoaded();
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);

				declaration.FullValidation = ZBool.True;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.FullValidation = ZBool.False;
				AssertEquals(ValidationModes.Preliminary, declaration.ValidationMode);

				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);
				declaration.JE_ClearanceMode = "";
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);

				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				declaration.OnLoaded();
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);

				declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
				declaration.ValidationMode = ValidationModes.Preliminary;
				declaration.OnLoaded();
				AssertEquals(ValidationModes.Full, declaration.ValidationMode);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				declaration.ValidationMode = ValidationModes.Full;

				Factory.Save();

				var declarationInAnotherFactory = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals("Default ValidationMode on loading TSD job", ValidationModes.Preliminary, declarationInAnotherFactory.ValidationMode);
			}
		}

		public void TestCusAgentProperties()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "TBR";
			broker.GS_FullName = "Test Broker";
			var declaration = Factory.New<JobDeclaration>();
			AssertNoExceptionThrown("Should not throw execption with null Agent", () =>
			{
				GlbStaffTestHelper.AssertCusAgentProperties(declaration);
			}

			);
			declaration.JE_GS_NKCusAgent = "TBR";
			GlbStaffTestHelper.AssertCusAgentProperties(declaration, brokerName: "Test Broker", cnoName: "Test Broker");
			broker.AddCert("ACE", "CN", "ACE001");
			broker.AddCert("BRK", "DE", "BRKDE");
			var cnBrk = broker.AddCert("BRK", "CN", "BRKCN");
			GlbStaffTestHelper.AssertCusAgentProperties(declaration, brokerName: "Test Broker", brokerNumber: "BRKCN", cnoName: "Test Broker");
			cnBrk.XZ_Comment = "Test Broker BRK Name";
			GlbStaffTestHelper.AssertCusAgentProperties(declaration, brokerName: "Test Broker BRK Name", brokerNumber: "BRKCN", cnoName: "Test Broker");
			var cnCNO = broker.AddCert("CNO", "CN", "CNO001");
			GlbStaffTestHelper.AssertCusAgentProperties(declaration, brokerName: "Test Broker BRK Name", brokerNumber: "BRKCN", cnoName: "Test Broker", cnoNumber: "CNO001");
			cnCNO.XZ_Comment = "Test Broker CNO Name";
			GlbStaffTestHelper.AssertCusAgentProperties(declaration, brokerName: "Test Broker BRK Name", brokerNumber: "BRKCN", cnoName: "Test Broker CNO Name", cnoNumber: "CNO001");
		}

		#region JE_MessageType

		public override void TestIsMessageTypeChangeAnError()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			Assert("[PreCondition]: IsMessageTypeChangeAnError should be false", !declaration.IsMessageTypeChangeAnError);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.DeclarationUnifiedNumber = "1";

			Assert("IsMessageTypeChangeAnError should be true", declaration.IsMessageTypeChangeAnError);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();

			Assert("[PreCondition]: IsMessageTypeChangeAnError should be false", !declaration.IsMessageTypeChangeAnError);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = JobMessageStatusList.Codes.AwaitingResponseCompletedDeclaration;

			Assert("IsMessageTypeChangeAnError should be true", declaration.IsMessageTypeChangeAnError);
		}

		#endregion

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		protected override bool ExpectedSupportInvoiceLineRefs => true;

		void CreateTestLocoMapping()
		{
			CreateUNLoco("CNS");
			CreateUNLoco("CNS1");
			CreateUNLoco("US111");
			CreateLocoMap("0001", "CNS", CNLocoMapSystemUsageList.Codes.CustomsPortCodeList);
			CreateLocoMap("0002", "US111", CNLocoMapSystemUsageList.Codes.CustomsPortCodeList);
			Factory.Save();
		}

		void CreateUNLoco(string locoCode)
		{
			var testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
			testUSLoco.RL_Code = locoCode;
			testUSLoco.RL_PortName = "TEST Port - " + locoCode;
			testUSLoco.RL_IsSystem = true;
			testUSLoco.RL_HasAirport = true;
			testUSLoco.RL_HasSeaport = true;
			testUSLoco.RL_RN_NKCountryCode = "CN";
		}

		void CreateLocoMap(string localPort, string unLoco, string usage)
		{
			var locoMap = Factory.NewWithValidTestData<RefLocoMap>();
			locoMap.RY_LocalPortCode = localPort;
			locoMap.RY_RL_NKLocoPort = unLoco;
			locoMap.RY_SystemUsage = usage;
			locoMap.RY_RN = Core.Constants.CountryGuids.China;
		}
	}

	class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategyExposed(BusinessObjectFactory alternateFactory, CloneType cloneType) => base.GetTemplateCopyStrategy(alternateFactory, cloneType);
	}
}
