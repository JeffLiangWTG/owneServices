using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(ValidationRuleMessages))]
sealed class ValidationRuleMessagesTest : ValidationRuleMessagesAbstractTest<ValidationRuleMessages>
{
	public void TestR0994_1Message()
	{
		AssertEquals("[NR0077] Total Gross Weight is different from the sum of House Consignments Gross Weight (10 Kg)", configuration.R0994_1Message("10"));
	}

	protected override Dictionary<string, string> RuleCodeReplacements => new()
	{
		{ "C0101-1", "C0101, R0859" },
		{ "R0994-1", "NR0077" },
	};
}
