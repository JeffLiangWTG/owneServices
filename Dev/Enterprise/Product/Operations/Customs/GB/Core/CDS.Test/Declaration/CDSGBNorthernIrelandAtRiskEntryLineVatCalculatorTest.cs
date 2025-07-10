using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSGBNorthernIrelandAtRiskEntryLineVatCalculatorTest : TestCaseWithFactory
	{
		public void TestVatCalculationIfRiskMovingToROI()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.20m, currentCountryCode, startDate, endDate);

			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGroupingCode: "EUN", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "", parent: eunDataGrouping);
			Factory.Save();

			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_CustomsValue = 1000m;
				entryLine.CL_ValueForVAT = 1000m;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;

				entryLine.InvoiceLines.Add(invoiceLine);

				using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
				{
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, "%", 10m, 1000m, 100m, "", false);
					VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 5m, 1000m, 50m, "", false);
				}

				var vatCalculator = new CDSGBNorthernIrelandAtRiskEntryLineVatCalculator(entryLine);
				var calculateVatFees = vatCalculator.CalculateVatFees();

				CombineAssertions("Calculated VAT fees", () =>
				{
					AssertEquals(2, calculateVatFees.Count());
					AssertEquals("Code", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, calculateVatFees.First().Code);
					AssertEquals("Rate", ordVat.ZZF_Value, calculateVatFees.First().Rate);
					AssertEquals("BaseValue", 1050m, calculateVatFees.First().BaseValue);
					AssertEquals("Amount", 210m, calculateVatFees.First().Amount);
					AssertEquals("MethodOfCalculation", "%", calculateVatFees.First().MethodOfCalculation);
					AssertEquals("Code", EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, calculateVatFees.Last().Code);
					AssertEquals("Rate", ordVat.ZZF_Value, calculateVatFees.Last().Rate);
					AssertEquals("BaseValue", 100m, calculateVatFees.Last().BaseValue);
					AssertEquals("Amount", 20m, calculateVatFees.Last().Amount);
					AssertEquals("MethodOfCalculation", "%", calculateVatFees.Last().MethodOfCalculation);
				});

				entryLine.CL_ValueForVAT = 1010m;
				calculateVatFees = vatCalculator.CalculateVatFees();
				CombineAssertions("Calculated VAT fees", () =>
				{
					AssertEquals("BaseValue", 1060m, calculateVatFees.First().BaseValue);
					AssertEquals("Amount", 212m, calculateVatFees.First().Amount);
				});
			}
		}

		public void TestVatCalculationWhenRefreshing()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.20m, currentCountryCode, startDate, endDate);

			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGroupingCode: "CDS", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var gbDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			refDataHelper.CreateNewOrGetExistingDataGrouping("CDS", "", parent: gbDataGrouping);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_NorthernIrelandMode = NIModeList.Codes.ImportIntoNiFromRestOfWorld;
			declaration.JE_NiGoodsAtRiskOfMovingToROI = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			invoiceLine.JI_LinePrice = 100;
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIIMP;

			Factory.Save();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			var entryLine = entry.AllEntryLines[0];

			var feeB00 = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat);
			AssertNotNull(feeB00);
			AssertEquals(100m, feeB00.CF_BaseValue);

			var feeB05 = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland);
			AssertNotNull(feeB05);
			AssertEquals(0m, feeB05.CF_BaseValue);

			var feeA50 = entryLine.Fees.AddNew();
			// This forces VatRefresher to run
			feeA50.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty;
			feeA50.CF_ChargeAmount = 20m;
			feeA50.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			AssertEquals(100m, feeB00.CF_BaseValue);
			AssertEquals(20m, feeB05.CF_BaseValue);

			declaration.JE_NiGoodsAtRiskOfMovingToROI = false;
			feeA50.CF_ChargeAmount = 50m;
			AssertEquals("The NIP profile has changed, so a new NI EntryLineVatCalculator should be in use.  If this fails, it suggests that the mainland GB calculator not the NI calculator is in use.  It's possible that someone changed the EU VatFeeRefresher so cache the calculator.  This is not right, we need to be able to use a fresh calculator each time",
							150m, feeB00.CF_BaseValue);
		}

		public void TestVatCalculationWhenRefreshingForNorthernIreland()
		{
			var ordVat = SetupAndSaveReferenceData();
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGroupingCode: "CDS", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var gbDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			refDataHelper.CreateNewOrGetExistingDataGrouping("CDS", "", parent: gbDataGrouping);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
			declaration.JE_NiGoodsAtRiskOfMovingToROI = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			invoiceLine.JI_LinePrice = 1000;
			invoiceLine.JI_PrimaryPreference = "100";
			invoiceLine.JI_CountryOfOrigin = "US";
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = GBCommonConstants.AdditonalInfoCodes.NIDOM;

			Factory.Save();

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			var entryLine = entry.AllEntryLines[0];

			var feeB00 = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat);
			AssertNotNull(feeB00);
			AssertEquals(0m, feeB00.CF_ChargeAmount);

			var feeB05 = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland);
			AssertNotNull(feeB05);
			AssertEquals(0m, feeB05.CF_ChargeAmount);

			entryLine.CL_CustomsValue = 200;

			var feeA50 = entryLine.Fees.AddNew();
			// This forces VatRefresher to run
			feeA50.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty;
			feeA50.CF_ChargeAmount = 20m;
			feeA50.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			AssertEquals(0m, feeB00.CF_ChargeAmount);
			AssertEquals(0m, feeB05.CF_ChargeAmount);

			declaration.JE_NiGoodsAtRiskOfMovingToROI = false;
			feeA50.CF_ChargeAmount = 50m;
			AssertNotEquals(40m, feeB00.CF_ChargeAmount);
			AssertNotEquals(4m, feeB05.CF_ChargeAmount);
		}

		Universal.RefCusTaxOrFee SetupAndSaveReferenceData()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.20m, currentCountryCode, startDate, endDate);

			var dtyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "", parent: eunDataGrouping);
			Factory.Save();

			return ordVat;
		}
	}
}
