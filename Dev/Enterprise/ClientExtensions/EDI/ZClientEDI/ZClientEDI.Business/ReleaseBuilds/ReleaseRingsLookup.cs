using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WTG.DevTools.Definitions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business
{
	[Immutable]
	public class ReleaseRingsLookup
	{
		static readonly Lazy<ImmutableArray<string>> lazyCheckInTaskTypes = new (() => ReleaseRings.List().Select(item => item.CheckinTask).ToImmutableArray());
		static readonly Lazy<ImmutableArray<string>> lazyCheckInTaskTypesExcludingAlpha = new (() => ReleaseRings.List().Where(item => item.Code != ReleaseRings.Codes.ALP).Select(item => item.CheckinTask).ToImmutableArray());
		static readonly Lazy<ImmutableArray<string>> lazyCheckInRingCodes = new (() => ReleaseRings.List().Select(item => item.Code).ToImmutableArray());
		static readonly Lazy<ImmutableDictionary<string, string>> lazyRingCodeByCheckInType = new (() => ReleaseRings.List().ToImmutableDictionary(x => x.CheckinTask, x => x.Code));

		public static ReleaseRingsLookup Instance => instance ??= new ReleaseRingsLookup();

		public static IEnumerable<string> CheckInTaskTypes => lazyCheckInTaskTypes.Value;

		public static IEnumerable<string> CheckInTaskTypesExcludingAlpha => lazyCheckInTaskTypesExcludingAlpha.Value;

		public static IEnumerable<string> CheckInRingCodes => lazyCheckInRingCodes.Value;

		public string GetPreviousRing(string ringCode) => ReleaseRings.Lookup(ringCode)?.PreviousRing?.Code ?? string.Empty;

		public static string GetRingCodeByCheckInType(string checkinType) => lazyRingCodeByCheckInType.Value.TryGetValue(checkinType, out var ringCode) ? ringCode : string.Empty;

		#region Private

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static ReleaseRingsLookup instance;

		ReleaseRingsLookup()
		{
		}

		#endregion
	}
}

