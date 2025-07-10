using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[UseSnapshotProtection]
	class ShortcutNonTransactionedTest : TestCase
	{
		public void TestRefresh()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Factory1", RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { NameForDebugging = "Factory2", RefreshEnabled = false };
			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(factory1, "AAA");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = factory1.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);

			BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);

			factory1.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();

				var cardControl = form.FindAll<TaskCardControl>().SingleOrDefault();

				AssertNotNull(cardControl);

				var loadedTask = factory2.Load<ProcessTask>(cardControl.CardContent.TaskIdentifier);
				loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				factory2.Save();

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();

				cardControl = form.FindAll<TaskCardControl>().SingleOrDefault();

				AssertNull(cardControl);
			}
		}

		public void TestRefresh_ZDateTimeField()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Factory1", RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { NameForDebugging = "Factory2", RefreshEnabled = false };
			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(factory1, "AAA");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = factory1.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);

			BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);

			factory1.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();

				var cardControl = form.FindAll<TaskCardControl>().SingleOrDefault();

				AssertNotNull(cardControl);

				var loadedTask = factory2.Load<ProcessTask>(cardControl.CardContent.TaskIdentifier);
				loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				factory2.Save();

				cardControl.ShowDetailedCard();
				Application.DoEvents();

				var zDateTimeBox = form.FindAll<ZDateEdit>().FirstOrDefault();
				zDateTimeBox.Focus();
				Application.DoEvents();

				form.OnKeyDown_ForTest(new KeyEventArgs(Keys.F5));
				form.AwaitAll();

				cardControl = form.FindAll<TaskCardControl>().SingleOrDefault();

				AssertNotNull(cardControl); // still not null because refresh didn't occur.
			}
		}
	}

	class ShortcutTest : BMSTestCaseWithFactory
	{
		public void TestNextAndPreviousChannel_OnlyWorksInBoardMeetingMode()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, cellsPerSubsection: 13, flowDirection: FlowDirectionList.Codes.Up, maxOverdueSlots: 0, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime);
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);
			Factory.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;
				var startingChannelWidth = control.Table.ColumnStyles[2].Width;
				AssertEquals("Pre Condition - Channels are not expanded", startingChannelWidth, control.Table.ColumnStyles[2].Width);
				AssertEquals("Pre Condition - Channels are not expanded", startingChannelWidth, control.Table.ColumnStyles[3].Width);

				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.Right);
				Application.DoEvents();
				AssertEquals("Navigation should not be enabgled outside of board meeting mode", startingChannelWidth, control.Table.ColumnStyles[2].Width);
				AssertEquals("Navigation should not be enabgled outside of board meeting mode", startingChannelWidth, control.Table.ColumnStyles[3].Width);

				form.EnterBoardMeetingMode();

				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.Right);
				Application.DoEvents();
				AssertEquals("Navigation should be enabgled inside of board meeting mode", startingChannelWidth, control.Table.ColumnStyles[2].Width);
				AssertEquals("Navigation should be enabgled inside of board meeting mode", startingChannelWidth * 3, control.Table.ColumnStyles[3].Width);

				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.Left);
				Application.DoEvents();
				AssertEquals("Navigation should be enabgled inside of board meeting mode", startingChannelWidth * 3, control.Table.ColumnStyles[2].Width);
				AssertEquals("Navigation should be enabgled inside of board meeting mode", startingChannelWidth, control.Table.ColumnStyles[3].Width);
			}
		}

		public void TestClearPopups()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow = BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);
			var task = workflow.Tasks.First();
			workflow.FH_CompletionStatement = "not ponies";

			Factory.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				var taskCard = form.FindAll<TaskCardControl>().First();
				taskCard.OnTaskCardControlClicked();

				AssertNotEquals(form.FindAll<TaskCardDetailControl>().Count(), 0);

				BMSFormTestHelper.PressHotkeys(form, Keys.Escape);
				Application.DoEvents();

				AssertEquals(form.FindAll<TaskCardDetailControl>().Count(), 0);
			}
		}

		public void TestSave()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow = BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);
			var task = workflow.Tasks.First();
			workflow.FH_CompletionStatement = "berkflow";
			task.P9_CardNote = "not ponies";

			AssertEquals(task.ProcessHeader, workflow);
			Factory.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var taskCard = form.FindAll<TaskCardControl>().First();
				taskCard.ShowDetailedCard();
				Application.DoEvents();

				var taskDetailedCard = form.FindAll<TaskCardDetailControl>().First();
				var textBox = taskDetailedCard.FindAll<ZTextBox>().Single(t => t.Text == "not ponies");
				textBox.Focus();
				textBox.Text = "still not ponies";
				Application.DoEvents();

				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.S);

				Application.DoEvents();
			}

			var loadedTask = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			AssertEquals(loadedTask.P9_CardNote, "still not ponies");
		}

		public void TestSave_CantSaveWithoutSaveButton()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow = BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);
			var task = workflow.Tasks.First();
			workflow.FH_CompletionStatement = "berkflow";
			task.P9_CardNote = "not ponies";

			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section.Board, customisation);

			AssertEquals(task.ProcessHeader, workflow);
			Factory.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var taskCard = form.FindAll<TaskCardControl>().First();
				taskCard.ShowDetailedCard();
				Application.DoEvents();

				var taskDetailedCard = form.FindAll<TaskCardDetailControl>().First();

				AssertNull(taskDetailedCard.FindAll<SaveButton>().FirstOrDefault());

				task.P9_CardNote = "still not ponies";
				Application.DoEvents();

				KeyEventArgs e = new KeyEventArgs(Keys.Control | Keys.S);
				form.OnKeyDown_ForTest(e);

				Application.DoEvents();
			}

			var loadedTask = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			AssertEquals(loadedTask.P9_CardNote, "not ponies");
		}
	}
}
