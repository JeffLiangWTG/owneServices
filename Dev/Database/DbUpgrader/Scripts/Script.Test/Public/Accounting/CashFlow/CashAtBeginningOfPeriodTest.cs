using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(CashAtBeginningOfPeriod))]
	class CashAtBeginningOfPeriodTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "B8DC6F5084D2D9B4CA195C972203748ABC0728FB797B249DC12DBA0E0492629F";
		protected override string expectedEdwDbFunctionHash => "D922BEE59B9B3D0CDD4F9681592CEBA373B9BCCFD27981604C270CA1B0DC9D47";

		protected override string edwScriptPath => "Function/Accounting/CashFlow/CashAtBeginningOfPeriod.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new CashAtBeginningOfPeriod();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.CashAtBeginningOfPeriod();
		}
	}
}
