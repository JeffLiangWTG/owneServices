using System.Data;
using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class DataFileTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadDataFromFileHandlesTransientFailure()
		{
			var dataFile = new ConcreteDataFile(TestFileConstants.TestDataFileRelativePath, null);
			AssertNotNull(dataFile.LoadDataFromFile());
			AssertEquals("Times LoadFromXml was called", 2, dataFile.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadDataFromGZipCompressedFile()
		{
			var dataFile = new CompressedFile();
			var result = dataFile.LoadDataFromFile();
			AssertEquals(2, result.Tables.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVersionRegistryItemName()
		{
			var dataFile = new ConcreteDataFile(@"Concrete\Concrete.xml", System.Array.Empty<string>());
			AssertEquals("VersionRegistryItem.Name", "XsdVersion-Enterprise.DbUpgrader.Data.Concrete.Concrete.xml", dataFile.VersionRegistryItem.ItemName);
		}

		sealed class CompressedFile : DataFile
		{
			public CompressedFile()
				: base(@"Public\RefCountry\RefCountry.xml.gz", "RefCountry", "RefCurrency")
			{
			}

			protected override DataSet LoadDataSet() => null;

			public override string DefaultDataFileBasePath => Path.Combine(TestFileConstants.BaseSourcePath, @"Database\Odyssey\Data\");
		}
	}
}
