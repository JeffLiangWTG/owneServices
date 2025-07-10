using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(AccMultilingualBalanceSheet))]
	class AccMultilingualBalanceSheetTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "33FA5C8C7584C16D98250A6DE6065CAAAFEA6D86065C14B19C16471A9361F3A3";
		protected override string expectedEdwDbFunctionHash => "C0B157EAC47E2F755304D8DF8A0DDAA22E34DA707CEDCE27CB889C70A5F181D1";

		protected override string edwScriptPath => "Function/Accounting/AccMultilingualBalanceSheet.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new AccMultilingualBalanceSheet();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.AccMultilingualBalanceSheet();
		}
	}
}

