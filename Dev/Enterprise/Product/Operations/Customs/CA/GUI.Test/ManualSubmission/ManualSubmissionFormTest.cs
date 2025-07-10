using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(ManualSubmissionForm))]
	sealed class ManualSubmissionFormTest : ZFormBasherTest
	{
		[TestDate(2017, 04, 06, 11, 00, 00)]
		public void TestSaveButtonClick()
		{
			const string officeCode = "!234";

			ManualSubmissionCancelBOTest.PrepareTestCustomOffice(officeCode, Factory);

			var declaration = GetTestDeclaration();
			var note = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.ManualSubmission.Description).FirstOrDefault();
			if (note != null)
			{
				note.Delete();
				Factory.Save();
			}

			var expectedNoteText = $@"REL
2017-04-06 10:00
{officeCode}
{Env.CurrentUser.LoginName}
2017-04-06 11:00";

			using (var form = new ManualSubmissionForm(declaration, MessageTypeList.Codes.EDIRelease, Factory))
			{
				var relCusEntryBO = form.BusinessEntity.CurrentEntrySubmissionBO;

				relCusEntryBO.PortOfClearanceOverride = officeCode;
				relCusEntryBO.ManualSubmissionDate = new ZDateTime(2017, 04, 06, 10, 00, 00);

				form.Show();
				form.ButtonSaveSubmission.PerformClick();
				note = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.ManualSubmission.Description).FirstOrDefault();

				AssertNotNull("ManualSubmission note should not be null", note);
				AssertEquals("Note Text", expectedNoteText, note.ST_NoteText.ToString().Trim());
			}
		}

		public void TestDeleteButtonClick()
		{
			const string officeCode = "!234";

			var declaration = GetTestDeclaration();
			var expectedNoteText = $@"REL
2017-04-05 14:26
{officeCode}
{Env.CurrentUser.LoginName}
2017-04-05 14:26";

			declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.ManualSubmission.Description).DeleteAll();
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.ManualSubmission.Description, expectedNoteText);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var form = new ManualSubmissionForm(declaration, MessageTypeList.Codes.EDIRelease, Factory))
			{
				form.Show();
				form.ButtonDeleteRELSubmission.PerformClick();
				var note = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.ManualSubmission.Description).FirstOrDefault();
				AssertNull("ManualSubmission note should be null", note);
			}
		}

		public void TestFormText()
		{
			var declaration = GetTestDeclaration();
			using (var form = new ManualSubmissionForm(declaration, MessageTypeList.Codes.EDIRelease, Factory))
			{
				form.Show();
				AssertEquals("Manual Submission", form.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestNoConcurrencyErrorWhenSaving()
		{
			const string officeCode = "!234";
			ManualSubmissionCancelBOTest.PrepareTestCustomOffice(officeCode, Factory);
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var declaration = GetTestDeclaration();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.TransactionNumber.AccountSecurityCode = "32450";
				declaration.TransactionNumber.SequentialNumber = "00006789";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);

				using (var form1 = new ManualSubmissionForm(declaration, MessageTypeList.Codes.B3CUSDEC, Factory))
				{
					using (var form2 = new ManualSubmissionForm(loadedDeclaration, MessageTypeList.Codes.B3CUSDEC, newFactory))
					{
						var relCusEntryBO2 = form2.BusinessEntity.CurrentEntrySubmissionBO;
						relCusEntryBO2.ManualSubmissionDate = new ZDateTime(2021, 04, 06, 09, 01, 00);
						relCusEntryBO2.PortOfClearanceOverride = officeCode;
						form2.Show();
						form2.ButtonSaveSubmission.PerformClick();
					}
					var relCusEntryBO = form1.BusinessEntity.CurrentEntrySubmissionBO;
					relCusEntryBO.ManualSubmissionDate = new ZDateTime(2021, 04, 06, 09, 02, 00);
					relCusEntryBO.PortOfClearanceOverride = officeCode;
					form1.Show();
					form1.ButtonSaveSubmission.PerformClick();
				}
			}
		}

		protected override Form GetFormToBashCore() => new ManualSubmissionForm(GetTestDeclaration(), MessageTypeList.Codes.EDIRelease, Factory);

		JobDeclaration GetTestDeclaration()
		{
			if (jobDeclaration == null)
			{
				jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var testB3CHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
				testB3CHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

				var testRELHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
				testRELHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			}

			return jobDeclaration;
		}
		JobDeclaration jobDeclaration;
	}
}
