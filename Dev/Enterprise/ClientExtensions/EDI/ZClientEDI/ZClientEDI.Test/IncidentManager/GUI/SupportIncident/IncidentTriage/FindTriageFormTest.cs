using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(FindTriageForm))]
	public class FindTriageFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var helper = new FindTriageFilterHelper(incident);
			return new FindTriageForm(helper);
		}

		public void TestAddToMessageButton_Click_NotPublished()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem1.IMC_IsPublished = true;
			checklistItem1.PublishedDescriptionText = "Number";
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem2.IMC_IsPublished = false;
			checklistItem2.PublishedDescriptionText = "Letter";
			var checklistItem3 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem3.IMC_IsPublished = false;
			checklistItem3.PublishedDescriptionText = "Symbol";
			var pivot1 = triage.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;
			var pivot2 = triage.ChecklistPivots.AddNew();
			pivot2.IMP_IMC_ChecklistItem = checklistItem2.PK;
			pivot2.IMP_Sequence = 2;
			var pivot3 = triage.ChecklistPivots.AddNew();
			pivot3.IMP_IMC_ChecklistItem = checklistItem3.PK;
			pivot3.IMP_Sequence = 3;
			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.Select(0);
				AssertEquals("Precondition: Should have selected the triage we added", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected the triage we added", triage.PK, resultsGrid.GetFirstSelectedRow().PK);
				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				//Navigate to second tab
				nextButton.PerformClick();

				var messageTextBox = (ZTextBox)form.Controls.Find("clientMessageBuilderTextBox", true).FirstOrDefault();
				messageTextBox.Text = "Original";

				var checklistItemsForMessageGrid = (ZGrid)form.Controls.Find("checklistItemsForMessageGrid", true).FirstOrDefault();
				checklistItemsForMessageGrid.SelectAllElements();
				var addToMessageButton = (ZButton)form.Controls.Find("addToMessageButton", true).FirstOrDefault();
				addToMessageButton.Enabled = true;
				UnitTestUserNotification.Instance.AddOKAnswer();
				addToMessageButton.PerformClick();
				AssertEquals(FormattableString.Invariant($"The message could not include the specified Checklist Item(s) (Sequence 2, 3) because they are not published."), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have added the published text", FormattableString.Invariant(
$@"Original

-- {checklistItem1.PublishedDescriptionText}
"), messageTextBox.Text);
			}
		}

		public void TestAddToMessageButton_Click_Order()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem1.IMC_IsPublished = true;
			checklistItem1.PublishedDescriptionText = "Number";
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem2.IMC_IsPublished = true;
			checklistItem2.PublishedDescriptionText = "Letter";
			var pivot1 = triage.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 2;
			var pivot2 = triage.ChecklistPivots.AddNew();
			pivot2.IMP_IMC_ChecklistItem = checklistItem2.PK;
			pivot2.IMP_Sequence = 1;
			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.Select(0);
				AssertEquals("Precondition: Should have selected the triage we added", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected the triage we added", triage.PK, resultsGrid.GetFirstSelectedRow().PK);
				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				//Navigate to second tab
				nextButton.PerformClick();

				var messageTextBox = (ZTextBox)form.Controls.Find("clientMessageBuilderTextBox", true).FirstOrDefault();
				messageTextBox.Text = "Original";

				var checklistItemsForMessageGrid = (ZGrid)form.Controls.Find("checklistItemsForMessageGrid", true).FirstOrDefault();
				checklistItemsForMessageGrid.SelectAllElements();
				var addToMessageButton = (ZButton)form.Controls.Find("addToMessageButton", true).FirstOrDefault();
				addToMessageButton.Enabled = true;
				AssertEquals("Precondition: Tab should be selected so parent should be visible", true, addToMessageButton.Parent.Visible);
				AssertEquals("Precondition", true, addToMessageButton.Enabled);
				AssertEquals("Precondition", true, addToMessageButton.CanSelect);
				AssertEquals("Precondition: Should have selected both checklist items", 2, checklistItemsForMessageGrid.SelectedRowCount);

				addToMessageButton.PerformClick();
				AssertEquals("Precondition: Should have selected both checklist items", 2, checklistItemsForMessageGrid.SelectedRowCount);
				AssertEquals("Should have added the published text in sequence order", FormattableString.Invariant(
$@"Original

-- {checklistItem2.PublishedDescriptionText}

-- {checklistItem1.PublishedDescriptionText}
"), messageTextBox.Text);
			}
		}

		public void TestNextButton_Click()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "Hello there";
			triage2.IMT_SupportDescription = "Hello";

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 2;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);
			helper.DescriptionKeyword1 = "Hello";
			helper.DescriptionKeyword2 = "there";

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.UnSelectAll();

				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				nextButton.PerformClick();
				AssertEquals("Should show warning to select single node", "Please select one triage node to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
				var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();
				var reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				AssertEquals("Should not toggle tab visibility", true, findTriageTabPage.TabVisible);
				AssertEquals("Should not toggle tab visibility", null, reviewChecklistTabPage);

				resultsGrid.SelectAllElements();
				AssertEquals("Precondition: Should have selected both triages", 2, resultsGrid.SelectedRowCount);
				nextButton.PerformClick();

				reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				AssertEquals("Should show warning to select single node", "Please select one triage node to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not toggle tab visibility", true, findTriageTabPage.TabVisible);
				AssertEquals("Should not toggle tab visibility", null, reviewChecklistTabPage);

				resultsGrid.UnSelectAll();
				resultsGrid.Select(0);
				AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
				UnitTestUserNotification.Instance.ClearMessages();
				nextButton.PerformClick();

				reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				AssertEquals("Should toggle tab visibility", false, findTriageTabPage.TabVisible);
				AssertEquals("Should toggle tab visibility", true, reviewChecklistTabPage.TabVisible);
			}
		}

		public void TestBackToSearchButton_Click()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "Hello there";

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);
			helper.DescriptionKeyword1 = "Hello";
			helper.DescriptionKeyword2 = "there";

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.Select(0);
				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

				AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
				nextButton.PerformClick();

				var reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				AssertEquals("Precondition: Should toggle tab visibility", false, findTriageTabPage.TabVisible);
				AssertEquals("Precondition: Should toggle tab visibility", true, reviewChecklistTabPage.TabVisible);

				var backToSearchButton = (ZButton)form.Controls.Find("backToSearchButton", true).FirstOrDefault();
				backToSearchButton.PerformClick();
				AssertEquals("Should toggle tab visibility", true, findTriageTabPage.TabVisible);
				AssertEquals("Should toggle tab visibility", false, reviewChecklistTabPage.TabVisible);

				nextButton.PerformClick();
				reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				AssertEquals("Precondition: Should toggle tab visibility", false, findTriageTabPage.TabVisible);
				AssertEquals("Precondition: Should toggle tab visibility", true, reviewChecklistTabPage.TabVisible);

				var initialSaveCount = Factory.SaveCount;
				triage1.SupportNotesAsBlob = new ZBlob(new byte[] { 1, 2, 3, 4 });
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				backToSearchButton.PerformClick();
				AssertEquals("Should not toggle tab visibility", false, findTriageTabPage.TabVisible);
				AssertEquals("Should not toggle tab visibility", true, reviewChecklistTabPage.TabVisible);
				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not save", initialSaveCount, Factory.SaveCount);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				backToSearchButton.PerformClick();
				AssertEquals("Should toggle tab visibility", true, findTriageTabPage.TabVisible);
				AssertEquals("Should toggle tab visibility", false, reviewChecklistTabPage.TabVisible);
				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not save", initialSaveCount, Factory.SaveCount);

				nextButton.PerformClick();
				AssertEquals("Precondition: Should toggle tab visibility", false, findTriageTabPage.TabVisible);
				AssertEquals("Precondition: Should toggle tab visibility", true, reviewChecklistTabPage.TabVisible);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				backToSearchButton.PerformClick();
				AssertEquals("Should toggle tab visibility", true, findTriageTabPage.TabVisible);
				AssertEquals("Should toggle tab visibility", false, reviewChecklistTabPage.TabVisible);
				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should save", initialSaveCount + 1, Factory.SaveCount);

				var newFactory = new BusinessObjectFactory();
				var triageReloaded = newFactory.Load<IncidentTriage>(triage1.PK);
				AssertEquals("Should have saved changes to the database", new ZBlob(new byte[] { 1, 2, 3, 4 }), triageReloaded.SupportNotesAsBlob);
			}
		}

		public void TestSaveAndCloseButton_Click()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.Select(0);
				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

				AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
				UnitTestUserNotification.Instance.ClearMessages();
				nextButton.PerformClick();

				var saveAndCloseButton = (ZButton)form.Controls.Find("saveAndCloseButton", true).FirstOrDefault();

				AssertNotEquals("Precondition: incident should not be linked to triage", triage1.PK, incident.IM_IMT_Triage);
				var isClosed = false;
				form.FormClosed += (_, e_) => { isClosed = true; };
				saveAndCloseButton.PerformClick();

				var newFactory = new BusinessObjectFactory();
				var incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
				AssertEquals("Should have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
				Assert("Form should be closed", isClosed);
			}
		}

		public void TestSaveAndCloseButton_Click_ClientMessage_AppendToEConversation()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				using (var form = new FindTriageForm(helper, parentIncidentForm))
				{
					form.Show();
					var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
					searchButton.PerformClick();
					var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
					resultsGrid.Select(0);
					var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
					var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

					AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
					AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
					UnitTestUserNotification.Instance.ClearMessages();
					nextButton.PerformClick();

					helper.ClientMessageBuilder = "Howdy";
					var existingMessage = "existing message...";
					parentIncidentForm.ConversationMessageTextBox.Text = existingMessage;

					var saveAndCloseButton = (ZButton)form.Controls.Find("saveAndCloseButton", true).FirstOrDefault();

					AssertNotEquals("Precondition: incident should not be linked to triage", triage1.PK, incident.IM_IMT_Triage);
					var isClosed = false;
					form.FormClosed += (_, e_) => { isClosed = true; };
					AssertEquals("Precondition: EConversation text box should be enabled so the client message can be added to it", true, parentIncidentForm.ConversationMessageTextBox.Enabled);
					saveAndCloseButton.PerformClick();

					var newFactory = new BusinessObjectFactory();
					var incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
					AssertEquals("Should have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
					AssertEquals("Should have copied the client message to the econversation on the incident form", existingMessage + helper.ClientMessageBuilder, parentIncidentForm.ConversationMessageTextBox.Text);
					Assert("Form should be closed", isClosed);
				}
			}
		}

		public void TestSaveAndCloseButton_Click_ClientMessage()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				using (var form = new FindTriageForm(helper, parentIncidentForm))
				{
					form.Show();
					var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
					searchButton.PerformClick();
					var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
					resultsGrid.Select(0);
					var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
					var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

					AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
					AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
					UnitTestUserNotification.Instance.ClearMessages();
					nextButton.PerformClick();

					helper.ClientMessageBuilder = "Howdy";

					var saveAndCloseButton = (ZButton)form.Controls.Find("saveAndCloseButton", true).FirstOrDefault();

					AssertNotEquals("Precondition: incident should not be linked to triage", triage1.PK, incident.IM_IMT_Triage);
					var isClosed = false;
					form.FormClosed += (_, e_) => { isClosed = true; };
					AssertEquals("Precondition: EConversation text box should be enabled so the client message can be added to it", true, parentIncidentForm.ConversationMessageTextBox.Enabled);
					saveAndCloseButton.PerformClick();

					var newFactory = new BusinessObjectFactory();
					var incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
					AssertEquals("Should have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
					AssertEquals("Should have copied the client message to the econversation on the incident form", helper.ClientMessageBuilder, parentIncidentForm.ConversationMessageTextBox.Text);
					Assert("Form should be closed", isClosed);
				}
			}
		}

		public void TestSaveAndCloseButton_Click_SupportNotes()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "HI";

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				using (var form = new FindTriageForm(helper, parentIncidentForm))
				{
					form.Show();
					var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
					searchButton.PerformClick();
					var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
					resultsGrid.Select(0);
					var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
					var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

					AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
					AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
					UnitTestUserNotification.Instance.ClearMessages();
					nextButton.PerformClick();

					helper.ClientMessageBuilder = "Howdy";
					triage1.SupportNotesAsBlob = new ZBlob(new byte[] { 1, 2, 3, 4 });

					var saveAndCloseButton = (ZButton)form.Controls.Find("saveAndCloseButton", true).FirstOrDefault();

					AssertNotEquals("Precondition: incident should not be linked to triage", triage1.PK, incident.IM_IMT_Triage);
					var isClosed = false;
					form.FormClosed += (_, e_) => { isClosed = true; };
					AssertEquals("Precondition: EConversation text box should be enabled so the client message can be added to it", true, parentIncidentForm.ConversationMessageTextBox.Enabled);
					saveAndCloseButton.PerformClick();

					var newFactory = new BusinessObjectFactory();
					var incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
					var triageReloaded = newFactory.Load<IncidentTriage>(triage1.PK);
					AssertEquals("Should have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
					AssertEquals("Should have copied the client message to the econversation on the incident form", helper.ClientMessageBuilder, parentIncidentForm.ConversationMessageTextBox.Text);
					AssertEquals("Should have saved support notes to triage", new ZBlob(new byte[] { 1, 2, 3, 4 }), triageReloaded.SupportNotesAsBlob);
					Assert("Form should be closed", isClosed);
				}
			}
		}

		public void TestSaveAndCloseButton_Click_ClientMessage_EConversationDisabledThenEnabled()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				using (var form = new FindTriageForm(helper, parentIncidentForm))
				{
					form.Show();
					var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
					searchButton.PerformClick();
					var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
					resultsGrid.Select(0);
					var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
					var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

					AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
					AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
					UnitTestUserNotification.Instance.ClearMessages();
					nextButton.PerformClick();

					helper.ClientMessageBuilder = "Howdy";
					parentIncidentForm.ConversationMessageTextBox.Enabled = false;

					var saveAndCloseButton = (ZButton)form.Controls.Find("saveAndCloseButton", true).FirstOrDefault();

					AssertNotEquals("Precondition: incident should not be linked to triage", triage1.PK, incident.IM_IMT_Triage);
					var isClosed = false;
					form.FormClosed += (_, e_) => { isClosed = true; };
					AssertEquals("Precondition: EConversation text box should be disabled", false, parentIncidentForm.ConversationMessageTextBox.Enabled);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					saveAndCloseButton.PerformClick();

					var newFactory = new BusinessObjectFactory();
					var incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
					AssertEquals("Should show warning when trying to save", "The Incident's eConversation is disabled. If you continue with this action, the client message on this form will be discarded. Alternatively you click No and make sure the eConversation is enabled before trying to save again. Would you like to continue and discard the client message?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotEquals("Should not have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
					AssertNotEquals("Should not have copied the client message to the econversation on the incident form", helper.ClientMessageBuilder, parentIncidentForm.ConversationMessageTextBox.Text);
					AssertNotEquals("Form should not be closed", true, isClosed);

					UnitTestUserNotification.Instance.ClearMessages();
					parentIncidentForm.ConversationMessageTextBox.Enabled = true;
					saveAndCloseButton.PerformClick();

					newFactory = new BusinessObjectFactory();
					incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
					AssertEquals("Should have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
					AssertEquals("Should have copied the client message to the econversation on the incident form", helper.ClientMessageBuilder, parentIncidentForm.ConversationMessageTextBox.Text);
					Assert("Form should be closed", isClosed);
				}
			}
		}

		public void TestSaveAndCloseButton_Click_ClientMessage_EConversationDisabled_DoNotSaveMessage()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				using (var form = new FindTriageForm(helper, parentIncidentForm))
				{
					form.Show();
					var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
					searchButton.PerformClick();
					var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
					resultsGrid.Select(0);
					var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
					var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

					AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
					AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
					UnitTestUserNotification.Instance.ClearMessages();
					nextButton.PerformClick();

					helper.ClientMessageBuilder = "Howdy";
					parentIncidentForm.ConversationMessageTextBox.Enabled = false;

					var saveAndCloseButton = (ZButton)form.Controls.Find("saveAndCloseButton", true).FirstOrDefault();

					AssertNotEquals("Precondition: incident should not be linked to triage", triage1.PK, incident.IM_IMT_Triage);
					var isClosed = false;
					form.FormClosed += (_, e_) => { isClosed = true; };
					AssertEquals("Precondition: EConversation text box should be disabled", false, parentIncidentForm.ConversationMessageTextBox.Enabled);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					saveAndCloseButton.PerformClick();

					var newFactory = new BusinessObjectFactory();
					var incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
					AssertEquals("Should have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
					AssertNotEquals("Should not have copied the client message to the econversation on the incident form", helper.ClientMessageBuilder, parentIncidentForm.ConversationMessageTextBox.Text);
					Assert("Form should be closed", isClosed);
				}
			}
		}

		public void TestSaveAndCloseButton_Click_NoClientMessage_EConversationDisabled()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);

			using (var parentIncidentForm = new SupportIncidentForm(incident))
			{
				parentIncidentForm.Show();
				parentIncidentForm.ConversationMessageTextBox.Enabled = false;

				using (var form = new FindTriageForm(helper, parentIncidentForm))
				{
					form.Show();
					var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
					searchButton.PerformClick();
					var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
					resultsGrid.Select(0);
					var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
					var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

					AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
					AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
					UnitTestUserNotification.Instance.ClearMessages();
					nextButton.PerformClick();

					var saveAndCloseButton = (ZButton)form.Controls.Find("saveAndCloseButton", true).FirstOrDefault();

					AssertNotEquals("Precondition: incident should not be linked to triage", triage1.PK, incident.IM_IMT_Triage);
					var isClosed = false;
					form.FormClosed += (_, e_) => { isClosed = true; };
					AssertEquals("Precondition: EConversation text box should be disabled", false, parentIncidentForm.ConversationMessageTextBox.Enabled);
					saveAndCloseButton.PerformClick();

					var newFactory = new BusinessObjectFactory();
					var incidentReloaded = newFactory.Load<SupportIncident>(incident.PK);
					AssertEquals("Should have saved triage's PK to incident's FK", triage1.PK, incidentReloaded.IM_IMT_Triage);
					Assert("Form should be closed", isClosed);
				}
			}
		}

		public void TestReviewChecklistCancelButton_Click_DoNotSave()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "Hello there";

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 2;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);
			helper.DescriptionKeyword1 = "Hello";
			helper.DescriptionKeyword2 = "there";

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.Select(0);
				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

				AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
				nextButton.PerformClick();
				var reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				var reviewChecklistCancelButton = (ZButton)form.Controls.Find("reviewChecklistCancelButton", true).FirstOrDefault();
				var isClosed = false;
				form.FormClosed += (_, e_) => { isClosed = true; };
				var initialSaveCount = Factory.SaveCount;
				triage1.SupportNotesAsBlob = new ZBlob(new byte[] { 1, 2, 3, 4 });
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				reviewChecklistCancelButton.PerformClick();

				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not save", initialSaveCount, Factory.SaveCount);
				AssertEquals("Should not close form", false, isClosed);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				reviewChecklistCancelButton.PerformClick();
				AssertEquals("Should close form", true, isClosed);
				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not save", initialSaveCount, Factory.SaveCount);

				var newFactory = new BusinessObjectFactory();
				var triageReloaded = newFactory.Load<IncidentTriage>(triage1.PK);
				AssertNotEquals("Should not have saved changes to the database", new ZBlob(new byte[] { 1, 2, 3, 4 }), triageReloaded.SupportNotesAsBlob);
			}
		}

		public void TestReviewChecklistCancelButton_Click_Save()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "Hello there";

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);
			helper.DescriptionKeyword1 = "Hello";
			helper.DescriptionKeyword2 = "there";

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.Select(0);
				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();

				AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
				nextButton.PerformClick();
				var reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				var reviewChecklistCancelButton = (ZButton)form.Controls.Find("reviewChecklistCancelButton", true).FirstOrDefault();
				var isClosed = false;
				form.FormClosed += (_, e_) => { isClosed = true; };
				var initialSaveCount = Factory.SaveCount;
				triage1.SupportNotesAsBlob = new ZBlob(new byte[] { 1, 2, 3, 4 });
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				reviewChecklistCancelButton.PerformClick();

				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not save", initialSaveCount, Factory.SaveCount);
				AssertEquals("Should not close form", false, isClosed);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				reviewChecklistCancelButton.PerformClick();
				AssertEquals("Should close form", true, isClosed);
				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should save", initialSaveCount + 1, Factory.SaveCount);

				var newFactory = new BusinessObjectFactory();
				var triageReloaded = newFactory.Load<IncidentTriage>(triage1.PK);
				AssertEquals("Should have saved changes to the database", new ZBlob(new byte[] { 1, 2, 3, 4 }), triageReloaded.SupportNotesAsBlob);
			}
		}

		public void TestCancelButton_Click_Save()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_SupportDescription = "Hello there";

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = triage1.ChecklistPivots.AddNew();
			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			pivot1.IMP_Sequence = 1;

			Factory.Save();

			var helper = new FindTriageFilterHelper(incident);
			helper.DescriptionKeyword1 = "Hello";
			helper.DescriptionKeyword2 = "there";

			using (var form = new FindTriageForm(helper))
			{
				form.Show();
				var searchButton = (ZButton)form.Controls.Find("searchButton", true).FirstOrDefault();
				searchButton.PerformClick();
				var resultsGrid = (ZGrid)form.Controls.Find("resultsGrid", true).FirstOrDefault();
				resultsGrid.Select(0);
				var nextButton = (ZButton)form.Controls.Find("nextButton", true).FirstOrDefault();
				var findTriageTabPage = (ZTabPage)form.Controls.Find("findTriageTabPage", true).FirstOrDefault();
				var cancelButton = (ZButton)form.Controls.Find("cancelButton", true).FirstOrDefault();

				AssertEquals("Precondition: Should have selected triage1", 1, resultsGrid.SelectedRowCount);
				AssertEquals("Precondition: Should have selected triage1", triage1.PK, resultsGrid.GetFirstSelectedRow().PK);
				nextButton.PerformClick();
				var reviewChecklistTabPage = (ZTabPage)form.Controls.Find("reviewChecklistTabPage", true).FirstOrDefault();
				var isClosed = false;
				form.FormClosed += (_, e_) => { isClosed = true; };
				var initialSaveCount = Factory.SaveCount;
				triage1.SupportNotesAsBlob = new ZBlob(new byte[] { 1, 2, 3, 4 });
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var backToSearchButton = (ZButton)form.Controls.Find("backToSearchButton", true).FirstOrDefault();
				backToSearchButton.PerformClick();
				AssertEquals("Precondition: Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				cancelButton.PerformClick();

				AssertEquals("Should close form", true, isClosed);
				AssertEquals("Should show prompt to save", "Do you want to save your changes to support notes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should save", initialSaveCount + 1, Factory.SaveCount);

				var newFactory = new BusinessObjectFactory();
				var triageReloaded = newFactory.Load<IncidentTriage>(triage1.PK);
				AssertEquals("Should have saved changes to the database", new ZBlob(new byte[] { 1, 2, 3, 4 }), triageReloaded.SupportNotesAsBlob);
			}
		}
	}
}
