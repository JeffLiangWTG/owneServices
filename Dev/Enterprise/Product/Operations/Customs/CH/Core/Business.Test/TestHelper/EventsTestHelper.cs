using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.Business.Testing;

public static class EventsTestHelper
{
	public static void AssertEventAdded(BusinessObject parent, Event expectedEvent, string expectedReference = null, ZDateTime? sinceUtc = null, string withReference = null, string message = null)
	{
		if (parent is IStmALogParent logParent)
		{
			var logEntry = logParent.Logs.MostRecentLogByEventTime(expectedEvent, GetExtraQuery(sinceUtc, withReference));
			AssertNotNull(GetMessage(message, $"Event {expectedEvent.Code} expected"), logEntry);
			if (logEntry != null && expectedReference != null)
			{
				AssertEquals($"{message}: SL_Reference", expectedReference, logEntry.SL_Reference);
			}
		}
		else
		{
			InvalidLogParent(parent, message, $"expected event: {expectedEvent.Code}");
		}
	}

	public static void AssertEventNotAdded(BusinessObject parent, Event unexpectedEvent, ZDateTime? sinceUtc = null, string withReference = null, string message = null)
	{
		if (parent is IStmALogParent logParent)
		{
			var logEntry = logParent.Logs.MostRecentLogByEventTime(unexpectedEvent, GetExtraQuery(sinceUtc, withReference));
			AssertNull(GetMessage(message, $"Event not expected: {logEntry?.SL_SE_NKEvent} {logEntry?.SL_Reference}"), logEntry);
		}
		else
		{
			InvalidLogParent(parent, message, $"unexpected event: {unexpectedEvent.Code}");
		}
	}

	static string GetMessage(string message, string info) => $"{message}{(message == null ? null : ": ")}{info}";

	static ZQuery GetExtraQuery(ZDateTime? sinceUtc, string withReference)
	{
		ZQuery extraQuery = null;

		if (sinceUtc != null)
		{
			extraQuery ??= new ZQuery();
			extraQuery.AddToFilter(StmALogSchema.SL_EventTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, sinceUtc.Value);
		}

		if (withReference != null)
		{
			extraQuery ??= new ZQuery();
			extraQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Like, withReference);
		}

		return extraQuery;
	}

	static void InvalidLogParent(BusinessObject parent, string message, string info)
	{
		if (parent == null)
		{
			Assert(GetMessage(message, $"Event parent BO is null ({info})"), false);
		}
		else
		{
			Assert(GetMessage(message, $"BO does not implement IStmALogParent: {parent.GetType().FullName} ({info})"), false);
		}
	}
}
