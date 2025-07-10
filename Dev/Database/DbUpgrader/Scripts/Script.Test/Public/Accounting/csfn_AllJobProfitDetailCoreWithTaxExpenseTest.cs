using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(csfn_AllJobProfitDetailCoreWithTaxExpense))]
	class csfn_AllJobProfitDetailCoreWithTaxExpenseTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "94447A50DB5DEA74F7EC3D2FF47EFB055A81A9474C3EF6BE1AA441323F0DBD48";
		protected override string expectedEdwDbFunctionHash => "2ED5939A600DA27816DD773E29407EF5D7E6E54771856FFBFFE0A151DCF8872D";

		protected override string edwScriptPath => "ReportFunctions/Accounting/csfn_AllJobProfitDetailCoreWithTaxExpense.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_AllJobProfitDetailCoreWithTaxExpense();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.csfn_AllJobProfitDetailCoreWithTaxExpense();
		}
	}
}

