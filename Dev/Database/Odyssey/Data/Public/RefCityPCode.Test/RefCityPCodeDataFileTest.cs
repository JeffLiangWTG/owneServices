using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefCityPCodeDataFileTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new RefCityPCodeDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}
	}
}
