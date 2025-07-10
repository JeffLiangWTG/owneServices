using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryCreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
{
	public void TestCreateEntryCreationStrategy()
	{
		var entryCreationStrategy = declaration.CreateEntryCreationStrategy();
		AssertEquals("EntryCreationStrategy", typeof(EntryCreationStrategy), entryCreationStrategy.GetType());
	}

	public override void TestGetKeyForHeader()
	{
		base.TestGetKeyForHeader();

		SetUpRefData();
		PopulateDeclarationDataForGetKeyForHeader();

		PopulateChildCollectionsWhichShouldNotAffectMergeKey();

		var expectedKeyForHeader = GetExpectedKeyForGetKeyForHeader();
		var entryCreationStrategy = declaration.CreateEntryCreationStrategy();
		var actualKeyForHeader = entryCreationStrategy.GetKeyForHeader(invoiceLine).Keys.ToArray();
		AssertArrayEqualsByElements(expectedKeyForHeader, actualKeyForHeader);
	}

	public void TestGetKeyForHeader_ExportUCC6()
	{
		try
		{
			additionalDeclarationSetup = (dec) =>
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.MessageVersion = MessageVersionList.Codes.XML;
			};
			base.TestGetKeyForHeader();

			SetUpRefData();
			PopulateDeclarationDataForGetKeyForHeader();

			PopulateChildCollectionsWhichShouldNotAffectMergeKey();
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var expectedKeyForHeader = GetExpectedKeyForExportUcc6GetKeyForHeader();
				var actualKeyForHeader = declaration.CreateEntryCreationStrategy().GetKeyForHeader(invoiceLine).Keys.ToArray();
				AssertArrayEqualsByElements(expectedKeyForHeader, actualKeyForHeader);
			}
		}
		finally
		{
			additionalDeclarationSetup = null;
		}
	}

	public void TestGetKeyForLine()
	{
		SetUpRefData();
		PopulateDeclarationDataForGetKeyForHeader();
		PopulateDeclarationDataForGetKeyForLineFromShared();
		PopulateDeclarationDataForGetKeyForLineFromEu();
		PopulateDeclarationDataForGetKeyForLineFromIt();
		PopulateChildCollectionsWhichShouldNotAffectMergeKey();

		AssertKeysForLine();
	}

	public void TestGetKeyForLine_ExportUCC6()
	{
		SetUpRefData();
		PopulateDeclarationDataForGetKeyForHeader();
		PopulateDeclarationDataForGetKeyForLineFromShared();
		PopulateDeclarationDataForGetKeyForLineFromEu();
		PopulateDeclarationDataForGetKeyForLineFromIt();
		PopulateChildCollectionsWhichShouldNotAffectMergeKey();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "IsUCC6Core", true, declaration.GetDefaultDataGroupingCode()))
		{
			AssertKeysForLine();
		}
	}

	public void TestMergeKeyForSupportingDocs()
	{
		var dec = Factory.New<JobDeclaration>();
		var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		var inv1 = dec.Invoices.AddNew();
		var invLine = inv1.JobComInvoiceLines.AddNew();
		invLine.SupportingDocuments.AddNew();

		invLine.JI_CEI = cei.PK;
		var entryCreationStrategy = (EntryCreationStrategy)dec.CreateEntryCreationStrategy();
		var supportingDocumentKeys = entryCreationStrategy.GetSupportingDocumentKeys();
		AssertArrayEqualsByElements(supportingDocumentKeys, GetExpectedSupportingDocumentKeys());
	}

	void AssertKeysForLine()
	{
		CombineAssertions("Assertions for GetKeyForHeader prefix", () =>
		{
			var entryCreationStrategy = declaration.CreateEntryCreationStrategy();
			var actualKeyForLine = entryCreationStrategy.GetKeyForLine(invoiceLine).Keys;
			var actualKeyForHeader = actualKeyForLine.Take(KeysFromGetKeyForHeaderPrefixCount).ToArray();

			var expectedKeyForHeader = GetExpectedKeyForHeader();
			AssertArrayEqualsByElements(expectedKeyForHeader, actualKeyForHeader);
		});

		AssertKeyForLine(string.Empty, invoiceLine.PK);
		AssertKeyForLine("NON", invoiceLine.PK);
		AssertKeyForLine("NOP", invoiceLine.PK);
		AssertKeyForLine("TRF", invoiceLine.JI_Tariff);
		AssertKeyForLine("TRM", invoiceLine.JI_Tariff);
		AssertKeyForLine("TRD", invoiceLine.JI_Description, invoiceLine.JI_Tariff);
		AssertKeyForLine("CLS", classification.CC_LookupCode, invoiceLine.JI_Tariff);
		AssertKeyForLine("CLD", classification.CC_LookupCode, invoiceLine.JI_Tariff);
		AssertKeyForLine("PNO", invoiceLine.JI_PartNo, classification.CC_LookupCode, invoiceLine.JI_Tariff);
		AssertKeyForLine("PNP", invoiceLine.JI_PartNo, classification.CC_LookupCode, invoiceLine.JI_Tariff);
	}

	void PopulateDeclarationDataForGetKeyForHeader()
	{
		invoice.AdditionalInfos.AddNew().CSI_Code = "RPTID";
		invoice.JZ_IncoTerm = "EXW";
		invoice.JZ_IncoTermPlace = "PLACE";
		invoice.ZG_AgreedPlaceCode = "1";
		invoice.JZ_ValuationCode = "20";
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invoice.JZ_AdditionalTerms = "TERM1";
	}

	void PopulateDeclarationDataForGetKeyForLineFromIt()
	{
		invoiceLine.ZG_SteelType = "9";
		declarationPackage.CW_PackType = "CT";
		invoiceLine.JI_StateOrRegionOfOrigin = "IT";
		invoiceLine.JI_RN_NKCountryOfExport = "AU";
	}

	void PopulateDeclarationDataForGetKeyForLineFromEu()
	{
		var buyer = Factory.NewWithValidTestData<OrgHeader>();
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		invoiceLine.JI_CountryOfOrigin = "CN";
		invoiceLine.JI_Procedure = "4000";
		invoiceLine.JI_PrimaryPreference = "A";
		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "60";
		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "70";
		invoiceLine.JI_SupplementaryCode1 = "40";
		invoiceLine.JI_SupplementaryCode2 = "50";
		invoiceLine.JI_ValuationCode = "1";
		invoiceLine.JI_ValuationMarkup = 0m;
		invoiceLine.JI_CustomsSecondUnitQty = "KGM";
		invoiceLine.JI_ConcessionOrder = "O";
		invoice.JZ_OH_Buyer = buyer.PK;
		invoice.JZ_OH_Supplier = supplier.PK;
		invoice.ZG_TransportChargesMethodOfPayment = "X";
		invoiceLine.ZG_CountryOfDestination = "IT";
		invoiceLine.ZG_CountryOfSupply = "US";
		invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "51";
		invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "52";
		invoiceLine.JI_CustomsThirdUnitQty = "X";

		invoiceLine.AdditionalInfos.AddNew().CSI_Code = "RPTID";
		invoiceLine.Taxes.AddNew().G4_Type = "A00";
		var sDocument = invoiceLine.SupportingDocuments.AddNew();
		sDocument.CSI_Code = "N380";
		sDocument.CSI_ReferenceNumber = "1234";
		sDocument.CSI_Status = SADConstants.CertificateFlag.DER;
		sDocument.CSI_UnitOfQuantity = "KGM";
		sDocument.CSI_YearOfIssue = "2020";
		sDocument.CSI_RN_NKCountryCode = "IT";
		invoiceLine.ZG_PortTaxRate = "A1";
		invoiceLine.JI_StateOrRegionOfOrigin = "IT";
	}

	void PopulateDeclarationDataForGetKeyForLineFromShared()
	{
		invoiceLine.JI_Tariff = "80";
		invoiceLine.JI_Description = "Description";
		invoiceLine.JI_PartNo = "PartNo";
		classification.CC_LookupCode = "Lookup";
		invoiceLine.JI_CC = classification.PK;
	}

	void PopulateChildCollectionsWhichShouldNotAffectMergeKey()
	{
		invoice.Charges.AddNew();
		invoice.AdditionalInfos.AddNew();
		invoice.PreviousDocuments.AddNew();
		invoice.HeaderDescriptions.AddNew();

		invoiceLine.CusLineTariffDetails.AddNew();
		invoiceLine.ApportionedCharges.AddNew();
		invoiceLine.Charges.AddNew();
		invoiceLine.ContainersPivot.AddNew();
		invoiceLine.UNDGs.AddNew();
		invoiceLine.PreviousDocuments.AddNew();
		invoiceLine.AdditionalInfos.AddNew();
		invoiceLine.Taxes.AddNew();
	}

	void AssertKeyForLine(string mergeBy, params IZType[] valuesToInsertAtTheBeginning)
	{
		declaration.JE_MergeBy = mergeBy;

		var entryCreationStrategy = declaration.CreateEntryCreationStrategy();
		var wholeActualKeyForLine = entryCreationStrategy.GetKeyForLine(invoiceLine).Keys;
		var actualKeyForLineWithoutHeaderPrefix = wholeActualKeyForLine.Skip(KeysFromGetKeyForHeaderPrefixCount).ToArray();
		var expectedKeysFromGetKeyForLine = GetExpectedKeysFromGetKeyForLine(valuesToInsertAtTheBeginning);
		AssertArrayEqualsByElements($"When merging by {mergeBy}", expectedKeysFromGetKeyForLine, actualKeyForLineWithoutHeaderPrefix);
	}

	IZType[] GetExpectedKeyForGetKeyForHeader()
	{
		return new IZType[]
		{
			declaration.JE_ApplicationCode,
			invoice.EffectiveValuationDate,
			invoiceLine.JI_CEI,
			invoice.RelatedIndicator,
			invoice.ZG_RelatedIndicator2,
			invoice.ZG_RelatedIndicator3,
			invoice.ZG_RelatedIndicator4,
			invoiceLine.JI_CEI,
			invoice.JZ_IncoTerm,
			invoice.JZ_IncoTermPlace,
			invoice.ZG_AgreedPlaceCode,
			invoice.JZ_ValuationCode,
			invoice.JZ_RX_NKInvoice_Currency,
			invoice.JZ_AdditionalTerms,
		};
	}

	IZType[] GetExpectedKeyForExportUcc6GetKeyForHeader()
	{
		return new IZType[]
		{
			declaration.JE_ApplicationCode,
			invoice.EffectiveValuationDate,
			invoiceLine.JI_CEI,
			invoiceLine.JI_CEI,
			invoice.JZ_IncoTerm,
			invoice.JZ_IncoTermPlace,
			invoice.ZG_AgreedPlaceCode,
			invoice.JZ_RX_NKInvoice_Currency,
			invoice.JZ_AdditionalTerms,
		};
	}

	IZType[] GetExpectedKeyForGetKeyForLineWithoutHeaderPrefix(bool isUCC6Export, params IZType[] valuesToInsertAtTheBeginning)
	{
		var expectedKeysFromGetKeyForLine = new List<IZType>(valuesToInsertAtTheBeginning);
		expectedKeysFromGetKeyForLine.AddRange(new IZType[]
		{
			invoiceLine.JI_CountryOfOrigin,
			invoiceLine.JI_Procedure,
			invoiceLine.JI_PrimaryPreference,
			new ZString($"{invoiceLine.JI_SupplementaryCode1}_{invoiceLine.JI_SupplementaryCode2}_{invoiceLine.AdditionalSupplementaryCodes[0].CY_Code}_{invoiceLine.AdditionalSupplementaryCodes[1].CY_Code}"),
			invoiceLine.JI_AdditionalSupplements,
			invoiceLine.JI_ValuationCode,
			invoiceLine.ZG_ValueAdjustmentCode,
			invoiceLine.JI_ValuationMarkup,
			invoiceLine.JI_CustomsSecondUnitQty,
			invoiceLine.JI_ConcessionOrder,
			invoice.JZ_OH_Buyer,
			invoice.JZ_OH_Supplier,
			invoice.ZG_TransportChargesMethodOfPayment,
			invoiceLine.ZG_CountryOfDestination,
			invoiceLine.ZG_CountryOfSupply,
		});

		if (!isUCC6Export)
		{
			expectedKeysFromGetKeyForLine.AddRange(new IZType[]
			{
			invoiceLine.RelatedIndicator,
			invoiceLine.ZG_RelatedIndicator2,
			invoiceLine.ZG_RelatedIndicator3,
			invoiceLine.ZG_RelatedIndicator4,
			});
		}

		expectedKeysFromGetKeyForLine.AddRange(new IZType[]
		{
			invoiceLine.AdditionalProcedureCodesAsString,
			invoiceLine.JI_CustomsThirdUnitQty,
			invoiceLine.ZG_CommercialReference,
			invoiceLine.ZG_TransNature,
			invoiceLine.JI_CustomsFourthUnitQty,
			invoiceLine.JI_CustomsFifthUnitQty,
		});
		expectedKeysFromGetKeyForLine.AddRange(GetExpectedSupportingDocumentsKeys(invoiceLine));

		expectedKeysFromGetKeyForLine.AddRange(new IZType[]
		{
			invoiceLine.ZG_CusNumber,
			invoiceLine.ZG_SteelType,
			declarationPackage.CW_PackType,
			invoiceLine.ZG_PortTaxRate,
			invoiceLine.JI_StateOrRegionOfOrigin,
		});

		return expectedKeysFromGetKeyForLine.ToArray();
	}

	IZType[] GetExpectedKeyForGetKeyForLineWithoutHeaderPrefix_ExportUCC6(params IZType[] valuesToInsertAtTheBeginning)
	{
		var expectedKeysFromGetKeyForLine = GetExpectedKeyForGetKeyForLineWithoutHeaderPrefix(true, valuesToInsertAtTheBeginning).ToList();
		expectedKeysFromGetKeyForLine.Add(invoice.JZ_ValuationCode);
		expectedKeysFromGetKeyForLine.Add(invoiceLine.JI_RN_NKCountryOfExport);
		return expectedKeysFromGetKeyForLine.ToArray();
	}

	IZType[] GetExpectedSupportingDocumentsKeys(JobComInvoiceLine invoiceLine)
	{
		var retList = new List<IZType>();

		foreach (SupportingDocument doc in invoiceLine.SupportingDocuments)
		{
			retList.Add(doc.CSI_Code);
			retList.Add(doc.CSI_ReferenceNumber);
			retList.Add(doc.CSI_Status);
			retList.Add(doc.CSI_UnitOfQuantity);
			retList.Add(doc.CSI_YearOfIssue);
			retList.Add(doc.CSI_RN_NKCountryCode);
		}

		return retList.ToArray();
	}

	protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		additionalDeclarationSetup?.Invoke(declaration);
		return declaration;
	}

	Action<JobDeclaration> additionalDeclarationSetup;

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
		var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "RPTID", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		cusCode.Attributes.AddNew("Direction", "IMPORT");
		cusCode.Attributes.AddNew("Direction", "EXPORT");
		cusCode.Attributes.AddNew("Level", "HEADER");
	}

	protected override string[] GetExpectedSupportingDocumentKeys()
	{
		return new string[]
		{
			SupportingDocument.Schema.CSI_Code,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_Status,
			SupportingDocument.Schema.CSI_UnitOfQuantity,
			SupportingDocument.Schema.CSI_YearOfIssue,
			SupportingDocument.Schema.CSI_RN_NKCountryCode,
		};
	}

	protected override string[] GetExpectedCusSupplyChainActorReferenceKeys()
	{
		return new string[]
		{
			CusSupplyChainActorReference.Schema.CFR_Code,
			CusSupplyChainActorReference.Schema.CFR_Reference,
		};
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		classification = Factory.New<BaseCusClassification>();
		classification.CC_LookupCode = "TestLookup";
		invoiceLine.JI_CC = classification.PK;
		var declarationBill = declaration.Bills.AddNew();
		var declarationBillPackingGroup = declarationBill.PackingGroups.AddNew();
		declarationPackage = declaration.Packages.AddNew();
		declarationPackage.CW_CR_HouseContainer = declarationBillPackingGroup.PK;
		var packagePivotInvoiceLine = invoiceLine.PackagesPivot.AddNew();
		packagePivotInvoiceLine.CHC_CW = declarationPackage.PK;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	BaseCusClassification classification;
	BasePackage declarationPackage;

	bool IsUCC6AndIsExport => declaration.IsUCC6AndIsExport;

	int KeysFromGetKeyForHeaderPrefixCount => IsUCC6AndIsExport ? 13 : 14;

	IZType[] GetExpectedKeysFromGetKeyForLine(params IZType[] valuesToInsertAtTheBeginning)
	{
		if (IsUCC6AndIsExport)
		{
			return GetExpectedKeyForGetKeyForLineWithoutHeaderPrefix_ExportUCC6(valuesToInsertAtTheBeginning);
		}
		return GetExpectedKeyForGetKeyForLineWithoutHeaderPrefix(false, valuesToInsertAtTheBeginning);
	}

	IZType[] GetExpectedKeyForHeader() => IsUCC6AndIsExport ? GetExpectedKeyForExportUcc6GetKeyForHeader() : GetExpectedKeyForGetKeyForHeader();
}
