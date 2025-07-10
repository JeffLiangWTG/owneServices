using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	/// <summary>
	/// Contains a DocumentFactory
	/// </summary>
	public class TestCaseWithDocumentFactory : TestCaseWithFactory
	{
		public TestCaseWithDocumentFactory()
		{
		}

		protected void AssertDocumentProperties(StorageDocsBase document, string expectedFileName, string expectedDocType, string expectedReferenceType, ZGuid expectedParentFK, string userInitials, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK)
		{
			AssertDocumentProperties(document, expectedFileName, expectedDocType, expectedReferenceType, expectedParentFK, userInitials);
			AssertEquals("Document's visible company should be recorded against the correct company PK", visibleCompanyPK, document.SC_GC_Company);
			AssertEquals("Document's visible branch should be recorded against the correct branch PK", visibleBranchPK, document.SC_GB_Branch);
			AssertEquals("Document's visible department should be recorded against the correct department PK", visibleDepartmentPK, document.SC_GE_Department);
		}

		protected void AssertDocumentProperties(StorageDocsBase document, string expectedFileName, string expectedDocType, string expectedReferenceType, ZGuid expectedParentFK, string userInitials)
		{
			AssertDocumentProperties(document, expectedFileName, expectedDocType, expectedReferenceType, expectedParentFK);
			AssertEquals("Document's log should be recorded against the correct staff memeber", userInitials, document.SC_SystemCreateUser);
		}

		protected void AssertDocumentProperties(StorageDocsBase document, string expectedFileName, string expectedDocType, string expectedReferenceType, ZGuid expectedParentFK)
		{
			AssertNotNull("Document shouldn't be null", document);
			AssertEquals("expected " + expectedFileName + " for File Name", expectedFileName, document.SC_FileName);
			AssertEquals("expected " + expectedDocType + " for Doc Type", expectedDocType, document.SC_DocType);

			if (expectedParentFK.IsEmpty)
			{
				AssertEquals("expected " + expectedReferenceType + " for Ref Type", expectedReferenceType, document.SC_DataType);
				AssertNull("expected Parent to be null", document.ParentMain);
			}
			else
			{
				AssertEquals("expected " + expectedReferenceType + " for Ref Type", expectedReferenceType, document.SM_Type);
				if (expectedReferenceType == "UNA")
				{
					AssertEquals("expected " + expectedParentFK + " for Parent FK", expectedParentFK, document.SC_ParentID);
				}
				else
				{
					AssertEquals("expected " + expectedParentFK + " for Parent FK", expectedParentFK, document.ParentMain.SM_ParentFK);
				}
			}
		}

		protected void AssertContentsEquals(string filenameExpected, string filenameActual)
		{
			AssertEquals(DocumentUtilities.GetFileAsBytes(filenameExpected), DocumentUtilities.GetFileAsBytes(filenameActual));
		}

		/// <summary>
		/// Creates a copy of SourceFile in DestinationFolder with the same filename.
		/// It will overwrite any existing files.
		/// The file attributes are set to normal. 
		/// </summary>
		protected string CopyFile(string sourceFile, string destinationFolder)
		{
			return CopyFile(sourceFile, Path.GetFileName(sourceFile), destinationFolder);
		}

		protected string CopyFile(string sourceFile, string newFileNameOnly, string destinationFolder)
		{
			string destinationFile = Path.Combine(destinationFolder, newFileNameOnly);
			File.Copy(sourceFile, destinationFile, true);
			File.SetAttributes(destinationFile, FileAttributes.Normal);
			return destinationFile;
		}

		#region Properties

		public DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		#endregion

		#region DBQueryHelper

		protected DocManagerDBHelper DBQueryHelper
		{
			get
			{
				if (fDBQueryHelper == null)
				{
					fDBQueryHelper = new DocManagerDBHelper();
				}
				return fDBQueryHelper;
			}
		}

		DocManagerDBHelper fDBQueryHelper;

		#endregion

		#region Overrides

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			var dBHelper = new DocManagerDBHelperTestClass();
			if (!dBHelper.DatabaseExists(1))
			{
				dBHelper.CreateDatabase(1);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearStorageDocsTables();
		}

		protected override void TearDown()
		{
			base.TearDown();
			ReleaseDocumentFactory();
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
			ReleaseDocumentFactory();
		}

		#endregion

		#region Implementation

		void ClearStorageDocsTables()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(DBQueryHelper.GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));
		}

		void ReleaseDocumentFactory()
		{
			if (fMasterFactory != null)
			{
				fMasterFactory = null;
			}
		}

		public void DeleteTempFiles()
		{
			foreach (var file in SerializableEDoc.fileNames)
			{
				if (File.Exists(file))
				{
					try
					{
						File.Delete(file);
					}
					catch (Exception) // file in use, etc. don't bother trying to handle.
					{
					}
				}
			}
			SerializableEDoc.fileNames.Clear();
		}
		#endregion
	}
}
