using System.Drawing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class DateAcceptabilityPictureTest : BMSTestCaseWithFactory
	{
		public void TestImage_Workflow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var system = config.System;
			var board = VisualBoardsTestHelper.CreateBoard(system, "Test");
			var bucket = config.Bucket;
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.CardContent).Returns(new TaskCardContent(task, sectionViewModel));
			using (var control = new DateAcceptabilityPicture(parent.Object))
			{
				AssertEquals(true, control.Visible);
				control.SetDataBinding(null, ".");
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(GUI.Properties.Resources.DA1, (Bitmap)control.BackgroundImage);
			}
		}

		public void TestImage_JobHeader()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			jobHeader.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var system = config.System;
			var board = VisualBoardsTestHelper.CreateBoard(system, "Test");
			var bucket = config.Bucket;
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.CardContent).Returns(new TaskCardContent(task, sectionViewModel));
			using (var control = new DateAcceptabilityPicture(parent.Object))
			{
				control.SetDataBinding(null, ".");

				AssertEquals(true, control.Visible);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(GUI.Properties.Resources.DA1, (Bitmap)control.BackgroundImage);
			}
		}

		public void TestImage_None()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var system = config.System;
			var board = VisualBoardsTestHelper.CreateBoard(system, "Test");
			var bucket = config.Bucket;
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.CardContent).Returns(new TaskCardContent(task, sectionViewModel));
			using (var control = new DateAcceptabilityPicture(parent.Object))
			{
				control.SetDataBinding(null, ".");

				AssertEquals(false, control.Visible);
				AssertNull(control.BackgroundImage);

				workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;

				control.SetDataBinding(null, ".");

				AssertEquals(true, control.Visible);
				BufferManagementSystemDrawerTest.AssertImagePixelsEqual(GUI.Properties.Resources.DA1, (Bitmap)control.BackgroundImage);
			}
		}

		[TestDate(2013, 5, 15)]
		public void TestShowDateAcceptabilityBalloon()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			workflow.JobHeader.FH_DoNotStartBeforeDate = ZDateTime.Today.AddDays(-1);
			workflow.FH_AgreedDeliveryDate = ZDateTime.Today.AddDays(1);

			Factory.Save();
			var cardContent = new TaskCardContent(task, null);
			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.CardContent).Returns(cardContent);
			parent.Setup(m => m.Task).Returns(cardContent.Task);
			using (var control = new DateAcceptabilityPicture(parent.Object))
			{
				AssertEquals(@"Can start earlier than the Agreed Delivery Date, can finish after the Agreed Delivery Date

Do not start before: 14-May-13
Agreed delivery date: 16-May-13
Right-click for more information...", ToolTipService.GetToolTip(control));
			}
		}

		public void TestClick()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var parent = new Mock<ITaskCardComponentParent>();
			parent.Setup(m => m.CardContent).Returns(new TaskCardContent(task, null));
			using (var control = new DateAcceptabilityPicture(parent.Object))
			{
				AssertEquals(true, control.Visible);
				control.ShowDateAcceptabilityLegend();

				AssertType<DateAcceptabilityLegendForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
