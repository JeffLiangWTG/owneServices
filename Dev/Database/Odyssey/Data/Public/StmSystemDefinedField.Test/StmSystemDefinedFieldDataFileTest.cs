using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class StmSystemDefinedFieldDataFileTest : StmSystemDefinedFieldTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new StmSystemDefinedFieldDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestLoadDataFromDatabase()
		{
			AssertData(InsertData());
		}
	}
}
