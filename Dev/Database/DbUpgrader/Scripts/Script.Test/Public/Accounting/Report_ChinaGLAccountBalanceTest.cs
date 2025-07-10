using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ChinaGLAccountBalance))]
	class Report_ChinaGLAccountBalanceTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "7C5A2C23E3A7C4ED4B64269821F3970672E9F4C50DFB3FB487969471B82A2C03";
		protected override string expectedEdwDbFunctionHash => "6490AE0A86110EDC4FE78A45D2EF48F88AF79B47E6A50E696F854F5D9090DFF0";

		protected override string edwScriptPath => "Function/Accounting/Report_ChinaGLAccountBalance.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_ChinaGLAccountBalance();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
				return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.Report_ChinaGLAccountBalance();
		}
	}
}

