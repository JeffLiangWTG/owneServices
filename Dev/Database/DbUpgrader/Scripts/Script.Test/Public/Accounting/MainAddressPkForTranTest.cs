using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(MainAddressPkForTran))]
	class MainAddressPkForTranTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "2D7D678D6EC9A6B49D67D28BAA82BBD52D9D4A7D86DFC298AE2F857470564E34";
		protected override string expectedEdwDbFunctionHash => "795B1217D9148E0AE884FD6849C01C04D7F3A6D23569A0733F2E3D893B5191B8";

		protected override string edwScriptPath => "Function/Accounting/MainAddressPkForTran.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new MainAddressPkForTran();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ComplianceReport.MainAddressPkForTran();
		}
	}
}

