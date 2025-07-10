using Enterprise.DbUpgrader.Resource;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class BiEdwDbTemplateTest : TestCase
	{
		public void TestNoEdwTablesStartWithSpecificPrefixes()
		{
			MainDbTemplateMetadataTest.AssertNoTableNamesStartWithPrefix(new ScriptManager().BiEdwDbSchemaScript, "CLIENT");
			MainDbTemplateMetadataTest.AssertNoTableNamesStartWithPrefix(new ScriptManager().BiEdwDbSchemaScript, "DUMMY");
		}
	}
}
