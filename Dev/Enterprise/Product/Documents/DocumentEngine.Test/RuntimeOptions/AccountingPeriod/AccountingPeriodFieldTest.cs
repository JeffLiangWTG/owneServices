using System;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(AccountingPeriodField))]
	class AccountingPeriodFieldTest : FitlerFieldTestWithClearValues
	{
		AccountingPeriodField apf;
		AccountingPeriodTestHelper PeriodTestHelper;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			PeriodTestHelper = new AccountingPeriodTestHelper();
			PeriodTestHelper.PostPeriodsForEntireYear(2003);
			PeriodTestHelper.PostPeriodsForEntireYear(2002);
			PeriodTestHelper.PostPeriodsForEntireYear(2001);
			PeriodTestHelper.PostPeriodsForEntireYear(2000);

			apf = GetNewFieldWithTestData();
		}

		AccountingPeriodField GetNewFieldWithTestData()
		{
			AccountingPeriodField result = new AccountingPeriodField(new BusinessObjectFactory());
			result.FieldName = "f";
			result.SinglePeriod = 200301;
			result.FromPeriod = 200202;
			result.ToPeriod = 200206;
			result.YearToPeriod = 200010;
			return result;
		}

		public void TestJsonConverter_Standard()
		{
			apf.DisplayName = "Json Test";
			apf.UsePeriodRange = true;
			apf.UseSinglePeriod = true;
			apf.UseAllPeriods = true;
			apf.UseYearToPeriod = true;

			var result = JsonConverterHelper.Serialize(apf);
			var newapf = JsonConverterHelper.Deserialize<AccountingPeriodField>(result);

			AssertEquals("Json Test", newapf.DisplayName);
			AssertEquals("f", newapf.FieldName);
			AssertEquals(ZBool.True, newapf.UsePeriodRange);
			AssertEquals(ZBool.True, newapf.UseSinglePeriod);
			AssertEquals(ZBool.True, newapf.UseAllPeriods);
			AssertEquals(ZBool.True, newapf.UseYearToPeriod);
			AssertEquals((ZInt)200301, newapf.SinglePeriod);
			AssertEquals((ZInt)200202, newapf.FromPeriod);
			AssertEquals((ZInt)200206, newapf.ToPeriod);
			AssertEquals((ZInt)200010, newapf.YearToPeriod);
		}

		public void TestJsonConverter_Schedule()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 1);
			apf.SetScheduleTask(scheduleTask);

			apf.DisplayName = "Apple";
			apf.FieldName = "Pie";

			apf.SinglePeriodSchedule.PeriodScope = PeriodScopeList.Codes.This;

			apf.FromPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			apf.FromPeriodSchedule.PeriodCount = 1;

			apf.ToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			apf.ToPeriodSchedule.PeriodCount = 2;

			apf.YearToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			apf.YearToPeriodSchedule.PeriodCount = 3;

			var result = JsonConverterHelper.Serialize(apf);
			var deserialisedField = JsonConverterHelper.Deserialize<AccountingPeriodField>(result);
			deserialisedField.SetScheduleTask(scheduleTask);

			AssertEquals("DisplayName", "Apple", deserialisedField.DisplayName);
			AssertEquals("FieldName", "Pie", deserialisedField.FieldName);

			AssertEquals("SinglePeriod", 200310, deserialisedField.SinglePeriod);
			AssertEquals("FromPeriod", 200309, deserialisedField.FromPeriod);
			AssertEquals("ToPeriod", 200312, deserialisedField.ToPeriod);
			AssertEquals("YearToPeriod", 200307, deserialisedField.YearToPeriod);

			deserialisedField.SinglePeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.SinglePeriodSchedule.PeriodCount = 2;

			deserialisedField.FromPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.FromPeriodSchedule.PeriodCount = 3;

			deserialisedField.ToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			deserialisedField.ToPeriodSchedule.PeriodCount = 1;

			deserialisedField.YearToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.YearToPeriodSchedule.PeriodCount = 4;

			AssertEquals("SinglePeriod", 200308, deserialisedField.SinglePeriod);
			AssertEquals("FromPeriod", 200307, deserialisedField.FromPeriod);
			AssertEquals("ToPeriod", 200311, deserialisedField.ToPeriod);
			AssertEquals("YearToPeriod", 200306, deserialisedField.YearToPeriod);
		}

		public virtual void TestSinglePeriodValidation()
		{
			Assert("Should be no errors on single period field", !apf.SinglePeriodInfo.HasErrors());
			apf.UseSinglePeriod = true;
			Assert("Should not be errors on Single Period Field before user sets field", !apf.SinglePeriodInfo.HasErrors());
			apf.SinglePeriod = 9999;
			Assert("Should be error on single period field", apf.SinglePeriodInfo.HasErrors());
			var expectedError = AccountingPeriodCalculator.GetInvalidPeriodValidationError(9999);
			AssertEquals("Error on Single Period Field", apf.SinglePeriodInfo.GetErrors().GetFirstMessage(), expectedError);

			apf.UseSinglePeriod = false;
			apf.UsePeriodRange = true;
			Assert("Single Period Field should now be readonly", apf.SinglePeriodInfo.ReadOnly);
			Assert("Single period should still have errors", apf.SinglePeriodInfo.HasErrors());
			AssertEquals("Single period value should still be 9999", 9999, apf.SinglePeriod);
		}

		public void TestPeriodRangeValidation()
		{
			Assert("Should be no errors on from period field", !apf.FromPeriodInfo.HasErrors());
			Assert("Should be no errors on to period field", !apf.ToPeriodInfo.HasErrors());
			apf.UsePeriodRange = true;
			Assert("Should be no errors on from period field", !apf.FromPeriodInfo.HasErrors());
			Assert("Should be no errors on to period field", !apf.ToPeriodInfo.HasErrors());
			apf.FromPeriod = 9999;
			Assert("Should be error on from period field", apf.FromPeriodInfo.HasErrors());
			Assert("Should not be error on to period field", !apf.ToPeriodInfo.HasErrors());
			var expectedError = AccountingPeriodCalculator.GetInvalidPeriodValidationError(9999);
			AssertEquals("Error on From Period Field", apf.FromPeriodInfo.GetErrors().GetFirstMessage(), expectedError);

			apf.UsePeriodRange = false;
			apf.UseSinglePeriod = true;
			Assert("From Period Field should now be readonly", apf.FromPeriodInfo.ReadOnly);
			Assert("To Period Field should now be readonly", apf.ToPeriodInfo.ReadOnly);
			Assert("From period should still have errors", apf.FromPeriodInfo.HasErrors());
			Assert("To period should not have errors", !apf.ToPeriodInfo.HasErrors());
			AssertEquals("From period value should be 9999", 9999, apf.FromPeriod);
			AssertEquals("To period value should be 200206", 200206, apf.ToPeriod);

			apf.UseSinglePeriod = false;
			apf.UsePeriodRange = true;
			apf.ToPeriod = 9999;
			Assert("Should still be error on from period field", apf.FromPeriodInfo.HasErrors());
			Assert("Should be error on to period field", apf.ToPeriodInfo.HasErrors());
			AssertEquals("Error on From Period Field", apf.ToPeriodInfo.GetErrors().GetFirstMessage(), expectedError);
		}

		public void TestYearToPeriodValidation()
		{
			Assert("Should be no errors on year to period field", !apf.YearToPeriodInfo.HasErrors());
			apf.UseYearToPeriod = true;
			Assert("Should not be errors on year to Period Field before user sets field", !apf.YearToPeriodInfo.HasErrors());
			apf.YearToPeriod = 9999;
			Assert("Should be error on year to period field", apf.YearToPeriodInfo.HasErrors());
			var expectedError = AccountingPeriodCalculator.GetInvalidPeriodValidationError(9999);
			AssertEquals("Error on year to Period Field", apf.YearToPeriodInfo.GetErrors().GetFirstMessage(), expectedError);

			apf.UseYearToPeriod = false;
			apf.UseSinglePeriod = true;
			Assert("Year to Period Field should now be readonly", apf.YearToPeriodInfo.ReadOnly);
			Assert("Year to period should still have errors", apf.YearToPeriodInfo.HasErrors());
			AssertEquals("Year to period value should be 9999", 9999, apf.YearToPeriod);
		}

		public virtual void TestSinglePeriod()
		{
			apf.UseSinglePeriod = true;
			Match whereClauseMatch = Regex.Match(apf.WhereClause(), @"f = (@p[0-9]+)");
			Assert("Where clause should be f = @p1 but was: " + apf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, apf.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", 200301, apf.SqlParameters()[0].Value);
		}

		public virtual void TestPeriodRange()
		{
			apf.UsePeriodRange = true;
			Match whereClauseMatch = Regex.Match(apf.WhereClause(), @"f >= (@p[0-9]+) AND f <= (@p[0-9]+)");
			Assert("Where clause should be f >= @p1 AND f <= @p2 but was: " + apf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, apf.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", 200202, apf.SqlParameters()[0].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, apf.SqlParameters()[1].ToString());
			AssertEquals("Param 2 value", 200206, apf.SqlParameters()[1].Value);
		}

		public virtual void TestYearToPeriod()
		{
			apf.UseYearToPeriod = true;
			Match whereClauseMatch = Regex.Match(apf.WhereClause(), @"f >= (@p[0-9]+) AND f <= (@p[0-9]+)");
			Assert("Where clause should be f >= @p1 AND f <= @p2 but was: " + apf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, apf.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", 200001, apf.SqlParameters()[0].Value);
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, apf.SqlParameters()[1].ToString());
			AssertEquals("Param 2 value", 200010, apf.SqlParameters()[1].Value);
		}

		public void TestAllPeriods()
		{
			apf.UseAllPeriods = true;
			AssertEquals("Should not be a where clause for all periods", "", apf.WhereClause());
		}

		public void TestDefaultsToAllPeriods()
		{
			Assert("Should default to All Periods", new AccountingPeriodField(new BusinessObjectFactory()).UseAllPeriods);
		}

		public void TestSingleValueForSinglePeriod()
		{
			apf.UseSinglePeriod = true;
			AssertEquals("ValueAsObject", "200301", apf.ValueAsObject);
		}

		public void TestSingleValueForPeriodRange()
		{
			apf.UsePeriodRange = true;
			AssertEquals("ValueAsObject", "200202 to 200206", apf.ValueAsObject);
		}

		public void TestSingleValueForYearToPeriod()
		{
			apf.UseYearToPeriod = true;
			AssertEquals("ValueAsObject", "Year To 200010", apf.ValueAsObject);
		}

		public void TestSingleValueForAllPeriods()
		{
			apf.UseAllPeriods = true;
			AssertEquals("ValueAsObject", "All Periods", apf.ValueAsObject);
		}

		public void TestSingleValueForZTypePeriods()
		{
			ZString zPeriod = "200301";
			apf.UseSinglePeriod = true;
			AssertEquals("ValueAsObject", "200301", ((IZTypeInternals)zPeriod).GetValueForLogicalDataLayer(true));
		}

		public void TestValueAsObjectTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("53794711-a4c4-4620-b6c4-be4965c7a177", new ResourceStringData("53794711-a4c4-4620-b6c4-be4965c7a177", "{0} 到 {1}"));
				mockChs.Put("3b6bb622-4055-4435-b4bf-448bff1d0c90", new ResourceStringData("3b6bb622-4055-4435-b4bf-448bff1d0c90", "直到年度 {0}"));
				mockChs.Put("1d7d3a31-2026-4833-aeb7-8e1febc04413", new ResourceStringData("1d7d3a31-2026-4833-aeb7-8e1febc04413", "全区间"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					apf.UsePeriodRange = true;
					AssertEquals("200202 到 200206", apf.ValueAsObject);

					apf.UsePeriodRange = false;
					apf.UseYearToPeriod = true;
					AssertEquals("ValueAsObject", "直到年度 200010", apf.ValueAsObject);

					apf.UsePeriodRange = false;
					apf.UseYearToPeriod = false;
					apf.UseAllPeriods = true;
					AssertEquals("全区间", apf.ValueAsObject);
				}
			}
		}

		public void TestIsEmpty_UseSinglePeriod()
		{
			AccountingPeriodField field = new AccountingPeriodField(Factory);

			field.UseSinglePeriod = true;
			AssertEquals("IsEmpty", true, field.IsEmpty);

			field.SinglePeriod = 1;
			AssertEquals("IsEmpty", false, field.IsEmpty);

			field.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			field.SinglePeriodSchedule.PeriodScope = string.Empty;
			AssertEquals("IsEmpty", true, field.IsEmpty);

			field.SinglePeriodSchedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", false, field.IsEmpty);
		}

		public void TestIsEmpty_UsePeriodRange()
		{
			AccountingPeriodField field = new AccountingPeriodField(Factory);

			field.UsePeriodRange = true;
			AssertEquals("IsEmpty", true, field.IsEmpty);

			field.FromPeriod = 1;
			AssertEquals("IsEmpty", false, field.IsEmpty);

			field.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			field.FromPeriodSchedule.PeriodScope = string.Empty;
			field.ToPeriodSchedule.PeriodScope = string.Empty;
			AssertEquals("IsEmpty", true, field.IsEmpty);

			field.ToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", false, field.IsEmpty);

			field.ToPeriodSchedule.PeriodScope = string.Empty;
			AssertEquals("IsEmpty", true, field.IsEmpty);

			field.FromPeriodSchedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", false, field.IsEmpty);

			field.ToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", false, field.IsEmpty);
		}

		public void TestIsEmpty_UseYearToPeriod()
		{
			AccountingPeriodField field = new AccountingPeriodField(Factory);

			field.UseYearToPeriod = true;
			AssertEquals("IsEmpty", true, field.IsEmpty);

			field.YearToPeriod = 1;
			AssertEquals("IsEmpty", false, field.IsEmpty);

			field.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			field.YearToPeriodSchedule.PeriodScope = string.Empty;
			AssertEquals("IsEmpty", true, field.IsEmpty);

			field.YearToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", false, field.IsEmpty);
		}

		public void TestSetScheduleTask()
		{
			AssertNull("SinglePeriodSchedule", apf.SinglePeriodSchedule);
			AssertNull("FromPeriodSchedule", apf.FromPeriodSchedule);
			AssertNull("ToPeriodSchedule", apf.ToPeriodSchedule);
			AssertNull("YearToPeriodSchedule", apf.YearToPeriodSchedule);

			ReportScheduleTask scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			apf.SetScheduleTask(scheduleTask1);
			AssertEquals("SinglePeriodSchedule.ScheduleTask", scheduleTask1, apf.SinglePeriodSchedule.ScheduleTask);
			AssertEquals("FromPeriodSchedule.ScheduleTask", scheduleTask1, apf.FromPeriodSchedule.ScheduleTask);
			AssertEquals("ToPeriodSchedule.ScheduleTask", scheduleTask1, apf.ToPeriodSchedule.ScheduleTask);
			AssertEquals("YearToPeriodSchedule.ScheduleTask", scheduleTask1, apf.YearToPeriodSchedule.ScheduleTask);

			apf.SinglePeriodSchedule.PeriodScope = PeriodScopeList.Codes.This;

			apf.FromPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			apf.FromPeriodSchedule.PeriodCount = 1;

			apf.ToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			apf.ToPeriodSchedule.PeriodCount = 2;

			apf.YearToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			apf.YearToPeriodSchedule.PeriodCount = 3;

			AssertEquals("SinglePeriod", 0, apf.SinglePeriod);
			AssertEquals("FromPeriod", 0, apf.FromPeriod);
			AssertEquals("ToPeriod", 0, apf.ToPeriod);
			AssertEquals("YearToPeriod", 0, apf.YearToPeriod);

			ReportScheduleTask scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 20);
			apf.SetScheduleTask(scheduleTask2);

			AssertEquals("SinglePeriodSchedule.ScheduleTask", scheduleTask2, apf.SinglePeriodSchedule.ScheduleTask);
			AssertEquals("FromPeriodSchedule.ScheduleTask", scheduleTask2, apf.FromPeriodSchedule.ScheduleTask);
			AssertEquals("ToPeriodSchedule.ScheduleTask", scheduleTask2, apf.ToPeriodSchedule.ScheduleTask);
			AssertEquals("YearToPeriodSchedule.ScheduleTask", scheduleTask2, apf.YearToPeriodSchedule.ScheduleTask);

			AssertEquals("SinglePeriod", 200310, apf.SinglePeriod);
			AssertEquals("FromPeriod", 200309, apf.FromPeriod);
			AssertEquals("ToPeriod", 200312, apf.ToPeriod);
			AssertEquals("YearToPeriod", 200307, apf.YearToPeriod);

			AssertEquals("SinglePeriodSchedule.GetSchedulePeriod()", 200310, apf.SinglePeriodSchedule.GetSchedulePeriod());
			AssertEquals("FromPeriodSchedule.GetSchedulePeriod()", 200309, apf.FromPeriodSchedule.GetSchedulePeriod());
			AssertEquals("ToPeriodSchedule.GetSchedulePeriod()", 200312, apf.ToPeriodSchedule.GetSchedulePeriod());
			AssertEquals("YearToPeriodSchedule.GetSchedulePeriod()", 200307, apf.YearToPeriodSchedule.GetSchedulePeriod());
		}

		public void TestValidatePeriod()
		{
			apf.UseSinglePeriod = true;
			apf.SinglePeriod = 2;
			AssertHasError(apf.SinglePeriodInfo, AccountingPeriodCalculator.GetInvalidPeriodValidationError(2));

			apf.SinglePeriod = 200204;
			AssertNoErrors(apf.SinglePeriodInfo);

			apf.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			apf.SinglePeriod = 2;
			AssertNoErrors(apf.SinglePeriodInfo);
		}

		public void TestPropertyInfoReadOnlyWhenScheduled()
		{
			apf.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());

			apf.UseSinglePeriod = true;
			AssertEquals("SinglePeriodInfo.ReadOnly", true, apf.SinglePeriodInfo.ReadOnly);

			apf.UsePeriodRange = true;
			AssertEquals("FromPeriodInfo.ReadOnly", true, apf.FromPeriodInfo.ReadOnly);
			AssertEquals("ToPeriodInfo.ReadOnly", true, apf.ToPeriodInfo.ReadOnly);

			apf.UseYearToPeriod = true;
			AssertEquals("YearToPeriodInfo.ReadOnly", true, apf.YearToPeriodInfo.ReadOnly);
		}

		public void TestSuggestedUserControlType()
		{
			apf.UseSinglePeriod = true;
			AssertEquals("SuggestedUserControlType", FilterFieldSuggestedUserControlType.AccountingPeriodFieldUserControl, apf.SuggestedUserControlType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1136:DoNotUseSystemRuntimeSerializationFormattersBinary", Justification = "WI00700503 - Pending migration")]
		public void TestRunAndScheduleSerialisation()
		{
			var runSinglePeriod = 200201;
			var runFromPeriod = 200202;
			var runToPeriod = 200210;
			var runYearToPeriod = 200301;

			var sourceField = new AccountingPeriodField(Factory);

			sourceField.UseSinglePeriod = true;
			sourceField.UsePeriodRange = true;
			sourceField.UseYearToPeriod = true;

			sourceField.SinglePeriod = runSinglePeriod;
			sourceField.FromPeriod = runFromPeriod;
			sourceField.ToPeriod = runToPeriod;
			sourceField.YearToPeriod = runYearToPeriod;

			var result = JsonConverterHelper.Serialize(sourceField);
			var deserialisedField = JsonConverterHelper.Deserialize<AccountingPeriodField>(result);

			AssertEquals("Run SinglePeriod", runSinglePeriod, deserialisedField.SinglePeriod);
			AssertEquals("Run FromPeriod", runFromPeriod, deserialisedField.FromPeriod);
			AssertEquals("Run ToPeriod", runToPeriod, deserialisedField.ToPeriod);
			AssertEquals("Run YearToPeriod", runYearToPeriod, deserialisedField.YearToPeriod);

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			deserialisedField.SetScheduleTask(scheduleTask);

			deserialisedField.SinglePeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.SinglePeriodSchedule.PeriodCount = 5;

			deserialisedField.FromPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.FromPeriodSchedule.PeriodCount = 3;

			deserialisedField.ToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			deserialisedField.ToPeriodSchedule.PeriodCount = 5;

			deserialisedField.YearToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			deserialisedField.YearToPeriodSchedule.PeriodCount = 7;

			result = JsonConverterHelper.Serialize(deserialisedField);
			var deserialisedField2 = JsonConverterHelper.Deserialize<AccountingPeriodField>(result);

			AssertEquals("Run SinglePeriod", runSinglePeriod, deserialisedField2.SinglePeriod);
			AssertEquals("Run FromPeriod", runFromPeriod, deserialisedField2.FromPeriod);
			AssertEquals("Run ToPeriod", runToPeriod, deserialisedField2.ToPeriod);
			AssertEquals("Run YearToPeriod", runYearToPeriod, deserialisedField2.YearToPeriod);

			deserialisedField2.SetScheduleTask(scheduleTask);

			AssertEquals("SinglePeriodSchedule.PeriodScope", deserialisedField.SinglePeriodSchedule.PeriodScope, deserialisedField2.SinglePeriodSchedule.PeriodScope);
			AssertEquals("SinglePeriodSchedule.PeriodCount", deserialisedField.SinglePeriodSchedule.PeriodCount, deserialisedField2.SinglePeriodSchedule.PeriodCount);
			AssertEquals("SinglePeriod", deserialisedField.SinglePeriod, deserialisedField2.SinglePeriod);

			AssertEquals("FromPeriodSchedule.PeriodScope", deserialisedField.FromPeriodSchedule.PeriodScope, deserialisedField2.FromPeriodSchedule.PeriodScope);
			AssertEquals("FromPeriodSchedule.PeriodCount", deserialisedField.FromPeriodSchedule.PeriodCount, deserialisedField2.FromPeriodSchedule.PeriodCount);
			AssertEquals("FromPeriod", deserialisedField.FromPeriod, deserialisedField2.FromPeriod);

			AssertEquals("ToPeriodSchedule.PeriodScope", deserialisedField.ToPeriodSchedule.PeriodScope, deserialisedField2.ToPeriodSchedule.PeriodScope);
			AssertEquals("ToPeriodSchedule.PeriodCount", deserialisedField.ToPeriodSchedule.PeriodCount, deserialisedField2.ToPeriodSchedule.PeriodCount);
			AssertEquals("ToPeriod", deserialisedField.ToPeriod, deserialisedField2.ToPeriod);

			AssertEquals("YearToPeriodSchedule.PeriodScope", deserialisedField.YearToPeriodSchedule.PeriodScope, deserialisedField2.YearToPeriodSchedule.PeriodScope);
			AssertEquals("YearToPeriodSchedule.PeriodCount", deserialisedField.YearToPeriodSchedule.PeriodCount, deserialisedField2.YearToPeriodSchedule.PeriodCount);
			AssertEquals("YearToPeriod", deserialisedField.YearToPeriod, deserialisedField2.YearToPeriod);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var runSinglePeriod = 200307;
			var runFromPeriod = 200308;
			var runToPeriod = 200309;
			var runYearToPeriod = 200310;

			var sourceField = new AccountingPeriodField(Factory);
			sourceField.UseSinglePeriod = true;
			sourceField.UsePeriodRange = true;
			sourceField.UseYearToPeriod = true;
			sourceField.UseAllPeriods = true;

			sourceField.SinglePeriod = runSinglePeriod;
			sourceField.FromPeriod = runFromPeriod;
			sourceField.ToPeriod = runToPeriod;
			sourceField.YearToPeriod = runYearToPeriod;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			sourceField.SetScheduleTask(scheduleTask);

			sourceField.SinglePeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.SinglePeriodSchedule.PeriodCount = 2;

			sourceField.FromPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.FromPeriodSchedule.PeriodCount = 3;

			sourceField.ToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			sourceField.ToPeriodSchedule.PeriodCount = 5;

			sourceField.YearToPeriodSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			sourceField.YearToPeriodSchedule.PeriodCount = 7;

			var destinationField = new AccountingPeriodField(Factory);
			destinationField.SafeCopyValuesFrom(sourceField);
			AssertEquals("Run SinglePeriod", runSinglePeriod, destinationField.SinglePeriod);
			AssertEquals("Run FromPeriod", runFromPeriod, destinationField.FromPeriod);
			AssertEquals("Run ToPeriod", runToPeriod, destinationField.ToPeriod);
			AssertEquals("Run YearToPeriod", runYearToPeriod, destinationField.YearToPeriod);
			AssertEquals("Use All Periods", true, destinationField.UseAllPeriods);

			destinationField.SetScheduleTask(scheduleTask);
			AssertEquals("SinglePeriodSchedule.PeriodScope", sourceField.SinglePeriodSchedule.PeriodScope, destinationField.SinglePeriodSchedule.PeriodScope);
			AssertEquals("SinglePeriodSchedule.PeriodCount", sourceField.SinglePeriodSchedule.PeriodCount, destinationField.SinglePeriodSchedule.PeriodCount);
			AssertEquals("SinglePeriod", sourceField.SinglePeriod, destinationField.FromPeriod);

			AssertEquals("FromPeriodSchedule.PeriodScope", sourceField.FromPeriodSchedule.PeriodScope, destinationField.FromPeriodSchedule.PeriodScope);
			AssertEquals("FromPeriodSchedule.PeriodCount", sourceField.FromPeriodSchedule.PeriodCount, destinationField.FromPeriodSchedule.PeriodCount);
			AssertEquals("FromPeriod", sourceField.FromPeriod, destinationField.FromPeriod);

			AssertEquals("ToPeriodSchedule.PeriodScope", sourceField.ToPeriodSchedule.PeriodScope, destinationField.ToPeriodSchedule.PeriodScope);
			AssertEquals("ToPeriodSchedule.PeriodCount", sourceField.ToPeriodSchedule.PeriodCount, destinationField.ToPeriodSchedule.PeriodCount);
			AssertEquals("ToPeriod", sourceField.ToPeriod, destinationField.ToPeriod);

			AssertEquals("YearToPeriodSchedule.PeriodScope", sourceField.YearToPeriodSchedule.PeriodScope, destinationField.YearToPeriodSchedule.PeriodScope);
			AssertEquals("YearToPeriodSchedule.PeriodCount", sourceField.YearToPeriodSchedule.PeriodCount, destinationField.YearToPeriodSchedule.PeriodCount);
			AssertEquals("YearToPeriod", sourceField.ToPeriod, destinationField.YearToPeriod);
		}

		public override void TestClearValues()
		{
			AccountingPeriodField field = new AccountingPeriodField(Factory);
			field.SinglePeriod = field.FromPeriod = field.ToPeriod = field.YearToPeriod = 200307;
			field.ClearValues();
			AssertEquals(ZInt.Zero, field.SinglePeriod);
			AssertEquals(ZInt.Zero, field.FromPeriod);
			AssertEquals(ZInt.Zero, field.ToPeriod);
			AssertEquals(ZInt.Zero, field.YearToPeriod);
		}

		public void TestSetFilterValue()
		{
			var reportFilterData = new ReportFilterData();
			reportFilterData.AccountingPeriodFilterCollection.Add(new AccountingPeriodFilter
			{
				UseSinglePeriod = true,
				UseYearToPeriod = true,
				UsePeriodRange = true,
				UseAllPeriods = true,
				SinglePeriod = 200302,
				YearToPeriod = 200303,
				FromPeriod = 200304,
				ToPeriod = 200305,
				DisplayName = "Period"
			});

			apf.DisplayName = "Period";

			apf.SetFilterValue(reportFilterData);
			Assert("Run value - UseSinglePeriod:", apf.UseSinglePeriod);
			Assert("Run value - UseYearToPeriod:", apf.UseYearToPeriod);
			Assert("Run value - UsePeriodRange:", apf.UsePeriodRange);
			Assert("Run value - UseAllPeriods:", apf.UseAllPeriods);

			AssertEquals("Run value - SinglePeriod: ", 200302, apf.SinglePeriod);
			AssertEquals("Run value - YearToPeriod: ", 200303, apf.YearToPeriod);
			AssertEquals("Run value - FromPeriod: ", 200304, apf.FromPeriod);
			AssertEquals("Run value - ToPeriod: ", 200305, apf.ToPeriod);

			reportFilterData.AccountingPeriodFilterCollection.Clear();
			apf.ClearValues();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 2);
			apf.SetScheduleTask(scheduleTask);
			reportFilterData.AccountingPeriodFilterCollection.Add(new AccountingPeriodFilter
			{
				UseSinglePeriod = true,
				UseYearToPeriod = true,
				UsePeriodRange = true,
				UseAllPeriods = true,
				ScheduleStorageSinglePeriod = new DateTime(1910, 1, 4),
				ScheduleStorageYearTo = new DateTime(1910, 2, 4),
				ScheduleStorageFrom = new DateTime(1909, 10, 4),
				ScheduleStorageTo = new DateTime(1909, 11, 4),
				DisplayName = "Period"
			});

			apf.SetFilterValue(reportFilterData);

			Assert("Schedule value - UseSinglePeriod:", apf.UseSinglePeriod);
			Assert("Schedule value - UseYearToPeriod:", apf.UseYearToPeriod);
			Assert("Schedule value - UsePeriodRange:", apf.UsePeriodRange);
			Assert("Schedule value - UseAllPeriods:", apf.UseAllPeriods);

			AssertEquals("Schedule Storage value - SinglePeriod: ", new DateTime(1910, 1, 4), apf.SinglePeriodSchedule.ToStorageValue());
			AssertEquals("Schedule Storage value - YearToPeriod: ", new DateTime(1910, 2, 4), apf.YearToPeriodSchedule.ToStorageValue());
			AssertEquals("Schedule Storage value - FromPeriod: ", new DateTime(1909, 10, 4), apf.FromPeriodSchedule.ToStorageValue());
			AssertEquals("Schedule Storage value - ToPeriod: ", new DateTime(1909, 11, 4), apf.ToPeriodSchedule.ToStorageValue());

			AssertEquals("Schedule value - SinglePeriod: ", 200310, apf.SinglePeriod);
			AssertEquals("Schedule value - YearToPeriod: ", 200311, apf.YearToPeriod);
			AssertEquals("Schedule value - FromPeriod: ", 200307, apf.FromPeriod);
			AssertEquals("Schedule value - ToPeriod: ", 200308, apf.ToPeriod);

			reportFilterData.AccountingPeriodFilterCollection.Clear();
			apf.ClearValues();

			reportFilterData.AccountingPeriodFilterCollection.Add(new AccountingPeriodFilter
			{
				UseSinglePeriod = true,
				UseYearToPeriod = true,
				UsePeriodRange = true,
				UseAllPeriods = true,
				DisplayName = "Period"
			});

			apf.SetFilterValue(reportFilterData);
			Assert(!apf.SinglePeriodSchedule.IsValid);
			Assert(!apf.YearToPeriodSchedule.IsValid);
			Assert(!apf.FromPeriodSchedule.IsValid);
			Assert(!apf.ToPeriodSchedule.IsValid);
		}

		public void TestFillFilterData()
		{
			apf.UseSinglePeriod = true;
			apf.UseYearToPeriod = true;
			apf.UsePeriodRange = true;
			apf.UseAllPeriods = true;

			apf.SinglePeriod = 200302;
			apf.YearToPeriod = 200303;
			apf.FromPeriod = 200304;
			apf.ToPeriod = 200305;

			apf.DisplayName = "Period";

			var reportData = new SelectedValueReportData();

			apf.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.AccountingPeriodFilterCollection.Count);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UseSinglePeriod);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UseYearToPeriod);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UsePeriodRange);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UseAllPeriods);
				AssertEquals(200302, reportData.FilterData.AccountingPeriodFilterCollection[0].SinglePeriod);
				AssertEquals(200303, reportData.FilterData.AccountingPeriodFilterCollection[0].YearToPeriod);
				AssertEquals(200304, reportData.FilterData.AccountingPeriodFilterCollection[0].FromPeriod);
				AssertEquals(200305, reportData.FilterData.AccountingPeriodFilterCollection[0].ToPeriod);
				AssertEquals("Period", reportData.FilterData.AccountingPeriodFilterCollection[0].DisplayName);
			});

			reportData.FilterData.AccountingPeriodFilterCollection.Clear();
			apf.ClearValues();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			apf.SetScheduleTask(scheduleTask);

			var reportFilterData = new ReportFilterData();
			reportFilterData.AccountingPeriodFilterCollection.Add(new AccountingPeriodFilter
			{
				UseSinglePeriod = true,
				UseYearToPeriod = true,
				UsePeriodRange = true,
				UseAllPeriods = true,
				ScheduleStorageSinglePeriod = new DateTime(1910, 1, 4),
				ScheduleStorageYearTo = new DateTime(1910, 2, 4),
				ScheduleStorageFrom = new DateTime(1909, 10, 4),
				ScheduleStorageTo = new DateTime(1909, 11, 4),
				DisplayName = "Period"
			});

			apf.SetFilterValue(reportFilterData);
			apf.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.AccountingPeriodFilterCollection.Count);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UseSinglePeriod);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UseYearToPeriod);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UsePeriodRange);
				AssertEquals(true, reportData.FilterData.AccountingPeriodFilterCollection[0].UseAllPeriods);
				AssertEquals(new DateTime(1910, 1, 4), reportData.FilterData.AccountingPeriodFilterCollection[0].ScheduleStorageSinglePeriod);
				AssertEquals(new DateTime(1910, 2, 4), reportData.FilterData.AccountingPeriodFilterCollection[0].ScheduleStorageYearTo);
				AssertEquals(new DateTime(1909, 10, 4), reportData.FilterData.AccountingPeriodFilterCollection[0].ScheduleStorageFrom);
				AssertEquals(new DateTime(1909, 11, 4), reportData.FilterData.AccountingPeriodFilterCollection[0].ScheduleStorageTo);
				AssertEquals("Period", reportData.FilterData.AccountingPeriodFilterCollection[0].DisplayName);
			});
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			apf.UseSinglePeriod = true;

			AccountingPeriodField apfRange = GetNewFieldWithTestData();
			apfRange.UsePeriodRange = true;

			AccountingPeriodField apfYearTo = GetNewFieldWithTestData();
			apfYearTo.UseYearToPeriod = true;

			return new FilterFieldWithUTSupport[]
			{
				apf,
				apfRange,
				apfYearTo
			};
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get { return 3; }
		}
	}
}
