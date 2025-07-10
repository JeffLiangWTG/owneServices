using System.Data;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalCopy.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using MailManager;
using NUnit.Framework;
using Res = MailManager.Module.Res;

namespace Enterprise.MailManager.GUI
{
	[TestedType(typeof(MailItemFormTestHelper))]
	class MailItemFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MailItemFormTestHelper(this.Factory.New<MailItemTestHelper>());
		}

		public void TestApplicationSetOnSave()
		{
			using (MailItemFormTestHelper testForm = GetFormToBash() as MailItemFormTestHelper)
			{
				var mail = (MailItem)testForm.BusinessEntity;
				AssertEquals("STD", mail.MI_Application);
				mail.MI_Subject = "[DocManager SHP ACV SSYD54633273]";
				mail.MI_From = "a@b.com";
				mail.MI_ReplyTo = "b@c.com";
				mail.MI_Direction = "RCV";
				mail.MI_ReceivedDateTime = ZDateTime.Now;
				testForm.FireSaveButton();
				AssertEquals("DMI", mail.MI_Application);
				Assert(mail.IsInDatabase);
			}
		}

		public void TestWarningIfApplicationNotSetOnSave()
		{
			using (MailItemFormTestHelper testForm = GetFormToBash() as MailItemFormTestHelper)
			{
				var mail = (MailItem)testForm.BusinessEntity;
				AssertEquals("STD", mail.MI_Application);
				mail.MI_Subject = "dfgsagdsgsd";
				mail.MI_From = "a@b.com";
				mail.MI_ReplyTo = "b@c.com";
				mail.MI_Direction = "RCV";
				mail.MI_ReceivedDateTime = ZDateTime.Now;
				testForm.FireSaveButton();
				AssertEquals("Note that no filters matched this piece of mail, so it is unlikely to be processed; verify that this is what you wanted, then press Yes to save or No to continue modifying the email.", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
				AssertEquals("STD", mail.MI_Application);
				Assert(!mail.IsInDatabase);
			}
		}

		public void TestSaveEntireEmailToDisk()
		{
			using (MailItemFormTestHelper testForm = GetFormToBash() as MailItemFormTestHelper)
			{
				AssertNotNull("Form", testForm);
				testForm.Show();
				MailItemTestHelper testItem = testForm.Item as MailItemTestHelper;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.SaveEntireEmail();
				AssertEquals(true, testItem.WholeEmailSaved);
				AssertEquals(Res.GetString("6dc56d55-ec22-4e6e-ac3f-cf6d4bb0ab22", @"Email was successfully saved as '{0}{1}'.", Env.TempPath, "Email.eml"), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleSaveAttachment()
		{
			using (MailItemFormTestHelper testForm = GetFormToBash() as MailItemFormTestHelper)
			{
				AssertNotNull("Form", testForm);
				testForm.Show();
				MailItemTestHelper testItem = testForm.Item as MailItemTestHelper;
				AssertNotNull("MailItem", testItem);
				testItem.MailAttachments.AddNew();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.SaveOneAttachment();
				AssertEquals("Attachments were not saved", false, testItem.AttachmentsSaved);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please select Attachments to save.", UnitTestUserNotification.Instance.LastMessage.Text);
				testForm.AttachmentsGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.SaveOneAttachment();
				Assert("Attachments saved", testItem.AttachmentsSaved);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals(Res.GetString("5dc56d55-ec22-4e6e-ac3f-cf6d4bb0ab22", "Attachments were successfully saved to the {0} directory.", Env.TempPath), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUniversalCopyItemsFullPermissions()
		{
			GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			AssertNoUniversalCopy();
		}

		public void TestUniversalCopyItemsDevPermissions()
		{
			GlbStaff.CurrentUser.GS_LoginName = "random dude";
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			AssertNoUniversalCopy();
		}

		public void TestUniversalCopyItemsSupportPermissions()
		{
			GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			AssertNoUniversalCopy();
		}

		public void TestUniversalCopyItemsNoPermissions()
		{
			GlbStaff.CurrentUser.GS_LoginName = "random dude";
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			AssertNoUniversalCopy();
		}

		void AssertNoUniversalCopy()
		{
			using (MailItemFormTestHelper testForm = GetFormToBash() as MailItemFormTestHelper)
			{
				testForm.ControllerID = ControllerIDs.MailItem;
				using (var manager = new FormUniversalCopyManager(testForm))
				{
					manager.AddMenuItems();
					AssertNotNull("Form", testForm);
					testForm.Show();
					var actions = testForm.Menu.MenuItems.FindByText("Actio&ns");
					AssertNull(actions.MenuItems.FindByText("Universal Copy"));
				}
			}
		}

		public void TestHandleSaveAllAttachments()
		{
			using (MailItemFormTestHelper testForm = GetFormToBash() as MailItemFormTestHelper)
			{
				AssertNotNull("Form", testForm);
				testForm.Show();
				MailItemTestHelper testItem = testForm.Item as MailItemTestHelper;
				AssertNotNull("MailItem", testItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.SaveAllAttachments();
				AssertEquals("Attachments were not saved", false, testItem.AttachmentsSaved);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("No Attachments available", UnitTestUserNotification.Instance.LastMessage.Text);
				testItem.MailAttachments.AddNew();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.SaveAllAttachments();
				Assert("Attachments saved", testItem.AttachmentsSaved);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals(Res.GetString("5dc56d55-ec22-4e6e-ac3f-cf6d4bb0ab22", "Attachments were successfully saved to the {0} directory.", Env.TempPath), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestAddAndRemoveAttachments()
		{
			using (var tempDir = new TempDirectory())
			{
				try
				{
					using (MailItemFormTestHelper testForm = GetFormToBash() as MailItemFormTestHelper)
					{
						testForm.Show();
						tempDirectory = tempDir.DirectoryName;
						testForm.Item.MI_ReceivedDateTime = ZDateTime.Now;
						testForm.Item.MI_Direction = DirectionList.Codes.Transmit;
						testForm.AddAttachments();
						testForm.SaveForTest();
						var mailItemFromDb = Factory.LoadTop1<MailItem>(new ZQuery());
						AssertEquals(2, mailItemFromDb.MailAttachments.Count);
						AssertEquals(ZBlob.FromAscii("File one"), mailItemFromDb.MailAttachments[0].MA_Data);
						AssertEquals(ZBlob.FromAscii("File two"), mailItemFromDb.MailAttachments[1].MA_Data);
						AssertEquals(StatusCodeList.Codes.Queued, mailItemFromDb.MI_Status);
						testForm.AttachmentsGrid.Select(0);
						testForm.RemoveSelectedAttachments();
						AssertEquals(1, testForm.Item.MailAttachments.Count);
						testForm.AttachmentsGrid.SelectAllElements();
						testForm.RemoveSelectedAttachments();
						AssertEquals(0, testForm.Item.MailAttachments.Count);
						testForm.DisposeForTesting();
					}
				}
				finally
				{
					foreach (var tempFile in new DirectoryInfo(tempDir.DirectoryName).GetFiles())
					{
						tempFile.Delete();
					}
				}
			}
		}

		internal static string tempDirectory;
		#region Implementation
		class MailItemFormTestHelper : MailItemForm
		{
			public MailItemFormTestHelper(MailItem businessEntity) : base(businessEntity)
			{
			}

			internal override ZOpenFileDialog.FileInfo[] GetAttachmentFilesToAddFromUser()
			{
				file1 = TempFile.NewInDirectory(MailItemFormTest.tempDirectory);
				File.WriteAllText(file1.Filename, "File one");
				file2 = TempFile.NewInDirectory(MailItemFormTest.tempDirectory);
				File.WriteAllText(file2.Filename, "File two");
				return new ZOpenFileDialog.FileInfo[] { new ZOpenFileDialog.FileInfo(file1.Filename), new ZOpenFileDialog.FileInfo(file2.Filename) };
			}

			internal void DisposeForTesting()
			{
				file1.Dispose();
				file2.Dispose();
			}

			TempFile file1;
			TempFile file2;
			protected override ZString GetAttachmentsSavePathFromUser()
			{
				return Env.TempPath;
			}

			internal override Stream GetEmailSaveStreamFromUser(out string displayFileName)
			{
				displayFileName = Path.Combine(Env.TempPath, "Email.eml");
				return new MemoryStream();
			}

			internal void SaveForTest()
			{
				SaveInternal();
			}
		}

		class MailItemTestHelper : MailItem
		{
			public MailItemTestHelper(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void SaveAttachmentsTo(MailAttachment[] attachments, string destinationDir)
			{
				AttachmentsSaved = true;
			}

			public override void SaveEntireEmailAsEml(Stream targetFileStream)
			{
				WholeEmailSaved = true;
			}

			public bool AttachmentsSaved;
			public bool WholeEmailSaved;
		}
		#endregion
	}
}
