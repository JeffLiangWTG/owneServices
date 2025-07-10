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
	sealed class DocumentConditionsContainerPanelTest : TestCaseWithFactory
	{
		public void TestPopulateDocumentConditionsContainerPanel()
		{
			var guidedDecisionMakingBasic = GetGuidedDecisionMakingBasic();

			using (var control = new DocumentConditionsContainerPanel())
			{
				control.PopulateDocumentConditionControls(guidedDecisionMakingBasic);
				var documentConditionsControl = control.FindAll<DocumentConditionsControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("Count", 2, documentConditionsControl.Length);
				AssertEquals("Should have the group title as 'Document Conditions'.", "", documentConditionsControl[0].Controls.Find("DocumentConditionDetailsGroupBox", false).First().Text);
			}
		}

		public void TestPopulateDocumentConditionsContainerPanel_InSummary()
		{
			var guidedDecisionMakingBasic = GetGuidedDecisionMakingBasic();
			var documentCondition = guidedDecisionMakingBasic.DocumentConditions.Cast<GuidedDecisionMakingCondition>().ToList();
			documentCondition[0].ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().ToList()[0].IsTicked = false;
			documentCondition[1].ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().ToList()[0].IsTicked = true;
			AssertDocumentConditionsContainerPanel_InSummary(guidedDecisionMakingBasic, 1);

			documentCondition[0].ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().ToList()[0].IsTicked = true;
			documentCondition[1].ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().ToList()[0].IsTicked = true;
			AssertDocumentConditionsContainerPanel_InSummary(guidedDecisionMakingBasic, 2);
		}

		void AssertDocumentConditionsContainerPanel_InSummary(GuidedDecisionMakingBasic guidedDecisionMakingBasic, int tickedNumber)
		{
			using (var control = new DocumentConditionsContainerPanel())
			{
				control.InSummary = true;
				control.PopulateDocumentConditionControls(guidedDecisionMakingBasic);
				var documentConditionsControls = control.FindAll<DocumentConditionsControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("Should only contain 1 group box for all conditons of different type", 1, documentConditionsControls.Length);
				AssertEquals("Should have the group title as 'Document Conditions'.", "Document Conditions", documentConditionsControls[0].Controls.Find("DocumentConditionDetailsGroupBox", false).First().Text);

				var documentConditionsControl = documentConditionsControls.First();

				AssertEquals("Should only contain checked conditons", tickedNumber, documentConditionsControl.FindAll<DocumentConditionDetailControl>().OrderBy(c => c.Top).ToArray().Length);
			}
		}

		GuidedDecisionMakingBasic GetGuidedDecisionMakingBasic()
		{
			SetupConditons();

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

		void SetupConditons()
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

			var testRate1 = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preferencePk: preference.PK);
			helper.CreateCusApplicability(testRate1, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add11", "");
			Factory.Save();
		}
	}
}
