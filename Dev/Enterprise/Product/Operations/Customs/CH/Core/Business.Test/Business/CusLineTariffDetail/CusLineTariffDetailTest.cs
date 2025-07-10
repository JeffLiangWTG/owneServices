using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusLineTariffDetail))]
internal class CusLineTariffDetailTest : Customs.Business.Testing.CusLineTariffDetailTest
{
	public void TestCusLineTariffDetail()
	{
		var cusLineTariffDetail = Factory.New<CusLineTariffDetail>();

		AssertType<CusLineTariffDetailLookups>("Lookups", cusLineTariffDetail.Lookups);
		AssertType<CusLineTariffDetailValidation>("Validation", cusLineTariffDetail.Validation);
	}

	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		var bizobj = Factory.New<CusLineTariffDetail>();
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(bizobj, "CHCusLineTariffDetail");
	}

	public void TestCaptions_ADT()
	{
		var cusLineTariffDetail = Factory.New<CusLineTariffDetail>();
		CombineAssertions(() =>
		{
			var type = UniversalReferenceConstants.RateTypes.AdditionalTaxes;
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_TaxTypeInfo, type, "Type", fullDescription: "Additional Tax Type");
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_TariffInfo, type, "Code", fullDescription: "Additional Tax Code");
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_Qty1Info, type, "Quantity", fullDescription: "Additional Tax Quantity");
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_UQ1Info, type, "UOM", fullDescription: "Unit Of Measurement");
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_AlcoholPercentageInfo, type, "Alcohol Percentage", fullDescription: "Alcohol Percentage for Spirit Tax");
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_BaseValueInfo, type, "Base Value", fullDescription: "Value for ad-valorem tax (ex. 660 additional tax)");
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_ManualRateInfo, type, "Manual Rate", fullDescription: "Manual Rate (ex. Tobacco additional tax rate)");
			CaptionTestHelper.AssertCaptions(cusLineTariffDetail.BZ_ValueInfo, type, "Tax Amount", fullDescription: "Calculated Additional Tax Amount");
		});
	}

	public void TestCaptions_FEE()
	{
		CombineAssertions(() =>
		{
			var type = UniversalReferenceConstants.RateTypes.AdditionalFees;
			CaptionTestHelper.AssertCaptions(CusLineTariffDetail.BZ_TariffInfo, type, caption: "Type", fullDescription: "Fee Type");
			CaptionTestHelper.AssertCaptions(CusLineTariffDetail.DescriptionInfo, type, caption: "Description", fullDescription: "Fee Type Description");
			CaptionTestHelper.AssertCaptions(CusLineTariffDetail.BZ_Qty1Info, type, caption: "Quantity", fullDescription: "Fee Quantity");
			CaptionTestHelper.AssertCaptions(CusLineTariffDetail.BZ_ManualRateInfo, type, caption: "Rate", fullDescription: "Fee Rate");
			CaptionTestHelper.AssertCaptions(CusLineTariffDetail.BZ_ValueInfo, type, caption: "Fee Amount", fullDescription: "Calculated Additional Fee Amount");
		});
	}

	public void TestDescription()
	{
		new RefCusRateTestHelper(Factory).CreateFeeRateCodes();

		CusLineTariffDetail.BZ_Type = RateTypes.AdditionalFees;

		CombineAssertions(() =>
		{
			CusLineTariffDetail.BZ_Tariff = ZString.Empty;
			AssertEquals("Empty", ZString.Empty, CusLineTariffDetail.Description);
			CusLineTariffDetail.BZ_Tariff = RefCusRateTestHelper.ValidFeeRateCode;
			AssertEquals("Valid code", nameof(RefCusRateTestHelper.ValidFeeRateCode), CusLineTariffDetail.Description);
			CusLineTariffDetail.BZ_Tariff = RefCusRateTestHelper.InvalidFeeRateCode;
			AssertEquals("Invalid code", ZString.Empty, CusLineTariffDetail.Description);
		});
	}

	public void TestBZ_Value()
	{
		new RefCusRateTestHelper(Factory).CreateFeeRateCodes();

		CusLineTariffDetail.BZ_Type = RateTypes.AdditionalFees;

		CombineAssertions(() =>
		{
			CusLineTariffDetail.BZ_Tariff = FeeRateCodes.CustomsReliefControlTax;
			CusLineTariffDetail.BZ_Qty1 = 2m;
			CusLineTariffDetail.BZ_ManualRate = 3m;
			AssertEquals(GetAssertionMessage(), 7m, CusLineTariffDetail.BZ_Value);
			CusLineTariffDetail.BZ_ManualRate = 4m;
			AssertEquals(GetAssertionMessage(), 8m, CusLineTariffDetail.BZ_Value);

			CusLineTariffDetail.BZ_Tariff = RefCusRateTestHelper.ValidFeeRateCode;
			CusLineTariffDetail.BZ_Qty1 = 2m;
			CusLineTariffDetail.BZ_ManualRate = 3m;
			AssertEquals(GetAssertionMessage(), 6m, CusLineTariffDetail.BZ_Value);
			CusLineTariffDetail.BZ_ManualRate = 4m;
			AssertEquals(GetAssertionMessage(), 8m, CusLineTariffDetail.BZ_Value);
		});

		string GetAssertionMessage() => $"BZ_Tariff={CusLineTariffDetail.BZ_Tariff} BZ_Qty1={CusLineTariffDetail.BZ_Qty1} BZ_ManualRate={CusLineTariffDetail.BZ_ManualRate}";
	}

	public void TestReadonly_ADT()
	{
		CusLineTariffDetail.BZ_Type = UniversalReferenceConstants.RateTypes.AdditionalTaxes;
		Assert("BZ_TaxType.ReadOnly", CusLineTariffDetail.BZ_TaxTypeInfo.ReadOnly);
		Assert("BZ_UQ1.ReadOnly", CusLineTariffDetail.BZ_UQ1Info.ReadOnly);
		Assert("BZ_Value.ReadOnly", CusLineTariffDetail.BZ_ValueInfo.ReadOnly);
		Assert("BZ_BaseValue.ReadOnly", CusLineTariffDetail.BZ_BaseValueInfo.ReadOnly);
	}

	public void TestReadonly_FEE()
	{
		CusLineTariffDetail.BZ_Type = UniversalReferenceConstants.RateTypes.AdditionalFees;
		Assert("BZ_Value.ReadOnly", CusLineTariffDetail.BZ_ValueInfo.ReadOnly);
	}

	public void TestMultipleKeysToUse()
	{
		var multipleKeySupport = (ISupportMultipleResourceStringData)CusLineTariffDetail;
		CombineAssertions(() =>
		{
			CusLineTariffDetail.BZ_Type = UniversalReferenceConstants.RateTypes.AdditionalTaxes;
			AssertSequencesEqual($"BZ_Type={CusLineTariffDetail.BZ_Type}", new[] { UniversalReferenceConstants.RateTypes.AdditionalTaxes }, multipleKeySupport.MultipleKeysToUse);
			CusLineTariffDetail.BZ_Type = UniversalReferenceConstants.RateTypes.AdditionalFees;
			AssertSequencesEqual($"BZ_Type={CusLineTariffDetail.BZ_Type}", new[] { UniversalReferenceConstants.RateTypes.AdditionalFees }, multipleKeySupport.MultipleKeysToUse);
		});
	}

	public void TestIsAdditionalFee()
	{
		CombineAssertions(() =>
		{
			AssertEquals("BZ_Type=empty", false, CusLineTariffDetail.IsAdditionalFee);

			CusLineTariffDetail.BZ_Type = RateTypes.AdditionalFees;
			AssertEquals("BZ_Type=FEE", true, CusLineTariffDetail.IsAdditionalFee);

			CusLineTariffDetail.BZ_Type = RateTypes.AdditionalTaxes;
			AssertEquals("BZ_Type=another type", false, CusLineTariffDetail.IsAdditionalFee);
		});
	}

	public void TestIsQuantityBasedAdditionalTax()
	{
		var invoiceLine = CreateImportInvoiceLine();
		var testCusLineTariffDetail = invoiceLine.AdditionalTaxes.AddNew();
		testCusLineTariffDetail.BZ_Tariff = ZString.Empty;
		AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", false, testCusLineTariffDetail.IsQuantityBasedAdditionalTax);
		testCusLineTariffDetail.BZ_Tariff = "450-000";
		AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", true, testCusLineTariffDetail.IsQuantityBasedAdditionalTax);
		testCusLineTariffDetail.BZ_Tariff = "451-000";
		AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", false, testCusLineTariffDetail.IsQuantityBasedAdditionalTax);
		testCusLineTariffDetail.BZ_Type = ZString.Empty;
		testCusLineTariffDetail.BZ_Tariff = "450-000";
		AssertEquals("BZ_Type = empty", false, testCusLineTariffDetail.IsQuantityBasedAdditionalTax);
	}

	public void TestIsAdditionalTax()
	{
		CombineAssertions(() =>
		{
			AssertEquals("BZ_Type=empty", false, CusLineTariffDetail.IsAdditionalTax);

			CusLineTariffDetail.BZ_Type = RateTypes.AdditionalTaxes;
			AssertEquals("BZ_Type=ADT", true, CusLineTariffDetail.IsAdditionalTax);

			CusLineTariffDetail.BZ_Type = RateTypes.AdditionalTaxes;
			AssertEquals("BZ_Type=another type", false, CusLineTariffDetail.IsAdditionalFee);
		});
	}

	public void TestIsAdditionalTaxApplied()
	{
		CombineAssertions(() =>
		{
			CusLineTariffDetail.BZ_Tariff = ZString.Empty;
			AssertEquals($"BZ_Tariff={CusLineTariffDetail.BZ_Tariff}", false, CusLineTariffDetail.IsAdditionalTaxApplied);
			CusLineTariffDetail.BZ_Tariff = "290-000";
			AssertEquals($"BZ_Tariff={CusLineTariffDetail.BZ_Tariff}", false, CusLineTariffDetail.IsAdditionalTaxApplied);
			CusLineTariffDetail.BZ_Tariff = "290-001";
			AssertEquals($"BZ_Tariff={CusLineTariffDetail.BZ_Tariff}", true, CusLineTariffDetail.IsAdditionalTaxApplied);
		});
	}

	public void TestIsSOTAAdditionalTax()
	{
		CombineAssertions(() =>
		{
			var invoiceLine = CreateImportInvoiceLine();
			var testCusLineTariffDetail = invoiceLine.AdditionalTaxes.AddNew();
			testCusLineTariffDetail.BZ_Tariff = ZString.Empty;
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", false, testCusLineTariffDetail.IsSOTAAdditionalTax);
			testCusLineTariffDetail.BZ_Tariff = "465-000";
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", false, testCusLineTariffDetail.IsSOTAAdditionalTax);
			testCusLineTariffDetail.BZ_Tariff = "465-002";
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", true, testCusLineTariffDetail.IsSOTAAdditionalTax);
			testCusLineTariffDetail.BZ_Tariff = "465-202";
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", true, testCusLineTariffDetail.IsSOTAAdditionalTax);
			testCusLineTariffDetail.BZ_Type = ZString.Empty;
			AssertEquals("BZ_Type = empty", false, testCusLineTariffDetail.IsSOTAAdditionalTax);
		});
	}

	public void TestIsPreventionAdditionalTax()
	{
		CombineAssertions(() =>
		{
			var invoiceLine = CreateImportInvoiceLine();
			var testCusLineTariffDetail = invoiceLine.AdditionalTaxes.AddNew();
			testCusLineTariffDetail.BZ_Tariff = ZString.Empty;
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", false, testCusLineTariffDetail.IsPreventionAdditionalTax);
			testCusLineTariffDetail.BZ_Tariff = "470-000";
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", false, testCusLineTariffDetail.IsPreventionAdditionalTax);
			testCusLineTariffDetail.BZ_Tariff = "470-002";
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", true, testCusLineTariffDetail.IsPreventionAdditionalTax);
			testCusLineTariffDetail.BZ_Tariff = "470-202";
			AssertEquals($"BZ_Tariff={testCusLineTariffDetail.BZ_Tariff}", true, testCusLineTariffDetail.IsPreventionAdditionalTax);
			testCusLineTariffDetail.BZ_Type = ZString.Empty;
			AssertEquals("BZ_Type = empty", false, testCusLineTariffDetail.IsPreventionAdditionalTax);
		});
	}

	public void TestBZ_Qty1_DecimalPlaces()
	{
		CombineAssertions(() =>
		{
			CusLineTariffDetail.BZ_Type = RateTypes.AdditionalTaxes;
			AssertEquals("BZ_Type=ADT", 3, CusLineTariffDetail.BZ_Qty1_DecimalPlaces);

			CusLineTariffDetail.BZ_Type = RateTypes.AdditionalFees;
			AssertEquals("BZ_Type=FEE", 1, CusLineTariffDetail.BZ_Qty1_DecimalPlaces);
		});
	}

	public void TestBZ_Qty1_Readonly_ADT()
	{
		var invoiceLine = CreateImportInvoiceLine();
		var additionalTax = invoiceLine.AdditionalTaxes.AddNew();

		CombineAssertions(() =>
		{
			additionalTax.BZ_UQ1 = ZString.Empty;
			AssertEquals("UOM is empty", true, additionalTax.BZ_Qty1Info.ReadOnly);

			additionalTax.BZ_UQ1 = "LPA";
			AssertEquals("UOM is not empty", false, additionalTax.BZ_Qty1Info.ReadOnly);
		});
	}

	public void TestBZ_Qty1_ShouldBe1_WhenTaxTypeIsCitesFlora_ADT()
	{
		var invoiceLine = CreateImportInvoiceLine();
		var additionalTax = invoiceLine.AdditionalTaxes.AddNew();

		AssertEquals("Precondition", ZDecimal.Zero, additionalTax.BZ_Qty1);

		additionalTax.BZ_TaxType = AdditionalTaxesTypes.CitesFlora;
		AssertEquals((ZDecimal)1, additionalTax.BZ_Qty1);
	}

	public void TestBZ_ManualRate_Readonly_ADT() => CombineAssertions(() =>
	{
		var invoiceLine = CreateImportInvoiceLine();

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var tariffWithoutFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "290-001");
		var optionalTariffWithoutFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "292-000");
		var tariffWithFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "290-002", rateFormula: "0.0021 * [KGM]");
		var tariffsWithoutFormulaButForcedManualRate = new[] {
			additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "450-001"),
			additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "450-009"),
			additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "450-201"),
			additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "450-209"),
		};
		var optionalTariffWithoutFormulaInForcedManualRateRange = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "450-000");

		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		var additionalTax = invoiceLine.AdditionalTaxes[0];

		AssertEquals("No tariff", true, additionalTax.BZ_ManualRateInfo.ReadOnly);

		additionalTax.BZ_Tariff = tariffWithoutFormula.ZZ1_TariffCode;
		AssertEquals("Tariff without formula", false, additionalTax.BZ_ManualRateInfo.ReadOnly);

		additionalTax.BZ_Tariff = tariffWithFormula.ZZ1_TariffCode;
		AssertEquals("Tariff with formula", true, additionalTax.BZ_ManualRateInfo.ReadOnly);

		additionalTax.BZ_Tariff = optionalTariffWithoutFormula.ZZ1_TariffCode;
		AssertEquals("Optional Tariff without formula", true, additionalTax.BZ_ManualRateInfo.ReadOnly);

		additionalTax.BZ_Tariff = optionalTariffWithoutFormulaInForcedManualRateRange.ZZ1_TariffCode;
		AssertEquals("Optional Tariff without formula in forced manual rate range", true, additionalTax.BZ_ManualRateInfo.ReadOnly);

		foreach (var tariff in tariffsWithoutFormulaButForcedManualRate)
		{
			additionalTax.BZ_Tariff = tariff.ZZ1_TariffCode;
			AssertEquals($"Optional Tariff without formula but forced manual rate BZ_Tariff={additionalTax.BZ_Tariff}", false, additionalTax.BZ_ManualRateInfo.ReadOnly);
		}
	});

	public void TestBZ_ManualRate_ShouldBeZero_WhenReadonly_ADT()
	{
		const decimal manualRate = 123m;

		var invoiceLine = CreateImportInvoiceLine();

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var tariffWithoutFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "290-001");
		var tariffWithFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "290-002", rateFormula: "0.0021 * [KGM]");

		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		var additionalTax = invoiceLine.AdditionalTaxes[0];

		CombineAssertions(() =>
		{
			AssertEquals("No tariff", true, additionalTax.BZ_ManualRateInfo.ReadOnly);

			additionalTax.BZ_ManualRate = manualRate;
			additionalTax.BZ_Tariff = tariffWithoutFormula.ZZ1_TariffCode;
			AssertEquals("Precondition", false, additionalTax.BZ_ManualRateInfo.ReadOnly);
			AssertEquals("Not read-only", manualRate, additionalTax.BZ_ManualRate);

			additionalTax.BZ_ManualRate = manualRate;
			additionalTax.BZ_Tariff = tariffWithFormula.ZZ1_TariffCode;
			AssertEquals("Precondition", true, additionalTax.BZ_ManualRateInfo.ReadOnly);
			AssertEquals("Read-only", 0m, additionalTax.BZ_ManualRate);
		});
	}

	public void TestBZ_AlcoholPercentage_Readonly_ADT()
	{
		var invoiceLine = CreateImportInvoiceLine();

		var additionalTax = invoiceLine.AdditionalTaxes.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Empty tax type", true, additionalTax.BZ_AlcoholPercentageInfo.ReadOnly);

			additionalTax.BZ_TaxType = AdditionalTaxesTypes.CitesFauna;
			AssertEquals("Not 280 tax type", true, additionalTax.BZ_AlcoholPercentageInfo.ReadOnly);

			additionalTax.BZ_TaxType = AdditionalTaxesTypes.Spirits;
			AssertEquals("280 tax type", false, additionalTax.BZ_AlcoholPercentageInfo.ReadOnly);
		});
	}

	public void TestBZ_UQ1_DefaultValues_WhenTaxTypeIsTobacco_ADT()
	{
		var testCases = new[]
		{
				(tariffCode: ImportTariffCodeGroups.Cigarets, expectedUOM: SwissCustomsConstants.MeasurementUnits.UnitOfOneThousandUOM),
				(tariffCode: ImportTariffCodeGroups.SmokingTobacco, expectedUOM: SwissCustomsConstants.MeasurementUnits.GrossWeightUOM),
				(tariffCode: ImportTariffCodeGroups.ProductsContainingTobacco, expectedUOM: SwissCustomsConstants.MeasurementUnits.GrossWeightUOM)
			};

		foreach (var (tariffCode, expectedUOM) in testCases)
		{
			TestOneCase(tariffCode, expectedUOM);
		}

		void TestOneCase(string tariffCode, string expectedUOM)
		{
			const string tobaccoTaxType = AdditionalTaxesTypes.Tobacco;
			const string notTobaccoTaxType = AdditionalTaxesTypes.Spirits;

			var invoiceLine = CreateImportInvoiceLine();
			invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;

			var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
			var testDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff(tariffCode);
			var tobaccoTariffWithoutUOM =
				additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: $"{tobaccoTaxType}-{tariffCode}1", additionalCode: tariffCode);
			var tobaccoTariffWithUOM =
				additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: $"{tobaccoTaxType}-{tariffCode}2", additionalCode: tariffCode);
			const string tariffUOM = "LPA";
			testDataHelper.CreateTariffUOM(tobaccoTariffWithUOM, UOMTypeList.Codes.CU1, tariffUOM);
			var notTobaccoTariffWithoutUOM =
				additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: $"{notTobaccoTaxType}-{tariffCode}", additionalCode: tariffCode);

			invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
			var tobaccoAdditionalTax = invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().Single(at => at.BZ_TaxType == tobaccoTaxType);
			var notTobaccoAdditionalTax = invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().Single(at => at.BZ_TaxType == notTobaccoTaxType);

			CombineAssertions(() =>
			{
				tobaccoAdditionalTax.BZ_Tariff = tobaccoTariffWithoutUOM.ZZ1_TariffCode;
				AssertEquals($"Main tariff code - {tariffCode}, Tobacco tariff without UOM", expectedUOM, tobaccoAdditionalTax.BZ_UQ1);

				tobaccoAdditionalTax.BZ_Tariff = tobaccoTariffWithUOM.ZZ1_TariffCode;
				AssertEquals($"Main tariff code - {tariffCode}, Tobacco tariff with UOM", tariffUOM, tobaccoAdditionalTax.BZ_UQ1);

				notTobaccoAdditionalTax.BZ_Tariff = notTobaccoTariffWithoutUOM.ZZ1_TariffCode;
				AssertNullOrEmpty($"Main tariff code - {tariffCode}, Not Tobacco tariff without UOM", notTobaccoAdditionalTax.BZ_UQ1);
			});
		}
	}

	public void TestDefaultTariffAndCalculateAdditionalTaxValue_ADT()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 50m);

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_LinePrice = 100m;

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var testDataHelper = new UniversalReferenceTestDataHelper(Factory);

		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var tariff1 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "290-001", rateFormula: "0.0021 * [KGM]");
		testDataHelper.CreateTariffUOM(tariff1, UOMTypeList.Codes.CU1, "KGM");

		var tariff2 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode: "290-002", rateFormula: "0.0022 * [LPA]");
		testDataHelper.CreateTariffUOM(tariff2, UOMTypeList.Codes.CU1, "LPA");

		var tariff3 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, tariffCode: "290-003", rateFormula: "0.04 * (VFD + DTY)");

		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		var additionalTax = invoiceLine.AdditionalTaxes[0];
		additionalTax.BZ_Qty1 = 100m;
		Assert("ShouldCalculateAdditionalTaxValueAfterMerge", !additionalTax.ShouldCalculateAdditionalTaxValueAfterMerge);

		additionalTax.BZ_Tariff = tariff1.ZZ1_TariffCode;
		Assert("ShouldCalculateAdditionalTaxValueAfterMerge", !additionalTax.ShouldCalculateAdditionalTaxValueAfterMerge);
		AssertEquals("KGM", additionalTax.BZ_UQ1);
		AssertEquals(0.21m, additionalTax.BZ_Value);

		additionalTax.BZ_Qty1 = 200m;
		AssertEquals(0.42m, additionalTax.BZ_Value);

		additionalTax.BZ_Tariff = tariff2.ZZ1_TariffCode;
		Assert("ShouldCalculateAdditionalTaxValueAfterMerge", !additionalTax.ShouldCalculateAdditionalTaxValueAfterMerge);
		AssertEquals("LPA", additionalTax.BZ_UQ1);
		AssertEquals(0.44m, additionalTax.BZ_Value);

		additionalTax.BZ_Qty1 = 100m;
		AssertEquals(0.22m, additionalTax.BZ_Value);

		additionalTax.BZ_Tariff = tariff3.ZZ1_TariffCode;
		Assert("ShouldCalculateAdditionalTaxValueAfterMerge", additionalTax.ShouldCalculateAdditionalTaxValueAfterMerge);
		AssertEquals("0.04 * (100 + 50)", 6m, additionalTax.BZ_Value);
	}

	public void TestBZ_Value_ShouldBeCalculatedWithManualRate_WhenRateFormulaIsEmpty_ADT()
	{
		var invoiceLine = CreateImportInvoiceLine();
		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_CustomsThirdQuantity = 100;

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var tariffWithoutFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("290-001");
		var tariffWithFormula = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("290-002", rateFormula: "100");
		var tariffForcedManualRateAndUOM = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("450-001", tariffUom: ("CU1", "KGM"));
		var tariffForcedManualRateNoUOM = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("450-201");

		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		var additionalTax = invoiceLine.AdditionalTaxes[0];
		additionalTax.BZ_ManualRate = 10;
		additionalTax.BZ_Qty1 = 2;

		CombineAssertions(() =>
		{
			additionalTax.BZ_Tariff = tariffWithoutFormula.ZZ1_TariffCode;
			AssertEquals("Tariff without formula", 20m, additionalTax.BZ_Value);

			additionalTax.BZ_ManualRate = 20;
			AssertEquals("Tariff without formula, Manual Rate changed", 40m, additionalTax.BZ_Value);

			additionalTax.BZ_Qty1 = 3;
			AssertEquals("Tariff without formula, Quantity changed", 60m, additionalTax.BZ_Value);

			additionalTax.BZ_Tariff = tariffWithFormula.ZZ1_TariffCode;
			AssertEquals("Tariff with formula", 100m, additionalTax.BZ_Value);

			additionalTax.BZ_Tariff = tariffForcedManualRateNoUOM.ZZ1_TariffCode;
			additionalTax.BZ_ManualRate = 20;
			AssertEquals("Tariff with forced manual rate and UOM", 60m, additionalTax.BZ_Value);

			additionalTax.BZ_Tariff = tariffForcedManualRateAndUOM.ZZ1_TariffCode;
			AssertEquals("Tariff with forced manual rate no UOM", 60m, additionalTax.BZ_Value);
		});
	}

	public void TestBZ_Qty1_ShouldBeCalculatedWithJI_CustomsThirdQuantity_WhenBZ_TaxTypeIs280_ADT()
	{
		var invoiceLine = CreateImportInvoiceLine();
		invoiceLine.JI_CustomsThirdQuantity = 100;

		var spiritsAdditionalTax = invoiceLine.AdditionalTaxes.AddNew();
		spiritsAdditionalTax.BZ_TaxType = AdditionalTaxesTypes.Spirits;
		spiritsAdditionalTax.BZ_AlcoholPercentage = 50;
		var notSpiritsAdditionalTax = invoiceLine.AdditionalTaxes.AddNew();
		notSpiritsAdditionalTax.BZ_TaxType = AdditionalTaxesTypes.CitesFauna;
		notSpiritsAdditionalTax.BZ_AlcoholPercentage = 50;

		CombineAssertions(() =>
		{
			AssertEquals("Spirits tariff", 50m, spiritsAdditionalTax.BZ_Qty1);
			AssertEquals("Not spirits tariff", 0m, notSpiritsAdditionalTax.BZ_Qty1);

			spiritsAdditionalTax.BZ_AlcoholPercentage = 60;
			AssertEquals("AlcoholPercentage changes", 60m, spiritsAdditionalTax.BZ_Qty1);

			invoiceLine.JI_CustomsThirdQuantity = 200;
			AssertEquals("CustomsThirdQuantity changes", 120m, spiritsAdditionalTax.BZ_Qty1);
		});
	}

	public void TestAssessmentCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff1 = additionalTaxTariffHelper.CreateParentImportTariff("11111111000111");
		var parentTariff2 = additionalTaxTariffHelper.CreateParentImportTariff("22222222000222");
		var tariff1 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{UniversalReferenceConstants.AdditionalTaxesTypes.Spirits}-001", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff1.ZZ1_TariffCode));
		var tariff2 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{UniversalReferenceConstants.AdditionalTaxesTypes.VeterinaryInspection}-001", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff2.ZZ1_TariffCode));

		additionalTaxTariffHelper.CreateAttributeForTariff(tariff1, UniversalReferenceConstants.TariffAttributes.AssessmentCode, "1");
		additionalTaxTariffHelper.CreateAttributeForTariff(tariff2, UniversalReferenceConstants.TariffAttributes.AssessmentCode, "2");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
			AssertEquals("Empty", 0, invoiceLine.AdditionalTaxes.Count);

			invoiceLine.JI_Tariff = parentTariff1.ZZ1_TariffCode;
			AssertEquals($"Count for {parentTariff1.ZZ1_TariffCode}", 1, invoiceLine.AdditionalTaxes.Count);
			AssertEquals($"{UniversalReferenceConstants.TariffAttributes.AssessmentCode} for {parentTariff1.ZZ1_TariffCode}", "1", invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().FirstOrDefault()?.AssessmentCode);

			invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultExcludedTradeGroupCountry;
			AssertEquals($"Count for ExcludedTradeGroupCountry", 0, invoiceLine.AdditionalTaxes.Count);

			invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
			invoiceLine.JI_Tariff = parentTariff2.ZZ1_TariffCode;
			AssertEquals($"Count for {parentTariff2.ZZ1_TariffCode}", 1, invoiceLine.AdditionalTaxes.Count);
			AssertEquals($"{UniversalReferenceConstants.TariffAttributes.AssessmentCode} for {parentTariff2.ZZ1_TariffCode}", "2", invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().FirstOrDefault()?.AssessmentCode);
		});
	}

	public void TestBZ_Qty1Default()
	{
		const string additionalCode = "11111111000111";
		const string includedType = AdditionalTaxesTypes.VeterinaryInspection;
		const string excludedType = AdditionalTaxesTypes.Beer;

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff(additionalCode);
		var excludedTariff001 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{excludedType}-001", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff.ZZ1_TariffCode));
		additionalTaxTariffHelper.CreateAttributeForTariff(excludedTariff001, UniversalReferenceConstants.TariffAttributes.AssessmentCode, "2");
		for (int i = 11; i <= 24; i++)
		{
			var includedTariff = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode($"{includedType}-{i:D3}", additionalTaxTariffHelper.GetTariffCodeWithoutCustomsFavourCode(parentTariff.ZZ1_TariffCode));
			additionalTaxTariffHelper.CreateAttributeForTariff(includedTariff, UniversalReferenceConstants.TariffAttributes.AssessmentCode, i.ToString());
		}

		var invoiceLine = CreateImportInvoiceLine();
		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		invoiceLine.JI_CustomsQuantity = 100;
		invoiceLine.JI_CustomsSecondQuantity = 200;
		invoiceLine.JI_CustomsThirdQuantity = 300;

		string setAdditionalTax(CusLineTariffDetail additionalTax, string tariff)
		{
			additionalTax.BZ_Qty1 = ZDecimal.Zero;
			additionalTax.BZ_Tariff = tariff;
			return tariff;
		}

		var includedTax = invoiceLine.AdditionalTaxes.Where(s => s.BZ_TaxType == includedType).FirstOrDefault();
		var excludedTax = invoiceLine.AdditionalTaxes.Where(s => s.BZ_TaxType == excludedType).FirstOrDefault();

		CombineAssertions(() =>
		{
			includedTax.BZ_Qty1 = 5;
			includedTax.BZ_TaxType = includedType;
			includedTax.BZ_Tariff = $"{includedType}-024";
			AssertEquals("BZ_Qty1 not empty", 5.0M, includedTax.BZ_Qty1);

			AssertEquals(setAdditionalTax(includedTax, $"{includedType}-011"), invoiceLine.JI_CustomsQuantity, includedTax.BZ_Qty1);
			for (int i = 12; i <= 23; i++)
			{
				AssertEquals(setAdditionalTax(includedTax, $"{includedType}-{i:D3}"), invoiceLine.JI_CustomsThirdQuantity, includedTax.BZ_Qty1);
			}
			AssertEquals(setAdditionalTax(includedTax, $"{includedType}-024"), invoiceLine.JI_CustomsSecondQuantity, includedTax.BZ_Qty1);
			AssertEquals(setAdditionalTax(excludedTax, $"{excludedType}-002"), invoiceLine.JI_CustomsThirdQuantity / 100, excludedTax.BZ_Qty1);
		});
	}

	public void TestTaxTariffKey() => CombineAssertions(() =>
	{
		CusLineTariffDetail.BZ_Tariff = "450-123";
		AssertEquals("BZ_Type=empty", ZString.Empty, CusLineTariffDetail.TaxTariffKey);

		CusLineTariffDetail.BZ_Type = RateTypes.AdditionalTaxes;
		AssertEquals("BZ_Type=ADT", "123", CusLineTariffDetail.TaxTariffKey);

		CusLineTariffDetail.BZ_Type = RateTypes.AdditionalFees;
		AssertEquals("BZ_Type=FEE", ZString.Empty, CusLineTariffDetail.TaxTariffKey);
	});

	public void TestBZ_Value_ShouldBeCalculatedWithManualRate_1000_Based() => CombineAssertions(() =>
	{
		var invoiceLine = CreateImportInvoiceLine();
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);

		var tariff1 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("450-002");
		var additionalTax1 = invoiceLine.AdditionalTaxes.AddNew();
		additionalTax1.BZ_ManualRate = 250;
		additionalTax1.BZ_Qty1 = 100;
		additionalTax1.BZ_Tariff = tariff1.ZZ1_TariffCode;

		AssertEquals("Tariff 450-002", 25m, additionalTax1.BZ_Value);

		var tariff2 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("450-202");
		var additionalTax2 = invoiceLine.AdditionalTaxes.AddNew();
		additionalTax2.BZ_ManualRate = 500;
		additionalTax2.BZ_Qty1 = 1000;
		additionalTax2.BZ_Tariff = tariff2.ZZ1_TariffCode;

		AssertEquals("Tariff 450-202", 500m, additionalTax2.BZ_Value);
	});

	public void TestIsForcedManualRate() => CombineAssertions(() =>
	{
		CusLineTariffDetail.BZ_Tariff = "450-001";
		AssertEquals("450-001", true, CusLineTariffDetail.IsForcedManualRate);
		CusLineTariffDetail.BZ_Tariff = "450-101";
		AssertEquals("450-101", false, CusLineTariffDetail.IsForcedManualRate);
	});

	protected override BusinessObject GetNewBusinessObject()
	{
		return CreateImportInvoiceLine().AdditionalTaxes.AddNew();
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject();
	}

	JobComInvoiceLine CreateImportInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine;
	}

	CusLineTariffDetail CusLineTariffDetail => cusLineTariffDetail ??= Factory.New<CusLineTariffDetail>();
	CusLineTariffDetail cusLineTariffDetail;
}
