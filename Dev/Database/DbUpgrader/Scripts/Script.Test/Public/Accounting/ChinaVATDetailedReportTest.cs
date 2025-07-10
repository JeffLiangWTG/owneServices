using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ChinaVATDetailedReport))]
	class ChinaVATDetailedReportTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "7827332A5D63C3582A66587B1EC044EF2A9456E40B44A2184B5398F46454CE90";
		protected override string expectedEdwDbFunctionHash => "B70F2C359EFEC016C7D9E3E3AF4C4E86860A660CE578811B521BF502C99AA025";

		protected override string edwScriptPath => "Function/Accounting/ChinaVATDetailedReport.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ChinaVATDetailedReport();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ChinaVATDetailedReport();
		}
	}
}

