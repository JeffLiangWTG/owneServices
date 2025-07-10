using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class JobComInvoiceLineDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneCountrySpecificData_Export()
		{
			var invoiceLineToClone = CreateExportInvoiceLineForClone(Factory);
			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, CloneType.TemplateCopy, invoiceLineToClone.InvoiceHeader, null).Clone();
			AssertExportInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);
		}

		public void TestCloneCountrySpecificData_ImportLicense()
		{
			var invoiceLineToClone = CreateImportLicenseInvoiceLineForClone(Factory);
			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, CloneType.TemplateCopy, invoiceLineToClone.InvoiceHeader, null).Clone();
			AssertImportLicenseInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);
		}

		public void TestCloneCountrySpecificData_ImportSiscomex()
		{
			var invoiceLineToClone = CreateImportSiscomexInvoiceLineForClone(Factory);
			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, CloneType.TemplateCopy, invoiceLineToClone.InvoiceHeader, null).Clone();
			AssertImportSiscomexInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);
		}

		public void TestCloneCountrySpecificData_Import()
		{
			var invoiceLineToClone = CreateImportInvoiceLineForClone(Factory);
			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLineToClone, CloneType.TemplateCopy, invoiceLineToClone.InvoiceHeader, null).Clone();
			AssertImportInvoiceLineCloned(clonedInvoiceLine, invoiceLineToClone);
		}

		public static JobComInvoiceLine CreateExportInvoiceLineForClone(BusinessObjectFactory factory)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_3061");
			factory.Save();

			var declaration = factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.EntranceOfficeCode = "1234567";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";
			invoiceLine.FullGoodsDescription = new ZString('X', invoiceLine.FullGoodsDescriptionInfo.MaxLength);
			invoiceLine.ComplementaryDescription = "TEST1";
			invoiceLine.JI_NFeNumber = "12";
			invoiceLine.JI_NFeItemNumber = "I1";
			invoiceLine.JI_CargoPriority = CargoPriorityList.Codes._5002;
			invoiceLine.JI_IntendedTermDays = 111;
			invoiceLine.JI_DigitalServiceDossier = "111";
			invoiceLine.JI_FinancedValue = 222.2m;
			invoiceLine.JI_AgentCommissionPercentage = 10;
			invoiceLine.JI_ExportJustificationInfo = "TEST IF WORKS";

			var att = invoiceLine.Attributes.GetFirstElementHaving("ATT_3061");
			att.CY_Data = "269020128";

			return invoiceLine;
		}

		public static void AssertExportInvoiceLineCloned(JobComInvoiceLine clonedInvoiceLine, JobComInvoiceLine invoiceLineToClone)
		{
			CombineAssertions(() =>
			{
				AssertEquals("FullGoodsDescription cloned", invoiceLineToClone.FullGoodsDescription, clonedInvoiceLine.FullGoodsDescription);
				AssertEquals("ComplementaryDescriptionExport cloned", invoiceLineToClone.ComplementaryDescription, clonedInvoiceLine.ComplementaryDescription);
				AssertEquals("JI_NFeNumber cloned", invoiceLineToClone.JI_NFeNumber, clonedInvoiceLine.JI_NFeNumber);
				AssertEquals("JI_NFeItemNumber cloned", invoiceLineToClone.JI_NFeItemNumber, clonedInvoiceLine.JI_NFeItemNumber);
				AssertEquals("JI_CargoPriority cloned", invoiceLineToClone.JI_CargoPriority, clonedInvoiceLine.JI_CargoPriority);
				AssertEquals("JI_IntendedTermDays cloned", invoiceLineToClone.JI_IntendedTermDays, clonedInvoiceLine.JI_IntendedTermDays);
				AssertEquals("JI_DigitalServiceDossier cloned", invoiceLineToClone.JI_DigitalServiceDossier, clonedInvoiceLine.JI_DigitalServiceDossier);
				AssertEquals("JI_FinancedValue cloned", invoiceLineToClone.JI_FinancedValue, clonedInvoiceLine.JI_FinancedValue);
				AssertEquals("JI_AgentCommissionPercentage cloned", invoiceLineToClone.JI_AgentCommissionPercentage, clonedInvoiceLine.JI_AgentCommissionPercentage);
				AssertEquals("JI_ExportJustificationInfo cloned", invoiceLineToClone.JI_ExportJustificationInfo, clonedInvoiceLine.JI_ExportJustificationInfo);

				AssertEquals("AttributeCusCodeDataCollection cloned", 1, clonedInvoiceLine.Attributes.Count);
				var attCloned = clonedInvoiceLine.Attributes[0];
				AssertEquals("CY_Code cloned", "ATT_3061", attCloned.CY_Code);
				AssertEquals("CY_Data cloned", "269020128", attCloned.CY_Data);

				Assert("Validation should be suspended", !clonedInvoiceLine.HasNotifications());
				Assert("HasChanges should not be set", !clonedInvoiceLine.HasChanges);
			});
		}

		public static JobComInvoiceLine CreateImportLicenseInvoiceLineForClone(BusinessObjectFactory factory)
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(factory);

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.EntranceOfficeCode = "1234567";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FullGoodsDescription = new ZString('X', invoiceLine.FullGoodsDescriptionInfo.MaxLength);
			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.DutyLegalBase = "2";
			invoiceLine.DrawbackCANumber = "11";
			invoiceLine.DrawbackModality = "1";
			invoiceLine.DrawbackItemNumber = 2;
			invoiceLine.JI_Tariff = "11111111";
			invoiceLine.NaladiHs = "123";
			invoiceLine.NaladiNcca = "234";

			var nve = invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("BA");
			nve.CY_Data = "0001";

			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			manufacturerDocAddress.E2_Address1 = "TestOrg1";
			manufacturerDocAddress.E2_RN_NKCountryCode = "US";

			var consentingProcess = invoiceLine.ConsentingProcessCollection.AddNew();
			consentingProcess.CSI_ReferenceNumber = "123";
			consentingProcess.CSI_CustomsOffice = "COF1";

			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "123";

			return invoiceLine;
		}

		public static void AssertImportLicenseInvoiceLineCloned(JobComInvoiceLine clonedInvoiceLine, JobComInvoiceLine invoiceLineToClone)
		{
			CombineAssertions(() =>
			{
				AssertEquals("FullGoodsDescription cloned", invoiceLineToClone.FullGoodsDescription, clonedInvoiceLine.FullGoodsDescription);
				AssertEquals("manufacturerDocAddress.E2_Address1 cloned", invoiceLineToClone.ManufacturerDocAddress.E2_Address1, clonedInvoiceLine.ManufacturerDocAddress.E2_Address1);
				AssertEquals("JI_ManufacturerIndicator cloned", invoiceLineToClone.JI_ManufacturerIndicator, clonedInvoiceLine.JI_ManufacturerIndicator);
				AssertEquals("JI_OA_ManufacturerAddress cloned", invoiceLineToClone.JI_OA_ManufacturerAddress, clonedInvoiceLine.JI_OA_ManufacturerAddress);
				AssertEquals("JI_CountryOfOrigin cloned", invoiceLineToClone.JI_CountryOfOrigin, clonedInvoiceLine.JI_CountryOfOrigin);
				AssertEquals("DutyTaxRegime cloned", "1", clonedInvoiceLine.DutyTaxRegime);
				AssertEquals("DutyLegalBase cloned", "2", clonedInvoiceLine.DutyLegalBase);
				AssertEquals("NaladiHs cloned", "123", clonedInvoiceLine.NaladiHs);
				AssertEquals("NaladiNcca cloned", "234", clonedInvoiceLine.NaladiNcca);
				AssertEquals("DrawbackCANumber cloned", "11", clonedInvoiceLine.DrawbackCANumber);
				AssertEquals("DrawbackModality cloned", "1", clonedInvoiceLine.DrawbackModality);
				AssertEquals("DrawbackItemNumber cloned", ZShort.Parse("2"), clonedInvoiceLine.DrawbackItemNumber);

				var clonedNve = clonedInvoiceLine.NVECusCodeDataCollection.First();
				AssertEquals("NVECusCodeDataCollection cloned", 2, clonedInvoiceLine.NVECusCodeDataCollection.Count);
				AssertEquals("CY_Code cloned", "BA", clonedNve.CY_Code);
				AssertEquals("CY_Data cloned", "0001", clonedNve.CY_Data);
				AssertEquals("Specification cloned", "Ductil com nodularidade até 80%", clonedNve.Specification);

				var clonedConsentingProcess = clonedInvoiceLine.ConsentingProcessCollection[0];
				AssertEquals("ConsentingProcessCollection cloned", 1, clonedInvoiceLine.ConsentingProcessCollection.Count);
				AssertEquals("CSI_ReferenceNumber cloned", "123", clonedConsentingProcess.CSI_ReferenceNumber);
				AssertEquals("CSI_CustomsOffice cloned", "COF1", clonedConsentingProcess.CSI_CustomsOffice);

				var clonedTariffDetach = clonedInvoiceLine.TariffDetachs[0];
				AssertEquals("TariffDetachCollection cloned", 1, clonedInvoiceLine.TariffDetachs.Count);
				AssertEquals("CY_Code cloned", "123", clonedTariffDetach.CY_Code);

				Assert("Validation should be suspended", !clonedInvoiceLine.HasNotifications());
				Assert("HasChanges should not be set", !clonedInvoiceLine.HasChanges);
			});
		}

		public static JobComInvoiceLine CreateImportSiscomexInvoiceLineForClone(BusinessObjectFactory factory)
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(factory);

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.FullGoodsDescription = new ZString('X', invoiceLine.FullGoodsDescriptionInfo.MaxLength);
			invoiceLine.JI_Tariff = "11111111";
			invoiceLine.NaladiHs = "123";
			invoiceLine.NaladiNcca = "234";

			var nve = invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("BA");
			nve.CY_Data = "0001";

			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "123";

			invoiceLine.ImportLicenseNumber = "123";
			invoiceLine.ImportLicenseType = "1";
			invoiceLine.ImportLicenseAuthorizationDate = new ZDateTime(2023, 1, 1);
			invoiceLine.ImportLicenseFeeType = "12";

			var linkedDocument = invoiceLine.PreviousDocuments.AddNew();
			linkedDocument.CSI_Code = "001";
			linkedDocument.CSI_ReferenceNumber = "1234";

			var mercosulForeignDeclaration = invoiceLine.MercosulForeignDeclarations.AddNew();
			invoiceLine.MercosulForeignDeclarationType = "1";
			mercosulForeignDeclaration.CSI_Description = "1";
			mercosulForeignDeclaration.CSI_ReferenceNumber = "1";
			mercosulForeignDeclaration.CSI_ReferenceNumber2 = "2";
			mercosulForeignDeclaration.CSI_RN_NKCountryCode = "BR";
			mercosulForeignDeclaration.CSI_Code = "2";
			mercosulForeignDeclaration.CSI_ItemNumber = 1;
			mercosulForeignDeclaration.CSI_Quantity3 = 1m;

			invoiceLine.ICMSTaxRegime = "1";
			invoiceLine.ICMSLegalBase = "2";
			invoiceLine.JI_ICMSRate = 5m;
			invoiceLine.JI_ICMSBaseValueReductionPercentage = 50m;

			invoiceLine.DutyTaxRegime = "4";
			invoiceLine.DutyLegalBase = "34";

			invoiceLine.IPITaxRegime = "1";
			invoiceLine.IPIRateIsOverridden = true;
			invoiceLine.IPIVigentRateValue = 50m;

			invoiceLine.IPITaxBenefitLegalActType = "1";
			invoiceLine.IPITaxBenefitLegalActIssuingBody = "2";
			invoiceLine.IPITaxBenefitLegalActNumber = "3";
			invoiceLine.IPITaxBenefitLegalActYear = "2021";

			invoiceLine.PisCofinsTaxRegime = "2";
			invoiceLine.PisCofinsLegalBase = "11";
			invoiceLine.PisRateIsOverridden = true;
			invoiceLine.PisVigentRateValue = 40m;
			invoiceLine.CofinsRateIsOverridden = true;
			invoiceLine.CofinsVigentRateValue = 50m;

			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			specialCaseTax.RateOrUnitValue = 12.34m;
			specialCaseTax.CurrencyCode = "USD";
			specialCaseTax.Quantity = 88;
			specialCaseTax.UnitOfMeasure = "KG";
			specialCaseTax.LegalActType = "4";
			specialCaseTax.LegalActIssuingBody = "5";
			specialCaseTax.LegalActNumber = "6";
			specialCaseTax.LegalActYear = "2023";

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine.DutyRateIsOverridden = true;
			invoiceLine.DutyVigentRateValue = 6m;

			var exDutyTariff = invoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff);
			exDutyTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			exDutyTariff.TariffCode = "001";

			var exIPITariff = invoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExIPITariff);
			exIPITariff.TariffType = ChildTariffTypeList.Codes.IPI;
			exIPITariff.TariffCode = "002";
			exIPITariff.LegalActType = "ADE";
			exIPITariff.LegalActIssuingBody = "ALADI";
			exIPITariff.LegalActNumber = "444";
			exIPITariff.LegalActYear = "2079";

			return invoiceLine;
		}

		public static void AssertImportSiscomexInvoiceLineCloned(JobComInvoiceLine clonedInvoiceLine, JobComInvoiceLine invoiceLineToClone)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_Tariff cloned", invoiceLineToClone.JI_Tariff, clonedInvoiceLine.JI_Tariff);
				AssertEquals("FullGoodsDescription cloned", invoiceLineToClone.FullGoodsDescription, clonedInvoiceLine.FullGoodsDescription);
				AssertEquals("ImportLicenseNumber cloned", "123", clonedInvoiceLine.ImportLicenseNumber);
				AssertEquals("ImportLicenseType cloned", "1", clonedInvoiceLine.ImportLicenseType);
				AssertEquals("ImportLicenseAuthorizationDate cloned", new ZDateTime(2023, 1, 1), clonedInvoiceLine.ImportLicenseAuthorizationDate);
				AssertEquals("ImportLicenseFeeType cloned", "12", clonedInvoiceLine.ImportLicenseFeeType);

				AssertEquals("NaladiHs cloned", "123", clonedInvoiceLine.NaladiHs);
				AssertEquals("NaladiNcca cloned", "234", clonedInvoiceLine.NaladiNcca);
				AssertEquals("JI_CEI cloned", invoiceLineToClone.JI_CEI, clonedInvoiceLine.JI_CEI);

				AssertEquals("PreviousDocuments cloned", invoiceLineToClone.PreviousDocuments.Count, clonedInvoiceLine.PreviousDocuments.Count);
				AssertEquals("PreviousDocuments.CSI_Code cloned", "001", clonedInvoiceLine.PreviousDocuments[0].CSI_Code);
				AssertEquals("PreviousDocuments.CSI_Code cloned", "1234", clonedInvoiceLine.PreviousDocuments[0].CSI_ReferenceNumber);

				var clonedMercosulForeignDeclaration = clonedInvoiceLine.MercosulForeignDeclarations[0];
				AssertEquals("MercosulForeignDeclarations cloned", invoiceLineToClone.MercosulForeignDeclarations.Count, clonedInvoiceLine.MercosulForeignDeclarations.Count);
				AssertEquals("MercosulForeignDeclarationType cloned", "1", clonedInvoiceLine.MercosulForeignDeclarationType);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_Description cloned", "1", clonedMercosulForeignDeclaration.CSI_Description);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_ReferenceNumber cloned", "1", clonedMercosulForeignDeclaration.CSI_ReferenceNumber);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_RN_NKCountryCode cloned", "BR", clonedMercosulForeignDeclaration.CSI_RN_NKCountryCode);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_Code cloned", "2", clonedMercosulForeignDeclaration.CSI_Code);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_ItemNumber cloned", (ZShort)1, clonedMercosulForeignDeclaration.CSI_ItemNumber);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_Quantity3 cloned", 1m, clonedMercosulForeignDeclaration.CSI_Quantity3);

				var clonedNve = clonedInvoiceLine.NVECusCodeDataCollection[0];
				AssertEquals("NVECusCodeDataCollection cloned", 2, clonedInvoiceLine.NVECusCodeDataCollection.Count);
				AssertEquals("CY_Code cloned", "BA", clonedNve.CY_Code);
				AssertEquals("CY_Data cloned", "0001", clonedNve.CY_Data);
				AssertEquals("Specification cloned", "Ductil com nodularidade até 80%", clonedNve.Specification);

				var clonedTariffDetach = clonedInvoiceLine.TariffDetachs[0];
				AssertEquals("TariffDetachCollection cloned", 1, clonedInvoiceLine.TariffDetachs.Count);
				AssertEquals("CY_Code cloned", "123", clonedTariffDetach.CY_Code);

				AssertEquals("ICMSTaxRegime cloned", "1", clonedInvoiceLine.ICMSTaxRegime);
				AssertEquals("ICMSLegalBase cloned", "2", clonedInvoiceLine.ICMSLegalBase);

				AssertEquals("JI_ICMSRate cloned", 5m, clonedInvoiceLine.JI_ICMSRate);
				AssertEquals("JI_ICMSBaseValueReductionPercentage cloned", 50m, clonedInvoiceLine.JI_ICMSBaseValueReductionPercentage);

				AssertEquals("DutyTaxRegime cloned", "4", clonedInvoiceLine.DutyTaxRegime);
				AssertEquals("DutyLegalBase cloned", "34", clonedInvoiceLine.DutyLegalBase);

				AssertEquals("IPITaxRegime cloned", "1", clonedInvoiceLine.IPITaxRegime);
				AssertEquals("IPIRateIsOverridden cloned", true, clonedInvoiceLine.IPIRateIsOverridden);
				AssertEquals("IPIVigentRateValue cloned", 50m, clonedInvoiceLine.IPIVigentRateValue);

				AssertEquals("IPITaxBenefitLegalActType cloned", "1", clonedInvoiceLine.IPITaxBenefitLegalActType);
				AssertEquals("IPITaxBenefitLegalActIssuingBody cloned", "2", clonedInvoiceLine.IPITaxBenefitLegalActIssuingBody);
				AssertEquals("IPITaxBenefitLegalActNumber cloned", "3", clonedInvoiceLine.IPITaxBenefitLegalActNumber);
				AssertEquals("IPITaxBenefitLegalActYear cloned", "2021", clonedInvoiceLine.IPITaxBenefitLegalActYear);

				AssertEquals("PisCofinsTaxRegime cloned", "2", clonedInvoiceLine.PisCofinsTaxRegime);
				AssertEquals("PisCofinsLegalBase cloned", "11", clonedInvoiceLine.PisCofinsLegalBase);
				AssertEquals("PisRateIsOverridden cloned", true, clonedInvoiceLine.PisRateIsOverridden);
				AssertEquals("PisVigentRateValue cloned", 40m, clonedInvoiceLine.PisVigentRateValue);
				AssertEquals("CofinsRateIsOverridden cloned", true, clonedInvoiceLine.CofinsRateIsOverridden);
				AssertEquals("CofinsVigentRateValue cloned", 50m, clonedInvoiceLine.CofinsVigentRateValue);

				AssertEquals("AntidumpingRateIsOverridden cloned", true, clonedInvoiceLine.AntidumpingRateIsOverridden);
				AssertEquals("AntidumpingRateValue cloned", 12.34m, clonedInvoiceLine.AntidumpingRateValue);

				AssertEquals("JI_PrimaryPreference cloned", Constants.RatePreferenceType.ExTariff, clonedInvoiceLine.JI_PrimaryPreference);
				AssertEquals("DutyRateIsOverridden cloned", true, clonedInvoiceLine.DutyRateIsOverridden);
				AssertEquals("OverriddenDutyRateValue cloned", 6m, clonedInvoiceLine.DutyVigentRateValue);

				AssertEquals("AdditionalTariffs.Count", 2, clonedInvoiceLine.AdditionalTariffs.Count);

				var clonedExDutyTariff = clonedInvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff);
				AssertEquals("ExDutyTariff TariffType", ChildTariffTypeList.Codes.LEBIT, clonedExDutyTariff.TariffType);
				AssertEquals("ExDutyTariff TariffCode", "001", clonedExDutyTariff.TariffCode);

				var clonedExIPITariff = clonedInvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExIPITariff);
				AssertEquals("ExIPITariff TariffType", ChildTariffTypeList.Codes.IPI, clonedExIPITariff.TariffType);
				AssertEquals("ExIPITariff TariffCode", "002", clonedExIPITariff.TariffCode);
				AssertEquals("ExIPITariff LegalActType", "ADE", clonedExIPITariff.LegalActType);
				AssertEquals("ExIPITariff LegalActIssuingBody", "ALADI", clonedExIPITariff.LegalActIssuingBody);
				AssertEquals("ExIPITariff LegalActNumber", "444", clonedExIPITariff.LegalActNumber);
				AssertEquals("ExIPITariff LegalActYear", "2079", clonedExIPITariff.LegalActYear);

				AssertEquals("QuantityPerUnitInfos should be cloned", 1, clonedInvoiceLine.QuantityPerUnitInfos.Count);
				AssertEquals("SpecalCaseTaxes should be cloned", 1, clonedInvoiceLine.SpecialCaseTaxes.Count);
				AssertEquals("LegalActInfos should be cloned", 4, clonedInvoiceLine.LegalActInfos.Count);

				var specialCaseTax = clonedInvoiceLine.SpecialCaseTaxes[0];
				AssertEquals("SpecalCaseTax TaxGroup cloned", Constants.RateCodes.Antidumping, specialCaseTax.TaxGroup);
				AssertEquals("SpecalCaseTax TaxType cloned", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, specialCaseTax.TaxType);
				AssertEquals("SpecalCaseTax RateOrUnitValue cloned", 12.34m, specialCaseTax.RateOrUnitValue);
				AssertEquals("SpecalCaseTax Currency cloned", "USD", specialCaseTax.CurrencyCode);
				AssertEquals("SpecalCaseTax Quantity cloned", 88m, specialCaseTax.Quantity);
				AssertEquals("SpecalCaseTax UnitOfMeasure cloned", "KG", specialCaseTax.UnitOfMeasure);
				AssertEquals("AntidumpingLegalActType cloned", "4", specialCaseTax.LegalActType);
				AssertEquals("AntidumpingLegalActIssuingBody cloned", "5", specialCaseTax.LegalActIssuingBody);
				AssertEquals("AntidumpingLegalActNumber cloned", "6", specialCaseTax.LegalActNumber);
				AssertEquals("AntidumpingLegalActYear cloned", "2023", specialCaseTax.LegalActYear);

				Assert("Validation should be suspended", !clonedInvoiceLine.HasNotifications());
				Assert("HasChanges should not be set", !clonedInvoiceLine.HasChanges);
			});
		}

		public static JobComInvoiceLine CreateImportInvoiceLineForClone(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var linkedDocument1 = invoiceLine.PreviousDocuments.AddNew();
			linkedDocument1.CSI_Code = "001";
			linkedDocument1.CSI_ReferenceNumber = "1234";
			linkedDocument1.CSI_ItemNumber = 10;

			var linkedDocument2 = invoiceLine.PreviousDocuments.AddNew();
			linkedDocument2.CSI_Code = "002";
			linkedDocument2.CSI_ReferenceNumber = "4567";
			linkedDocument2.CSI_ItemNumber = 20;

			var mercosulForeignDeclaration = invoiceLine.MercosulForeignDeclarations.AddNew();
			invoiceLine.MercosulForeignDeclarationType = "1";
			mercosulForeignDeclaration.CSI_Code = "2";
			mercosulForeignDeclaration.CSI_Quantity3 = 1m;

			var permit1 = invoiceLine.Permits.AddNew();
			permit1.CSI_ReferenceNumber = "1";
			permit1.CSI_UnitOfQuantity = "2";
			permit1.CSI_Quantity = 10;

			var permit2 = invoiceLine.Permits.AddNew();
			permit2.CSI_ReferenceNumber = "3";
			permit2.CSI_UnitOfQuantity = "4";
			permit2.CSI_Quantity = 20;

			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxGroup = Constants.RateCodes.Antidumping;
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			specialCaseTax.RateOrUnitValue = 12.34m;

			return invoiceLine;
		}

		public static void AssertImportInvoiceLineCloned(JobComInvoiceLine clonedInvoiceLine, JobComInvoiceLine invoiceLineToClone)
		{
			CombineAssertions(() =>
			{
				AssertEquals("PreviousDocuments cloned", invoiceLineToClone.PreviousDocuments.Count, clonedInvoiceLine.PreviousDocuments.Count);
				AssertEquals("PreviousDocuments.CSI_Code cloned", "001", clonedInvoiceLine.PreviousDocuments[0].CSI_Code);
				AssertEquals("PreviousDocuments.CSI_ReferenceNumber cloned", "1234", clonedInvoiceLine.PreviousDocuments[0].CSI_ReferenceNumber);
				AssertEquals("PreviousDocuments.CSI_ItemNumber cloned", 10, clonedInvoiceLine.PreviousDocuments[0].CSI_ItemNumber);

				AssertEquals("PreviousDocuments.CSI_Code cloned", "002", clonedInvoiceLine.PreviousDocuments[1].CSI_Code);
				AssertEquals("PreviousDocuments.CSI_ReferenceNumber cloned", "4567", clonedInvoiceLine.PreviousDocuments[1].CSI_ReferenceNumber);
				AssertEquals("PreviousDocuments.CSI_ItemNumber cloned", 20, clonedInvoiceLine.PreviousDocuments[1].CSI_ItemNumber);

				var clonedMercosulForeignDeclaration = clonedInvoiceLine.MercosulForeignDeclarations[0];
				AssertEquals("MercosulForeignDeclarations cloned", invoiceLineToClone.MercosulForeignDeclarations.Count, clonedInvoiceLine.MercosulForeignDeclarations.Count);
				AssertEquals("MercosulForeignDeclarationType cloned", "1", clonedInvoiceLine.MercosulForeignDeclarationType);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_Code cloned", "2", clonedMercosulForeignDeclaration.CSI_Code);
				AssertEquals("MercosulForeignDeclarationCollection.CSI_Quantity3 cloned", 1m, clonedMercosulForeignDeclaration.CSI_Quantity3);

				AssertEquals("Permits cloned", invoiceLineToClone.Permits.Count, clonedInvoiceLine.Permits.Count);
				AssertEquals("Permits.CSI_ReferenceNumber cloned", "1", clonedInvoiceLine.Permits[0].CSI_ReferenceNumber);
				AssertEquals("Permits.CSI_UnitOfQuantity cloned", "2", clonedInvoiceLine.Permits[0].CSI_UnitOfQuantity);
				AssertEquals("Permits.CSI_Quantity cloned", 10m, clonedInvoiceLine.Permits[0].CSI_Quantity);

				AssertEquals("Permits.CSI_ReferenceNumber cloned", "3", clonedInvoiceLine.Permits[1].CSI_ReferenceNumber);
				AssertEquals("Permits.CSI_UnitOfQuantity cloned", "4", clonedInvoiceLine.Permits[1].CSI_UnitOfQuantity);
				AssertEquals("Permits.CSI_Quantity cloned", 20m, clonedInvoiceLine.Permits[1].CSI_Quantity);

				Assert("Validation should be suspended", !clonedInvoiceLine.HasNotifications());
				Assert("HasChanges should not be set", !clonedInvoiceLine.HasChanges);

				var specialCaseTax = clonedInvoiceLine.SpecialCaseTaxes[0];
				AssertEquals("SpecalCaseTax TaxGroup cloned", Constants.RateCodes.Antidumping, specialCaseTax.TaxGroup);
				AssertEquals("SpecalCaseTax TaxType cloned", SpecialCaseTaxTypeList.Codes.AdValoremRate, specialCaseTax.TaxType);
				AssertEquals("SpecalCaseTax RateOrUnitValue cloned", 12.34m, specialCaseTax.RateOrUnitValue);
			});
		}
	}
}
