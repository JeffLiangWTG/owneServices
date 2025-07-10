using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class RateCalcMeursingExpressionReplacerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new RateCalcMeursingExpressionReplacer(null));
				AssertExceptionThrown<ArgumentNullException>("Exception expected when Declaration is null", () => new RateCalcMeursingExpressionReplacer(Factory.New<CusEntryLine>()));
			});
		}

		public void TestGetMeursingValueList_WhenCustomsFormulaIsEmpty()
		{
			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacer(entryLine);

			CombineAssertions("When RateView Rate Formula is empty or null", () =>
			{
				AssertEquals("Result Count", 0, rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(null).Count);
				AssertEquals("Result Count", 0, rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(Factory.New<RateView>()).Count);
			});
		}

		public void TestGetMeursingValueList()
		{
			var rateView = SetupReferenceData();

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = "STD";
			invLine1.JI_Tariff = "1111111";
			invLine1.JI_SupplementaryCode1 = "7023";
			invLine1.JI_CustomsQuantity = 10;
			invLine1.JI_CustomsUnitQty = "KG";
			invLine1.JI_CustomsSecondQuantity = 0.02;
			invLine1.JI_CustomsSecondUnitQty = "HLT";

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 20;

			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacer(entryLine);
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

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = "STD";
			invLine1.JI_Tariff = "1111111";
			invLine1.JI_SupplementaryCode1 = "2023";
			invLine1.JI_CustomsQuantity = 10;
			invLine1.JI_CustomsUnitQty = "KG";
			invLine1.JI_CustomsSecondQuantity = 0.02;
			invLine1.JI_CustomsSecondUnitQty = "HLT";

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 20;

			ReplaceApplicableMeursingExpressionAndAssertMeursingExpressionListIsEmpty(rateView);

			invLine1.JI_SupplementaryCode1 = "7024";
			ReplaceApplicableMeursingExpressionAndAssertMeursingExpressionListIsEmpty(rateView);
		}

		public void TestGetMeursingValueList_WhenRateViewIsNotDuty()
		{
			var rateView = SetupReferenceData();

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = "STD";
			invLine1.JI_Tariff = "1111111";
			invLine1.JI_SupplementaryCode1 = "2023";
			invLine1.JI_CustomsQuantity = 10;
			invLine1.JI_CustomsUnitQty = "KG";
			invLine1.JI_CustomsSecondQuantity = 0.02;
			invLine1.JI_CustomsSecondUnitQty = "HLT";

			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 20;

			rateView.CusRateCode.ZY1_RateType = "EXC";
			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacer(entryLine);
			var meursingExpressionList = rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(rateView);
			AssertEquals("Result MeursingExpressionList count", 0, meursingExpressionList.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();
		}

		JobDeclaration declaration;
		CusEntryLine entryLine;

		void ReplaceApplicableMeursingExpressionAndAssertMeursingExpressionListIsEmpty(RateView rateView)
		{
			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacer(entryLine);
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

			var dtyTariff = refDataHelper.CreateTariff(grouping, impTariffType.PK, "1111111", startDate, endDate, taxOrFeeCode: "DTY");
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

			refDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffDtyRateA10 = refDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA10.PK, startDate, endDate, rateFormula: "VFD*0.1 + #ADFM(1)#", preferencePk: stdPreference.PK);
			tariffDtyRateA10.Factory.Save();
			refDataHelper.CreateCusApplicability(tariffDtyRateA10.PK, stdTradeGroup, startDate, endDate, additionalCode: "7023");
			var tariffMeuRateEa = refDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeEa.PK, startDate, endDate, rateFormula: "2*[LTR]");
			tariffMeuRateEa.Factory.Save();
			refDataHelper.CreateCusApplicability(tariffMeuRateEa.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");
			var tariffMeuRateAdfm = refDataHelper.CreateRefCusRate(meuTariff.PK, rateCodeAdfm.PK, startDate, endDate, rateFormula: "0.4*[KGM]");
			tariffMeuRateAdfm.Factory.Save();
			refDataHelper.CreateCusApplicability(tariffMeuRateAdfm.PK, stdTradeGroup, startDate, endDate, additionalCode: "1");
			Factory.Save();

			return Factory.Load<RateView>(tariffDtyRateA00.PK);
		}

		const string RateFormula = "MIN(VFD*0.2 + #EA(1)#, VFD*0.8 + #EAR(1)#) + MIN(VFD*0.9 + #EA(1)#, VFD*0.3 + #ADFM(1)#) + #ADFM(2)#";
	}
}
