using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class DocumentConditionsControlTest : TestCaseWithFactory
	{
		public void TestDocumentConditionDetailsShouldBeDistinctByCode_InSummary()
		{
			var guidedDecisionMakingBasic = GetGuidedDecisionMakingBasicForTest();
			var condition = new GuidedDecisionMakingCondition(guidedDecisionMakingBasic);
			var conditionDetails = new GuidedDecisionMakingConditionDetailCollection(condition);
			var conditionDetail1 = conditionDetails.AddNew();
			conditionDetail1.Code = "COD1";
			var conditionDetail2 = conditionDetails.AddNew();
			conditionDetail2.Code = "COD1";
			var conditionDetail3 = conditionDetails.AddNew();
			conditionDetail3.Code = "COD2";
			using (var form = new ZForm())
			using (var control = new DocumentConditionsControl(condition, conditionDetails.Cast<GuidedDecisionMakingConditionDetail>(), true))
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					var documentConditionDetailControls = control.FindAll<DocumentConditionDetailControl>().OrderBy(c => c.Top).ToArray();
					AssertContainsExactElementsInExactOrder("Only 2 DocumentConditionDetailControls with different codes should exist.", new[] { "COD1", "COD2" }, documentConditionDetailControls.Select(c => c.FindAll<ZCodeFindBox>().FirstOrDefault().CurrentCode));
				});
			}
		}

		public void TestPopulateDocumentConditionsControl()
		{
			GuidedDecisionMakingTestHelper.SetupConditons(Factory);
			var guidedDecisionMakingBasic = GetGuidedDecisionMakingBasicForTest();
			var documentCondition = guidedDecisionMakingBasic.DocumentConditions.Cast<GuidedDecisionMakingCondition>().FirstOrDefault(x => x.ConditionType == "RAT1");
			using (var form = new ZForm(guidedDecisionMakingBasic))
			using (var control = new DocumentConditionsControl(documentCondition))
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var controlHeaderControl = control.FindSingle<DocumentConditionHeaderControl>("DocumentConditionHeaderControl");
					AssertNotNull("DocumentConditionHeaderControl should be populated.", controlHeaderControl);

					var documentConditionDetailControls = control.FindAll<DocumentConditionDetailControl>().OrderBy(c => c.Top).ToArray();
					AssertEquals("There should be 3 DocumentConditionDetailControls be created to the number of the conditionDetails.", 3, documentConditionDetailControls.Length);

					var checkBoxes = control.FindAll<ZCheckBox>().OrderBy(c => c.Top).ToArray();
					AssertArrayEqualsByElements("Text of all the checkboxes in the documentConditionControl should be empty.", new[] { ZString.Empty, ZString.Empty, ZString.Empty }, checkBoxes.Select(c => new ZString(c.Text)).ToArray());

					AssertArrayEqualsByElements("Only R111 should be checked by default.", new[] { false, true, false }, checkBoxes.Select(c => c.Checked).ToArray());

					var textBoxes = control.FindAll<ZTextBox>(maxLevelsDeep: 3).OrderBy(c => c.Top).ToArray();
					AssertEquals("The quantity of the Reference TextBox should be equivalent to the number of the conditionDetails.", 3, textBoxes.Length);
					AssertEquals("Reference TextBox for R111 reference should be filled with the captured text.", "REF 1", textBoxes[1].Text);
					Assert("Reference TextBox for R111 reference should be visible.", textBoxes[1].Visible);
					Assert("Reference TextBox for R111 should not be readonly.", !textBoxes[1].ReadOnly);

					AssertEquals("Reference TextBox for C111 reference should be empty.", "", textBoxes[0].Text);
					Assert("Reference TextBox for C111 reference should be invisible.", !textBoxes[0].Visible);

					var dateTimeEdits = control.FindAll<ZDateEdit>(maxLevelsDeep: 3).OrderBy(c => c.Top).ToArray();
					AssertEquals("Date of issue DateEdit should be empty.", ZString.Empty, dateTimeEdits[1].Text);
					Assert("Date of issue DateEdit for R111 should be visible.", dateTimeEdits[1].Visible);
				});
			}
		}

		GuidedDecisionMakingBasic GetGuidedDecisionMakingBasicForTest()
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
			invoiceLine.SupportingDocuments.AddNew("R111", "REF 1");
			var guidedDecisionMakingBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			return guidedDecisionMakingBasic;
		}
	}
}
