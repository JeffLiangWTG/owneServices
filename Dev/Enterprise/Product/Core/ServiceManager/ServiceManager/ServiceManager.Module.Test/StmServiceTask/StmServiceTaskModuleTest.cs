using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.GUI;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(StmServiceTaskModule))]

	class StmServiceTaskModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.StmServiceTask;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			if (collection is StmServiceTaskCollection stmServiceTaskCollection)
			{
				var task = stmServiceTaskCollection.AddNew();
				task.SST_ServiceTaskCode = "~T1";
				task.SST_NextRunTime = ZDateTimeOffset.Now;
				collection.Factory.Save();
			}

			base.AddTestObjects(collection);
		}

		public void TestLogViewerForm()
		{
			using (var module = new StmServiceTaskModule())
			{
				using (var form = StmServiceHostMenuHelper.ShowLogForm())
				{
					Assert("Log Viewer Form must be shown", form is ServiceTaskLogViewerForm);
				}
			}
			if (ErrorReporter.LastMessageReported.StartsWith(@"Caught exception trying to execute delegate ""Enterprise.ServiceManager.HostClient.HttpClient") &&
				ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestShouldShowMaintenanceItems()
		{
			var originalHostedLocation = EnvProxy.HostedLocation;
			try
			{
				using (var module = new StmServiceTaskModule())
				{
					using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
					{
						EnvProxy.SetHostedLocationForTest("SYD");
						Assert("Support should be able to view the maintenance menu items for hosted systems.", module.ShouldShowMaintenanceItems());
						EnvProxy.SetHostedLocationForTest("");
						Assert("Support should be able to view the maintenance menu items for self-hosted systems.", module.ShouldShowMaintenanceItems());
					}

					using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
					{
						EnvProxy.SetHostedLocationForTest("SYD");
						Assert("Non-support should not be able to view the maintenance menu items for hosted systems.", !module.ShouldShowMaintenanceItems());
						EnvProxy.SetHostedLocationForTest("");
						Assert("Non-support should be able to view maintenance menu items for self-hosted systems.", module.ShouldShowMaintenanceItems());
					}
				}
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedLocation);
			}
		}

		[ExpectNoExceptions]
		public void TestLogViewerForm_NullParent()
		{
			var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.ConfigControlTypeAssemblyName == "A" &&
				a.ConfigControlTypeName == "B" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock);
			var hostedServiceAttributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceAttributeProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);
			using (ObjectFactory.Substitute(hostedServiceAttributeProviderMock.Object))
			using (var form = new ZForm())
			using (var module = new StmServiceTaskModule())
			{
				var filterControl = module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();

				module.GridCollection.Add(Factory.New<StmServiceTask>());
				module.DisplayGrid.Select(0);
				using (var logForm = StmServiceHostMenuHelper.ShowLogForm())
				{
					Assert("Log Viewer Form must be shown", logForm is ServiceTaskLogViewerForm);
				}
			}
			if (ErrorReporter.LastMessageReported.StartsWith(@"Caught exception trying to execute delegate ""Enterprise.ServiceManager.HostClient.HttpClient") &&
				ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		[StressTest]
		public override void TestAllGridColumnsCanBeExportedToExcel()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as ZFilterModule)
			{
				try
				{
					if (module.ModuleDecisionProvider.AllowExcelExport && module.FilterBusinessObject != null)
					{
						var filterStripControl = module.EmbeddedControl as IFilterControl;
						ZGrid grid = filterStripControl.FilteredGrid;

						// SchedulePeriodDuration column does not have Excel export capability
						var protectedColumns = new[] { "SchedulePeriodDuration" };
						foreach (ZGridColumnInfo style in grid.ColumnStyles.ToArray().Reverse())
						{
							if (protectedColumns.Contains(style.ColumnName))
							{
								grid.ColumnStyles.Remove(style);
							}
						}

						foreach (ZGridColumnInfo info in grid.ColumnStyles)
						{
							info.IsVisible = true;
						}

						((IFilterStripCommonControlInternalsForTesting)filterStripControl).Bind();

						grid.RefreshTableStyles();

						foreach (var column in grid.Columns)
						{
							if (!(column.ColumnStyle.PropertyDescriptor.PropertyType.IsInterface && column.ColumnStyle.PropertyDescriptor.PropertyType == typeof(IZType)))
							{
								var zInterface = column.ColumnStyle.PropertyDescriptor.PropertyType.GetInterface(nameof(IZType));
								AssertNotNull(string.Format("The property '{0}' of data source bound to ZGrid should implement IZType interface for exporting to excel", column.ColumnName), zInterface);
							}
						}
					}
					else
					{
						Assert("If excel export isn't supported on this module, no problem", true);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
					Assert("If the module isn't used for gui, no excel export will occur", true);
				}
			}
		}

		public void TestInstallSetsActiveTrue()
		{
			var host = ServiceManagerHelper.GetHostName();
			var serviceHost = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, host));
			var hostOriginallyInstalled = (serviceHost != null);
			var hostOriginallyActive = (serviceHost != null) ? serviceHost.SH_IsActive : ZBool.False;
			if (hostOriginallyInstalled)
			{
				serviceHost.Delete();
				Factory.Save();
				serviceHost = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, host));
			}
			AssertNull("Failed to remove service host", serviceHost);

			try
			{
				using (var module = new StmServiceTaskModule())
				{
					StmServiceHostMenuHelper.SetServiceHostActiveStatus(host, true);
					serviceHost = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, host));
					AssertNotNull("Service host was not added", serviceHost);
					Assert("Service host IsActive should be true", serviceHost.SH_IsActive);

					StmServiceHostMenuHelper.SetServiceHostActiveStatus(host, false);
					Assert("Service host IsActive should be false", !Factory.Load<StmServiceHost>(serviceHost.PK).SH_IsActive);

					StmServiceHostMenuHelper.SetServiceHostActiveStatus(host, true);
					Assert("Service host IsActive should be true", Factory.Load<StmServiceHost>(serviceHost.PK).SH_IsActive);
				}
			}
			finally
			{
				if (hostOriginallyInstalled)
				{
					serviceHost = Factory.New<StmServiceHost>();
					serviceHost.SH_HostName = host;
					serviceHost.SH_IsActive = hostOriginallyActive;
					Factory.Save();
				}
			}
		}

		public override void TestModuleShowsAndCanSearch()
		{
			// Arrange
			var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(o =>
				o.Description == "Dummy Description" && o.Category == "TST" &&
				o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			using (ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>(provider =>
					   provider.GetClientHostedServiceAttribute(It.IsAny<string>()) ==
					   hostedServiceConfigMock)))
			{
				// Act, Assert
				base.TestModuleShowsAndCanSearch();
			}
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;

		public class MiscellaneousTest : TestCaseWithFactory
		{
			[GuiTest]
			[ExpectNoExceptions]
			public void TestDelayedLoadQueriesServiceStatus()
			{
				// Arrange
				var task = CreateTask();
				var serviceTaskScheduleStatusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
				serviceTaskScheduleStatusProviderMock
					.Setup(provider => provider.GetServiceStatus())
					.Returns(new Dictionary<string, TaskInstanceStatus> { [task.SST_ServiceTaskCode.ToString()] = new TaskInstanceStatus(), });

				using (ObjectFactory.Substitute(serviceTaskScheduleStatusProviderMock.Object))
				using (var serviceTaskModule = new StmServiceTaskModule())
				{
					var filter = (StmServiceTaskFilterBusinessObject)serviceTaskModule.FilterBusinessObject;
					var collection = (StmServiceTaskCollection)serviceTaskModule.GridCollection;
					var activeStatusFilter = (ModuleTextFilter)filter["Active Status"];
					activeStatusFilter.IsActive = true;
					activeStatusFilter.Property = "Active";

					// Act
					serviceTaskModule.PerformSearch_ForTest();

					// Assert
					serviceTaskScheduleStatusProviderMock.Verify(provider => provider.GetServiceStatus(), Times.Once);
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				StmServiceTask CreateTask()
				{
					var schedule = Factory.New<StmServiceTask>();
					schedule.SST_ServiceTaskCode = "TST";
					schedule.SST_Active = true;
					return schedule;
				}
			}
		}

		sealed class ProxyHostAndDbServerTest : TestCase
		{
			public void TestMenuProxyHostAndDbServer()
			{
				using var zMenuItem = new ZMenuItem();
				var menuProxy = new StmServiceTaskModule.MenuProxy(zMenuItem).Add("xxx", null, "Host", "DbServer", true);
				AssertEquals("Host", menuProxy.Host);
				AssertEquals("DbServer", menuProxy.DbServer);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
			public void TestToolStripMenuProxyHostAndDbServer()
			{
				using var toolStripDropDownButton = new ToolStripDropDownButton();
				var menuProxy = new StmServiceTaskModule.ToolStripMenuProxy(toolStripDropDownButton).Add("xxx", null, "Host", "DbServer", true);
				AssertEquals("Host", menuProxy.Host);
				AssertEquals("DbServer", menuProxy.DbServer);
			}
		}
	}
}
