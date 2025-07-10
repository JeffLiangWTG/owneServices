using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.BR.Business.Constants;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AdditionalInformationGenerator))]
	class AdditionalInformationGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateAdditionalInformation()
		{
			ReferenceTestDataHelper.CreateCustomsOfficeCodes(Factory);
			ReferenceTestDataHelper.CreateCustomsEnclosureCodes(Factory);
			ReferenceTestDataHelper.CreateWarehousingSectorsCodes(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForICMSLegalBaseList(Factory);

			var pkgCodes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("01", "AMARRADO/ATADO/FEIXE"),
				new KeyValuePair<string, string>("02", "BARRICA DE FERRO"),
				new KeyValuePair<string, string>("03", "BARRICA DE FIBRA DE VIDRO")
			};

			ReferenceTestDataHelper.CreateRefCusCodeList(Factory, new KeyValuePair<string, string>(ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package"), pkgCodes);

			var refCurrecny = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			refCurrecny.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.2m);

			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "1501/0";
			refVessel.RV_RN_NKCountryOfReg = "CA";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "importer test";
			importer.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_DeclarationReference = "BJOBT";
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MasterBill = "5584584";
			declaration.JE_HouseBill = "5589578";
			declaration.JE_Folio = "001";
			declaration.JE_CargoArrivalDocumentNumber = "3369852";
			declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday;
			declaration.JE_DateAtFinalDestination = ZDateTime.BrettsBirthday;

			declaration.JE_CustomsOffice = "CO00001";
			declaration.JE_LocationOfGoods = "CE00001";
			declaration.JE_SubLocationOfGoods = "001";
			declaration.EntranceOfficeCode = "CO00002";

			declaration.JE_RL_NKPortOfLoading = "CAVAN";
			declaration.JE_RL_NKFinalDestination = "BRPOA";

			var groupHeader = declaration.AllGroupHeaders.FirstOrDefault() as JobComInvoiceGroupHeader;

			var freightPrepaid = groupHeader.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			freightPrepaid.J7_Amount = 40m;
			freightPrepaid.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var warehouseID = declaration.WarehouseAreas.AddNew();
			warehouseID.CY_Code = "00001";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 300m;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_NoOfPacks = 55;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var freightCollectHeader = invoiceHeader.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code);
			freightCollectHeader.J7_Amount = 20m;
			freightCollectHeader.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var insurence = invoiceHeader.Charges.AddNew(ImportChargesProvider.OverseasInsurance.Code);
			insurence.J7_Amount = 100m;
			insurence.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_CustomsValue = 500m;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_CustomsValue = 300m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.JI_Tariff = "10102020";
			invoiceLine1.DutyTaxRegime = "1";
			invoiceLine1.IPITaxRegime = "2";
			invoiceLine1.PisCofinsTaxRegime = "3";
			invoiceLine1.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine1.ICMSLegalBase = "01";
			invoiceLine1.ImportLicenseNumber = "TST1";
			invoiceLine1.JI_ICMSBaseValueReductionPercentage = 12m;
			invoiceLine1.JI_ICMSTotalAmountReductionPercentage = 20m;
			invoiceLine1.JI_ICMSRate = 10m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine1.DutyRateIsOverridden = true;

			AddExTariff(invoiceLine1);
			invoiceLine1.AdditionalTariffs.Rebuild();

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_NetWeight = 50m;
			invoiceLine2.JI_Tariff = "90909090";
			invoiceLine2.DutyTaxRegime = "1";
			invoiceLine2.IPITaxRegime = "2";
			invoiceLine2.PisCofinsTaxRegime = "3";
			invoiceLine2.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine2.ICMSLegalBase = "01";
			invoiceLine2.ImportLicenseNumber = "TST2";
			invoiceLine2.JI_ICMSBaseValueReductionPercentage = 12m;
			invoiceLine2.JI_ICMSTotalAmountReductionPercentage = 20m;
			invoiceLine2.JI_ICMSRate = 10m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.DutyRateIsOverridden = false;

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 2;
			package1.CW_PackType = "01";

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 5;
			package2.CW_PackType = "02";

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 10;
			package3.CW_PackType = "03";

			AddExTariff(invoiceLine2);
			declaration.ResumeApportionment();

			foreach (var entryLine in entryHeader.MergedLines)
			{
				var lineNumber = entryLine.CL_LineNumber;
				var lineFeeDuty = entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypesList.Codes.DTY);
				lineFeeDuty.CF_BaseValue = 9m + lineNumber;
				lineFeeDuty.CF_Rate = 1.333m + lineNumber;
				lineFeeDuty.CF_ChargeAmount = 19m + lineNumber;

				var lineFeeIPI = entryLine.Fees.GetOrAddFeeByFeeType(RateTypes.IPI);
				lineFeeIPI.CF_BaseValue = 9m + lineNumber;
				lineFeeIPI.CF_Rate = 4.333m + lineNumber;
				lineFeeIPI.CF_ChargeAmount = 49m + lineNumber;

				var lineFeePIS = entryLine.Fees.GetOrAddFeeByFeeType(RateTypes.PIS);
				lineFeePIS.CF_BaseValue = 19m + lineNumber;
				lineFeePIS.CF_Rate = 1.333m + lineNumber;
				lineFeePIS.CF_ChargeAmount = 39m + lineNumber;

				var lineFeeCofins = entryLine.Fees.GetOrAddFeeByFeeType(RateTypes.Cofins);
				lineFeeCofins.CF_BaseValue = 29m + lineNumber;
				lineFeeCofins.CF_Rate = 8.333m + lineNumber;
				lineFeeCofins.CF_ChargeAmount = 89m + lineNumber;

				var lineFeeAntidumping = entryLine.Fees.GetOrAddFeeByFeeType(RateTypes.Antidumping);
				lineFeeAntidumping.CF_BaseValue = 39m + lineNumber;
				lineFeeAntidumping.CF_Rate = 6.333m + lineNumber;
				lineFeeAntidumping.CF_ChargeAmount = 139m + lineNumber;

				var lineFeeSUF = entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee);
				lineFeeSUF.CF_ChargeAmount = 6.88m + lineNumber;

				var lineFeeAfrmm = entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax);
				lineFeeAfrmm.CF_ChargeAmount = 34m + lineNumber;

				var lineFeeICMS = entryLine.Fees.GetOrAddFeeByFeeType(RateTypes.ICMS);
				lineFeeICMS.CF_BaseValue = 9m + lineNumber;
				lineFeeICMS.CF_Rate = 3.333m + lineNumber;
				lineFeeICMS.CF_ChargeAmount = 39m + lineNumber;
			}

			entryLine1.Fees.AddOrUpdate(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode, 25m);
			entryLine2.Fees.AddOrUpdate(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode, 50m);

			AdditionalInformationGenerator additionalInformationGeneratingObjectParent;

			foreach (var transportMode in new string[] { TransportTypeList.Codes.Lake, TransportTypeList.Codes.Sea, TransportTypeList.Codes.River })
			{
				declaration.JE_TransportMode = transportMode;
				declaration.JE_VesselName = refVessel.RV_Code;
				declaration.JE_UCR = "447845";

				additionalInformationGeneratingObjectParent = new AdditionalInformationGenerator(declaration);

				entryInstruction.CEI_AdditionalInformationOption = ZString.Empty;

				additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();
				Assert("Entry Instruction Additional Information for " + transportMode + " should be empty",  entryInstruction.AdditionalInformation.IsEmpty);

				entryInstruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.OnlyFreeText;

				additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();
				AssertMultilineEquals("Entry Instruction Additional Information for " + transportMode, AdditionalInformationPreviewIMP, entryInstruction.AdditionalInformation, '\'');
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			declaration.JE_CustomsOffice = "CO00001";
			declaration.JE_LocationOfGoods = "CE00001";
			declaration.JE_SubLocationOfGoods = "001";
			declaration.EntranceOfficeCode = "CO00002";
			foreach (var transportMode in new string[] { TransportTypeList.Codes.Lake, TransportTypeList.Codes.Sea, TransportTypeList.Codes.River })
			{
				declaration.JE_TransportMode = transportMode;
				declaration.JE_VesselName = refVessel.RV_Code;
				declaration.JE_UCR = "447845";

				additionalInformationGeneratingObjectParent = new AdditionalInformationGenerator(declaration);

				entryInstruction.CEI_AdditionalInformationOption = ZString.Empty;

				additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();
				Assert("Entry Instruction Additional Information for " + transportMode + " should be empty", entryInstruction.AdditionalInformation.IsEmpty);

				entryInstruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.OnlyFreeText;

				additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();
				AssertMultilineEquals("Entry Instruction Additional Information for " + transportMode, AdditionalInformationPreviewISW, entryInstruction.AdditionalInformation, '\'');
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF253";
			additionalInformationGeneratingObjectParent = new AdditionalInformationGenerator(declaration);
			additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();
			AssertContains("Número do Vôo", "Número do Vôo: QF253", entryInstruction.AdditionalInformation);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_VoyageFlightNo = "QF253";
			additionalInformationGeneratingObjectParent = new AdditionalInformationGenerator(declaration);
			additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();
			AssertContains("Registro", "Registro: QF253", entryInstruction.AdditionalInformation);

			var additionTariff = invoiceLine1.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement);
			additionTariff.LegalActType = ZString.Empty;
			additionTariff.LegalActIssuingBody = ZString.Empty;
			additionTariff.LegalActNumber = ZString.Empty;
			additionTariff.LegalActYear = ZString.Empty;

			additionalInformationGeneratingObjectParent = new AdditionalInformationGenerator(declaration);
			additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();
			AssertContains("Empty Tariff Agreement", "Acordo Tarifario: Tariff Agreement - MX99 - 336 - Ex 001", entryInstruction.AdditionalInformation);

			AssertContains("Volume", "Volume: 2x AMARRADO/ATADO/FEIXE", entryInstruction.AdditionalInformation);
			AssertContains("Volume", "Volume: 5x BARRICA DE FERRO", entryInstruction.AdditionalInformation);
			AssertContains("Volume", "Volume: 10x BARRICA DE FIBRA DE VIDRO", entryInstruction.AdditionalInformation);

			declaration.Packages.RemoveAndDeleteAll();
			additionalInformationGeneratingObjectParent = new AdditionalInformationGenerator(declaration);
			additionalInformationGeneratingObjectParent.GenerateAdditionalInformation();

			AssertNotContains("Volume", "Volume: 2x AMARRADO/ATADO/FEIXE", entryInstruction.AdditionalInformation);
			AssertNotContains("Volume", "Volume: 5x BARRICA DE FERRO", entryInstruction.AdditionalInformation);
			AssertNotContains("Volume", "Volume: 10x BARRICA DE FIBRA DE VIDRO", entryInstruction.AdditionalInformation);
		}

		public void TestGenerateAdditionalInformationWithEmptyData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var additionalInformationGeneratingObjectParent = new AdditionalInformationGenerator(declaration);
			AssertNoExceptionThrown("No Exception Thrown", () => additionalInformationGeneratingObjectParent.GenerateAdditionalInformation());
		}

		void AddExTariff(JobComInvoiceLine invoiceLine)
		{
			var tariffDetailExduty = GetOrAddTariffDetail(AdditionalTaxTypeList.Codes.ExDutyTariff, invoiceLine.CusLineTariffDetails);
			tariffDetailExduty.BZ_Type = ChildTariffTypeList.Codes.LEBIT;

			GetOrAddTariffDetail(AdditionalTaxTypeList.Codes.ExIPITariff, invoiceLine.CusLineTariffDetails);

			var tariffDetailOMC = GetOrAddTariffDetail(AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine.CusLineTariffDetails);
			tariffDetailOMC.BZ_Type = "MX99";

			AddOrUpdateLegalActInfo(AdditionalTaxTypeList.Codes.ExDutyTariff, invoiceLine.LegalActInfos);

			AddOrUpdateLegalActInfo(AdditionalTaxTypeList.Codes.ExIPITariff, invoiceLine.LegalActInfos);

			AddOrUpdateLegalActInfo(AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine.LegalActInfos);
		}

		void AddOrUpdateLegalActInfo(string subject, LegalActInfoCollection collection)
		{
			var legalAct = collection.FindBySubject(subject) ?? collection.AddNew(subject);
			legalAct.CSI_IssuerType = "123";
			legalAct.CSI_Code = "000";
			legalAct.CSI_ReferenceNumber = "456";
			legalAct.CSI_YearOfIssue = "2023";
		}

		CusLineTariffDetail GetOrAddTariffDetail(string subject, ICusLineTariffDetailCollection<CusLineTariffDetail> collection)
		{
			var tariffDetail = collection.FirstOrDefault(x => x.BZ_LegalActSubject == subject) ?? collection.AddNew();
			tariffDetail.BZ_LegalActSubject = subject;
			tariffDetail.BZ_Tariff = "09022000_001";
			return tariffDetail;
		}

		string AdditionalInformationPreviewISW => @$"Processo: BJOBT
Importador: importer test
CNPJ: 58.500.398/0001-05

Master: 5584584
House: 5589578
Conhecimento Eletrônico: 447845
Embarcação: 1501/0
Bandeira: Canadá
Folio: 001
Número do Manifesto: 3369852
Porto de Origem: Vancouver
Previsão de Saída: 18/09/1971
Porto de Destino: Porto Alegre
Previsão de Chegada: 18/09/1971

Volume: 0x 
Volume: 2x AMARRADO/ATADO/FEIXE
Volume: 5x BARRICA DE FERRO
Volume: 10x BARRICA DE FIBRA DE VIDRO

Local de Desembaraço
URF: CO00001 - Customs Office Test1
Recinto Aduaneiro: CE00001 - Customs Enclosure Test1

Setor: 001 - Warehousing Sector Test1
Área: 00001

Local de Entrada
URF: CO00002 - Customs Office Test2

Total do FOB: 1500 BRL
Total do Frete (Collect): 100 BRL
Total do Frete (Prepaid): 40.0 BRL
Seguro: 100.00 BRL

Total do II: 41 BRL
Total do IPI: 101 BRL
Total do PIS: 81 BRL
Total do COFINS: 181 BRL
Total do ICMS: 81 BRL
Total do Antidumping: 281 BRL

Adição 1 NCM: 10102020
Número do Licenciamento: TST1
Valor FOB: 200 USD
Valor Aduaneiro: 500 BRL

Imposto de Importação (II) Ex-Tarifario: EX Duty Tariff - LEBIT - Ex 001 - Ato Legal: 000 - 123 - 456 - 2023
IPI Ex-Tarifario: EX IPI Tariff - Ex 001 - Ato Legal: 000 - 123 - 456 - 2023
Acordo Tarifario: Tariff Agreement - MX99 - 336 - Ato Legal: 000 - 123 - 456 - 2023 - Ex 001

II Regime de Tributação: Recolhimento Integral Base de Cálculo: 10 BRL Alíquota: 2.33 Valor Total: 20 BRL
IPI Regime de Tributação: Redução Base de Cálculo: 10 BRL Alíquota: 5.33 Valor Total: 50 BRL
PIS Regime de Tributação: Isenção Base de Cálculo: 20 BRL Alíquota: 2.33 Valor Total: 40 BRL
COFINS Regime de Tributação: Isenção Base de Cálculo: 30 BRL Alíquota: 9.33 Valor Total: 90 BRL
Antidumping Base de Cálculo: 40 BRL Alíquota: 7.33 Valor Total: 140 BRL
Taxa de Utilização do Siscomex: 7.88 BRL
Valor da Multa do Licenciamento: 25 BRL
AFRMM: 35 BRL
ICMS Regime de Tributação: Redução Base de Cálculo: 10 BRL Alíquota: 4.33 Redução da Base de Cálculo: 12 Redução do Valor Total: 20 Base Legal: ICMS Legal Base 01 Valor Total: 40 BRL

Adição 2 NCM: 90909090
Número do Licenciamento: TST2
Valor FOB: 100 USD
Valor Aduaneiro: 300 BRL

Imposto de Importação (II) Ex-Tarifario: EX Duty Tariff - LEBIT - Ex 001 - Ato Legal: 000 - 123 - 456 - 2023
IPI Ex-Tarifario: EX IPI Tariff - Ex 001 - Ato Legal: 000 - 123 - 456 - 2023
Acordo Tarifario: Tariff Agreement - MX99 - 336 - Ato Legal: 000 - 123 - 456 - 2023 - Ex 001

II Regime de Tributação: Recolhimento Integral Base de Cálculo: 11 BRL Alíquota: 3.33 Valor Total: 21 BRL
IPI Regime de Tributação: Redução Base de Cálculo: 11 BRL Alíquota: 6.33 Valor Total: 51 BRL
PIS Regime de Tributação: Isenção Base de Cálculo: 21 BRL Alíquota: 3.33 Valor Total: 41 BRL
COFINS Regime de Tributação: Isenção Base de Cálculo: 31 BRL Alíquota: 10.33 Valor Total: 91 BRL
Antidumping Base de Cálculo: 41 BRL Alíquota: 8.33 Valor Total: 141 BRL
Taxa de Utilização do Siscomex: 8.88 BRL
Valor da Multa do Licenciamento: 50 BRL
AFRMM: 36 BRL
ICMS Regime de Tributação: Redução Base de Cálculo: 11 BRL Alíquota: 5.33 Redução da Base de Cálculo: 12 Redução do Valor Total: 20 Base Legal: ICMS Legal Base 01 Valor Total: 41 BRL";

		string AdditionalInformationPreviewIMP => @$"Processo: BJOBT
Importador: importer test
CNPJ: 58.500.398/0001-05

Master: 5584584
House: 5589578
Conhecimento Eletrônico: 447845
Embarcação: 1501/0
Bandeira: Canadá
Folio: 001
Número do Manifesto: 3369852
Porto de Origem: Vancouver
Previsão de Saída: 18/09/1971
Porto de Destino: Porto Alegre
Previsão de Chegada: 18/09/1971

Volume: 0x 
Volume: 2x AMARRADO/ATADO/FEIXE
Volume: 5x BARRICA DE FERRO
Volume: 10x BARRICA DE FIBRA DE VIDRO

Local de Desembaraço
URF: CO00001 - Customs Office Test1
Recinto Aduaneiro: CE00001 - Customs Enclosure Test1

Setor: 001 - Warehousing Sector Test1
Área: 00001

Local de Entrada
URF: CO00002 - Customs Office Test2

Total do FOB: 1500 BRL
Total do Frete (Collect): 100 BRL
Total do Frete (Prepaid): 40.0 BRL
Seguro: 100.00 BRL

Total do II: 41 BRL
Total do IPI: 101 BRL
Total do PIS: 81 BRL
Total do COFINS: 181 BRL
Total do ICMS: 81 BRL
Total do Antidumping: 281 BRL";
	}
}
