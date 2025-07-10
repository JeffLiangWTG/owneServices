using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;
using RefCusConditionType = Enterprise.Customs.KR.Messaging.Constants.ZZ.RefCusConditionType;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EXPJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestTransactionTypeCode()
		{
			CombineAssertions("Check TransactionTypeCode", () =>
			{
				Declaration.Validation.ValidateJE_ExportGoodsType();
				AssertHasMessageErrorContaining(Declaration.JE_ExportGoodsTypeInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_ExportGoodsType = "123";
				AssertHasMessageErrorContaining(Declaration.JE_ExportGoodsTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._100;
				AssertNoMessageErrors(Declaration.JE_ExportGoodsTypeInfo);

				Declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.AD;
				Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._11;
				AssertNoMessageErrorContaining(declaration.JE_ExportGoodsTypeInfo, "If Auto Drawback is ‘AD’, then Transaction Type must be ‘11’.");

				declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._78;
				AssertHasMessageErrorContaining(declaration.JE_ExportGoodsTypeInfo, "If Auto Drawback is ‘AD’, then Transaction Type must be ‘11’.");

				Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
				Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._71;
				AssertNoMessageErrorContaining(declaration.JE_ExportGoodsTypeInfo, "If Declaration Type is 'M', then Transaction Type should be 71, 78, or 79.");

				Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._78;
				AssertNoMessageErrorContaining(declaration.JE_ExportGoodsTypeInfo, "If Declaration Type is 'M', then Transaction Type should be 71, 78, or 79.");

				Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._79;
				AssertNoMessageErrorContaining(declaration.JE_ExportGoodsTypeInfo, "If Declaration Type is 'M', then Transaction Type should be 71, 78, or 79.");

				Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._100;
				AssertHasMessageErrorContaining(declaration.JE_ExportGoodsTypeInfo, "If Declaration Type is 'M', then Transaction Type should be 71, 78, or 79.");
			});
		}

		public void TestExportTypeCode()
		{
			Declaration.JE_ProcedureType = "E";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageErrors(Declaration.JE_MessageSubTypeInfo);

			Declaration.JE_ProcedureType = "H";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_MessageSubType = "Z";
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_MessageSubType = ExportTypeCodeList.Codes.A;
			AssertNoMessageErrors(Declaration.JE_MessageSubTypeInfo);

			Declaration.JE_ExportGoodsType = "80";
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Transaction Type is '80', then Export Type must be ‘B’ or ‘H’.");

			Declaration.JE_MessageSubType = ExportTypeCodeList.Codes.B;
			AssertNoMessageErrors(Declaration.JE_MessageSubTypeInfo);
		}

		public void TestDeclarationCustomsOffice()
		{
			CombineAssertions("Check DeclarationCustomsOffice", () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsOffice, "Customs Office");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				Factory.Save();

				Declaration.JE_ProcedureType = "E";
				Declaration.JE_CustomsDivision = ZString.Empty;
				Declaration.Validation.ValidateJE_CustomsOffice();
				AssertNoMessageErrors(Declaration.JE_CustomsOfficeInfo);

				Declaration.JE_CustomsDivision = "10";
				Declaration.Validation.ValidateJE_CustomsOffice();
				AssertHasMessageErrorContaining(Declaration.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_ProcedureType = "H";
				Declaration.Validation.ValidateJE_CustomsOffice();
				AssertHasMessageErrorContaining(Declaration.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_CustomsOffice = "X";
				AssertHasMessageErrorContaining(Declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_CustomsOffice = "010";
				AssertNoMessageErrors(Declaration.JE_CustomsOfficeInfo);
			});
		}

		public void TestCountryofDestination()
		{
			CombineAssertions("Check CountryofDestination", () =>
			{
				Declaration.Validation.ValidateJE_GoodsDestination();
				AssertHasMessageErrorContaining(Declaration.JE_GoodsDestinationInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_GoodsDestination = "XX";
				AssertHasMessageErrorContaining(Declaration.JE_GoodsDestinationInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Kenya;
				AssertNoMessageErrors(Declaration.JE_GoodsDestinationInfo);
			});
		}

		public void TestGoodsLocationPostcode()
		{
			CombineAssertions("Check GoodsLocationPostcode", () =>
			{
				Declaration.Validation.ValidateJE_SubLocationOfGoods();
				AssertHasMessageErrorContaining(Declaration.JE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_SubLocationOfGoods = "12345";
				AssertNoMessageErrors(Declaration.JE_SubLocationOfGoodsInfo);
			});
		}

		public void TestGoodsLocationAdditionalDetails()
		{
			CombineAssertions("Check GoodsLocationAdditionalDetails", () =>
			{
				Declaration.Validation.ValidateJE_LocationOfGoods();
				AssertHasMessageErrorContaining(Declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_LocationOfGoods = "장치장소명";
				AssertNoMessageErrors(Declaration.JE_LocationOfGoodsInfo);
			});
		}

		public void TestGoodsLocationBondedAreaCode()
		{
			Declaration.JE_ProcedureType = "E";
			Declaration.Validation.ValidateJE_LocationOtherInformation();
			AssertNoMessageErrors(Declaration.JE_LocationOtherInformationInfo);

			Declaration.JE_ProcedureType = "B";
			Declaration.Validation.ValidateJE_LocationOtherInformation();
			AssertHasMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_LocationOtherInformation = "11111111";
			AssertNoMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_ProcedureType = "M";
			Declaration.Validation.ValidateJE_LocationOtherInformation();
			AssertNoMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		//Cargo Management No: Column is not ready.

		public void TestJE_TransportMode()
		{
			CombineAssertions("Check JE_TransportMode", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ProcedureType = "B";
				declaration.JE_TransportMode = "";
				AssertHasMessageErrorContaining(declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TransportMode = "Err";
				AssertHasMessageErrorContaining(declaration.JE_TransportModeInfo, "The code you have selected is not in the list.");

				declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
				AssertNoMessageErrors(declaration.JE_TransportModeInfo);

				declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
				AssertNoMessageErrors(declaration.JE_TransportModeInfo);

				declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Mail;
				AssertNoMessageErrors(declaration.JE_TransportModeInfo);

				declaration.JE_ProcedureType = "E";
				declaration.JE_TransportMode = "";
				AssertNoMessageErrors(declaration.JE_TransportModeInfo);

				declaration.JE_TransportMode = "Err";
				AssertHasMessageErrorContaining(declaration.JE_TransportModeInfo, "The code you have selected is not in the list.");
			});
		}

		public void TestBrokerAddress()
		{
			CombineAssertions("Check BrokerAddress", () =>
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Code = "KRC";
				company.GC_RN_NKCountryCode = "KR";
				var branch = company.Branches.AddNew();
				branch.GB_Code = "KRC";
				var broker = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "");
				broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
				branch.GB_OH_OrgProxy = broker.PK;

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_GB = branch.PK;
				AssertHasMessageErrorContaining(declaration.JE_GBInfo, "The company name of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a company name for the proxy organization.");
				AssertHasMessageErrorContaining(declaration.JE_GBInfo, "The name of this company's representative is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");

				broker.OH_FullName = "READYKOREA";
				TestOrgDataSetUpHelper.AddOrgContact(broker, "RepresentativeName", true);
				declaration.JE_GB = branch.PK;
				AssertNoMessageErrorContaining(declaration.JE_GBInfo, "The company name of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a company name for the proxy organization.");
				AssertNoMessageErrorContaining(declaration.JE_GBInfo, "The name of this company's representative is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
			});
		}

		public void TestJE_OA_SellerAddress()
		{
			CombineAssertions("Check JE_OA_SellerAddress", () =>
			{
				var sellerEmpty = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
				var seller = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA");
				var sellerCodes = new IDNumberAndType[]
				{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "123456789012345" },
				};
				TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCodes);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ProcedureType = "E";
				declaration.SellerOrgPK = ZGuid.Empty;
				declaration.JE_OA_SellerAddress = ZGuid.Empty;
				AssertNoErrorContaining(declaration.JE_OA_SellerAddressInfo, "Please select an address for the entered organization.");
				AssertHasMessageErrorContaining(declaration.JE_OA_SellerAddressInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.SellerOrgPK = sellerEmpty.PK;
				AssertHasErrorContaining(declaration.JE_OA_SellerAddressInfo, "Please select an address for the entered organization.");
				AssertHasMessageErrorContaining(declaration.JE_OA_SellerAddressInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_OA_SellerAddress = sellerEmpty.MainAddress.PK;
				AssertNoErrorContaining(declaration.JE_OA_SellerAddressInfo, "Please select an address for the entered organization.");
				AssertNoMessageErrorContaining(declaration.JE_OA_SellerAddressInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(declaration.JE_OA_SellerAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
				AssertNoMessageErrorContaining(declaration.JE_OA_SellerAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

				declaration.JE_ProcedureType = "B";
				declaration.Validation.ValidateJE_OA_SellerAddress();
				AssertHasMessageErrorContaining(declaration.JE_OA_SellerAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

				declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
				AssertNoErrorContaining(declaration.JE_OA_SellerAddressInfo, "Please select an address for the entered organization.");
				AssertNoMessageErrorContaining(declaration.JE_OA_SellerAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			});
		}

		public void TestJE_OA_ManufacturerAddress()
		{
			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "ReadyKorea");
			var declaration = Factory.New<JobDeclaration>();

			declaration.ManufacturerOrgPK = ZGuid.Empty;
			declaration.JE_OA_ManufacturerAddress = ZGuid.Empty;
			AssertNoErrorContaining(declaration.JE_OA_ManufacturerAddressInfo, "Please select an address for the entered organization.");

			declaration.ManufacturerOrgPK = manufacturer.PK;
			declaration.JE_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasErrorContaining(declaration.JE_OA_ManufacturerAddressInfo, "Please select an address for the entered organization.");

			declaration.JE_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertNoMessageErrorContaining(declaration.JE_OA_ManufacturerAddressInfo, "Please select an address for the entered organization.");
		}

		public void TestJE_TotalNoOfPacksPackType()
		{
			CombineAssertions("Check JE_TotalNoOfPacksPackType", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TotalNoOfPacksPackType = "";
				AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_TotalNoOfPacksPackType = "33";
				AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.BA;
				AssertNoMessageErrors(declaration.JE_TotalNoOfPacksPackTypeInfo);
			});
		}

		public void TestForwarderOC_ContactName()
		{
			var emptyContactforwarder = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "READYKOREA");
			var forwarder = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "READYKOREA");
			var forwardarContactName = forwarder.Contacts.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "A";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageErrors(declaration.JE_TransportModeInfo);

			declaration.JE_MessageSubType = "B";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
			declaration.JE_MessageSubType = "D";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
			declaration.JE_MessageSubType = "E";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");

			TestOrgDataSetUpHelper.AddOrgContact(forwarder, "ForwarderName", true);
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);

			declaration.JE_MessageSubType = "D";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);

			declaration.JE_MessageSubType = "B";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);

			declaration.JE_ProcedureType = "M";
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, "When Declaration Type is 'M', you should not enter Forwarder");

			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);
		}

		public void TestIndustrialParkCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.IndustrialParkCode, "Industrial Park Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.IndustrialParkCode, "101", "한국수출", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var org = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTest1", "CompanyName1");
			var cusCodes = new IDNumberAndType[]
			{
			new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org, cusCodes);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTest2", "CompanyName2");
			var cusCodes2 = new IDNumberAndType[]
			{
			new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "123", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org2.MainAddress, cusCodes2);

			var org3 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTest3", "CompanyName3");
			var cusCodes3 = new IDNumberAndType[]
			{
			new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "101", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org3.MainAddress, cusCodes3);
			Factory.Save();

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			Declaration.JE_ProcedureType = "E";
			Declaration.JE_OA_ManufacturerAddress = org.MainAddress.PK;
			var validation = (EXPJobDeclarationValidation)Declaration.Validation;
			validation.ValidateIndustrialParkCode();
			AssertNoMessageErrorContaining(Declaration.IndustrialParkCodeInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_ProcedureType = "H";
			validation.ValidateIndustrialParkCode();
			AssertNoMessageErrorContaining(Declaration.IndustrialParkCodeInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_OA_ManufacturerAddress = org.MainAddress.PK;
			validation.ValidateIndustrialParkCode();
			AssertNoMessageErrorContaining(Declaration.IndustrialParkCodeInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_OA_ManufacturerAddress = org2.MainAddress.PK;
			validation.ValidateIndustrialParkCode();
			AssertHasMessageErrorContaining(Declaration.IndustrialParkCodeInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_OA_ManufacturerAddress = org3.MainAddress.PK;
			validation.ValidateIndustrialParkCode();
			AssertNoNotifications(Declaration.IndustrialParkCodeInfo);
		}

		public void TestFinalBondedWarehouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01001001", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var org = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST1", "CompanyName1");
			var cusCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org, cusCodes);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST2", "CompanyName2");
			TestOrgDataSetUpHelper.AddOrgAddress(org2.MainAddress, "서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", "", "06636");
			var cusCodes2 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103898", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org2, cusCodes2);
			var cusAddressCodes2 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ControlledPremisesID, Number = "01012311", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org2.MainAddress, cusAddressCodes2);

			var org3 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST3", "CompanyName3");
			TestOrgDataSetUpHelper.AddOrgAddress(org3.MainAddress, "서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", "", "06636");
			var cusCodes3 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103899", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org3, cusCodes3);
			var cusAddressCodes3 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ControlledPremisesID, Number = "01001001", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org3.MainAddress, cusAddressCodes3);
			Factory.Save();

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			Declaration.ContainerTerminalOperatorDocAddress.DocAddressType = MasterFiles.Integration.DocAddressType.CustomsContainerTerminalOperatorAddress;
			var validation = (EXPJobDeclarationValidation)Declaration.Validation;

			Declaration.JE_ProcedureType = "E";
			validation.ValidateFinalBondedWarehouse();
			AssertNoWarningContaining(Declaration.FinalBondedWarehouseInfo, "Please check if this CTO has a Final Bonded Warehouse assigned. As there is no Final Bonded Warehouse, 'Customs Office + 99999' will be sent in a declaration.");
			AssertNoMessageErrorContaining(Declaration.FinalBondedWarehouseInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_ProcedureType = "H";
			validation.ValidateFinalBondedWarehouse();
			AssertHasWarningContaining(Declaration.FinalBondedWarehouseInfo, "Please check if this CTO has a Final Bonded Warehouse assigned. As there is no Final Bonded Warehouse, 'Customs Office + 99999' will be sent in a declaration.");
			AssertNoMessageErrorContaining(Declaration.FinalBondedWarehouseInfo, ListValidation.InvalidCodeMessageError);

			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = org.MainAddress.PK;
			validation.ValidateFinalBondedWarehouse();
			AssertHasWarningContaining(Declaration.FinalBondedWarehouseInfo, "Please check if this CTO has a Final Bonded Warehouse assigned. As there is no Final Bonded Warehouse, 'Customs Office + 99999' will be sent in a declaration.");
			AssertNoMessageErrorContaining(Declaration.FinalBondedWarehouseInfo, ListValidation.InvalidCodeMessageError);

			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = org2.MainAddress.PK;
			validation.ValidateFinalBondedWarehouse();
			AssertNoWarningContaining(Declaration.FinalBondedWarehouseInfo, "Please check if this CTO has a Final Bonded Warehouse assigned. As there is no Final Bonded Warehouse, 'Customs Office + 99999' will be sent in a declaration.");
			AssertHasMessageErrorContaining(Declaration.FinalBondedWarehouseInfo, ListValidation.InvalidCodeMessageError);

			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = org3.MainAddress.PK;
			validation.ValidateFinalBondedWarehouse();
			AssertNoWarningContaining(Declaration.FinalBondedWarehouseInfo, "Please check if this CTO has a Final Bonded Warehouse assigned. As there is no Final Bonded Warehouse, 'Customs Office + 99999' will be sent in a declaration.");
			AssertNoMessageErrorContaining(Declaration.FinalBondedWarehouseInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2022, 12, 13)]
		public void TestPreferredInspectionDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			var validation = (EXPJobDeclarationValidation)declaration.Validation;
			validation.ValidateInspectionDate();
			AssertNoMessageErrors("If the Declaration Procedure Type is E, it does not matter if the Preferred Inspection Date is empty.", declaration.InspectionDateInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			validation.ValidateInspectionDate();
			AssertHasMessageErrorContaining("If the Declaration Procedure Type is not E, the Preferred Inspection Date must not be empty.", declaration.InspectionDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = ExportTypeCodeList.Codes.G;
			validation.ValidateInspectionDate();
			AssertNoMessageErrors("If the Transaction Type is G, it does not matter if the Preferred Inspection Date is empty.", declaration.InspectionDateInfo);

			declaration.JE_MessageSubType = ExportTypeCodeList.Codes.A;
			validation.ValidateInspectionDate();
			AssertHasMessageErrorContaining("If the Transaction Type is G, it does not matter if the Preferred Inspection Date is empty.", declaration.InspectionDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.InspectionDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageErrorContaining(declaration.InspectionDateInfo, "The Preferred Inspection Date must be greater than or equal to the Declaration Date.");

			declaration.InspectionDate = ZDateTime.Today;
			AssertNoMessageErrors(declaration.InspectionDateInfo);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = declaration.JE_MessageType;
			entry1.EntryNumber = "1234567890I";
			var entryNum1 = entry1.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2022, 12, 10, 08, 36, 00);

			declaration.InspectionDate = new ZDateTime(2022, 12, 10, 00, 00, 00);
			AssertNoMessageErrors("The time is different, but the date is the same.", declaration.InspectionDateInfo);
		}

		public void TestUnderbondMovementDepartureDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "B";

			var validation = (EXPJobDeclarationValidation)declaration.Validation;
			validation.ValidateUnderbondMovementDepartureDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementDepartureDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckExportTypeError);

			declaration.JE_MessageSubType = "D";
			validation.ValidateUnderbondMovementDepartureDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementDepartureDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckExportTypeError);

			declaration.JE_MessageSubType = "E";
			validation.ValidateUnderbondMovementDepartureDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementDepartureDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckExportTypeError);

			declaration.JE_MessageSubType = "Z";
			validation.ValidateUnderbondMovementDepartureDate();
			AssertNoMessageErrors(declaration.UnderbondMovementDepartureDateInfo);

			declaration.UnderbondMovementDepartureDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageErrorContaining(declaration.UnderbondMovementDepartureDateInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_MessageSubType = "B";
			validation.ValidateUnderbondMovementDepartureDate();
			AssertNoMessageErrors(declaration.UnderbondMovementDepartureDateInfo);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today;
			validation.ValidateUnderbondMovementDepartureDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementDepartureDateInfo, EXPJobDeclarationValidation.ExpiryDateMessageError);

			declaration.UnderbondMovementDepartureDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageErrorContaining(declaration.UnderbondMovementDepartureDateInfo, EXPJobDeclarationValidation.ExpiryDateMessageError);

			declaration.UnderbondMovementDepartureDate = ZDateTime.Today;
			AssertNoMessageErrors(declaration.UnderbondMovementDepartureDateInfo);

			declaration.JE_ProcedureType = "M";
			validation.ValidateUnderbondMovementDepartureDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementDepartureDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckDeclarationTypeError);

			declaration.JE_ProcedureType = "M";
			declaration.UnderbondMovementDepartureDate = ZDateTime.Empty;
			AssertNoMessageErrors(declaration.UnderbondMovementDepartureDateInfo);
		}

		public void TestUnderbondMovementArrivalDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "B";

			var validation = (EXPJobDeclarationValidation)declaration.Validation;
			validation.ValidateUnderbondMovementArrivalDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckExportTypeError);

			declaration.JE_MessageSubType = "D";
			validation.ValidateUnderbondMovementArrivalDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckExportTypeError);

			declaration.JE_MessageSubType = "E";
			validation.ValidateUnderbondMovementArrivalDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckExportTypeError);

			declaration.JE_MessageSubType = "Z";
			validation.ValidateUnderbondMovementArrivalDate();
			AssertNoMessageErrors(declaration.UnderbondMovementArrivalDateInfo);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today;
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_MessageSubType = "B";
			validation.ValidateUnderbondMovementArrivalDate();
			AssertNoMessageErrors(declaration.UnderbondMovementArrivalDateInfo);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Empty;
			declaration.UnderbondMovementDepartureDate = ZDateTime.Today;
			validation.ValidateUnderbondMovementArrivalDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, EXPJobDeclarationValidation.IssueDateMessageError);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(+1);
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, EXPJobDeclarationValidation.IssueDateMessageError);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today;
			AssertNoMessageErrors(declaration.UnderbondMovementArrivalDateInfo);

			declaration.JE_ProcedureType = "M";
			validation.ValidateUnderbondMovementArrivalDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, EXPJobDeclarationValidation.UnderbondPeriodCheckDeclarationTypeError);

			declaration.JE_ProcedureType = "M";
			declaration.UnderbondMovementArrivalDate = ZDateTime.Empty;
			AssertNoMessageErrors(declaration.UnderbondMovementArrivalDateInfo);
		}

		public void TestExportGoodsType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ProcedureType = ExportDeclarationTypeCodeList.Codes.B;
			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._11;
			AssertNoMessageErrors(declaration.JE_ExportGoodsTypeInfo);

			var validation = (EXPJobDeclarationValidation)declaration.Validation;

			declaration.JE_ProcedureType = ExportDeclarationTypeCodeList.Codes.E;
			validation.ValidateJE_ExportGoodsType();
			AssertHasMessageErrorContaining(declaration.JE_ExportGoodsTypeInfo, EXPJobDeclarationValidation.ExportGoodsTypeCheckError);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._15;
			AssertNoMessageErrors(declaration.JE_ExportGoodsTypeInfo);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._17;
			AssertNoMessageErrors(declaration.JE_ExportGoodsTypeInfo);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._103;
			AssertNoMessageErrors(declaration.JE_ExportGoodsTypeInfo);
		}

		public void TestJE_CarrierCode()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.KoreaSouth;
			carrier1.ZZ4_Code = "ABC";
			carrier1.ZZ4_IsAir = true;
			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.KoreaSouth;
			carrier2.ZZ4_Code = "DEF";
			carrier2.ZZ4_IsSea = true;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(1, declaration.Lookups.CarrierCodeCollection.Count);
			declaration.JE_CarrierCode = "DEF";
			AssertHasMessageErrorContaining(Declaration.JE_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_CarrierCode = "ABC";
			AssertNoMessageErrors(Declaration.JE_CarrierCodeInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(1, declaration.Lookups.CarrierCodeCollection.Count);
			declaration.JE_CarrierCode = "ABC";
			AssertHasMessageErrorContaining(Declaration.JE_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_CarrierCode = "DEF";
			AssertNoMessageErrors(Declaration.JE_CarrierCodeInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals(2, declaration.Lookups.CarrierCodeCollection.Count);
			declaration.JE_CarrierCode = "ABC";
			AssertNoMessageErrors(Declaration.JE_CarrierCodeInfo);
			declaration.JE_CarrierCode = "DEF";
			AssertNoMessageErrors(Declaration.JE_CarrierCodeInfo);
			declaration.JE_CarrierCode = "GHI";
			AssertHasMessageErrorContaining(Declaration.JE_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_LocationQualifier()
		{
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_LocationQualifierInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationQualifier = "08600";
			AssertNoMessageErrors(Declaration.JE_LocationQualifierInfo);
		}

		public void TestJE_OA_SupplierAddress()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "86420");

			var declarant = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "인천효민관세사무소");
			declarant.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var declarantCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "012345678901234" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(declarant, declarantCodes);
			Declaration.Branch.GB_OH_OrgProxy = declarant.PK;

			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA1", "(주)레디코리아");
			var sellerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리아123456" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCodes);
			Declaration.JE_OA_SellerAddress = seller.MainAddress.PK;

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA2", "(주)레디코리아");
			var supplierCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1234567890123" },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리아123456" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "SupplierName", true);
			var supplierAddress = supplier.MainAddress;
			supplier.MainAddress.Postcode = "123456";
			Declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The maximum number of digits in a zip code is 5 digits.");

			supplier.MainAddress.Postcode = "12345";
			Declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The maximum number of digits in a zip code is 5 digits.");

			CombineAssertions("Check Valid value.", () =>
			{
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_OA_SupplierAddress = ZGuid.Empty;
				AssertNoErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "Please select an address for the entered organization.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_OH_Supplier = supplier.PK;
				Declaration.JE_OA_SupplierAddress = ZGuid.Empty;
				AssertHasErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "Please select an address for the entered organization.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_OA_SupplierAddress = supplierAddress.PK;
				AssertNoErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "Please select an address for the entered organization.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Check JZ_OA_SupplierAddress", () =>
			{
				var supplierEmpty = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "");

				Declaration.JE_ProcedureType = "H";
				Declaration.JE_OA_SupplierAddress = supplierEmpty.MainAddress.PK;
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The post code of this company is missing. Press F3 here and enter the post code.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

				Declaration.JE_ProcedureType = "E";
				Declaration.JE_OA_SupplierAddress = supplierEmpty.MainAddress.PK;
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

				Declaration.JE_ProcedureType = "H";
				Declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The post code of this company is missing. Press F3 here and enter the post code.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

				var individualEmpty = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "RK", "");

				var individual = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "RK", "Supplier");
				var individualCodes = new IDNumberAndType[]
				{
					new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "1234567890123" },
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "123456789012345" }
				};
				TestOrgDataSetUpHelper.AddCustomsCode(individual, individualCodes);
				TestOrgDataSetUpHelper.AddOrgContact(individual, "SupplierName", true);
				var individualAddress = individual.MainAddress;
				individualAddress.Postcode = "12345";

				Declaration.JE_ProcedureType = "H";
				Declaration.JE_OA_SupplierAddress = individualEmpty.MainAddress.PK;
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The post code of this company is missing. Press F3 here and enter the post code.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Individual Registration Number for this organization. Please press F3 here and add a number of type '01' or 'PAS' or '03' or '05' in Config > Registration Numbers/Codes on the Organization form.");
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

				Declaration.JE_ProcedureType = "E";
				Declaration.JE_OA_SupplierAddress = individualEmpty.MainAddress.PK;
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

				Declaration.JE_ProcedureType = "H";
				Declaration.JE_OA_SupplierAddress = individualAddress.PK;
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The post code of this company is missing. Press F3 here and enter the post code.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Individual Registration Number for this organization. Please press F3 here and add a number of type '01' or 'PAS' or '03' or '05' in Config > Registration Numbers/Codes on the Organization form.");
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");
			});

			CombineAssertions("If the first number of the declarant code is not 1 to 5, Declarant and Supplier must be the same company.", () =>
			{
				Declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
				AssertEquals("86420", KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithFallbackDefault(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty).ToString());
				AssertEquals("012345678901234", Declaration.BrokerAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization));
				AssertEquals("레디코리아123456", Declaration.SupplierAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization));
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "For the self - declaring suppliers, the declarant's and supplier's Unipass IDs should be the same, but they are different. The supplier has '레디코리아123456' and the declarant has '012345678901234'.You can see the supplier's Unipass ID (type '06') if you press F3 and go to Config > Registration Numbers/Codes on the Organization form. You can see the declarant's Unipass ID in the Organization Proxy of the branch(if there is a Customs Address of Record) or the Company profile.");

				Declaration.JE_OA_SupplierAddress = declarant.MainAddress.PK;
				AssertEquals("012345678901234", Declaration.BrokerAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization));
				AssertEquals("012345678901234", Declaration.SupplierAddress.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization));
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "For the self - declaring suppliers, the declarant's and supplier's Unipass IDs should be the same, but they are different. The supplier has '레디코리아123456' and the declarant has '012345678901234'.You can see the supplier's Unipass ID (type '06') if you press F3 and go to Config > Registration Numbers/Codes on the Organization form. You can see the declarant's Unipass ID in the Organization Proxy of the branch(if there is a Customs Address of Record) or the Company profile.");
			});

			CombineAssertions("If ExporterType is A or C, and Exporter and Supplier have different Unipass IDs", () =>
			{
				var differentSupplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KOREAREADY", "(주)코리아레디");
				var differentSupplierCodes = new IDNumberAndType[]
				{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "코리아레디123456" }
				};
				TestOrgDataSetUpHelper.AddCustomsCode(differentSupplier, differentSupplierCodes);
				Declaration.JE_OA_SupplierAddress = differentSupplier.MainAddress.PK;

				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.A;
				Declaration.Validation.ValidateJE_OA_SupplierAddress();
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "If exporter type is 'A' or 'C', the exporter company name and Unipass ID must be the same as the supplier company name and Unipass ID. Please press F3 here and edit Details > Full Name or Config > Registration Numbers/Codes type of '06' on the Organization form.");
				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.C;
				Declaration.Validation.ValidateJE_OA_SupplierAddress();
				AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "If exporter type is 'A' or 'C', the exporter company name and Unipass ID must be the same as the supplier company name and Unipass ID. Please press F3 here and edit Details > Full Name or Config > Registration Numbers/Codes type of '06' on the Organization form.");

				Declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.A;
				Declaration.Validation.ValidateJE_OA_SupplierAddress();
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "If exporter type is 'A' or 'C', the exporter company name and Unipass ID must be the same as the supplier company name and Unipass ID. Please press F3 here and edit Details > Full Name or Config > Registration Numbers/Codes type of '06' on the Organization form.");
				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.C;
				Declaration.Validation.ValidateJE_OA_SupplierAddress();
				AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "If exporter type is 'A' or 'C', the exporter company name and Unipass ID must be the same as the supplier company name and Unipass ID. Please press F3 here and edit Details > Full Name or Config > Registration Numbers/Codes type of '06' on the Organization form.");
			});

			Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.B;
			Declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
			Declaration.JE_OA_SupplierAddress = seller.MainAddress.PK;
			AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "You have indicated that the exporter type is 'B - Agent'. However the supplier and the exporter agent are set to the same organization. Please check.");

			Declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			AssertNoMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "You have indicated that the exporter type is 'B - Agent'. However the supplier and the exporter agent are set to the same organization. Please check.");

			var exporter = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "TestCompanyName");
			var unipassCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "가나다라1234001", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(exporter, unipassCode);

			Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.D;
			Declaration.JE_OA_SellerAddress = exporter.MainAddress.PK;
			Declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;

			AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "If the Export Type is 'D', the Supplier and Exporter must have the same Unipass ID and company name.");

			supplier.OH_FullName = "TestCompanyName";
			Declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertHasMessageErrorContaining(Declaration.JE_OA_SupplierAddressInfo, "If the Export Type is 'D', the Supplier and Exporter must have the same Unipass ID and company name.");

			TestOrgDataSetUpHelper.AddCustomsCode(supplier, unipassCode);
			Declaration.Validation.ValidateJE_OA_SupplierAddress();
			AssertNoMessageErrors(Declaration.JE_OH_SupplierInfo);
		}

		public void DeclarationCustomsDivision()
		{
			CombineAssertions("Check DeclarationCustomsDivision", () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				Factory.Save();

				Declaration.JE_ProcedureType = "E";
				Declaration.JE_CustomsOffice = ZString.Empty;
				Declaration.Validation.ValidateJE_CustomsDivision();
				AssertNoMessageErrors(Declaration.JE_CustomsDivisionInfo);

				Declaration.JE_CustomsOffice = "010";
				Declaration.Validation.ValidateJE_CustomsDivision();
				AssertHasMessageErrorContaining(Declaration.JE_CustomsDivisionInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_ProcedureType = "H";
				Declaration.Validation.ValidateJE_CustomsDivision();
				AssertHasMessageErrorContaining(Declaration.JE_CustomsDivisionInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_CustomsDivision = "X";
				AssertHasMessageError(Declaration.JE_CustomsDivisionInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_CustomsDivision = "20";
				AssertNoMessageErrors(Declaration.JE_CustomsDivisionInfo);
			});
		}

		public void TestExporterType()
		{
			CombineAssertions("Check ExporterType", () =>
			{
				Declaration.Validation.ValidateJE_ExporterType();
				AssertHasMessageErrorContaining(Declaration.JE_ExporterTypeInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_ExporterType = "1";
				AssertHasMessageErrorContaining(Declaration.JE_ExporterTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.A;
				AssertNoMessageErrors(Declaration.JE_ExporterTypeInfo);

				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.B;
				AssertNoMessageErrors(Declaration.JE_ExporterTypeInfo);

				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.C;
				AssertNoMessageErrors(Declaration.JE_ExporterTypeInfo);

				Declaration.JE_ExporterType = ExporterTypeCodeList.Codes.D;
				AssertNoMessageErrors(Declaration.JE_ExporterTypeInfo);
			});
		}

		public void TestReturnReason()
		{
			CombineAssertions("Check ReturnReason", () =>
			{
				Declaration.JE_ProcedureType = "H";
				Declaration.Validation.ValidateJE_ReturnReason();
				AssertNoMessageErrors(Declaration.JE_ReturnReasonInfo);

				Declaration.JE_ReturnReason = "10";
				AssertHasMessageErrorContaining(Declaration.JE_ReturnReasonInfo, MandatoryValidation.DoNotEntered);

				Declaration.JE_ProcedureType = "M";
				Declaration.JE_ReturnReason = "10";
				AssertHasMessageErrorContaining(Declaration.JE_ReturnReasonInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_ReturnReason = ReturnReasonCodeList.Codes._11;
				AssertNoMessageErrors(Declaration.JE_ReturnReasonInfo);

				Declaration.JE_ReturnReason = "";
				AssertHasMessageErrorContaining(Declaration.JE_ReturnReasonInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestReturnType()
		{
			CombineAssertions("Check ReturnType", () =>
			{
				Declaration.JE_ProcedureType = "H";
				Declaration.Validation.ValidateJE_ReturnType();
				AssertNoMessageErrors(Declaration.JE_ReturnTypeInfo);

				Declaration.JE_ReturnType = "1";
				AssertHasMessageErrorContaining(Declaration.JE_ReturnTypeInfo, MandatoryValidation.DoNotEntered);

				Declaration.JE_ProcedureType = "M";
				Declaration.JE_ReturnType = "1";
				AssertHasMessageErrorContaining(Declaration.JE_ReturnTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_ReturnType = ReturnTypeCodeList.Codes.A;
				AssertNoMessageErrors(Declaration.JE_ReturnTypeInfo);

				Declaration.JE_ReturnType = "";
				AssertHasMessageErrorContaining(Declaration.JE_ReturnTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestGoodsStatus()
		{
			CombineAssertions("Check GoodsStatus", () =>
			{
				Declaration.JE_ProcedureType = "E";
				Declaration.Validation.ValidateJE_GoodsCondition();
				AssertNoMessageErrors(Declaration.JE_GoodsConditionInfo);

				Declaration.JE_ProcedureType = "H";
				Declaration.Validation.ValidateJE_GoodsCondition();
				AssertHasMessageErrorContaining(Declaration.JE_GoodsConditionInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_GoodsCondition = "1";
				AssertHasMessageErrorContaining(Declaration.JE_GoodsConditionInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_GoodsCondition = GoodsStatusCodeList.Codes.New;
				AssertNoMessageErrors(Declaration.JE_GoodsConditionInfo);
			});
		}

		public void TestApplicationForSimpleDrawback()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0202201000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0202301000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0303230000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var condtionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", RefCusConditionType.SimpleDrawback);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.KoreaSouth, condtionType.PK, tariff1.PK, "Valid test value", false, true, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.KoreaSouth, condtionType.PK, tariff2.PK, "Invalid test value", false, true, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			Factory.Save();

			Declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertHasMessageErrorContaining(Declaration.JE_SimpleDRWAppInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_SimpleDRWApp = "12";
			AssertHasMessageErrorContaining(Declaration.JE_SimpleDRWAppInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.NO;
			AssertNoMessageErrors(Declaration.JE_SimpleDRWAppInfo);

			Declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.AD;
			AssertNoMessageErrors(Declaration.JE_SimpleDRWAppInfo);

			var invoice = Declaration.Invoices.AddNew();
			var firtstInvoiceLine = invoice.InvoiceLines.AddNew();
			firtstInvoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
			Declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertHasMessageErrorContaining(Declaration.JE_SimpleDRWAppInfo, "When a simple drawback is applied, there must be at least one HS code which is eligible for the process, but there are currently none.");

			firtstInvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			Declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertHasMessageErrorContaining(Declaration.JE_SimpleDRWAppInfo, "When a simple drawback is applied, there must be at least one HS code which is eligible for the process, but there are currently none.");

			firtstInvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			Declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertNoMessageErrors(Declaration.JE_SimpleDRWAppInfo);

			var secondInvoiceLine = invoice.InvoiceLines.AddNew();
			secondInvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			Declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertNoMessageErrors(Declaration.JE_SimpleDRWAppInfo);

			var thirdInvoiceLine = invoice.InvoiceLines.AddNew();
			thirdInvoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
			Declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertNoMessageErrors(Declaration.JE_SimpleDRWAppInfo);

			Factory.Save();
			var factory = new BusinessObjectFactory();
			var declaration = factory.Load<JobDeclaration>(Declaration.PK);
			declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertNoMessageErrors(declaration.JE_SimpleDRWAppInfo);

			declaration.InvoiceLines[0].Delete();
			declaration.Validation.ValidateJE_SimpleDRWApp();
			AssertHasMessageErrorContaining(declaration.JE_SimpleDRWAppInfo, "When a simple drawback is applied, there must be at least one HS code which is eligible for the process, but there are currently none.");

			factory.Save();
			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			declaration2.Validation.ValidateJE_SimpleDRWApp();
			AssertHasMessageErrorContaining(declaration2.JE_SimpleDRWAppInfo, "When a simple drawback is applied, there must be at least one HS code which is eligible for the process, but there are currently none.");
		}

		public void TestSouthNorthTradeYN()
		{
			CombineAssertions("Check SouthNorthTradeYN", () =>
			{
				Declaration.Validation.ValidateJE_TradeIndicatorWithKP();
				AssertNoMessageErrors(Declaration.JE_TradeIndicatorWithKPInfo);

				Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.KoreaNorth;
				Declaration.Validation.ValidateJE_TradeIndicatorWithKP();
				AssertHasMessageErrorContaining(Declaration.JE_TradeIndicatorWithKPInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_TradeIndicatorWithKP = "1";
				AssertHasMessageErrorContaining(Declaration.JE_TradeIndicatorWithKPInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.A;
				AssertNoMessageErrors(Declaration.JE_TradeIndicatorWithKPInfo);
			});
		}

		public void TestJE_TradeIDWithKP()
		{
			Declaration.Validation.ValidateJE_TradeIDWithKP();
			AssertNoMessageErrors(Declaration.JE_TradeIDWithKPInfo);

			Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.KoreaNorth;
			Declaration.Validation.ValidateJE_TradeIDWithKP();
			AssertHasMessageErrorContaining(Declaration.JE_TradeIDWithKPInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_TradeIDWithKP = "XX";
			AssertHasMessageErrorContaining(Declaration.JE_TradeIDWithKPInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_TradeIDWithKP = SouthNorthTradeIdentificationCodeList.Codes.GS;
			AssertNoMessageErrors(Declaration.JE_TradeIDWithKPInfo);
		}

		public void TestCheckJE_ContainerPackMode()
		{
			Declaration.JE_ProcedureType = "E";
			Declaration.JE_ContainerPackMode = ZString.Empty;
			AssertNoMessageErrors(Declaration.JE_ContainerPackModeInfo);

			Declaration.JE_ProcedureType = "H";
			Declaration.JE_ContainerPackMode = ZString.Empty;
			AssertHasMessageErrorContaining(Declaration.JE_ContainerPackModeInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_ContainerPackMode = "123";
			AssertHasMessageErrorContaining(Declaration.JE_ContainerPackModeInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.BU;
			AssertNoMessageErrors(Declaration.JE_ContainerPackModeInfo);
		}

		public void TestJE_ProcedureType()
		{
			Declaration.JE_ProcedureType = ZString.Empty;
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_ProcedureType = "Z";
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			AssertNoMessageErrors(Declaration.JE_ProcedureTypeInfo);

			Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._78;
			Declaration.JE_ProcedureType = ZString.Empty;
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, "If Transaction Type Code is '78' or '79' then, Declaration Procedure Type must be 'M'.");
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			AssertNoMessageErrors(Declaration.JE_ProcedureTypeInfo);

			Declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._79;
			Declaration.JE_ProcedureType = "";
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, "If Transaction Type Code is '78' or '79' then, Declaration Procedure Type must be 'M'.");
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			AssertNoMessageErrors(Declaration.JE_ProcedureTypeInfo);

			Declaration.JE_ExportGoodsType = "";
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			AssertNoMessageErrors(Declaration.JE_ProcedureTypeInfo);

			Declaration.JE_MessageSubType = ExportTypeCodeList.Codes.F;
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, EXPJobDeclarationValidation.ExportTypeCheckDeclarationTypeError);

			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.L;
			AssertNoMessageErrors(Declaration.JE_ProcedureTypeInfo);

			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			AssertHasMessageErrorContaining(Declaration.JE_ProcedureTypeInfo, EXPJobDeclarationValidation.ExportTypeCheckDeclarationTypeError);
		}

		public void TestJE_ContainerPackMode()
		{
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			Declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.BU;
			AssertHasMessageErrorContaining(Declaration.JE_ContainerPackModeInfo, "If 'Declaration Type' is 'B' then 'Container Pack' must be 'FC' or 'LC'.");

			Declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.FC;
			AssertNoMessageErrorContaining(Declaration.JE_ContainerPackModeInfo, "If 'Declaration Type' is 'B' then 'Container Pack' must be 'FC' or 'LC'.");

			Declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.LC;
			AssertNoMessageErrorContaining(Declaration.JE_ContainerPackModeInfo, "If 'Declaration Type' is 'B' then 'Container Pack' must be 'FC' or 'LC'.");

			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			Declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.RO;
			AssertNoMessageErrors(Declaration.JE_ContainerPackModeInfo);

			Declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.FC;
			AssertNoMessageErrors(Declaration.JE_ContainerPackModeInfo);

			Declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.BU;
			AssertNoMessageErrors(Declaration.JE_ContainerPackModeInfo);
		}

		public void TestJE_LocationIDInBondedArea()
		{
			var validation = Declaration.Validation;

			Declaration.JE_ProcedureType = "E";
			validation.ValidateJE_LocationIDInBondedArea();
			AssertNoMessageErrors(Declaration.JE_LocationIDInBondedAreaInfo);

			Declaration.JE_ProcedureType = "B";
			validation.ValidateJE_LocationIDInBondedArea();
			AssertHasMessageErrorContaining(Declaration.JE_LocationIDInBondedAreaInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_ProcedureType = "B";
			Declaration.JE_LocationIDInBondedArea = "aaaa";
			AssertNoMessageErrors(Declaration.JE_LocationIDInBondedAreaInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
		}

		JobDeclaration Declaration => (JobDeclaration)declaration;
	}
}
