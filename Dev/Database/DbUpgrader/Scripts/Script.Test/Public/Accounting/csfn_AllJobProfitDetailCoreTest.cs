using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(csfn_AllJobProfitDetailCore))]
	class csfn_AllJobProfitDetailCoreTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "1F806E340B05D2D59DDE3A64D80056E6B6E42400D3E8AB2F34E54C4C15F49DC1";
		protected override string expectedEdwDbFunctionHash => "452B0C1149D1B9E1DCA4FE0D7F89D7EAA9758FC5C5090A1BC751C82AFDBFBF1C";

		protected override string edwScriptPath => "ReportFunctions/Accounting/csfn_AllJobProfitDetailCore.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_AllJobProfitDetailCore();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.csfn_AllJobProfitDetailCore();
		}
	}
}

