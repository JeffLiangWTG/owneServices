using System;
using System.Collections.Immutable;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.EntryStatus;

namespace Enterprise.Customs.DE.Business
{
	static class CUSTAXEntryClearedStatusHelper
	{
		public static bool IsCleared(string status) => ClearedStatuses.Contains(status);

		[ThreadStatic]
		static ImmutableHashSet<string> clearedStatuses;

		static ImmutableHashSet<string> ClearedStatuses => clearedStatuses ??= ImmutableHashSet.Create(TX1, TX2, TX3, TX5, TX6, TX8);
	}
}
