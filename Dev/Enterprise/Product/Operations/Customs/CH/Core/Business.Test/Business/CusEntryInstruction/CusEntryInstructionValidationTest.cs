using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryInstructionValidation))]
sealed class CusEntryInstructionValidationTests : BusinessObjectValidationTestCase
{
	public void TestStyleList()
	{
		RefCusCodeTestHelper.CreateEnstyCodeList(Factory);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Instruction.CEI_StyleInfo, "98", "99");
	}

	public void TestSubStyleList()
	{
		RefCusCodeTestHelper.CreateEnsubCodeList(Factory);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Instruction.CEI_SubStyleInfo, "98", "99");
	}

	public void TestCheckCEI_OA_Warehouse()
	{
		Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		Instruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.BondedWarehouse;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Instruction.CEI_OA_WarehouseInfo);
	}

	public void TestCheckCEI_OH_Owner()
	{
		Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		Instruction.CEI_WarehouseType = UniversalReferenceConstants.WarehouseTypeCodes.BondedWarehouse;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Instruction.CEI_OH_OwnerInfo);
	}

	public void TestCEI_Style_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Instruction.CEI_StyleInfo);

	public void TestCEI_Description_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Instruction.CEI_DescriptionInfo);

	public void TestCEI_SubStyle_Mandatory()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Instruction.CEI_SubStyleInfo);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		Instruction.Validation.ValidateCEI_SubStyle();
		AssertNoMessageErrorContaining("Mandatory message error is not shown for EXP Declarations", Instruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Instruction.CEI_SubStyle = ZString.Empty;
		AssertNoMessageErrorContaining("Mandatory message error is not shown when the MessageType is EDA and the Activation Type is PAS", Instruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Instruction.CEI_SubStyle = ZString.Empty;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Instruction.CEI_SubStyleInfo);
	}

	public void TestCheckCEI_Procedure()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Instruction.CEI_ProcedureInfo);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertErrorFieldIsNotMandatory(Instruction.CEI_ProcedureInfo);
	}

	public void TestCheckCEI_PartialDelivery_NS30003() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNS30003_Ticked(Instruction.CEI_PartialDeliveryInfo.HumanReadableName);
		PlausiValidationTestHelper.AssertNS30003(Instruction, Instruction.CEI_PartialDeliveryInfo, messageError);
	});

	public void TestCheckCEI_WarehouseType()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		RefCusCodeTestHelper.CreateWarehouseCodeList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(Instruction.CEI_WarehouseTypeInfo, RefCusCodeTestHelper.InvalidWarehouseTypeCode, RefCusCodeTestHelper.ValidWarehouseTypeCode);
	}

	public void TestCheckCEI_DeclarationReason_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		RefCusCodeTestHelper.CreatePreasCodeList(Factory);

		CombineAssertions(() =>
		{
			AssertNoMessageErrors("Reason not mandatory if time = Reservation", Instruction.CEI_DeclarationReasonInfo);
			AssertNoWarnings("no warning if reason not entered", Instruction.CEI_DeclarationReasonInfo);

			Instruction.CEI_SubStyle = RefCusCodeTestHelper.ValidStyleListCode;
			AssertNoMessageErrors("Reason not mandatory if type != Provisional", Instruction.CEI_DeclarationReasonInfo);

			Instruction.CEI_Style = RefCusCodeTestHelper.ProvisionalStyleListCode;

			Instruction.CEI_DeclarationReason = ZString.Empty;
			AssertHasMessageErrors("Reason mandatory if type = Provisional", Instruction.CEI_DeclarationReasonInfo);

			Instruction.CEI_DeclarationReason = DeclarationReason.ProofOfOriginEUCountries;
			AssertNoMessageErrors("time = Reservation, valid reason entered", Instruction.CEI_DeclarationReasonInfo);

			Instruction.CEI_DeclarationReason = RefCusCodeTestHelper.InvalidDeclarationReasonCode;
			AssertHasMessageErrors("time = Reservation, invalid reason entered", Instruction.CEI_DeclarationReasonInfo);
		});
	}

	public void TestCheckCEI_DeclarationReason_R288()
	{
		var messageError = ValidationMessages.Plausi.MessageR288;

		RefCusCodeTestHelper.CreatePreasCodeList(Factory);
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = Declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = Instruction.PK;
		invoiceLine2.JI_CEI = Instruction.PK;

		invoiceLine1.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
		invoiceLine2.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;
		Instruction.CEI_Style = DeclarationTypeCodes.Provisional;
		Instruction.CEI_DeclarationReason = DeclarationReason.ProofOfOriginDevelopingCountries;
		AssertNoMessageError("Has development country (1)", Instruction.CEI_DeclarationReasonInfo, messageError);

		invoiceLine1.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;
		Instruction.Validation.ValidateCEI_DeclarationReason();
		AssertHasMessageError("Development country required (2)", Instruction.CEI_DeclarationReasonInfo, messageError);

		invoiceLine1.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
		Instruction.Validation.ValidateCEI_DeclarationReason();
		AssertNoMessageError("Has development country (3)", Instruction.CEI_DeclarationReasonInfo, messageError);
		invoiceLine1.JI_CEI = ZGuid.Empty;
		Instruction.Validation.ValidateCEI_DeclarationReason();
		AssertHasMessageError("Development country required (4)", Instruction.CEI_DeclarationReasonInfo, messageError);
		invoiceLine1.JI_CEI = Instruction.PK;
		Instruction.Validation.ValidateCEI_DeclarationReason();
		AssertNoMessageError("Development country required (5)", Instruction.CEI_DeclarationReasonInfo, messageError);

		invoiceLine1.Delete();
		Instruction.Validation.ValidateCEI_DeclarationReason();
		AssertHasMessageError("Development country required (6)", Instruction.CEI_DeclarationReasonInfo, messageError);

		Instruction.CEI_Style = DeclarationTypeCodes.Provisional;
		Instruction.CEI_DeclarationReason = DeclarationReason.ProofOfOriginEUCountries;
		AssertNoMessageError("Not ProofOfOriginForDevelopingCountries (7)", Instruction.CEI_DeclarationReasonInfo, messageError);

		Instruction.CEI_Style = DeclarationTypeCodes.Definitive;
		Instruction.CEI_DeclarationReason = DeclarationReason.ProofOfOriginDevelopingCountries;
		AssertNoMessageError("Not Provisional (8)", Instruction.CEI_DeclarationReasonInfo, messageError);
	}

	public void TestCheckCEI_DeclarationReason_R166()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = Instruction.PK;
		invoiceLine1.JI_PrimaryPreference = PrimaryPreferenceCodes.NormalTariff;
		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = Instruction.PK;
		invoiceLine2.JI_PrimaryPreference = PrimaryPreferenceCodes.NormalTariff;
		Instruction.CEI_Style = DeclarationTypeCodes.Provisional;

		RefCusCodeTestHelper.CreatePreasCodeList(Factory);

		CombineAssertions(() =>
		{
			Instruction.CEI_DeclarationReason = "1";
			AssertNoMessageError("", Instruction.CEI_DeclarationReasonInfo, ValidationMessages.Plausi.MessageR166c);

			invoiceLine2.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;
			Instruction.CEI_DeclarationReason = "1";
			AssertHasMessageError("Validation Check R166", Instruction.CEI_DeclarationReasonInfo, ValidationMessages.Plausi.MessageR166c);

			Instruction.CEI_DeclarationReason = "2";
			AssertHasMessageError("Validation Check R166", Instruction.CEI_DeclarationReasonInfo, ValidationMessages.Plausi.MessageR166c);

			Instruction.CEI_DeclarationReason = "3";
			AssertHasMessageError("Validation Check R166", Instruction.CEI_DeclarationReasonInfo, ValidationMessages.Plausi.MessageR166c);

			Instruction.CEI_DeclarationReason = "4";
			AssertHasMessageError("Validation Check R166", Instruction.CEI_DeclarationReasonInfo, ValidationMessages.Plausi.MessageR166c);
		});
	}

	public void TestCheckCEI_NextProcedure()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		RefCusCodeTestHelper.CreateNextProcedureList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(Instruction.CEI_NextProcedureInfo, RefCusCodeTestHelper.InvalidNextProcedureCode, RefCusCodeTestHelper.ValidNextProcedureCode);
	}

	public void TestCheckCEI_TransportChargesMethodOfPayment_NS30108()
	{
		RefCusTradeGroupTestHelper.CreateTestNCL0147CountryList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		Declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew().PK;
		Declaration.SupplierDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<JobDocAddress>().Organisation.PK;
		var codeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;

		CombineAssertions("Method of payment must be empty when County of Destination belongs to EU Security Zone", () =>
		{
			string messageError = PassarValidationMessages.MessageNS30108_EuSecurityZoneMethodOfPayment;

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			AssertNoMessageErrorContaining("Assert if MOP is empty no error", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Instruction.CEI_TransportChargesMethodOfPayment = "T";
			AssertHasMessageErrorContaining("Assert GoodsDestination is in the list and MOP is not empty error occur", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			instruction.Validation.ValidateCEI_TransportChargesMethodOfPayment();
			AssertNoMessageErrorContaining("Assert GoodsDestination is out of the list no error", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUSECButNotInEUN;
			instruction.Validation.ValidateCEI_TransportChargesMethodOfPayment();
			AssertNoMessageErrorContaining("Assert GoodsDestination is out of the list no error", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.JE_GoodsDestination = ZString.Empty;
			instruction.Validation.ValidateCEI_TransportChargesMethodOfPayment();
			AssertNoMessageErrorContaining("Assert if GoodsDestination is empty no error", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);
		});

		CombineAssertions("When Country of Destination is outside the European Security Zone, and either Representative or Exporter AEO number is empty, Method of Payment cannot be empty", () =>
		{
			string messageError = PassarValidationMessages.MessageNS30108_TransportMethodOfPayment;
			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			Instruction.CEI_TransportChargesMethodOfPayment = ZString.Empty;
			AssertNoMessageErrorContaining("Assert if Country in the list no error", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			instruction.Validation.ValidateCEI_TransportChargesMethodOfPayment();
			AssertHasMessageErrorContaining("When Country not in the list and, Declarant and Supplier has not AEO and MOP is empty an error occur", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.DeclarantAddress.Header.CustomsCodes.AddNew(codeType, "AEO123");
			instruction.Validation.ValidateCEI_TransportChargesMethodOfPayment();
			AssertHasMessageErrorContaining("When Country not in the list and Supplier has not AEO and MOP is empty an error occur", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Instruction.CEI_TransportChargesMethodOfPayment = "T";
			AssertNoMessageErrorContaining("When Country not in the list and Supplier has not AEO and MOP is not empty no error occur", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew().PK;
			Declaration.SupplierDocumentaryAddress.Organisation.CustomsCodes.AddNew(codeType, "AEO123", RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC);
			Instruction.CEI_TransportChargesMethodOfPayment = ZString.Empty;
			AssertHasMessageErrorContaining("When Country not in the list and, Declarant has not AEO and MOP is empty an error occur", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.DeclarantAddress.Header.CustomsCodes.AddNew(codeType, "AEO123");
			instruction.Validation.ValidateCEI_TransportChargesMethodOfPayment();
			AssertNoMessageErrorContaining("When Country not in the list and, AEO is set in the Supplier and Declarant and MOP is empty no error", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);

			Declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().Addresses.AddNew().PK;
			Instruction.CEI_TransportChargesMethodOfPayment = "T";
			AssertNoMessageErrorContaining("When Country is not in the list and, Declarant and Supplier has not AEO and MOP is not empty no error", Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);
		});
	}

	public void TestCheckCEI_TransportChargesMethodOfPayment_NS30003()
	{
		var messageError = PassarValidationMessages.MessageNS30003_NotEmpty(Instruction.CEI_TransportChargesMethodOfPaymentInfo.HumanReadableName);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		PlausiValidationTestHelper.AssertNS30003(Instruction, Instruction.CEI_TransportChargesMethodOfPaymentInfo, messageError);
	}

	public void TestCheckCEI_PartialDelivery_NP70212() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNP702012_NoV1201;

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

		Instruction.CEI_PartialDelivery = true;
		AssertHasMessageError("ParitalDelivery true / no addInfor / no V1201", Instruction.CEI_PartialDeliveryInfo, messageError);

		var addInfo1 = Instruction.AdditionalInformations.AddNew();
		addInfo1.CSI_Code = "V1200";
		addInfo1.CSI_Description = "xxx";
		Instruction.Validation.ValidateCEI_PartialDelivery();
		AssertHasMessageError("ParitalDelivery true / has addInfo / no V1201", Instruction.CEI_PartialDeliveryInfo, messageError);

		var addInfo2 = Instruction.AdditionalInformations.AddNew();
		addInfo2.CSI_Code = AdditionalInformationTypeCodes.PartialShipmentNumber;
		addInfo2.CSI_Description = "1";
		Instruction.Validation.ValidateCEI_PartialDelivery();
		AssertNoMessageError("ParitalDelivery true / has addInfo / has V1201", Instruction.CEI_PartialDeliveryInfo, messageError);

		Instruction.CEI_PartialDelivery = false;
		Instruction.AdditionalInformations.RemoveAndDeleteAll();
		Instruction.Validation.ValidateCEI_PartialDelivery();
		AssertNoMessageError("ParitalDelivery false / no addInfo / no V1201", Instruction.CEI_PartialDeliveryInfo, messageError);
	});

	JobDeclaration Declaration => declaration ??= CreateJobDeclaration();
	JobDeclaration declaration;

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration CreateJobDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		return declaration;
	}
}
