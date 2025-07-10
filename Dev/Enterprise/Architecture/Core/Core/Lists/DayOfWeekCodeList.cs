using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZArchitecture.Core
{
	public class DayOfWeekCodeList : AutoDayOfWeekCodeList
	{
		public sealed class LocalizedCodes
		{
			LocalizedCodes() { }

			public static MultilingualString Sunday => SourceGenerated.ResString.GetMultilingualString("b2e50a4c-a36a-4794-a531-a9cf7fa65149", "SUN");
			public static MultilingualString Monday => SourceGenerated.ResString.GetMultilingualString("bced10b8-11e8-447f-8114-d9aaa9a9569d", "MON");
			public static MultilingualString Tuesday => SourceGenerated.ResString.GetMultilingualString("9ec9bfe8-663d-43e8-9610-1b43bd7e45f0", "TUE");
			public static MultilingualString Wednesday => SourceGenerated.ResString.GetMultilingualString("a1fb9a6f-5f01-42c8-a6b2-985b1840480a", "WED");
			public static MultilingualString Thursday => SourceGenerated.ResString.GetMultilingualString("a7f2ccb6-0b6a-4635-b5a9-9bbca1dfb026", "THU");
			public static MultilingualString Friday => SourceGenerated.ResString.GetMultilingualString("7928127f-404e-400f-bb88-2de359da1795", "FRI");
			public static MultilingualString Saturday => SourceGenerated.ResString.GetMultilingualString("ab0ffc93-5c3a-4894-9ba2-1d6db065074e", "SAT");
		}

		public DayOfWeek GetDayOfWeek(string code)
		{
			if (!Mappping.TryGetValue(code, out var result))
			{
				throw new ArgumentException("Unknown code '" + code + "'", nameof(code));
			}
			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "This uses an ImmutableDictionary and each of its element is immutable as well.")]
		public static IImmutableDictionary<string, DayOfWeek> Mappping { get; } = new Dictionary<string, DayOfWeek>()
		{
			[Codes.Sunday] = DayOfWeek.Sunday,
			[Codes.Monday] = DayOfWeek.Monday,
			[Codes.Tuesday] = DayOfWeek.Tuesday,
			[Codes.Wednesday] = DayOfWeek.Wednesday,
			[Codes.Thursday] = DayOfWeek.Thursday,
			[Codes.Friday] = DayOfWeek.Friday,
			[Codes.Saturday] = DayOfWeek.Saturday,
		}
		.ToImmutableDictionary();
	}
}
