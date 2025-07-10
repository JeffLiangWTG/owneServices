using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class ExportJobDeclarationValidationTest : CommonJobDeclarationValidationTest
{
	public void TestCheckJE_LocationQualifier_MustBeEmptyValidation()
	{
		declaration.JE_LocationQualifier = "";
		declaration.ZG_AuthorisationNumber = "";

		AssertNoMessageErrorContaining(declaration.JE_LocationQualifierInfo, ValidationCaptions.JobDeclaration.LocationQualifierMustBeEmpty);
		declaration.ZG_AuthorisationNumber = "123456";
		AssertNoMessageErrorContaining(declaration.JE_LocationQualifierInfo, ValidationCaptions.JobDeclaration.LocationQualifierMustBeEmpty);
		declaration.JE_LocationQualifier = "D";
		AssertHasMessageErrorContaining(declaration.JE_LocationQualifierInfo, ValidationCaptions.JobDeclaration.LocationQualifierMustBeEmpty);
		declaration.ZG_AuthorisationNumber = "";
		declaration.Validation.ValidateJE_LocationQualifier();
		AssertNoMessageErrorContaining(declaration.JE_LocationQualifierInfo, ValidationCaptions.JobDeclaration.LocationQualifierMustBeEmpty);
	}

	public void TestCheckJE_LocationOfGoods_MandatoryValidation()
	{
		var youHaveNotEnteredMessage = MandatoryValidation.YouHaveNotEntered;
		var doNotEnteredMessage = MandatoryValidation.DoNotEntered;

		entryInstruction.CEI_Style = ZString.Empty;
		declaration.JE_LocationOfGoods = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, youHaveNotEnteredMessage);
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, doNotEnteredMessage);

		entryInstruction.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo;
		declaration.JE_LocationOfGoods = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, youHaveNotEnteredMessage);
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, doNotEnteredMessage);

		declaration.JE_LocationOfGoods = "AAA";
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, youHaveNotEnteredMessage);
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, doNotEnteredMessage);

		entryInstruction.CEI_Style = SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana;
		declaration.JE_LocationOfGoods = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, youHaveNotEnteredMessage);
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, doNotEnteredMessage);

		declaration.JE_LocationOfGoods = "BBB";
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, youHaveNotEnteredMessage);
		AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, doNotEnteredMessage);
	}

	public void TestCheckJE_RL_NKPortOfLoading()
	{
		var declaration = Factory.New<JobDeclaration>();
		var expectedMessageError = "Provide a port code to determine a port tax rate";
		declaration.JE_MessageType = "EXP";
		declaration.JE_TransportMode = "SEA";

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		declaration.JE_RL_NKPortOfLoading = "";
		declaration.Validation.ValidateJE_RL_NKPortOfLoading();
		AssertNoWarningContaining(declaration.JE_RL_NKPortOfLoadingInfo, expectedMessageError);

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		declaration.JE_RL_NKPortOfLoading = "";
		declaration.Validation.ValidateJE_RL_NKPortOfLoading();
		AssertHasWarningContaining(declaration.JE_RL_NKPortOfLoadingInfo, expectedMessageError);

		declaration.JE_RL_NKPortOfLoading = "ITVCE";
		declaration.Validation.ValidateJE_RL_NKPortOfLoading();
		AssertNoWarningContaining(declaration.JE_RL_NKPortOfLoadingInfo, expectedMessageError);

		declaration.JE_RL_NKPortOfLoading = "XXXXX";
		declaration.Validation.ValidateJE_RL_NKPortOfLoading();
		AssertHasWarningContaining(declaration.JE_RL_NKPortOfLoadingInfo, expectedMessageError);

		declaration.JE_TransportMode = "AIR";
		declaration.JE_RL_NKPortOfLoading = "";
		declaration.Validation.ValidateJE_RL_NKPortOfLoading();
		AssertNoWarningContaining(declaration.JE_RL_NKPortOfLoadingInfo, expectedMessageError);
	}

	public void TestJE_RL_NKOrigin()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_RL_NKOrigin = "IT";
		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.OH_RL_NKClosestPort = "ITTAR";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;

		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;

		CombineAssertions("Country codes in Declaration and Suppliers", () =>
		{
			AssertNoWarning(invoiceHeader1.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);
			AssertNoWarning(declaration.JE_RL_NKOriginInfo, ValidationCaptions.JobDeclaration.CannotHaveDifferentSuppliers);

			orgHeader1.OH_RL_NKClosestPort = "AUSYD";
			invoiceHeader1.Validation.ValidateJZ_OH_Supplier();
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertHasWarning(invoiceHeader1.JZ_OH_SupplierInfo, ValidationCaptions.InvoiceHeader.CannotHaveDifferentSuppliers);
			AssertHasWarning(declaration.JE_RL_NKOriginInfo, ValidationCaptions.JobDeclaration.CannotHaveDifferentSuppliers);
		});
	}

	public void TestFinalDestinationValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKFinalDestinationInfo);
	}

	public void TestOriginValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKOriginInfo);
	}

	public void TestPortOfArrivalValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKPortOfArrivalInfo);
	}

	public void TestPortOfLoadingValidationAddsWarningNotification()
	{
		AssertListValidationAddsWarningNotification(declaration.JE_RL_NKPortOfLoadingInfo);
	}

	public void TestCheckJE_RN_NKTransportNationality_BasedOnTransportMode()
	{
		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
		declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
		declaration.JE_RN_NKTransportNationality = "SG";
		AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJE_TransportModeInlandHonoursOfficeCode()
	{
		var officeOfExit = declaration.CustomsOffices.GetFirstElementHaving("EXT");
		officeOfExit.CY_Data = "IT379100";
		declaration.JE_CustomsOffice = "IT303199";
		declaration.JE_TransportModeInland = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_CustomsOffice = "IT379100";
		declaration.JE_TransportModeInland = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_CustomsOffice = "IT303199";
		declaration.JE_TransportModeInland = "ROA";
		AssertNoMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		officeOfExit.CY_Data = ZString.Empty;
		declaration.JE_TransportModeInland = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportModeInland = "ROA";
		AssertNoMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		officeOfExit.CY_Data = "IT379100";
		declaration.JE_CustomsOffice = ZString.Empty;
		declaration.JE_TransportModeInland = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportModeInland = "ROA";
		AssertNoMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.JE_TransportModeInland = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_CustomsOffice = "IT303199";
		declaration.JE_TransportModeInland = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_TransportModeInland = "ROA";
		AssertNoMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJE_LocationQualifierListValidation()
	{
		const string expectedMessageError = "The code you have selected is not in the list";

		declaration.JE_LocationQualifier = "";
		AssertNoMessageErrorContaining("Empty JE_LocationQualifierInfo", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection;
		AssertHasMessageErrorContaining("Code 'D' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit;
		AssertHasMessageErrorContaining("Code 'F' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
		AssertHasMessageErrorContaining("Code 'FC' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
		AssertHasMessageErrorContaining("Code 'LB' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace;
		AssertHasMessageErrorContaining("Code 'LC' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.ZG_AuthorisationNumber = "ARG099";

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
		AssertHasMessageErrorContaining("Code 'LB' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace;
		AssertHasMessageErrorContaining("Code 'LC' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = "XX";
		AssertHasMessageErrorContaining("Code 'XX' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	new JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
}
