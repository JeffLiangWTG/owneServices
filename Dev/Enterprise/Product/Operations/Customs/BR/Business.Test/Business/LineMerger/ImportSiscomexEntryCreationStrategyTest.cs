using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportSiscomexEntryCreationStrategyTest : EntryCreationStrategyTest<ImportSiscomexEntryCreationForTesting>
	{
		public void TestGetKeyForLine()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CompanyName = "MANUFACTURER";
			manufacturer.MainAddress.OA_Email = "MANUFACTURER@TEST.COM";
			manufacturer.MainAddress.OA_Address1 = "MANUFACTURER ADDRESS 1";
			manufacturer.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "MANUFACTURER ADDITIONAL ADDRESS";
			manufacturer.MainAddress.OA_City = "MANUFACTURER CITY";
			manufacturer.MainAddress.OA_State = "MS";
			manufacturer.MainAddress.OA_RN_NKCountryCode = "CV";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var supplier = Factory.New<OrgHeader>();
			invoice.JZ_OH_Supplier = manufacturer.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoice.JZ_ValuationCode = ValuationCodeList.Codes._01;
			invoice.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;

			invoice.ExchangeHedge.CSI_Code = ExchangeHedgeList.Codes._1;
			invoice.ExchangeHedge.CSI_IssuerType = "2";
			invoice.ExchangeHedge.CSI_AdditionalDescription = "3";
			invoice.ExchangeHedge.CSI_ReferenceNumber = "4";

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;

			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.NaladiHs = "01010000";
			invoiceLine.NaladiNcca = "01010002";
			invoiceLine.MercosulForeignDeclarationType = CertificateTypeList.Codes.CCPTC;
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine.ICMSTaxRegime = "1";
			invoiceLine.ICMSLegalBase = "2";
			invoiceLine.DutyTaxRegime = "3";
			invoiceLine.DutyLegalBase = "4";
			invoiceLine.JI_TemporaryAdmissionReason = "5";
			invoiceLine.JI_ICMSRate = 12m;
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.ImportLicenseType = ImportLicenseType.Codes.PostBoarding;
			invoiceLine.ImportLicenseNumber = "123";
			invoiceLine.ImportLicenseAuthorizationDate = new ZDateTime(2023, 1, 1);
			invoiceLine.JI_ICMSBaseValueReductionPercentage = 1m;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = 1.5m;
			invoiceLine.IPITaxRegime = "5";
			invoiceLine.JI_ComplementaryNote = "60";
			invoiceLine.PisCofinsTaxRegime = "6";
			invoiceLine.IPITaxBenefitLegalActType = "1";
			invoiceLine.IPITaxBenefitLegalActNumber = "123";
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine.DutyRateIsOverridden = true;
			invoiceLine.DutyVigentRateValue = 10m;
			invoiceLine.FMMBenefit = "E";
			invoiceLine.JI_GoodsApplication = GoodsApplicationTypeList.Codes.Resale;
			invoiceLine.JI_GoodsCondition = GoodsConditionTypeList.Codes.GoodsMadeToOrder;
			invoiceLine.JI_ICMSFormula = "F";
			invoiceLine.ICMSFCPRateValue = 10m;

			var specalCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specalCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			specalCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			specalCaseTax.LegalActType = "2";
			specalCaseTax.LegalActNumber = "456";

			var previousDoc1 = invoiceLine.PreviousDocuments.AddNew();
			previousDoc1.CSI_Code = ImportSiscomexPreviousDocumentList.Codes.RE;
			previousDoc1.CSI_ReferenceNumber = "9";

			var previousDoc2 = invoiceLine.PreviousDocuments.AddNew();
			previousDoc2.CSI_Code = ImportSiscomexPreviousDocumentList.Codes.DI;
			previousDoc2.CSI_ReferenceNumber = "8";

			var previousDoc3 = invoiceLine.PreviousDocuments.AddNew();
			previousDoc3.CSI_Code = ImportSiscomexPreviousDocumentList.Codes.DI;
			previousDoc3.CSI_ReferenceNumber = "6";

			var mercosulDec1 = invoiceLine.MercosulForeignDeclarations[0];
			mercosulDec1.CSI_Description = "023";
			mercosulDec1.CSI_ReferenceNumber2 = "046";
			mercosulDec1.CSI_ItemNumber = 7676;

			var mercosulDec2 = invoiceLine.MercosulForeignDeclarations.AddNew();
			mercosulDec2.CSI_Description = "013";
			mercosulDec2.CSI_ReferenceNumber2 = "026";
			mercosulDec2.CSI_ItemNumber = 5656;

			var nve = invoiceLine.NVECusCodeDataCollection.AddNew();
			nve.CY_Code = "AA";
			nve.CY_Data = "0001";
			nve.CY_Order = 6;

			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "999";

			var additionalTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionalTariff.ExNumber = "001";
			additionalTariff.TariffType = "XXXX";
			additionalTariff.LegalActType = "XX";
			additionalTariff.LegalActIssuingBody = "X";
			additionalTariff.LegalActNumber = "123";
			additionalTariff.LegalActYear = "2021";
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var mergeKeys = new ImportSiscomexEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine).Keys;
			CombineAssertions("Merge Keys for NON", () =>
			{
				AssertEquals(4, mergeKeys.Count);
				Assert("Keys contain JI_PK", mergeKeys.Contains(invoiceLine.PK));
			});

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			mergeKeys = new ImportSiscomexEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine).Keys;
			var keyCount = 4;
			CombineAssertions("Merge Keys for CLS", () =>
			{
				AssertEquals(52, mergeKeys.Count);

				AssertEquals("Keys contain JI_Tariff", new ZString("01010101"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JZ_RX_NKInvoice_Currency", new ZString("BRL"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JZ_IncoTerm", new ZString("FOB"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JZ_ValuationCode", new ZString(ValuationCodeList.Codes._01), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JZ_OA_SupplierAddress", manufacturer.MainAddress.PK, mergeKeys[keyCount++]);
				AssertEquals("Keys contain ExchangeHedgeType", new ZString(ExchangeHedgeList.Codes._1), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ExchangeHedgeFinancialInstitution", new ZString("2"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ExchangeHedgeReason", new ZString("3"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ExchangeHedgeROFBACENNumber", new ZString("4"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain TariffDetachConcatenated", new ZString("999"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain NVEConcatenated", new ZString("6|AA|0001"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain NaladiHs", new ZString("01010000"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain NaladiNcca", new ZString("01010002"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain MercosulForeignDeclarationType", new ZString(CertificateTypeList.Codes.CCPTC), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_PrimaryPreference", new ZString("EXTARIFF"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_OA_ManufacturerAddress", manufacturer.MainAddress.PK, mergeKeys[keyCount++]);
				AssertEquals("Keys contain Charge OFT,ONS", new ZString("OFT,ONS"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ICMSTaxRegime 1", new ZString("1"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ICMSLegalBase 2", new ZString("2"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_ICMSRate", 12m, mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_ICMSBaseValueReductionPercentage", 1m, mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_ICMSTotalAmountReductionPercentage", 1.5m, mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_ICMSFormula", new ZString("F"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain DutyTaxRegime 3", new ZString("3"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain DutyLegalBase 4", new ZString("4"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_TemporaryAdmissionReason 5", new ZString("5"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ImportLicenseType", new ZString(ImportLicenseType.Codes.PostBoarding), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ImportLicenseNumber", new ZString("123"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain ImportLicenseAuthorizationDate", new ZDateTime(2023, 1, 1), mergeKeys[keyCount++]);
				AssertEquals("Keys contain IPITaxRegime", new ZString("5"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_ComplementaryNote", new ZString("60"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain PisCofinsTaxRegime", new ZString("6"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain IPITaxBenefitLegalActType", new ZString("1"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain IPITaxBenefitLegalActNumber", new ZString("123"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain AntidumpingLegalActType", new ZString("2"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain AntidumpingLegalActNumber", new ZString("456"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain AdditionalTariffConcatenated", new ZString($"1||||||,3|001|XXXX|XX|X|123|2021"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain DutyRateIsOverridden", true, mergeKeys[keyCount++]);
				AssertEquals("Keys contain DutyVigentRateValue", 10m, mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_ManufacturerIndicator", new ZString("2"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain Empty string when JI_ManufacturerIndicator != 3", ZString.Empty, mergeKeys[keyCount]);
				invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
				mergeKeys = new ImportSiscomexEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine).Keys;
				AssertEquals("Keys contains JI_CountryOfOrigin when JI_ManufacturerIndicator == 3", new ZString("CV"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain FMMBenefit", new ZString("E"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_GoodsApplication", new ZString(GoodsApplicationTypeList.Codes.Resale), mergeKeys[keyCount++]);
				AssertEquals("Keys contain JI_GoodsCondition", new ZString(GoodsConditionTypeList.Codes.GoodsMadeToOrder), mergeKeys[keyCount++]);
				AssertEquals("Keys contain PreviousDocumentConcatenated", new ZString("DI|6,DI|8,RE|9"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain MercosulForeignDeclarationConcatenated", new ZString("013|026|5656,023|046|7676"), mergeKeys[keyCount++]);
				AssertEquals("Keys contain Cloned Entry Instruction PK", ZGuid.Empty, mergeKeys[keyCount++]);
				AssertEquals("Keys contain ICMSFCPRateValue", 10m, mergeKeys[keyCount++]);
			});
		}

		public void TestGetKeyForLineWithAttachImportLicense()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST1";
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0123456";
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0123456";
			var invoiceLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0123456";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(1, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var licInvHeader = licDeclaration.Invoices.AddNew();

			var licEntryInstruction1 = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction1.CEI_Description = "TEST1";

			var licInvLine1 = licInvHeader.InvoiceLines.AddNew();
			licInvLine1.JI_CEI = licEntryInstruction1.PK;

			var licEntryInstruction2 = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction2.CEI_Description = "TEST1";

			var licInvLine2 = licInvHeader.InvoiceLines.AddNew();
			licInvLine2.JI_CEI = licEntryInstruction2.PK;

			invoiceLine1.JI_ParentID = licInvLine1.PK;
			invoiceLine2.JI_ParentID = licInvLine2.PK;

			Assert("MergeKeys contain Linked Entry Instruction PK", new ImportSiscomexEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine1).Keys.Contains(licEntryInstruction1.PK));
			Assert("MergeKeys contain Linked Entry Instruction PK", new ImportSiscomexEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine2).Keys.Contains(licEntryInstruction2.PK));

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(3, declaration.ActiveEntryHeaders[0].MergedLines.Count);
		}

		protected override ImportSiscomexEntryCreationForTesting CreateNewEntryCreationStrategy(JobDeclaration declaration)
		{
			return new ImportSiscomexEntryCreationForTesting(declaration);
		}

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override ZString ExpectedMessageTypeToNewEntryHeader => MessageTypeList.Codes.ISW;
	}

	class ImportSiscomexEntryCreationForTesting : ImportSiscomexEntryCreationStrategy, IEntryCreationStrategyForTesting
	{
		public ImportSiscomexEntryCreationForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public bool UseAdditionalEntryLineLinksExposed => base.UseAdditionalEntryLineLinks;

		public bool IsActiveExposed => base.IsActiveCore;

		public string MessageTypeToNewEntryHeaderExposed => base.CH_MessageTypeToNewEntryHeader;

		public void ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(BaseJobComInvoiceLine invoiceLine) => ClearReferenceToEntryLineWhenLineIsNotValidForMerge(invoiceLine);
	}
}
