using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Test
{
	[TestedType(typeof(UpgradeForm))]
	class UpgradeFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestSendButtonText()
		{
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());

			using (UpgradeForm form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();

				AssertEquals("DefaultRadioButton.Checked", true, form.DefaultRadioButton.Checked);
				AssertEquals("SendButton.Text", "Send", form.SendButton.Text);

				form.SaveToDiskRadioButton.Checked = true;
				AssertEquals("SendButton.Text", "Save", form.SendButton.Text);

				form.SaveToDiskRadioButton.Checked = false;
				AssertEquals("SendButton.Text", "Send", form.SendButton.Text);
			}
		}

		public void TestSend()
		{
			TestSendNoMessage();
			TestSendPreUpgradeError();
			TestSendPromptUpgradeConfirmation();
			TestSendPostUpgradeError();
			TestSendPromptUpgradeResult();
		}

		void TestSendNoMessage()
		{
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());

			using (UpgradeFormTestHelper form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(true, UpgradeRequestCollectionContainer.NoBuildSelectedMessage);
				form.SendButton.PerformClick();
				AssertEquals("LastMessage should be an error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("LastMessage.Text", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Upgrade.Send() should not be called.", false, dummy.IsSendCalled);
				AssertEquals("Upgrade.SaveToDisk() should not be called.", false, dummy.IsSaveToDiskCalled);
				AssertNull("EmailContactForm should not be shown.", form.LastEmailForm);
				AssertEquals("Form should not be closed.", true, form.Visible);
			}
		}

		void LoadBuild(UpgradeRequestCollectionContainerTestHelper dummy, bool active = true)
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			HttpDownload httpVersion = new HttpDownload();
			build.HL_MajorVersion = httpVersion.VersionMajorNumber;
			build.HL_MinorVersion = httpVersion.VersionMinorNumber;
			build.HL_Release = httpVersion.VersionReleaseNumber;
			build.HL_Superceded = false;
			build.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			build.HL_IsActive = active;
			dummy.ReleaseBuildPK = build.PK;
		}

		void TestSendPreUpgradeError()
		{
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());

			using (UpgradeFormTestHelper form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				LoadBuild(dummy);
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(true, "Error!");
				form.SendButton.PerformClick();
				AssertEquals("LastMessage should be an error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("LastMessage.Text", "Error!", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Upgrade.Send() should not be called.", false, dummy.IsSendCalled);
				AssertEquals("Upgrade.SaveToDisk() should not be called.", false, dummy.IsSaveToDiskCalled);
				AssertNull("EmailContactForm should not be shown.", form.LastEmailForm);
				AssertEquals("Form should not be closed.", true, form.Visible);
			}
		}

		void TestSendPromptUpgradeConfirmation()
		{
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());

			using (UpgradeFormTestHelper form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				LoadBuild(dummy);
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(false, "No Error!");
				form.SendButton.PerformClick();
				AssertEquals("LastMessage should be a question.", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				string expectedMessage = "No Error!\r\n\r\nTotal number of upgrade requests to add: 1.\r\n\r\nNumber of upgrade requests to download via Http: 1.\r\n\r\nDo you want to proceed with the upgrade?";
				AssertEquals("LastMessage.Text", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Upgrade.Send() should not be called.", false, dummy.IsSendCalled);
				AssertEquals("Upgrade.SaveToDisk() should not be called.", false, dummy.IsSaveToDiskCalled);
				AssertNull("EmailContactForm should not be shown.", form.LastEmailForm);
				AssertEquals("Form should not be closed.", true, form.Visible);
			}
		}
		void TestSendPostUpgradeError()
		{
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());

			using (UpgradeFormTestHelper form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();
				UserIdleWorker.Flush();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				LoadBuild(dummy);
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(false, string.Empty);
				dummy.DummySendResult = dummy.CreatePostUpgradeStatus(true, "Error sending upgrade!");
				form.SendButton.PerformClick();
				AssertEquals("LastMessage should be an error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("LastMessage.Text", "Error sending upgrade!", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Upgrade.Send() should be called.", true, dummy.IsSendCalled);
				AssertEquals("Upgrade.SaveToDisk() should not be called.", false, dummy.IsSaveToDiskCalled);
				AssertNull("EmailContactForm should not be shown.", form.LastEmailForm);
				AssertEquals("Form should be closed.", false, form.Visible);
			}
		}

		void TestSendPromptUpgradeResult()
		{
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());

			using (UpgradeFormTestHelper form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				LoadBuild(dummy);
				EDIOrgHeader client = Factory.New<EDIOrgHeader>();
				CustomerServiceEmail email = new CustomerServiceEmail(client);
				dummy.IsSendCalled = false;
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(false, string.Empty);
				dummy.DummySendResult = dummy.CreatePostUpgradeStatus(false, "Upgrade sent!", email);
				form.SendButton.PerformClick();

				using (EmailContactForm emailForm = form.LastEmailForm)
				{
					AssertEquals("EmailForm.BusinessEntity", email, emailForm.BusinessEntity);
				}

				AssertEquals("LastMessage should be an info message.", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("LastMessage.Text", "Upgrade sent!\r\n\r\nTotal number of notification emails sent: 1\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Upgrade.Send() should be called.", true, dummy.IsSendCalled);
				AssertEquals("Upgrade.SaveToDisk() should not be called.", false, dummy.IsSaveToDiskCalled);
				AssertEquals("Form should be closed.", false, form.Visible);
			}
		}

		public void TestSaveToDisk()
		{
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());
			dummy.IsSaveToDisk = true;

			using (UpgradeForm form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dummy.IsSendCalled = false;
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(false, "");
				dummy.DummySaveToDiskResult = dummy.CreatePostUpgradeStatus(false, "Upgrade saved!");
				form.SendButton.PerformClick();
				AssertEquals("LastMessage should be an info message.", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("LastMessage.Text", "Upgrade saved!\r\n\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Upgrade.Send() should not be called.", false, dummy.IsSendCalled);
				AssertEquals("Upgrade.SaveToDisk() should be called.", true, dummy.IsSaveToDiskCalled);
				AssertEquals("Form should be closed.", false, form.Visible);
			}
		}

#if WINZOR
		public void TestSaveAndSend()
		{
			var fileService = new Moq.Mock<WinzorFramework.JSInterop.IFileService>();
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());
			dummy.IsSaveToDisk = true;
			using (UpgradeForm form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();
				form.CargoWiseClientServices.FileService = fileService.Object;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dummy.IsSendCalled = false;
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(false, "");
				
				using var file = ZArchitecture.Core.TempFile.New();
				dummy.DummySaveToDiskResult = dummy.CreateSaveToDiskStatus(false, file.Filename);
				form.SendButton.PerformClick();
				AssertEquals("File is saved to temp folder on server", EnvProxy.Instance.TempPath, dummy.saveToDiskDirectory);
				Assert("Temp file deleted after send", !System.IO.File.Exists(file.Filename));
				var remotePath = System.IO.Path.Combine(UpgradeFormTestHelper.DestinationDirectory, System.IO.Path.GetFileName(file.Filename));
				fileService.Verify(s => s.SaveFileByPathAsync(remotePath, Moq.It.IsAny<byte[]>()), Moq.Times.Once());
			}
		}
#endif

		public void TestSaveToDiskThrowArgumentException()
		{
			var dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, GetNewOrganisation());
			dummy.IsSaveToDisk = true;

			using (var form = new UpgradeFormTestHelper(dummy))
			{
				form.Show();
				form.throwArgExceptionCounter = 3;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dummy.IsSendCalled = false;
				dummy.DummyPreUpgradeStatus = dummy.CreatePreUpgradeStatus(false, "");
				dummy.DummySaveToDiskResult = dummy.CreatePostUpgradeStatus(false, "Upgrade saved!");
				try
				{ form.SendButton.PerformClick(); }
				catch { }

				AssertEquals("LastMessage should be an info message.", false, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("LastMessage.Text", "Selected directory is inaccessible or otherwise invalid. Please select another location or try again", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Upgrade.Send() should not be called.", false, dummy.IsSendCalled);
				AssertEquals("Upgrade.SaveToDisk() should be called.", false, dummy.IsSaveToDiskCalled);
				AssertEquals("Form should be closed.", true, form.Visible);
			}
		}

		public void TestUpgradeDeliveryInstruction()
		{
			EDIOrgHeader headerForTest = GetNewOrganisation();
			UpgradeRequestCollectionContainerTestHelper dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, headerForTest);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (UpgradeForm form = new UpgradeFormTestHelper(dummy))
			{
				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.Show();

				AssertNull("No messages should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			headerForTest.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Delivery Instruction");
			dummy = new UpgradeRequestCollectionContainerTestHelper(Factory, headerForTest);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (UpgradeForm form = new UpgradeFormTestHelper(dummy))
			{
				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.Show();

				AssertEquals("LastMessage should be a question.", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("LastMessage.Text", "Delivery Instruction", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			using (UpgradeForm form = new UpgradeFormTestHelper(dummy))
			{
				AssertNull("Precondition: No messages should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.Show();

				AssertEquals("LastMessage should be information.", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("LastMessage.Text", "Selected organisations do not support upgrades.\r\nPlease, change your selection.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new UpgradeFormTestHelper(new UpgradeRequestCollectionContainer(Factory, GetNewOrganisation()));
		}

		EDIOrgHeader GetNewOrganisation()
		{
			EDIOrgHeader organisation = Factory.New<EDIOrgHeader>();
			organisation.OH_Code = "ABCSYD";
			organisation.Contacts.AddNew();
			organisation.Contacts[0].OC_Email = "Test.Contact@edi.com";

			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = organisation.PK;
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();

			organisation.LicCompany.LC_LE = licEnt.PK;
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_LE = organisation.LicEnterprise.PK;
			licDatabase.LD_OC_ContractInstallerOrInternalTechContact = organisation.Contacts[0].PK;

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			HttpDownload httpVersion = new HttpDownload();
			build.HL_MajorVersion = httpVersion.VersionMajorNumber;
			build.HL_MinorVersion = httpVersion.VersionMinorNumber;
			build.HL_Release = httpVersion.VersionReleaseNumber;
			licDatabase.LD_HL_CurrentRunningVersion = build.PK;
			licDatabase.LD_PublicEmailAddressForUpdate = "test@cargowise.com";

			LicenceHeader header = organisation.LicCompany.LicHeadersForAllDatabases.AddNew();
			header.LA_LC = organisation.LicCompany.PK;
			header.LA_LD = licDatabase.PK;

			return organisation;
		}

		#region UpgradeFormTestHelper

		class UpgradeFormTestHelper : UpgradeForm
		{
			public const string DestinationDirectory = "DummyPath";
			EmailContactForm lastEmailForm;

			public UpgradeFormTestHelper(UpgradeRequestCollectionContainer upgrades)
				: base(upgrades)
			{
			}

			public EmailContactForm LastEmailForm
			{
				get { return lastEmailForm; }
			}

			protected override bool AskForDestinationFolder(out string destinationDirectory)
			{
				destinationDirectory = DestinationDirectory;
				if (throwArgExceptionCounter > 0)
				{
					throwArgExceptionCounter--;
					#pragma warning disable CA2208
					throw new ArgumentException(); // throwing any exception with a message will destroy the second assert in TestSaveToDiskThrowArgumentException
					#pragma warning restore CA2208
				}
				return true;
			}

			public int throwArgExceptionCounter;

			protected override void ShowEmailForm(EmailContactForm form)
			{
				if (LastEmailForm != null)
				{
					LastEmailForm.Dispose();
				}
				lastEmailForm = form;
			}
		}

		#endregion

		#region class UpgradeRequestCollectionContainerTestHelper

		class UpgradeRequestCollectionContainerTestHelper : UpgradeRequestCollectionContainer
		{
			public UpgradeRequestCollectionContainerTestHelper(BusinessObjectFactory factory, params EDIOrgHeader[] organisations)
				: base(factory, organisations)
			{
			}

			public override IPreUpgradeStatus GetPreUpgradeStatus()
			{
				return DummyPreUpgradeStatus;
			}

			public override IPostUpgradeStatus PlaceUpgradesToClients()
			{
				IsSendCalled = true;
				return DummySendResult;
			}

			public override IPostUpgradeStatus SaveToDisk(string saveToDiskDirectory)
			{
				IsSaveToDiskCalled = true;
				this.saveToDiskDirectory = saveToDiskDirectory;
				return DummySaveToDiskResult;
			}

			public IPreUpgradeStatus CreatePreUpgradeStatus(bool isError, string message)
			{
				UpgradeStatus status = new UpgradeStatus(isError, message);
				if (!isError && Upgrades.Count > 0)
				{
					status.upgradesToSendViaHttp.Add(this.Upgrades[0]);
				}
				return status;
			}

			public IPostUpgradeStatus CreatePostUpgradeStatus(bool isError, string message, params CustomerServiceEmail[] emails)
			{
				UpgradeStatus status = new UpgradeStatus(isError, message);
				status.notificationEmails.AddRange(emails);
				return status;
			}

			public IPostUpgradeStatus CreateSaveToDiskStatus(bool isError, string packagePath)
			{
				UpgradeStatus status = new UpgradeStatus(isError, string.Empty)
				{
					PackagePath = packagePath
				};
				return status;
			}

			public bool IsSendCalled;
			public bool IsSaveToDiskCalled;
			public string saveToDiskDirectory;
			public IPreUpgradeStatus DummyPreUpgradeStatus;
			public IPostUpgradeStatus DummySendResult;
			public IPostUpgradeStatus DummySaveToDiskResult;
		}

		#endregion

		#endregion
	}
}
