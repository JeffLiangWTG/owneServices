using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Constants = Enterprise.Core.Constants;
using UniversalReferenceConstants = Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class SummaryControlTest : TestCaseWithFactory
	{
		public void TestDynamicPanelVisibility_ShouldBeUpdatedWithTickedStatus()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			var code = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			var vat = guidedDecisionMakingBasic.VATApplicabilities.AddNew();

			using (var control = new SummaryControl())
			{
				var status = new List<bool> { true, false };
				var allTickedStatuses = status.SelectMany(x => status.Select(y => new[] { x, y })).SelectMany(z => status.Select(k => z.Append(k).ToArray()));
				var allStatuses = status.SelectMany(x => status.Select(y => new[] { x, y })).SelectMany(z => new List<bool> { false, false }.Select(k => z.Append(k).ToArray())).ToArray();

				var i = 0;
				foreach (var tickedStatus in allTickedStatuses)
				{
					vat.IsTicked = tickedStatus[2];
					code.IsTicked = tickedStatus[0];
					guidedDecisionMakingBasic.DocumentConditions.RemoveAndDeleteAll();
					guidedDecisionMakingBasic.DocumentConditions.AddNew().ConditionDetails.AddNew().IsTicked = tickedStatus[1];
					
					control.PopulateSummaryControls(guidedDecisionMakingBasic);

					var additionalCodesPanel = control.FindSingle<AdditionalCodesContainerPanel>("additionalCodesContainerPanel");
					var documentConditionsPanel = control.FindSingle<DocumentConditionsContainerPanel>("documentConditionsContainerPanel");
					var vatApplicabiliesPanel = control.FindSingle<VATContainerPanel>("vatContainerPanel");

					var vatTicked = guidedDecisionMakingBasic.VATApplicabilities.Cast<GuidedDecisionMakingVAT>().Any(v => v.IsTicked);
					var additionalCodesTicked = guidedDecisionMakingBasic.AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>().Any(x => x.IsTicked);
					var documentTicked = guidedDecisionMakingBasic.DocumentConditions.Cast<GuidedDecisionMakingCondition>().SelectMany(x => x.ConditionDetails).Cast<GuidedDecisionMakingConditionDetail>().Any(y => y.IsTicked);

					AssertArrayEqualsByElements("Prerequisites: The ticking status of the additional codes, documents and vats.", tickedStatus, new[] { additionalCodesTicked, documentTicked, vatTicked });
					AssertArrayEqualsByElements("Visibility of Dynamic panel should be set according to if any items are ticked except for VAT if is not import.", allStatuses[i], new[] { additionalCodesPanel.Visible, documentConditionsPanel.Visible, vatApplicabiliesPanel.Visible });
					i++;
				}
			}

			guidedDecisionMakingBasic.IsImport = true;
			using (var control = new SummaryControl())
			{
				var status = new List<bool> { true, false };
				var allTickedStatuses = status.SelectMany(x => status.Select(y => new[] { x, y })).SelectMany(z => status.Select(k => z.Append(k).ToArray()));
				foreach (var tickedStatus in allTickedStatuses)
				{
					vat.IsTicked = tickedStatus[2];
					code.IsTicked = tickedStatus[0];
					guidedDecisionMakingBasic.DocumentConditions.RemoveAndDeleteAll();
					guidedDecisionMakingBasic.DocumentConditions.AddNew().ConditionDetails.AddNew().IsTicked = tickedStatus[1];
					control.PopulateSummaryControls(guidedDecisionMakingBasic);

					var additionalCodesPanel = control.FindSingle<AdditionalCodesContainerPanel>("additionalCodesContainerPanel");
					var documentConditionsPanel = control.FindSingle<DocumentConditionsContainerPanel>("documentConditionsContainerPanel");
					var vatApplicabiliesPanel = control.FindSingle<VATContainerPanel>("vatContainerPanel");

					var vatTicked = guidedDecisionMakingBasic.VATApplicabilities.Cast<GuidedDecisionMakingVAT>().Any(v => v.IsTicked);
					var additionalCodesTicked = guidedDecisionMakingBasic.AdditionalCodes.Cast<GuidedDecisionMakingAdditionalCode>().Any(x => x.IsTicked);
					var documentTicked = guidedDecisionMakingBasic.DocumentConditions.Cast<GuidedDecisionMakingCondition>().SelectMany(x => x.ConditionDetails).Cast<GuidedDecisionMakingConditionDetail>().Any(y => y.IsTicked);

					AssertArrayEqualsByElements("Prerequisites: The ticking status of the additional codes, documents and vats.", tickedStatus, new[] { additionalCodesTicked, documentTicked, vatTicked });
					AssertArrayEqualsByElements("Visibility of Dynamic panel should be set according to if any items are ticked.", tickedStatus, new[] { additionalCodesPanel.Visible, documentConditionsPanel.Visible, vatApplicabiliesPanel.Visible });
				}
			}
		}

		public void TestPopulateVATControl()
		{
			var gdmBasic = VATContainerPanelTest.SetUpGDMBasicWithVATForTest(Factory);
			gdmBasic.VATApplicabilities[2].IsTicked = true;

			using (var control = new SummaryControl())
			{
				control.PopulateSummaryControls(gdmBasic);
				var vatControl = control.FindAll<VATDetailControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("Only ticked VAT should be displayed in the summary control.", 1, vatControl.Length);
			}
		}

		public void TestPopulateDocumentConditionsControl()
		{
			var guidedDecisionMakingBasic = GetGuidedDecisionMakingBasic();
			var documentCondition = guidedDecisionMakingBasic.DocumentConditions.Cast<GuidedDecisionMakingCondition>().ToList();
			documentCondition[0].ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().ToList()[0].IsTicked = false;
			documentCondition[1].ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().ToList()[0].IsTicked = true;

			guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
			guidedDecisionMakingBasic.AdditionalCodes[1].IsTicked = false;

			using (var control = new SummaryControl())
			{
				control.PopulateSummaryControls(guidedDecisionMakingBasic);
				var documentConditionsControl = control.FindAll<DocumentConditionsControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("DocumentConditionsControl should only show ticked", 1, documentConditionsControl.Length);
				var additionalCodesControl = control.FindAll<AdditionalCodesControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("AdditionalCodesControl should only show ticked", 1, additionalCodesControl.Length);
			}
		}

		public void TestMeursingResultUserControlPopulated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";
			invoiceLine.SupportingDocuments.AddNew("C111", "REF 1");
			var sourceWrapper = new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine);
			Factory.SuspendValidation();
			var guidedDecisionMakingBasic = new Mock<GuidedDecisionMakingBasic>(sourceWrapper, Factory)
			{
				CallBase = true
			};
			using (var control = new SummaryControl())
			{
				guidedDecisionMakingBasic.Setup(x => x.IsMeursingApplicable).Returns(false);
				control.PopulateSummaryControls(guidedDecisionMakingBasic.Object);
				var meursingResultControl = control.Controls.Find("MeursingResultDropEdit", true);
				AssertEquals("MeursingResultUserControl is not shown by default", 0, meursingResultControl.Length);

				guidedDecisionMakingBasic.Setup(x => x.IsMeursingApplicable).Returns(true);
				control.PopulateSummaryControls(guidedDecisionMakingBasic.Object);
				meursingResultControl = control.Controls.Find("MeursingResultDropEdit", true);
				AssertEquals("MeursingResultUserControl is not shown by default", 1, meursingResultControl.Length);
			}
		}

		GuidedDecisionMakingBasic GetGuidedDecisionMakingBasic()
		{
			SetupTariffAndRateAndConditons();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";
			invoiceLine.SupportingDocuments.AddNew("C111", "REF 1");
			return new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine), Factory);
		}

		void SetupTariffAndRateAndConditons()
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var rateType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "RAT1", "Test Rate Condition Type");
			var ctrlType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR1", "Test Ctrl Condition Type");
			var classType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, "CAS2", "Test Class Condition Type");
			var preference = helper.CreatePreferenceForCountry("P1", "TestPreference", dataGrouping);
			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1122334455667", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);

			var condition1ValueType1 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
			var condition1ValueType2 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "SupportingDocumentNoReferenceNumber");
			var condition1ValueType3 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.PresentationOfSupportingDoc, "PresentationOfSupportingDoc");
			var condition1 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, rateType.PK, tariff.PK, "C1", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition1.PK, "C111");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition1.PK, "R111");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType3.PK, condition1.PK, "P111");
			var condition2 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C2", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition2.PK, "C222");
			var condition3 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, classType.PK, tariff.PK, "C3", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition3.PK, "R222");
			Factory.Save();

			var rateCode3 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC2", dutyRateType.PK);
			var addRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.AntiDumping, "ADD");
			var rateCode2 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			var preferenceRED = RefDataHelper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
			RefDataHelper.CreatePreferenceForCountry("MFN", "Most-favored Nation Duty", dataGrouping);
			Factory.Save();

			var testRate1 = RefDataHelper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preferencePk: preference.PK);
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add11", "ord11");
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add12", "ord12");

			var testRate2 = RefDataHelper.CreateRate(tariff, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add21", "ord21");
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add22", "ord22");

			var testRate3 = RefDataHelper.CreateRate(tariff, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate3, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add31", "ord31");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
