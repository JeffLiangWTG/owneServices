using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class StorageFileValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSC_FileNameForRenamingNotValidatedThroughValidateAll()
		{
			var factory = new DocumentFactoryProvider().GetFactory(Factory);
			var main = factory.New<StorageMain>();
			var file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_Desc = "test";
			file.SC_DocType = file.SC_DocType_List[0].Code;
			file.SC_FileName = "zubin.xls";
			file.SC_ImageData = ZBlob.FromAscii("this is a test");
			file.Validation.ValidateAll();
			AssertNoErrors("SC_FileNameForRenaming should not be validated when saving the object", file);
		}

		public void TestValidateFileNameUniqueInCollection()
		{
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain main = factory.New<StorageMain>();
			StorageFile oldFile = (StorageFile)main.eDocs.AddNew(typeof(StorageFile));
			oldFile.SC_FileName = "kalos";
			oldFile.SC_DataType = "DOC";

			StorageFile file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin";
			file.SC_FileNameForRenaming = "kalos.doc";
			file.SC_ImageData = ZBlob.FromAscii("this is a test");
			file.Validation.ValidateSC_FileNameForRenaming();
			AssertHasError(file.SC_FileNameForRenamingInfo, "This file name already exists. Please choose another filename. If you cannot see that file, then it's probably set to visible only to specific Company / Branch / Department or unpublished.");
		}

		public void TestValidateFileName_TooLong()
		{
			const string filenameLongerThan255 = "A very long filename 123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.doc";
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain main = factory.New<StorageMain>();
			StorageFile file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin";
			file.SC_DataType = "DOC";
			file.SC_FileNameForRenaming = filenameLongerThan255;

			AssertHasError(file.SC_FileNameForRenamingInfo, "This filename is longer than 255 characters");
		}

		public void TestValidateFileName_IsDangerousFile()
		{
			const string filenameLongerThan255AndIsExe = "A very long filename 123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.exe";
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain main = factory.New<StorageMain>();
			StorageFile file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin";
			file.SC_DataType = "DOC";
			file.SC_FileNameForRenaming = filenameLongerThan255AndIsExe;

			AssertHasError(file.SC_FileNameForRenamingInfo, "This filename is longer than 255 characters");

			const string filenameShorterThanOrEqual255AndIsExe = "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111.exe";
			file.SC_FileNameForRenaming = filenameShorterThanOrEqual255AndIsExe;

			AssertNoError(file.SC_FileNameForRenamingInfo, "This filename is longer than 255 characters");
			AssertHasError(file.SC_FileNameForRenamingInfo, $"The file cannot be renamed to {filenameShorterThanOrEqual255AndIsExe} as it is a potentially dangerous file type.");
		}

		public void TestValidateFileName_ExtensionTooLong()
		{
			const string filenameExtensionLongerThan20 = "A filename .abcdefghijklmnopqrstuvwxyz";
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain main = factory.New<StorageMain>();

			StorageFile file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin";
			file.SC_FileNameForRenaming = filenameExtensionLongerThan20;
			AssertHasWarning(file.SC_FileNameForRenamingInfo, "The filename extension is longer than 20 characters");
		}

		public void TestValidateFileName_AcceptableExtension()
		{
			var filenameExtensionNotAcceptable = "sango.exe";
			var factory = new DocumentFactoryProvider().GetFactory(Factory);
			var main = factory.New<StorageMain>();

			var file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "sango";
			file.SC_FileNameForRenaming = filenameExtensionNotAcceptable;
			AssertHasErrorContaining(file.SC_FileNameForRenamingInfo, @"The file cannot be renamed to sango.exe as it is a potentially dangerous file type.");
		}

		public void TestValidateFileName_ExtensionWithMultipleDots()
		{
			const string filenameExtensionMultipleDots = "A filename.tar.gz";
			var factory = new DocumentFactoryProvider().GetFactory(Factory);
			var main = factory.New<StorageMain>();

			var file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin";
			file.SC_DataType = "GZ";
			file.SC_FileNameForRenaming = filenameExtensionMultipleDots;

			AssertNoErrors(file.SC_FileNameForRenamingInfo);
			AssertNoWarnings(file.SC_FileNameForRenamingInfo);
		}

		public void TestSC_DocType()
		{
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(Factory);
			StorageMain main = factory.New<StorageMain>();
			StorageFile file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin.xls";
			file.SC_ImageData = ZBlob.FromAscii("this is a test");
			main.SM_DB = 1;

			var code1 = file.SC_DocType_List[0].Code;
			var code2 = file.SC_DocType_List[1].Code;

			Env.Security.GetDocumentTypeUploadCheckPoint(code1).IsAllowed = false;
			Env.Security.GetDocumentTypeUploadCheckPoint(code2).IsAllowed = false;

			file.SC_DocType = code1;
			AssertHasErrors(file.SC_DocTypeInfo);
			file.SC_DocType = code2;
			AssertHasErrors(file.SC_DocTypeInfo);

			Env.Security.GetDocumentTypeUploadCheckPoint(code1).IsAllowed = true;
			file.SC_DocType = code1;
			AssertNoErrors(file.SC_DocTypeInfo);
		}

		public void TestSC_DocTypeShouldNotValidateIfNoChanges()
		{
			var factory = new DocumentFactoryProvider().GetFactory(Factory);
			var source = factory.NewWithValidTestData<RefDocSource>();
			source.RDS_IsActive = true;
			source.RDS_Code = "ABA";
			var main = factory.New<StorageMain>();
			var file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin.xls";
			file.SC_ImageData = ZBlob.FromAscii("this is a test");
			main.SM_DB = 1;

			var code1 = file.SC_DocType_List[0].Code;

			Env.Security.GetDocumentTypeUploadCheckPoint(code1).IsAllowed = true;
			file.SC_DocType = code1;
			AssertNoErrors(file.SC_DocTypeInfo);
			factory.Save();

			Env.Security.GetDocumentTypeUploadCheckPoint(code1).IsAllowed = false;

			var newFactory = new BusinessObjectFactory();
			var newDocFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var reloadedFile = newDocFactory.Load<StorageFile>(file.PK);

			reloadedFile.Validation.ValidateSC_DocType();
			AssertNoErrors(reloadedFile.SC_DocTypeInfo);
		}

		public void TestSC_RDS_NKDocSourceShouldNotValidateIfNoChanges()
		{
			var factory = new DocumentFactoryProvider().GetFactory(Factory);
			var source = factory.NewWithValidTestData<RefDocSource>();
			source.RDS_IsActive = true;
			source.RDS_Code = "ABA";
			var main = factory.New<StorageMain>();
			var file = factory.New<StorageFile>();
			file.SC_SM = main.PK;
			file.SC_FileName = "zubin.xls";
			file.SC_ImageData = ZBlob.FromAscii("this is a test");
			main.SM_DB = 1;

			var code1 = file.SC_DocType_List[0].Code;

			Env.Security.GetDocumentTypeUploadCheckPoint(code1).IsAllowed = true;
			file.SC_DocType = code1;
			file.SC_RDS_NKDocSource = "ABA";
			AssertNoErrors(file.SC_RDS_NKDocSourceInfo);
			factory.Save();

			source.RDS_IsActive = false;
			factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newDocFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var reloadedFile = newDocFactory.Load<StorageFile>(file.PK);

			reloadedFile.Validation.ValidateSC_RDS_NKDocSource();
			AssertNoErrors(reloadedFile.SC_RDS_NKDocSourceInfo);
		}
	}
}
