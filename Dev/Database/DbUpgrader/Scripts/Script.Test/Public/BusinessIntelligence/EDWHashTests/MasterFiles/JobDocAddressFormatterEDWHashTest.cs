using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.MasterFiles
{
	[TestedType(typeof(JobDocAddressFormatter))]
	class JobDocAddressFormatterEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/MasterFiles/JobDocAddressFormatter.sql";
		protected override string expectedMainDbFunctionHash => "98A09566130E2536B0C5F268FE63295448ECDEF616BB1CBB8991159726B80C49";
		protected override string expectedEdwDbFunctionHash => "894AA1FF0E93E58E34CC0806A7E1C3301446D1A4B6414A3B03539E4C230155B7";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new JobDocAddressFormatter();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.JobDocAddressFormatter();
		}
	}
}

