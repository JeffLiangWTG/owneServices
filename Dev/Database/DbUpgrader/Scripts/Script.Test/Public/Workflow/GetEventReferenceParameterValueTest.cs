using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Workflow.Testing
{
	[TestedType(typeof(GetEventReferenceParameterValue))]
	sealed class GetEventReferenceParameterValueTest : DbCreateScriptTest
	{
		public void TestFunction()
		{
			Assert("|", "AAA", string.Empty);
			Assert("", "AAA", string.Empty);
			Assert("Some reference", "AAA", string.Empty);
			Assert("Some reference|", "AAA", string.Empty);
			Assert("|BBB=bbb", "AAA", string.Empty);
			Assert("Some reference|BBB=bbb value|CCC=ccc value", "AAA", string.Empty);
			Assert("Some reference|AAA=aaa value|CCC=ccc value", "AAA", "aaa value");
			Assert("Some reference|CCC=ccc value|AAA=aaa value", "AAA", "aaa value");
			Assert("|AAA=aaa value", "AAA", "aaa value");
		}

		void Assert(string eventReference, string parameter, string expectedValue)
		{
			var sql = FormattableString.Invariant($"SELECT Value FROM dbo.GetEventReferenceParameterValue('{eventReference}', '{parameter}')");

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				reader.Read();

				var actualValue = reader.GetString(0);
				AssertEquals(expectedValue, actualValue);
			}
		}
	}
}
