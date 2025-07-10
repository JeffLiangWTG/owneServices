using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.MasterFiles
{
	[TestedType(typeof(OrgAddressFormatter))]
	class OrgAddressFormatterEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/MasterFiles/OrgAddressFormatter.sql";
		protected override string expectedMainDbFunctionHash => "070E8D669339A124D5BDAA315A9F4E3548A628CE0DA1BBE4BF35818CBBC1588C";
		protected override string expectedEdwDbFunctionHash => "917F145EC94088C363C4D0D30444FB08E62A9866A39AAE678E7155BAC57B685F";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new OrgAddressFormatter();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.OrgAddressFormatter();
		}
	}
}

