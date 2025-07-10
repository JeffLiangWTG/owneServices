using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	class JXCDataImporterBizOValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(JXCDataImporterBizOValidation), DataImporter.Validation.AutoValidationType);
		}

		public void TestValidateAll()
		{
			Assert("Pre-condition", !DataImporter.HasErrors);
			DataImporter.Validation.ValidateAll();
			Assert("Should call validation methods", DataImporter.HasErrors);
			Assert("Should call ValidateImportFilePath()", DataImporter.ImportFilePathInfo.HasErrors());
		}

		public void TestValidateImportFilePath()
		{
			AssertNoErrors("Pre-condition", DataImporter.ImportFilePathInfo);
			DataImporter.Validation.ValidateImportFilePath();
			AssertMandatoryValidationError(DataImporter.ImportFilePathInfo, true);
			DataImporter.ImportFilePath = "test";
			AssertMandatoryValidationError(DataImporter.ImportFilePathInfo, false);
			AssertHasError(DataImporter.ImportFilePathInfo, "File does not exist. Please choose a different file path.");
			string testFileName = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			try
			{
				using (File.Create(testFileName))
				{
				}

				DataImporter.ImportFilePath = testFileName;
				AssertNoErrors("Should have no errors if ImportFilePath is specified and file exists", DataImporter.ImportFilePathInfo);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		JXCDataImporterBizO DataImporter
		{
			get
			{
				if (fDataImporter == null)
				{
					fDataImporter = new JXCDataImporterBizO();
				}

				return fDataImporter;
			}
		}

		JXCDataImporterBizO fDataImporter;
	}
}
