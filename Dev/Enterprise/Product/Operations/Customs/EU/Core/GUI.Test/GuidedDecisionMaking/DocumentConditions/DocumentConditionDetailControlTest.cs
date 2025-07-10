using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using UniversalReferenceConstants = Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class DocumentConditionDetailControlTest : TestCaseWithFactory
	{
		public void TestDocumentConditionDetailControl()
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
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew("C111", "C111 REFERENCE");
			supportingDocument.CSI_DateOfIssue = new ZDateTime(2023, 06, 22);
			var guidedDecisionMakingBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var detail = guidedDecisionMakingBasic.DocumentConditions[0].ConditionDetails[0];

			using (var form = new ZForm(guidedDecisionMakingBasic))
			using (var control = new DocumentConditionDetailControl(detail))
			{
				control.BindingSource.SetDataBinding(detail, "");
				form.Controls.Add(control);
				form.Show();
				UserIdleWorker.Flush();
				CombineAssertions(() =>
				{
					var checkBox = control.FindSingle<ZCheckBox>();
					AssertEquals("CheckBox: C111", ZString.Empty, checkBox.Text);
					AssertEquals("CheckBox: C111 Checked by default", true, checkBox.Checked);

					var codeFindBox = control.FindSingle<ZCodeFindBox>();
					AssertEquals("CodeFindBox: C111 code should be filled in the CodeBox", "C111", codeFindBox.CodeBox.Text);
					AssertEquals("CodeFindBox: C111 Descrption should be filled in the DescriptionBox", "C111 desc", codeFindBox.DescriptionBox.Text);
					Assert("CodeFindBox: ReadOnly", codeFindBox.ReadOnly);
					Assert("CodeFindBox: CodeBox should be ReadOnly", codeFindBox.CodeBox.ReadOnly);
					Assert("CodeFindBox: DescriptionBox should be ReadOnly", codeFindBox.DescriptionBox.ReadOnly);

					var textBox = control.FindSingle<ZTextBox>(maxLevelsDeep: 0);
					AssertEquals("TextBox: C111 REFERENCE", "C111 REFERENCE", textBox.Text);
					AssertEquals("TextBox: can edit", false, textBox.ReadOnly);

					var dateEdit = control.FindSingle<ZDateEdit>(maxLevelsDeep: 0);
					AssertEquals("DateEdit: 22-JUN-23", "22-JUN-23", dateEdit.Text);
					AssertEquals("DateEdit: can edit", false, dateEdit.ReadOnly);

					checkBox.Checked = false;
					AssertEquals("UnTicked", false, detail.IsTicked);
					AssertEquals("TextBox: can not edit when not ticked", true, textBox.ReadOnly);

					checkBox.Checked = true;
					AssertEquals("Ticked", true, detail.IsTicked);
					AssertEquals("TextBox: can edit when ticked", false, textBox.ReadOnly);

					AssertEquals("Maximum length of supporting document reference is 50", 50, textBox.MaxLength);
				});
			}

			detail.Type = Enterprise.Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
			using (var form = new ZForm(guidedDecisionMakingBasic))
			using (var control = new DocumentConditionDetailControl(detail))
			{
				control.BindingSource.SetDataBinding(detail, "");
				form.Controls.Add(control);
				form.Show();

				var textBox = control.FindSingle<ZTextBox>(maxLevelsDeep: 0);
				AssertEquals("TextBox: be invisible for SupportingDocumentNoReferenceNumber", false, textBox.Visible);

				var dateEdit = control.FindSingle<ZDateEdit>(maxLevelsDeep: 0);
				AssertEquals("DateEdit: be invisible for SupportingDocumentNoReferenceNumber", false, textBox.Visible);
			}
		}

		void SetupConditons()
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var rateType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "RAT1", "Test Rate Condition Type");
			var preference = helper.CreatePreferenceForCountry("P1", "TestPreference", dataGrouping);
			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1122334455667", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);

			var condition1ValueType1 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
			var condition1 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, rateType.PK, tariff.PK, "C1", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition1.PK, "C111");
			Factory.Save();

			var testRate1 = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preferencePk: preference.PK);
			helper.CreateCusApplicability(testRate1, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add11", "");

			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "C111", "C111 desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
