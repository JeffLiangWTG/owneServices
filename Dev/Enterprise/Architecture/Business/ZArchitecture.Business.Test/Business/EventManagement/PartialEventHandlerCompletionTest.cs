using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;
using static CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class PartialEventHandlerCompletionTest : TestCaseWithFactory
	{
		[TestDate(2022, 7, 5)]
		public void TestCompletionDateTime()
		{
			var dummyLogParent = Factory.New<DummyWithLogs>();

			var log1 = dummyLogParent.Logs.AddNew(Events.Authorised,
				new ZDateTimeOffset(2022, 7, 4, 10, 0, 0),
				ZBool.False,
				new KeyValuePair<string, string>(
					Partial,
					3.ToString()),
				new KeyValuePair<string, string>(
					Total,
					10.ToString()),
				new KeyValuePair<string, string>(
					Location,
					"AUSYD"));

			var log2 = dummyLogParent.Logs.AddNew(Events.Authorised,
				new ZDateTimeOffset(2022, 7, 4, 11, 0, 0),
				ZBool.False,
				new KeyValuePair<string, string>(
					Partial,
					7.ToString()),
				new KeyValuePair<string, string>(
					Total,
					10.ToString()),
				new KeyValuePair<string, string>(
					Location,
					"AUSYD"));

			var handler = new PartialEventHandlerForTest();

			AssertContainsExactElementsInExactOrder("expected only partial events",
				new[]
				{
					"ATH|04-Jul-22 10:00:00|LOC=AUSYD|PTL=3|TTL=10",
					"ATH|04-Jul-22 11:00:00|LOC=AUSYD|PTL=7|TTL=10"
				},
				FormatLogs(dummyLogParent));

			handler.Handle(log2, dummyLogParent);

			AssertContainsExactElementsInExactOrder("expected partial and completion events, the completion event should take event time from last partial event",
				new[]
				{
					"ATH|04-Jul-22 10:00:00|LOC=AUSYD|PTL=3|TTL=10",
					"ATH|04-Jul-22 11:00:00|LOC=AUSYD|PTL=7|TTL=10",
					"ATH|04-Jul-22 11:00:00|LOC=AUSYD|PTL|TTL=10"
				},
				FormatLogs(dummyLogParent));
		}

		string FormatLog(IStmALog log) => string.Format(
			$"{log.SL_SE_NKEvent}|{log.SL_EventTime}|{GetParameter(log, Location)}|{GetParameter(log, Partial)}|{GetParameter(log, Total)}");

		string[] FormatLogs(IStmALogParent logParent) =>
			logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.AuthorisedCode)
				.OrderBy(log => log.SL_EventTime)
				.Select(FormatLog)
				.ToArray();

		string GetParameter(IStmALog log, string parameterCode)
		{
			return log.Parameters.TryGetValue(parameterCode, out var res)
				? string.Concat(parameterCode, "=", res)
				: parameterCode;
		}
	}
}
