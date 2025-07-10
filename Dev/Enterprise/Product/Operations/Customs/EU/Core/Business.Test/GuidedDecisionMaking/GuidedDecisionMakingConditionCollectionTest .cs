using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingConditionCollection))]
	sealed class GuidedDecisionMakingConditionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuidedDecisionMakingConditionCollection>
	{
		public void TestLogicalGroupLoaded()
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var rateType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "RAT1", "Test Rate Condition Type");
			var preference = helper.CreatePreferenceForCountry("P1", "TestPreference", dataGrouping);
			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1122334455667", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);

			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "SupportingDocumentNoReferenceNumber");
			var condition1 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, rateType.PK, tariff.PK, "C1", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "R111", 1);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "C111", 1);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "P111", 2);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "T111", 2);

			var condition2 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, rateType.PK, tariff.PK, "C2", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition2.PK, "R111", 0);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition2.PK, "U111", 0);

			var testRate1 = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preferencePk: preference.PK);
			helper.CreateCusApplicability(testRate1, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add1", "");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";

			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			var guidedDecisionMakingConditionCollection = new GuidedDecisionMakingConditionCollection(gDMBasic);
			guidedDecisionMakingConditionCollection.Load();
			var guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();

			var conditionDetail = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C1").ConditionDetails.Where(x => x.Code == "R111").FirstOrDefault();
			AssertEquals("LogicalGroup should match condition value ZX3_LogicalORWithinGroup", 1, conditionDetail.LogicalGroup);
			var conditionDetail2 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C1").ConditionDetails.Where(x => x.Code == "C111").FirstOrDefault();
			AssertEquals("LogicalGroup should match condition value ZX3_LogicalORWithinGroup", 1, conditionDetail2.LogicalGroup);
			var conditionDetail3 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C1").ConditionDetails.Where(x => x.Code == "P111").FirstOrDefault();
			AssertEquals("LogicalGroup should match condition value ZX3_LogicalORWithinGroup", 2, conditionDetail3.LogicalGroup);
			var conditionDetail4 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C1").ConditionDetails.Where(x => x.Code == "T111").FirstOrDefault();
			AssertEquals("LogicalGroup should match condition value ZX3_LogicalORWithinGroup", 2, conditionDetail4.LogicalGroup);

			var conditionDetail5 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C2").ConditionDetails.Where(x => x.Code == "R111").FirstOrDefault();
			AssertEquals("LogicalGroup should match condition value ZX3_LogicalORWithinGroup", 0, conditionDetail5.LogicalGroup);
			var conditionDetail6 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C2").ConditionDetails.Where(x => x.Code == "U111").FirstOrDefault();
			AssertEquals("LogicalGroup should match condition value ZX3_LogicalORWithinGroup", 0, conditionDetail6.LogicalGroup);
		}

		public void TestConditionsWithSameCodeShouldBeHandledAsOneObject()
		{
			GuidedDecisionMakingTestHelper.SetupConditons(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;

			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			var guidedDecisionMakingConditionCollection = new GuidedDecisionMakingConditionCollection(gDMBasic);
			guidedDecisionMakingConditionCollection.Load();
			var guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();

			var details1 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C1").ConditionDetails;
			AssertContainsExactElementsInExactOrder("Rate conditon: only SUP/SNR condition values are selected out", new[] { "C111", "R111", "R12" }, details1.Select(x => x.Code.ToString()).ToArray());

			var details2 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C2").ConditionDetails;
			AssertContainsExactElementsInExactOrder("Rate conditon: only SUP/SNR condition values are selected out", new[] { "R12", "R222" }, details2.Select(x => x.Code.ToString()).ToArray());

			var details1_R12 = details1.Where(x => x.Code == "R12").FirstOrDefault();
			var details2_R12 = details2.Where(x => x.Code == "R12").FirstOrDefault();

			AssertSame("Condition value with the same code should be handled as one object.", details1_R12, details2_R12);
			details1_R12.IsTicked = true;
			AssertEquals("R12 in details2 should be ticked as well.", true, details2_R12.IsTicked);
			details2_R12.IsTicked = false;
			AssertEquals("R12 in details1 should be unticked as well.", false, details1_R12.IsTicked);
		}

		public void TestLoadVatCondtion()
		{
			GuidedDecisionMakingTestHelper.SetupConditons(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew("C777", "REF 1");
			supportingDocument.CSI_DateOfIssue = new ZDateTime(2023, 06, 22);

			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			var vat1 = gDMBasic.VATApplicabilities.AddNew();
			vat1.AdditionalCode = "add2";
			vat1.IsTicked = true;

			CombineAssertions(() =>
			{
				var guidedDecisionMakingConditionCollection = new GuidedDecisionMakingConditionCollection(gDMBasic);
				guidedDecisionMakingConditionCollection.Load();
				var guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();

				gDMBasic.AdditionalCodes[0].IsTicked = true;
				guidedDecisionMakingConditionCollection.Load();
				guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();
				AssertConditions(guidedDecisionMakingConditions, "A VAT condition can be selected", 5,
					new ZString[] { "CTR1", "CTR1", "CTR1", "RAT1", "VAT1" },
					new ZString[] { "Test Ctrl Condition Type", "Test Ctrl Condition Type", "Test Ctrl Condition Type", "Test Rate Condition Type", "Test Vat Condition Type" },
					new ZString[] { "C2", "C4", "C5", "C1", "C7" },
					new ZBool[] { ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.True },
					new ZBool[] { ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False },
				new ZString[] { ZString.Empty, ZString.Empty, "INF1 or INF2", ZString.Empty, ZString.Empty });
			});
		}

		public void TestLoad()
		{
			GuidedDecisionMakingTestHelper.SetupConditons(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew("R111", "REF 1");
			supportingDocument.CSI_DateOfIssue = new ZDateTime(2023, 06, 22);
			CombineAssertions(() =>
			{
				var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
				var guidedDecisionMakingConditionCollection = new GuidedDecisionMakingConditionCollection(gDMBasic);
				guidedDecisionMakingConditionCollection.Load();
				var guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();

				AssertConditions(guidedDecisionMakingConditions, "Additional code is not ticked", 3,
					new ZString[] { "CTR1", "CTR1", "RAT1" },
					new ZString[] { "Test Ctrl Condition Type", "Test Ctrl Condition Type", "Test Rate Condition Type" },
					new ZString[] { "C2", "C5", "C1" },
					new ZBool[] { ZBool.False, ZBool.True, ZBool.True },
					new ZBool[] { ZBool.False, ZBool.True, ZBool.False },
					new ZString[] { ZString.Empty, "INF1 or INF2", ZString.Empty });

				gDMBasic.AdditionalCodes[0].IsTicked = true;
				guidedDecisionMakingConditionCollection.Load();
				guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();
				AssertConditions(guidedDecisionMakingConditions, "Import: only CTR/RAT conditions are selected out", 4,
					new ZString[] { "CTR1", "CTR1", "CTR1", "RAT1" },
					new ZString[] { "Test Ctrl Condition Type", "Test Ctrl Condition Type", "Test Ctrl Condition Type", "Test Rate Condition Type" },
					new ZString[] { "C2", "C4", "C5", "C1" },
					new ZBool[] { ZBool.False, ZBool.True, ZBool.True, ZBool.True },
					new ZBool[] { ZBool.False, ZBool.False, ZBool.True, ZBool.False },
					new ZString[] { ZString.Empty, ZString.Empty, "INF1 or INF2", ZString.Empty });

				var details1 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C2").ConditionDetails;
				AssertEquals("Control condition: Count", 2, details1.Count);
				AssertConditionDetail(details1[0], "CTR1_R12", "R12", Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "", ZDateTime.Empty, false, false);
				AssertConditionDetail(details1[1], "CTR1_R222", "R222", Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "", ZDateTime.Empty, false, false);

				var details2 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C1").ConditionDetails;
				AssertEquals("Rate condition: only SUP/SNR condition values are selected out", 3, details2.Count);
				AssertConditionDetail(details2[0], "RAT1_C111", "C111", Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "", ZDateTime.Empty, false, false);
				AssertConditionDetail(details2[1], "RAT1_R111", "R111", Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "REF 1", new ZDateTime(2023, 06, 22), true, true);
				AssertConditionDetail(details2[2], "RAT1_R12", "R12", Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "", ZDateTime.Empty, false, false);

				var details3 = guidedDecisionMakingConditions.FirstOrDefault(x => x.ConditionSatisfactionType == "C4").ConditionDetails;
				AssertEquals("Control condition: Count", 1, details3.Count);
				AssertConditionDetail(details3[0], "CTR1", "C444", Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "", ZDateTime.Empty, true, true);

				gDMBasic.CustomsFirstQuantity = 1m;
				guidedDecisionMakingConditionCollection.Load();
				guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();
				var ctrlConditionC2 = guidedDecisionMakingConditions.Where(x => x.ConditionSatisfactionType == "C2");
				AssertEquals("Control condition C2: when the Formula matched then ignored on GDM", 0, ctrlConditionC2.Count());
				var rateCondition = guidedDecisionMakingConditions.Where(x => x.ConditionSatisfactionType == "C1");
				AssertEquals("Rate condition: no change", 1, rateCondition.Count());
				var ctrlConditionC4 = guidedDecisionMakingConditions.Where(x => x.ConditionSatisfactionType == "C4");
				AssertEquals("Control condition C4: no change", 1, ctrlConditionC4.Count());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
				guidedDecisionMakingConditionCollection = new GuidedDecisionMakingConditionCollection(gDMBasic);
				guidedDecisionMakingConditionCollection.Load();
				AssertEquals("Export: empty guidedDecisionMakingConditionCollection", 0, guidedDecisionMakingConditionCollection.Count);
			});
		}

		public void TestIfOnlyOneConditionInGroupThenTickByDefault()
		{
			GuidedDecisionMakingTestHelper.SetupConditons(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";

			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			var guidedDecisionMakingConditionCollection = new GuidedDecisionMakingConditionCollection(gDMBasic);
			guidedDecisionMakingConditionCollection.Load();
			var guidedDecisionMakingConditions = guidedDecisionMakingConditionCollection.Cast<GuidedDecisionMakingCondition>();

			foreach (var condition in guidedDecisionMakingConditions)
			{
				var conditionDetails = condition.ConditionDetails;

				if (conditionDetails.Count == 1)
				{
					var conditionDetail = conditionDetails.First() as GuidedDecisionMakingConditionDetail;
					AssertEquals("Condition should be ticked by default when there is only one condition in the group.", true, conditionDetail.IsTicked);
				}
				else
				{
					foreach (GuidedDecisionMakingConditionDetail conditionDetail in conditionDetails)
					{
						AssertEquals("Condition should NOT be ticked by default when there are multiple conditions in the group.", false, conditionDetail.IsTicked);
					}
				}
			}
		}

		void AssertConditions(IEnumerable<GuidedDecisionMakingCondition> guidedDecisionMakingConditions, ZString message, int count, ZString[] types, ZString[] descriptions, ZString[] satisfactionTypes, ZBool[] isSatisfiedArray, ZBool[] isInformationCondition, ZString[] informationValues)
		{
			AssertEquals(message + ": Count", count, guidedDecisionMakingConditions.Count());
			AssertContainsExactElementsInExactOrder(message + ": ConditionSatisfactionType", satisfactionTypes, guidedDecisionMakingConditions.Select(x => x.ConditionSatisfactionType));
			AssertContainsExactElementsInExactOrder(message + ": ConditionType", types, guidedDecisionMakingConditions.Select(x => x.ConditionType));
			AssertContainsExactElementsInExactOrder(message + ": ConditionTypeDescription", descriptions, guidedDecisionMakingConditions.Select(x => x.ConditionTypeDescription));
			AssertContainsExactElementsInExactOrder(message + ": IsSatisfied", isSatisfiedArray, guidedDecisionMakingConditions.Select(x => x.IsSatisfied));
			AssertContainsExactElementsInExactOrder(message + ": IsInformationCondition", isInformationCondition, guidedDecisionMakingConditions.Select(x => x.IsInformationCondition));
			AssertContainsExactElementsInExactOrder(message + ": InformationValue", informationValues, guidedDecisionMakingConditions.Select(x => x.InformationValue));
		}

		void AssertConditionDetail(GuidedDecisionMakingConditionDetail conditionDetail, ZString message, ZString code, ZString type, ZString reference, ZDateTime dateOfIssue, bool isSatisfied, bool isTicked)
		{
			AssertEquals(message + ": Code", code, conditionDetail.Code);
			AssertEquals(message + ": Type", type, conditionDetail.Type);
			AssertEquals(message + ": Reference", reference, conditionDetail.Reference);
			AssertEquals(message + ": DateOfIssue", dateOfIssue, conditionDetail.DateOfIssue);
			AssertEquals(message + ": IsSatisfied", isSatisfied, conditionDetail.IsSatisfied);
			AssertEquals(message + ": IsTicked", isTicked, conditionDetail.IsTicked);
		}

		protected override GuidedDecisionMakingConditionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			return new GuidedDecisionMakingConditionCollection(gDMBasic);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new GuidedDecisionMakingCondition();
	}
}
