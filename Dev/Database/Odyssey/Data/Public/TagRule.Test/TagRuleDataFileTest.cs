using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class TagRuleDataFileTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new TagRuleDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}
	}
}
