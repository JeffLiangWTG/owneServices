using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GuidedDecisionMakingForm))]
	sealed class GuidedDecisionMakingFormTest : ZFormBasherTest
	{
		public void TestTabRefreshedIfAdditionalCodeIsChanged()
		{
			SetUpReferenceData();
			using (var form = (GuidedDecisionMakingFormForTest)GetFormToBashCore())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.DataGrouping = "LV";
				gdmBasic.CountryOfOrigin = "FR";
				gdmBasic.CountryOfDestination = "FR";
				gdmBasic.EffectiveDate = ZDate.Today;
				gdmBasic.TariffType = "IMP";
				gdmBasic.TariffCode = "19990728";
				gdmBasic.Preference = "100";
				gdmBasic.IsImport = true;
				form.Show();

				AssertEquals("Prerequisite: 3 additional codes is loaded.", 3, gdmBasic.AdditionalCodes.Count);

				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				var tabsManagement = form.gdmTabsManagementExposed;
				var additionalCodes = gdmBasic.AdditionalCodes;
				nextButton.PerformClick();

				AssertTabStatus("Meursing tab and Conditions tab should be NotApplicable when no additional codes are ticked.", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Current, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Incomplete });

				TickAdditionalCodes(additionalCodes, "ADD1");
				AssertTabStatus("Conditions tabs should be Unfinished when additional code for condition is ticked.", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Current, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Incomplete, GuidedDecisionMakingTabStatus.Incomplete });

				TickAdditionalCodes(additionalCodes, "ADD2");
				AssertTabStatus("Meursing tab and Conditions tab should be NotApplicable when additional code for non-meursing rate is ticked.", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Current, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Incomplete });

				TickAdditionalCodes(additionalCodes, "ADD3");
				AssertTabStatus("Meursing tab should be Unfinished when additional code for meursing rate is ticked.", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Current, GuidedDecisionMakingTabStatus.Incomplete, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Incomplete });

				TickAdditionalCodes(additionalCodes, "ADD1", "ADD3");
				AssertTabStatus("Meursing tab and Conditions tab should both be Unfinished when additional codes for condition and meursing rate are ticked.", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Current, GuidedDecisionMakingTabStatus.Incomplete, GuidedDecisionMakingTabStatus.Incomplete, GuidedDecisionMakingTabStatus.Incomplete });
			}

			void TickAdditionalCodes(GuidedDecisionMakingAdditionalCodeCollection additionalCodes, params string[] codes)
			{
				foreach (var code in additionalCodes.Cast<GuidedDecisionMakingAdditionalCode>())
				{
					code.IsTicked = codes.Contains(code.AdditionalCode.ToString());
				}
			}
		}

		void SetUpReferenceData()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2);
			RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, date1, date2);
			var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = RefDataHelper.CreateTariff(dataGrouping, tariffType.PK, "19990728", date1, date2, "Dummy Description");
			var ctrlType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR", "Test Ctrl Condition Type");
			var preference = RefDataHelper.CreatePreferenceView("100", "No Preference", dataGrouping);

			var conditionValueType = RefDataHelper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
			var condition1 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C1", true, false, date1, date2);
			RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "C111");
			RefDataHelper.CreateCusApplicability(condition1, tradeGroupStandard, date1, date2, "ADD1");

			var rateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "RT1");
			var rateCode = RefDataHelper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			var rate = RefDataHelper.CreateRate(tariff, rateCode.PK, date1, date2, "RATE", preference.PK, dataGrouping: dataGrouping);
			RefDataHelper.CreateCusApplicability(rate, tradeGroupStandard, date1, date2, "ADD2");

			var meursingCode = RefDataHelper.CreateCusRateCode(Factory, "RC2", rateType.PK);
			var meursingRate = RefDataHelper.CreateRate(tariff, meursingCode.PK, date1, date2, "#ADFM(2)#", preference.PK, dataGrouping: dataGrouping);
			RefDataHelper.CreateCusApplicability(meursingRate, tradeGroupStandard, date1, date2, "ADD3");

			Factory.Save();
		}

		[RequiresSTA]
		public void TestTabsOrder()
		{
			using (var form = (GuidedDecisionMakingFormForTest)GetFormToBashCore())
			{
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				AssertContainsExactElementsInExactOrder(new[] { "BasicTabPage", "VATTabPage", "AdditionalCodesTabPage", "MeursingTabPage", "ConditionsTabPage", "SummaryTabPage" }, gdmTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name).ToList());
			}
		}

		public void TestCompletedWithWarning_AdditionalCodes()
		{
			SetUpReferenceData();
			using (var form = (GuidedDecisionMakingFormForTest)GetFormToBashCore())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.DataGrouping = "LV";
				gdmBasic.CountryOfOrigin = "FR";
				gdmBasic.CountryOfDestination = "FR";
				gdmBasic.EffectiveDate = ZDate.Today;
				gdmBasic.TariffType = "IMP";
				gdmBasic.TariffCode = "19990728";
				gdmBasic.Preference = "100";
				gdmBasic.IsImport = true;
				form.Show();

				AssertEquals("Prerequisite: 3 additional codes is loaded.", 3, gdmBasic.AdditionalCodes.Count);

				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				var tabsManagement = form.gdmTabsManagementExposed;
				var additionalCodes = gdmBasic.AdditionalCodes;

				UnitTestUserNotification.Instance.AddYesAnswer();
				nextButton.PerformClick();

				UnitTestUserNotification.Instance.AddYesAnswer();
				nextButton.PerformClick();

				AssertTabStatus("AdditionalCodes tab should have status CompletedWithWarning", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.CompletedWithWarning, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Current });
			}

			void SetUpReferenceData()
			{
				var date1 = new ZDate(2010, 12, 10);
				var date2 = new ZDate(2079, 06, 06);
				var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

				var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2);
				RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, date1, date2);
				var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
				Factory.Save();

				var tariff = RefDataHelper.CreateTariff(dataGrouping, tariffType.PK, "19990728", date1, date2, "Dummy Description");
				var ctrlType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR", "Test Ctrl Condition Type");
				var preference = RefDataHelper.CreatePreferenceView("100", "No Preference", dataGrouping);

				var conditionValueType = RefDataHelper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
				var condition1 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C1", true, false, date1, date2);
				RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "C111");
				RefDataHelper.CreateCusApplicability(condition1, tradeGroupStandard, date1, date2, "ADD1");

				var rateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "RT1");
				var rateCode = RefDataHelper.CreateCusRateCode(Factory, "RC1", rateType.PK);
				var rate = RefDataHelper.CreateRate(tariff, rateCode.PK, date1, date2, "RATE", preference.PK, dataGrouping: dataGrouping);
				RefDataHelper.CreateCusApplicability(rate, tradeGroupStandard, date1, date2, "ADD2");

				var meursingCode = RefDataHelper.CreateCusRateCode(Factory, "RC2", rateType.PK);
				var meursingRate = RefDataHelper.CreateRate(tariff, meursingCode.PK, date1, date2, "#ADFM(2)#", preference.PK, dataGrouping: dataGrouping);
				RefDataHelper.CreateCusApplicability(meursingRate, tradeGroupStandard, date1, date2, "ADD3");

				Factory.Save();
			}
		}

		[RequiresSTA]
		public void TestCompletedWithWarning_Meursing()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				Assert("Prerequisite: meursing should be applicable for tariff.", gdmBasic.IsMeursingApplicable);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var tabsManagement = form.gdmTabsManagementExposed;
				var meursingTabPage = form.Controls.Find("MeursingTabPage", true)[0] as ZTabPage;
				meursingTabPage.Tag = "true";
				gdmTabControl.SelectedTab = meursingTabPage;
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				Assert("Prerequisite:", gdmTabControl.SelectedTab == meursingTabPage);

				UnitTestUserNotification.Instance.AddYesAnswer();
				nextButton.PerformClick();
				AssertTabStatus("Meursing tab should have status CompletedWithWarning", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.CompletedWithWarning, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Current });
			}
		}

		public void TestCompletedWithWarning_Conditions()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.MeursingResult = "7000";

				var documentConditions = gdmBasic.DocumentConditions;
				var condition = documentConditions.AddNew();
				condition.ConditionType = "CT1";
				var conditionDetail = condition.ConditionDetails.AddNew();
				conditionDetail.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				Assert("Prerequisite: all conditions should be satisfied.", !condition.IsSatisfied);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var tabsManagement = form.gdmTabsManagementExposed;
				var conditionsTabPage = form.Controls.Find("ConditionsTabPage", true)[0] as ZTabPage;
				conditionsTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();
				nextButton.PerformClick();
				Assert("Prerequisite:", gdmTabControl.SelectedTab == conditionsTabPage);
				UnitTestUserNotification.Instance.AddYesAnswer();
				nextButton.PerformClick();
				AssertTabStatus("Conditions tab should have status CompletedWithWarning", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.CompletedWithWarning, GuidedDecisionMakingTabStatus.Current });
			}
		}

		public void TestCompletedWithWarning_VAT()
		{
			SetUpReferenceData();
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.DataGrouping = "LV";
				gdmBasic.CountryOfOrigin = "FR";
				gdmBasic.CountryOfDestination = "FR";
				gdmBasic.EffectiveDate = ZDate.Today;
				gdmBasic.TariffType = "IMP";
				gdmBasic.TariffCode = "19990728";
				gdmBasic.Preference = "100";
				gdmBasic.IsImport = true;

				VATContainerPanelTest.SetUpGDMBasicWithVATForTest(Factory, gdmBasic);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var tabsManagement = form.gdmTabsManagementExposed;
				var vatTabPage = form.Controls.Find("VATTabPage", true)[0] as ZTabPage;
				vatTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();

				Assert("Prerequisite: Current tab page should be VAT.", gdmTabControl.SelectedTab == vatTabPage);
				UnitTestUserNotification.Instance.AddYesAnswer();
				nextButton.PerformClick();
				AssertTabStatus("VAT tab should have status CompletedWithWarning", gdmTabControl, tabsManagement, new[] { GuidedDecisionMakingTabStatus.Completed, GuidedDecisionMakingTabStatus.CompletedWithWarning, GuidedDecisionMakingTabStatus.Current, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.NotApplicable, GuidedDecisionMakingTabStatus.Incomplete });
			}
		}

		[RequiresSTA]
		public void TestNoWarningWhenCaptureIsComplete_VAT()
		{
			SetUpReferenceData();
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.DataGrouping = "LV";
				gdmBasic.CountryOfOrigin = "FR";
				gdmBasic.CountryOfDestination = "FR";
				gdmBasic.EffectiveDate = ZDate.Today;
				gdmBasic.TariffType = "IMP";
				gdmBasic.TariffCode = "19990728";
				gdmBasic.Preference = "100";
				gdmBasic.IsImport = true;

				VATContainerPanelTest.SetUpGDMBasicWithVATForTest(Factory, gdmBasic);
				gdmBasic.VATApplicabilities[0].IsTicked = true;

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var vatTabPage = form.Controls.Find("VATTabPage", true)[0] as ZTabPage;
				vatTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();

				Assert("Prerequisite: Current tab page should be VAT.", gdmTabControl.SelectedTab == vatTabPage);
				nextButton.PerformClick();
				AssertNull("No Warning Text as a VAT has been ticked.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWaringWhenCaptureIsComplete_VAT()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.MeursingResult = "7000";
				gdmBasic.IsImport = true;
				gdmBasic.IsExport = false;
				gdmBasic.CountryOfDestination = "";
				gdmBasic.CustomsFirstQuantity = 10;

				gdmBasic.Validation.ValidateAll();
				VATContainerPanelTest.SetUpGDMBasicWithVATForTest(Factory, gdmBasic);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var vatTabPage = form.Controls.Find("VATTabPage", true)[0] as ZTabPage;
				vatTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();

				Assert("Prerequisite: Current tab page should be VAT.", gdmTabControl.SelectedTab == vatTabPage);
				nextButton.PerformClick();
				AssertContains("You didn't choose any VAT code.\r\nDo you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestNoWarningWhenCaptureIsComplete_Meursing()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;

				Assert("Prerequisite: meursing should be applicable for tariff.", gdmBasic.IsMeursingApplicable);
				gdmBasic.MeursingResult = "7000";

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var meursingTabPage = form.Controls.Find("MeursingTabPage", true)[0] as ZTabPage;
				meursingTabPage.Tag = "true";
				gdmTabControl.SelectedTab = meursingTabPage;
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				Assert("Prerequisite:", gdmTabControl.SelectedTab == meursingTabPage);
				nextButton.PerformClick();
				AssertNull("No Warning message box fired when meursing capture is complete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWarningWhenCaptureIsIncomplete_Meursing()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;

				Assert("Prerequisite: meursing should be applicable for tariff.", gdmBasic.IsMeursingApplicable);
				gdmBasic.MeursingResult = ZString.Empty;

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var meursingTabPage = form.Controls.Find("MeursingTabPage", true)[0] as ZTabPage;
				meursingTabPage.Tag = "true";
				gdmTabControl.SelectedTab = meursingTabPage;
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				Assert("Prerequisite:", gdmTabControl.SelectedTab == meursingTabPage);
				nextButton.PerformClick();
				AssertContains("Warning message box should popup when Meursing capture is incomplete.", "A supplementary code matching pattern '7NNN' should exist for Meursing duty calculation purposes, do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoWarningWhenCaptureIsComplete_Conditions()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.MeursingResult = "7000";

				var documentConditions = gdmBasic.DocumentConditions;
				var condition = documentConditions.AddNew();
				condition.ConditionType = "CT1";
				var conditionDetail = condition.ConditionDetails.AddNew();
				conditionDetail.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail.IsTicked = true;
				Assert("Prerequisite: all conditions should be satisfied.", condition.IsSatisfied);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var conditionsTabPage = form.Controls.Find("ConditionsTabPage", true)[0] as ZTabPage;
				conditionsTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();
				nextButton.PerformClick();
				Assert("Prerequisite:", gdmTabControl.SelectedTab == conditionsTabPage);
				nextButton.PerformClick();
				AssertNull("No Warning message box fired when conditions capture is complete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWarningWhenCaptureIsIncomplete_NonInformationConditions()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.MeursingResult = "7000";

				var documentConditions = gdmBasic.DocumentConditions;
				var condition1 = documentConditions.AddNew();
				condition1.ConditionType = "CT1";
				condition1.InformationValue = ZString.Empty;
				condition1.ConditionTypeDescription = "Condition Type Description 1";
				var conditionDetail11 = condition1.ConditionDetails.AddNew();
				conditionDetail11.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				conditionDetail11.IsTicked = true;
				AssertEquals("Prerequisite: first condition should not be satisfied.", false, condition1.IsSatisfied);

				var conditionDetail12 = condition1.ConditionDetails.AddNew();
				conditionDetail12.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail12.IsTicked = false;
				AssertEquals("Prerequisite: second condition should not be satisfied.", false, condition1.IsSatisfied);

				var condition2 = documentConditions.AddNew();
				condition2.ConditionType = "CT2";
				condition2.InformationValue = ZString.Empty;
				condition2.ConditionTypeDescription = "Condition Type Description 2";
				var conditionDetail21 = condition2.ConditionDetails.AddNew();
				conditionDetail21.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail21.IsTicked = false;
				AssertEquals("Prerequisite: third condition should not be satisfied.", false, condition2.IsSatisfied);

				var conditionDetail22 = condition2.ConditionDetails.AddNew();
				conditionDetail22.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail22.IsTicked = false;
				AssertEquals("Prerequisite: fourth condition should not be satisfied.", false, condition2.IsSatisfied);

				var condition3 = documentConditions.AddNew();
				condition3.ConditionType = "CT3";
				condition3.InformationValue = ZString.Empty;
				condition3.ConditionTypeDescription = "Condition Type Description 3";
				var conditionDetail3 = condition3.ConditionDetails.AddNew();
				conditionDetail3.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail3.IsTicked = true;
				Assert("Prerequisite: fifth condition should be satisfied.", conditionDetail3.IsSatisfied);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var conditionsTabPage = form.Controls.Find("ConditionsTabPage", true)[0] as ZTabPage;
				conditionsTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();
				nextButton.PerformClick();
				Assert("Prerequisite:", gdmTabControl.SelectedTab == conditionsTabPage);
				nextButton.PerformClick();
				AssertContains("Warning message box should popup when conditions capture is incomplete.", "You didn't fill in document reference for\r\n- CT1, Condition Type Description 1\r\nYou didn't choose any conditions for\r\n- CT2, Condition Type Description 2\r\n\r\nDo you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestWarningWhenInformationConditionsNotFulfilled()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.MeursingResult = "7000";

				var documentConditions = gdmBasic.DocumentConditions;
				var condition1 = documentConditions.AddNew();
				condition1.ConditionType = "CT1";
				condition1.InformationValue = "INF";
				condition1.ConditionTypeDescription = "Condition Type Description 1";
				var conditionDetail11 = condition1.ConditionDetails.AddNew();
				conditionDetail11.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				conditionDetail11.IsTicked = true;
				AssertEquals("Prerequisite: first condition should not be satisfied.", false, condition1.IsSatisfied);

				var conditionDetail12 = condition1.ConditionDetails.AddNew();
				conditionDetail12.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail12.IsTicked = false;
				AssertEquals("Prerequisite: second condition should not be satisfied.", false, condition1.IsSatisfied);

				var condition2 = documentConditions.AddNew();
				condition2.ConditionType = "CT2";
				condition2.InformationValue = "INF";
				condition2.ConditionTypeDescription = "Condition Type Description 2";
				var conditionDetail21 = condition2.ConditionDetails.AddNew();
				conditionDetail21.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail21.IsTicked = false;
				AssertEquals("Prerequisite: third condition should not be satisfied.", false, condition2.IsSatisfied);

				var conditionDetail22 = condition2.ConditionDetails.AddNew();
				conditionDetail22.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail22.IsTicked = false;
				AssertEquals("Prerequisite: fourth condition should not be satisfied.", false, condition2.IsSatisfied);

				var condition3 = documentConditions.AddNew();
				condition3.ConditionType = "CT3";
				condition3.InformationValue = ZString.Empty;
				condition3.ConditionTypeDescription = "Condition Type Description 3";
				var conditionDetail3 = condition3.ConditionDetails.AddNew();
				conditionDetail3.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail3.IsTicked = true;
				Assert("Prerequisite: fifth condition should be satisfied.", conditionDetail3.IsSatisfied);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var conditionsTabPage = form.Controls.Find("ConditionsTabPage", true)[0] as ZTabPage;
				conditionsTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();
				nextButton.PerformClick();
				Assert("Prerequisite:", gdmTabControl.SelectedTab == conditionsTabPage);
				nextButton.PerformClick();
				AssertNotContains("Not selecting an information document should not be a reason to prevent user from navigating the next tab.", "You didn't fill in document reference for\r\n- CT1, Condition Type Description 1\r\nYou didn't choose any conditions for\r\n- CT2, Condition Type Description 2\r\n\r\nDo you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

#if !WINZOR

		[RequiresSTA]
		public void TestNoWarningWhenCaptureIsComplete_AdditionalCodes()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				AssertEquals("Prerequisite : no error on gdmBasic", false, gdmBasic.HasErrors);

				var additionalCodes = gdmBasic.AdditionalCodes;
				var additionalCode = additionalCodes.AddNew();
				additionalCode.AdditionalCode = "MCD";
				additionalCode.ApplicableToDescription = "Description";
				additionalCode.IsTicked = true;

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var additionalCodesTabPage = form.Controls.Find("AdditionalCodesTabPage", true)[0] as ZTabPage;
				additionalCodesTabPage.Tag = "true";
				gdmTabControl.SelectedTab = additionalCodesTabPage;
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				Assert("Prerequisite:", gdmTabControl.SelectedTab == additionalCodesTabPage);
				nextButton.PerformClick();
				AssertNull("No Warning message box fired because Additional Codes capture is complete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWarningWhenCaptureIsIncomplete_AdditionalCodes()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;

				var additionalCodes = gdmBasic.AdditionalCodes;

				var additionalCode11 = additionalCodes.AddNew();
				additionalCode11.AdditionalCode = "Q201";
				additionalCode11.ApplicableToType = "RED";
				additionalCode11.ApplicableToDescription = "RateType";
				additionalCode11.IsTicked = false;

				var additionalCode12 = additionalCodes.AddNew();
				additionalCode12.AdditionalCode = "Q202";
				additionalCode12.ApplicableToType = "RED";
				additionalCode12.ApplicableToDescription = "RateType";
				additionalCode12.IsTicked = false;

				var additionalCode21 = additionalCodes.AddNew();
				additionalCode21.AdditionalCode = "C001";
				additionalCode21.ApplicableToType = "CTR";
				additionalCode21.ApplicableToDescription = "Control";
				additionalCode21.IsTicked = false;

				var additionalCode22 = additionalCodes.AddNew();
				additionalCode22.AdditionalCode = "C002";
				additionalCode22.ApplicableToType = "CTR";
				additionalCode22.ApplicableToDescription = "Control";
				additionalCode22.IsTicked = false;

				var additionalCode31 = additionalCodes.AddNew();
				additionalCode31.AdditionalCode = "X007";
				additionalCode31.ApplicableToType = "BDU";
				additionalCode31.ApplicableToDescription = "Dual use";
				additionalCode31.IsTicked = true;

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var additionalCodesTabPage = form.Controls.Find("AdditionalCodesTabPage", true)[0] as ZTabPage;
				additionalCodesTabPage.Tag = "true";
				var nextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				nextButton.PerformClick();
				Assert("Prerequisite:", gdmTabControl.SelectedTab == additionalCodesTabPage);
				nextButton.PerformClick();
				AssertContains("Warning message box should popup when Additional Code capture is incomplete.", "You didn't choose any additional code for\r\n- Control\r\n- RateType\r\n\r\nDo you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

#endif

		public void TestNoErrorCheckWhenClickingDoneButton()
		{
			using (var form = (GuidedDecisionMakingFormForTest)GetFormToBashCore())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				AssertEquals("Prerequisite : error on one of the tabs.", true, gdmBasic.HasErrors);

				form.Show();
				var gdmTabControl = form.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
				var summaryTab = form.Controls.Find("SummaryTabPage", true)[0] as ZTabPage;
				gdmTabControl.SelectedTab = summaryTab;
				var previousButton = form.Controls.Find("PreviousButton", true)[0] as ZButton;
				AssertEquals("Previous (1. Basic)", previousButton.Text);
				var summaryDoneButton = form.Controls.Find("DoneButton", true)[0] as ZButton;
				summaryDoneButton.PerformClick();
				AssertNull("No error message box", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestErrorCheckWhenClickingBasicNextButton()
		{
			using (var form = (GuidedDecisionMakingFormForTest)GetFormToBashCore())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				AssertEquals("Prerequisite: error in Basic tab.", true, gdmBasic.HasErrors);

				form.Show();
				var previousButton = form.Controls.Find("PreviousButton", true)[0] as ZButton;
				AssertEquals(false, previousButton.Visible);
				var basicNextButton = form.Controls.Find("NextButton", true)[0] as ZButton;
				AssertEquals("Next (6. Summary)", basicNextButton.Text);
				basicNextButton.PerformClick();
				AssertContains("Error message box fired", "You must fix errors before switching to next tab.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestGDMBasicLayoutPanel()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNotNull("GDMBasicLayoutPanel", form.Controls.Find("GDMBasicLayoutPanel", false));
			}
		}

		[RequiresSTA]
		public void TestAdditionalCodesContainerPanel()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNotNull("AdditionalCodesContainerPanel", form.Controls.Find("AdditionalCodesContainerPanel", false));
			}
		}

		[RequiresSTA]
		public void TestDocumentConditionsContainerPanel()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNotNull("DocumentConditionsContainerPanel", form.Controls.Find("DocumentConditionsContainerPanel", false));
			}
		}

		[RequiresSTA]
		public void TestSummaryContainerPanel()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNotNull("SummaryContainerPanel", form.Controls.Find("SummaryContainerPanel", false));
			}
		}

		public void TestMeursingTabPageContainsMeursingUserControl()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var meursingTabPage = form.Controls.Find("MeursingTabPage", true)[0];
				var meursingGroupBox = meursingTabPage.Controls.Find("MeursingGroupBox", true)[0];

				var meursingUserControl = meursingGroupBox.Controls.Find("MeursingUserControl", true)[0];
				AssertNotNull(meursingUserControl);
				AssertType<MeursingUserControl>(meursingUserControl);

				var meursingResultUserControl = meursingGroupBox.Controls.Find("MeursingResultUserControl", true)[0];
				AssertNotNull(meursingResultUserControl);
				AssertType<MeursingResultUserControl>(meursingResultUserControl);
			}
		}

		[RequiresSTA]
		public void TestSelectWaivers_TicksWaiver_WhenConditionsAreMet()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.MeursingResult = "7000";

				var documentConditions = gdmBasic.DocumentConditions;
				var condition1 = documentConditions.AddNew();
				condition1.ConditionTypeDescription = "Condition Type Description 1";
				var conditionDetail11 = condition1.ConditionDetails.AddNew();
				conditionDetail11.Code = "Y123";
				conditionDetail11.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				conditionDetail11.IsTicked = false;

				var conditionDetail12 = condition1.ConditionDetails.AddNew();
				conditionDetail12.Code = "NONWAIVER";
				conditionDetail12.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail12.IsTicked = false;

				form.SelectWaiversExposed();

				var waiverDetail = condition1.ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().First(x => x.Code == "Y123");
				Assert(waiverDetail.IsTicked);
			}
		}

		[RequiresSTA]
		public void TestSelectWaivers_DoesNotTickWhenConditionsAreNotMet()
		{
			using (var form = PrepareTabStatusTest())
			{
				var gdmBasic = form.GetBusinessEntityExposed;
				gdmBasic.MeursingResult = "7000";

				var documentConditions = gdmBasic.DocumentConditions;
				var condition1 = documentConditions.AddNew();
				condition1.ConditionType = "CT1";
				var conditionDetail11 = condition1.ConditionDetails.AddNew();
				conditionDetail11.Code = "A123";
				conditionDetail11.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				conditionDetail11.IsTicked = false;

				var conditionDetail12 = condition1.ConditionDetails.AddNew();
				conditionDetail12.Code = "NONWAIVER";
				conditionDetail12.Type = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				conditionDetail12.IsTicked = false;

				form.SelectWaiversExposed();

				var waiverDetail = condition1.ConditionDetails.Cast<GuidedDecisionMakingConditionDetail>().First(x => x.Code == "A123");
				Assert(!waiverDetail.IsTicked);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.FillWithValidTestData();
			var target = new GuidedDecisionMakingSingleInvoiceLineTarget(line1);
			return new GuidedDecisionMakingFormForTest(new GuidedDecisionMakingSingleInvoiceLineSource(line1), target, Factory);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;

		public class GuidedDecisionMakingFormForTest : GuidedDecisionMakingForm
		{
			public GuidedDecisionMakingFormForTest(IGuidedDecisionMakingSource guidedDecisionMakingSource, IGuidedDecisionMakingTarget guidedDecisionMakingTarget, BusinessObjectFactory factory)
			: base(new GuidedDecisionMakingBasic(guidedDecisionMakingSource, factory), guidedDecisionMakingTarget)
			{
			}

			public GuidedDecisionMakingBasic GetBusinessEntityExposed => (GuidedDecisionMakingBasic)base.BusinessEntity;

			public GuidedDecisionMakingTabsManagement gdmTabsManagementExposed => base.gdmTabsManagement;

			public List<ZTabPage> TabPageSequence
			{
				get
				{
					var gdmTabControl = this.Controls.Find("GuidedDecisionMakingTabControl", true)[0] as ZTabControl;
					var basicTabPage = this.Controls.Find("BasicTabPage", true)[0] as ZTabPage;
					var vatTabPage = this.Controls.Find("VATTabPage", true)[0] as ZTabPage;
					var additionalCodesTabPage = this.Controls.Find("AdditionalCodesTabPage", true)[0] as ZTabPage;
					var meursingTabPage = this.Controls.Find("MeursingTabPage", true)[0] as ZTabPage;
					var conditionsTabPage = this.Controls.Find("ConditionsTabPage", true)[0] as ZTabPage;
					var summaryTabPage = this.Controls.Find("SummaryTabPage", true)[0] as ZTabPage;

					return new List<ZTabPage> { basicTabPage, vatTabPage, additionalCodesTabPage, meursingTabPage, conditionsTabPage, summaryTabPage };
				}
			}

			public void SelectWaiversExposed() => SelectWaivers();
		}

		void AssertTabStatus(string message, ZTabControl tabControl, GuidedDecisionMakingTabsManagement tabsManagement, GuidedDecisionMakingTabStatus[] expectedStatuses)
		{
			var statuses = tabControl.TabPages.ToList<ZTabPage>().Select(x => tabsManagement.FindGuidedDecisionMakingTab(x).Status);
			AssertSequencesEqual(message, expectedStatuses, statuses);
		}
		GuidedDecisionMakingFormForTest PrepareTabStatusTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDate.Today.AddDays(-2);
			var endDate = ZDate.Today.AddDays(2);
			helper.CreateNewOrGetExistingDataGrouping("US", "US");
			var tradeGroup = helper.CreateTradeGroup("FR", "TG1", startDate, endDate);
			helper.AddCountry(tradeGroup, "US", startDate, endDate, "America");
			var tariffType = helper.CreateNewOrGetExistingTariffType("FR", "ADD");
			Factory.Save();

			var tariff = helper.CreateTariff("FR", tariffType.PK, "22222222", startDate, endDate);
			var rateType = helper.CreateNewOrGetExistingRateType("FR", "RTE");
			var rateCode = helper.CreateCusRateCode(Factory, "RCD", rateType.PK);
			var preference = helper.CreatePreferenceView("100", "No Preference", "FR");
			var rate = helper.CreateRate(tariff, rateCode.PK, startDate, endDate, "#ADFM(2)#", preference.PK, dataGrouping: "FR");
			helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate, "", "ORD1");

			var form = (GuidedDecisionMakingFormForTest)GetFormToBashCore();

			var gdmBasic = form.GetBusinessEntityExposed;
			gdmBasic.DataGrouping = "FR";
			gdmBasic.CountryOfOrigin = "US";
			gdmBasic.CountryOfDestination = "US";
			gdmBasic.EffectiveDate = ZDate.Today;
			gdmBasic.TariffType = "ADD";
			gdmBasic.TariffCode = tariff.ZZ1_TariffCode;
			gdmBasic.Preference = preference.ZZS_Preference;
			gdmBasic.QuotaOrderNumber = "ORD1";

			return form;
		}
	}
}
