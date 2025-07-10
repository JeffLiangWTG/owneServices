using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LEXJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestJE_MessageSubType()
		{
			Declaration.JE_MessageSubType = ZString.Empty;
			AssertHasErrorContaining(Declaration.JE_MessageSubTypeInfo, MandatoryValidation.MustBeEntered);

			Declaration.JE_MessageSubType = "X";
			AssertHasErrorContaining(Declaration.JE_MessageSubTypeInfo, ListValidation.InvalidCodeError);

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._04;
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");

			Declaration.TransportMeans.AddNew();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._04;
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");
		}

		public void TestGoodsLocationAdditionalDetails()
		{
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			Declaration.Validation.ValidateJE_SubLocationOfGoods();
			AssertNoMessageErrors(Declaration.JE_SubLocationOfGoodsInfo);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			Declaration.Validation.ValidateJE_SubLocationOfGoods();
			AssertHasMessageErrorContaining(Declaration.JE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_SubLocationOfGoods = "장치장소명";
			AssertNoMessageErrors(Declaration.JE_SubLocationOfGoodsInfo);
		}

		public void TestGoodsLocationBondedAreaCode()
		{
			CombineAssertions("Check GoodsLocationBondedAreaCode", () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "11111111", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01011223", "이사화물장치장(서울)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				Factory.Save();

				Declaration.Validation.ValidateJE_LocationOtherInformation();
				AssertNoMessageErrors(Declaration.JE_LocationOtherInformationInfo);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				Declaration.JE_LocationOtherInformation = "11111111";
				AssertHasMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, MandatoryValidation.DoNotEntered);

				Declaration.JE_LocationOtherInformation = ZString.Empty;
				AssertNoMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, MandatoryValidation.DoNotEntered);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
				Declaration.Validation.ValidateJE_LocationOtherInformation();
				AssertHasMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_LocationOtherInformation = "XXX";
				AssertHasMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_CustomsOffice = "011";
				Declaration.JE_LocationOtherInformation = "01011223";
				AssertHasMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, "Bonded Area Code's Customs Office and Declaration Customs Office must be the same.");

				Declaration.JE_CustomsOffice = "010";
				Declaration.Validation.ValidateJE_LocationOtherInformation();
				AssertNoMessageErrors(Declaration.JE_LocationOtherInformationInfo);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
				Declaration.Validation.ValidateJE_LocationOtherInformation();
				AssertNoMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, MandatoryValidation.DoNotEntered);
			});
		}

		public void TestDeclarationType()
		{
			CombineAssertions("Check DeclarationType", () =>
			{
				Declaration.Validation.ValidateJE_MessageSubType();
				AssertHasErrorContaining(Declaration.JE_MessageSubTypeInfo, MandatoryValidation.MustBeEntered);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
				AssertNoErrors(Declaration.JE_MessageSubTypeInfo);
				Declaration.JE_MessageSubType = "A";
				AssertHasErrorContaining(Declaration.JE_MessageSubTypeInfo, ListValidation.InvalidCodeError);
				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;
				AssertNoErrors(Declaration.JE_MessageSubTypeInfo);
			});
		}

		public void TestDeclarationCustomsOffice()
		{
			CombineAssertions("Check DeclarationCustomsOffice", () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				Factory.Save();

				Declaration.Validation.ValidateJE_CustomsOffice();
				AssertHasMessageErrorContaining(Declaration.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_CustomsOffice = "X";
				AssertHasMessageErrorContaining(Declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_CustomsOffice = "010";
				AssertNoMessageErrors(Declaration.JE_CustomsOfficeInfo);
			});
		}

		public void TestDeclarationCustomsDivision()
		{
			CombineAssertions("Check DeclarationCustomsDivision", () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				Factory.Save();

				Declaration.Validation.ValidateJE_CustomsDivision();
				AssertHasMessageErrorContaining(Declaration.JE_CustomsDivisionInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_CustomsDivision = "X";
				AssertHasMessageError(Declaration.JE_CustomsDivisionInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_CustomsDivision = "20";
				AssertNoMessageErrors(Declaration.JE_CustomsDivisionInfo);
			});
		}

		public void TestDeclarationDate()
		{
			CombineAssertions("Check DeclarationDate", () =>
			{
				Declaration.Validation.ValidateJE_EntryDate();
				AssertNoMessageErrors(Declaration.JE_EntryDateInfo);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
				Declaration.JE_EntryDate = ZDate.Today;
				AssertHasMessageErrorContaining(Declaration.JE_EntryDateInfo, MandatoryValidation.DoNotEntered);
				Declaration.JE_IsBlanketDeclaration = "Y";
				Declaration.Validation.ValidateJE_EntryDate();
				AssertNoMessageErrors(Declaration.JE_EntryDateInfo);
				Declaration.JE_EntryDate = ZDate.Empty;
				AssertHasMessageErrorContaining(Declaration.JE_EntryDateInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.JE_EntryDate = ZDate.Today;
				AssertNoMessageErrors(Declaration.JE_EntryDateInfo);

				Declaration.JE_EntryDate = ZDate.Empty;
				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
				Declaration.Validation.ValidateJE_EntryDate();
				AssertHasMessageErrorContaining(Declaration.JE_EntryDateInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.JE_EntryDate = ZDate.Today;
				AssertNoMessageErrors(Declaration.JE_EntryDateInfo);
			});
		}

		public void TestJE_MessageSubTypeStevedoreAndOtherTransportMean()
		{
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "Stevedores must be entered if Declaration Type is '07', '09' or '17'.");
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is '07',’09’ or '17', ‘Other Transport Mean’ must be entered.");
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "Stevedores must be entered if Declaration Type is '07', '09' or '17'.");
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is '07',’09’ or '17', ‘Other Transport Mean’ must be entered.");
			Declaration.Persons.AddNew();
			Declaration.TransportMeans.AddNew();
			Declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "Stevedores must be entered if Declaration Type is '07', '09' or '17'.");
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is '07',’09’ or '17', ‘Other Transport Mean’ must be entered.");

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "Stevedores must not be entered if Declaration Type is not '07', '09' or '17'.");
			AssertHasMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "Stevedores must not be entered if Declaration Type is not '07', '09' or '17'.");
			AssertNoMessageErrorContaining(Declaration.JE_MessageSubTypeInfo, "If Declaration Type is not '07',’09’ or '17', ‘Other Transport Mean’ must not be entered.");
		}

		public void TestFlightNoOrVesselName()
		{
			CombineAssertions("Check JE_VesselName and JE_VoyageFlightNo", () =>
			{
				var vessel = RefVessel.New(Factory);
				vessel.RV_Code = "KI1098";
				vessel.RV_RadioCallSign = "5VDP9";

				Declaration.Validation.ValidateJE_VesselName();
				Declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageErrors(Declaration.JE_VesselNameInfo);
				AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				Declaration.Validation.ValidateJE_VesselName();
				Declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertHasMessageErrorContaining(Declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
				Declaration.JE_VesselName = "KI1098";
				Declaration.JE_VoyageFlightNo = "";
				AssertNoMessageErrors(Declaration.JE_VesselNameInfo);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
				Declaration.Validation.ValidateJE_VesselName();
				Declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageErrors(Declaration.JE_VesselNameInfo);
				AssertHasMessageErrorContaining(Declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.JE_VoyageFlightNo = "123456";
				Declaration.JE_VesselName = "";
				AssertNoMessageErrorContaining(Declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
				Declaration.Validation.ValidateJE_VesselName();
				Declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertHasMessageErrorContaining(Declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
				Declaration.JE_VesselName = "KI1098";
				Declaration.JE_VoyageFlightNo = "";
				AssertNoMessageErrors(Declaration.JE_VesselNameInfo);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._18;
				Declaration.Validation.ValidateJE_VesselName();
				Declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageErrors(Declaration.JE_VesselNameInfo);
				AssertHasMessageErrorContaining(Declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.JE_VoyageFlightNo = "123456";
				Declaration.JE_VesselName = "";
				AssertNoMessageErrorContaining(Declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
				Declaration.Validation.ValidateJE_VesselName();
				Declaration.Validation.ValidateJE_VoyageFlightNo();
				AssertHasMessageErrorContaining(Declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrors(Declaration.JE_VoyageFlightNoInfo);
				Declaration.JE_VesselName = "KI1098";
				Declaration.JE_VoyageFlightNo = "";
				AssertNoMessageErrors(Declaration.JE_VesselNameInfo);
			});
		}

		public void TestGoodsType()
		{
			CombineAssertions("Check JE_ExportGoodsType", () =>
			{
				Declaration.Validation.ValidateJE_ExportGoodsType();
				AssertHasMessageErrorContaining(Declaration.JE_ExportGoodsTypeInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.JE_ExportGoodsType = "A";
				AssertHasMessageErrorContaining(Declaration.JE_ExportGoodsTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.JE_ExportGoodsType = "1";
				AssertNoMessageErrors(Declaration.JE_ExportGoodsTypeInfo);
			});
		}

		public void TestVesselRadioCallSign()
		{
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			Declaration.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining(Declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertEquals(Declaration.Vessel, null);
			Declaration.JE_VesselName = "KI1098";
			AssertHasMessageErrorContaining(Declaration.JE_VesselNameInfo, ListValidation.InvalidCodeMessageError);
			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = Declaration.JE_VesselName;
			Declaration.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining(Declaration.JE_VesselNameInfo, "Radio call sign must be entered on the selected vessel.");
			vessel.RV_RadioCallSign = "5VDP9";
			Declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrors(Declaration.JE_VesselNameInfo);
		}

		public void TestMRNNo()
		{
			CombineAssertions("Check J3_ReferenceNumber", () =>
			{
				((JobDeclarationValidation)Declaration.Validation).ValidateMRNNo();
				AssertNoMessageErrors(Declaration.MRNJ3_ReferenceNumberInfo);
				Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
				Declaration.MRNJ3_ReferenceNumber = "";
				AssertHasMessageErrorContaining(Declaration.MRNJ3_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.MRNJ3_ReferenceNumber = "MRN";
				AssertNoMessageErrors(Declaration.MRNJ3_ReferenceNumberInfo);
			});
		}

		public override void TestJE_ShipmentIncoTerm()
		{
			Declaration.JE_ShipmentIncoTerm = "XXX";
			AssertHasMessageErrorContaining(Declaration.JE_ShipmentIncoTermInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_ShipmentIncoTerm = LocalExportIncotermList.Codes.FreeAlongsideShip;
			AssertNoMessageErrors(Declaration.JE_ShipmentIncoTermInfo);
		}

		public void TestStevedoreCompanyAddress()
		{
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			var cusPerson = Declaration.Persons.AddNew();
			var glbPerson = Factory.New<GlbPerson>();
			glbPerson.PER_FullName = "Kenny G";
			cusPerson.CPN_PER_Person = glbPerson.PK;

			Declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(Declaration.StevedoreCompanyAddressInfo, "Stevedore must be entered.");
			Declaration.StevedoreCompanyAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertNoMessageErrors(Declaration.StevedoreCompanyAddressInfo);
		}

		public void TestJE_OH_Exporter()
		{
			var exporter = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA1", "레디코리아1");
			exporter.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1234567890", Core.Constants.CountryCodes.KoreaSouth);
			var emptyExporter = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA2", "레디코리아2");

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			AssertMessageTypeIs5DP();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;
			AssertMessageTypeIs5DP();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._03;
			AssertMessageTypeIs5DP();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._04;
			AssertMessageTypeIs5DP();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;
			AssertMessageTypeIs5DP();

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertMessageTypeIs5DQ();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertMessageTypeIs5DQ();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			AssertMessageTypeIs5DQ();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
			AssertMessageTypeIs5DQ();
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._18;
			AssertMessageTypeIs5DQ();

			void AssertMessageTypeIs5DP()
			{
				Declaration.JE_OH_Exporter = ZGuid.Empty;
				AssertNoMessageErrors(Declaration.JE_OH_ExporterInfo);
				Declaration.JE_OH_Exporter = exporter.PK;
				AssertHasMessageErrorContaining(Declaration.JE_OH_ExporterInfo, MandatoryValidation.DoNotEntered);
			}
			void AssertMessageTypeIs5DQ()
			{
				Declaration.JE_OH_Exporter = ZGuid.Empty;
				AssertHasMessageErrorContaining(Declaration.JE_OH_ExporterInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.JE_OH_Exporter = emptyExporter.PK;
				AssertHasMessageErrorContaining(Declaration.JE_OH_ExporterInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
				Declaration.JE_OH_Exporter = exporter.PK;
				AssertNoMessageErrors(Declaration.JE_OH_ExporterInfo);
			}
		}

		public void TestJE_OH_Supplier()
		{
			var wrongSupplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "RK1", "");
			var emptySupplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "");
			var supplierCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1234567890" },
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "123456789012345" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);

			Declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageErrorContaining(Declaration.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.JE_OH_Supplier = wrongSupplier.PK;
			AssertNoMessageErrorContaining(Declaration.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Declaration.JE_OH_SupplierInfo, "The Supplier must not be an individual.");
			AssertNoMessageErrorContaining(Declaration.JE_OH_SupplierInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
			AssertNoMessageErrorContaining(Declaration.JE_OH_SupplierInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

			Declaration.JE_OH_Supplier = emptySupplier.PK;
			AssertNoMessageErrorContaining(Declaration.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Declaration.JE_OH_SupplierInfo, "The Supplier must not be an individual.");
			AssertHasMessageErrorContaining(Declaration.JE_OH_SupplierInfo, "There is no Business Registration Number for this organization. Please press F3 here and add a number of type 'GBR' in Config > Registration Numbers/Codes on the Organization form.");
			AssertHasMessageErrorContaining(Declaration.JE_OH_SupplierInfo, "There is no Unipass ID for this organization. Please press F3 here and add a number of type '06' in Config > Registration Numbers/Codes on the Organization form.");

			Declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrors(Declaration.JE_OH_SupplierInfo);
		}

		public void TestCrewCount()
		{
			Declaration.Validation.ValidateJE_NoOfCrew();
			AssertNoMessageErrors(Declaration.JE_NoOfCrewInfo);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			Declaration.Validation.ValidateJE_NoOfCrew();
			AssertHasMessageErrorContaining(Declaration.JE_NoOfCrewInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_NoOfCrew = 15;
			AssertNoMessageErrors(Declaration.JE_NoOfCrewInfo);

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertNoMessageErrors(Declaration.JE_NoOfCrewInfo);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			Declaration.JE_NoOfCrew = ZInt.Zero;
			AssertHasMessageErrorContaining(Declaration.JE_NoOfCrewInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_NoOfCrew = 20;
			AssertNoMessageErrors(Declaration.JE_NoOfCrewInfo);

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;
			AssertNoMessageErrors(Declaration.JE_NoOfCrewInfo);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
			Declaration.JE_NoOfCrew = ZInt.Zero;
			AssertHasMessageErrorContaining(Declaration.JE_NoOfCrewInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_NoOfCrew = 20;
			AssertNoMessageErrors(Declaration.JE_NoOfCrewInfo);
		}

		public void TestScheduledSailingDays()
		{
			Declaration.Validation.ValidateJE_VoyageDuration();
			AssertNoMessageErrors(Declaration.JE_VoyageDurationInfo);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			Declaration.Validation.ValidateJE_VoyageDuration();
			AssertHasMessageErrorContaining(Declaration.JE_VoyageDurationInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_VoyageDuration = 24;
			AssertNoMessageErrors(Declaration.JE_VoyageDurationInfo);

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertNoMessageErrors(Declaration.JE_VoyageDurationInfo);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			Declaration.JE_VoyageDuration = ZInt.Zero;
			AssertHasMessageErrorContaining(Declaration.JE_VoyageDurationInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_VoyageDuration = 12;
			AssertNoMessageErrors(Declaration.JE_VoyageDurationInfo);

			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;
			AssertNoMessageErrors(Declaration.JE_VoyageDurationInfo);
			Declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
			Declaration.JE_VoyageDuration = ZInt.Zero;
			AssertHasMessageErrorContaining(Declaration.JE_VoyageDurationInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.JE_VoyageDuration = 8;
			AssertNoMessageErrors(Declaration.JE_VoyageDurationInfo);
		}

		public void TestMRNType()
		{
			Declaration.Validation.ValidateJE_MRNType();
			AssertNoMessageErrors(Declaration.JE_MRNTypeInfo);
			Declaration.JE_MRNType = "9";
			AssertHasMessageErrorContaining(Declaration.JE_MRNTypeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;
			AssertNoMessageErrors(Declaration.JE_MRNTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			Declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
		}

		JobDeclaration Declaration => (JobDeclaration)declaration;
	}
}
