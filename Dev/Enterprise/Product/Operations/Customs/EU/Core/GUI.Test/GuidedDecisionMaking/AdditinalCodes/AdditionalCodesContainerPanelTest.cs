using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using UniversalReferenceConstants = Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class AdditionalCodesContainerPanelTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestPopulateAdditionalCodePanels()
		{
			var guidedDecisionMakingBasic = GetGuidedDecisionMakingBasic();

			using (var control = new AdditionalCodesContainerPanel())
			{
				control.PopulateAdditionalCodeControls(guidedDecisionMakingBasic);
				var additionalCodesControls = control.FindAll<AdditionalCodesControl>().OrderBy(c => c.Top).ToArray();

				CombineAssertions(() =>
				{
					AssertEquals("Count", 2, additionalCodesControls.Length);

					AssertEquals("Caption: ADD", "Rate Type: ADD ADD desc", additionalCodesControls[0].AdditionalCodesFlowLayoutPanelGroupBox.Text);
					AssertEquals("Caption: DTY", "Rate Type: DTY Duty desc", additionalCodesControls[1].AdditionalCodesFlowLayoutPanelGroupBox.Text);
				});
			}
		}

		public void TestPopulateAdditionalCodePanels_InSummary()
		{
			var guidedDecisionMakingBasic = GetGuidedDecisionMakingBasic();
			guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
			guidedDecisionMakingBasic.AdditionalCodes[1].IsTicked = false;

			using (var control = new AdditionalCodesContainerPanel())
			{
				control.InSummary = true;
				control.PopulateAdditionalCodeControls(guidedDecisionMakingBasic);
				var additionalCodesControls = control.FindAll<AdditionalCodesControl>().OrderBy(c => c.Top).ToArray();

				CombineAssertions(() =>
				{
					AssertEquals("Count", 1, additionalCodesControls.Length);
					AssertEquals("Should only show ticked", "Additional Codes", additionalCodesControls[0].AdditionalCodesFlowLayoutPanelGroupBox.Text);
				});
			}
		}

		GuidedDecisionMakingBasic GetGuidedDecisionMakingBasic()
		{
			SetupTariffAndRate();

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
			return new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine), Factory);
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

			var dutyRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty desc");
			var rateCode1 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var rateCode3 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC2", dutyRateType.PK);
			var addRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.AntiDumping, "ADD desc");
			var rateCode2 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			var preferenceSTD = RefDataHelper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
			var preferenceRED = RefDataHelper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
			RefDataHelper.CreatePreferenceForCountry("MFN", "Most-favored Nation Duty", dataGrouping);
			Factory.Save();

			var testRate1 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add11", "ord11");
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add12", "ord12");

			var testRate2 = RefDataHelper.CreateRate(cusTariff, rateCode3.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add21", "ord21");
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add22", "ord22");

			var testRate3 = RefDataHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, "add31", "ord31");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
