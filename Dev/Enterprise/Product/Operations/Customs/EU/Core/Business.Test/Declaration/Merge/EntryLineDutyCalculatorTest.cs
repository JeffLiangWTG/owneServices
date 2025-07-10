using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryLine = Enterprise.Customs.EU.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(EntryLineDutyCalculator))]
	sealed class EntryLineDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<CusEntryLine, EUUniversalRateCalcData>
	{
		public void TestCalculateWithNoReplaceableFormulaExpressions()
		{
			// Set up universal reference data
			(var stdTradeGroup, var stdPreference, var dtyTariff, var dtyRateType) = PopulateTestReferenceData();
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "IF(HAS(\"CERT\",\"D2\"),0.5 * [KGM],0.2 * [KGM] + 0.3 * VFD)", preferencePk: stdPreference.PK);
			tariffDtyRateA00.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add(RefCusCodeListAttributeTypes.Codes.Level, new string[] { UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item });
			RefDataHelper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.Latvia,
				new string[] { importCodeType, exportCodeType }, "D2", "D2 DESC", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			// Set up entry line with invoice line data
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_CustomsQuantity = 100;
			invLine1.JI_CustomsUnitQty = "KGM";
			invLine1.JI_CustomsSecondQuantity = 1000;
			invLine1.JI_CustomsSecondUnitQty = "GRM";
			invLine1.JI_CustomsThirdQuantity = 50;
			invLine1.JI_CustomsThirdUnitQty = "KGM";

			var supportingDocument1 = invLine1.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Type = "XXX";
			supportingDocument1.CSI_Code = "D1";

			var invLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine2.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine2.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine2.JI_CustomsQuantity = 400;
			invLine2.JI_CustomsUnitQty = "KGM";

			var invLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine3.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine3.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine3.JI_CustomsQuantity = 10;
			invLine3.JI_CustomsUnitQty = "KGM";
			invLine3.JI_CustomsSecondQuantity = 300;
			invLine3.JI_CustomsSecondUnitQty = "KGM";

			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invLine1);
			entryLine.InvoiceLines.Add(invLine2);
			entryLine.InvoiceLines.Add(invLine3);
			entryLine.CL_CustomsValue = 20;

			var dutyRateForCalculation = entryLine.RandomLine.UniversalDutyRate;

			CalculateAndAssert(
				entryLine,
				dutyRateForCalculation,
				expectedFinalResultAmount: 108m,
				expectedIntermediateResults: new[]
				{
					new DutyCalculationIntermediateResult(102m, 0.2m, 510m, "KGM"),
					new DutyCalculationIntermediateResult(6m, 0.3m, 20m, "%")
			});

			// Add SUP document so the HAS expression yields true
			var supportingDocument2 = invLine1.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Type = "SUP";
			supportingDocument2.CSI_Code = "D2";

			CalculateAndAssert(
				entryLine,
				dutyRateForCalculation,
				expectedFinalResultAmount: 255m,
				expectedIntermediateResults: new[]
				{
					new DutyCalculationIntermediateResult(255m, 0.5m, 510m, "KGM"),
				}
			);
		}

		public void TestCalculateWithFormulaCleansing()
		{
			// Set up universal reference data
			var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData();
			var meuTariff = CreateMeursingTariff();
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var rateCodeDtyA10 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, dtyRateType.PK);
			var rateCodeEa = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent, dtyRateType.PK);
			var rateCodeAdfm = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents, dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "MIN(VFD*0.2 + #EA(1)#, VFD*0.8 + #EAR(1)#) + MIN(VFD*0.9 + #EA(1)#, VFD*0.3 + #ADFM(1)#) + #ADFM(2)#", preferencePk: stdPreference.PK);
			tariffDtyRateA00.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffDtyRateA10 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA10.PK, startDate, endDate, rateFormula: "VFD*0.1 + #ADFM(1)#", preferencePk: stdPreference.PK);
			tariffDtyRateA10.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA10.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffMeuRateEa = RefDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeEa.PK, startDate, endDate, rateFormula: "2*[LTR]");
			tariffMeuRateEa.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffMeuRateEa.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");
			var tariffMeuRateAdfm = RefDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeAdfm.PK, startDate, endDate, rateFormula: "0.4*[KGM]");
			tariffMeuRateAdfm.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffMeuRateAdfm.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");

			Factory.Save();

			// Set up entry line with invoice line data
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_SupplementaryCode1 = "7023";
			invLine1.JI_CustomsQuantity = 10;
			invLine1.JI_CustomsUnitQty = "KG";
			invLine1.JI_CustomsSecondQuantity = 0.02;
			invLine1.JI_CustomsSecondUnitQty = "HLT";

			var invLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine2.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine2.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine2.JI_CustomsQuantity = 40;
			invLine2.JI_CustomsUnitQty = "KG";
			invLine2.JI_CustomsSecondQuantity = 0.03;
			invLine2.JI_CustomsSecondUnitQty = "HLT";

			var entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.InvoiceLines.Add(invLine2);
			entryLine.CL_CustomsValue = 20;

			CalculateAndAssert(
				entryLine,
				tariffDtyRateA00.PK,
				expectedFinalResultAmount: 40m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(4m, 0.2m, 20m, "%"),
					new DutyCalculationIntermediateResult(10m, 2m, 5m, "LTR"),
					new DutyCalculationIntermediateResult(6m, 0.3m, 20m, "%"),
					new DutyCalculationIntermediateResult(20m, 0.4m, 50m, "KGM"),
				}
			);

			CalculateAndAssert(
				entryLine,
				tariffDtyRateA10.PK,
				expectedFinalResultAmount: 22m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(2m, 0.1m, 20m, "%"),
					new DutyCalculationIntermediateResult(20m, 0.4m, 50m, "KGM"),
				}
			);
		}

		public void TestCalculateWithFormulaCleansingForTariffDataGrouping()
		{
			var tariffDataGroup = "ABC";
			// Set up universal reference data
			var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData(tariffDataGroup);
			var meuTariff = CreateMeursingTariff(tariffDataGroup);
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var rateCodeDtyA10 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, dtyRateType.PK);
			var rateCodeEa = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent, dtyRateType.PK);
			var rateCodeAdfm = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents, dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "MIN(VFD*0.2 + #EA(1)#, VFD*0.8 + #EAR(1)#) + MIN(VFD*0.9 + #EA(1)#, VFD*0.3 + #ADFM(1)#) + #ADFM(2)#", preferencePk: stdPreference.PK);
			tariffDtyRateA00.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffDtyRateA10 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA10.PK, startDate, endDate, rateFormula: "VFD*0.1 + #ADFM(1)#", preferencePk: stdPreference.PK);
			tariffDtyRateA10.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA10.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffMeuRateEa = RefDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeEa.PK, startDate, endDate, rateFormula: "2*[LTR]");
			tariffMeuRateEa.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffMeuRateEa.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");
			var tariffMeuRateAdfm = RefDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeAdfm.PK, startDate, endDate, rateFormula: "0.4*[KGM]");
			tariffMeuRateAdfm.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffMeuRateAdfm.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");

			Factory.Save();

			// Set up entry line with invoice line data
			var declaration = Factory.New<Business.Declaration.Testing.TariffDataGroupingDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.TariffDataGrouping = tariffDataGroup;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_SupplementaryCode1 = "7023";
			invLine1.JI_CustomsQuantity = 10;
			invLine1.JI_CustomsUnitQty = "KG";
			invLine1.JI_CustomsSecondQuantity = 0.02;
			invLine1.JI_CustomsSecondUnitQty = "HLT";

			var invLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine2.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine2.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine2.JI_CustomsQuantity = 40;
			invLine2.JI_CustomsUnitQty = "KG";
			invLine2.JI_CustomsSecondQuantity = 0.03;
			invLine2.JI_CustomsSecondUnitQty = "HLT";

			var entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.InvoiceLines.Add(invLine2);
			entryLine.CL_CustomsValue = 20;

			CalculateAndAssert(
				entryLine,
				tariffDtyRateA00.PK,
				expectedFinalResultAmount: 40m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(4m, 0.2m, 20m, "%"),
					new DutyCalculationIntermediateResult(10m, 2m, 5m, "LTR"),
					new DutyCalculationIntermediateResult(6m, 0.3m, 20m, "%"),
					new DutyCalculationIntermediateResult(20m, 0.4m, 50m, "KGM"),
				}
			);

			CalculateAndAssert(
				entryLine,
				tariffDtyRateA10.PK,
				expectedFinalResultAmount: 22m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(2m, 0.1m, 20m, "%"),
					new DutyCalculationIntermediateResult(20m, 0.4m, 50m, "KGM"),
				}
			);
		}

		public void TestCalculateWithConvertibleUnitOfMeasure()
		{
			// Set up universal reference data
			var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData();
			var meuTariff = CreateMeursingTariff();
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var rateCodeDtyA10 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, dtyRateType.PK);
			var rateCodeAdfm = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents, dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "VFD*0.8 + 4*[KLT] + VFD*0.3 + #ADFM(1)#", preferencePk: stdPreference.PK);
			tariffDtyRateA00.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffDtyRateA10 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA10.PK, startDate, endDate, rateFormula: "VFD*0.1 + #ADFM(1)#", preferencePk: stdPreference.PK);
			tariffDtyRateA10.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA10.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffMeuRateAdfm = RefDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeAdfm.PK, startDate, endDate, rateFormula: "0.011*[GRM]");
			tariffMeuRateAdfm.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffMeuRateAdfm.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");

			Factory.Save();

			// Set up entry line with invoice line data
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_SupplementaryCode1 = "7023";
			invLine1.JI_CustomsQuantity = 3;
			invLine1.JI_CustomsUnitQty = "KGM";
			invLine1.JI_CustomsSecondQuantity = 200;
			invLine1.JI_CustomsSecondUnitQty = "LTR";

			var invLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine2.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine2.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine2.JI_CustomsQuantity = 4;
			invLine2.JI_CustomsUnitQty = "KGM";
			invLine2.JI_CustomsSecondQuantity = 300;
			invLine2.JI_CustomsSecondUnitQty = "LTR";

			var entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.InvoiceLines.Add(invLine2);
			entryLine.CL_CustomsValue = 20;

			CalculateAndAssert(
				entryLine,
				tariffDtyRateA00.PK,
				expectedFinalResultAmount: 101m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(16m, 0.8m, 20m, "%"),
					new DutyCalculationIntermediateResult(2m, 4m, 0.5m, "KLT"),
					new DutyCalculationIntermediateResult(6m, 0.3m, 20m, "%"),
					new DutyCalculationIntermediateResult(77m, 0.011m, 7000m, "GRM"),
				}
			);

			CalculateAndAssert(
				entryLine,
				tariffDtyRateA10.PK,
				expectedFinalResultAmount: 79m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(2m, 0.1m, 20m, "%"),
					new DutyCalculationIntermediateResult(77m, 0.011m, 7000m, "GRM"),
				}
			);
		}

		public void TestCalculateWithNoMeasuringTariff()
		{
			// Set up universal reference data
			var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData();
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			rateCodeDtyA00.Factory.Save();
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "MIN(0.5*[KGM] + #EA(1)#, 0.6*[KGM])", preferencePk: stdPreference.PK);
			tariffDtyRateA00.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate, "7023");

			Factory.Save();

			// Set up entry line with invoice line data
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_SupplementaryCode1 = "7023";
			invLine1.JI_CustomsQuantity = 10;
			invLine1.JI_CustomsUnitQty = "KGM";

			var entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();

			entryLine.InvoiceLines.Add(invLine1);

			var dutyRateForCalculation = entryLine.RandomLine.UniversalDutyRate;

			CalculateAndAssert(
				entryLine,
				dutyRateForCalculation,
				expectedFinalResultAmount: 5m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(5m, 0.5m, 10m, "KGM"),
				}
			);
		}

		public void TestCalculateWithNoMeasuringRateApplicableToReplaceableExpressionAdditionalCodes()
		{
			// Set up universal reference data
			var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData();
			var meuTariff = CreateMeursingTariff();
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var rateCodeEar = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "EAR", dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "IF(VFD*0.2 + #EAR(2)# > VFD*0.3 + #EAR(3)#, [KGM], [HLT])", preferencePk: stdPreference.PK);
			tariffDtyRateA00.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate, "7023");
			var tariffMeuRateEar = RefDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeEar.PK, startDate, endDate, rateFormula: "VFD");
			tariffMeuRateEar.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffMeuRateEar.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");

			Factory.Save();

			// Set up entry line with invoice line data
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_SupplementaryCode1 = "7023";
			invLine1.JI_CustomsQuantity = 25;
			invLine1.JI_CustomsUnitQty = "KGM";
			invLine1.JI_CustomsSecondQuantity = 5;
			invLine1.JI_CustomsSecondUnitQty = "HLT";

			var entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 1;

			var dutyRateForCalculation = entryLine.RandomLine.UniversalDutyRate;

			CalculateAndAssert(
				entryLine,
				dutyRateForCalculation,
				expectedFinalResultAmount: 5m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(5m, 1m, 5m, "HLT"),
				}
			);
		}

		public void TestCalculateWithNoSupplementaryCode()
		{
			// Set up universal reference data
			var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData();
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "VFD*0.5 + #EA(1)#", preferencePk: stdPreference.PK);
			tariffDtyRateA00.Factory.Save();
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate);

			Factory.Save();

			// Set up entry line with invoice line data
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;

			var entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 10;
			var applicableDutyRate = entryLine.RandomLine.UniversalDutyRate;

			// No supplementary code means formula replaceable tokens (in this case #EA(1)#) are replaced with 0 (zero)
			CalculateAndAssert(
				entryLine,
				applicableDutyRate,
				expectedFinalResultAmount: 5m,
				expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
				{
					new DutyCalculationIntermediateResult(5m, 0.5m, 10m, "%"),
				}
			);
		}

		protected override UniversalDutyCalculator<CusEntryLine, EUUniversalRateCalcData> CreateDutyCalculator()
		{
			return new EntryLineDutyCalculator(Factory.New<CusEntryLine>(), RateCalculationVisitorMode.Default);
		}

		void CalculateAndAssert(CusEntryLine entryLine, ZGuid refCusRatePk, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedIntermediateResults, ErrorInformation[] expectedErrors = null)
		{
			var dutyRateForCalculation = Factory.Load<RateView>(refCusRatePk);
			CalculateAndAssert(entryLine, dutyRateForCalculation, expectedFinalResultAmount, expectedIntermediateResults, expectedErrors);
		}

		void CalculateAndAssert(CusEntryLine entryLine, RateView dutyRateForCalculation, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedIntermediateResults, ErrorInformation[] expectedErrors = null)
		{
			var assertionMessage = $"Calculation Result\r\nFormula = {dutyRateForCalculation.ZZ2_RateFormula}\r\n";
			var dutyCalculator = new EntryLineDutyCalculatorForTesting(entryLine);
			var calculationResult = dutyCalculator.CleanFormulaAndCalculate(dutyRateForCalculation);
			RateCalculationVisitorTestBase.AssertCalculation(assertionMessage, calculationResult, expectedFinalResultAmount, expectedIntermediateResults, expectedErrors);
		}

		(CusRefTradeGroupView, CusRefPreferenceView, TariffView, RefCusRateType) PopulateTestReferenceData(string overrideDataGrouping = "")
		{
			var grouping = GetDataGroupingCode(overrideDataGrouping);
			var stdTradeGroup = RefDataHelper.CreateTradeGroup(grouping, "STANDARD", startDate, endDate);
			var impTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(grouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var stdPreference = RefDataHelper.CreatePreferenceView("STD", "Standard", grouping);
			Factory.Save();
			var dtyTariff = RefDataHelper.CreateTariff(grouping, impTariffType.PK, "1111111", startDate, endDate, taxOrFeeCode: "DTY");
			var dtyRateType = RefDataHelper.CreateNewOrGetExistingRateType(grouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
			return (stdTradeGroup, stdPreference, dtyTariff, dtyRateType);
		}

		TariffView CreateMeursingTariff(string overrideDataGrouping = "")
		{
			var grouping = GetDataGroupingCode(overrideDataGrouping);
			var meuTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(grouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.MeursingTariff);
			Factory.Save();
			var meuTariff = RefDataHelper.CreateTariff(grouping, meuTariffType.PK, "7023", startDate, endDate);
			return meuTariff;
		}

		class EntryLineDutyCalculatorForTesting : EntryLineDutyCalculator
		{
			public EntryLineDutyCalculatorForTesting(CusEntryLine entryLine) : base(entryLine, RateCalculationVisitorMode.Default)
			{
			}

			public IDutyCalculationResult CalculateFromFormula(ZString rateFormula, RateView rateViewForCalculation) => Calculate(rateFormula, rateViewForCalculation);
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;

		ZString GetDataGroupingCode(string overrideDataGrouping) => string.IsNullOrEmpty(overrideDataGrouping) ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : new ZString(overrideDataGrouping);

		readonly ZDateTime startDate = ZDateTime.Today.AddYears(-1);
		readonly ZDateTime endDate = ZDateTime.Today.AddYears(1);
	}
}
