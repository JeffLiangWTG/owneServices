using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class LocalCartageDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new LocalCartageDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}
	}
}
