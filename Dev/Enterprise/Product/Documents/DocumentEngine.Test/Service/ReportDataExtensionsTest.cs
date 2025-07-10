using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using MultipleChoice = Enterprise.DocumentEngine.RuntimeOptions.MultipleChoice;
using NumberRangeField = Enterprise.DocumentEngine.RuntimeOptions.NumberRangeField;
using SecurityFilterField = Enterprise.DocumentEngine.RuntimeOptions.SecurityFilterField;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportDataExtensionsTest : TestCaseWithFactory
	{
		public void TestReportLanguageAndOrientationData()
		{
			var command = new ReportDataServiceTest().StaffProfileReport;
			var reportData = command.GetReportData();

			AssertEquals("EN-US", reportData.Language.PrintLanguage);
			Assert(!reportData.Language.HiddenInGui);
			AssertNotEquals(0, reportData.Language.LanguageList.Count);

			AssertEquals(ReportOrientationTypeList.Codes.Default, reportData.Orientation.Orientation);
			Assert(!reportData.Orientation.HiddenInGui);
			var orientationCodeList = reportData.Orientation.OrientationList.Select(c => c.Code).ToList();
			AssertCollectionContains(ReportOrientationTypeList.Codes.Default, orientationCodeList);
			AssertCollectionContains(ReportOrientationTypeList.Codes.Landscape, orientationCodeList);
			AssertCollectionContains(ReportOrientationTypeList.Codes.Portrait, orientationCodeList);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetReportData()
		{
			var reportData = ReportCommand.GetReportData();

			AssertEquals(reportData.Id, ReportCommand.PK);
			AssertEquals(reportData.ReportName, ReportCommand.SU_MenuName);
			AssertEquals(3, reportData.GroupBys.GroupByCollection.Count);
			AssertEquals(2, reportData.SortOrderCollection.Count);
			AssertEquals(2, reportData.OptionalTemplateCollection.Count);
			AssertNullOrEmpty(reportData.Language.PrintLanguage);
			Assert(reportData.Language.HiddenInGui);
			AssertEquals(0, reportData.Language.LanguageList.Count);
			AssertNullOrEmpty(reportData.Orientation.Orientation);
			Assert(reportData.Orientation.HiddenInGui);
			AssertEquals(0, reportData.Orientation.OrientationList.Count);
			AssertEquals("LookupFieldUserControl", reportData.LinkedLookupFilter.DisplayName);
			AssertEquals(1, reportData.WorkSheets.Count);

			AssertEquals(1, reportData.FilterData.TextRangeFilterCollection.Count);
			AssertEquals("TextRangeFieldUserControl", reportData.FilterData.TextRangeFilterCollection[0].DisplayName);

			AssertEquals(2, reportData.FilterData.TextFilterCollection.Count);
			AssertEquals("TextFieldUserControl", reportData.FilterData.TextFilterCollection[0].DisplayName);
			AssertEquals("StillTextFieldUserControl", reportData.FilterData.TextFilterCollection[1].DisplayName);

			AssertEquals(1, reportData.FilterData.NumberFilterCollection.Count);
			AssertEquals("NumberUserControl", reportData.FilterData.NumberFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.NumberRangeFilterCollection.Count);
			AssertEquals("NumberRangeUserControl", reportData.FilterData.NumberRangeFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.NumberNotInRangeFilterCollection.Count);
			AssertEquals("NumberNotInRangeUserControl", reportData.FilterData.NumberNotInRangeFilterCollection[0].DisplayName);

			AssertEquals(2, reportData.FilterData.DateFilterCollection.Count);
			AssertEquals("DateFieldUserControl", reportData.FilterData.DateFilterCollection[0].DisplayName);
			AssertEquals("DateFieldUserControlEmpty", reportData.FilterData.DateFilterCollection[1].DisplayName);

			AssertEquals(1, reportData.FilterData.DateRangeFilterCollection.Count);
			AssertEquals("DateRangeFieldUserControl", reportData.FilterData.DateRangeFilterCollection[0].DisplayName);
			AssertEquals("Short", reportData.FilterData.DateRangeFilterCollection[0].DateFormat);

			AssertEquals(1, reportData.FilterData.SecurityFilterCollection.Count);
			AssertEquals("SecurityFilterControl", reportData.FilterData.SecurityFilterCollection[0].DisplayName);

			AssertEquals(2, reportData.FilterData.MultipleSelectionLookupFilterCollection.Count);
			AssertEquals("MultipleSelectionLookupUserControl (Org)", reportData.FilterData.MultipleSelectionLookupFilterCollection[0].DisplayName);
			AssertEquals("MultipleSelectionLookupUserControl (Invoice Currency)", reportData.FilterData.MultipleSelectionLookupFilterCollection[1].DisplayName);

			AssertEquals(1, reportData.FilterData.LookupFilterCollection.Count);
			AssertEquals("LookupFieldUserControl", reportData.FilterData.LookupFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.CodeLookupFilterCollection.Count);
			AssertEquals("CodeLookupFieldUserControl (Staff Member)", reportData.FilterData.CodeLookupFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.CodeListMultipleChoiceFilterCollection.Count);
			AssertEquals("CodeListMultipleChoiceUserControl (Consolidation Category)", reportData.FilterData.CodeListMultipleChoiceFilterCollection[0].DisplayName);
			AssertNotEquals(0, reportData.FilterData.CodeListMultipleChoiceFilterCollection[0].List.Count);

			AssertEquals(1, reportData.FilterData.OptionGroupFilterCollection.Count);
			AssertEquals("OptionGroupUserControl", reportData.FilterData.OptionGroupFilterCollection[0].DisplayName);
			AssertEquals(3, reportData.FilterData.OptionGroupFilterCollection[0].Options.Count);
			var options = reportData.FilterData.OptionGroupFilterCollection[0].Options.Select(o => o.Description);
			Assert(options.Contains("Credit Controller"));
			Assert(options.Contains("Customer Service Representative"));
			Assert(options.Contains("Sales Representative"));

			AssertEquals(1, reportData.FilterData.MultipleChoiceFilterCollection.Count);
			AssertEquals("MultipleChoiceUserControl", reportData.FilterData.MultipleChoiceFilterCollection[0].DisplayName);
			AssertEquals(2, reportData.FilterData.MultipleChoiceFilterCollection[0].List.Count);

			AssertEquals(1, reportData.FilterData.SingleAccountingPeriodFilterCollection.Count);
			AssertEquals("SingleAccountingPeriodUserControl", reportData.FilterData.SingleAccountingPeriodFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.AccountingPeriodsRangeFilterCollection.Count);
			AssertEquals("AccountingPeriodsRangeUserControl", reportData.FilterData.AccountingPeriodsRangeFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.AccountingNumberRangeFilterCollection.Count);
			AssertEquals("AccountingNumberRangeUserControl", reportData.FilterData.AccountingNumberRangeFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.AccountingPeriodFilterCollection.Count);
			AssertEquals("AccountingPeriodFieldUserControl", reportData.FilterData.AccountingPeriodFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.SalesTradeLaneChecklistFilterCollection.Count);
			AssertEquals("SalesTradeLaneChecklistUserControl", reportData.FilterData.SalesTradeLaneChecklistFilterCollection[0].DisplayName);
			AssertNotEquals(0, reportData.FilterData.SalesTradeLaneChecklistFilterCollection[0].RootItems);

			AssertEquals(1, reportData.FilterData.RegistrationCodedFilterCollection.Count);
			AssertEquals("RegistrationCodedUserControl", reportData.FilterData.RegistrationCodedFilterCollection[0].DisplayName);

			AssertEquals(1, reportData.FilterData.PermitTypeChecklistFilterCollection.Count);
			AssertEquals("Permit Type/Sub Type", reportData.FilterData.PermitTypeChecklistFilterCollection[0].DisplayName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetReportData_ForLookUpFilter()
		{
			var reportData = ReportCommandForLookUpFilter.GetReportData();

			AssertEquals(6, reportData.FilterData.LookupFilterCollection.Count);
			AssertEquals("LookupFieldUserControl", reportData.FilterData.LookupFilterCollection[0].DisplayName);
			AssertEquals("organisation", reportData.FilterData.LookupFilterCollection[0].LookupType);
			AssertEquals("LookupFieldUserControl2", reportData.FilterData.LookupFilterCollection[1].DisplayName);
			AssertEquals("address", reportData.FilterData.LookupFilterCollection[1].LookupType);
			AssertEquals("LookupFieldUserControl3", reportData.FilterData.LookupFilterCollection[2].DisplayName);
			AssertEquals("carrier", reportData.FilterData.LookupFilterCollection[2].LookupType);
			AssertEquals("LookupFieldUserControl4", reportData.FilterData.LookupFilterCollection[3].DisplayName);
			AssertEquals("warehouse", reportData.FilterData.LookupFilterCollection[3].LookupType);
			AssertEquals("LookupFieldUserControl5", reportData.FilterData.LookupFilterCollection[4].DisplayName);
			AssertEquals("warehouse client", reportData.FilterData.LookupFilterCollection[4].LookupType);
			AssertEquals("LookupFieldUserControl6", reportData.FilterData.LookupFilterCollection[5].DisplayName);
			AssertEquals("warehouse product", reportData.FilterData.LookupFilterCollection[5].LookupType);

			AssertEquals(1, reportData.FilterData.CodeLookupFilterCollection.Count);
			AssertEquals("CodeLookupFieldUserControl (Staff Member)", reportData.FilterData.CodeLookupFilterCollection[0].DisplayName);
			AssertEquals("staff", reportData.FilterData.CodeLookupFilterCollection[0].LookupType);

			AssertEquals(1, reportData.FilterData.MultipleSelectionLookupFilterCollection.Count);
			AssertEquals("Orgs", reportData.FilterData.MultipleSelectionLookupFilterCollection[0].DisplayName);
			AssertEquals("organisation", reportData.FilterData.MultipleSelectionLookupFilterCollection[0].LookupType);

			using (Env.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				reportData = ReportCommandForLookUpFilter.GetReportData();

				AssertEquals(4, reportData.FilterData.LookupFilterCollection.Count);
				AssertEquals("LookupFieldUserControl3", reportData.FilterData.LookupFilterCollection[0].DisplayName);
				AssertEquals("carrier", reportData.FilterData.LookupFilterCollection[0].LookupType);
				AssertEquals("LookupFieldUserControl4", reportData.FilterData.LookupFilterCollection[1].DisplayName);
				AssertEquals("warehouse", reportData.FilterData.LookupFilterCollection[1].LookupType);
				AssertEquals("LookupFieldUserControl5", reportData.FilterData.LookupFilterCollection[2].DisplayName);
				AssertEquals("warehouse client", reportData.FilterData.LookupFilterCollection[2].LookupType);
				AssertEquals("LookupFieldUserControl6", reportData.FilterData.LookupFilterCollection[3].DisplayName);
				AssertEquals("warehouse product", reportData.FilterData.LookupFilterCollection[3].LookupType);

				AssertEquals(0, reportData.FilterData.CodeLookupFilterCollection.Count);

				AssertEquals(0, reportData.FilterData.MultipleSelectionLookupFilterCollection.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFillReportData()
		{
			using var report = ReportCommand.GetReport();
			var reportData = PrepareReportData(report);

			report.FillReportData(reportData);

			Assert(report.GroupByCollection.BreakPageOverride);
			var selectedGroupBys = report.GroupByCollection.Where(g => g.BoolValue).ToList();
			AssertEquals(1, selectedGroupBys.Count);
			AssertEquals("Alpha", ((GroupBy)selectedGroupBys[0]).DisplayName);

				var sortOrders = report.SortOrderCollection.Where(s => s.BoolValue).ToList();
				AssertEquals(1, sortOrders.Count);
				AssertEquals("SecondSort", ((RuntimeOptions.SortOrder)sortOrders[0]).DisplayName);

			var optionalTemplates = report.OptionalTemplateSheetCollection.Where(o => o.BoolValue).ToList();
			AssertEquals(1, optionalTemplates.Count);
			AssertEquals("Second Template", ((OptionalTemplateSheet)optionalTemplates[0]).DisplayName);

			var textRangeFilter = report.FilterCollection["TextRangeFieldUserControl"] as TextRangeField;
			AssertEquals("From", textRangeFilter.From);
			AssertEquals("To", textRangeFilter.To);

			var textFilter1 = report.FilterCollection["TextFieldUserControl"] as TextField;
			AssertEquals("TextValue1", textFilter1.Value);
			var textFilter2 = report.FilterCollection["StillTextFieldUserControl"] as ExactTextField;
			AssertEquals("TextValue2", textFilter2.Value);

			var numberFilter = report.FilterCollection["NumberUserControl"] as NumberField;
			AssertEquals(2.2m, numberFilter.ZValue);

			var numberRangeFilter = report.FilterCollection["NumberRangeUserControl"] as NumberRangeField;
			AssertEquals(1.1m, numberRangeFilter.From);
			AssertEquals(2.2m, numberRangeFilter.To);

			var numberNotInRangeFilter = report.FilterCollection["NumberNotInRangeUserControl"] as NumberNotInRangeField;
			AssertEquals(3.3m, numberNotInRangeFilter.From);
			AssertEquals(4.4m, numberNotInRangeFilter.To);

			var dateFilter = report.FilterCollection["DateFieldUserControl"] as DateField;
			AssertEquals(reportData.FilterData.DateFilterCollection.First(f => f.DisplayName == "DateFieldUserControl").Value, dateFilter.Value);
			AssertEquals(DocEngineDatePickerFormats.Short, dateFilter.PickerFormat);

			var dateFilterEmpty = report.FilterCollection["DateFieldUserControlEmpty"] as DateField;
			AssertNoErrors(dateFilterEmpty.ValueInfo);

			var dateRange = report.FilterCollection["DateRangeFieldUserControl"] as DateRangeField;
			AssertEquals(reportData.FilterData.DateRangeFilterCollection.First(f => f.DisplayName == "DateRangeFieldUserControl").ValueLow, dateRange.ValueLow);
			AssertEquals(reportData.FilterData.DateRangeFilterCollection.First(f => f.DisplayName == "DateRangeFieldUserControl").ValueHigh, dateRange.ValueHigh);
			AssertEquals(DocEngineDatePickerFormats.Short, dateRange.PickerFormat);

			var security = report.FilterCollection["SecurityFilterControl"] as SecurityFilterField;
			AssertEquals("GLBCLASSROOMSESSIONPUBLICEXPORTTOEXCEL", security.FilterContainer.LookupKey.Code);

			var multipleSelection1 = report.FilterCollection["MultipleSelectionLookupUserControl (Org)"] as MultipleSelectionLookup;
			AssertEquals(1, multipleSelection1.BindToList.Count);
			AssertContainsExactElementsInAnyOrder(multipleSelection1.BindToList.ToArray().Select(bo => bo.PK), reportData.FilterData.MultipleSelectionLookupFilterCollection.First(f => f.DisplayName == "MultipleSelectionLookupUserControl (Org)").SelectedValue);

			var multipleSelection2 = report.FilterCollection["MultipleSelectionLookupUserControl (Invoice Currency)"] as MultipleSelectionLookup;
			AssertEquals(1, multipleSelection2.BindToList.Count);
			AssertContainsExactElementsInAnyOrder(multipleSelection2.BindToList.ToArray().Select(bo => bo.PK), reportData.FilterData.MultipleSelectionLookupFilterCollection.First(f => f.DisplayName == "MultipleSelectionLookupUserControl (Invoice Currency)").SelectedValue);

			var lookup = report.FilterCollection["LookupFieldUserControl"] as LookupField;
			AssertEquals(reportData.FilterData.LookupFilterCollection.First(f => f.DisplayName == "LookupFieldUserControl").Value, lookup.ZValue);

			var codeLookUp = report.FilterCollection["CodeLookupFieldUserControl (Staff Member)"] as CodeLookupField;
			AssertEquals(reportData.FilterData.CodeLookupFilterCollection.First(f => f.DisplayName == "CodeLookupFieldUserControl (Staff Member)").Value, codeLookUp.ZValue);

			var codeListMultipleChoice = report.FilterCollection["CodeListMultipleChoiceUserControl (Consolidation Category)"] as CodeListMultipleChoice;
			AssertEquals("MIN", codeListMultipleChoice.ZValue);

			var optionGroup = report.FilterCollection["OptionGroupUserControl"] as OptionGroup;
			var options = optionGroup.DescriptionCodePairList.Where(b => b.Value).Select(b => b.Description).ToList();
			AssertEquals(2, options.Count);
			Assert(options.Contains("Credit Controller"));
			Assert(options.Contains("Customer Service Representative"));

			var multipleChoice = report.FilterCollection["MultipleChoiceUserControl"] as MultipleChoice;
			AssertEquals("DBT", multipleChoice.ZValue);

			var singleAccountingPeriod = report.FilterCollection["SingleAccountingPeriodUserControl"] as SingleAccountingPeriodField;
			AssertEquals(5, singleAccountingPeriod.SinglePeriod);

			var accountingPeriodRange = report.FilterCollection["AccountingPeriodsRangeUserControl"] as AccountingPeriodsRangeField;
			AssertEquals(1, accountingPeriodRange.PeriodTo);
			AssertEquals(2, accountingPeriodRange.PeriodFrom);

			var accountingNumberRange = report.FilterCollection["AccountingNumberRangeUserControl"] as AccountingNumberRangeField;
			AssertEquals("1212", accountingNumberRange.From);
			AssertEquals("0202", accountingNumberRange.To);

			var accountingPeriod = report.FilterCollection["AccountingPeriodFieldUserControl"] as AccountingPeriodField;
			AssertEquals(5, accountingPeriod.FromPeriod);

			var salesTrade = report.FilterCollection["SalesTradeLaneChecklistUserControl"] as SalesTradeLaneChecklistField;
			AssertEquals(1, salesTrade.RootItemsCollection.Count);
			AssertEquals(1, salesTrade.RootItemsCollection[0].SubItemsCollection.Count);

			var registration = report.FilterCollection["RegistrationCodedUserControl"] as RegistrationCodeField;
			AssertEquals("Country", registration.CodeCountry);
			AssertEquals("Type", registration.CustomType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetSelectedValueReportData()
		{
			using var report = ReportCommand.GetReport();
			var expectedReportData = PrepareReportData(report);
			report.FillReportData(expectedReportData);

			var actualReportData = report.GetSelectedValueReportData();

			AssertGetSelectedValueReportData(expectedReportData, actualReportData);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetSelectedValueReportData_Scheduled()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			using var report = ReportCommand.GetReport();
			report.SetScheduleTask(scheduleTask);
			var expectedReportData = PrepareReportData(report);
			report.FillReportData(expectedReportData);

			var actualReportData = report.GetSelectedValueReportData();

			AssertGetSelectedValueReportData(expectedReportData, actualReportData, isSchedule: true);
		}

		void AssertGetSelectedValueReportData(SelectedValueReportData expectedReportData, SelectedValueReportData actualReportData, bool isSchedule = false)
		{
			Assert(actualReportData.GroupBy.BreakPage);
			AssertEquals("Alpha", actualReportData.GroupBy.GroupBy);

			AssertEquals("SecondSort", actualReportData.SortOrder);
			AssertEquals("Second Template", actualReportData.OptionalTemplates[0]);

			var textRangeFilter = actualReportData.FilterData.TextRangeFilterCollection.First(f => f.DisplayName == "TextRangeFieldUserControl");
			AssertEquals("From", textRangeFilter.From);
			AssertEquals("To", textRangeFilter.To);

			var textFilter1 = actualReportData.FilterData.TextFilterCollection.First(f => f.DisplayName == "TextFieldUserControl");
			AssertEquals("TextValue1", textFilter1.Value);
			var textFilter2 = actualReportData.FilterData.TextFilterCollection.First(f => f.DisplayName == "StillTextFieldUserControl");
			AssertEquals("TextValue2", textFilter2.Value);

			var numberFilter = actualReportData.FilterData.NumberFilterCollection.First(f => f.DisplayName == "NumberUserControl");
			AssertEquals(2.2m, numberFilter.Value);

			var numberRangeFilter = actualReportData.FilterData.NumberRangeFilterCollection.First(f => f.DisplayName == "NumberRangeUserControl");
			AssertEquals(1.1m, numberRangeFilter.From);
			AssertEquals(2.2m, numberRangeFilter.To);

			var numberNotInRangeFilter = actualReportData.FilterData.NumberNotInRangeFilterCollection.First(f => f.DisplayName == "NumberNotInRangeUserControl");
			AssertEquals(3.3m, numberNotInRangeFilter.From);
			AssertEquals(4.4m, numberNotInRangeFilter.To);

			var dateFilter = actualReportData.FilterData.DateFilterCollection.First(f => f.DisplayName == "DateFieldUserControl");
			AssertEquals(expectedReportData.FilterData.DateFilterCollection.First(f => f.DisplayName == "DateFieldUserControl").Value, dateFilter.Value);
			AssertEquals(nameof(DocEngineDatePickerFormats.Short), dateFilter.DateFormat);

			var dateFilterEmpty = actualReportData.FilterData.DateFilterCollection.First(f => f.DisplayName == "DateFieldUserControlEmpty");
			AssertEquals(DateTime.MinValue, dateFilterEmpty.Value);

			var dateRange = actualReportData.FilterData.DateRangeFilterCollection.First(f => f.DisplayName == "DateRangeFieldUserControl");
			AssertEquals(expectedReportData.FilterData.DateRangeFilterCollection.First(f => f.DisplayName == "DateRangeFieldUserControl").ValueLow, dateRange.ValueLow);
			AssertEquals(expectedReportData.FilterData.DateRangeFilterCollection.First(f => f.DisplayName == "DateRangeFieldUserControl").ValueHigh, dateRange.ValueHigh);
			AssertEquals(nameof(DocEngineDatePickerFormats.Short), dateRange.DateFormat);

			var security = actualReportData.FilterData.SecurityFilterCollection.First(f => f.DisplayName == "SecurityFilterControl");
			AssertEquals("GLBCLASSROOMSESSIONPUBLICEXPORTTOEXCEL", security.SelectedValue);

			var multipleSelection1 = actualReportData.FilterData.MultipleSelectionLookupFilterCollection.First(f => f.DisplayName == "MultipleSelectionLookupUserControl (Org)");
			AssertEquals(1, multipleSelection1.SelectedValue.Count);
			AssertArrayEqualsByElements(expectedReportData.FilterData.MultipleSelectionLookupFilterCollection.First(f => f.DisplayName == "MultipleSelectionLookupUserControl (Org)").SelectedValue.ToArray(), multipleSelection1.SelectedValue.ToArray());

			var multipleSelection2 = actualReportData.FilterData.MultipleSelectionLookupFilterCollection.First(f => f.DisplayName == "MultipleSelectionLookupUserControl (Invoice Currency)");
			AssertEquals(1, multipleSelection2.SelectedValue.Count);
			AssertArrayEqualsByElements(expectedReportData.FilterData.MultipleSelectionLookupFilterCollection.First(f => f.DisplayName == "MultipleSelectionLookupUserControl (Invoice Currency)").SelectedValue.ToArray(), multipleSelection2.SelectedValue.ToArray());

			var lookup = actualReportData.FilterData.LookupFilterCollection.First(f => f.DisplayName == "LookupFieldUserControl");
			AssertEquals(expectedReportData.FilterData.LookupFilterCollection.First(f => f.DisplayName == "LookupFieldUserControl").Value, lookup.Value);

			var codeLookUp = actualReportData.FilterData.CodeLookupFilterCollection.First(f => f.DisplayName == "CodeLookupFieldUserControl (Staff Member)");
			AssertEquals(expectedReportData.FilterData.CodeLookupFilterCollection.First(f => f.DisplayName == "CodeLookupFieldUserControl (Staff Member)").Value, codeLookUp.Value);

			var codeListMultipleChoice = actualReportData.FilterData.CodeListMultipleChoiceFilterCollection.First(f => f.DisplayName == "CodeListMultipleChoiceUserControl (Consolidation Category)");
			AssertEquals("MIN", codeListMultipleChoice.Value);

			var optionGroup = actualReportData.FilterData.OptionGroupFilterCollection.First(f => f.DisplayName == "OptionGroupUserControl");
			AssertEquals(3, optionGroup.Options.Count);
			AssertEquals(true, optionGroup.Options.First(o => o.Description == "Credit Controller").Value);
			AssertEquals(true, optionGroup.Options.First(o => o.Description == "Customer Service Representative").Value);
			AssertEquals(false, optionGroup.Options.First(o => o.Description == "Sales Representative").Value);

			var multipleChoice = actualReportData.FilterData.MultipleChoiceFilterCollection.First(f => f.DisplayName == "MultipleChoiceUserControl");
			AssertEquals("DBT", multipleChoice.Value);

			var singleAccountingPeriod = actualReportData.FilterData.SingleAccountingPeriodFilterCollection.First(f => f.DisplayName == "SingleAccountingPeriodUserControl");
			var accountingPeriodRange = actualReportData.FilterData.AccountingPeriodsRangeFilterCollection.First(f => f.DisplayName == "AccountingPeriodsRangeUserControl");
			var accountingPeriod = actualReportData.FilterData.AccountingPeriodFilterCollection.First(f => f.DisplayName == "AccountingPeriodFieldUserControl");
			if (isSchedule)
			{
				AssertEquals(new DateTime(1910, 1, 4), singleAccountingPeriod.ScheduleStorageValue);
				AssertEquals(new DateTime(1910, 2, 4), accountingPeriodRange.ScheduleStorageTo);
				AssertEquals(new DateTime(1909, 10, 4), accountingPeriodRange.ScheduleStorageFrom);
				AssertEquals(new DateTime(1909, 10, 4), accountingPeriod.ScheduleStorageFrom);
			}
			else
			{
				AssertEquals(5, singleAccountingPeriod.SinglePeriod);
				AssertEquals(1, accountingPeriodRange.PeriodTo);
				AssertEquals(2, accountingPeriodRange.PeriodFrom);
				AssertEquals(5, accountingPeriod.FromPeriod);
			}

			var accountingNumberRange = actualReportData.FilterData.AccountingNumberRangeFilterCollection.First(f => f.DisplayName == "AccountingNumberRangeUserControl");
			AssertEquals("1212", accountingNumberRange.From);
			AssertEquals("0202", accountingNumberRange.To);

			var salesTrade = actualReportData.FilterData.SalesTradeLaneChecklistFilterCollection.First(f => f.DisplayName == "SalesTradeLaneChecklistUserControl");
			AssertEquals(1, salesTrade.RootItems.Count);
			AssertEquals(1, salesTrade.RootItems[0].SubItems.Count);

			var registration = actualReportData.FilterData.RegistrationCodedFilterCollection.First(f => f.DisplayName == "RegistrationCodedUserControl");
			AssertEquals("Country", registration.CodeCountry);
			AssertEquals("Type", registration.CustomType);
		}

		SelectedValueReportData PrepareReportData(Report report)
		{
			var isSchedule = report.ScheduleTask != null;

			var reportData = new SelectedValueReportData();
			reportData.Id = ReportCommand.PK.ToGuid();
			reportData.GroupBy = new SelectedValueGroupByData { BreakPage = true, GroupBy = "Alpha" };
			reportData.SortOrder = "SecondSort";
			reportData.OptionalTemplates.Add("Second Template");
			reportData.FilterData.TextRangeFilterCollection.Add(new TextRangeFilter { DisplayName = "TextRangeFieldUserControl", From = "From", To = "To" });
			reportData.FilterData.TextFilterCollection.Add(new TextFilter { DisplayName = "TextFieldUserControl", Value = "TextValue1" });
			reportData.FilterData.TextFilterCollection.Add(new TextFilter { DisplayName = "StillTextFieldUserControl", Value = "TextValue2" });
			reportData.FilterData.NumberFilterCollection.Add(new NumberFilter { DisplayName = "NumberUserControl", Value = 2.2m });
			reportData.FilterData.NumberRangeFilterCollection.Add(new NumberRangeFilter { DisplayName = "NumberRangeUserControl", From = 1.1m, To = 2.2m });
			reportData.FilterData.NumberNotInRangeFilterCollection.Add(new NumberNotInRangeFilter { DisplayName = "NumberNotInRangeUserControl", From = 3.3m, To = 4.4m });
			var dateFieldDate = isSchedule ? new DateTime(1910, 1, 1, 0, 1, 0) : new DateTime(2019, 3, 15);
			reportData.FilterData.DateFilterCollection.Add(new DateFilter { DisplayName = "DateFieldUserControl", Value = dateFieldDate });
			reportData.FilterData.DateFilterCollection.Add(new DateFilter { DisplayName = "DateFieldUserControlEmpty", Value = new DateTime() });

			var dateRangeLow = isSchedule ? new DateTime(1909, 11, 1, 0, 7, 0) : new DateTime(2019, 3, 15);
			var dateRangeHigh = isSchedule ? new DateTime(1910, 1, 1, 0, 1, 0) : new DateTime(2019, 3, 20);
			reportData.FilterData.DateRangeFilterCollection.Add(new DateRangeFilter { DisplayName = "DateRangeFieldUserControl", ValueLow = dateRangeLow, ValueHigh = dateRangeHigh });
			reportData.FilterData.SecurityFilterCollection.Add(new SecurityFilter { DisplayName = "SecurityFilterControl", SelectedValue = "GLBCLASSROOMSESSIONPUBLICEXPORTTOEXCEL" });
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			var multi1 = new MultipleSelectionLookupFilter { DisplayName = "MultipleSelectionLookupUserControl (Org)" };
			multi1.SelectedValue.Add(org.PK.ToGuid());
			var multi2 = new MultipleSelectionLookupFilter { DisplayName = "MultipleSelectionLookupUserControl (Invoice Currency)" };
			multi2.SelectedValue.Add(currency.PK.ToGuid());
			reportData.FilterData.MultipleSelectionLookupFilterCollection.Add(multi1);
			reportData.FilterData.MultipleSelectionLookupFilterCollection.Add(multi2);
			reportData.FilterData.LookupFilterCollection.Add(new LookupFilter { DisplayName = "LookupFieldUserControl", Value = org.PK.ToGuid() });
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
			reportData.FilterData.CodeLookupFilterCollection.Add(new CodeLookupFilter { DisplayName = "CodeLookupFieldUserControl (Staff Member)", Value = staff.GS_Code });
			reportData.FilterData.CodeListMultipleChoiceFilterCollection.Add(new CodeListMultipleChoiceFilter { DisplayName = "CodeListMultipleChoiceUserControl (Consolidation Category)", Value = "MIN" });
			var option = new OptionGroupFilter { DisplayName = "OptionGroupUserControl" };
			option.Options.AddRange(new[]
			{
					new BoolDescription { Description = "Credit Controller", Value = true },
					new BoolDescription { Description = "Customer Service Representative", Value = true },
					new BoolDescription { Description = "Sales Representative", Value = false }
				});
			reportData.FilterData.OptionGroupFilterCollection.Add(option);
			reportData.FilterData.MultipleChoiceFilterCollection.Add(new MultipleChoiceFilter { DisplayName = "MultipleChoiceUserControl", Value = "DBT" });
			reportData.FilterData.SingleAccountingPeriodFilterCollection.Add(new SingleAccountingPeriodFilter { DisplayName = "SingleAccountingPeriodUserControl", SinglePeriod = 5, ScheduleStorageValue = new DateTime(1910, 1, 4) });
			reportData.FilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter { DisplayName = "AccountingPeriodsRangeUserControl", PeriodTo = 1, PeriodFrom = 2, ScheduleStorageTo = new DateTime(1910, 2, 4), ScheduleStorageFrom = new DateTime(1909, 10, 4) });
			reportData.FilterData.AccountingNumberRangeFilterCollection.Add(new AccountingNumberRangeFilter { DisplayName = "AccountingNumberRangeUserControl", From = "1212", To = "0202" });
			reportData.FilterData.AccountingPeriodFilterCollection.Add(new AccountingPeriodFilter { DisplayName = "AccountingPeriodFieldUserControl", FromPeriod = 5, ScheduleStorageFrom = new DateTime(1909, 10, 4), UsePeriodRange = true, });
			var salesTradeLaneChecklistFilter = new SalesTradeLaneChecklistFilter { DisplayName = "SalesTradeLaneChecklistUserControl" };
			SalesTradeLaneChecklistField.CopySalesTradeLaneChecklist(salesTradeLaneChecklistFilter.RootItems, ((SalesTradeLaneChecklistField)report.FilterCollection["SalesTradeLaneChecklistUserControl"]).RootItemsCollection);
			salesTradeLaneChecklistFilter.RootItems[0].Include = true;
			salesTradeLaneChecklistFilter.RootItems[0].SubItems[0].Include = true;
			reportData.FilterData.SalesTradeLaneChecklistFilterCollection.Add(salesTradeLaneChecklistFilter);
			reportData.FilterData.RegistrationCodedFilterCollection.Add(new RegistrationCodedFilter { DisplayName = "RegistrationCodedUserControl", CodeCountry = "Country", CustomType = "Type" });

			return reportData;
		}

		ReportCommand ReportCommand
		{
			get
			{
				if (reportCommand == null)
				{
					var excelTemplate = new ExcelTemplateForUnitTesting("AllReportElementsContained.xlsx", TestFilesSubFolder.ReportTestFiles);
					var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportData", excelTemplate.GetAsByteArray(), "GenericFreightJob");
					reportCommand = Factory.New<ReportCommand>();
					reportCommand.SU_MenuName = "Test Get Report Data";
					var pivot = reportCommand.Documents.AddNew();
					pivot.SI_SU = reportCommand.PK;
					pivot.SI_SO = template.PK;
					Factory.Save();
				}

				return reportCommand;
			}
		}
		ReportCommand reportCommand;

		ReportCommand ReportCommandForLookUpFilter
		{
			get
			{
				if (reportCommandForLookUpFilter == null)
				{
					var excelTemplate = new ExcelTemplateForUnitTesting("LookupFieldReportForWeb.xlsx", TestFilesSubFolder.ReportTestFiles);
					var template = TemplateTestHelper.CreateTemplate(Factory, "TestGetReportDataForLookUpFilter", excelTemplate.GetAsByteArray(), "GenericFreightJob");
					reportCommandForLookUpFilter = Factory.New<ReportCommand>();
					reportCommandForLookUpFilter.SU_MenuName = "Test Get Report Data For LookUp Filter";
					var pivot = reportCommandForLookUpFilter.Documents.AddNew();
					pivot.SI_SU = reportCommandForLookUpFilter.PK;
					pivot.SI_SO = template.PK;
					Factory.Save();
				}

				return reportCommandForLookUpFilter;
			}
		}
		ReportCommand reportCommandForLookUpFilter;
	}
}
