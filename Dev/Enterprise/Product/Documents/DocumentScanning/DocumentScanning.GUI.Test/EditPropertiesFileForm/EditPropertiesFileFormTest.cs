using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	[TestedType(typeof(EditPropertiesFileForm))]
	sealed class EditPropertiesFileFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var parent = (StorageMain)MasterFactory.NewWithValidTestData(typeof(StorageMain));
			var file = parent.Files.AddNew();
			file.SC_FileName = "hello";
			file.SC_DataType = "DOC";
			MasterFactory.Save();
			return new EditPropertiesFileForm(file);
		}

		[RequiresSTA]
		public void TestConstructorSetsSC_FileNameForRenaming()
		{
			var parent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			var file = parent.Files.AddNew();
			file.SC_FileName = "Hello";
			file.SC_DataType = "DOC";

			Assert("Precondition: should be empty", file.SC_FileNameForRenaming.IsEmpty);

			using (Form form = new EditPropertiesFileForm(file))
			{
				AssertEquals("Property should now be the same as the filename. Form constructor should set it.", file.SC_FileNameWithExtension, file.SC_FileNameForRenaming);
			}
		}

		public void TestClickOK()
		{
			var parent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			var file = parent.Files.AddNew();
			file.SC_FileName = "Hello";
			file.SC_DataType = "DOC";

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				file.SC_FileNameForRenaming = "abc.txt";
				file.SC_DocType = "CIV";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();
				AssertEquals("Should be contents of SC_FileForRenaming before '.'", "abc", file.SC_FileName);
				AssertEquals("Should be Contents of SC_FileFOrRenaming after '.'", "TXT", file.SC_DataType);
				AssertEquals("Should have set doctype", "CIV", file.SC_DocType);
			}
		}

		public void TestClickOKWithNewFileUnpublished()
		{
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var fileToSupersede = parent.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
			{
				FileName = "Hello.txt",
				DocumentType = "COO",
				ShouldSupersedeOlderVersion = false,
			});
			fileToSupersede.SC_IsPublished = true;

			var file = parent.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
			{
				FileName = "Hello.txt",
				DocumentType = "COO",
				ShouldSupersedeOlderVersion = false
			});
			file.SC_IsPublished = false;

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();
				AssertEquals("Superseded file should still be published when clicking on OK", true, fileToSupersede.SC_IsPublished);
				AssertEquals("Superseded flag should be false as the new file is not published", false, fileToSupersede.IsSupersededByNewVersion);
			}
		}

		public void TestClickOKToSupersededOldDocs()
		{
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var fileToSupersede = parent.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
			{
				FileName = "Hello.txt",
				DocumentType = "COO",
				ShouldSupersedeOlderVersion = false
			});

			var file = parent.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
			{
				FileName = "Hello.txt",
				DocumentType = "COO",
				ShouldSupersedeOlderVersion = false
			});

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();
				AssertEquals("Superseded file should be unpublished when clicking on OK", false, fileToSupersede.SC_IsPublished);
				AssertEquals("Superseded flag should be set to true if new file is published", true, fileToSupersede.IsSupersededByNewVersion);
			}
		}

		public void TestClickOKWithErrors()
		{
			var parent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var file = parent.Files.AddNew();
			file.SC_FileName = "Hello";
			file.SC_DataType = "DOC";

			var file2 = parent.Files.AddNew();
			file2.SC_FileName = "Goodbye";
			file2.SC_DataType = "DOC";

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				file.SC_FileNameForRenaming = "Goodbye.doc";
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();
				Assert("File should have errors - trying to set the file to the same as another in the collection", file.HasErrors);
				Assert("Form should still be open because of errors", form.Visible);

				file.SC_DocType = "ZZZ";
				form.OkButton.PerformClick();
				Assert("File should have errors - trying to set an invalid doctype", file.HasErrors);
				Assert("Form should stil be open because of errors", form.Visible);

				file.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				form.OkButton.PerformClick();
				Assert("File should have errors - no description for miscellaneous type", file.HasErrors);
				Assert("Form should still be open because of errors", form.Visible);

				file.SC_DocType = "CIV";
				file.SC_FileNameForRenaming = "hello.doc";
				form.OkButton.PerformClick();
				Assert("File should not have errors - trying to set the file to the same name as itself (with just casing different) should not produce an error", !file.SC_FileNameForRenamingInfo.HasErrors());
				AssertEquals("Should be contents of SC_FileForRenaming before '.'", "hello", file.SC_FileName);
				AssertEquals("Should be Contents of SC_FileFOrRenaming after '.'", "DOC", file.SC_DataType);
				AssertEquals("Should have set doctype", "CIV", file.SC_DocType);
				Assert("Form shouldn't be visible if everything was OK", !form.Visible);
			}
		}

		public void TestPreviewButton()
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var mockDocument = documentFactory.NewMoq<StorageDocs>();

			mockDocument.Setup(m => m.IsImageFile).Returns(true);
			using (var form = new EditPropertiesFileForm(mockDocument.Object))
			{
				form.Show();
				Assert("Preview button should be visible.", form.PreviewButton.Visible);
			}

			mockDocument.Setup(m => m.IsImageFile).Returns(false);
			using (var form = new EditPropertiesFileForm(mockDocument.Object))
			{
				form.Show();
				Assert("Preview button should not be visible.", !form.PreviewButton.Visible);
			}
			mockDocument.VerifyAll();
		}

		public void TestPreviewImages()
		{
			using (var bmp = new Bitmap(10, 10))
			using (var ms = new MemoryStream())
			{
				bmp.Save(ms, ImageFormat.Bmp);
				ms.Position = 0;

				var imageBytes = new byte[ms.Length];
				ms.Read(imageBytes, 0, imageBytes.Length);

				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var parent = documentFactory.New<StorageMain>();

				var document = (StorageDocs)parent.AddFileOrDocument(imageBytes, new AddFileOrDocumentDto { FileName = "test.bmp", DocumentType = "MSC" });

				using (var form = new EditPropertiesFileForm(document))
				{
					form.Show();
					form.PreviewButton.PerformClick();

					AssertEquals(typeof(GraphicalDisplayForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					form.Close();
				}
			}
		}

		public void TestClickCancelToCancelSupersedeOldDocuments()
		{
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var fileToSupersede = parent.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
			{
				FileName = "Hello",
				DocumentType = "COO",
				ShouldSupersedeOlderVersion = false
			});

			var file = parent.AddFileOrDocument(new byte[] { 1, 2, 3 }, new AddFileOrDocumentDto
			{
				FileName = "Hello",
				DocumentType = "COO",
				ShouldSupersedeOlderVersion = false
			});

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.CancelRenameButton.PerformClick();
				AssertEquals("Superseded file should still be published because cancel is clicked", true, fileToSupersede.SC_IsPublished);
				AssertEquals("Superseded flag should be false if it's not superseded", false, fileToSupersede.IsSupersededByNewVersion);
			}
		}

		public void TestClickCancel()
		{
			var parent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			var file = parent.Files.AddNew();
			file.SC_FileName = "Hello";
			file.SC_DataType = "DOC";
			file.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			file.SC_Desc = "A test description";
			file.SC_RDS_NKDocSource = "SHP";
			file.SC_IsPublished = true;
			file.IsParsingEnabled = true;
			file.SC_GC_Company = ZGuid.Empty;
			file.SC_GB_Branch = ZGuid.Empty;
			file.SC_GE_Department = ZGuid.Empty;

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				file.SC_FileNameForRenaming = "abc.txt";
				file.SC_DocType = "AAA";
				file.SC_Desc = "BBB";
				file.SC_IsPublished = false;
				file.SC_RDS_NKDocSource = ZString.Empty;
				file.IsParsingEnabled = false;
				file.SC_GC_Company = new ZGuid();
				file.SC_GB_Branch = new ZGuid();
				file.SC_GE_Department = new ZGuid();
				form.CancelRenameButton.PerformClick();
				AssertEquals("Should go back to original value", "Hello", file.SC_FileName);
				AssertEquals("Should go back to original value", "DOC", file.SC_DataType);
				AssertEquals("Should go back to original value", Core.Constants.RefDocTypes.MiscellaneousDocument, file.SC_DocType);
				AssertEquals("Should go back to original value", "A test description", file.SC_Desc);
				AssertEquals("Should go back to original value", true, file.SC_IsPublished);
				AssertEquals("Should go back to original value", true, file.IsParsingEnabledValue);
				AssertEquals("Should go back to original value", ZGuid.Empty, file.SC_GC_Company);
				AssertEquals("Should go back to original value", ZGuid.Empty, file.SC_GB_Branch);
				AssertEquals("Should go back to original value", ZGuid.Empty, file.SC_GE_Department);
				AssertEquals("Should go back to original value", "SHP", file.SC_RDS_NKDocSource);
			}

			file.SC_FileName = "Goodbye";
			file.SC_DataType = "PDF";
			file.SC_DocType = "CIV";
			file.SC_Desc = "Commercial Invoice";
			file.SC_IsPublished = false;

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				file.SC_FileNameForRenaming = "abc.txt";
				file.SC_DocType = "AAA";
				file.SC_Desc = "BBB";
				file.SC_IsPublished = true;
				form.CancelRenameButton.PerformClick();
				AssertEquals("Should go back to the last value", "Goodbye", file.SC_FileName);
				AssertEquals("Should go back to the last value", "PDF", file.SC_DataType);
				AssertEquals("Should go back to the last value", "CIV", file.SC_DocType);
				AssertEquals("Should go back to the last value", "Commercial Invoice", file.SC_Desc);
				AssertEquals("Should go back to the last value", false, file.SC_IsPublished);
			}
		}

		public void TestCreateNewDocTypeWhenAccessIsDenied()
		{
			bool previousValue = Env.Security.DocumentTypesModify.IsAllowed;

			try
			{
				Env.Security.DocumentTypesModify.IsAllowed = false;
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);

				var parent = (StorageMain)masterFactory.New(typeof(StorageMain));
				var file = parent.Files.AddNew();

				using (var form = new EditPropertiesFileForm(file))
				{
					form.Show();
					AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", form.CreateNewDocumentTypeButton.PerformClick);
				}
			}
			finally
			{
				Env.Security.DocumentTypesModify.IsAllowed = previousValue;
			}
		}

		[RequiresSTA]
		public void TestDocTypeCategoryIsSet()
		{
			var companyData = new CompanyData();

			var parent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			parent.SM_Type = Enterprise.Core.Constants.DocManagerCodes.Company;
			var file = parent.Files.AddNew();

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				try
				{
					form.CreateNewDocumentTypeButton.PerformClick();
					RefDocType newDocType = (RefDocType)((ZForm)form.RefDocTypeController.LastShownForm).BusinessEntity;
					AssertNotNull(newDocType);

					// TODO: Check this test
					AssertEquals("NewDocType should have the DocTypeCategory from the Company data, not the reference type", companyData.ReferenceType, newDocType.RT_ReferenceType);
				}
				finally
				{
					if (form.RefDocTypeController.LastShownForm != null)
					{
						form.RefDocTypeController.LastShownForm.Dispose();
					}
				}
			}
		}

		public void TestNullBusinessEntityOnClosing()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var parent = masterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Company;
			var document = parent.Documents.AddNew();

			using (var form = new EditPropertiesFileForm(document))
			{
				form.Show();
				(form as KForm).DataSource = null;
				AssertNoExceptionThrown(form.Close);
			}
		}

		public void TestFileNameAcceptsChineseCharacters()
		{
			var parent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var file = parent.Files.AddNew();
			file.SC_FileName = "Hello";
			file.SC_DataType = "DOC";
			file.SC_Desc = "test";
			file.SC_DocType = file.SC_DocType_List[0].Code;

			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				file.SC_FileNameForRenaming = "書本.txt";
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();
				AssertNoErrors(file);
				AssertEquals("Should be contents of SC_FileForRenaming before '.'", "書本", file.SC_FileName);
				AssertEquals("Should be Contents of SC_FileFOrRenaming after '.'", "TXT", file.SC_DataType);
			}
		}

		public void TestDescriptionMaxLengthBinding()
		{
			var parent = (StorageMain)MasterFactory.New(typeof(StorageMain));
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			var file = parent.Files.AddNew();
			file.SC_DataType = "MSC";
			using (var form = new EditPropertiesFileForm(file))
			{
				form.Show();
				var descTextBox = (TextBox)form.Controls.Find("SC_DescTextBox", true)[0];
				AssertEquals(StorageDocsBase.Schema.SC_DescMaxLength, descTextBox.MaxLength);
			}
		}

		public void TestIsParsingEnabled_InitAsEnabled()
		{
			TestIsParsingEnabled(true);
		}

		public void TestIsParsingEnabled_InitAsDisabled()
		{
			TestIsParsingEnabled(false);
		}

		void TestIsParsingEnabled(bool isEnabled)
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var parent = documentFactory.New<StorageMain>();
				parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

				var file = parent.Files.AddNew();
				file.SC_FileName = "Test";
				file.SC_DocType = "CIV";
				file.SC_ImageData = new byte[] { 1, 2, 3 };
				file.SC_DataType = "PDF";
				file.IsParsingEnabled = isEnabled;

				using (var form = new EditPropertiesFileForm(file))
				{
					form.Show();
					AssertEquals($"Checkbox should be {isEnabled}", isEnabled, form.isParsingEnabledCheckBox.Checked);

					form.isParsingEnabledCheckBox.Checked = !isEnabled;
					form.OkButton.PerformClick();

					AssertEquals($"IsParsingEnabled should be {isEnabled}", !isEnabled, file.IsParsingEnabled);
				}
			}
		}

		public void TestIsParsingEnabledCheckBoxDisabled_RequiredParsingType_Disabled()
		{
			// We want DocumentParsing to be enabled but the required parsing type to be disabled for this test.
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var parent = documentFactory.New<StorageMain>();
				parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

				var file = parent.Files.AddNew();
				file.SC_FileName = "Test";
				file.SC_DocType = "CIV";
				file.SC_ImageData = new byte[] { 1, 2, 3 };
				file.SC_DataType = "PDF";
				file.IsParsingEnabled = true;

				using var form = new EditPropertiesFileForm(file);
				form.Show();
				AssertEquals("Checkbox should be false", expected: false, form.isParsingEnabledCheckBox.Checked);
				AssertEquals("Checkbox should be readonly", expected: true, form.isParsingEnabledCheckBox.ReadOnly);
			}
		}

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			new DocManagerDBHelper().LastWritableDatabaseWithFreeSpace(); //to ensure SD001 exists
			MockSourceControl.Setup();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		DocumentFactory MasterFactory;

		#endregion
	}
}
