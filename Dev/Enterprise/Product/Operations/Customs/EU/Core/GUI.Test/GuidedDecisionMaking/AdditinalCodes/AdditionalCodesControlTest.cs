using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Core.Constants;
using UniversalReferenceConstants = Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class AdditionalCodesControlTest : TestCaseWithFactory
	{
		public void TestPopulateAdditionalCodesControl()
		{
			SetupTariffAndRate();
			SetupCusCodeList();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_ConcessionOrder = "";
			var guidedDecisionMakingBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var additionalCodes = guidedDecisionMakingBasic.AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>().Where(x => x.ApplicableToType == "DTY").ToList();
			using (var form = new ZForm(guidedDecisionMakingBasic))
			using (var control = new AdditionalCodesControl("Rate Type - DTY with description", additionalCodes, false))
			{
				form.Controls.Add(control);
				form.Show();
				UserIdleWorker.Flush();
				foreach (var additionalCode in additionalCodes)
				{
					additionalCode.IsTicked = false;
				}
				AssertEquals("DTY", "Rate Type - DTY with description", control.FindAll<ZGroupBox>().FirstOrDefault().Text);

				var additionalCodeDetailControls = control.FindAll<AdditionalCodeDetailControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals(2, additionalCodeDetailControls.Length);
				var radioButton1 = additionalCodeDetailControls[0].FindAll<ZRadioButton>().FirstOrDefault();
				var radioButton2 = additionalCodeDetailControls[1].FindAll<ZRadioButton>().FirstOrDefault();
				var codeFindBox1 = additionalCodeDetailControls[0].FindAll<ZCodeFindBox>().FirstOrDefault();
				var codeFindBox2 = additionalCodeDetailControls[1].FindAll<ZCodeFindBox>().FirstOrDefault();
				CombineAssertions("CodeFindBox should contail the information of the Add. Code", () =>
				{
					AssertEquals("CodeBox of CodeFindBox should contain the Add. Code", "ADD11", codeFindBox1.CodeBox.Text);
					AssertEquals("CodeBox of CodeFindBox should contain the Add. Code", "ADD12", codeFindBox2.CodeBox.Text);
					AssertEquals("DescriptionBox of CodeFindBox should contain the Add. Description", "ADD11 Descriptions", codeFindBox1.DescriptionBox.Text);
					AssertEquals("DescriptionBox of CodeFindBox should contain the Add. Description", "ADD12 Descriptions", codeFindBox2.DescriptionBox.Text);
				});
				CombineAssertions("AdditionalCodeDetailControl in one single AdditionalCodesControl should be selected 'EXCLUSIVELY'", () =>
				{
					Assert("RadioButton should be unchecked by default", !radioButton1.Checked);
					Assert("RadioButton should be unchecked by default", !radioButton2.Checked);

					radioButton1.Checked = true;
					Assert("radioButton1 should be checked", radioButton1.Checked);
					Assert("radioButton2 should be unchecked", !radioButton2.Checked);

					radioButton2.Checked = true;
					Assert("radioButton1 should be unchecked", !radioButton1.Checked);
					Assert("radioButton2 should be checked", radioButton2.Checked);
				});
			}
		}

		public void TestPopulateAdditionalCodes_NoDulicateCode_InSummary()
		{
			SetupTariffAndRate();
			SetupCusCodeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "";
			invoiceLine.JI_ConcessionOrder = "";
			var guidedDecisionMakingBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var additionalCodes = guidedDecisionMakingBasic.AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>().ToList();
			AssertEquals("Pre-requisite: Additional Code Count.", 6, additionalCodes.Count);

			using (var form = new ZForm(guidedDecisionMakingBasic))
			using (var control = new AdditionalCodesControl("Additional Codes in Summary", additionalCodes, true))
			{
				form.Controls.Add(control);
				form.Show();
				UserIdleWorker.Flush();
				var additionalCodeDetailControls = control.FindAll<AdditionalCodeDetailControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("AdditionalCodeDetailControl Count, duplicate ADD12 should be removed.", 6, additionalCodeDetailControls.Length);
			}
		}

		void SetupTariffAndRate()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, date1, date4);
			var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var cusTariff = RefDataHelper.CreateTariff(dataGrouping, tariffType.PK, "1122334455667", date1, date4, "dummy Description 0");

			var dutyRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var addRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.AntiDumping, "ADD");
			var rateCode2 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			var preferenceSTD = RefDataHelper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
			var preferenceRED = RefDataHelper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
			Factory.Save();

			var testRate1 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "ADD11", "ord11");
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "ADD12", "ord12");

			var testRate2 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "ADD12", "ord11");
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "ADD21", "ord21");
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "ADD22", "ord22");

			var testRate3 = RefDataHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, "ADD31", "ord31");

			var ctrlType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR", "Test Ctrl Condition Type");
			var conditionValueType = RefDataHelper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
			var condition1 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, cusTariff.PK, "C1", true, false, date1, date4);
			RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "C111");
			RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "R111");
			RefDataHelper.CreateCusApplicability(condition1, tradeGroupStandard, date1, date4, "AddCon");
			Factory.Save();
		}

		void SetupCusCodeList()
		{
			const string additionalCodes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes;
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var datetime1 = ZDateTime.Today.AddDays(-1);
			var datetime2 = ZDateTime.Today.AddDays(1);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", currentCountry);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "ADD11", "ADD11 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "ADD12", "ADD12 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "ADD21", "ADD21 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "ADD22", "ADD22 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "ADD31", "ADD31 Descriptions", datetime1, datetime2);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
