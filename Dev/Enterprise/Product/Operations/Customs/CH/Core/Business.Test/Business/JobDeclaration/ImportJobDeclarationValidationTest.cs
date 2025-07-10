using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ImportJobDeclarationValidation))]
sealed class ImportJobDeclarationValidationTest : JobDeclarationValidationTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Import;

	public void TestCheckJE_OH_Consignee() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_OH_ConsigneeInfo);

	public void TestCheckJE_RL_NKPortOfLoading() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_RL_NKPortOfLoadingInfo);

	public void TestCheckJE_OA_Representative()
	{
		AssertNoMessageErrors(Declaration.JE_OA_RepresentativeInfo);

		Declaration.JE_ClearanceLocation = UniversalReferenceConstants.ClearanceLocation.Domicile;
		Declaration.JE_OA_Representative = Factory.New<OrgAddress>().PK;
		AssertNoMessageError(Declaration.JE_OA_RepresentativeInfo, ValidationMessages.Plausi.MessageR348);

		Declaration.JE_OA_Representative = ZGuid.Empty;
		AssertHasMessageError(Declaration.JE_OA_RepresentativeInfo, ValidationMessages.Plausi.MessageR348);
	}

	public void TestCheckJE_OA_Representative_CountryCHorLI()
	{
		var authConsignee = Factory.New<OrgAddress>();

		CombineAssertions(() =>
		{
			authConsignee.OA_RL_NKRelatedPortCode = "DEBER";
			authConsignee.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			Declaration.JE_OA_Representative = authConsignee.PK;
			AssertHasMessageError(Declaration.JE_OA_RepresentativeInfo, ValidationMessages.Plausi.GetMessageR121("Authorized Consignee"));

			authConsignee.OA_RL_NKRelatedPortCode = "CHABL";
			authConsignee.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			Declaration.JE_OA_Representative = authConsignee.PK;
			AssertNoMessageError(Declaration.JE_OA_RepresentativeInfo, ValidationMessages.Plausi.GetMessageR121("Authorized Consignee"));

			authConsignee.OA_RL_NKRelatedPortCode = "LIBAZ";
			authConsignee.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Liechtenstein;
			Declaration.JE_OA_Representative = authConsignee.PK;
			AssertNoMessageError(Declaration.JE_OA_RepresentativeInfo, ValidationMessages.Plausi.GetMessageR121("Authorized Consignee"));
		});
	}

	public void TestCheckJE_PaymentMethod_Mandatory() => ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Declaration.JE_PaymentMethodInfo, "XXX", DeclarationPayerList.Codes.Cash);

	public void TestCheckJE_PaymentMethod_RegistrationNumber() => AssertPaymentRegistrationNumber(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, Declaration.JE_PaymentMethodInfo);

	public void TestCheckJE_PaymentMethod()
	{
		var paidByTestHelper = new PaidTestHelper(Factory);
		CombineAssertions(() =>
		{
			paidByTestHelper.TestCheckPaidBy(d => d.JE_PaymentMethodInfo, (d, v) => d.JE_VATPaidBy = v, @"If ""Cash"" is selected, ""Cash"" must also be selected");
		});
	}

	public void TestCheckJE_DeclarationLanguage_Mandatory() => ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Declaration.JE_DeclarationLanguageInfo, "EN", SwissCustomsLanguageList.Codes.German);

	public void TestCheckJE_VesselName_TransportMode() => AssertTransportMode(Declaration, Declaration.JE_VesselNameInfo, TransportTypeGenericList.Codes.Road, "MS Markus Bürkler");

	public void TestCheckJE_HouseBill_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_HouseBillInfo);

	public void TestCheckJE_VATPaidBy_RegistrationNumber() => AssertPaymentRegistrationNumber(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, Declaration.JE_VATPaidByInfo);

	public void TestCheckJE_LocationOfGoods_ClearanceLocation()
	{
		Declaration.JE_ClearanceLocation = UniversalReferenceConstants.ClearanceLocation.CustomsOffice;
		AssertNoMessageErrors(Declaration.JE_LocationOfGoodsInfo);
		Declaration.JE_ClearanceLocation = UniversalReferenceConstants.ClearanceLocation.Domicile;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_LocationOfGoodsInfo);
		Declaration.JE_LocationOfGoods = "Behind the corner.";
		AssertNoMessageErrors(Declaration.JE_LocationOfGoodsInfo);
	}

	public void TestCheckJE_ShipmentIncoTerm_InofficialIncoTerm()
	{
		var unofficialIncoterm = $"It will be converted to {IncoTerms.FreeCarrier} in the declaration sending message to customs";

		Declaration.JE_ShipmentIncoTerm = IncoTerms.FreeCarrierSeller;
		AssertHasWarningContaining(Declaration.JE_ShipmentIncoTermInfo, unofficialIncoterm);
		Declaration.JE_ShipmentIncoTerm = IncoTerms.FreeCarrier;
		AssertNoWarning(Declaration.JE_ShipmentIncoTermInfo, unofficialIncoterm);
		Declaration.JE_ShipmentIncoTerm = IncoTerms.FreeCarrierBuyer;
		AssertHasWarningContaining(Declaration.JE_ShipmentIncoTermInfo, unofficialIncoterm);
	}

	public void TestCheckDispatchCountryCode()
	{
		string messageError = ValidationMessages.Plausi.MessageR167c;

		RefCusCodeTestHelper.CreateDirectTransportationCodeList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_CountryOfOrigin = RefCusCodeTestHelper.ValidDirectTransportationCountry;
			invoiceLine.JI_Tariff = "25123456099100";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			declaration.JE_RL_NKPortOfLoading = RefCusCodeTestHelper.ValidDirectTransportationCountry + "XXX";
			AssertNoMessageError("Same country", declaration.DispatchCountryCodeInfo, messageError);

			invoiceLine.JI_CountryOfOrigin = RefCusCodeTestHelper.InvalidDirectTransportationCountry;
			invoiceLine.JI_Tariff = "25123456099100";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			declaration.JE_RL_NKPortOfLoading = "NLAMS";
			declaration.Validation.ValidateDispatchCountryCode();
			AssertNoMessageError("Other origin", declaration.DispatchCountryCodeInfo, messageError);

			invoiceLine.JI_CountryOfOrigin = RefCusCodeTestHelper.ValidDirectTransportationCountry;
			invoiceLine.JI_Tariff = "15123456099100";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			declaration.Validation.ValidateDispatchCountryCode();
			AssertNoMessageError("Other tariff", declaration.DispatchCountryCodeInfo, messageError);

			invoiceLine.JI_CountryOfOrigin = RefCusCodeTestHelper.ValidDirectTransportationCountry;
			invoiceLine.JI_Tariff = "25123456099100";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			declaration.Validation.ValidateDispatchCountryCode();
			AssertNoMessageError("Normal tariff", declaration.DispatchCountryCodeInfo, messageError);

			invoiceLine.JI_CountryOfOrigin = RefCusCodeTestHelper.ValidDirectTransportationCountry;
			invoiceLine.JI_Tariff = "25123456099100";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			declaration.Validation.ValidateDispatchCountryCode();
			AssertHasMessageError("All error conditions met", declaration.DispatchCountryCodeInfo, messageError);

			declaration.JE_DispatchCountryConfirmation = true;
			AssertNoMessageError("DispatchCountryConfirmation", declaration.DispatchCountryCodeInfo, messageError);
		});
	}

	public void TestCheckJE_OH_Importer()
	{
		string errorMessage = ValidationMessages.Plausi.MessageR168;

		var orgHeaderWithoutVAT = Factory.NewWithValidTestData<OrgHeader>();

		var orgHeaderWithVAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderWithVAT.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.RelocationProcedure;
			declaration.JE_OH_Importer = orgHeaderWithoutVAT.PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError($"{nameof(invoiceLine.JI_ZZF_NKTaxType)}={invoiceLine.JI_ZZF_NKTaxType} requires VAT no.", declaration.JE_OH_ImporterInfo, errorMessage);

			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.StandardRate;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError($"{nameof(invoiceLine.JI_ZZF_NKTaxType)}={invoiceLine.JI_ZZF_NKTaxType} does not require VAT no.", declaration.JE_OH_ImporterInfo, errorMessage);

			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.ProcessingTraffic;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError($"{nameof(invoiceLine.JI_ZZF_NKTaxType)}={invoiceLine.JI_ZZF_NKTaxType} requires VAT no.", declaration.JE_OH_ImporterInfo, errorMessage);

			declaration.JE_OH_Importer = orgHeaderWithVAT.PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError("Importer has VAT no.", declaration.JE_OH_ImporterInfo, errorMessage);
		});
	}

	public void TestCheckJE_OH_Importer_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Declaration.JE_OH_ImporterInfo);

	public void TestCheckJE_OH_Importer_CountryCHorLI()
	{
		var importerDocumentaryAddress = Declaration.ImporterDocumentaryAddress;

		CombineAssertions(() =>
		{
			var importer = Factory.New<OrgHeader>();
			Declaration.JE_OH_Importer = importer.PK;

			importerDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "DEBER";
			importerDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			Declaration.JE_OH_Importer = importer.PK;
			AssertHasMessageError(Declaration.JE_OH_ImporterInfo, ValidationMessages.Plausi.GetMessageR121("Importer"));

			importerDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "CHABL";
			importerDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			Declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageError(Declaration.JE_OH_ImporterInfo, ValidationMessages.Plausi.GetMessageR121("Importer"));

			importerDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "LIBAZ";
			importerDocumentaryAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Liechtenstein;
			Declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageError(Declaration.JE_OH_ImporterInfo, ValidationMessages.Plausi.GetMessageR121("Importer"));
		});
	}

	public void TestCheckJE_GS_NKCusAgent() => AssertCheckJE_GS_NKCusAgent(CHJobMessageTypeList.Codes.Import, true);

	void AssertPaymentRegistrationNumber(string customsCode, Action<JobDeclaration, string> payerSetter, ZPropertyInfo payerPropertyInfo)
	{
		string missingCode = $"Account was not found. Please check {customsCode} Type Registration Number in Config/Registration Numbers of the corresponding organization";

		var withoutCAD = CreateOrgHeader("ORG01", $"Organization without {customsCode}", null);
		var withCAD = CreateOrgHeader("ORG02", $"Organization with {customsCode}", customsCode);

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertPaymentRegistrationNumberCase(payerSetter, (d, v) => d.JE_OH_Supplier = v, payerPropertyInfo, withoutCAD.PK, withCAD.PK, DeclarationPayerList.Codes.Consignor, missingCode);
			AssertPaymentRegistrationNumberCase(payerSetter, (d, v) => d.JE_OH_Importer = v, payerPropertyInfo, withoutCAD.PK, withCAD.PK, DeclarationPayerList.Codes.Importer, missingCode);
			AssertPaymentRegistrationNumberCase(payerSetter, (d, v) => d.JE_OH_Consignee = v, payerPropertyInfo, withoutCAD.PK, withCAD.PK, DeclarationPayerList.Codes.Consignee, missingCode);
			AssertPaymentRegistrationNumberCase(payerSetter, (d, v) => d.JE_OH_Forwarder = v, payerPropertyInfo, withoutCAD.PK, withCAD.PK, DeclarationPayerList.Codes.Forwarder, missingCode);
		});
	}

	void AssertPaymentRegistrationNumberCase(Action<JobDeclaration, string> payerSetter, Action<JobDeclaration, ZGuid> addressSetter, ZPropertyInfo payerPropertyInfo, ZGuid withoutCAD, ZGuid withCAD, string paidBy, string missingCAD)
	{
		addressSetter(Declaration, withoutCAD);
		payerSetter(Declaration, paidBy);
		AssertHasMessageErrorContaining(payerPropertyInfo, missingCAD);
		addressSetter(Declaration, withCAD);
		AssertNoMessageErrors(payerPropertyInfo);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(payerPropertyInfo, "X", paidBy);
	}

	public void TestJE_TransportMode_CH0001() => AssertJE_TransportMode_CH0001(TransportTypeList.Codes.Sea);

	public void TestCheckJE_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		Declaration.JE_TransportMode = TransportTypeGenericList.Codes.Road;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertEquals("JE_RN_NKTransportNationality does have mandatory error message with JE_MessageType being IMP in ROA transportMode", true, Declaration.JE_RN_NKTransportNationalityInfo.HasMessageErrors());

		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		Declaration.Validation.ValidateJE_RN_NKTransportNationality();
		AssertEquals("JE_RN_NKTransportNationality does not have mandatory error message with JE_MessageType being EXP in ROA transportMode", false, Declaration.JE_RN_NKTransportNationalityInfo.HasMessageErrors());
	});

	public void TestJE_VATPaidBy_Import_Mandatory()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(jobDeclaration.JE_VATPaidByInfo, "XXX", DeclarationPayerList.Codes.Cash);
	}

	OrgHeader CreateOrgHeader(string companyCode, string fullName, string customsCode)
	{
		var org = Factory.New<OrgHeader>();
		org.OH_Code = companyCode;
		org.OH_FullName = fullName;
		if (!string.IsNullOrEmpty(customsCode))
		{
			org.CustomsCodes.AddNew(customsCode, "100000", Core.Constants.CountryCodes.Switzerland);
		}
		return org;
	}
}
