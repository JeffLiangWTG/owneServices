using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.KNA.Testing
{
	[TestedType(typeof(KNADataRegistry))]
	public class KNADataRegistryTest : RegistryItemSetTestCase<KNADataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Should be 2 user visible registry item", 2, AllItems.Count);
			AssertVisible(ItemSet.DirectoryToImportXmlRaw);
			AssertVisible(ItemSet.ProcessedDirectoryForXmlFilesRaw);
		}

		public void TestDirectoryToImportXml()
		{
			ItemSet.DirectoryToImportXml = "blah";
			AssertEquals("DirectoryToImportXml is not 'blah'", "blah", ItemSet.DirectoryToImportXml);
		}

		public void TestProcessedDirectoryForXmlFiles()
		{
			ItemSet.ProcessedDirectoryForXmlFiles = "abc";
			AssertEquals("ProcessedDirectoryForXmlFilesRaw in not 'abc'", "abc", ItemSet.ProcessedDirectoryForXmlFiles);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestDirectoryToImportXmlRaw()
		{
			AssertEquals("Caption", "Directory To Import Xml Files", ItemSet.DirectoryToImportXmlRaw.Caption);
			AssertEquals("Hint", "Specify Directory to store xml files for CargoWise One to access.", ItemSet.DirectoryToImportXmlRaw.Hint);
			AssertEquals("Storage Flag", RegistryStorageFlags.System, ItemSet.DirectoryToImportXmlRaw.Storage);
		}

		public void TestProcessedDirectoryForXmlFilesRaw()
		{
			AssertEquals("Caption", "Processed Directory For Xml Files", ItemSet.ProcessedDirectoryForXmlFilesRaw.Caption);
			AssertEquals("Hint", "Specify directory where xml files will be moved after they have been processed", ItemSet.ProcessedDirectoryForXmlFilesRaw.Hint);
			AssertEquals("Storage Flag", RegistryStorageFlags.System, ItemSet.ProcessedDirectoryForXmlFilesRaw.Storage);
		}
	}
}
