using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ChinaStatementOfShareholdersEquity))]
	class ChinaStatementOfShareholdersEquityTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "BA65FF115694BD62CBE5D68AAA0834E1CD5CA11F5C24C3FEC90A56ACECE1B2E0";
		protected override string expectedEdwDbFunctionHash => "BF75F72EC87C6E426F4EF63E307A5389D250940111B6F44002EAA87E1F232D87";

		protected override string edwScriptPath => "Function/Accounting/ChinaStatementOfShareholdersEquity.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ChinaStatementOfShareholdersEquity();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ChinaStatementOfShareholdersEquity();
		}
	}
}

