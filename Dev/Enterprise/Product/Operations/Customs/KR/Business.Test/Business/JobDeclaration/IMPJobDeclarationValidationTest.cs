using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class IMPJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestDeclarationCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsOffice = "X";
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsOffice = "010";
			AssertNoMessageErrors(declaration.JE_CustomsOfficeInfo);
		}

		public void TestDeclarationCustomsDivision()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_CustomsDivision();
			AssertHasMessageErrorContaining(declaration.JE_CustomsDivisionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsDivision = "X";
			AssertHasMessageErrorContaining(declaration.JE_CustomsDivisionInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsDivision = "20";
			AssertNoMessageErrors(declaration.JE_CustomsDivisionInfo);
		}

		public void TestArrivalDateAtDischargePort()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_DateOfArrival();
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarationPlan = "D";
			declaration.JE_DateOfArrival = ZDate.Today.AddDays(-1);
			AssertNoMessageErrors(declaration.JE_DateOfArrivalInfo);

			declaration.JE_DeclarationPlan = "A";
			declaration.Validation.ValidateJE_DateOfArrival();
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, "Arrival Date at Discharge Port must be greater than or equal to Declaration Date for Declaration Plan A or B");
		}

		public void TestUnderbondMovementArrivalDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			declaration.JE_DeclarationPlan = "A";
			var validation = (IMPJobDeclarationValidation)declaration.Validation;
			validation.ValidateUnderbondMovementArrivalDate();
			AssertNoMessageErrors(declaration.UnderbondMovementArrivalDateInfo);

			declaration.JE_DeclarationPlan = "D";
			validation.ValidateUnderbondMovementArrivalDate();
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, "Underbond Movement Arrival Date is mandatory for the Declaration Plan Code D or F");

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrors(declaration.UnderbondMovementArrivalDateInfo);

			declaration.JE_DateOfArrival = ZDate.Empty;
			validation.ValidateUnderbondMovementArrivalDate();
			AssertNoMessageErrors(declaration.UnderbondMovementArrivalDateInfo);

			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, "Underbond Movement Arrival Date must be less than or equal to Declaration Date");

			declaration.JE_DateOfArrival = ZDate.Today.AddDays(5);
			declaration.UnderbondMovementArrivalDate = ZDateTime.Today.AddDays(3);
			AssertHasMessageErrorContaining(declaration.UnderbondMovementArrivalDateInfo, "Underbond Movement Arrival Date must be greater than or equal to Arrival Date At Discharge Port");
		}

		public override void TestJE_LocationOtherInformation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "11111111", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.JE_LocationOtherInformation = "XXX";
			AssertHasMessageErrorContaining(declaration.JE_LocationOtherInformationInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_LocationOtherInformation = "11111111";
			AssertHasMessageErrorContaining(declaration.JE_LocationOtherInformationInfo, "The first three characters of the bonded warehouse code is not equal to the Customs office code.");
		}

		public void TestCarrierID()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.KoreaSouth;
			carrier1.ZZ4_Code = "ABC";
			carrier1.ZZ4_IsAir = true;

			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.KoreaSouth;
			carrier2.ZZ4_Code = "DEF";
			carrier2.ZZ4_IsSea = true;

			declaration.Validation.ValidateJE_CarrierCode();
			AssertNoMessageErrors(declaration.JE_CarrierCodeInfo);

			declaration.JE_CarrierCode = carrier1.ZZ4_Code;
			AssertNoMessageErrors(declaration.JE_CarrierCodeInfo);

			declaration.JE_CarrierCode = carrier2.ZZ4_Code;
			AssertNoMessageErrors(declaration.JE_CarrierCodeInfo);

			declaration.JE_DeclarationPlan = "A";
			declaration.Invoices[0].JZ_ImportCargoManagementNumber = "NO";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_CarrierCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CarrierCode = carrier1.ZZ4_Code;
			AssertHasMessageErrorContaining(declaration.JE_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CarrierCode = carrier2.ZZ4_Code;
			AssertNoMessageErrors(declaration.JE_CarrierCodeInfo);

			declaration.JE_DeclarationPlan = "B";
			declaration.JE_CarrierCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CarrierCode = carrier1.ZZ4_Code;
			AssertHasMessageErrorContaining(declaration.JE_CarrierCodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CarrierCode = carrier2.ZZ4_Code;
			AssertNoMessageErrors(declaration.JE_CarrierCodeInfo);

			declaration.Invoices.AddNew().JZ_ImportCargoManagementNumber = "XX";
			declaration.JE_CarrierCode = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestBondedAreaCode()
		{
			declaration.Validation.ValidateJE_LocationOtherInformation();
			AssertHasMessageErrorContaining(declaration.JE_LocationOtherInformationInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsOffice = "030";
			declaration.JE_LocationOtherInformation = "01012345";
			AssertHasMessageErrorContaining(declaration.JE_LocationOtherInformationInfo, "The first three characters of the bonded warehouse code is not equal to the Customs office code. The Customs this declaration is sent under should be identical with the Customs which manages the bonded warehouse.");

			declaration.JE_LocationOtherInformation = "03012345";
			AssertNoMessageErrorContaining(declaration.JE_LocationOtherInformationInfo, "The first three characters of the bonded warehouse code is not equal to the Customs office code. The Customs this declaration is sent under should be identical with the Customs which manages the bonded warehouse.");
		}

		public void TestCourierCompanyID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.ExpressDeliveryServiceIDs, "Express Delivery Service IDs");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ExpressDeliveryServiceIDs, "AD0001", "유나이티드파슬서비스코리아(주)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var emptyOrgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "READYKOREA1");

			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA2");
			var orgHeaderCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = Constants.IdentificationType.CourierCompanyID, Number = "AD0001" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(orgHeader, orgHeaderCodes);

			var wrongOrgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "READYKOREA3");
			var wrongOrgHeaderCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = Constants.IdentificationType.CourierCompanyID, Number = "ABCDEF" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(wrongOrgHeader, wrongOrgHeaderCodes);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var lookups = new JobDeclarationLookups(declaration);
			var expressDeliveryServiceIDsList = lookups.CourierCompanyList;
			expressDeliveryServiceIDsList.Load();
			AssertEquals(1, expressDeliveryServiceIDsList.Count);

			declaration.Validation.ValidateJE_OH_Forwarder();
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);

			declaration.JE_OH_Forwarder = emptyOrgHeader.PK;
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);

			declaration.JE_OH_Forwarder = orgHeader.PK;
			AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);

			declaration.JE_OH_Forwarder = wrongOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, "There is an invalid Courier Company ID for this organization. Please press F3 here and modify a number of type 'SDC' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_MessageSubType = ExportTypeCodeList.Codes.E;
			AssertMessageSubTypeIsEOrPaymentMethodIs18();

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._18;
			AssertMessageSubTypeIsEOrPaymentMethodIs18();

			void AssertMessageSubTypeIsEOrPaymentMethodIs18()
			{
				declaration.JE_OH_Forwarder = ZGuid.Empty;
				AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_OH_Forwarder = emptyOrgHeader.PK;
				AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, "There is no Courier Company ID for this organization. Please press F3 here and add a number of type 'SDC' in Config > Registration Numbers/Codes on the Organization form.");

				declaration.JE_OH_Forwarder = orgHeader.PK;
				AssertNoMessageErrors(declaration.JE_OH_ForwarderInfo);

				declaration.JE_OH_Forwarder = wrongOrgHeader.PK;
				AssertHasMessageErrorContaining(declaration.JE_OH_ForwarderInfo, "There is an invalid Courier Company ID for this organization. Please press F3 here and modify a number of type 'SDC' in Config > Registration Numbers/Codes on the Organization form.");
			}
		}

		public void TestCustomsBrokerCommentCode()
		{
			declaration.CustomsBrokerCommentCode1 = "X";
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode1Info, ListValidation.InvalidCodeMessageError);

			declaration.CustomsBrokerCommentCode1 = "A";
			declaration.CustomsBrokerCommentCode2 = "";
			declaration.CustomsBrokerCommentCode3 = "";
			AssertNoMessageErrors(declaration.CustomsBrokerCommentCode1Info);
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode2Info, IMPJobDeclarationValidation.CustomsBrokerCommentCodeError);
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode3Info, IMPJobDeclarationValidation.CustomsBrokerCommentCodeError);

			declaration.CustomsBrokerCommentCode2 = "X";
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode2Info, ListValidation.InvalidCodeMessageError);

			declaration.CustomsBrokerCommentCode1 = "";
			declaration.CustomsBrokerCommentCode2 = "B";
			declaration.CustomsBrokerCommentCode3 = "";
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode1Info, IMPJobDeclarationValidation.CustomsBrokerCommentCodeError);
			AssertNoMessageErrors(declaration.CustomsBrokerCommentCode2Info);
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode3Info, IMPJobDeclarationValidation.CustomsBrokerCommentCodeError);

			declaration.CustomsBrokerCommentCode3 = "X";
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode3Info, ListValidation.InvalidCodeMessageError);

			declaration.CustomsBrokerCommentCode1 = "";
			declaration.CustomsBrokerCommentCode2 = "";
			declaration.CustomsBrokerCommentCode3 = "C";
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode1Info, IMPJobDeclarationValidation.CustomsBrokerCommentCodeError);
			AssertHasMessageErrorContaining(declaration.CustomsBrokerCommentCode2Info, IMPJobDeclarationValidation.CustomsBrokerCommentCodeError);
			AssertNoMessageErrors(declaration.CustomsBrokerCommentCode3Info);
		}

		public void TestJE_TransportMode()
		{
			declaration.JE_TransportMode = "";
			AssertHasMessageErrorContaining(declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "XX";
			AssertHasErrorContaining(declaration.JE_TransportModeInfo, ListValidation.InvalidCodeError);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoNotifications(declaration.JE_TransportModeInfo);

			declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.FC;
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._16;
			declaration.JE_MessageSubType = "A";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			AssertHasMessageErrorContaining(declaration.JE_TransportModeInfo, IMPJobDeclarationValidation.ContainerYouHaveNotEntered);

			declaration.JE_MessageSubType = "C";
			declaration.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrors(declaration.JE_TransportModeInfo);

			declaration.CusContainers.AddNew();
			declaration.JE_MessageSubType = "F";
			declaration.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrors(declaration.JE_TransportModeInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertHasMessageErrorContaining(declaration.JE_TransportModeInfo, IMPJobDeclarationValidation.ContainerDoNotEntered);

			declaration.CusContainers.DeleteAll();
			declaration.Validation.ValidateJE_TransportMode();
			AssertNoMessageErrors(declaration.JE_TransportModeInfo);
		}

		public void TestJE_TotalNoOfPacksPackType()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_PackQty = 10;
			declaration.Validation.ValidateJE_TotalNoOfPacksPackType();
			AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TotalNoOfPacksPackType = "XX";
			AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.BA;
			AssertNoMessageErrors(declaration.JE_TotalNoOfPacksPackTypeInfo);

			instruction.CEI_PackQty = 0;
			declaration.Validation.ValidateJE_TotalNoOfPacksPackType();
			AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.VG;
			AssertNoMessageErrors(declaration.JE_TotalNoOfPacksPackTypeInfo);
		}

		public void TestJE_RL_NKPortOfArrival()
		{
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			declaration.JE_RL_NKPortOfArrival = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_RL_NKPortOfArrival = "XXPUS";
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfArrivalInfo, "This port code is invalid. Please check against the transport mode and shipment type.");

			declaration.JE_RL_NKPortOfArrival = "KRSEL";
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfArrivalInfo);
		}

		public void TestJE_CustomsLoadPort()
		{
			SetCusMap();

			declaration.JE_CustomsLoadPort = "";
			AssertHasMessageErrorContaining(declaration.JE_CustomsLoadPortInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsLoadPort = "ZZ";
			AssertHasMessageErrorContaining(declaration.JE_CustomsLoadPortInfo, "This port code is invalid. Please check against the transport mode and shipment type.");

			var country = Factory.New<RefCountry>();
			country.RN_Code = "XX";
			Factory.Save();
			declaration.JE_CustomsLoadPort = "XX";
			AssertHasMessageErrorContaining(declaration.JE_CustomsLoadPortInfo, "You entered a code that does not correspond to the departure country KRC code.");

			declaration.JE_CustomsLoadPort = "KR";
			AssertNoMessageErrors(declaration.JE_CustomsLoadPortInfo);
		}

		public void TestJE_VesselName()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._13;
			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrors(declaration.JE_VesselNameInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			declaration.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrors(declaration.JE_VesselNameInfo);
		}

		public void TestJE_VoyageFlightNo()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._13;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrors(declaration.JE_VoyageFlightNoInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrors(declaration.JE_VoyageFlightNoInfo);
		}

		public void TestJE_RN_NKTransportNationality()
		{
			SetCusMap();

			var validation = (IMPJobDeclarationValidation)declaration.Validation;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "ADEAC";

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._13;
			AssertNullOrEmpty(declaration.JE_RN_NKTransportNationality);
			validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageErrors(declaration.JE_RN_NKTransportNationalityInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			AssertNullOrEmpty(declaration.JE_RN_NKTransportNationality);
			validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Andorra;
			AssertNoMessageErrors(declaration.JE_RN_NKTransportNationalityInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "AEPSL";

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._29;
			declaration.JE_RN_NKTransportNationality = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_RN_NKTransportNationalityInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			declaration.JE_RN_NKTransportNationality = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.UnitedArabEmirates;
			AssertNoMessageErrors(declaration.JE_RN_NKTransportNationalityInfo);

			declaration.JE_RN_NKTransportNationality = "XX";
			AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJE_MasterBill()
		{
			declaration.JE_DeclarationPlan = "A";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MasterBill = "MB1";
			AssertNoMessageErrors(declaration.JE_MasterBillInfo);

			declaration.JE_DeclarationPlan = "H";
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_MasterBill = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_MasterBillInfo);
		}

		void SetCusMap()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMapType(Constants.ZZ.RefCusMap.CountryKRCCode, "OUT", "Country Code Mapping", false);
			helper.CreateCusMap(Constants.ZZ.RefCusMap.CountryKRCCode, "ZZ", "Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap(Constants.ZZ.RefCusMap.CountryKRCCode, Core.Constants.CountryCodes.UnitedArabEmirates, "U.A.E", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap(Constants.ZZ.RefCusMap.CountryKRCCode, Core.Constants.CountryCodes.KoreaSouth, "R.KOREA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
		}
		public void TestCheckJE_MessageSubType()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrorContaining(declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = "X";
			AssertHasMessageErrorContaining(declaration.JE_MessageSubTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			AssertNoMessageErrors(declaration.JE_MessageSubTypeInfo);
		}

		public void TestJE_PaymentMethod()
		{
			#region Check Mandatory and List Validation
			declaration.JE_PaymentMethod = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_PaymentMethod = "XX";
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._00;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._11;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._12;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._13;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._14;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._18;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._21;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._22;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._23;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._24;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._43;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._53;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			#endregion

			#region Check When JE_MessageSubType is 'E'
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.E;
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._11;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._13;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._14;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._18;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._43;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._53;
			AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._00;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'.");
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'.");
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._12;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'.");
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._21;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'.");
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._22;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'.");
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._23;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'.");
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._24;
			AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "You have entered E as the import type. A method of payment needs to be '11', '13', '14', '18', '33', '43', '53'.");
			#endregion

			declaration.JE_MessageSubType = ZString.Empty;
			#region Check When JE_ProcedureType in ('12', '14', '18', '20', '27', '30', '31', '33','35','37')
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._14;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._18;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._20;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._27;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType =	 DeclarationProcedureTypeCodeList.Codes._30;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._31;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._33;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._35;
			AssertJE_PaymentMethodMustBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._37;
			AssertJE_PaymentMethodMustBe00();
			#endregion

			#region Check When JE_ProcedureType in ('13', '15', '16', '17', '21', '26', '28', '36')
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._13;
			AssertJE_PaymentMethodMustNotBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._15;
			AssertJE_PaymentMethodMustNotBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._16;
			AssertJE_PaymentMethodMustNotBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._17;
			AssertJE_PaymentMethodMustNotBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._21;
			AssertJE_PaymentMethodMustNotBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._26;
			AssertJE_PaymentMethodMustNotBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._28;
			AssertJE_PaymentMethodMustNotBe00();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._36;
			AssertJE_PaymentMethodMustNotBe00();
			#endregion

			#region Check When JE_ProcedureType in ('22', '23')
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._22;
			AssertJE_PaymentMethodMustBe01();
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._23;
			AssertJE_PaymentMethodMustBe01();
			#endregion

			void AssertJE_PaymentMethodMustBe00()
			{
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._00;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);

				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._11;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._12;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._13;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._14;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._18;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._21;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._22;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._23;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._24;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._43;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._53;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '00' for the selected declaration procedure type.");
			}

			void AssertJE_PaymentMethodMustNotBe00()
			{
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._00;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must not be '00' for the selected declaration procedure type.");

				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._11;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._12;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._13;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._14;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._18;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._21;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._22;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._23;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._24;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._43;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._53;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			}

			void AssertJE_PaymentMethodMustBe01()
			{
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);

				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._00;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._11;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._12;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._13;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._14;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._18;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._21;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._22;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._23;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._24;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._43;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
				declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._53;
				AssertHasMessageErrorContaining(declaration.JE_PaymentMethodInfo, "A method of payment must be '01' for the selected declaration procedure type.");
			}
		}

		public void TestJE_PaidBy()
		{
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			declaration.JE_PaidBy = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_PaidByInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_PaidBy = "X";
			AssertHasMessageErrorContaining(declaration.JE_PaidByInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			AssertJE_PaidByMustBeEntered();
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.B;
			AssertJE_PaidByMustBeEntered();
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.F;
			AssertJE_PaidByMustBeEntered();
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.G;
			AssertJE_PaidByMustBeEntered();
			
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			AssertJE_PaidByMustBeEntered();

			void AssertJE_PaidByMustBeEntered()
			{
				declaration.JE_PaidBy = ZString.Empty;
				AssertHasMessageErrorContaining(declaration.JE_PaidByInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_PaidBy = PaidByCodeList.Codes.CLI;
				AssertNoMessageErrors(declaration.JE_PaidByInfo);
				declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
				AssertNoMessageErrors(declaration.JE_PaidByInfo);
			}
		}

		public void TestBrokerAddress()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			var broker = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "");
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			broker.MainAddress.OA_Email = ZString.Empty;
			broker.MainAddress.OA_Phone = ZString.Empty;
			branch.GB_OH_OrgProxy = broker.PK;
			declaration.JE_GB = branch.PK;
			AssertHasMessageErrorContaining(declaration.JE_GBInfo, "The company name of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a company name for the proxy organization.");
			AssertHasMessageErrorContaining(declaration.JE_GBInfo, "The email of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a email for the proxy organization.");
			AssertHasMessageErrorContaining(declaration.JE_GBInfo, "The phone number of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a phone number for the proxy organization.");

			broker.OH_FullName = "(주)레디코리아";
			broker.MainAddress.OA_Email = "ReadyKorea@wisetechglobal.com";
			broker.MainAddress.OA_Phone = "01044587479";
			declaration.Validation.ValidateJE_GB();
			AssertNoMessageErrorContaining(declaration.JE_GBInfo, "The company name of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a company name for the proxy organization.");
			AssertNoMessageErrorContaining(declaration.JE_GBInfo, "The email of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a email for the proxy organization.");
			AssertNoMessageErrorContaining(declaration.JE_GBInfo, "The phone number of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a phone number for the proxy organization.");
		}

		public void TestJE_OA_ImporterAddress()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "(주)레디코리아");
			TestOrgDataSetUpHelper.AddOrgContact(importer, "BuyerName", true);
			var importerAddress = importer.MainAddress;

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			declaration.Validation.ValidateJE_OA_ImporterAddress();
			AssertNoMessageErrors(declaration.JE_OA_ImporterAddressInfo);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			declaration.JE_OA_ImporterAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_ImporterAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			AssertNoMessageErrors(declaration.JE_OA_ImporterAddressInfo);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.B;
			declaration.JE_OA_ImporterAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_ImporterAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			AssertNoMessageErrors(declaration.JE_OA_ImporterAddressInfo);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.F;
			declaration.JE_OA_ImporterAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_ImporterAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			AssertNoMessageErrors(declaration.JE_OA_ImporterAddressInfo);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.G;
			declaration.JE_OA_ImporterAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_ImporterAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			AssertNoMessageErrors(declaration.JE_OA_ImporterAddressInfo);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			declaration.JE_OA_ImporterAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_ImporterAddressInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_OA_ImporterAddress = importerAddress.PK;
			AssertNoMessageErrors(declaration.JE_OA_ImporterAddressInfo);
		}

		public void TestCheckJE_OH_DutyPayer()
		{
			declaration.JE_PaidBy = PaidByCodeList.Codes.CLI;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, MandatoryValidation.YouHaveNotEntered);

			var emptyOrgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			emptyOrgHeader.MainAddress.Address1 = ZString.Empty;
			declaration.JE_OH_DutyPayer = emptyOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.GetMissingRegistrationNumberMessage("Business Registration Number", Constants.IdentificationType.BusinessRegNo));
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.NotEnteredOrgAddressMessage);
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.MissingPhoneNumberMessage);
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.MissingRepresentativeMessage);

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA");
			TestOrgDataSetUpHelper.AddOrgContact(payer, "송기홍", true);
			TestOrgDataSetUpHelper.AddOrgAddress(payer.MainAddress, "서울특별시 영등포구 국제금융로 10", "(여의도동, 서울 국제금융 센터)");
			payer.MainAddress.OA_Phone = "023345877";
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertNoMessageErrors(declaration.JE_OH_DutyPayerInfo);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_DutyPayer = ZGuid.Empty;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "You have indicated the payer type as different to importer, but they have the same business registration number.");

			declaration.JE_OH_Importer = payer.PK;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "You have indicated the payer type as different to importer, but they have the same business registration number.");

			declaration.JE_PaidBy = PaidByCodeList.Codes.CLI;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "You have indicated the payer type as different to importer, but they have the same business registration number.");

			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA2");
			TestOrgDataSetUpHelper.AddCustomsCode(importer, payerCodes);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "You have indicated the payer type as different to importer, but they have the same business registration number.");

			declaration.JE_OH_Importer = emptyOrgHeader.PK;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertNoMessageErrors(declaration.JE_OH_DutyPayerInfo);
		}

		public void TestCheckJE_OH_DutyPayer5UL()
		{
			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UL);
			var emptyOrgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			emptyOrgHeader.MainAddress.Address1 = ZString.Empty;
			declaration.JE_OH_DutyPayer = emptyOrgHeader.PK;

			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.NotEnteredOrgAddressMessage);
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, IMPJobDeclarationValidation.MissingBankAccountNo);
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, IMPJobDeclarationValidation.MissingBankCode);

			emptyOrgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Citizen Registration Number", IdentificationType.KoreanRegNoForResident));
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Unipass ID", IdentificationType.UnipassIDForIndividual));
			
			emptyOrgHeader.OH_Category = OrgConstants.Category.Business;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Business Registration Number", IdentificationType.BusinessRegNo));
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Unipass ID", IdentificationType.UnipassIDForOrganization));

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA");
			TestOrgDataSetUpHelper.AddOrgContact(payer, "송기홍", true);
			TestOrgDataSetUpHelper.AddOrgAddress(payer.MainAddress, "서울특별시 영등포구 국제금융로 10", "(여의도동, 서울 국제금융 센터)");
			payer.MainAddress.OA_Phone = "023345877";
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "1010101234567", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForIndividual, Number = "1010101234567", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "1010101234567", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.DutyPayerWrapper.ZO_BankAccNo = "123-45-678901";
			declaration.DutyPayerWrapper.ZO_BankCode = "X";

			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);
			payer.OH_Category = OrgConstants.Category.Business;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Business Registration Number", IdentificationType.BusinessRegNo));
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Unipass ID", IdentificationType.UnipassIDForOrganization));

			payer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Citizen Registration Number", IdentificationType.KoreanRegNoForResident));
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobComInvoiceHeaderValidation.GetMissingRegistrationNumberMessage("Unipass ID", IdentificationType.UnipassIDForIndividual));
			
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.MissingCompanyNameMessage);
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, JobDeclarationValidation.NotEnteredOrgAddressMessage);
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, IMPJobDeclarationValidation.MissingBankAccountNo);
			
			declaration.DutyPayerWrapper.ZO_BankCode = "XXX";
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, IMPJobDeclarationValidation.InvalidBankCode);

			declaration.DutyPayerWrapper.ZO_BankCode = BankTypeList.Codes._023;
			declaration.Validation.ValidateJE_OH_DutyPayer();
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, IMPJobDeclarationValidation.InvalidBankCode);
		}

		public void TestCheckJE_OH_DutyPayer5FN()
		{
			var businessPayer_GBR = CreatePayer("BUS", "RK1", "PAYER1", IdentificationType.BusinessRegNo, "1208174197");
			var businessPayer_07 = CreatePayer("BUS", "RK2", "PAYER2", IdentificationType.ForeignCompanyID, "USACEANT0001T");
			var businessPayer_Empty = CreatePayer("BUS", "RK3", "PAYER4");

			var individualPayer_01 = CreatePayer("NAT", "RK4", "PAYER4", IdentificationType.KoreanRegNoForResident, "1234567890123");
			var individualPayer_PAS = CreatePayer("NAT", "RK5", "PAYER5", IdentificationType.PassportNo, "YC00158522354");
			var individualPayer_03 = CreatePayer("NAT", "RK6", "PAYER6", IdentificationType.KoreanRegNoForForeigner, "KR0843652");
			var individualPayer_05 = CreatePayer("NAT", "RK7", "PAYER7", IdentificationType.UnipassIDForIndividual, "P811111234569");
			var individualPayer_Empty = CreatePayer("NAT", "RK8", "PAYER8");

			string messageErrorForBusiness = "There is no identification number for this organization. Please press F3 here and add a number of type 'GBR' or '07' in Config > Registration Numbers/Codes on the Organization form.";
			string messageErrorForIndividual = "There is no identification number for this organization. Press F3 here and add one of the types '01', 'PAS', '03', '05' in the organization form under Config > Registration Numbers/Codes.";

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5FN);
			declaration.JE_OH_DutyPayer = businessPayer_GBR.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForBusiness);
			declaration.JE_OH_DutyPayer = businessPayer_07.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForBusiness);
			declaration.JE_OH_DutyPayer = businessPayer_Empty.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForBusiness);

			declaration.JE_OH_DutyPayer = individualPayer_01.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForIndividual);
			declaration.JE_OH_DutyPayer = individualPayer_PAS.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForIndividual);
			declaration.JE_OH_DutyPayer = individualPayer_03.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForIndividual);
			declaration.JE_OH_DutyPayer = individualPayer_05.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForIndividual);
			declaration.JE_OH_DutyPayer = individualPayer_Empty.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, messageErrorForIndividual);

			OrgHeader CreatePayer(string category, string code, string fullName, string codeType = "", string customsRegNo = "")
			{
				var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, category, code, fullName);
				var payerCode = payer.CustomsCodes.AddNew();
				payerCode.OK_CodeType = codeType;
				payerCode.OK_CustomsRegNo = customsRegNo;

				return payer;
			}
		}

		public void TestJE_OH_DutyPayer934()
		{
			var emptyBusinessOrgHeader = CreateOrganizationData("RK1", "", "", "", "");
			var emptyPersonOrgHeader = CreateOrganizationData("RK2", "", "", "", "", "NAT");
			var businessOrgHeader = CreateOrganizationData("RK3", "(주)레디코리아", "주소1", "주소2", "대표자명");
			TestOrgDataSetUpHelper.AddCustomsCode(businessOrgHeader, new IDNumberAndType[] { new IDNumberAndType() { Type = "GBR", Number = "사업자등록번호" } });
			var personOrgHeader = CreateOrganizationData("RK4", "(주)레디코리아", "주소1", "주소2", "대표자명", "NAT");
			TestOrgDataSetUpHelper.AddCustomsCode(personOrgHeader, new IDNumberAndType[] { new IDNumberAndType() { Type = "01", Number = "주민등록번호" } });

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			declaration.JE_OH_DutyPayer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_DutyPayer = emptyBusinessOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "There is no Identification ID for this organization. Please press F3 here and add a number of type 'GBR', '07' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_OH_DutyPayer = emptyPersonOrgHeader.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertHasMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "There is no Identification ID for this organization. Please press F3 here and add a number of type '01', 'PAS', '03', '05' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_OH_DutyPayer = businessOrgHeader.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "There is no Identification ID for this organization. Please press F3 here and add a number of type 'GBR', '07' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_OH_DutyPayer = personOrgHeader.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertNoMessageErrorContaining(declaration.JE_OH_DutyPayerInfo, "There is no Identification ID for this organization. Please press F3 here and add a number of type '01', 'PAS', '03', '05' in Config > Registration Numbers/Codes on the Organization form.");

			OrgHeader CreateOrganizationData(string code, string companyName, string address1, string address2, string ceoName, string category = "BUS")
			{
				var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, category, code, companyName);
				TestOrgDataSetUpHelper.AddOrgAddress(orgHeader.MainAddress, address1, address2);
				TestOrgDataSetUpHelper.AddOrgContact(orgHeader, ceoName, true);

				return orgHeader;
			}
		}

		public void TestImporterType()
		{
			var validation = (IMPJobDeclarationValidation)declaration.Validation;
			declaration.ImporterType = ZString.Empty;
			validation.ValidateImporterType();
			AssertNoMessageErrors(declaration.ImporterTypeInfo);

			declaration.ImporterType = "X";
			validation.ValidateImporterType();
			AssertHasMessageErrorContaining(declaration.ImporterTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.ImporterType = ImporterTypeCodeList.Codes.A;
			validation.ValidateImporterType();
			AssertNoMessageErrors(declaration.ImporterTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			base.declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			base.declaration.Invoices.AddNew();
		}

		public void TestJE_TradeIndicatorWithKP()
		{
			declaration.JE_TradeIndicatorWithKP = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);

			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.A;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.C;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.E;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.F;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.G;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.I;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.K;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.M;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.N;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.P;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.S;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.Y;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);
			declaration.JE_TradeIndicatorWithKP = SouthNorthTradeYNCodeList.Codes.Z;
			AssertNoMessageErrors(declaration.JE_TradeIndicatorWithKPInfo);

			declaration.JE_TradeIndicatorWithKP = "0";
			AssertHasMessageError(declaration.JE_TradeIndicatorWithKPInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJE_GoldTrade()
		{
			declaration.JE_GoldTrade = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_GoldTradeInfo);

			declaration.JE_GoldTrade = YesNoList.Codes.Yes;
			AssertNoMessageErrors(declaration.JE_GoldTradeInfo);

			declaration.JE_GoldTrade = "X";
			AssertHasMessageError(declaration.JE_GoldTradeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJE_TradeType()
		{
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._11;
			AssertNoMessageErrors(declaration.JE_TradeTypeInfo);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.Validation.ValidateJE_TradeType();
			AssertNoMessageErrors(declaration.JE_TradeTypeInfo);

			instruction.OnlineOrders.AddNew();
			declaration.Validation.ValidateJE_TradeType();
			AssertHasMessageErrorContaining(declaration.JE_TradeTypeInfo, IMPJobDeclarationValidation.OnlineOrdersDoNotEntered);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			AssertNoMessageErrors(declaration.JE_TradeTypeInfo);
		}

		public void TestJE_ContainerPackMode()
		{
			declaration.JE_ContainerPackMode = "";
			AssertHasMessageErrorContaining(declaration.JE_ContainerPackModeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ContainerPackMode = "XX";
			AssertHasMessageErrorContaining(declaration.JE_ContainerPackModeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_ContainerPackMode = ContainerPackModeCodeList.Codes.BU;
			AssertNoMessageErrors(declaration.JE_ContainerPackModeInfo);
		}

		public void TestCheckJE_DeclarationPlan()
		{
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			declaration.Validation.ValidateJE_DeclarationPlan();
			AssertNoMessageErrorContaining(declaration.JE_DeclarationPlanInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			declaration.Validation.ValidateJE_DeclarationPlan();
			AssertHasMessageErrorContaining(declaration.JE_DeclarationPlanInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarationPlan = "X";
			AssertHasMessageErrorContaining(declaration.JE_DeclarationPlanInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_DeclarationPlan = ImportCustomsClearancePlanCodeList.Codes.A;
			AssertNoMessageErrors(declaration.JE_DeclarationPlanInfo);
		}

		public void TestCheckJE_ProcedureType()
		{
			declaration.Validation.ValidateJE_ProcedureType();
			AssertHasMessageErrorContaining(declaration.JE_ProcedureTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ProcedureType = "XX";
			AssertHasMessageErrorContaining(declaration.JE_ProcedureTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			AssertNoMessageErrorContaining(declaration.JE_ProcedureTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsLoadPort = "KR";
			declaration.Validation.ValidateJE_ProcedureType();
			AssertHasMessageErrorContaining(declaration.JE_ProcedureTypeInfo, "If Departure Country Code is 'KR' then, Declaration Procedure Type must be ('13', '29', '15', '28', '33', '36')");

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			declaration.Validation.ValidateJE_ProcedureType();
			AssertHasMessageErrorContaining(declaration.JE_ProcedureTypeInfo, "If Departure Country Code is 'KR' then, Declaration Procedure Type must be ('13', '29', '15', '28', '33', '36')");
			AssertHasMessageErrorContaining(declaration.JE_ProcedureTypeInfo, "If Payment Type = '33' then, Declaration Procedure Type must be ('11', '13', '15', '16', '29', '36')");

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._33;
			AssertNoMessageErrorContaining(declaration.JE_ProcedureTypeInfo, "If Departure Country Code is 'KR' then, Declaration Procedure Type must be ('13', '29', '15', '28', '33', '36')");
			AssertHasMessageErrorContaining(declaration.JE_ProcedureTypeInfo, "If Payment Type = '33' then, Declaration Procedure Type must be ('11', '13', '15', '16', '29', '36')");

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._36;
			AssertNoMessageErrorContaining(declaration.JE_ProcedureTypeInfo, "If Departure Country Code is 'KR' then, Declaration Procedure Type must be ('13', '29', '15', '28', '33', '36')");
			AssertNoMessageErrorContaining(declaration.JE_ProcedureTypeInfo, "If Payment Type = '33' then, Declaration Procedure Type must be ('11', '13', '15', '16', '29', '36')");
		}

		public void TestJE_TransshipmentPort()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "XXINC";
			port.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			Factory.Save();

			declaration.Validation.ValidateJE_TransshipmentPort();
			AssertNoMessageErrors(declaration.JE_TransshipmentPortInfo);

			declaration.JE_TransshipmentPort = "XXXXX";
			AssertHasMessageErrorContaining(declaration.JE_TransshipmentPortInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_TransshipmentPort = port.RL_Code;
			AssertNoMessageErrors(declaration.JE_TransshipmentPortInfo);
		}

		public void TestJE_MissedDecPenaltyRate()
		{
			declaration.JE_MissedDecPenaltyRate = -1;
			AssertHasWarningContaining(declaration.JE_MissedDecPenaltyRateInfo, "Percentage value should be between 0 and 100");

			declaration.JE_MissedDecPenaltyRate = 100;
			AssertNoWarnings(declaration.JE_MissedDecPenaltyRateInfo);

			declaration.JE_MissedDecPenaltyRate = 101;
			AssertHasWarningContaining(declaration.JE_MissedDecPenaltyRateInfo, "Percentage value should be between 0 and 100");
		}
		public void TestJE_TaxOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.TaxOffice, "Tax Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, "100", "서울청", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.JE_TaxOffice = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_TaxOfficeInfo);

			declaration.JE_TaxOffice = "100";
			AssertNoMessageErrors(declaration.JE_TaxOfficeInfo);

			declaration.JE_TaxOffice = "0";
			AssertHasMessageError(declaration.JE_TaxOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJE_AuthorJobTitle()
		{
			declaration.JE_AuthorJobTitle = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AuthorJobTitleInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			declaration.JE_AuthorJobTitle = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_AuthorJobTitleInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuthorJobTitle = "실무팀장";
			AssertNoMessageErrors(declaration.JE_AuthorJobTitleInfo);
		}

		public void TestJE_AuthorName()
		{
			declaration.JE_AuthorName = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AuthorNameInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			declaration.JE_AuthorName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_AuthorNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuthorName = "실무자";
			AssertNoMessageErrors(declaration.JE_AuthorNameInfo);
		}

		public void TestJE_AuthorPhone()
		{
			declaration.JE_AuthorPhone = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AuthorPhoneInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			declaration.JE_AuthorPhone = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_AuthorPhoneInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuthorPhone = "+82-123-4567";
			AssertNoMessageErrors(declaration.JE_AuthorPhoneInfo);
		}

		public void TestJE_AuditorJobTitle()
		{
			declaration.JE_AuditorJobTitle = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AuditorJobTitleInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			declaration.JE_AuditorJobTitle = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_AuditorJobTitleInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuditorJobTitle = "책임팀장";
			AssertNoMessageErrors(declaration.JE_AuditorJobTitleInfo);
		}

		public void TestJE_AuditorName()
		{
			declaration.JE_AuditorName = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AuditorNameInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			declaration.JE_AuditorName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_AuditorNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuditorName = "책임자";
			AssertNoMessageErrors(declaration.JE_AuditorNameInfo);
		}

		public void TestJE_AuditorPhone()
		{
			declaration.JE_AuditorPhone = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_AuditorPhoneInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			declaration.JE_AuditorPhone = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_AuditorPhoneInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_AuditorPhone = "+82-123-4567";
			AssertNoMessageErrors(declaration.JE_AuditorPhoneInfo);
		}

		new JobDeclaration declaration => (JobDeclaration)base.declaration;
	}
}
