using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(vw_AccTransactionLines))]
	class vw_AccTransactionLinesTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "B49E53FD467B3D38022056CDA43D2AC3D3122C0EDDFD14725B70B55D2B071973";
		protected override string expectedEdwDbFunctionHash => "1DBBC9AAD0969F28010FE3896A5305E5D4988B1EF9BF46F6076E9D1C6E7E3664";

		protected override string edwScriptPath => "ReportFunctions/Accounting/vw_AccTransactionLines.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new vw_AccTransactionLines();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.vw_AccTransactionLines();
		}
	}
}

