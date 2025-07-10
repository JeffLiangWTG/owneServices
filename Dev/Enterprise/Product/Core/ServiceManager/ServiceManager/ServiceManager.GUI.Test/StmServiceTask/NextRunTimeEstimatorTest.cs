using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	class NextRunTimeEstimatorControlTest : ZFormBasherTest
	{
		[TestDate(2024, 1, 1, 11, 0, 0)]
		public void TestRunTimesDisplayAndReloadOnChange()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 1 };
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;

			using var form = new StmServiceTaskForm(serviceTask);

			// Act
			form.Show();

			Assert("Single NextRunTimeEstimator control should be found", TryGetChildControl(form, "NextRunTimeEstimatorControl", out var estimatorControl));
			Assert("Single RunTimeEstimator grid should be found", TryGetChildControl(estimatorControl, "NextRunTimeEstimatorGrid", out var estimatorGridControl));

			var estimatorGrid = estimatorGridControl as ZDisplayGrid;
			estimatorGrid.Select(0);
			var firstDate = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTime;

			// Assert
			AssertEquals(ZDateTime.UtcNow.AddMinutes(1), firstDate);

			// Act
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorHours { Period = 1 };
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow; // Resetting the runtime as updating the calculator will recalculate next runtime
			estimatorGrid.Select(0);
			firstDate = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTime;

			// Assert
			AssertEquals(ZDateTime.UtcNow.AddHours(1), firstDate);
		}

		[TestDate(2024, 1, 1, 11, 0, 0)]
		public void TestRunTimesReloadsOnUpdateOfNextRunTime()
		{
			AssertRunTimesReload((StmServiceTask task) => task.NextRunTime = ZDateTime.UtcNow.AddHours(1), ZDateTime.UtcNow.AddHours(1).AddMinutes(1));
		}

		[TestDate(2024, 1, 1, 11, 0, 0)]
		public void TestRunTimesReloadsOnUpdateOfNextRunTimeCalculator()
		{
			AssertRunTimesReload((StmServiceTask task) => task.NextRunTimeCalculator = new NextRunTimeCalculatorHours() { Period = 1 }, ZDateTime.UtcNow.AddHours(2));
		}

		void AssertRunTimesReload(Action<StmServiceTask> action, ZDateTime expectedEstimatedTime)
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 1 };
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;

			using var form = new StmServiceTaskForm(serviceTask);

			// Act
			form.Show();

			Assert("Single NextRunTimeEstimator control should be found", TryGetChildControl(form, "NextRunTimeEstimatorControl", out var estimatorControl));
			Assert("Single RunTimeEstimator grid should be found", TryGetChildControl(estimatorControl, "NextRunTimeEstimatorGrid", out var estimatorGridControl));

			var estimatorGrid = estimatorGridControl as ZDisplayGrid;
			estimatorGrid.Select(0);
			var firstDate = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTime;

			// Assert
			AssertEquals(ZDateTime.UtcNow.AddMinutes(1), firstDate);

			// Act
			action(serviceTask);
			estimatorGrid.Select(0);
			firstDate = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTime;

			// Assert
			AssertEquals(expectedEstimatedTime, firstDate);
		}

		[TestDate(2024, 1, 1, 11, 0, 0)]
		public void TestNextRunTimeLocalDisplayAndReloadOnChange()
		{
			// Arrange
			var nextRunTimeUtc = DateTime.UtcNow;
			var nextRunTimeLocal = nextRunTimeUtc.ToLocalTime();

			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 1 };
			serviceTask.SST_NextRunTime = nextRunTimeUtc;

			using var form = new StmServiceTaskForm(serviceTask);

			// Act
			form.Show();

			Assert("Single NextRunTimeEstimator control should be found", TryGetChildControl(form, "NextRunTimeEstimatorControl", out var estimatorControl));
			Assert("Single RunTimeEstimator grid should be found", TryGetChildControl(estimatorControl, "NextRunTimeEstimatorGrid", out var estimatorGridControl));

			var estimatorGrid = estimatorGridControl as ZDisplayGrid;
			estimatorGrid.Select(0);
			var firstDate = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTimeLocal;

			// Assert
			AssertEquals(nextRunTimeLocal.AddMinutes(1), firstDate);

			// Act
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorHours { Period = 1 };
			serviceTask.SST_NextRunTime = nextRunTimeUtc; // Resetting the runtime as updating the calculator will recalculate next runtime
			estimatorGrid.Select(0);
			firstDate = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTimeLocal;

			// Assert
			AssertEquals(nextRunTimeLocal.AddHours(1), firstDate);
		}

		[TestDate(2024, 1, 1, 11, 0, 0)]
		public void TestNextRunTimeLocalReloadsOnUpdateOfNextRunTime()
		{
			var nextRunTimeUtc = DateTime.Now;
			AssertRunTimesReload((StmServiceTask task) => task.NextRunTime = nextRunTimeUtc.AddHours(1),
				nextRunTimeUtc.ToLocalTime().AddHours(1).AddMinutes(1));
		}

		[TestDate(2024, 1, 1, 11, 0, 0)]
		public void TestNextRunTimeLocalReloadsOnUpdateOfNextRunTimeCalculator()
		{
			void ResetCalculator(StmServiceTask task)
			{
				task.NextRunTimeCalculator = new NextRunTimeCalculatorHours() { Period = 1 };
				task.SST_NextRunTime = ZDateTimeOffset.Now;
			}

			AssertRunTimesReload(
				ResetCalculator,
				ZDateTime.Now.AddHours(1));
		}

		[TestDate(2024, 1, 1, 11, 0, 0)]
		[TestTimeZone]
		public void TestNextRunTimeLocalMatchWithEstimatedRunTime()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 1 };
			serviceTask.SST_NextRunTime = ZDateTimeOffset.UtcNow;

			using var form = new StmServiceTaskForm(serviceTask);

			// Act
			form.Show();

			Assert("Single NextRunTimeEstimator control should be found", TryGetChildControl(form, "NextRunTimeEstimatorControl", out var estimatorControl));
			Assert("Single RunTimeEstimator grid should be found", TryGetChildControl(estimatorControl, "NextRunTimeEstimatorGrid", out var estimatorGridControl));

			var estimatorGrid = estimatorGridControl as ZDisplayGrid;
			estimatorGrid.Select(0);
			var firstDateLocal = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTimeLocal;
			var firstDateUtc = ((NextRunTimeRecord)estimatorGrid.GetFirstSelectedRow()).NextRunTime;

			// Assert
			AssertEquals("RunTimeEstimatorLocal should matches with RunTimeEstimatorUTC", firstDateUtc, firstDateLocal.ToDateTime().ToUniversalTime());
		}

		protected override Form GetFormToBashCore()
		{
			var taskSchedule = Factory.New<StmServiceTask>();
			return StmServiceTaskFormTest.GetFormToBashWithExcludedCaptions(taskSchedule);
		}

		bool TryGetChildControl(Control parentControl, string childControlName, out Control childControl)
		{
			var controls = parentControl.Controls.Find(childControlName, searchAllChildren: true);
			childControl = controls.Length == 1 ? controls[0] : null;

			return childControl is not null;
		}

		protected override void SetUp()
		{
			base.SetUp();

			ObjectFactory.DisposeSubstitutions();

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes") &&
				a.Code == "~T1");
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
	}
}
