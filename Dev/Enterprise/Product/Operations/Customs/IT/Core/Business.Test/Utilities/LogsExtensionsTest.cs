using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class LogsExtensionsTest : TestCaseWithFactory
{
	public void TestGuardClauses()
	{
		AssertExceptionThrown<ArgumentNullException>("When logCollection is null", () => LogsExtensions.LogCustomsStatusOverride(null, "", "", ""));
	}

	public void TestLogCustomsStatusOverride()
	{
		var logs = GetLogsCollection();
		AssertEquals("PRE-CONDITION: Has Status Override Log", false, logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO"));

		logs.LogCustomsStatusOverride("A0001", "AAA", "BBB");
		AssertEquals("POST-CONDITION: Has Status Override Log", true, logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference == "Entry A0001 Sent in status: AAA, BBB"));
	}

	Logs GetLogsCollection() => Factory.New<JobDeclaration>().Logs;
}
