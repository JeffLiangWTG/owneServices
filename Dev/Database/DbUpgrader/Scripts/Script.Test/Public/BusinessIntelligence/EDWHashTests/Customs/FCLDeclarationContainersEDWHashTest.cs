using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Customs
{
	[TestedType(typeof(FCLDeclarationContainers))]
	class FCLDeclarationContainersEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Customs/FCLDeclarationContainers.sql";
		protected override string expectedMainDbFunctionHash => "BD49C951BC5D29323A08D6C4D56C6320C13F396B703BADFAB9F7FF0212B7E7B8";
		protected override string expectedEdwDbFunctionHash => "B0B907072FC1C173D935F5AAEB98C8C9E93FF311C293B9B8706C42C113E78258";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new FCLDeclarationContainers();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs.FCLDeclarationContainers();
		}
	}
}

