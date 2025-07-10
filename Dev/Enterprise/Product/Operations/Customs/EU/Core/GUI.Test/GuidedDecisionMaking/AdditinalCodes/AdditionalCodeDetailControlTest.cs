using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using UniversalReferenceConstants = Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class AdditionalCodeDetailControlTest : TestCaseWithFactory
	{
		public void TestAddtionalCodeDetailControl()
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
			var additionalCode = new GuidedDecisionMakingAdditionalCode(guidedDecisionMakingBasic);
			additionalCode.AdditionalCode = "ADD11";

			using (var form = new ZForm())
			using (var detailControl = new AdditionalCodeDetailControl(additionalCode))
			{
				form.Controls.Add(detailControl);
				detailControl.BindingSource.SetDataBinding(additionalCode, "");
				form.Show();
				additionalCode.IsTicked = false;

				var radioButton = detailControl.FindSingle<ZRadioButton>();
				AssertEquals("RadioButton in AdditionalCodeDetailControl should be Text-Empty", ZString.Empty, radioButton.Text);
				AssertEquals("RadioButton should be unchecked by default", false, radioButton.Checked);

				UserIdleWorker.Flush();
				var codeFindBox = detailControl.FindSingle<ZCodeFindBox>();
				AssertEquals("CodeFindBox.CodeBox should be filled with Add. Code", "ADD11", codeFindBox.CodeBox.Text);
				AssertEquals("CodeFindBox.DescriptionBox should be filled with Add. Description", "ADD11 Descriptions", codeFindBox.DescriptionBox.Text);
				AssertEquals("CodeFindBox Should be readonly", true, codeFindBox.ReadOnly);

				radioButton.Checked = true;
				Assert("Additional Code can be ticked by the radio button", additionalCode.IsTicked);
				radioButton.Checked = false;
				Assert("Additional Code can be ticked by the radio button", !additionalCode.IsTicked);
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
