using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class MainDbTemplateWithExtraDevelopmentObjectsTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtraDevelopmentObjectsXml()
		{
			string expectedXml = File.ReadAllText(BaseSourcePath + @"Database\DbUpgrader\Schema\Engine\Template\ExtraDevelopmentObjects.xml");
			AssertEquals("XML loaded from resource should be same as in source file", expectedXml, MainDbTemplateWithExtraDevelopmentObjects.DummyTableCreator.ExtraDevelopmentObjectsXml);
		}
	}
}
