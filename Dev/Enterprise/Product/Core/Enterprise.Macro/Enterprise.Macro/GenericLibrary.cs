using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Macro.Extensions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Macro
{
	public sealed class GenericLibrary : MacroLibraryBase
	{
		public override IEnumerator<IMacroMetaData> GetEnumerator() => lazyMacrosRegister.Value.GetEnumerator();

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<ICollection<IMacroMetaData>> lazyMacrosRegister = new Lazy<ICollection<IMacroMetaData>>(() => Load(MacroHandlers));

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Macro strings")]
		static IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<DateTime>>(
					"Now",
					NowMacroDescription,
					() => ZDateTime.Now.ToDateTime(),
					"Now");
				yield return new Handler<Func<DateTime>>(
					"UtcNow",
					UtcNowMacroDescription,
					() => ZDateTime.UtcNow.ToDateTime(),
					"UtcNow");
				yield return new Handler<Func<ZDateTimeOffset>>(
					"NowDateTimeOffset",
					NowDateTimeOffsetMacroDescription,
					() => ZDateTimeOffset.Now,
					"NowDateTimeOffset");
				yield return new Handler<Func<DateTime>>(
					"Today",
					TodayMacroDescription,
					() => ZDate.Today.ToDateTime(),
					"Today");
				yield return new Handler<Func<BusinessObject, string, bool>>(
					"HasEvent",
					HasEventMacroDescription,
					(bizo, eventCode) => HasEvent(bizo, eventCode),
					"HasEvent({EventCode})");
				yield return new Handler<Func<ZDateTime, string, string, string, string, ZDateTime>>(
					"AddDurationToDate",
					AddDurationToDateMacroDescription,
					(odate, span, value, workDate, branch) => AddDurationToDate(odate, span, value, workDate, branch), "AddDurationToDate({Date}, {Span}, {Value}[ ,{WorkDay}][ ,{Branch}])");
				yield return new Handler<Func<ZDateTime, string, string, string, ZDateTime>>(
					"AddDurationToDate",
					AddDurationToDateMacroDescription,
					(odate, span, value, workDate) => AddDurationToDate(odate, span, value, workDate));
				yield return new Handler<Func<ZDateTime, string, string, ZDateTime>>(
					"AddDurationToDate",
					AddDurationToDateMacroDescription,
					(odate, span, value) => AddDurationToDate(odate, span, value));
				yield return new Handler<Func<ZDateTime, ZDateTime>>(
					"ToLocal",
					ToLocalMacroDescription,
					(odate) => Env.Time.GetLocalTimeFromUtc(odate.ToDateTime()),
					"ToLocal()");
				yield return new Handler<Func<ZDateTime, ZDateTime>>(
					"ToUtc",
					ToUtcMacroDescription,
					(odate) => Env.Time.GetUtcFromLocalTime(odate.ToDateTime()),
					"ToUtc()");
				yield return new Handler<Func<ICustomFieldProvider, string, object>>(
					"GetCustomField",
					GetCustomFieldMacroDescription,
					(provider, name) => GetCustomField(provider, name), "GetCustomField({customFieldName}[,{fieldIdentifier}])");
				yield return new Handler<Func<ICustomFieldProvider, string, int, object>>(
					"GetCustomField",
					GetCustomFieldMacroDescription,
					(provider, name, index) => GetCustomField(provider, name, index));
				yield return new Handler<Func<object, object, ZString>>(
					"FormatNumber",
					FormatNumberDescription,
					(value, currencyCode) => FormatNumber(value, currencyCode), "FormatNumber({value}, {currencyCodeOrDecimalPlaces})");
			}
		}

		static readonly MultilingualString NowMacroDescription = ResString.GetMultilingualString("A5FB6D48-A9DC-46A5-BB7F-158E21C99609", @"Syntax:
{0}
Returns current logged in user system/branch local date and time.
Examples:
{0}", "Now");

		static readonly MultilingualString UtcNowMacroDescription = ResString.GetMultilingualString("A96E2D8C-3B54-448F-8277-01D18E958F4E", @"Syntax:
{0}
Returns current logged in user system/branch time expressed as Coordinated Universal Time (UTC).
Examples:
{0}", "UtcNow");

		static readonly MultilingualString NowDateTimeOffsetMacroDescription = ResString.GetMultilingualString("D0C68E0C-41A6-4884-9ADE-CAD70ECF87C3", @"Syntax:
{0}
Returns current logged in user system/branch local date and time with time zone offset information included.
Usage:
{0}
{1} -- returns the difference in minutes between the local date and time and Coordinated Universal Time (UTC)", "NowDateTimeOffset", "NowDateTimeOffset.Offset.TotalMinutes");

		static readonly MultilingualString TodayMacroDescription = ResString.GetMultilingualString("EEE18763-F126-4C85-A4CC-13547388EF0E", @"Syntax:
{0}
Returns current logged in user system/branch local date.
Examples:
{0}", "Today");

		static readonly MultilingualString HasEventMacroDescription = ResString.GetMultilingualString("327EA0E0-DE4F-4C1E-AB11-0F7BECF9CAE6", @"Syntax:
{0}
Returns true if the specified event which is neither canceled nor an estimate exists and returns false if such an event does not exist.
Examples:
{1}", "HasEvent({EventCode})", "HasEvent(\"FLO\")");

		static readonly MultilingualString GetCustomFieldMacroDescription = ResString.GetMultilingualString("BC1AA09A-DF3A-4621-BEAB-0738B3F1D91B", @"Syntax:
{0}
Returns the value of the custom field.
If the custom field type is Combo Box an additional parameter {1} needs to be included to retrieve the 'Description' value of combo box. Only a value of 2 is accepted for {1}. 
Examples:
{2}
{3} -- returns the Code of combo box custom field
{4} -- returns the Description of combo box custom field", "GetCustomField({customFieldName}[,{fieldIdentifier}])", "{fieldIdentifier}", "GetCustomField(strCustomfield)", "GetCustomField(comboBoxfield)", "GetCustomField(comboBoxfield,2)");

		static readonly MultilingualString FormatNumberDescription = ResString.GetMultilingualString("8269d2ef-99b1-4f42-80f8-5767210eeb2c", @"Syntax:
{0}
Returns the supplied numeric value formatted according to the login company's culture-specific number format settings.
Replace {1} with a positive integer to force the number of decimals, or a negative integer to decide the number of decimals based on the current login company's reciprocal nature. 
Examples:
{2}
{3}", "FormatNumber({value}, {currencyCodeOrDecimalPlaces})", "{currencyCodeOrDecimalPlaces}", "FormatNumber(JS_ActualWeight, 6)", "FormatNumber(JS_GoodsValue, \"AUD\")");

		static readonly MultilingualString AddDurationToDateMacroDescription = ResString.GetMultilingualString("77FDF15C-3CB2-40E4-92F3-DD0F9DD92D8A", @"Syntax:
{0}
Returns a date after adding or subtracting the specified date/time value from the date.
If the optional parameter {1} is included with the value Y, then only working days will be included in the calculation.
The date span can be only DAYS in workday parameter is included.
If the branch is specified that will be used to find out the working days, otherwise Saturday and Sunday will be treated as the non-working days of the week.
Examples:
{2}
{3}
{4}", "AddDurationToDate({Date}, {Span}, {Value}[ ,{WorkDay}][ ,{Branch}])", "WorkDay", "AddDurationToDate(JS_E_DEP, \"HOURS\",\"23\")", "AddDurationToDate(JS_E_DEP, \"DAYS\", \"2\", \"Y\", \"SYD\")", "AddDurationToDate(JS_E_ARV, \"DAYS\", \"-1\", \"Y\")");

		static readonly MultilingualString ToLocalMacroDescription = ResString.GetMultilingualString("AB8B49CF-F0EA-4A14-8F7A-A046369CB8C6", @"Syntax:
{0}
Converts a Coordinated Universal Time (UTC) date and time to the current logged in user system/branch local time zone and returns it.
Usage:
{1}
{2}", "UtcDateTime.ToLocal()", "JS_SystemLastEditTimeUtc.ToLocal()", "datetime.Parse(\"08/18/2018 07:22:16\").ToLocal()");

		static readonly MultilingualString ToUtcMacroDescription = ResString.GetMultilingualString("32727E22-9A8D-4F6F-B92A-9DDF09EA8742", @"Syntax:
{0}
Converts a date and time from the current logged in user system/branch local time zone to Coordinated Universal Time (UTC) and returns it.
Usage:
{1}
{2}", "LocalDateTime.ToUtc()", "JS_E_ARV.ToUtc()", "datetime.Parse(\"08/18/2018 18:22:16\").ToUtc()");

		#region HasEvent

		static bool HasEvent(BusinessObject bizo, string eventCode)
		{
			if (bizo == null || eventCode == null || Events.All[eventCode] == null)
			{
				return false;
			}
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, bizo.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, false);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			return bizo.Factory.Exists(typeof(StmALog), query);
		}
		#endregion

		#region AddDurationToDate

		static ZDateTime AddDurationToDate(ZDateTime odate, string span, string value)
		{
			return AddDurationToDateHelper.AddDurationToDate(odate, span, value, null, null);
		}

		static ZDateTime AddDurationToDate(ZDateTime odate, string span, string value, string workDate)
		{
			return AddDurationToDateHelper.AddDurationToDate(odate, span, value, workDate, null);
		}

		static ZDateTime AddDurationToDate(ZDateTime odate, string span, string value, string workDate, string branch)
		{
			return AddDurationToDateHelper.AddDurationToDate(odate, span, value, workDate, branch);
		}
		#endregion

		#region GetCustomField

		static object GetCustomField(ICustomFieldProvider provider, string name)
		{
			return CustomiseFieldsMacroHelper.GetCustomField(provider, name, new BusinessObjectFactory(), false) ?? throw new InvalidOperationException($"The Custom Field with the name '{name}' is not defined.");
		}

		static object GetCustomField(ICustomFieldProvider provider, string name, int index)
		{
			if (index != 2)
			{
				throw new InvalidOperationException("Field identifier can only be 2.");
			}
			return CustomiseFieldsMacroHelper.GetCustomField(provider, name, new BusinessObjectFactory(), false, index) ?? throw new InvalidOperationException($"The Custom Field with the name '{name}' is not defined.");
		}
		#endregion

		#region FormatNumber
		static ZString FormatNumber(object value, object currencyCode)
		{
			return FormatNumberHelper.FormatNumber(value, currencyCode);
		}

		#endregion
	}
}
