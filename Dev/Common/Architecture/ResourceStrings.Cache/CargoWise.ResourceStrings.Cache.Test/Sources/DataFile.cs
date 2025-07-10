using System.Reflection;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Cache.Testing
{
	class DataFilesDirectoryTest : TestCase
	{
		public void TestIsLanguageResourceExists()
		{
			Assert(DataFile.IsLanguageFileExists("FR-FR"));
			Assert(!DataFile.IsLanguageFileExists("DDD"));
		}

		public void TestDataFilesAssemblyNotReferenced_ForDesignTimeUse()
		{
			foreach (AssemblyName name in typeof(DataFile).Assembly.GetReferencedAssemblies())
			{
				if (name.Name.Contains("DataFiles"))
				{
					Fail("You cannot reference ResourceStrings.DataFiles because the designer will not be able to resolve it outside of AssemblyLoader.");
				}
			}
			Assert(true);
		}
	}
}
