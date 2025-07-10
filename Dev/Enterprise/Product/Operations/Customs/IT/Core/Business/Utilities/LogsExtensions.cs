using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business;

public static class LogsExtensions
{
	public static void LogCustomsStatusOverride(this Logs logCollection, ZString entryReference, ZString messageStatus, ZString customsStatus)
	{
		Argument.NotNull(logCollection, nameof(logCollection));

		var logText = FormattableString.Invariant($"Entry {entryReference} Sent in status: {new ZStringBuilder().AppendIfNotEmpty(messageStatus).AppendIfNotEmpty(customsStatus).ToStringWithDelimiterBetweenAppends(", ")}");
		logCollection.AddNew(Events.CustomsStatusOverride, logText, ZDateTimeOffset.Now);
	}
}
