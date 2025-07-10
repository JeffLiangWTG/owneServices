using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.EConversation.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(NewWorkItemForm))]
	public class NewWorkItemFormTest : ZFormBasherTest
	{
		public void TestRelatedItemsTabShouldContainUnifiedControlsForRelatedItems_AndCreateIncidentsButton()
		{
			using (var form = (NewWorkItemForm)GetFormToBashCore())
			{
				form.Show();
				var relatedTab = form.TopLevelTabControl_Exposed.GetTabPageByNameOrText("Related Items");
				AssertNotNull(relatedTab);
				form.TopLevelTabControl_Exposed.SelectedTab = relatedTab;
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("RelatedItemGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("NetworkDiagramGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ParentWorkflowGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ChildWorkflowGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZButton>("CreateIncidentsButton"));
			}
		}

		public void TestWorkItemHasChangesWhenNotInDatabse()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			AssertEquals(false, workItem.HasChanges);
			using (var form = new NewWorkItemForm(workItem))
			{
				AssertEquals(true, workItem.HasChanges);
			}

			Factory.Save();
			using (var form = new NewWorkItemForm(workItem))
			{
				AssertEquals(false, workItem.HasChanges);
			}
		}

		public void TestTaskStatsRefreshOnTabChange()
		{
			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "ST2";
			using (var form = (NewWorkItemForm)GetFormToBashCore())
			{
				NewWorkItem workItem = form.DataSource;
				ProcessTask task = workItem.WorkflowItems.AddNew();
				form.Show();
				task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				form.TopLevelTabControl_Exposed.SelectedIndex = 1;
				form.TopLevelTabControl_Exposed.SelectedIndex = 0;
				AssertEquals(GlbStaff.CurrentUser.PK, workItem.AssignedToStaff.PK);
				task.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
				form.TopLevelTabControl_Exposed.SelectedIndex = 1;
				form.TopLevelTabControl_Exposed.SelectedIndex = 0;
				AssertEquals(staff2.PK, workItem.AssignedToStaff.PK);
			}
		}

		public void TestSave()
		{
			EDIDataRegistry.Instance.RequireCompletedCodeReviewForCheckin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "test";
			staff.GS_FullName = "Test Test";
			ShelvesetToolsTest.AddCheckInTaskTypes();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Description = "Merge PR";
			task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("http://example.com/pullrequest/123"));
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				EDIShelvesetInfo shelfInfo = new EDIShelvesetInfo("", "", "", task);
				AssertEquals("Shelf for this ProcessTask should NOT be scheduled yet", false, crikeyDataAccess.IsShelfScheduled(shelfInfo));
				using (NewWorkItemForm form = new NewWorkItemForm(workItem))
				{
					form.FireSaveButton();
					AssertNoErrors(workItem);
					AssertEquals("Shelf for this ProcessTask should be scheduled already", true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
					task.P9_Notes = ZBlob.FromAscii("Cancelled for Test");
					form.FireSaveButton();
					AssertEquals("Shelf for this ProcessTask should be removed from schedule", false, crikeyDataAccess.IsShelfScheduled(shelfInfo));
				}
			}
		}

		public void TestSaveWithCheckinReassign()
		{
			EDIDataRegistry.Instance.RequireCompletedCodeReviewForCheckin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "DV1";
			staff1.GS_LoginName = "dev.one";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "DV2";
			staff2.GS_LoginName = "dev.two";
			ShelvesetToolsTest.AddCheckInTaskTypes();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task.P9_Description = "Merge PR";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("http://example.com/pullrequest/123"));
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
				EDIShelvesetInfo shelfInfo = new EDIShelvesetInfo("", "", "", task);
				AssertEquals("Shelf for this ProcessTask should NOT be scheduled yet", false, crikeyDataAccess.IsShelfScheduled(shelfInfo));
				using (NewWorkItemForm form = new NewWorkItemForm(workItem))
				{
					form.FireSaveButton();
					AssertNoErrors(workItem);
					AssertEquals("Shelf for this ProcessTask should be scheduled already", true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
				}

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				NewWorkItem workItem2 = factory2.Load<NewWorkItem>(workItem.PK);
				using (NewWorkItemForm form = new NewWorkItemForm(workItem2))
				{
					WorkItemProcessTask task2 = workItem2.WorkflowItems[0];
					task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
					Assert(task2.P9_GS_NKAssignedStaffMemberInfo.HasChanges);
					task2.P9_Description = "another shelf";
					form.FireSaveButton();
					AssertNoErrors(workItem);
					AssertEquals("Shelf for this ProcessTask should be re-scheduled", true, crikeyDataAccess.IsShelfScheduled(shelfInfo));
				}
			}
		}

		public void TestSavePopuplatesShelfComments()
		{
			EDIDataRegistry.Instance.RequireCompletedCodeReviewForCheckin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "test";
			staff.GS_FullName = "Test Test";
			ShelvesetToolsTest.AddCheckInTaskTypes();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Description = "Merge PR";
			task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf(@"
http://example.com/pullrequest/123
Rtf Test Message"));
			using (var form = new NewWorkItemForm(workItem))
			{
				form.FireSaveButton();
				AssertNoErrors(workItem);
				using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
				{
					using (var cmd = connection.Command("select UH_Comments from UserTestHeader where UH_P9 = @taskPk"))
					{
						cmd.AddParameter("taskPk", SqlDbType.UniqueIdentifier, task.PK.ToGuid());
						AssertContains("Rtf Test Message", (string)cmd.ExecuteScalar());
					}

					task.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf(@"http://example.com/pullrequest/123
New Test Message"));
					form.FireSaveButton();
					using (var cmd = connection.Command("select UH_Comments from UserTestHeader where UH_P9 = @taskPk ORDER BY UH_DateRecordAdded DESC"))
					{
						cmd.AddParameter("taskPk", SqlDbType.UniqueIdentifier, task.PK.ToGuid());
						AssertContains("New Test Message", (string)cmd.ExecuteScalar());
					}
				}
			}
		}

		public void TestSaveWithRelatedItemsModification()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "test";
			staff.GS_FullName = "Test Test";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "UDF";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Description = "undefined";
			AssertEquals(0, workItem.RelatedItems.Count);
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var genPivot = Factory.NewWithValidTestData<GenPivot>();
			genPivot.XX_RelationType = EDIGenPivotTypes.DefectCausedByWorkItem;
			genPivot.XX_Relation2TableCode = IncidentMainSchema.Constants.Prefix;
			genPivot.XX_Relation1ID = workItem.PK;
			genPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = incident.PK;
			workItem.RelatedItems.Add(incident);
			AssertEquals(1, workItem.RelatedItems.Count);
			incident.CustomerNotifier = new EvilIncidentCustomerNotifier(workItem)
			{ RelatedIncident = incident };
			using (NewWorkItemForm form = new NewWorkItemForm(workItem))
			{
				form.FireSaveButton();
				AssertNoErrors(workItem);
			}
		}

		public void TestEConversationButtonSecurity()
		{
			EDISecurityCheckpoints.WorkItemEConversationSendMessages.IsAllowed = false;
			EDISecurityCheckpoints.WorkItemEConversationAddInternalComment.IsAllowed = false;
			EDISecurityCheckpoints.WorkItemEConversationBroadcast.IsAllowed = false;
			using (var form = new NewWorkItemForm(Factory.NewWithValidTestData<NewWorkItem>()))
			{
				var eConversation = form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);
				var userControl = ((EConversationFullControl)eConversation.UserControl);
				AssertEquals(false, userControl.SendButtonForTest.Enabled);
				AssertEquals(false, userControl.AddInternalCommentButtonForTest.Enabled);
				AssertEquals(false, userControl.BroadcastButtonForTest.Enabled);
			}

			EDISecurityCheckpoints.WorkItemEConversationSendMessages.IsAllowed = true;
			EDISecurityCheckpoints.WorkItemEConversationAddInternalComment.IsAllowed = true;
			EDISecurityCheckpoints.WorkItemEConversationBroadcast.IsAllowed = true;
			using (var form = new NewWorkItemForm(Factory.NewWithValidTestData<NewWorkItem>()))
			{
				var eConversation = form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);
				var userControl = ((EConversationFullControl)eConversation.UserControl);
				AssertEquals(true, userControl.SendButtonForTest.Enabled);
				AssertEquals(true, userControl.AddInternalCommentButtonForTest.Enabled);
				AssertEquals(true, userControl.BroadcastButtonForTest.Enabled);
			}
		}

		public class EvilIncidentCustomerNotifier : IIncidentCustomerNotifier
		{
			public EvilIncidentCustomerNotifier(NewWorkItem unsuspectingTarget)
			{
				this.unsuspectingTarget = unsuspectingTarget;
			}

			readonly NewWorkItem unsuspectingTarget;
			public SupportIncident RelatedIncident { get; set; }

			public void SendQueuedEmails()
			{
				var incident = unsuspectingTarget.Factory.NewWithValidTestData<SupportIncident>();
				var genPivot = unsuspectingTarget.Factory.NewWithValidTestData<GenPivot>();
				genPivot.XX_RelationType = EDIGenPivotTypes.DefectCausedByWorkItem;
				genPivot.XX_Relation2TableCode = IncidentMainSchema.Constants.Prefix;
				genPivot.XX_Relation1ID = unsuspectingTarget.PK;
				genPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
				genPivot.XX_Relation2ID = incident.PK;
				unsuspectingTarget.RelatedItems.Add(incident);
			}

			public void SendChanges()
			{
			}

			public void SendEmailNow(SupportIncidentEmail email)
			{
				throw new InvalidOperationException();
			}

			public void SendEmailOnIncidentSave(SupportIncidentEmail email)
			{
				throw new InvalidOperationException();
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			workItem.WKI_Summary = "test";
			Factory.Save();
			NewWorkItemForm result = new NewWorkItemForm(workItem);
			result.ControllerID = ControllerIDs.WorkItem;
			result.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1524);
			return result;
		}

		#endregion
	}
}
