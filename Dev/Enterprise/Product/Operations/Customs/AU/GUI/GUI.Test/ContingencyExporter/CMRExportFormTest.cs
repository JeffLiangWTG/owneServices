using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(CMRExportForm))]
	sealed class CMRExportFormTest : ZFormBasherTest
	{
		public void TestShowingCompulsoryTextRequiredInDeclarationContingency()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();

			var exporter = new DeclarationIMDExporter(declaration);
			using (var testForm = new TestCMRExportForm(null, exporter))
			{
				using (var dir = new TempDirectory())
				{
					string fullPath = "";
					try
					{
						testForm.NextSaveFolderDialogName = dir.DirectoryName;
						testForm.FormResult = DialogResult.No;
						testForm.Export();
						Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(CMRExportForm.CompulsoryTextRequiredInDeclarationContingency));
					}
					finally
					{
						if (File.Exists(fullPath))
						{
							File.Delete(fullPath);
						}
					}
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			var anotherExporter = new DummyExporter(dummyBizObj);
			using (var testForm = new TestCMRExportForm(null, anotherExporter))
			{
				using (var dir = new TempDirectory())
				{
					string fullPath = "";
					try
					{
						testForm.NextSaveFolderDialogName = dir.DirectoryName;
						testForm.FormResult = DialogResult.No;
						testForm.Export();
						Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(CMRExportForm.CompulsoryTextRequiredInDeclarationContingency));
					}
					finally
					{
						if (File.Exists(fullPath))
						{
							File.Delete(fullPath);
						}
					}
				}
			}
		}

		public void TestSaveFiredWhenBizObjChanged()
		{
			var dummyBizOjb = Factory.New<DummyBusinessObject>();
			dummyBizOjb.HasChanges = true;
			DummyExporter exporter = new DummyExporter(dummyBizOjb);
			using (var parentForm = new ZForm(dummyBizOjb))
			{
				using (var exportForm = new TestCMRExportForm(parentForm, exporter))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					exportForm.Export();
					AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestExportAndSave()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			DummyExporter exporter = new DummyExporter(Factory.New<DummyBusinessObject>());
			using (TestCMRExportForm form = new TestCMRExportForm(null, exporter))
			{
				using (TempDirectory dir = new TempDirectory())
				{
					form.NextSaveFolderDialogName = dir.DirectoryName;
					string fullPath = "";
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					try
					{
						form.FormResult = DialogResult.No;
						AssertEquals("Pre-condition", 0, exporter.numOfeDocsExposedToTest);
						fullPath = form.Export();
						AssertEquals("1 doc attached to eDocs", 1, exporter.numOfeDocsExposedToTest);
						AssertEquals("An output file should be generated but and NOT DELETED as the user chose to save the file", true, File.Exists(fullPath));
						AssertEquals("No email sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
						AssertEquals("Message text descriptive", true, UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Contingency Data saved to '"));
					}
					finally
					{
						if (File.Exists(fullPath))
						{
							File.Delete(fullPath);
						}
					}
				}
			}
		}

		[TestDate(2007, 3, 5, 7, 30, 1, 50)]
		public void TestExportAndEmail()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			DummyExporter exporter = new DummyExporter(Factory.New<DummyBusinessObject>());
			using (TestCMRExportForm form = new TestCMRExportForm(null, exporter))
			{
				string fullPath = "";
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				try
				{
					form.FormResult = DialogResult.Yes;
					AssertEquals("Pre-condition", 0, exporter.numOfeDocsExposedToTest);
					fullPath = form.Export();
					AssertEquals("1 doc attached to eDocs", 1, exporter.numOfeDocsExposedToTest);
					AssertEquals("An output file should be generated but then DELETED as the emailing was successful", false, File.Exists(fullPath));
					AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
					AssertEquals("Contingency Test Report", email.Subject);
					AssertEquals("zubin@example.com;zubin1@example.com", email.Recipients[0].Email);
					AssertEquals("ZUBIN073001DummySuffix.csv", email.Attachments[0].DisplayName);
					AssertEquals("This is the body text", email.Body);
				}
				finally
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					if (File.Exists(fullPath))
					{
						File.Delete(fullPath);
					}
				}
			}
		}

		public void TestDontSaveFileIfUserPressesCancel()
		{
			DummyExporter exporter = new DummyExporter(Factory.New<DummyBusinessObject>());
			using (TestCMRExportForm form = new TestCMRExportForm(null, exporter))
			{
				form.NextSaveFolderDialogName = Env.TempPath;
				form.NextSaveFolderDialogResponse = DialogResult.Cancel;
				AssertEquals("Pre-condition", 0, exporter.numOfeDocsExposedToTest);
				string fileName = Path.GetFileName(form.Export());
				AssertEquals("0 doc attached to eDocs", 0, exporter.numOfeDocsExposedToTest);
				AssertEquals("An output file should NOT be generated as the user cancelled the file save", false, File.Exists(Path.Combine(form.NextSaveFolderDialogName, fileName)));
			}
		}

		protected override Form GetFormToBashCore() => new CMRExportForm(null, new DummyExporter(Factory.New<DummyBusinessObject>()));

		protected override void SetUp()
		{
			base.SetUp();
			currentyCompanyABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "89 123 441 321";
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = currentyCompanyABN;
			base.TearDown();
		}

		string currentyCompanyABN;

		sealed class TestCMRExportForm : CMRExportForm
		{
			public TestCMRExportForm(ZForm parentForm, CMRDataExporterCSV exporter) : base(parentForm, exporter)
			{
			}

			string nextSaveFolderDialogName;
			public string NextSaveFolderDialogName
			{
				get
				{
					if (nextSaveFolderDialogName != null)
					{
						return nextSaveFolderDialogName;
					}
					else
					{
						throw new Exception("Value must be assigned to NextSaveFolderDialogName before testing");
					}
				}

				set => nextSaveFolderDialogName = value;
			}

			public DialogResult NextSaveFolderDialogResponse = DialogResult.OK;

			protected override DialogResult ShowFolderBrowseDialogToUser(ZFolderBrowserDialog dialog)
			{
				dialog.SelectedPath = NextSaveFolderDialogName;
				return NextSaveFolderDialogResponse;
			}

			protected override string[] GetEmailRecipients() => new string[] { "zubin@example.com;zubin1@example.com" };
		}
	}
}
