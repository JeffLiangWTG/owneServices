using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(StmServiceTaskForm))]
	public class StmServiceTaskFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var taskSchedule = Factory.New<StmServiceTask>();
			return GetFormToBashWithExcludedCaptions(taskSchedule);
		}

		public void TestFormCaption()
		{
			CombineAssertions(() =>
			{
				Test(string.Empty, "Service Schedule Task ");
				Test(null, "Service Schedule Task ");
				Test("AD", "Service Schedule Task AD");
				Test("BGC", "Service Schedule Task BGC");
			});

			void Test(string serviceTaskCode, string expectedCaption)
			{
				// Arrange
				var serviceTaskSchedule = Factory.New<StmServiceTask>();
				serviceTaskSchedule.SST_ServiceTaskCode = serviceTaskCode;

				// Act
				using var serviceTaskForm = new StmServiceTaskForm(serviceTaskSchedule);

				// Assert
				AssertEquals("FormCaption should be Service Schedule Task", expectedCaption, serviceTaskForm.FormCaption);
			}
		}

		[RequiresSTA]
		public void TestDisableNewAction()
		{
			// Arrange
			var serviceTask = Factory.NewWithValidTestData<StmServiceTask>();
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;

			Factory.Save();
			using (var serviceTaskForm = new StmServiceTaskForm(serviceTask))
			{
				// Act
				serviceTaskForm.Show();

				// Assert
				AssertEquals("Display mode should be NewSaved", ODisplayMode.NewSaved, serviceTaskForm.DisplayMode);
			}
		}

		[RequiresSTA]
		public void TestBranchVisibility()
		{
			hostedServiceAttributeProviderDisposable.Dispose();

			CombineAssertions(() =>
			{
				Test(true, false);
				Test(false, true);
			});

			void Test(bool canRunInAnyBranch, bool expected)
			{
				// Arrange
				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.CanRunInAnyBranch == canRunInAnyBranch &&
					a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var serviceTask = Factory.New<StmServiceTask>();

					using (var form = new StmServiceTaskForm(serviceTask))
					{
						// Act
						form.Show();

						// Assert
						var branchFindBox = form.Controls.Find("BranchFindBox", true)[0];
						AssertEquals(expected, branchFindBox.Visible);
					}
				}
			}
		}

		public void TestIsMandatory()
		{
			hostedServiceAttributeProviderDisposable.Dispose();

			CombineAssertions(() =>
			{
				Test(true, true);
				Test(false, false);
			});

			void Test(bool isMandatory, bool expected)
			{
				// Arrange
				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.IsMandatory == isMandatory &&
					!a.IsScheduleReadOnly &&
					!a.IsReadOnlyForWiseCloudClient &&
					a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var serviceTask = Factory.New<StmServiceTask>();
					using (var form = new StmServiceTaskForm(serviceTask))
					{
						// Act
						form.Show();

						// Assert
						var isActive = (ZCheckBox)form.Controls.Find("IsActiveCheckBox", true)[0];
						AssertEquals(expected, isActive.ReadOnly);
					}
				}
			}
		}

		public void TestDefaultSecondaryProcessesMaxCount()
		{
			// Arrange
			hostedServiceAttributeProviderDisposable.Dispose();
			var mockDefaultSchedule = Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes");

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.AllowsMultipleInstances &&
				a.DefaultSchedule == mockDefaultSchedule);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var taskSchedule = Factory.New<StmServiceTask>();

				// Act
				using var form = new FormForTest(taskSchedule);
				form.Show();

				// Assert
				form.MainTabControl_Exposed.SelectTab("ExtendedConfigTabPage");
				var extendedConfigControl = form.Controls.Find("ExtendedConfigControl", true)[0];
				var dropEditSecondaryProcessesMaxCountControl = (ZDropEdit)extendedConfigControl.Controls.Find("dropEditSecondaryProcessesMaxCount", true)[0];
				AssertEquals("SYSTEM MANAGED", dropEditSecondaryProcessesMaxCountControl.Text);
				AssertEquals(101, dropEditSecondaryProcessesMaxCountControl.List.Count);
			}
		}

		public void TestNudgeableTaskSchedule_RecurrenceControl_Disabled_And_DefaultSchedule_Control_Shifted_Up()
		{
			// Arrange
			var taskScheduleNonNudgeable = Factory.New<StmServiceTask>();

			using var formWithNonNudgeableTask = new FormForTest(taskScheduleNonNudgeable);
			formWithNonNudgeableTask.Show();
			var fullMinimumSize = formWithNonNudgeableTask.MinimumSize;
			var recurrenceControl = formWithNonNudgeableTask.FindSingle<ZUserControl>("RecurrenceControl");

			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
				{
						new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
						new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
						new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
						new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null),
				})))
			{
				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

				var taskSchedule = Factory.NewWithValidTestData<StmServiceTask>();
				taskSchedule.SST_ServiceTaskCode = "111";

				var serviceTaskBusinessObjectBindingEnabled = SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled;

				try
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;

					Assert(ObjectFactory.Get<ISystemDataRegistry>().ServiceTaskBusinessObjectBindingEnabled);
					AssertEquals(2, taskSchedule.ServiceTaskBindingsCount);
					Assert("Service task having business object bindings is nudge-able", taskSchedule.IsNudgeable);

					using var form = new FormForTest(taskSchedule);

					// Act
					form.Show();

					var topPanelControl = form.FindSingle<ZPanel>("TopPanel");
					var hiddenRecurrenceControl = form.Controls.Find("RecurrenceControl", true)[0];
					var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
					var recurrenceGroupBox = hiddenRecurrenceControl.Controls.Find("MainGroupBox", true)[0];
					var expectedMininumFormSize = ControlDpiScalingHelper.NewScaledSize(
						ControlDpiScalingHelper.UnscaleFromCurrentDpiX(fullMinimumSize.Width),
						ControlDpiScalingHelper.UnscaleFromCurrentDpiY(fullMinimumSize.Height) - ControlDpiScalingHelper.UnscaleFromCurrentDpiY(recurrenceControl.Height)
					);

					// Assert
					CombineAssertions(() =>
					{
						Assert("Recurrence MainGroupBox is made hidden", !recurrenceGroupBox.Visible);
						AssertEquals("DefaultScheduleControl location is the same as RecurrenceControl", recurrenceControl.Location, defaultScheduleControl.Location);
						AssertEquals("Form minimum size is reduced", expectedMininumFormSize, form.MinimumSize);
						AssertEquals("DefaultScheduleControl should be the first in the z-order for TopPanel", 0, topPanelControl.Controls.GetChildIndex(defaultScheduleControl));
					});
				}
				finally
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = serviceTaskBusinessObjectBindingEnabled;
				}
			}
		}

		public void TestDefaultScheduleControlVisibleAndDisabled()
		{
			// Arrange
			var taskSchedule = Factory.New<StmServiceTask>();
			taskSchedule.SST_ServiceTaskCode = "ABC";

			// Act
			using var form = new FormForTest(taskSchedule);
			form.Show();

			// Assert
			var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
			var defaultScheduleGroupBox = (ZGroupBox)(defaultScheduleControl.Controls.Find("MainGroupBox", true)[0]);

			CombineAssertions(() =>
			{
				Assert("DefaultScheduleControl is visible", defaultScheduleControl.Visible);
				Assert("DefaultScheduleGroupBox is visible", defaultScheduleGroupBox.Visible);
				Assert("DefaultScheduleGroupBox is disabled", !defaultScheduleGroupBox.Enabled);
			});
		}

		public void TestDefaultScheduleControlDisplaysPanelsCorrectly()
		{
			hostedServiceAttributeProviderDisposable.Dispose();

			CombineAssertions(() =>
			{
				Test("20seconds", "SecondPanel", null, null);
				Test("15minutes", "MinutePanel", null, null);
				Test("2hours", "HourlyPanel", null, null);
				Test("1days", "DailyPanel", null, null);
				Test("2weeks", "WeeklyPanel", null, DayOfWeek.Sunday);
				Test("1month", "MonthlyPanel", 1, null);
				Test("1year", "YearlyPanel", null, null);
			});

			void Test(string scheduleOccurence, string panelName, int? dayOfMonth, params DayOfWeek[] daysOfWeek)
			{
				// Arrange
				var mockDefaultSchedule = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == scheduleOccurence &&
					d.DayOfMonth == (dayOfMonth ?? 0) &&
					d.DaysOfWeek == daysOfWeek);

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == mockDefaultSchedule);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var taskSchedule = Factory.New<StmServiceTask>();
					taskSchedule.SST_ServiceTaskCode = "ABC";

					// Act
					using var form = new StmServiceTaskForm(taskSchedule);
					form.Show();

					// Assert
					var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
					var panelControl = (ZPanel)defaultScheduleControl.Controls.Find(panelName, true)[0];

					Assert($"{panelName} is visible", panelControl.Visible);
				}
			}
		}

		public void TestDefaultScheduleRandomStartOffsetIsVisibleWhenSet()
		{
			hostedServiceAttributeProviderDisposable.Dispose();

			CombineAssertions(() =>
			{
				Test("20seconds", "SecondlyRandomStartOffset", null, null);
				Test("15minutes", "MinuteRandomStartOffset", null, null);
				Test("2hours", "HourlyRandomStartOffset", null, null);
				Test("1days", "DailyRandomStartOffset", null, null);
				Test("2weeks", "WeeklyRandomStartOffset", null, DayOfWeek.Sunday);
				Test("1month", "MonthlyRandomStartOffset", 1, null);
				Test("1year", "YearlyRandomStartOffset", null, null);
			});

			void Test(string scheduleOccurence, string controlName, int? dayOfMonth, params DayOfWeek[] daysOfWeek)
			{
				// Arrange
				var mockDefaultSchedule = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == scheduleOccurence &&
					d.DayOfMonth == (dayOfMonth ?? 0) &&
					d.DaysOfWeek == daysOfWeek &&
					d.RandomStartOffset == "2hours");

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == mockDefaultSchedule);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var taskSchedule = Factory.New<StmServiceTask>();
					taskSchedule.SST_ServiceTaskCode = "ABC";

					// Act
					using var form = new FormForTest(taskSchedule);
					form.Show();

					// Assert
					var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
					var randomStartOffsetControl = defaultScheduleControl.Controls.Find(controlName, true)[0];

					Assert($"{controlName} should be visible", randomStartOffsetControl.Visible);
				}
			}
		}

		public void TestDefaultScheduleRandomStartOffsetHiddenWhenNotSet()
		{
			hostedServiceAttributeProviderDisposable.Dispose();

			CombineAssertions(() =>
			{
				Test("20seconds", "SecondlyRandomStartOffset", null, null);
				Test("15minutes", "MinuteRandomStartOffset", null, null);
				Test("2hours", "HourlyRandomStartOffset", null, null);
				Test("1days", "DailyRandomStartOffset", null, null);
				Test("2weeks", "WeeklyRandomStartOffset", null, DayOfWeek.Sunday);
				Test("1month", "MonthlyRandomStartOffset", 1, null);
				Test("1year", "YearlyRandomStartOffset", null, null);
			});

			void Test(string scheduleOccurence, string controlName, int? dayOfMonth, params DayOfWeek[] daysOfWeek)
			{
				// Arrange
				var mockDefaultSchedule = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == scheduleOccurence &&
					d.DayOfMonth == (dayOfMonth ?? 0) &&
					d.DaysOfWeek == daysOfWeek);

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == mockDefaultSchedule);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var taskSchedule = Factory.New<StmServiceTask>();
					taskSchedule.SST_ServiceTaskCode = "ABC";

					// Act
					using var form = new FormForTest(taskSchedule);
					form.Show();

					// Assert
					var defaultScheduleControl = form.Controls.Find("DefaultScheduleControl", true)[0];
					var randomStartOffsetControl = defaultScheduleControl.Controls.Find(controlName, true)[0];

					Assert($"{controlName} should not be visible", !randomStartOffsetControl.Visible);
				}
			}
		}

		public void TestLoggingTabNotShown()
		{
			var taskSchedule = Factory.New<StmServiceTask>();
			taskSchedule.SST_ServiceTaskCode = "ABC";

			// Act
			using var form = new FormForTest(taskSchedule);
			form.Show();

			// Assert
			Assert("LogsTabPage should not be found", form.Controls.Find("LogsTabPage", true).Length.Equals(0));
		}

		public void TestAuditTabShown()
		{
			var taskSchedule = Factory.New<StmServiceTask>();
			taskSchedule.SST_ServiceTaskCode = "ABC";

			// Act
			using var form = new StmServiceTaskForm(taskSchedule);
			form.Show();

			// Assert
			Assert(form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
		}

		public void TestSaveButtonUserControlInBottomRight()
		{
			var taskSchedule = Factory.New<StmServiceTask>();
			taskSchedule.SST_ServiceTaskCode = "ABC";

			// Act
			using var form = new StmServiceTaskForm(taskSchedule);
			form.Show();

			// Assert
			var saveButtonControl = form.Controls.Find("SaveButtonUserControl", true)[0];
			AssertEquals("SaveButtonUserControl should stay in bottom right on resize", saveButtonControl.Anchor, AnchorStyles.Bottom | AnchorStyles.Right);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ObjectFactory.DisposeSubstitutions();

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes") &&
				a.Code == "ABC" &&
				a.Description == "ABC Description");
			var hostedServiceProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(a =>
				a.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceMock);
			hostedServiceAttributeProviderDisposable = ObjectFactory.Substitute(hostedServiceProviderMock);
		}

		protected override void TearDown()
		{
			hostedServiceAttributeProviderDisposable?.Dispose();
			base.TearDown();
		}

		IDisposable hostedServiceAttributeProviderDisposable;

		public static ZForm GetFormToBashWithExcludedCaptions(StmServiceTask serviceTask)
		{
			var formToBash = new StmServiceTaskForm(serviceTask);
			var stmServiceTaskRecurrenceControl = formToBash.Controls.Find("RecurrenceControl", searchAllChildren: true).Single();

			MissingResourceStringChecker.ExcludeFromTest(stmServiceTaskRecurrenceControl.Controls.Find("DailyDaysNumber", searchAllChildren: true).Single());
			MissingResourceStringChecker.ExcludeFromTest(stmServiceTaskRecurrenceControl.Controls.Find("MonthlyDate", searchAllChildren: true).Single());
			MissingResourceStringChecker.ExcludeFromTest(stmServiceTaskRecurrenceControl.Controls.Find("MonthlyNumWeek", searchAllChildren: true).Single());
			MissingResourceStringChecker.ExcludeFromTest(stmServiceTaskRecurrenceControl.Controls.Find("MonthlyWeekDayDropEdit", searchAllChildren: true).Single());
			MissingResourceStringChecker.ExcludeFromTest(stmServiceTaskRecurrenceControl.Controls.Find("YearlyMonth", searchAllChildren: true).Single());
			MissingResourceStringChecker.ExcludeFromTest(stmServiceTaskRecurrenceControl.Controls.Find("YearlyWeekNum", searchAllChildren: true).Single());
			MissingResourceStringChecker.ExcludeFromTest(stmServiceTaskRecurrenceControl.Controls.Find("YearlyWeekDayDropEdit", searchAllChildren: true).Single());

			return formToBash;
		}

		class FormForTest : StmServiceTaskForm
		{
			public FormForTest(StmServiceTask taskSchedule)
				: base(taskSchedule)
			{
			}

			public ZTemplateTabControl MainTabControl_Exposed { get { return MainTabControl; } }
		}
	}
}
