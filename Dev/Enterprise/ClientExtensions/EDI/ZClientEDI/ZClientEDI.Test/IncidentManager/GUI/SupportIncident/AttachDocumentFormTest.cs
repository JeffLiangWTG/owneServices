using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Tools.SpellCheck.TestFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(AttachDocumentForm))]
	public class AttachDocumentFormTest : BaseIncidentPopupFormTest
	{
		public void TestFormTitle()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			using (var form = new AttachDocumentForm(action, "Testing Form"))
			{
				AssertEquals(form.Text, "Testing Form");
			}
		}

		public void TestFileBrowseButton_Click_OK()
		{
			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "COR";
			docType.RT_Desc = "ALL";
			docType.RT_ReferenceType = "ALL";
			docType.RT_IsPublished = true;
			Factory.Save();
			var fileBrowseButton_ClickMethodInfo = typeof(AttachDocumentForm).GetMethod("addEdocButton_Click", BindingFlags.Instance | BindingFlags.NonPublic);
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			using (var form = new AttachDocumentForm(action, "Testing Form"))
			using (var file = TempFile.NewWithExtension("txt"))
			{
				File.AppendAllText(file.Filename, "hi");
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				fileBrowseButton_ClickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
				AssertEquals(1, incident.DocManagerInfo.AllEDocs.Count);
			}
		}

		public void TestFileBrowseButton_Click_OK_EmptyFile()
		{
			var fileBrowseButton_ClickMethodInfo = typeof(AttachDocumentForm).GetMethod("addEdocButton_Click", BindingFlags.Instance | BindingFlags.NonPublic);
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			using (var form = new AttachDocumentForm(action, "Testing Form"))
			using (var file = TempFile.NewWithExtension("txt"))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				fileBrowseButton_ClickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
				AssertEquals(0, incident.DocManagerInfo.AllEDocs.Count);
			}
		}

		public void TestFileBrowseButton_Click_OK_DangerousFile()
		{
			var fileBrowseButton_ClickMethodInfo = typeof(AttachDocumentForm).GetMethod("addEdocButton_Click", BindingFlags.Instance | BindingFlags.NonPublic);
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			using (var form = new AttachDocumentForm(action, "Testing Form"))
			using (var file = TempFile.NewWithExtension("tmp"))
			{
				File.AppendAllText(file.Filename, "hi");
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				fileBrowseButton_ClickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
				AssertEquals(0, incident.DocManagerInfo.AllEDocs.Count);
			}
		}

		public void TestFileBrowseButton_Click_Cancel()
		{
			var fileBrowseButton_ClickMethodInfo = typeof(AttachDocumentForm).GetMethod("addEdocButton_Click", BindingFlags.Instance | BindingFlags.NonPublic);
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			using (var form = new AttachDocumentForm(action, "Testing Form"))
			using (var file = TempFile.NewWithExtension("txt"))
			{
				File.AppendAllText(file.Filename, "hi");
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				fileBrowseButton_ClickMethodInfo.Invoke(form, new object[] { null, EventArgs.Empty });
				AssertEquals(0, incident.DocManagerInfo.AllEDocs.Count);
			}
		}

		public void TestSpellCheckerEnabledResolutionCommentTextBox()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "OrgOneBcbcbc Bcd";
			org1.OH_Code = "BCBCBC";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "ContactOneJkjkjk Kj";
			contact1.OC_Email = "jkjkjk@jk.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "OrgTwoHihihi Hih";
			org2.OH_Code = "HIH";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "ContactTwoXyxyxy Xyx";
			contact2.OC_Email = "xyxyxy@jk.com";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "EEF";
			staff1.GS_FullName = "StaffOneEfefef Efr";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "GFG";
			staff2.GS_FullName = "StaffTwoGoordenx";
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.FeatureRequestClientPK = org1.PK;
			incident.FeatureRequestContactPK = contact1.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Description = "Test Incident";
			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Type = "INV";
			task1.P9_Description = "Task 1";
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1.TaskProperties.ActualDate = ZDateTimeOffset.UtcNow.AddMinutes(-1);
			task1.P9_CompletedTime = ZDateTimeOffset.UtcNow;
			task1.P9_ActualDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 0, 1, 0);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.EConversation.Conversation.Staff.AddNewParticipant(staff2);
			incident.EConversation.Conversation.RelatedParties.AddNewParticipant(contact2);
			var action = new SupportIncidentCloseAction(incident);
			action.SendSoftwareQuote = true;
			Factory.Save();
			using (var form = new AttachDocumentFormForTesting(action, "TestForm"))
			{
				form.Show();
				AssertNotNull("SpellChecker field is null", form.ResolutionCommentTextBoxSpellCheckerForTest);
				AssertEquals("SpellChecker not enabled", true, form.ResolutionCommentTextBoxSpellCheckerForTest.SpellCheckerEnabled_Exposed);
				var checkSpellingMenuItem = form.ResolutionCommentTextBoxForTest.ContextMenuStrip.Items.Find("checkSpelling", false)[0];
				AssertNotNull("User should be able to use Check Spelling form", checkSpellingMenuItem);
				form.ResolutionCommentTextBoxForTest.Text = "I am ContactTwoXyxyxy Xyx from OrgTwoHihihi, you're ContactOneJkjkjk Kj from OrgOneBcbcbc Bcd. StaffOneEfefef Efr and StaffTwoGoordenx will help us.";
				using (new SpellCheckFormTestHelper(Change))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					checkSpellingMenuItem.PerformClick();
					AssertEquals("No errors found.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				form.ResolutionCommentTextBoxForTest.Text = "I can't sspell";
				using (new SpellCheckFormTestHelper(Change))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					checkSpellingMenuItem.PerformClick();
				}

				AssertEquals("I can't spell", form.ResolutionCommentTextBoxForTest.Text);
			}
		}

		SpellCheckFormAction Change(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, null);
		}

		public void TestAttachToEDocs()
		{
			var attachToEDocsMethodInfo = typeof(AttachDocumentForm).GetMethod("AttachToEDocs", BindingFlags.Instance | BindingFlags.NonPublic);
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			action.SendDevelopmentEstimate = true;
			using (var form = new AttachDocumentForm(action, "Testing Form"))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				attachToEDocsMethodInfo.Invoke(form, new object[] { "***???AAA.PDF" });
				AssertEquals("Document type SES is not found, or not active, or not published.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReportAttachmentException()
		{
			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "ALL";
			docType.RT_DocType = "SES";
			docType.RT_Desc = "Software Estimate";
			docType.RT_IsPublished = ZBool.True;
			var attachToEDocsMethodInfo = typeof(AttachDocumentForm).GetMethod("AttachToEDocs", BindingFlags.Instance | BindingFlags.NonPublic);
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			action.SendDevelopmentEstimate = true;
			using (var form = new AttachDocumentForm(action, "Testing Form"))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				attachToEDocsMethodInfo.Invoke(form, new object[] { "***???AAA.PDF" });
				AssertEquals("AttachDocumentForm - AttachToEDocs", ErrorReporter.LastKeyReported);
				AssertEquals("Failed to attach file ***???AAA.PDF to eDocs.", ErrorReporter.LastMessageReported);
#if WINZOR
				var errorMessage = "The filename, directory name, or volume label syntax is incorrect.";
				AssertContains(errorMessage, ErrorReporter.LastExceptionReported.Message);
				AssertContains(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
#else
				AssertEquals("Illegal characters in path.", ErrorReporter.LastExceptionReported.Message);
				AssertEquals("Failed to attach file ***???AAA.PDF to eDocs.", UnitTestUserNotification.Instance.LastMessage.Text);
#endif
				ErrorReporter.Clear();
			}
		}

		public void TestCloseButton_WhenSetERequestStatusOnlyFalse()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			action.SendDevelopmentEstimate = true;
			using (var form = new AttachDocumentFormForTesting(action, "Testing Form"))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CloseButtonForTest.PerformClick();

				AssertEquals("Please find estimate attached.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestSetERequestStatusOnlyShouldRemoveAndRestoreComment()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			action.SendDevelopmentEstimate = true;
			action.Comment = "Please find estimate attached.";
			using (var form = new AttachDocumentFormForTesting(action, "Testing Form"))
			{
				form.Show();
				AssertEquals("Precondition", false, action.SetERequestStatusOnly);
				AssertEquals("Precondition: Default text", "Please find estimate attached.", action.Comment);
				action.SetERequestStatusOnly = true;

				AssertEquals("Action should be erased", string.Empty, action.Comment);
				action.SetERequestStatusOnly = false;
				AssertEquals("Action should be restored", "Please find estimate attached.", action.Comment);

				action.Comment = "Lights camera action";
				action.SetERequestStatusOnly = true;

				AssertEquals("Action should be erased", string.Empty, action.Comment);
				action.SetERequestStatusOnly = false;
				AssertEquals("Action should be restored", "Lights camera action", action.Comment);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			return new AttachDocumentForm(action, "");
		}

		class AttachDocumentFormForTesting : AttachDocumentForm
		{
			public AttachDocumentFormForTesting()
			{
			}

			public AttachDocumentFormForTesting(SupportIncidentCloseAction action, string formTitle) : base(action, formTitle)
			{
			}

			public ZArchitecture.ZTextBox ResolutionCommentTextBoxForTest => ResolutionCommentTextBox;
			public SpellChecker ResolutionCommentTextBoxSpellCheckerForTest => resolutionCommentTextBoxSpellChecker;
			public ZButton CloseButtonForTest => CloseButton;
		}
	}
}
