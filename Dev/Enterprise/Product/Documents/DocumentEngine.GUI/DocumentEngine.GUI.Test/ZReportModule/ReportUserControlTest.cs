using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class ReportUserControlTest : TestCaseWithFactory
	{
		public void TestReportUserControlWhenNoReportIsSelected()
		{
			var collection = new ReportCommandCollection(Factory, "TestModuleID");
			collection.Load();

			using (var parentModule = (ZEmbeddedModule)ZModuleFactory.Instance.Create(ModuleIDs.CashBookReports))
			{
				var reportControl = new ReportTestUserControl(collection, parentModule.SecurityCheckpoint);

				using (var testForm = new ZForm(Dummy))
				{
					testForm.Controls.Add(reportControl);
					testForm.Show();

					AssertEquals("Row Count on grid", 0, reportControl.ReportGrid.ListManager.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();
					AssertEquals("Error message", "There are no reports to print.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("LastPrintedSet", null, reportControl.LastPrintSet);
				}
			}
		}

		[RequiresSTA]
		public void TestReportUserControl()
		{
			var command1 = Factory.New<ReportCommand>();
			command1.SU_MenuName = "Test Report 99";
			command1.SU_IsSystemDefined = true;
			command1.SU_IsPublished = true;
			command1.SU_BusinessContext = nameof(BusinessContext.CBReport);

			Factory.Save();

			var collection = new ReportCommandCollection(Factory, nameof(BusinessContext.CBReport));
			collection.Load();

			using (var parentModule = (ZEmbeddedModule)ZModuleFactory.Instance.Create(ModuleIDs.CashBookReports))
			{
				var reportControl = new ReportTestUserControl(collection, parentModule.SecurityCheckpoint);

				using (ZForm testForm = new ZForm(Dummy))
				{
					testForm.Controls.Add(reportControl);
					testForm.Show();

					AssertEquals("Row Count on grid", 1, reportControl.ReportGrid.ListManager.Count);
					AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "Run"));
					AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "Schedule"));
					AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "Customize Reports"));
					AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "&Customize Columns"));

					AssertNull("LastPrintedSet", reportControl.LastPrintSet);

					reportControl.ReportGrid.ListManager.Position = 0;
					reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();

					AssertEquals("LastPrintedSet Pack Count", 1, reportControl.LastPrintSet.Count);
					AssertNotEquals("Should use different factory to run report", Factory, reportControl.LastPrintSet[0].Factory);
				}
			}
		}

		public void TestReportUserControlSecurity()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			command.SU_IsPublished = true;
			command.SU_BusinessContext = nameof(BusinessContext.CBReport);

			Factory.Save();

			var collection = new ReportCommandCollection(Factory, nameof(BusinessContext.CBReport));
			collection.Load();

			var parentCheckpoint = new SecurityCheckpoint("TestReportUserControlSecurity", (NoResString)"", null, Env.Security);
			var reportControl = new ReportTestUserControl(collection, parentCheckpoint);

			using (var dummyForm = new ZForm())
			{
				dummyForm.Controls.Add(reportControl);
				dummyForm.Show();
			}

			var customizeCheckpoint = Env.Security.FindOrCreateReportCustomizeCheckpoint(ModuleIDs.Packing, parentCheckpoint);
			AssertEquals(customizeCheckpoint, reportControl.LastCustomizeReportCheckpoint);
		}

		MenuItem FindMenuItem(MenuItem.MenuItemCollection items, string text)
		{
			return items.Cast<MenuItem>().FirstOrDefault(item => item.Text == text);
		}

		public void TestReportSecurity_ReportModuleNoSecurity()
		{
			var command1 = Factory.New<ReportCommand>();
			command1.SU_MenuName = "Test Report 99";
			command1.SU_IsSystemDefined = true;
			command1.SU_IsPublished = true;
			command1.SU_BusinessContext = Env.Security.OrderReports.Code;
			Factory.Save();

			var parent = Env.Security.OrderReports;
			parent.IsAllowed = false;
			var reportCheckpoint = Env.Security.FindOrCreateReportCheckpoint(command1.PK.ToGuid(), command1.SU_MenuNameMultilingual, ModuleIDs.OrdersReport, parent);
			reportCheckpoint.IsAllowed = false;

			var collection = new ReportCommandCollection(Factory, Env.Security.OrderReports.Code);
			collection.Load();

			var reportControl = new ReportTestUserControl(collection, Env.Security.None);

			using (ZForm testForm = new ZForm(Dummy))
			{
				testForm.Controls.Add(reportControl);
				testForm.Show();

				AssertEquals("Row Count on grid", 1, reportControl.ReportGrid.ListManager.Count);
				AssertNull("LastPrintedSet", reportControl.LastPrintSet);

				reportControl.ReportGrid.ListManager.Position = 0;
				reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();

				AssertNotNull("LastPrintedSet Pack Count should not be null as the module has no security", reportControl.LastPrintSet);
			}
		}

		public void TestReportSecurity_IndividualUserRights()
		{
			var command1 = Factory.New<ReportCommand>();
			command1.SU_MenuName = "Test Report 99";
			command1.SU_IsSystemDefined = true;
			command1.SU_IsPublished = true;
			command1.SU_BusinessContext = Env.Security.OrderReports.Code;
			Factory.Save();

			var parent = Env.Security.OrderReports;
			parent.IsAllowed = false;
			var reportCheckpoint = Env.Security.FindOrCreateReportCheckpoint(command1.PK.ToGuid(), command1.SU_MenuNameMultilingual, ModuleIDs.OrdersReport, parent);
			reportCheckpoint.IsAllowed = false;

			var collection = new ReportCommandCollection(Factory, Env.Security.OrderReports.Code);
			collection.Load();

			using (var parentModule = (ZEmbeddedModule)ZModuleFactory.Instance.Create(ModuleIDs.OrdersReport))
			{
				var reportControl = new ReportTestUserControl(collection, parentModule.SecurityCheckpoint);

				using (var testForm = new ZForm(Dummy))
				{
					testForm.Controls.Add(reportControl);
					testForm.Show();

					AssertEquals("Row Count on grid", 1, reportControl.ReportGrid.ListManager.Count);
					AssertNull("LastPrintedSet", reportControl.LastPrintSet);

					reportControl.ReportGrid.ListManager.Position = 0;
					reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();

					AssertEquals("Access Denied error should be shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Order Manager -> Reports -> Run Reports -> Test Report 99", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNull("LastPrintedSet Pack Count should be null", reportControl.LastPrintSet);

					reportCheckpoint.IsAllowed = true;
					Factory.Save();

					reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();
					AssertEquals("LastPrintedSet Pack Count", 1, reportControl.LastPrintSet.Count);
					reportControl.LastPrintSet.Clear();

					parent.IsAllowed = true;
					reportCheckpoint.IsAllowed = false;
					Factory.Save();

					reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();
					AssertEquals("LastPrintedSet Pack Count", 0, reportControl.LastPrintSet.Count);
					AssertEquals("Access Denied error should be shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Order Manager -> Reports -> Run Reports -> Test Report 99", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestReportSecurity_InheritedRights()
		{
			var command1 = Factory.New<ReportCommand>();
			command1.SU_MenuName = "Test Report 1";
			command1.SU_IsSystemDefined = true;
			command1.SU_IsPublished = true;
			command1.SU_BusinessContext = Env.Security.OrderReports.Code;

			var command2 = Factory.New<ReportCommand>();
			command2.SU_MenuName = "Test Report 2";
			command2.SU_IsSystemDefined = true;
			command2.SU_IsPublished = true;
			command2.SU_BusinessContext = Env.Security.OrderReports.Code;

			var command3 = Factory.New<ReportCommand>();
			command3.SU_MenuName = "Test Report 3";
			command3.SU_IsSystemDefined = true;
			command3.SU_IsPublished = true;
			command3.SU_BusinessContext = Env.Security.OrderReports.Code;

			var testGroup = Factory.New<GlbGroup>();
			testGroup.GG_Code = "BAM";
			testGroup.GG_IsActive = true;

			var testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = "Bob.test2";

			var testLink = Factory.New<GlbGroupLink>();
			testLink.GK_GS = testStaff.PK;
			testLink.GK_GG = testGroup.PK;

			var reportSecurityItem = Factory.New<GlbSecurity>();
			reportSecurityItem.GU_GG = testGroup.PK;
			reportSecurityItem.GU_ItemGUID = command1.PK.ToGuid();
			reportSecurityItem.GU_SecurityRight = "Report";
			reportSecurityItem.GU_SecurityItemIsAllowed = false;

			var reportSecurityItem2 = Factory.New<GlbSecurity>();
			reportSecurityItem2.GU_GG = testGroup.PK;
			reportSecurityItem2.GU_ItemGUID = command2.PK.ToGuid();
			reportSecurityItem2.GU_SecurityRight = "Report";
			reportSecurityItem2.GU_SecurityItemIsAllowed = true;

			var reportSecurityItemR = Factory.New<GlbSecurity>();
			reportSecurityItemR.GU_GG = testGroup.PK;
			reportSecurityItemR.GU_SecurityRight = Env.Security.OrderReports.Code;
			reportSecurityItemR.GU_SecurityItemIsAllowed = true;

			var reportSecurityItemRR = Factory.New<GlbSecurity>();
			reportSecurityItemRR.GU_GG = testGroup.PK;
			reportSecurityItemRR.GU_SecurityRight = Env.Security.FindOrCreateReportRunCheckpoint(ModuleIDs.Packing, Env.Security.OrderReports).Code;
			reportSecurityItemRR.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext("Bob.test2", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var collection = new ReportCommandCollection(Factory, Env.Security.OrderReports.Code);
				collection.Load();
				collection.Add(command1);
				collection.Add(command2);
				collection.Sort(ReportCommand.Schema.SU_MenuName);

				using (var parentModule = (ZEmbeddedModule)ZModuleFactory.Instance.Create(ModuleIDs.OrdersReport))
				{
					var reportControl = new ReportTestUserControl(collection, parentModule.SecurityCheckpoint);

					using (var testForm = new ZForm(Dummy))
					{
						testForm.Controls.Add(reportControl);
						testForm.Show();

						reportControl.ReportGrid.ListManager.Position = 0;
						AssertEquals("ReportGrid.ListManager.GetCurrent() == Command1", command1, reportControl.ReportGrid.ListManager.GetCurrent());
						reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();

						AssertNull("LastPrintedSet Pack Count should be null", reportControl.LastPrintSet);
						AssertEquals("Access Denied error should be shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Order Manager -> Reports -> Run Reports -> Test Report 1", UnitTestUserNotification.Instance.LastMessage.Text);

						reportControl.ReportGrid.ListManager.Position = 1;
						AssertEquals("ReportGrid.ListManager.GetCurrent() == Command2", command2, reportControl.ReportGrid.ListManager.GetCurrent());
						reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();
						AssertEquals("LastPrintedSet Pack Count", 1, reportControl.LastPrintSet.Count);

						reportControl.ReportGrid.ListManager.Position = 2;
						AssertEquals("ReportGrid.ListManager.GetCurrent() == Command3", command3, reportControl.ReportGrid.ListManager.GetCurrent());
						var oldLastPrintSet = reportControl.LastPrintSet;
						reportControl.ReportGrid.ContextMenu.MenuItems[0].PerformClick();

						AssertEquals("LastPrintedSet hasn't changed", oldLastPrintSet, reportControl.LastPrintSet);
						AssertEquals("Access Denied error should be shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Order Manager -> Reports -> Run Reports -> Test Report 3", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestCannotScheduleIfIsNotAllowed()
		{
			var reports = new ReportCommandCollection(Factory, nameof(BusinessContext.Consol));
			reports.Load();

			using (ZForm testForm = new ZForm(reports))
			using (ZEmbeddedModule parentModule = (ZEmbeddedModule)ZModuleFactory.Instance.Create(ModuleIDs.OrdersReport))
			using (ReportTestUserControl control = new ReportTestUserControl(reports, parentModule.SecurityCheckpoint))
			{
				testForm.Controls.Add(control);
				testForm.Show();

				AssertNull("Precondition: No errors should be shown yet.", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.ScheduledTaskNew.IsAllowed = false;
				control.ReportGrid.ContextMenu.MenuItems[1].PerformClick();
				AssertEquals(
					"An error should be shown.",
					Env.Security.ScheduledTaskNew.ErrorMessageForNotAllowed,
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("LastPrintSet", control.LastPrintSet);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.ScheduledTaskNew.IsAllowed = true;
				control.ReportGrid.ContextMenu.MenuItems[1].PerformClick();
				AssertNull("No errors should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				using (IZForm form = ((ZController)control.LastPrintSet.LastScheduleController).LastShownForm)
				{
					AssertEquals("The schedule form should be shown.", typeof(ScheduleTaskForm), form.GetType());
					AssertNotEquals(Factory, ((ScheduleTaskForm)form).BusinessEntity.Factory);
				}
			}
		}

		public void TestCannotConstructWithoutReport()
		{
			using (var form = new ZForm())
			using (var control = new ReportUserControl())
			{
				form.Controls.Add(control);
				form.Show();
			}

			var lastEx = ErrorReporter.LastExceptionReported as InvalidOperationException;
			AssertNotNull("Error Reporter should have received error", lastEx);
			AssertEquals("Error Reporter error should indicate incorrect instantiation of ReportUserControl object", "A ReportUserControl was created without a Report Collection", lastEx.Message);
			ErrorReporter.Clear();
		}

		#region Implementation

		DummyBusinessObject Dummy;

		protected override void SetUp()
		{
			base.SetUp();
			CustomisationMenusMaker<MenuItem>.ShouldAddDebugOnlyMenuItemsForTesting = true;
			Dummy = Factory.New<DummyBusinessObject>();
			AssertNotNull("Dummy should be created!", Dummy);

			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
			CustomisationMenusMaker<MenuItem>.ShouldAddDebugOnlyMenuItemsForTesting = false;
		}

		public class ReportTestUserControl : ReportUserControl
		{
			public ReportTestUserControl(ReportCommandCollection reports, ISecurityCheckpoint securityCheckpoint)
				: base(ModuleIDs.Packing, reports, securityCheckpoint)
			{
			}

			protected override ReportPrintSet GetNewPrintSet(ReportCommand command)
			{
				lastPrintSet = new MockReportPrintSet(command);
				return lastPrintSet;
			}

			public ReportPrintSet LastPrintSet
			{
				get { return lastPrintSet; }
			}

			ReportPrintSet lastPrintSet;

			public override ReportCustomisationMenusMaker Maker(ISecurityCheckpoint customizeReportCheckpoint)
			{
				LastCustomizeReportCheckpoint = customizeReportCheckpoint;
				return base.Maker(customizeReportCheckpoint);
			}

			public ISecurityCheckpoint LastCustomizeReportCheckpoint { get; private set; }
		}

		class MockReportPrintSet : ReportPrintSet
		{
			public MockReportPrintSet(ReportCommand command)
				: base(command)
			{
			}

			public override DeliveryInstructionDestination RunWithPartialInstructions(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				return DeliveryInstructionDestination.None;
			}
		}

		#endregion
	}
}
