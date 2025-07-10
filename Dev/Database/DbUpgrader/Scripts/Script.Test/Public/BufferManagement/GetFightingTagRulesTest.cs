using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetFightingTagRules))]
	sealed class GetFightingTagRulesTest : DbCreateScriptTest
	{
		// This function is tested at the business layer: Enterprise.BufferManagement.Business.Test.TagRuleMonitorTest
	}
}

