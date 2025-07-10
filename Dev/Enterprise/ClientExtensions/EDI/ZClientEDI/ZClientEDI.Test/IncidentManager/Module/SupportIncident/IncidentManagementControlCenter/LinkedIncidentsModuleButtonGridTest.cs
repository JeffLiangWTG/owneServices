using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(LinkedIncidentsModuleButtonGrid))]
	public class LinkedIncidentsModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestAttachCore()
		{
			var factory = new BusinessObjectFactory();
			var incident = factory.New<SupportIncident>();
			var group = factory.New<IncidentManagementGroup>();
			factory.Save();

			var findBoxList = new IncidentManagementLinkCollection(factory);
			var attacher = new LinkedIncidentsGridAttacherForTest(incident.RelatedItems, findBoxList, ClientModuleRegistration.IncidentManagementGroup, group);
			attacher.TestAttachCore(incident, new List<BusinessObject>());

			Assert("incident.RelatedItems should contain group", incident.RelatedItems.Contains(group));
		}

		public void TestDetachButtonClick()
		{
			var factory = new BusinessObjectFactory();
			var registryCollection = new IncidentGroupTypeCollection();
			registryCollection.RemoveAll();

			var groupType = registryCollection.AddNew();
			groupType.GroupType = "MIM";
			var invStage = groupType.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.CascadeProductDetails = true;
			invStage.IncidentCompleted = false;

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCollection);

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";

			var org = factory.NewWithValidTestData<OrgHeader>();
			var uncontrolledIncident = factory.NewWithValidTestData<SupportIncident>();
			var controlledIncident = factory.NewWithValidTestData<SupportIncident>();
			var task1 = uncontrolledIncident.WorkflowItems.AddNew();
			var task2 = controlledIncident.WorkflowItems.AddNew();

			uncontrolledIncident.IM_OH_Client = org.PK;
			uncontrolledIncident.IM_Priority = "CR8";
			uncontrolledIncident.IM_Product = "ENT";
			uncontrolledIncident.IM_Module = "CEC";
			uncontrolledIncident.IM_ServiceType = "TOP";
			uncontrolledIncident.IM_SourceModuleId = "INC";
			uncontrolledIncident.IM_ProgramArea = "GEO";

			controlledIncident.IM_OH_Client = org.PK;
			controlledIncident.IM_Priority = "CR8";
			controlledIncident.IM_Product = "ENT";
			controlledIncident.IM_Module = "CEC";
			controlledIncident.IM_ServiceType = "TOP";
			controlledIncident.IM_SourceModuleId = "INC";
			controlledIncident.IM_ProgramArea = "GEO";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			factory.Save();

			var group = factory.NewWithValidTestData<IncidentManagementGroup>();
			var link1 = factory.New<IncidentManagementLink>();
			var link2 = factory.New<IncidentManagementLink>();

			group.ING_Type = "MIM";
			group.ING_Status = "INV";

			group.ING_Priority = "CR8";
			group.ING_Product = "ENT";
			group.ING_Module = "WTC";
			group.ING_ServiceType = "TOP";
			group.ING_SourceModuleId = "test";
			group.ING_ProductArea = "TST";

			link1.INL_IM_Incident = uncontrolledIncident.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			link2.INL_IM_Incident = controlledIncident.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_IsGroupControlled = true;

			factory.Save();

			controlledIncident.Reload();
			uncontrolledIncident.Reload();
			group.Reload();

			Assert("The Uncontrolled incident's tasks should not be cancelled", task1.IsCurrent);
			Assert("The Controlled incident's tasks should be cancelled", !task2.IsCurrent);

			using (var form = new TestForm(group))
			{
				form.Show();
				var grid = form.LinkedGrid;
				grid.InnerGrid.SelectAllElements();

				AssertEquals(2, grid.InnerGrid.ListManager.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var detachButton = grid.DetachButtonForTest;

				AssertEquals("The controlled incident's product details should be same with group", group.ING_ProductArea, controlledIncident.ProductArea);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				detachButton.PerformClick();
				AssertNull("SupportIncident should not be detached when clicking No", controlledIncident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DetachedCode)).FirstOrDefault());
				AssertNull("SupportIncident should not be detached when clicking No", group.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DetachedCode)).FirstOrDefault());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				detachButton.PerformClick();
				var expectedPrompt = $"You are about to detach incidents.";
				AssertContains(expectedPrompt, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No incident should be linked to Group", 0, grid.InnerGrid.ListManager.Count);

				factory.Save();
			}

			AssertNull("IncidentManagementLink object should be delete after clicking detach.", factory.Load<IncidentManagementLink>(new ZQuery(IncidentManagementLinkSchema.INL_ING_Group, group.PK)).FirstOrDefault());
			Assert("The controlled incident's tasks should be reopened after detaching.", task2.IsCurrent);

			var dtcEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DetachedCode).AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"JOB={group.Number}");
			var uncontrolledDTC = dtcEventQuery.DeepClone().AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"RFN={uncontrolledIncident.Number}");
			var controlledDTC = dtcEventQuery.DeepClone().AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"RFN={controlledIncident.Number}");

			AssertNotNull("(Uncontrolled Incident)An DTC event should be found in the incident.", uncontrolledIncident.Logs.Find(uncontrolledDTC).FirstOrDefault());
			AssertNotNull("(Controlled Incident)An DTC event should be found in the incident.", controlledIncident.Logs.Find(controlledDTC).FirstOrDefault());

			AssertNotNull("(Uncontrolled Incident)An DTC event should be found in the group.", group.Logs.Find(uncontrolledDTC).FirstOrDefault());
			AssertNotNull("(Controlled Incident)An DTC event should be found in the group.", group.Logs.Find(controlledDTC).FirstOrDefault());

			var uckEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UnlockForEditCode).AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"JOB={group.Number}");
			var uncontrolledUCK = uckEventQuery.DeepClone().AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"RFN={uncontrolledIncident.Number}");
			var controlledUCK = uckEventQuery.DeepClone().AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"RFN={controlledIncident.Number}");

			AssertNull("Any UCK event should not be found in the uncontrolled incident.", uncontrolledIncident.Logs.Find(uncontrolledUCK).FirstOrDefault());
			AssertNotNull("An UCK event should be found in the controlled incident.", controlledIncident.Logs.Find(controlledUCK).FirstOrDefault());

			AssertNull("Group should not have the UCK event about uncontrolled incident.", group.Logs.Find(uncontrolledUCK).FirstOrDefault());
			AssertNotNull("An UCK event should be found in the group.", group.Logs.Find(controlledUCK).FirstOrDefault());

			group.ING_SourceModuleId = "NTZZ";
			factory.Save();

			AssertNotEquals("The product details of the incident that is not linked to group should not sync with group after detaching", group.ING_SourceModuleId, uncontrolledIncident.IM_SourceModuleId);
			AssertNotEquals("The product details of the incident that is not linked to group should not sync with group after detaching", group.ING_SourceModuleId, controlledIncident.IM_SourceModuleId);

			AssertEquals("Product area should be recalculated", "FIN", uncontrolledIncident.ProductArea);
			AssertEquals("Product area should be recalculated", "FIN", controlledIncident.ProductArea);
		}

		public void TestTesksShouldNotBeReopenedWhenIncidentCompletedIsTrue()
		{
			var factory = new BusinessObjectFactory();
			var registryCollection = new IncidentGroupTypeCollection();
			registryCollection.RemoveAll();

			var groupType = registryCollection.AddNew();
			groupType.GroupType = "MIM";
			var invStage = groupType.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCollection);

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";

			var controlledIncident = factory.NewWithValidTestData<SupportIncident>();
			var task = controlledIncident.WorkflowItems.AddNew();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			factory.Save();

			var group = factory.NewWithValidTestData<IncidentManagementGroup>();
			var link = factory.New<IncidentManagementLink>();

			group.ING_Type = "MIM";
			group.ING_Status = "INV";

			link.INL_IM_Incident = controlledIncident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			factory.Save();
			group.Reload();

			AssertEquals("The Controlled incident's tasks should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
			AssertNotNull("The Controlled incident's tasks should be cancelled by CNC event", controlledIncident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Cancelled.Code)).FirstOrDefault());

			using (var form = new TestForm(group))
			{
				form.Show();
				var grid = form.LinkedGrid;
				grid.InnerGrid.SelectAllElements();

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				grid.DetachButtonForTest.PerformClick();
				factory.Save();
			}

			Assert("Current stage's IncidentComplete should be true", group.NowStage.IncidentCompleted);
			AssertEquals("The Controlled incident's tasks should keep up cancelled if IncidentCompleted is true", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
		}

		public void TestNeedsSaveToShowEditForm()
		{
			using (var grid = new LinkedIncidentsModuleButtonGrid())
			{
				var mInfoMethod =
					grid.GetType().GetMethod(
						"NeedsSaveToShowEditForm",
						BindingFlags.Instance | BindingFlags.NonPublic,
						Type.DefaultBinder,
						new[] { typeof(BusinessObject) },
						null);

				AssertEquals(false, (bool)mInfoMethod.Invoke(grid, new[] { (BusinessObject)null }));
			}
		}
	}

	[TestedType(typeof(IncidentManagementLinkCollectionFetchStrategy))]
	public class IncidentManagementLinkCollectionFetchStrategyTest : TestCaseWithFactory
	{
		IncidentManagementGroup CreateTestData(BusinessObjectFactory factory)
		{
			var group = factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "TST";

			for (var i = 0; i < 10; i++)
			{
				var link = CreateAnIncidentLink(group, i);
				Assert("Incident should be controlled", link.IsControlled);
				Assert("Incident\'s flagged should be true", link.Flagged);
			}

			return group;
		}

		IncidentManagementLink CreateAnIncidentLink(IncidentManagementGroup group, int number)
		{
			var factory = group.Factory;
			var incident = factory.New<SupportIncident>();
			incident.IM_IncidentNumber = $"CS0000{number}";
			incident.IM_RN_NKCountry = "CN";

			var org = factory.New<OrgHeader>();
			org.OH_Code = $"ClientOrg{number}";
			org.OH_FullName = $"ntorg{number}";
			org.OH_RL_NKClosestPort = "AUSYD";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = $"OCC{number}";
			contact.OC_Email = $"AOCCC{number}@123.com";

			var message = incident.EConversation.Conversation.Messages.AddNew();
			message.JCM_Body = $"MessageBody{number}";
			message.JCM_JCP_Participant = incident.EConversation.Conversation.Participants.GetOrAdd(contact).PK;

			incident.IM_OC_Contact = contact.PK;
			incident.IM_OH_Client = org.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, $"this is a defect{number}");

			var link = group.LinkedIncidents.AddNew();
			link.INL_ING_Group = group.PK;
			link.INL_IM_Incident = incident.PK;
			link.INL_IsGroupControlled = true;

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = $"ST{number}";
			staff.GS_FullName = $"ST{number}";
			link.INL_GS_NKResponder = staff.GS_Code;

			var workItem = factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_WorkItemNumber = $"WI000000{number}";
			incident.RelatedWorkItems.Add(workItem);

			return link;
		}

		public void TestCollectionFetchStrategy()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue.RemoveAndDeleteAll();
			var groupType = registryValue.AddNew();
			groupType.GroupType = "TST";
			foreach (var item in groupType.IncidentGroupStatusConfigurations)
			{
				item.ControlIncidents = true;
			}

			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var group = CreateTestData(Factory);
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ IncidentManagementLink.Schema.TableName, 1 },
				{ IncidentRequest.Schema.TableName, 0 },
				{ JobConversation.Schema.TableName, 0 },
				{ JobConversationParticipant.Schema.TableName, 0 },
				{ JobConversationMessage.Schema.TableName, 2 },
				{ SupportIncident.Schema.TableName, 1 },
				{ OrgHeader.Schema.TableName, 1 },
				{ GlbStaff.Schema.TableName, 1 },
				{ GenPivot.Schema.TableName, 1 },
				{ WorkItem.Schema.TableName, 1 },
				{ StmALog.Schema.TableName, 1 }
			};

			var userFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var userGroup = userFactory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, group.PK));

			using (var userForm = new GridForm(userGroup))
			{
				userForm.Show();
				var grid = userForm.LinkedGrid;
				AssertNotNull(grid);
				AssertEquals("Currently 19 columns has been added(If this test failed, that means you added new column(s) to the LinkedIncidentGrid. Please check whether the columns can be refreshed when another session edited the values and update IncidentManagementLinkCollectionFetchStrategy)", 19, grid.ColumnStyles.Count);
				Application.DoEvents();
				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, userFactory))
				{
					userFactory.ResetDatabaseLoadCount();
					userForm.LinkedGrid.ForceRefresh();
				}
			}
		}

		class GridForm : ZForm
		{
			public GridForm(IncidentManagementGroup group) : base()
			{
				var userControlGrid = controlCenter.Controls.Find("LinkedIncidentsGrid", true).First() as LinkedIncidentsModuleButtonGrid;
				LinkedGrid = new LinkedIncidentsModuleButtonGrid();

				this.SuspendLayout();
				this.SuspendDrawing();
				LinkedGrid.SuspendLayout();
				LinkedGrid.SuspendDrawing();
				LinkedGrid.InnerGrid.SuspendLayout();
				LinkedGrid.InnerGrid.SuspendDrawing();

				this.SetDataBinding(group, "");
				this.BindingSource.SetBindingMember(LinkedGrid, "LinkedIncidents");

				foreach (ZGridColumnInfo column in userControlGrid.InnerGrid.ColumnStyles)
				{
					LinkedGrid.ColumnStyles.Add(column);
				}

				LinkedGrid.InnerGrid.SetAllColumnsVisible(true);
				LinkedGrid.BindToFindBoxList = "Lookups+IncidentsNotLinked";
				this.Controls.Add(LinkedGrid);
				LinkedGrid.Dock = DockStyle.Fill;

				LinkedGrid.InnerGrid.ResumeDrawing();
				LinkedGrid.InnerGrid.ResumeLayout(false);
				LinkedGrid.ResumeDrawing();
				LinkedGrid.ResumeLayout(false);
				this.ResumeDrawing();
				this.ResumeLayout(false);
			}

			public LinkedIncidentsModuleButtonGrid LinkedGrid { get; private set; }

			readonly IncidentManagementControlCenterUserControl controlCenter = new IncidentManagementControlCenterUserControl();

			protected override void Dispose(bool disposing)
			{
				controlCenter.Dispose();
				base.Dispose(disposing);
			}
		}
	}

	class LinkedIncidentsGridAttacherForTest : LinkedIncidentsModuleButtonGrid.LinkedIncidentsGridAttacher
	{
		public LinkedIncidentsGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, IncidentManagementGroup incidentManagementGroup)
			: base(destinationCollection, findBoxList, moduleID, incidentManagementGroup)
		{
		}

		public bool TestAttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}

	class TestForm : IncidentManagementGroupForm
	{
		readonly IncidentManagementControlCenterUserControl userControl;

		public IncidentManagementControlCenterUserControl UserControl => Controls.Find("incidentManagementControlCenterUserControl", true)[0] as IncidentManagementControlCenterUserControl;

		public TestForm(IncidentManagementGroup incidentManagementGroup) : base(incidentManagementGroup)
		{
			this.MainTabControl.SelectTab("incidentControlCenterTabPage");
			LinkedGrid.InnerGrid.SetAllColumnsVisible(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && userControl != null)
			{
				userControl.Dispose();
			}

			base.Dispose(disposing);
		}

		public LinkedIncidentsModuleButtonGrid LinkedGrid => UserControl.Controls.Find("LinkedIncidentsGrid", true).First() as LinkedIncidentsModuleButtonGrid;
		public IncidentManagementGroupControlCenterCommunicationAreaUserControl CommunicationArea => UserControl.Controls.Find("CommunicationArea", true).First() as IncidentManagementGroupControlCenterCommunicationAreaUserControl;
	}
}
