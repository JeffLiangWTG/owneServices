using System;
using System.Data;
using System.IO;
using CargoWise.Async;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageFile))]
	public class StorageFileTest : StorageDocsBaseTest
	{
		public class StorageFileForTesting : StorageFile, IIsLaunched
		{
			public StorageFileForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsLaunched { get; private set; }

			protected override void LaunchProcess()
			{
				IsLaunched = true;
			}
		}

		protected override StorageDocsBase GetNewTestBizO(NumberedBusinessObjectFactory factory)
		{
			StorageFile file = StorageFile.NewWithParent_DEBUG(factory);
			file.SC_DataType = "PDF";
			return file;
		}

		protected override StorageDocsBase GetNewTestBizOForSave(NumberedBusinessObjectFactory factory)
		{
			return GetNewTestBizO(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory seperateFactory)
		{
			return Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return StorageFile.NewWithParent_DEBUG(MasterFactory);
		}

		public void TestNewWithParent()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			AssertNotNull("New File should exist", newFile);
			AssertNotNull("New File should have a parent object", newFile.ParentMain);
			AssertEquals("NewFile should be using the same factory that was passed in", MasterFactory, newFile.Factory);
		}

		public void TestNew()
		{
			StorageFile newFile = StorageFile.New_DEBUG(MasterFactory);
			AssertNotNull("File should exist", newFile);
			AssertNull("New file doesn't have a parent object", newFile.ParentMain);
			AssertEquals("File should be using the same factory that was passed in", MasterFactory, newFile.Factory);
		}

		public void TestParentMainType()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("Parent should always be a StorageMain", typeof(StorageMain), newFile.ParentMain.GetType());
		}

		#region Properties

		public void TestSC_ImageData()
		{
			using (StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory))
			{
				newFile.SC_FileName = "Small";
				newFile.SC_DataType = "TIF";
				newFile.SC_ImageData = SmallTiffBytes;
				newFile.SaveToTempFile();

				string tempFilePath = newFile.TempFileName;
				AssertEquals("Temp file should exist", true, File.Exists(newFile.TempFileName));

				newFile.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");
				AssertEquals("Setting the image blob should delete the temp file", false, File.Exists(tempFilePath));
				Assert("TempFileName on the file object should be empty", newFile.TempFileName.IsEmpty);
			}
		}

		public void TestSC_FileNameForRenaming()
		{
			using (StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory))
			{
				Assert("SC_Filename for renaming empty by default", newFile.SC_FileName.IsEmpty);
				newFile.SC_FileName = "hello";
				newFile.SC_DataType = "PDF";
				newFile.SetDefaultSC_FileNameForRenaming();

				AssertEquals("hello.pdf", newFile.SC_FileNameForRenaming);
			}
		}

		public void TestSC_FileNameForRenamingInfo()
		{
			using (StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory))
			{
				AssertEquals("length should accept max length for filename + '.' + extension", StorageDocsSchema.SC_FileName.MaxLength + StorageDocsSchema.SC_DataType.MaxLength + 1, newFile.SC_FileNameForRenamingInfo.MaxLength);
			}
		}

		public void TestValidateSC_FileNameForRenamingAfterRenamingHasBeenDoneOnce()
		{
			using (StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory))
			{
				newFile.SC_FileName = "hello";
				newFile.SC_DataType = "DOC";

				Factory.Save();

				// user has already renamed it once
				newFile.SC_FileName = "Banana";
				newFile.SC_DataType = "XLS";

				newFile.SC_FileNameForRenaming = "apple";
				AssertHasErrorContaining(newFile.SC_FileNameForRenamingInfo, "You must specify a file extension");

				newFile.SC_FileNameForRenaming = ".doc";
				AssertHasErrorContaining(newFile.SC_FileNameForRenamingInfo, "You must specify a valid file name with extension");

				newFile.SC_FileNameForRenaming = "apple.";
				AssertHasErrorContaining(newFile.SC_FileNameForRenamingInfo, "You must specify a valid file name with extension");

				newFile.SC_FileNameForRenaming = "<hasta la vista, baby>.txt";
				AssertHasErrorContaining(newFile.SC_FileNameForRenamingInfo, "Filename contains Invalid character(s). The name should be a Windows compatible file name.");

				newFile.SC_FileNameForRenaming = "test.txt";
				Assert("Shouldn't have any errors - valid file", !newFile.SC_FileNameForRenamingInfo.HasErrors());

				newFile.SC_FileNameForRenaming = "hello.txt";
				AssertHasWarningContaining(newFile.SC_FileNameForRenamingInfo, "Changing the file extension from 'XLS' may make the file unusable.");

				newFile.SC_FileNameForRenaming = "non-European" + (char)257 + ".xls";
				AssertNoErrors(newFile.SC_FileNameForRenamingInfo);
			}
		}

		public void TestValidateSC_FileNameForRenamingWithCollection()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			StorageFile file1 = parent.Files.AddNew();
			file1.SC_FileName = "hello";
			file1.SC_DataType = "doc";

			StorageFile newFile = parent.Files.AddNew();
			newFile.SC_FileName = "Hello";
			newFile.SC_DataType = "DOC";
			newFile.SetDefaultSC_FileNameForRenaming();
			newFile.Validation.ValidateSC_FileNameForRenaming();
			Assert("File should have errors - 'hello.doc' already exits in the collection", newFile.SC_FileNameForRenamingInfo.HasErrors());
			newFile.SC_FileNameForRenaming = "Hello.pdf";
			Assert("File should not have errors - 'hello.pdf' is unique", !newFile.SC_FileNameForRenamingInfo.HasErrors());

			file1.SetDefaultSC_FileNameForRenaming();
			file1.SC_FileName = "Hello";
			newFile.Validation.ValidateSC_FileNameForRenaming();
			Assert("File should NOT have errors - this file is already 'hello.doc' in the collection so it can be renamed to the same thing with case change", !newFile.SC_FileNameForRenamingInfo.HasErrors());

			newFile.SC_FileNameForRenaming = "<hasta la vista, baby>.txt";
			newFile.Validation.ValidateSC_FileNameForRenaming();
			AssertHasErrorContaining(newFile.SC_FileNameForRenamingInfo, "Filename contains Invalid character(s). The name should be a Windows compatible file name.");
		}

		public void TestSC_FilenameWithExtensionInfo()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			int expectedMaxLength = newFile.SC_FileNameInfo.MaxLength + newFile.SC_DataTypeInfo.MaxLength + 1;
			AssertEquals("Filename max length", expectedMaxLength, newFile.SC_FileNameWithExtensionInfo.MaxLength);
		}

		public void TestSC_FilenameInfo()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("Filename info should always be readonly", true, newFile.SC_FileNameInfo.ReadOnly);
		}

		public void TestSC_DateInfo()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("Date info should always be readonly, you should never be able to modify the date", true, newFile.SC_DateInfo.ReadOnly);
		}

		public void TestValidateSC_DocType()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parent.SM_DB = 1;
			StorageFile file1 = parent.Files.AddNew();

			file1.SC_DocType = "AAA";
			AssertHasErrors("should error because it is not a valid code", file1.SC_DocTypeInfo);

			file1.SC_DocType = "";
			AssertHasErrors("should error - empty is not a valid state", file1.SC_DocTypeInfo);

			file1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertNoErrors("if a valid doc type is supplied, should be no error", file1.SC_DocTypeInfo);

			file1.SC_DocType = "";
			MasterFactory.Save();
			file1.RunPreSaveValidation();
			AssertNoErrors("Should not have error - empty type is allowed in obsolete file", file1.SC_DocTypeInfo);

			parent.SM_DB = 0;
			StorageFile file2 = parent.Files.AddNew();
			AssertEquals("File2 should belong in db 0", MasterFactory, file2.Factory);
			file2.SC_DocType = "AAA";
			AssertNoErrors("invalid type should not error - db0 does not validate", file2.SC_DocTypeInfo);
			file2.SC_DocType = "";
			AssertNoErrors("empty string should not error - db0 does not validate", file2.SC_DocTypeInfo);
			file2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			AssertNoErrors("valid doc type - should not error", file2.SC_DocTypeInfo);
		}

		public void TestValidateSC_ImageData()
		{
			int oldValue = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value;

			try
			{
				SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

				using (StorageFileForTesting newFile = MasterFactory.New<StorageFileForTesting>())
				{
					newFile.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.2MB.dat");
					Assert("Document should have error because blob file size is too big", newFile.SC_ImageDataInfo.HasErrors());

					SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
					newFile.Validation.ValidateSC_ImageData();
					Assert("Document shouldn't have error because blob file size is under registry specified limit", !newFile.SC_ImageDataInfo.HasErrors());
				}
			}
			finally
			{
				SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);
			}
		}

		#endregion

		#region Open in Filesystem

		protected override StorageDocsBase GetObjectForTestingOpeningFile()
		{
			var doc = MasterFactory.NewWithParent(typeof(StorageFileForTesting));
			doc.ParentMain.SM_DB = 1;
			return doc;
		}

		// This test replicates the saving behavior of Office 2007.
		public void TestOpenForEditAndOnSavingViaRename()
		{
			using (var syncContext = SynchronizationContextForTest.Enable())
			using (StorageFileForTesting file = MasterFactory.NewWithParent(typeof(StorageFileForTesting)) as StorageFileForTesting)
			{
				file.ParentMain.SM_DB = 1;
				file.SC_FileName = "Original";
				MasterFactory.Save();
				AssertEquals("Precondition: HasChanges", false, file.HasChanges);

				using (file.OpenForEdit())
				{
					AssertEquals("HasChanges should not be set by opening for editing in Excel 2007.", false, file.HasChanges);

					string directory = Path.GetDirectoryName(file.TempFileName);
					string backupFile = Path.Combine(directory, "Backup.tmp");
					string newFile = Path.Combine(directory, "New.tmp");

					File.WriteAllText(newFile, "Moo");
					File.Move(file.TempFileName, backupFile);
					File.Move(newFile, file.TempFileName);
					File.Delete(backupFile);
					Assert("Expected watcher_changed event to be raised", syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1)));
					AssertEquals("HasChanges should be set when template data is read back in triggered by Excel 2007 saving the Template.", true, file.HasChanges);
				}
			}
		}

		#endregion

		#region IDeliverable Members

		protected override void AssertDeliveryInfo(StorageDocsBase doc, DeliveryInfo info)
		{
			AssertEquals("AttachedFilename", doc.SC_FileName, info.AttachedFilename);
			AssertEquals("DeliveryFormat", DeliveryInfo.DeliveryFormats.File, info.DeliveryFormat);
		}

		public override void TestDeliveryMode()
		{
			AssertEquals("DeliveryMode", "EML,EPR", ((IDeliverable)GetNewBusinessObject()).DeliveryMode);

			var nonHPRefDoctype = Factory.New<RefDocType>();
			nonHPRefDoctype.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			nonHPRefDoctype.RT_DocType = "ZZZ";
			nonHPRefDoctype.RT_Desc = "Test for Non HP Raw";
			nonHPRefDoctype.RT_IsActive = true;
			nonHPRefDoctype.RT_IsSystem = true;
			nonHPRefDoctype.RT_HPPclPrintFile = false;

			var hPRefDoctype = Factory.New<RefDocType>();
			hPRefDoctype.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			hPRefDoctype.RT_DocType = "YYY";
			hPRefDoctype.RT_Desc = "Test for HP Raw";
			hPRefDoctype.RT_IsActive = true;
			hPRefDoctype.RT_IsSystem = true;
			hPRefDoctype.RT_HPPclPrintFile = true;
			Factory.Save();

			var file = (StorageFile)GetNewBusinessObject();
			file.SC_DocType = nonHPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "EML,EPR", file.DeliveryMode);

			file.SC_DocType = hPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "PRN", file.DeliveryMode);

			hPRefDoctype.RT_IsActive = false;
			Factory.Save();
			file.SC_DocType = hPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "EML,EPR", file.DeliveryMode);

			hPRefDoctype.RT_IsActive = true;
			hPRefDoctype.RT_IsSystem = false;
			file.SC_DocType = hPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "EML,EPR", file.DeliveryMode);

			file.SC_DocType = null;
			file.SC_DataType = FileFormats.PDF;
			AssertEquals("DeliveryMode", "PRN,EPR,EML", file.DeliveryMode);

			file.SC_DataType = "pDf";
			AssertEquals("DeliveryMode", "PRN,EPR,EML", file.DeliveryMode);
		}

		public override void TestGetSupportedDeliveryMethodsCore()
		{
			var doctype = Factory.New<RefDocType>();
			doctype.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doctype.RT_DocType = "ZZZ";
			doctype.RT_Desc = "Test for Non HP Raw";
			doctype.RT_IsActive = true;
			doctype.RT_IsSystem = true;
			doctype.RT_HPPclPrintFile = true;
			Factory.Save();

			var file = (StorageFile)GetNewBusinessObject();
			file.SC_DataType = FileFormats.PDF;

			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Print, ContactNotifyModes.EPrint, ContactNotifyModes.Email }, file.GetSupportedDeliveryMethods());

			file.SC_DocType = doctype.RT_DocType;
			file.SC_DataType = FileFormats.XLS;

			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Print }, file.GetSupportedDeliveryMethods());

			doctype.RT_HPPclPrintFile = false;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Email, ContactNotifyModes.EPrint, ContactNotifyModes.Print }, file.GetSupportedDeliveryMethods());

			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			var eDoc = menu.EDocsView.AddNew();
			eDoc.SX_RT_DocType = doctype.PK;
			eDoc.SX_PrintCopyType = nameof(PrintCopyType.EML);
			((IDeliverable)file).MenuItem = menu;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Email }, file.GetSupportedDeliveryMethods());

			eDoc.SX_PrintCopyType = nameof(PrintCopyType.ALL);
			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Email, ContactNotifyModes.EPrint, ContactNotifyModes.Print }, file.GetSupportedDeliveryMethods());
		}

		public override void TestSupportsDeliveryMethod()
		{
			var file = (StorageFile)GetNewBusinessObject();
			file.SC_DataType = FileFormats.PDF;

			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.Email));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Electronic));
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.EPrint));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Fax));
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.Print));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Ftp));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.DoNotDeliver));

			var doctype = Factory.New<RefDocType>();
			doctype.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doctype.RT_DocType = "ZZZ";
			doctype.RT_Desc = "Test for Non HP Raw";
			doctype.RT_IsActive = true;
			doctype.RT_IsSystem = true;
			doctype.RT_HPPclPrintFile = true;
			file.SC_DocType = doctype.RT_DocType;
			file.SC_DataType = FileFormats.XLS;
			Factory.Save();

			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Email));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Electronic));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.EPrint));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Fax));
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.Print));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Ftp));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.DoNotDeliver));

			doctype.RT_HPPclPrintFile = false;
			Factory.Save();
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.Email));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Electronic));
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.EPrint));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Fax));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Ftp));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.DoNotDeliver));

			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			var eDoc = menu.EDocsView.AddNew();
			eDoc.SX_RT_DocType = doctype.PK;
			eDoc.SX_PrintCopyType = nameof(PrintCopyType.EML);
			((IDeliverable)file).MenuItem = menu;
			Factory.Save();
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.Email));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Electronic));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.EPrint));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Fax));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Print));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Ftp));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.DoNotDeliver));

			eDoc.SX_PrintCopyType = nameof(PrintCopyType.ALL);
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.Email));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Electronic));
			Assert(file.SupportsDeliveryMethod(ContactNotifyModes.EPrint));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Fax));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.Ftp));
			Assert(!file.SupportsDeliveryMethod(ContactNotifyModes.DoNotDeliver));
		}

		public override void TestGetSupportedDeliveryMethodDispiteOfPrintCopyType()
		{
			var doctype = Factory.New<RefDocType>();
			doctype.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doctype.RT_DocType = "ZZZ";
			doctype.RT_Desc = "Test for Non HP Raw";
			doctype.RT_IsActive = true;
			doctype.RT_IsSystem = true;
			doctype.RT_HPPclPrintFile = true;
			Factory.Save();

			var file = (StorageFile)GetNewBusinessObject();
			file.SC_DataType = FileFormats.PDF;

			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Print, ContactNotifyModes.EPrint, ContactNotifyModes.Email }, file.GetSupportedDeliveryMethodDespiteOfPrintCopyType());

			file.SC_DocType = doctype.RT_DocType;
			file.SC_DataType = FileFormats.XLS;

			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Print }, file.GetSupportedDeliveryMethodDespiteOfPrintCopyType());

			doctype.RT_HPPclPrintFile = false;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Email, ContactNotifyModes.EPrint, ContactNotifyModes.Print }, file.GetSupportedDeliveryMethodDespiteOfPrintCopyType());

			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			var eDoc = menu.EDocsView.AddNew();
			eDoc.SX_RT_DocType = doctype.PK;
			eDoc.SX_PrintCopyType = nameof(PrintCopyType.EML);
			((IDeliverable)file).MenuItem = menu;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new string[] { ContactNotifyModes.Email, ContactNotifyModes.EPrint, ContactNotifyModes.Print }, file.GetSupportedDeliveryMethodDespiteOfPrintCopyType());
		}

		public override void TestFileExtension()
		{
			StorageFile file = (StorageFile)GetNewBusinessObject();
			file.SC_DataType = "txt";
			AssertEquals("FileExtension", "txt", ((IDeliverable)file).FileExtension);
		}

		public override void TestAllAvailableDeliveryModes()
		{
			AssertEquals("DeliveryMode", "EML,EPR", ((IDeliverable)GetNewBusinessObject()).AllAvailableDeliveryModes);

			var nonHPRefDoctype = Factory.New<RefDocType>();
			nonHPRefDoctype.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			nonHPRefDoctype.RT_DocType = "ZZZ";
			nonHPRefDoctype.RT_Desc = "Test for Non HP Raw";
			nonHPRefDoctype.RT_IsActive = true;
			nonHPRefDoctype.RT_IsSystem = true;
			nonHPRefDoctype.RT_HPPclPrintFile = false;

			var hPRefDoctype = Factory.New<RefDocType>();
			hPRefDoctype.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			hPRefDoctype.RT_DocType = "YYY";
			hPRefDoctype.RT_Desc = "Test for HP Raw";
			hPRefDoctype.RT_IsActive = true;
			hPRefDoctype.RT_IsSystem = true;
			hPRefDoctype.RT_HPPclPrintFile = true;
			Factory.Save();

			var file = (StorageFile)GetNewBusinessObject();
			file.SC_DocType = nonHPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "EML,EPR", file.AllAvailableDeliveryModes);

			file.SC_DocType = hPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "PRN", file.AllAvailableDeliveryModes);

			hPRefDoctype.RT_IsActive = false;
			Factory.Save();
			file.SC_DocType = hPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "EML,EPR", file.AllAvailableDeliveryModes);

			hPRefDoctype.RT_IsActive = true;
			hPRefDoctype.RT_IsSystem = false;
			file.SC_DocType = hPRefDoctype.RT_DocType;
			AssertEquals("DeliveryMode", "EML,EPR", file.AllAvailableDeliveryModes);

			file.SC_DocType = null;
			file.SC_DataType = FileFormats.PDF;
			AssertEquals("DeliveryMode", "PRN,EPR,EML", file.AllAvailableDeliveryModes);

			file.SC_DataType = "pDf";
			AssertEquals("DeliveryMode", "PRN,EPR,EML", file.AllAvailableDeliveryModes);
		}

		public void TestAllAvailableDelieryModesForExcel()
		{
			var file = (StorageFile)GetNewBusinessObject();

			file.SC_DataType = FileFormats.XLS;
			AssertEquals("DeliveryMode", "PRN,EPR,EML", file.AllAvailableDeliveryModes);

			file.SC_DataType = FileFormats.XLSX;
			AssertEquals("DeliveryMode", "PRN,EPR,EML", file.AllAvailableDeliveryModes);

			file.SC_DocType = "QRP";
			AssertEquals("DeliveryMode", "PRN", file.AllAvailableDeliveryModes);
		}

		public override void TestName()
		{
			var file = (StorageFile)GetNewBusinessObject();
			file.SC_DataType = "txt";
			file.SC_FileName = "passwords";
			AssertEquals("Name", "passwords.txt", ((IDeliverable)file).Name);
		}

		public override void TestBindingName()
		{
			var file = (StorageFile)GetNewBusinessObject();
			file.SC_DataType = "txt";
			file.SC_FileName = "passwords";
			AssertEquals("BindingName", "passwords.txt", ((IDeliverable)file).NameForBinding);
		}

		#endregion

		public void TestSaveToFilesystem_StripsOutIllegalCharacters()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			newFile.SC_FileName = "C:\\<yoloswag>"; // Testing for illegal characters in filename
			newFile.SC_DataType = "TIF";
			newFile.SC_ImageData = SmallTiffBytes;
			try
			{
				newFile.SaveToTempFile();
				AssertEquals("NewFile's temp filename should strip out the illegal characters", "C   yoloswag ", Path.GetFileNameWithoutExtension(newFile.TempFileName));
			}
			finally
			{
				if (File.Exists(newFile.TempFileName))
				{
					File.Delete(newFile.TempFileName);
				}
			}
		}

		public void TestSetImageData()
		{
			using (TempFile tempFile = TempFile.NewWithExtension("tif"))
			using (StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory))
			{
				var testDocPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

				newFile.SC_FileName = "abc";
				newFile.SC_DataType = "TIF";
				newFile.SC_ImageData = DocumentUtilities.GetFileAsBytes(tempFile.Filename);
				newFile.SaveToTempFile();

				AssertEquals("Nothing in SC_ImageData", 0, newFile.SC_ImageData.Length);

				File.Copy(testDocPath, newFile.TempFileName, true);
				File.SetAttributes(newFile.TempFileName, FileAttributes.Normal);

				newFile.SetImageData();
				Assert("SC_ImageData should have been set; should not be empty", newFile.SC_ImageData.Length > 0);

				MasterFactory.Save();
				DocumentFactory newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var fileInSecondFactory = newFactory.Load<StorageFile>(newFile.PK);
				DataRow row = ((INeedRow)fileInSecondFactory).Row;
				AssertEquals(true, fileInSecondFactory.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
				fileInSecondFactory.Validation.ValidateSC_ImageData();
				AssertEquals(true, fileInSecondFactory.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
				ZBlob blob = fileInSecondFactory.SC_ImageData;
				Assert(blob.Length > 0);
				AssertEquals(false, fileInSecondFactory.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
			}
		}

		[TestDate(2024, 11, 11, 11, 11, 11)]
		public void TestSaveStorageFileShouldUpdateSC_Date()
		{
			var doc = PrepareTestDocument(1, [1, 2, 3]);
			MasterFactory.Save();

			var originalSC_Date = doc.SC_Date;
			TestDateAttribute.AddMinutes(10);
			AssertEquals("Original SC_Date", originalSC_Date, doc.SC_Date);

			var newSC_Date = TestDateAttribute.Date;
			SetUpS3Registries();
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(true, true))
			{
				var result = doc.SC_ImageData;

				AssertEquals("The content from S3 file should not be empty", false, result.IsEmpty);
				doc.SC_ImageData = new ZBlob([1, 2, 3, 4]);
				MasterFactory.Save();
				AssertEquals("Should update SC_Date", newSC_Date, doc.SC_Date);
			}
		}

		[ExpectNoExceptions]
		public void TestOpenForEditWithWindowsReservedFileNameThrowNoException()
		{
			using (StorageFile document = StorageFile.NewWithParent_DEBUG(MasterFactory))
			{
				ZDateTime originalDate = new ZDateTime(2004, 12, 10);

				string fileName = "PRN";

				document.SC_FileName = fileName;
				document.SC_DocType = "ACV";
				document.SC_DataType = "MSG";
				document.SC_ImageData = SmallTiffBytes;
				document.SC_IsPublished = false;
				document.SC_Date = originalDate;
				document.ReadOnly = false;

				MasterFactory.Save();
				Assert("No temp file should be stored yet", document.TempFileName.IsEmpty);
				AssertEquals("HasChanges should be false", false, document.HasChanges);

				document.ReadOnly = false;
				using (document.OpenForEdit())
				{
					AssertEquals("SC_Date should not have changed", originalDate, document.SC_Date);
					Assert("HasChanges should be false on the object", !document.HasChanges);
					Assert("Should now be a temp file stored", !document.TempFileName.IsEmpty);
				}
			}
		}

		public void TestSetNewFilename()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			Assert("Precondition: filename is default", newFile.SC_FileName.IsDefault);
			Assert("Precondition: extension is default", newFile.SC_FileName.IsDefault);

			newFile.SetNewFileName("hello", "doc");

			AssertEquals("Should be set to the new info", "hello", newFile.SC_FileName);
			AssertEquals("should be set to the new info - uppercase extension", "DOC", newFile.SC_DataType);
			Assert("No notification on filename for renaming property", !newFile.SC_FileNameForRenamingInfo.HasErrors());
		}

		public void TestSetNewFilenameWithFilenameOnly()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			Assert("Precondition: filename is default", newFile.SC_FileName.IsDefault);
			Assert("Precondition: extension is default", newFile.SC_FileName.IsDefault);

			newFile.SetNewFileName("hello.doc");
			AssertEquals("Should be set to the new info", "hello", newFile.SC_FileName);
			AssertEquals("should be set to the new info - uppercase extension", "DOC", newFile.SC_DataType);
			Assert("No notification on filename for renaming property", !newFile.SC_FileNameForRenamingInfo.HasErrors());
		}

		public void TestSetNewFilenameReplacesIllegalCharactersWithDash()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			Assert("Precondition: filename is default", newFile.SC_FileName.IsDefault);
			Assert("Precondition: extension is default", newFile.SC_FileName.IsDefault);

			AssertNoExceptionThrown(() => { newFile.SetNewFileName("He<re>Ar:eS\"om/eI\\ll|eg?al*Characters.doc"); });
			AssertEquals("Should be set to the new info", "He-re-Ar-eS-om-eI-ll-eg-al-Characters", newFile.SC_FileName);
			AssertEquals("should be set to the new info - uppercase extension", "DOC", newFile.SC_DataType);
			Assert("No notification on filename for renaming property", !newFile.SC_FileNameForRenamingInfo.HasErrors());
		}

		public void TestGetExtensionFromFilename()
		{
			var maxLength = StorageDocsSchema.SC_DataType.MaxLength;
			AssertEquals("Extension portion should be returned, in uppercase", "DOC", StorageFile.GetExtensionFromFilename("hello.doc"));
			AssertEquals("Extension portion should be returned, in uppercase", "PDF", StorageFile.GetExtensionFromFilename("hello.pdf"));
			AssertEquals("no extension, should return empty string", "", StorageFile.GetExtensionFromFilename("hello"));
			AssertEquals("Extension over the max length should truncate", ZString.Replicate('A', maxLength), StorageFile.GetExtensionFromFilename("hello." + ZString.Replicate('a', maxLength + 1)));
			AssertEquals("Extension should be returned", "XLSX", StorageFile.GetExtensionFromFilename("generic.xlsx.file.xlsx"));
			AssertEquals("Extension should be returned", "XLSX", StorageFile.GetExtensionFromFilename("The current version of UAT Alpha is 18.10.14.7 with a database schema of 3596.0.xlsx"));
		}

		public void TestGetFileNameOnlyFromFilename()
		{
			int maxLength = StorageDocsSchema.SC_FileName.MaxLength;
			AssertEquals("filename portion should be returned in same casing", "Hello", StorageFile.GetFileNameOnlyFromFilename("Hello.doc"));
			AssertEquals("filename portion should be returned in same casing", "Goodbye", StorageFile.GetFileNameOnlyFromFilename("Goodbye.doc"));
			AssertEquals("no filename, should return empty string", "", StorageFile.GetFileNameOnlyFromFilename(".doc"));

			string longFilename = ZString.Replicate('a', maxLength + 1) + ".txt";
			string expectedFilename = ZString.Replicate('a', maxLength);
			AssertEquals("Filename over the max length should truncate", ZString.Replicate('a', maxLength), StorageFile.GetFileNameOnlyFromFilename(longFilename));

			longFilename = ZString.Replicate('a', maxLength - 1) + " some more words.txt";
			expectedFilename = ZString.Replicate('a', maxLength - 1);
			AssertEquals("Filename over the max length with a space as last character should truncate", expectedFilename, StorageFile.GetFileNameOnlyFromFilename(longFilename));
		}

		public void TestIsTifFile()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("StorageFile should always return false for IsTifFile", false, newFile.IsImageFile);
		}

		public void TestDispose()
		{
			StorageFile newFile;
			using (newFile = StorageFile.NewWithParent_DEBUG(MasterFactory))
			{
				newFile.SC_FileName = "Small";
				newFile.SC_ImageData = SmallTiffBytes;
				newFile.SaveToTempFile();
				AssertEquals("File should exist on the filesystem.", true, File.Exists(newFile.TempFileName));
				using (newFile.OpenForEdit())
				{
					File.WriteAllText(newFile.TempFileName, "Something.");
				}
			}
			AssertEquals("File should have been cleaned up on dispose.", false, File.Exists(newFile.TempFileName));
		}

		public void TestSC_DocType_List()
		{
			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			AssertNotNull("doc type list shouldn't be null if parent doesn't have a reference type", newFile.SC_DocType_List);

			newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			newFile.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			AssertNotNull("doc type list", newFile.SC_DocType_List);
			Assert("doc type list should have elements", newFile.SC_DocType_List.Count > 0);
		}

		public void TestValidateSC_Desc()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parent.SM_DB = 1;

			StorageFile newFile = parent.Files.AddNew();
			newFile.SC_DocType = Core.Constants.RefDocTypes.CommercialInvoice;

			ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);
			query.AddToFilter(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.CommercialInvoice);
			RefDocType docType = MasterFactory.LoadTop1<RefDocType>(query);

			AssertEquals("Should default to the description for doctype CIV", docType.RT_Desc, newFile.SC_Desc);
			AssertNoErrors("shouldn't have any errors", newFile.SC_DescInfo);

			newFile.SC_Desc = ZString.Empty;
			AssertHasErrors("should have errors - desc can't be empty if a doctype is assigned", newFile.SC_DescInfo);

			newFile.SC_DocType = "";
			newFile.SC_Desc = "";
			AssertHasErrors("should have errors - desc can't be empty", newFile.SC_DescInfo);

			newFile.SC_Desc = "";
			MasterFactory.Save();
			newFile.RunPreSaveValidation();
			AssertNoErrors("Shouldn't have errors - desc can be empty if the file is obsolete", newFile.SC_DescInfo);

			parent.SM_DB = 0;
			StorageFile file2 = parent.Files.AddNew();
			AssertEquals("File2 should belong in db 0", MasterFactory, file2.Factory);
			file2.SC_DocType = "CIV";
			AssertEquals("shouldn't have any errors (should default to description for doctype CIV)", false, file2.SC_DescInfo.HasErrors());

			file2.SC_Desc = "";
			AssertEquals("empty string should not error - db0 does not validate", false, file2.SC_DocTypeInfo.HasErrors());

			file2.SC_DocType = "";
			file2.SC_Desc = "";
			AssertEquals("empty string and empty doctype should not error = db0 does not validate", false, file2.SC_DocTypeInfo.HasErrors());
		}

		public void TestAddDocType()
		{
			RefDocType newDocType = Factory.New<RefDocType>();
			newDocType.RT_DocType = "000";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			newDocType.RT_Desc = "000 Doc Type";

			StorageFile newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			newFile.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			int initialDocTypeCount = newFile.SC_DocType_List.Count;

			newFile.AddDocType(newDocType);

			AssertEquals("Should be one more document type in the list", initialDocTypeCount + 1, newFile.SC_DocType_List.Count);
			AssertEquals("The new doc type should be first in the list alphabetically", newDocType.RT_DocType, newFile.SC_DocType_List[0].Code);
		}

		public void TestSC_DescriptionForWeb()
		{
			StorageMain parent = MasterFactory.New<StorageMain>();
			StorageFile newFile = parent.Files.AddNew();
			newFile.SC_FileName = "Test";
			newFile.SC_DataType = "PNG";

			AssertEquals("Should be the filename and extension", "Test.png", newFile.SC_DescriptionForWeb);

			newFile.SC_DataType = "ABC";
			AssertEquals("Should be the filename and extension not cached", "Test.abc", newFile.SC_DescriptionForWeb);
		}

		public void TestGetFileNameOnlyWithoutExtensionForSavingToFileSystem()
		{
			var newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			newFile.SC_FileName = "hi!*<> there?";

			AssertEquals("Should have all illegal characters stripped out", "hi!    there ", newFile.GetFileNameOnlyWithoutExtension());
		}

		public void TestGetFileNameOnlyWithExtensionForSavingToFileSystem()
		{
			var newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			newFile.SC_FileName = "hi!*<> there?";

			AssertEquals("Should have all illegal characters stripped out", "hi!    there .tif", newFile.GetFileNameOnlyWithExtension());
		}

		public void TestLock()
		{
			StorageFile file1 = StorageFile.NewWithParent_DEBUG(MasterFactory);
			StorageFile file2 = StorageFile.NewWithParent_DEBUG(MasterFactory);

			using (IDisposable lock1 = file1.Lock())
			{
				Assert("Should lock file", file1.IsLocked);
				Assert("Should not touch other file", !file2.IsLocked);

				AssertNotNull("Should lock file", lock1);
				AssertNull("No double locking", file1.Lock());

				using (IDisposable lock2 = file2.Lock())
				{
					Assert("Should lock file", file2.IsLocked);
					Assert("Should keep other locks", file1.IsLocked);

					AssertNotNull("Should lock different files simultaneously", lock2);
					AssertNull("No double locking", file2.Lock());
				}

				Assert("Should release lock", !file2.IsLocked);
			}
			Assert("Should release lock", !file1.IsLocked);

			using (IDisposable lock1 = file1.Lock())
			{
				Assert("Should lock file", file1.IsLocked);
				AssertNotNull("Should lock file after it was released", lock1);
			}
			Assert("Should release lock", !file1.IsLocked);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			return factory;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] SmallTiffBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		public override bool ExpectedTIF
		{
			get { return false; }
		}

		public override bool ExpectedJPG
		{
			get { return false; }
		}

		public override bool ExpectedTXT
		{
			get { return false; }
		}
	}
}
