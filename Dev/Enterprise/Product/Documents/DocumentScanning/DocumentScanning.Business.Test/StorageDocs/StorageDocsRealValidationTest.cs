using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocs))]
	sealed class StorageDocsRealValidationTest : StorageDocsTest
	{
		public void TestValidateSC_Desc()
		{
			var documentUnallocated = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			documentUnallocated.SC_Desc = "";
			Assert("Description is empty, but document is Unallocated, so no error expected", !documentUnallocated.SC_DescInfo.HasErrors());

			documentUnallocated.ParentMain.SM_ParentFK = Org.PK;
			documentUnallocated.SC_Desc = "";
			Assert("Description is empty and document is allocated, error expected", documentUnallocated.SC_DescInfo.HasErrors());

			documentUnallocated.SC_Desc = "abcdefg";
			Assert("Description is not empty, shouldn't have error", !documentUnallocated.SC_DescInfo.HasErrors());
		}

		public void TestValidateSC_DescOnDeletedDocumentShouldNotRun()
		{
			Document.ParentMain.SM_ParentFK = Org.PK;
			Document.SC_IsDeleted = true;
			Document.SC_Desc = "";
			AssertNoErrors("Empty doc but deleted - validation shouldn't run", Document.SC_DescInfo);

			Document.SC_IsDeleted = false;
			Document.SC_Desc = "";
			AssertHasErrors("Doc is empty, allocated - validation should run and must have error", Document.SC_DescInfo);

			Document.SC_Desc = "hello";
			AssertNoErrors("Description not empty, validation should run but no errors expected", Document.SC_DescInfo);

			StorageDocs documentObsolete = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			documentObsolete.SC_DocType = Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument;
			documentObsolete.SC_Desc = "";
			MasterFactory.Save();
			documentObsolete.RunPreSaveValidation();
			AssertNoErrors("Description is empty but document is obsolete - shouldn't be errors", documentObsolete.SC_DescInfo);
		}

		public void TestValidateSC_DocType()
		{
			Document.SC_DocType = "";
			AssertNoErrors("A blank doctypet is OK for an unallocated doc, no errors", Document.SC_DocTypeInfo);

			Document.ParentMain.SM_ParentFK = Org.PK;
			Document.Validation.ValidateSC_DocType();
			AssertHasErrors("A blank doctype is not OK for an allocated doc, errors expected", Document.SC_DocTypeInfo);

			Document.SC_DocType = "AAA";
			AssertHasErrors("Not using a valid doc type, should have errors", Document.SC_DocTypeInfo);

			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertNoErrors("Using a valid doc type, no errors expected", Document.SC_DocTypeInfo);

			Document.SC_DocType = Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument;
			AssertNoErrors("Using an internal doctype, validation should allow it, no errors expected", Document.SC_DocTypeInfo);

			StorageDocs documentObsolete = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			documentObsolete.SC_DocType = "";
			MasterFactory.Save();
			documentObsolete.RunPreSaveValidation();
			AssertNoErrors("Using blank doc type but obsolete, no errors expected", documentObsolete.SC_DocTypeInfo);

			Env.Security.GetDocumentTypeUploadCheckPoint("MSC").IsAllowed = false;
			Env.Security.GetDocumentTypeUploadCheckPoint("AGI").IsAllowed = false;

			Document.SC_DocType = "MSC";
			AssertHasErrors(Document.SC_DocTypeInfo);
			Document.SC_DocType = "AGI";
			AssertHasErrors(Document.SC_DocTypeInfo);

			Env.Security.GetDocumentTypeUploadCheckPoint("MSC").IsAllowed = true;
			Document.SC_DocType = "MSC";
			AssertNoErrors(Document.SC_DocTypeInfo);
		}

		public void TestValidateSC_DocTypeOnDeletedDocumentShouldNotRun()
		{
			Document.ParentMain.SM_ParentFK = Org.PK;
			Document.SC_IsDeleted = true;
			Document.SC_DocType = "AAA";
			Assert("Doctype invalid, doc is allocated, but it is deleted - validation shouldn't run", !Document.SC_DocTypeInfo.HasErrors());

			Document.SC_IsDeleted = false;
			Document.SC_DocType = "AAA";
			Assert("Doctype invalid, doc is allocated, NOT deleted - validation should run", Document.SC_DocTypeInfo.HasErrors());

			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Assert("Using valid doctype, validation should run but no errors expected", !Document.SC_DocTypeInfo.HasErrors());
		}

		public void TestValidateSC_ImageData()
		{
			Assert("Document should not have any errors on SC_ImageData, ever!", !Document.SC_ImageDataInfo.HasErrors());
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				Document.SC_ImageData = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.5pages.tif");
			}
			Assert("Document should not have any errors on SC_ImageData, ever!", !Document.SC_ImageDataInfo.HasErrors());
		}

		public void TestValidateSC_FileName()
		{
			var document = MasterFactory.NewWithParent(typeof(TempStorageDocs)) as TempStorageDocs;
			document.ParentMain.SM_ParentFK = Org.PK;

			var doc = document.ParentMain.Documents.AddNew();
			doc.SC_SM = document.ParentMain.PK;
			doc.SC_FileName = "Test name error";

			document.SC_FileNameForRenaming = "LPT1";
			AssertHasError(document.SC_FileNameForRenamingInfo, "Filename is Microsoft MS-DOS reserved. Please choose another filename.");

			document.SC_FileNameForRenaming = "Test name error";
			AssertHasError(document.SC_FileNameForRenamingInfo, "This file name already exists. Please choose another filename. If you cannot see that file, then it's probably set to visible only to specific Company / Branch / Department or unpublished.");
		}

		public void TestFilenameIsntMandatoryUnlessModified()
		{
			var document = Parent.Documents.AddNew();
			document.SC_FileName = "not empty";
			document.Validation.ValidateAll();
			AssertNoErrors(document.SC_FileNameInfo);

			document.SC_FileName = string.Empty;
			AssertHasErrorContaining(document.SC_FileNameInfo, MandatoryValidation.MustBeEntered);

			MasterFactory.Save();

			document.SC_FileName = "something else";
			document.SC_FileName = string.Empty;

			document.Validation.ValidateAll();
			AssertHasWarningContaining(document.SC_FileNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestIllegalCharactersSC_FileName()
		{
			var document = MasterFactory.NewWithParent(typeof(TempStorageDocs)) as TempStorageDocs;
			document.ParentMain.SM_ParentFK = Org.PK;

			AssertNoExceptionThrown(() => document.SC_FileNameForRenaming = "Illegal> Characters on :File /Name.doc/x");
			AssertHasError(document.SC_FileNameForRenamingInfo, "Filename contains Invalid character(s). The name should be a Windows compatible file name.");
		}

		public void TestSystemGeneratedSQTEDocType()
		{
			var document = Parent.Documents.AddNew();
			document.SC_IsSystemGenerated = false;
			document.SC_DocType = Core.Constants.RefDocTypes.SystemQuotation;
			AssertHasError("Not system generated SystemQuotation", document.SC_DocTypeInfo, "The Doc Type is only for System Use");

			var document2 = Parent.Documents.AddNew();
			document2.SC_IsSystemGenerated = true;
			document2.SC_DocType = Core.Constants.RefDocTypes.SystemQuotation;
			AssertNoErrors("System generated SystemQuotation", document2.SC_DocTypeInfo);

			var document3 = Parent.Documents.AddNew();
			document3.SC_IsSystemGenerated = true;
			document3.SC_IsPublished = true;
			document3.SC_DocType = Core.Constants.RefDocTypes.SystemQuotation;
			AssertNoErrors("Published system generated SystemQuotation", document3.SC_DocTypeInfo);

			MasterFactory.Save();

			document3.SC_DocType = Core.Constants.RefDocTypes.Quotation;
			AssertHasError("Modifying SystemQuotation", document3.SC_DocTypeInfo, "The SQTE Document Type cannot be modified");
		}
	}
}
