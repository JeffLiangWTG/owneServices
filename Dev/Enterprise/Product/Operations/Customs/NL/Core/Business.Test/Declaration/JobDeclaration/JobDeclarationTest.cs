using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
{
	public void TestDistinctCountriesOnInvoiceLines() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine1 = declaration.InvoiceLines.AddNew();
		var invoiceLine2 = declaration.InvoiceLines.AddNew();
		var invoiceLine3 = declaration.InvoiceLines.AddNew();
		var invoiceLine4 = declaration.InvoiceLines.AddNew();
		invoiceLine1.JI_RN_NKCountryOfExport = "BE";
		invoiceLine2.JI_RN_NKCountryOfExport = "DE";
		invoiceLine3.JI_RN_NKCountryOfExport = "DE";
		invoiceLine4.JI_RN_NKCountryOfExport = string.Empty;
		declaration.JE_RL_NKOrigin = "NLRTM";
		AssertEquals("count", 3, declaration.DistinctCountriesOnInvoiceLines.Count);
		AssertContainsExactElementsInAnyOrder("elements", ["BE", "DE", "NL"], declaration.DistinctCountriesOnInvoiceLines);
	});

	public void TestGetCusSupportingInfoTypes() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var supportingInfoTypeSupporter = (ICusSupportingInfoTypeSupporter)declaration;

		AssertEquals("FiscalReference", typeof(FiscalReference), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.FiscalReference]);
		AssertEquals("AdditionalInfo", typeof(AdditionalInfo), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		AssertEquals("SupportingDocument", typeof(SupportingDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		AssertEquals("PreviousDocument", typeof(PreviousDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	});

	public override void TestDefaultValuesForExport()
	{
		GlbDepartment.CurrentDepartment.GE_Import = false;
		GlbDepartment.CurrentDepartment.GE_Export = true;

		var declaration = Factory.New<JobDeclaration>();

		AssertEquals(ZString.Empty, declaration.JE_DeclarantType);
		AssertEquals("C", declaration.ZG_CTStatusID);
		AssertEquals("0", declaration.ZG_TypeOfSecurity);
	}

	public void TestSetDefaultValueForJE_DeclarantType()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

		OrgAddress representativeAddress = Factory.NewWithValidTestData<OrgAddress>();
		representativeAddress.OA_OH = orgHeader.PK;
		representativeAddress.OA_Code = "AAA";

		OrgAddress declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
		declarantAddress.OA_OH = orgHeader.PK;
		declarantAddress.OA_Code = "BBB";

		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		declaration.JE_OA_Representative = declarantAddress.PK;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("make represent equals declarant", RepresentationTypeList.Codes._3Indirect, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = null;
			declaration.JE_OA_Representative = representativeAddress.PK;
			Factory.Save();
			AssertEquals("make represent not equals declarant", RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = null;
			declaration.JE_OA_DeclarantAddress = representativeAddress.PK;
			Factory.Save();
			AssertEquals("make declarant equals represent", RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = null;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			Factory.Save();
			AssertEquals("make declarant not equals represent", RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = null;
			declaration.JE_OA_Representative = ZGuid.Empty;
			Factory.Save();
			AssertEquals("represant is empty", RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);
		});
	}

	public void TestJE_DeclarantTypeReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("Representative Type is read only", true, declaration.JE_DeclarantTypeInfo.ReadOnly);
	}

	public void TestLookups_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationLookups>(declaration.Lookups);
	}

	public void TestLookups_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<JobDeclarationLookups>(declaration.Lookups);
	}

	public void TestLookups_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobDeclarationLookups>(declaration.Lookups);
	}

	public void TestValidation_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationValidation>(declaration.Validation);
	}

	public void TestValidation_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationValidation>(declaration.Validation);
	}

	public void TestValidation_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobDeclarationValidation>(declaration.Validation);
	}

	public void TestInvoices()
	{
		AssertType<InvoiceHeaderActiveCollection>(declaration.Invoices);
	}

	public void TestInvoiceLines()
	{
		AssertType<InvoiceLineCompleteCollection>(declaration.InvoiceLines);
	}

	public void TestJobComInvoiceGroupHeaders()
	{
		AssertType<BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>>(declaration.JobComInvoiceGroupHeaders);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Netherlands, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
	}

	public override void TestAreMultipleEntryInstructionsAllowed()
	{
		AssertEquals(true, declaration.AreMultipleEntryInstructionsAllowed);
	}

	public void TestEORIValue()
	{
		var orgHeaderControllingAgent = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderControllingAgent, Core.Constants.CountryCodes.Netherlands, "7654321");
		orgHeaderControllingAgent.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		OrgAddress addressControllingAgent = Factory.New<OrgAddress>();
		addressControllingAgent.OA_Code = "AAA";
		addressControllingAgent.OA_OH = orgHeaderControllingAgent.PK;
		addressControllingAgent.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		var orgHeaderImporter = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderImporter, Core.Constants.CountryCodes.Netherlands, "1234567");
		orgHeaderImporter.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		OrgAddress addressImporter = Factory.New<OrgAddress>();
		addressImporter.OA_Code = "BBB";
		addressImporter.OA_OH = orgHeaderImporter.PK;
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		var orgHeaderDeclarant = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderDeclarant, Core.Constants.CountryCodes.Netherlands, "89123456");
		orgHeaderDeclarant.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		OrgAddress addressDeclarant = Factory.New<OrgAddress>();
		addressDeclarant.OA_Code = "CCC";
		addressDeclarant.OA_OH = orgHeaderDeclarant.PK;
		addressDeclarant.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		var orgHeaderDeferment = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderDeferment, Core.Constants.CountryCodes.Netherlands, "65432198");
		orgHeaderDeferment.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		OrgAddress addressDeferment = Factory.New<OrgAddress>();
		addressDeferment.OA_Code = "DDD";
		addressDeferment.OA_OH = orgHeaderDeferment.PK;
		addressDeferment.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_ControllingAgent = orgHeaderControllingAgent.PK;
		declaration.JE_OA_ImporterAddress = addressImporter.PK;
		declaration.JE_OH_Importer = orgHeaderImporter.PK;
		declaration.JE_OA_DeclarantAddress = addressDeclarant.PK;
		declaration.DefermentPartyDocAddress.OrganisationPK = orgHeaderDeferment.PK;
		declaration.JE_DeclarantType = ZString.Empty;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		AssertEquals("NL1234567", declaration.PaymentPartyEORINumber);
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		AssertEquals("NL7654321", declaration.PaymentPartyEORINumber);
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountStandingAuthority;
		AssertEquals("NL7654321", declaration.PaymentPartyEORINumber);
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration;
		AssertEquals("NL7654321", declaration.PaymentPartyEORINumber);
		declaration.JE_PaymentMethod = "";
		AssertNullOrEmpty(declaration.PaymentPartyEORINumber);

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		AssertEquals("NL7654321", declaration.PaymentPartyEORINumber);

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		declaration.JE_OA_DeclarantAddress = addressDeclarant.PK;
		AssertEquals("Direct", "NL89123456", declaration.PaymentPartyEORINumber);

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		declaration.JE_OA_DeclarantAddress = addressDeclarant.PK;
		AssertEquals("Indirect", "NL89123456", declaration.PaymentPartyEORINumber);

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		AssertEquals("NL65432198", declaration.PaymentPartyEORINumber);

		declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		declaration.JE_OA_DeclarantAddress = addressDeclarant.PK;
		AssertEquals("Self", "NL89123456", declaration.PaymentPartyEORINumber);
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		declaration.JE_OA_DeclarantAddress = addressDeclarant.PK;
		AssertEquals("Consignees Account", "NL89123456", declaration.PaymentPartyEORINumber);
	}

	public void TestPaymentPartyEoriNumber_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Payment Party EORI Number - Caption", "Payment Party EORI Number", DataBoundResourceStrings.GetDataForProperty(declaration.PaymentPartyEORINumberInfo).Caption);
		AssertEquals("Payment Party EORI Number - Medium Caption", "EORI Number", DataBoundResourceStrings.GetDataForProperty(declaration.PaymentPartyEORINumberInfo).MediumCaption);
	}

	public void TestVATValue()
	{
		var orgHeaderDeferment = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderDeferment, Core.Constants.CountryCodes.Netherlands, "1234567", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Netherlands));
		orgHeaderDeferment.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		OrgAddress addressDeferment = Factory.New<OrgAddress>();
		addressDeferment.OA_Code = "AAA";
		addressDeferment.OA_OH = orgHeaderDeferment.PK;
		addressDeferment.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.DefermentPartyDocAddress.OrganisationPK = orgHeaderDeferment.PK;
		declaration.ZG_VATDeferType = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		AssertEquals("NL1234567", declaration.VATPartyTaxNumber);
		declaration.ZG_VATDeferType = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		AssertEquals("NL1234567", declaration.VATPartyTaxNumber);
		declaration.ZG_VATDeferType = DefermentMethodList.Codes.ConsigneesAccountStandingAuthority;
		AssertEquals("NL1234567", declaration.VATPartyTaxNumber);
		declaration.ZG_VATDeferType = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration;
		AssertEquals("NL1234567", declaration.VATPartyTaxNumber);

		AddCustomsCodeForTest(orgHeaderDeferment, Core.Constants.CountryCodes.Netherlands, "159357B02", OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode);
		var authorisation = Factory.New<CusAuthorisationHeader>();
		authorisation.CPH_OH_PermitHolder = orgHeaderDeferment.PK;
		authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
		authorisation.CPH_EndDate = ZDate.Today.AddDays(1);
		authorisation.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.LFR;
		authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		authorisation.CPH_Number = "159357B02";

		AssertEquals("NL159357B02", declaration.VATPartyTaxNumber);
	}

	public void TestVATPartyTaxNumber_Caption()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("VAT Party Tax Number - Caption", "VAT Party Tax Number", DataBoundResourceStrings.GetDataForProperty(declaration.VATPartyTaxNumberInfo).Caption);
		AssertEquals("VAT Party Tax Number - Medium Caption", "VAT Number", DataBoundResourceStrings.GetDataForProperty(declaration.VATPartyTaxNumberInfo).MediumCaption);
	}

	public override void TestGetCustomsEntryInstructionProviderCore()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType(typeof(EntryInstructionProvider), dec.CustomsEntryInstructionProvider);
	}

	public void TestCustomsEntryInstructions()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<CusEntryInstructionCollection>(dec.CustomsEntryInstructions);
	}

	public void TestBorderTransport()
	{
		AssertType<BorderTransportCollection>(declaration.BorderTransports);
	}

	public override void TestGetCusCodeDataType() => CombineAssertions(() =>
	{
		AssertEquals("TPI", typeof(InlandTransport), ((ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[EU.Business.CusCodeDataTypeList.Codes.TransportInland]);
		AssertEquals("TPB", typeof(BorderTransport), ((ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.TransportAtBorder]);
	});

	public void TestPaymentMethodDefaultValue()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var org = Factory.New<OrgHeader>();
		var orgImpAddInfo = RegionOrgImpAddInfo.Get(org, declaration.CountryCode);
		orgImpAddInfo.ZO_OtherDeferType = "A";
		declaration.JE_OH_Importer = org.PK;

		AssertContains(orgImpAddInfo.ZO_OtherDeferType, declaration.JE_PaymentMethod);
	}

	public override void TestCustomsOfficeRequirementHelper()
	{
		AssertType<JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
	}

	public void TestInlandTransports()
	{
		AssertType<InlandTransportCollection>(declaration.InlandTransports);
	}

	public void TestFiscalReferences()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertType<FiscalReferenceCollection>(dec.FiscalReferences);
	}

	public void TestAdditionalInfo()
	{
		AssertType<AdditionalInfoCollection>(declaration.AdditionalInfos);
	}

	public void TestSupportingDocumentCollectionType()
	{
		AssertType<SupportingDocumentCollection>(declaration.SupportingDocuments);
	}

	public void TestPreviousDocumentCollectionType()
	{
		AssertType<PreviousDocumentCollection>(declaration.PreviousDocuments);
	}

	public void TestImporterDocumentaryAddressChanged_NoLFR()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var orgHeaderImporter = Factory.New<OrgHeader>();
		orgHeaderImporter.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		orgHeaderImporter.OH_FullName = "Importername";
		OrgAddress addressImporter = Factory.New<OrgAddress>();
		addressImporter.OA_Code = "BBB";
		addressImporter.OA_OH = orgHeaderImporter.PK;
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertEquals("Deferment Party Doc Address should not be filled when importer has no VAT", ZGuid.Empty, declaration.DefermentPartyDocAddress.E2_OA_Address);

		AddCustomsCodeForTest(orgHeaderImporter, Core.Constants.CountryCodes.France, "123456", OrgCusCode.FranceCodeTypes.TVA);
		declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertEquals("Deferment Party Doc Address should not be filled when importer has a foreign VAT", ZGuid.Empty, declaration.DefermentPartyDocAddress.E2_OA_Address);

		orgHeaderImporter.CustomsCodes.RemoveAll();
		AddCustomsCodeForTest(orgHeaderImporter, Core.Constants.CountryCodes.Netherlands, "654321", OrgCusCode.EuropeanUnionSharedCodeTypes.BTW);
		declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertEquals("Deferment Party Doc Address should be filled when importer has only a NL VAT", addressImporter.PK, declaration.DefermentPartyDocAddress.E2_OA_Address);
	}

	public void TestImporterDocumentaryAddressChanged_LFR()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var orgHeaderImporter = Factory.New<OrgHeader>();
		orgHeaderImporter.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		orgHeaderImporter.OH_FullName = "Importername";
		OrgAddress addressImporter = Factory.New<OrgAddress>();
		addressImporter.OA_Code = "BBB";
		addressImporter.OA_OH = orgHeaderImporter.PK;
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		AddCustomsCodeForTest(orgHeaderImporter, Core.Constants.CountryCodes.France, "123456", OrgCusCode.FranceCodeTypes.TVA);

		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertEquals("Deferment Party Doc Address should not be filled if importer has a foreign VAT", ZGuid.Empty, declaration.DefermentPartyDocAddress.E2_OA_Address);
		AssertEquals("VAT Party Tax number should not be filled", ZString.Empty, declaration.VATPartyTaxNumber);

		AddCustomsCodeForTest(orgHeaderImporter, Core.Constants.CountryCodes.Netherlands, "159357B02", OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode);

		var authorisation = Factory.New<CusAuthorisationHeader>();
		authorisation.CPH_OH_PermitHolder = orgHeaderImporter.PK;
		authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
		authorisation.CPH_EndDate = ZDate.Today.AddDays(1);
		authorisation.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.LFR;
		authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		authorisation.CPH_Number = "159357B02";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = addressImporter.PK;
		AssertEquals("Deferment Party Doc Address should be filled when importer has a NL LFR", declaration.DefermentPartyDocAddress.E2_OA_Address, addressImporter.PK);
		AssertEquals("VAT Party Tax number should be the LFR-number", "NL159357B02", declaration.VATPartyTaxNumber);
	}

	#region CustomsAccount
	public void TestCustomsAccountReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("Customs account is read only", true, declaration.CustomsAccountInfo.ReadOnly);
	}

	public void TestCustomsAccountValue()
	{
		var orgHeaderControllingAgent = Factory.NewWithValidTestData<OrgHeader>();
		var orgHeaderDeclarant = Factory.NewWithValidTestData<OrgHeader>();
		var addressOrgHeaderDeclarant = Factory.New<OrgAddress>();
		addressOrgHeaderDeclarant.OA_Address1 = "AD1";
		addressOrgHeaderDeclarant.OA_OH = orgHeaderDeclarant.PK;
		var collection = new SenderInfoCollection();
		var senderInfo1 = collection.AddNew();
		senderInfo1.OrganizationPK = orgHeaderControllingAgent.PK;
		senderInfo1.SenderID = "123";
		senderInfo1.DefaultSenderID = true;
		var senderInfo2 = collection.AddNew();
		senderInfo2.OrganizationPK = orgHeaderDeclarant.PK;
		senderInfo2.SenderID = "456";
		senderInfo2.DefaultSenderID = false;
		Factory.Save();

		NLCustomsRegistry.Instance.SenderIDs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_OH_ControllingAgent = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_DeclarantType = "";
		AssertEquals("Customs Account should be empty with no Rep Type selected", ZString.Empty, declaration.CustomsAccount);

		declaration.JE_DeclarantType = "DIR";
		AssertEquals("Customs Account should be empty with Rep Type DIR and no Controlling Agent", ZString.Empty, declaration.CustomsAccount);

		declaration.JE_OH_ControllingAgent = orgHeaderControllingAgent.PK;
		AssertEquals("Customs Account should be filled with Rep Type DIR and Controlling Agent filled", senderInfo1.SenderID, declaration.CustomsAccount);

		declaration.JE_DeclarantType = "IND";
		AssertEquals("Customs Account should be empty with Rep Type IND and no Declarant", ZString.Empty, declaration.CustomsAccount);

		declaration.JE_OA_DeclarantAddress = addressOrgHeaderDeclarant.PK;
		AssertEquals("Customs Account should be filled with Rep Type IND and Declarant filled", senderInfo2.SenderID, declaration.CustomsAccount);

		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_DeclarantType = "SEL";
		AssertEquals("Customs Account should be empty with Rep Type SEL and no Declarant", ZString.Empty, declaration.CustomsAccount);

		declaration.JE_OA_DeclarantAddress = addressOrgHeaderDeclarant.PK;
		AssertEquals("Customs Account should be filled with Rep Type SEL and Declarant filled", senderInfo2.SenderID, declaration.CustomsAccount);
	}
	#endregion

	public void TestJE_ShipmentIncoTermPlace_OnValidUnloco()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ShipmentIncoTermPlace = "abc";
			declaration.EUD_AgreedPlaceCode = Core.Constants.CountryCodes.Netherlands;
			AssertEquals("AgreedPlaceCode is country", "abc", declaration.JE_ShipmentIncoTermPlace);

			declaration.EUD_AgreedPlaceCode = "NLAAA";
			AssertEquals("Invalid Unloco Code", "abc", declaration.JE_ShipmentIncoTermPlace);

			declaration.JE_ShipmentIncoTermPlace = "abc";
			declaration.EUD_AgreedPlaceCode = "NLAMS";
			AssertEquals("AgreedPlaceCode enabled - Valid Unloco Code", ZString.Empty, declaration.JE_ShipmentIncoTermPlace);
		});
	}

	public override void TestZG_SpecificCircumstanceIndicator()
	{
		var dec = Factory.New<JobDeclaration>();
		AssertEquals(3, dec.ZG_SpecificCircumstanceIndicatorInfo.MaxLength);
	}

	public void TestPresentationStartDate()
	{
		AssertEquals("Start", Factory.New<JobDeclaration>().ZG_PresentationStartDateInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public void TestPresentationEndDate()
	{
		AssertEquals("End", Factory.New<JobDeclaration>().ZG_PresentationEndDateInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public void TestJE_ControllingCustomer_Caption()
	{
		AssertEquals("Controlling Customer - Caption", "Controlling Customer", DataBoundResourceStrings.GetDataForProperty(declaration.JE_OH_ControllingCustomerInfo).Caption);
	}

	public override void TestJE_TransportMode_Caption()
	{
		AssertCaptionAndFullDescription(declaration.JE_TransportModeInfo, "Transport", "[UCC 7/4] Transport");
	}

	public void TestJE_ShipmentIncoTerm_Caption()
	{
		AssertEquals("[UCC 4/1] Incoterm - Caption", "[UCC 4/1] Incoterm", DataBoundResourceStrings.GetDataForProperty(declaration.JE_ShipmentIncoTermInfo).Caption);
	}

	public void TestJE_UCR_Caption()
	{
		AssertEquals("[UCC 2/4] DUCR  - Caption", "[UCC 2/4] DUCR", DataBoundResourceStrings.GetDataForProperty(declaration.JE_UCRInfo).Caption);
	}

	public void TestJE_RL_NKOrigin_Caption()
	{
		AssertEquals("[UCC 5/14] Dispatch - Caption", "[UCC 5/14] Dispatch", DataBoundResourceStrings.GetDataForProperty(declaration.JE_RL_NKOriginInfo).Caption);
	}

	public void TestJE_RL_NKFinalDestination_Caption()
	{
		AssertCaptionAndFullDescription(declaration.JE_RL_NKFinalDestinationInfo, "Destination", "[UCC 5/8] Destination");
	}

	public void TestJE_RN_NKTransportNationality_Caption()
	{
		AssertCaptionAndFullDescription(declaration.JE_RN_NKTransportNationalityInfo, "Nationality", "[UCC 7/8] Nationality");
	}

	public void TestZG_SpecificCircumstanceIndicator_Caption()
	{
		AssertCaptionAndFullDescription(declaration.ZG_SpecificCircumstanceIndicatorInfo, "Circumstance", "[UCC 1/7] Circumstance");
	}

	public void TestJE_VesselName_Caption()
	{
		AssertCaptionAndFullDescription(declaration.JE_VesselNameInfo, "Vessel", "[UCC 7/7] Vessel");
	}

	public void TestJE_DeclarantType_Caption()
	{
		AssertEquals("Declarant Type - Caption", "[UCC 3/21] Rep. Type", DataBoundResourceStrings.GetDataForProperty(declaration.JE_DeclarantTypeInfo).FullDescription);
		AssertEquals("Declarant Type - Caption", "Rep. Type", DataBoundResourceStrings.GetDataForProperty(declaration.JE_DeclarantTypeInfo).Caption);
	}

	public void TestJE_MessageTypeChanged_ClearInvoiceHeaderFields()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			invoiceHeader.JZ_UCR = "ucr";
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "C";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("InvoiceHeader JZ_UCR should be empty", ZString.Empty, invoiceHeader.JZ_UCR);
			AssertEquals("InvoiceHeader ZG_TransportChargesMethodOfPayment should be empty", ZString.Empty, invoiceHeader.ZG_TransportChargesMethodOfPayment);

			invoiceHeader.JZ_UCR = "ucr";
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "C";
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("InvoiceHeader JZ_UCR should not be cleared", "ucr", invoiceHeader.JZ_UCR);
			AssertEquals("InvoiceHeader ZG_TransportChargesMethodOfPayment should not be cleared", "C", invoiceHeader.ZG_TransportChargesMethodOfPayment);
		});
	}

	public void TestJE_MessageTypeChanged_ClearItineraryCountries()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			declaration.ItineraryCountries.AddNew();
			AssertEquals("export 1 added", 1, declaration.ItineraryCountries.Count);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("changed to import", 0, declaration.ItineraryCountries.Count);
		});
	}

	public void TestJE_MessageTypeChanged_ClearInvoiceLineOrganizations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
		consigneeAddress.OA_OH = consignee.PK;
		consigneeAddress.OA_Code = "AAA";

		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		var consignorAddress = Factory.NewWithValidTestData<OrgAddress>();
		consignorAddress.OA_OH = consignor.PK;
		consignorAddress.OA_Code = "BBB";

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		invoiceLine.JI_OA_ConsigneeAddress = consigneeAddress.PK;
		invoiceLine.JI_OA_ExporterAddress = consignorAddress.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Export - Consignee filled on invoice line", consigneeAddress.PK, invoiceLine.JI_OA_ConsigneeAddress);
			AssertEquals("Export - Consignor filled on invoice line", consignorAddress.PK, invoiceLine.JI_OA_ExporterAddress);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import - Consignee cleared on invoice line", ZGuid.Empty, invoiceLine.JI_OA_ConsigneeAddress);
			AssertEquals("Import - Consignor cleared on invoice line", ZGuid.Empty, invoiceLine.JI_OA_ExporterAddress);
		});
	}

	public void TestJE_RN_NKTransportNationalityDisabled()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertEquals("JE_RN_NKTransportNationalityInfo should not be readonly for transport mode empty", false, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.Air;
			AssertEquals("JE_RN_NKTransportNationalityInfo should not be readonly for transport mode air", false, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			AssertEquals("JE_RN_NKTransportNationalityInfo should be readonly for transport mode fix", true, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.InlandWaterwayTransport;
			AssertEquals("JE_RN_NKTransportNationalityInfo should not be readonly for transport mode iwt", false, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.Mail;
			AssertEquals("JE_RN_NKTransportNationalityInfo should be readonly for transport mode mail", true, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.OwnPropulsion;
			AssertEquals("JE_RN_NKTransportNationalityInfo should not be readonly for transport mode own", false, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.Rail;
			AssertEquals("JE_RN_NKTransportNationalityInfo should be readonly for transport mode rail", true, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.Road;
			AssertEquals("JE_RN_NKTransportNationalityInfo should not be readonly for transport mode road", false, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
			declaration.JE_TransportMode = TransportModes.Sea;
			AssertEquals("JE_RN_NKTransportNationalityInfo should not be readonly for transport mode sea", false, declaration.JE_RN_NKTransportNationalityInfo.ReadOnly);
		});
	}

	public void TestTransportIDInlandDisabled()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			AssertEquals("JE_TransportIDInlandInfo should not be readonly for transport mode empty", false, declaration.JE_TransportIDInlandInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.Rail;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "10";
			AssertEquals("JE_TransportIDInlandInfo should be readonly for transport mode Rail and procedure code 10", true, declaration.JE_TransportIDInlandInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.Mail;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "11";
			AssertEquals("JE_TransportIDInlandInfo should be readonly for transport mode Mail procedure code 11", true, declaration.JE_TransportIDInlandInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "23";
			AssertEquals("JE_TransportIDInlandInfo should be readonly for transport mode FixedTransportInstallations and procedure code 23", true, declaration.JE_TransportIDInlandInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "31";
			AssertEquals("JE_TransportIDInlandInfo should be readonly for transport mode FixedTransportInstallations and procedure code 31", true, declaration.JE_TransportIDInlandInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.Mail;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76";
			AssertEquals("JE_TransportIDInlandInfo should be readonly for transport mode Mail procedure code 76", true, declaration.JE_TransportIDInlandInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "77";
			AssertEquals("JE_TransportIDInlandInfo should be readonly for transport mode FixedTransportInstallations and procedure code 77", true, declaration.JE_TransportIDInlandInfo.ReadOnly);
		});
	}

	public void TestTransportMeansDisabled()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			AssertEquals("JE_TransportMeansInfo should not be readonly for transport mode empty", false, declaration.JE_TransportMeansInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.Rail;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "10";
			AssertEquals("JE_TransportMeansInfo should be readonly for transport mode Rail and procedure code 10", true, declaration.JE_TransportMeansInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.Mail;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "11";
			AssertEquals("JE_TransportMeansInfo should be readonly for transport mode Mail procedure code 11", true, declaration.JE_TransportMeansInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "23";
			AssertEquals("JE_TransportMeansInfo should be readonly for transport mode FixedTransportInstallations and procedure code 23", true, declaration.JE_TransportMeansInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "31";
			AssertEquals("JE_TransportMeansInfo should be readonly for transport mode FixedTransportInstallations and procedure code 31", true, declaration.JE_TransportMeansInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.Mail;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76";
			AssertEquals("JE_TransportMeansInfo should be readonly for transport mode Mail procedure code 76", true, declaration.JE_TransportMeansInfo.ReadOnly);

			declaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			declaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "77";
			AssertEquals("JE_TransportMeansInfo should be readonly for transport mode FixedTransportInstallations and procedure code 77", true, declaration.JE_TransportMeansInfo.ReadOnly);
		});
	}

	public void TestCustomSenderInfoForCustomsAccount()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		var collection = new SenderInfoCollection();
		var senderInfo = collection.AddNew();
		senderInfo.OrganizationPK = orgHeader.PK;
		senderInfo.SenderID = "123";
		senderInfo.DefaultSenderID = true;
		Factory.Save();

		NLCustomsRegistry.Instance.SenderIDs.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, collection);

		CombineAssertions(() =>
		{
			AssertEquals(senderInfo.SenderID, declaration.GetSenderInfoCustomsAccount(orgHeader.PK));
			AssertEquals(ZString.Empty, declaration.GetSenderInfoCustomsAccount(orgHeader2.PK));
		});
	}

	public void TestHasProcedureIntoWarehouse()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "74", "79", "", "Desc", "IMP", intoWarehouse: true, group: "VZL");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals(false, declaration.HasProcedureIntoWarehouse);

			instruction.CEI_Style = "EZL";
			invoiceLine.JI_Procedure = "7479";
			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			AssertEquals(true, declaration.HasProcedureIntoWarehouse);
		});
	}

	public void TestHasProcedureOutOfWarehouse()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "72", "78", "", "Desc", "IMP", outOfWarehouse: true, group: "EZL");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals(false, declaration.HasProcedureOutOfWarehouse);

			instruction.CEI_Style = "EZL";
			invoiceLine.JI_Procedure = "7278";
			instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			AssertEquals(true, declaration.HasProcedureOutOfWarehouse);
		});
	}

	public void TestGetFetchStrategyCore()
	{
		AssertType<JobDeclarationFetchStrategy>(GetJobDeclaration().FetchStrategy);
	}

	public void TestHasInvoiceLineWithC9008Procedure()
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Procedure on invoiceLine is empty, does not begin with 10, 11, 23, 31", false, declaration.HasInvoiceLineWithC9008Procedure);
			invoiceLine.JI_Procedure = "1200";
			AssertEquals("Procedure on invoiceLine does not begin with 10, 11, 23 or 31", false, declaration.HasInvoiceLineWithC9008Procedure);
			invoiceLine.JI_Procedure = "1000";
			AssertEquals("Procedure on invoiceLine begins with 10", true, declaration.HasInvoiceLineWithC9008Procedure);
			invoiceLine.JI_Procedure = "1100";
			AssertEquals("Procedure on invoiceLine begins with 11", true, declaration.HasInvoiceLineWithC9008Procedure);
			invoiceLine.JI_Procedure = "2300";
			AssertEquals("Procedure on invoiceLine begins with 23", true, declaration.HasInvoiceLineWithC9008Procedure);
			invoiceLine.JI_Procedure = "3100";
			AssertEquals("Procedure on invoiceLine begins with 31", true, declaration.HasInvoiceLineWithC9008Procedure);
		});
	}

	public void TestJobDeclarationValueSetStrategy()
	{
		CombineAssertions(() =>
		{
			var declarationForTestting = GetJobDeclarationForTesting();
			var declarationValueSetStrategy = declarationForTestting.GetValueSetStrategyExposed();
			AssertType<JobDeclarationValueSetStrategy>("Type NL", declarationValueSetStrategy);
			AssertSame("Cached", declarationValueSetStrategy, declarationForTestting.GetValueSetStrategyExposed());
		});
	}

	public override void TestDefaultCTStatusIDSwitchingBetweenExportAndImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertEquals("C", declaration.ZG_CTStatusID);
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals(ZString.Empty, declaration.ZG_CTStatusID);
	}

	public void TestShipmentSynchronizer()
	{
		declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
		AssertType<JobDeclarationSynchroniser>(declaration.ShipmentSynchroniser);
	}

	public void TestJE_OA_Representative_ReadOnly()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._3Indirect;
			AssertEquals("Representative is read only for indirect representation", true, declaration.JE_OA_RepresentativeInfo.ReadOnly);
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
			AssertEquals("Representative is read only for self representation", true, declaration.JE_OA_RepresentativeInfo.ReadOnly);
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._2Direct;
			AssertEquals("Representative is available for direct representation", false, declaration.JE_OA_RepresentativeInfo.ReadOnly);
		});
	}

	public void TestSetDeclarantTypesAndParties()
	{
		var supplierWithIndirectRepresentation = Factory.NewWithValidTestData<OrgHeader>();
		var supplierAddInfo = EUOrgImpAddInfo.Get(supplierWithIndirectRepresentation, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		supplierAddInfo.Deserialise();
		supplierAddInfo.ZO_Box14UseIndirectRepresentationForExporter = true;
		Factory.Save();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var supplier = Factory.NewWithValidTestData<OrgHeader>();

		var supplierWithDirectRepresentation = Factory.New<OrgHeader>();

		CombineAssertions(() =>
		{
			declaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.SetDeclarantTypeFromSupplier();
			AssertEquals("SELF - Declarant type should be SEL", "SEL", declaration.JE_DeclarantType);
			AssertEquals("SELF - Declarant Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_DeclarantAddress);
			AssertEquals("SELF - Exporter Address equals to login broker", declaration.Branch?.OrgProxy?.PK, declaration.ExporterDocAddress.OrganisationPK);
			AssertEquals("SELF - Representative Address is empty", ZGuid.Empty, declaration.JE_OA_Representative);

			declaration.SupplierDocumentaryAddress.OrganisationPK = supplierWithDirectRepresentation.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.SetDeclarantTypeFromSupplier();
			AssertEquals("DIR - Declarant type should be DIR", "DIR", declaration.JE_DeclarantType);
			AssertEquals("DIR - Declarant Address equals to supplier", declaration.SupplierDocumentaryAddress.E2_OA_Address, declaration.JE_OA_DeclarantAddress);
			AssertEquals("DIR - Exporter Address equals to supplier", declaration.SupplierDocumentaryAddress.OrganisationPK, declaration.ExporterDocAddress.OrganisationPK);
			AssertEquals("DIR - Representative Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_Representative);

			declaration.SupplierDocumentaryAddress.OrganisationPK = supplierWithIndirectRepresentation.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.SetDeclarantTypeFromSupplier();
			AssertEquals("IND - Declarant type should be IND", "IND", declaration.JE_DeclarantType);
			AssertEquals("IND - Declarant Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_DeclarantAddress);
			AssertEquals("IND - Exporter Address equals to supplier", declaration.SupplierDocumentaryAddress.OrganisationPK, declaration.ExporterDocAddress.OrganisationPK);
			AssertEquals("IND - Representative Address equals to login broker", declaration.Branch.OrgProxy?.MainAddress?.PK, declaration.JE_OA_Representative);
		});
	}

	public void TestContainerControlCheckboxVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("ContainerControlCheckboxVisible is set to true for NL", true, declaration.ContainerControlCheckboxVisible);
	}

	public void TestContainerUnloadedCheckboxVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("ContainerUnloadedCheckboxVisible is set to true for NL", true, declaration.ContainerUnloadedCheckboxVisible);
	}

	public void TestEntrySelections()
	{
		AssertType<EntrySelectionCollection>(declaration.EntrySelections);
	}

	protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;

	JobDeclarationForTesting GetJobDeclarationForTesting()
	{
		var dec = Factory.New<JobDeclarationForTesting>();
		dec.DisableDefaultPackingInformation = true;
		dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		return dec;
	}

	OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo, string codeType = null)
	{
		OrgCusCode taxCode = organisation.CustomsCodes.AddNew();
		taxCode.OK_RN_NKCodeCountry = country;
		taxCode.OK_CodeType = codeType ?? OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		taxCode.OK_CustomsRegNo = customsRegNo;
		return taxCode;
	}

	void AssertCaptionAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, string messageType = "")
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null, [messageType == "IMP" ? JobDeclaration.CaptionKeyImportUCC6 : messageType == "EXP" ? JobDeclaration.CaptionKeyExportUCC6 : null]);

		CombineAssertions(() =>
		{
			AssertEquals($"{propertyInfo.Name} {messageType} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} {messageType} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		});
	}

	protected override ZString DefaultBorderTransportModeForSea => "11";

	#region JobDeclarationForTesting

	public class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed
		{
			get { return LocalCurrencyCodeCore; }
		}

		public IValueSetStrategy GetValueSetStrategyExposed() => base.GetValueSetStrategy();
	}

	#endregion
}
