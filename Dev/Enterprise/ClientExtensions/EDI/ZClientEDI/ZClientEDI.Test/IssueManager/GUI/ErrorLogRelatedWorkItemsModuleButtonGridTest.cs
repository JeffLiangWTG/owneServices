using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.GUI
{
	class ErrorLogRelatedWorkItemsModuleButtonGridTest : TestCaseWithFactory
	{
		EdiHelpErrorLog issue;
		public void TestModuleID()
		{
			using (ErrorLogRelatedWorkItemsModuleButtonGrid grid = new ErrorLogRelatedWorkItemsModuleButtonGrid())
			{
				AssertEquals("ModuleID", ModuleIDs.WorkItem, grid.ModuleID);
			}
		}

		public void TestCreateIncidents()
		{
			using (TestForm form = new TestForm(Issue))
			{
				form.Show();
				form.Grid.createIncidentsButton.PerformClick();
				AssertEquals("The no related work item message should be shown.", "There are no work items attached to this issue. Please create a work item first in order to create incidents.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Issue.RelatedIncidents.Count", 0, Issue.RelatedIncidents.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Issue.RelatedWorkItems.AddNew();
				form.Grid.createIncidentsButton.PerformClick();
				AssertEquals("The no occurrences message should be shown.", "There are no occurrences for this issue. Incidents cannot be created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Issue.RelatedIncidents.Count", 0, Issue.RelatedIncidents.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
				EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
				org1.CreateAndLoadLicenceForOrg();
				org2.CreateAndLoadLicenceForOrg();
				org1.OH_FullName = "Foo";
				org2.OH_FullName = "Bar";
				org1.LicenceEnterpriseCode = "OH1";
				org2.LicenceEnterpriseCode = "OH2";
				var database1 = org1.LicCompany.LicDatabases.AddNew();
				var licence1 = org1.LicCompany.GetHeader(database1);
				var clientCompany1 = Factory.New<ClientCompany>();
				clientCompany1.LCC_Code = "AAA";
				clientCompany1.LCC_LD = database1.PK;
				clientCompany1.LCC_OH = org1.PK;
				var database2 = org2.LicCompany.LicDatabases.AddNew();
				var licence2 = org2.LicCompany.GetHeader(database2);
				var clientCompany2 = Factory.New<ClientCompany>();
				clientCompany2.LCC_Code = "BBB";
				clientCompany2.LCC_LD = database2.PK;
				clientCompany2.LCC_OH = org2.PK;
				Issue.Occurrences.AddNew().HO_LD = licence1.LA_LD;
				Issue.Occurrences.AddNew().HO_LD = licence2.LA_LD;
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.Grid.createIncidentsButton.PerformClick();
				AssertEquals("The confirmation message should be shown.", "This operation may create lots of incidents, do you want to continue?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Issue.RelatedIncidents.Count", 2, Issue.RelatedIncidents.Count);
				AssertEquals("Issue.RelatedIncidents[0].IsInDatabase", true, Issue.RelatedIncidents[0].IsInDatabase);
				AssertEquals("Issue.RelatedIncidents[1].IsInDatabase", true, Issue.RelatedIncidents[1].IsInDatabase);
				Issue.RelatedIncidents.Sort(IncidentMainSchema.Constants.IM_IncidentNumber);
				string expectedMessage = string.Format("The following incidents have been created:\r\n\r\n{0} for {1} - {2}\r\n{3} for {4} - {5}", Issue.RelatedIncidents[0].IM_IncidentNumber, Issue.RelatedIncidents[0].Client.OH_Code, Issue.RelatedIncidents[0].Client.OH_FullName, Issue.RelatedIncidents[1].IM_IncidentNumber, Issue.RelatedIncidents[1].Client.OH_Code, Issue.RelatedIncidents[1].Client.OH_FullName);
				AssertEquals("The incidents created summary message should be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.Grid.createIncidentsButton.PerformClick();
				AssertEquals("The no new incidents message should be shown.", "No new incidents were created. All the client databases that the occurrences belong to already have a related work item attached to this issue.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Issue.RelatedIncidents.Count", 2, Issue.RelatedIncidents.Count);
			}
		}

		public void TestSavingReloadsRelatedIncidents()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			workItem.RelatedItems.Add(Issue);
			Factory.Save();
			AssertEquals("Issue.RelatedIncidents.Count", 1, Issue.RelatedIncidents.Count);
			using (TestForm form = new TestForm(Issue))
			{
				form.Show();
				form.Grid.EditForTest(workItem);
				using (NewWorkItemForm lastShownZForm = (NewWorkItemForm)form.Grid.LastShownZForm)
				{
					lastShownZForm.DataSource.RelatedItems.Remove(incident.PK);
					lastShownZForm.DataSource.Factory.Save();
					AssertEquals("Issue.RelatedIncidents.Count", 0, Issue.RelatedIncidents.Count);
					int dbLoadCount = Factory.DatabaseLoadCount;
					lastShownZForm.Close();
					((NewWorkItem)lastShownZForm.LastDataSourceForTest).Factory.Save();
					AssertEquals("Factory.DatabaseLoadCount", dbLoadCount, Factory.DatabaseLoadCount);
				}

				form.Grid.EditForTest(workItem);
				using (NewWorkItemForm lastShownZForm = (NewWorkItemForm)form.Grid.LastShownZForm)
				{
					lastShownZForm.DataSource.Factory.Load<SupportIncident>(incident.PK).RelatedItems.Add(lastShownZForm.DataSource);
					lastShownZForm.DataSource.Factory.Save();
					AssertEquals("Issue.RelatedIncidents.Count", 1, Issue.RelatedIncidents.Count);
				}
			}
		}

		public void TestShowNewFormFromController()
		{
			Issue.HE_ExceptionMessage = "Something";
			using (TestForm form = new TestForm(Issue))
			{
				form.Show();
				form.Grid.FireNewButtonClick();
				using (NewWorkItemForm lastShownZForm = (NewWorkItemForm)form.Grid.LastShownZForm)
				{
					AssertEquals("LastShownZForm.BusinessEntity.IM_Description", "Something", lastShownZForm.DataSource.WKI_Summary);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestEdit()
		{
			using (var form = new TestForm(Issue))
			{
				form.Show();
				form.Grid.NewButton_ClickForTest(form.Grid, EventArgs.Empty);
				using (var wiForm = form.Grid.LastShownZForm as NewWorkItemForm)
				{
					wiForm.BusinessEntity.Factory.Save();
					wiForm.Close();

					AssertNotEquals(form.Grid.CollectionForTest.Count, 0);

					form.Grid.EditForTest((BusinessObject)form.Grid.CollectionForTest[0]);
					form.Grid.LastShownZForm.Dispose();
				}
			}
		}

		EdiHelpErrorLog Issue
		{
			get
			{
				return issue ?? (issue = Factory.New<EdiHelpErrorLog>());
			}
		}

		#region MockForm
		class ErrorLogRelatedWorkItemsModuleButtonGridForTest : ErrorLogRelatedWorkItemsModuleButtonGrid
		{
			internal void EditForTest(BusinessObject selected)
			{
				base.Edit(selected, null);
			}

			internal void NewButton_ClickForTest(object sender, EventArgs e)
			{
				NewButton_Click(sender, e);
			}

			internal IBusinessObjectCollection CollectionForTest => base.Collection;
		}

		class TestForm : ZForm
		{
			ErrorLogRelatedWorkItemsModuleButtonGridForTest grid;
			public TestForm(EdiHelpErrorLog businessEntity) : base(businessEntity)
			{
				Grid.Issue = businessEntity;
			}

			public ErrorLogRelatedWorkItemsModuleButtonGridForTest Grid
			{
				get
				{
					return grid;
				}
			}

			protected override void InitializeComponent()
			{
				grid = new ErrorLogRelatedWorkItemsModuleButtonGridForTest();
				grid.BindToFindBoxList = "Lookups+WorkItems";
				grid.BindToGridList = "RelatedWorkItems";
				ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Incident Number";
				column.ColumnName = "IM_IncidentNumber";
				grid.ColumnStyles.Add(column);
				Controls.Add(grid);
			}
		}
		#endregion
	}
}
