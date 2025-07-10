using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	public abstract class FlatFileDataImporterTestCase : TestCaseWithFactory
	{
		protected abstract FlatFileDataImporter GetDataImporter();
		protected abstract string PathToTestFile { get; }

		public void TestXsdIsNotNull()
		{
			var importer = GetDataImporter();
			AssertNotNull("XSD should not be null", importer.CreateXsdForTest());
		}

		public void TestFlatFileFormatIsNotNull()
		{
			var importer = GetDataImporter();
			AssertNotNull("FlatFileFormat should not be null", importer.FlatFileFormatForTest);
		}

		public void TestConverterIsNotNull()
		{
			var importer = GetDataImporter();
			AssertNotNull("Converter object should not be null", importer.CreateConverterForTest(null));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDoesNotThrowExceptions()
		{
			var importer = GetDataImporter();
			Assert("Precondition: PathToTestFile must point to a valid test file that exists on the filesystem", File.Exists(PathToTestFile));
			importer.ImportData(PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
		}
	}
}
