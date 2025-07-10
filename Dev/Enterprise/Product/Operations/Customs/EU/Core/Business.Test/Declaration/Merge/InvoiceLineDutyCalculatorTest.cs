using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.InvoiceLineDutyCalculator;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(InvoiceLineDutyCalculator))]
	sealed class InvoiceLineDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<BaseJobComInvoiceLine, EUUniversalRateCalcDataForInvoiceLine>
	{
		protected override UniversalDutyCalculator<BaseJobComInvoiceLine, EUUniversalRateCalcDataForInvoiceLine> CreateDutyCalculator()
		{
			return new InvoiceLineDutyCalculator(Factory.New<JobComInvoiceLine>(), RateCalculationVisitorMode.Default);
		}
	}

	sealed class EUUniversalRateCalcDataForInvoiceLineTest : TestCaseWithFactory
	{
		public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>("Exception expected whith null arguments", () => new EUUniversalRateCalcDataForInvoiceLine(null, null));

		public void TestCountrySpecificValueList()
		{
			rateCalcData = new EUUniversalRateCalcDataForInvoiceLine(invoiceLine, Factory.New<RateView>());
			AssertEquals("CountrySpecificValueList.Count", 1, rateCalcData.CountrySpecificValueList.Count);
			AssertEquals("CountrySpecificValueList[NIHIL]", 0m, rateCalcData.CountrySpecificValueList["NIHIL"]);
		}

		public void TestMeursingExpressionList_WithNoMeursingExpressionsInRateFormula()
		{
			var meursingExpressionList = rateCalcData.MeursingExpressionList;
			AssertArrayEqualsByElements(nameof(rateCalcData.MeursingExpressionList), new Dictionary<string, string>().ToArray(), meursingExpressionList.ToArray());
			AssertSame("Cached MeursingExpressionList", meursingExpressionList, rateCalcData.MeursingExpressionList);
		}

		public void TestAdditionalInformationList()
		{
			var doc11 = invoiceLine.SupportingDocuments.AddNew();
			doc11.CSI_Type = "SUP";
			doc11.CSI_Code = "C11";
			var doc12 = invoiceLine.SupportingDocuments.AddNew();
			doc12.CSI_Type = "XXX";
			doc12.CSI_Code = "C12";

			rateCalcData = new EUUniversalRateCalcDataForInvoiceLine(invoiceLine, Factory.New<RateView>());
			AssertEquals("AdditionalInformationList.Count", 1, rateCalcData.AdditionalInformationList.Count);
			AssertEquals("AdditionalInformationList[0]", new Tuple<string, string>("CERT", "C11"), rateCalcData.AdditionalInformationList[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine = invoice.InvoiceLines.AddNew();

			rateCalcData = new EUUniversalRateCalcDataForInvoiceLine(invoiceLine, Factory.New<RateView>());
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		EUUniversalRateCalcDataForInvoiceLine rateCalcData;
	}

	#region RateCalcMeursingExpressionReplacerForInvoiceLineTest

	sealed class RateCalcMeursingExpressionReplacerForInvoiceLineTest : TestCaseWithFactory
	{
		public void TestGetMeursingValueList_WhenCustomsFormulaIsEmpty()
		{
			var rateCalcMeursing = new RateCalcMeursingExpressionReplacerForInvoiceLine(invoiceLine);

			CombineAssertions("When RateView Rate Formula is empty or null", () =>
			{
				AssertEquals("Result Count", 0, rateCalcMeursing.ReplaceApplicableMeursingExpressions(null).Count);
				AssertEquals("Result Count", 0, rateCalcMeursing.ReplaceApplicableMeursingExpressions(Factory.New<RateView>()).Count);
			});
		}

		public void TestGetMeursingValueList()
		{
			var rateView = SetupReferenceData();
			SetUpInvoiceLine("STD", "1111111", "7023", 10m, "KG", 0.02m, "HLT");

			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacerForInvoiceLine(invoiceLine);
			var meursingExpressionList = rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(rateView);

			AssertEquals("Result MeursingExpressionList count", 4, meursingExpressionList.Count);
			AssertArrayEqualsByElements("", new[] { "EA(1)", "EAR(1)", "ADFM(1)", "ADFM(2)" }, meursingExpressionList.Keys.ToArray());

			CombineAssertions(() =>
			{
				AssertEquals("EA(1) Expression", "2*[LTR]", meursingExpressionList["EA(1)"]);
				AssertEquals("EAR(1) Expression", "0", meursingExpressionList["EAR(1)"]);
				AssertEquals("ADFM(1) Expression", "0.4*[KGM]", meursingExpressionList["ADFM(1)"]);
				AssertEquals("ADFM(12) Expression", "0", meursingExpressionList["ADFM(2)"]);
			});
		}

		public void TestGetMeursingValueList_WhenSupplementaryCodeIsNotMeursing()
		{
			var rateView = SetupReferenceData();
			SetUpInvoiceLine("STD", "1111111", "2023", 10m, "KG", 0.02m, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Hectolitre);

			ReplaceApplicableMeursingExpressionAndAssertMeursingExpressionListIsEmpty(rateView);

			invoiceLine.JI_SupplementaryCode1 = "7024";
			ReplaceApplicableMeursingExpressionAndAssertMeursingExpressionListIsEmpty(rateView);
		}

		public void TestGetMeursingValueList_WhenRateViewIsNotDuty()
		{
			var rateView = SetupReferenceData();
			SetUpInvoiceLine("STD", "1111111", "2023", 10m, "KG", 0.02m, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Hectolitre);

			rateView.CusRateCode.ZY1_RateType = "EXC";
			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacerForInvoiceLine(invoiceLine);
			var meursingExpressionList = rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(rateView);
			AssertEquals("Result MeursingExpressionList count", 0, meursingExpressionList.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		void SetUpInvoiceLine(string primaryPreference, string tariff, string supplementaryCode, decimal qty, string unit, decimal secondQty, string seconUnit)
		{
			invoiceLine.JI_PrimaryPreference = primaryPreference;
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_SupplementaryCode1 = supplementaryCode;
			invoiceLine.JI_CustomsQuantity = qty;
			invoiceLine.JI_CustomsUnitQty = unit;
			invoiceLine.JI_CustomsSecondQuantity = secondQty;
			invoiceLine.JI_CustomsSecondUnitQty = seconUnit;
		}

		void ReplaceApplicableMeursingExpressionAndAssertMeursingExpressionListIsEmpty(RateView rateView)
		{
			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacerForInvoiceLine(invoiceLine);
			var meursingExpressionList = rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(rateView);

			AssertEquals("Result MeursingExpressionList count", 4, meursingExpressionList.Count);
			AssertArrayEqualsByElements("", new[] { "EA(1)", "EAR(1)", "ADFM(1)", "ADFM(2)" }, meursingExpressionList.Keys.ToArray());

			CombineAssertions(() =>
			{
				AssertEquals("EA(1) Expression", "0", meursingExpressionList["EA(1)"]);
				AssertEquals("EAR(1) Expression", "0", meursingExpressionList["EAR(1)"]);
				AssertEquals("ADFM(1) Expression", "0", meursingExpressionList["ADFM(1)"]);
				AssertEquals("ADFM(12) Expression", "0", meursingExpressionList["ADFM(2)"]);
			});
		}

		RateView SetupReferenceData()
		{
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);

			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var stdTradeGroup = refDataHelper.CreateTradeGroup(grouping, "STANDARD", startDate, endDate);
			var impTariffType = refDataHelper.CreateNewOrGetExistingTariffType(grouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var stdPreference = refDataHelper.CreatePreferenceView("STD", "Standard", grouping);
			Factory.Save();

			var dtyTariff = refDataHelper.CreateTariff(grouping, impTariffType.PK, "1111111", startDate, endDate, taxOrFeeCode: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(grouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
			var meuTariffType = refDataHelper.CreateNewOrGetExistingTariffType(grouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.MeursingTariff);
			Factory.Save();

			var meuTariff = refDataHelper.CreateTariff(grouping, meuTariffType.PK, "7023", startDate, endDate);

			var rateCodeDtyA00 = refDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var rateCodeDtyA10 = refDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, dtyRateType.PK);
			var rateCodeEa = refDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent, dtyRateType.PK);
			var rateCodeAdfm = refDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnFlourContents, dtyRateType.PK);
			var tariffDtyRateA00 = refDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: RateFormula, preferencePk: stdPreference.PK);
			Factory.Save();

			_ = refDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffDtyRateA10 = refDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA10.PK, startDate, endDate, rateFormula: "VFD*0.1 + #ADFM(1)#", preferencePk: stdPreference.PK);
			tariffDtyRateA10.Factory.Save();
			_ = refDataHelper.CreateCusApplicability(tariffDtyRateA10.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffMeuRateEa = refDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeEa.PK, startDate, endDate, rateFormula: "2*[LTR]");
			tariffMeuRateEa.Factory.Save();
			_ = refDataHelper.CreateCusApplicability(tariffMeuRateEa.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");
			var tariffMeuRateAdfm = refDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeAdfm.PK, startDate, endDate, rateFormula: "0.4*[KGM]");
			tariffMeuRateAdfm.Factory.Save();
			_ = refDataHelper.CreateCusApplicability(tariffMeuRateAdfm.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");
			Factory.Save();

			return Factory.Load<RateView>(tariffDtyRateA00.PK);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		const string RateFormula = "MIN(VFD*0.2 + #EA(1)#, VFD*0.8 + #EAR(1)#) + MIN(VFD*0.9 + #EA(1)#, VFD*0.3 + #ADFM(1)#) + #ADFM(2)#";
	}

	#endregion
}
