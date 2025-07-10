using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentScanning.Business.Testing
{
	sealed class DocumentSplitConfigInfoValidationTest : TestCaseWithFactory
	{
		DocumentSplitConfigInfo GetNewDocumentSplitConfigInfo()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
				doc.SC_DataType = Core.Constants.FileFormats.PDF;
				doc.SC_ImageData = new ZBlob(resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.PdfFileHavingThreePages.pdf"));
				var splitManager = new DocumentSplitManager(doc);

				return splitManager.DocumentSplitConfigCollection.AddNew();
			}
		}

		DocumentFactory MasterFactory
		{
			get
			{
				return masterFactory ?? (masterFactory = new DocumentFactoryProvider().GetFactory(Factory));
			}
		}

		DocumentFactory masterFactory;

		public void TestValidateDocumentName_InvalidCharacter()
		{
			var documentSplitConfigInfo = GetNewDocumentSplitConfigInfo();
			documentSplitConfigInfo.DocumentName = "123er";

			AssertNoErrors(documentSplitConfigInfo.DocumentNameInfo);

			documentSplitConfigInfo.DocumentName = "1234/df";

			AssertHasError(documentSplitConfigInfo.DocumentNameInfo, "Filename contains Invalid character(s). The name should be a Windows compatible file name.");
		}

		public void TestValidateDocumentName_MaximumLength()
		{
			var documentSplitConfigInfo = GetNewDocumentSplitConfigInfo();
			documentSplitConfigInfo.DocumentName = new ZString('1', 256);

			AssertNoErrors(documentSplitConfigInfo.DocumentNameInfo);

			documentSplitConfigInfo.DocumentName = new ZString('1', 257);

			AssertHasError(documentSplitConfigInfo.DocumentNameInfo, "This filename is longer than 256 characters.");
		}

		public void TestValidateDocumentName_WindowsReservedName()
		{
			var documentSplitConfigInfo = GetNewDocumentSplitConfigInfo();
			documentSplitConfigInfo.DocumentName = "LPT5";

			AssertNoErrors(documentSplitConfigInfo.DocumentNameInfo);

			documentSplitConfigInfo.DocumentName = "LPT2";

			AssertHasError(documentSplitConfigInfo.DocumentNameInfo, "Filename is Microsoft MS-DOS reserved. Please choose another filename.");
		}

		public void TestValidateDocumentType()
		{
			var obj = GetNewDocumentSplitConfigInfo();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			obj.DocumentType = "";

			obj.SplitManager.DocumentToSplit.ParentMain.SM_ParentFK = org.PK;
			obj.Validation.ValidateDocumentType();
			AssertHasError("A blank doc type is not OK for an allocated doc, errors expected", obj.DocumentTypeInfo, "Please enter a value.");

			obj.DocumentType = "AAA";
			AssertHasError("Not using a valid doc type, should have errors", obj.DocumentTypeInfo, "Enter a valid selection.");

			obj.DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertNoErrors("Using a valid doc type, no errors expected", obj.DocumentTypeInfo);

			obj.DocumentType = Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument;
			AssertNoErrors("Using an internal doc type, validation should allow it, no errors expected", obj.DocumentTypeInfo);

			Env.Security.GetDocumentTypeUploadCheckPoint("MSC").IsAllowed = false;
			Env.Security.GetDocumentTypeUploadCheckPoint("AGI").IsAllowed = false;

			obj.DocumentType = "MSC";
			AssertHasError(obj.DocumentTypeInfo, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> Allocate eDocs -> eDocs Tab - Upload Specific Document Type -> MSC");

			obj.DocumentType = "AGI";
			AssertHasError(obj.DocumentTypeInfo, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> Allocate eDocs -> eDocs Tab - Upload Specific Document Type -> AGI");

			Env.Security.GetDocumentTypeUploadCheckPoint("MSC").IsAllowed = true;
			obj.DocumentType = "MSC";
			AssertNoErrors(obj.DocumentTypeInfo);
		}

		public void TestValidateStartPage()
		{
			var obj = GetNewDocumentSplitConfigInfo();
			obj.EndPage = 2;
			obj.StartPage = 1;
			AssertNoErrors(obj.StartPageInfo);
			obj.StartPage = 3;
			AssertHasError(obj.StartPageInfo, "Start page number should be smaller than end page number.");
			AssertHasError(obj.EndPageInfo, "End page number should be bigger than start page number.");
			obj.StartPage = 5;
			AssertHasError(obj.StartPageInfo, "Page number should be inside the page range (1 to 3) of the source document");
		}

		public void TestValidateEndPage()
		{
			var obj = GetNewDocumentSplitConfigInfo();
			obj.EndPage = 3;
			obj.StartPage = 2;
			AssertNoErrors(obj.EndPageInfo);
			obj.EndPage = 1;
			AssertHasError(obj.StartPageInfo, "Start page number should be smaller than end page number.");
			AssertHasError(obj.EndPageInfo, "End page number should be bigger than start page number.");
			obj.EndPage = 11;
			AssertHasError(obj.EndPageInfo, "Page number should be inside the page range (1 to 3) of the source document");
		}
	}
}
