using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportsTabUserControlTest : TestCaseWithFactory
	{
		public void TestMessagesTabAndUserControl()
		{
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			{
				var cusExitHeader = Factory.New<CusExitHeader>();
				userControl.SetDataBinding(cusExitHeader, "");

				var messageTabPage = userControl.MessagesTabPage;
				CombineAssertions(() =>
				{
					AssertSame("MessagesTabPage is last in MessagesTabUserControl", messageTabPage, userControl.ReportTabControl.Controls[userControl.ReportTabControl.Controls.Count - 1]);
					AssertEquals("Caption", "Messages", messageTabPage.CaptionResourceString.Caption);

					var reportItemsUserControl = messageTabPage.FindSingle<MessagesTabUserControl>(nameof(MessagesTabUserControl));
					AssertEquals("Dock", DockStyle.Fill, reportItemsUserControl.Dock);
					AssertEquals("BindingMember", "Messages", reportItemsUserControl.GetBindingMember());
				});
			}
		}

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitReportCollection<CusExitReport>), userControl.BindingSource.DataSourceType);
		}

		public void TestReportsSplitContainer()
		{
			CombineAssertions(() =>
			{
				var reportsSplitContainer = userControl.ReportsSplitContainer;
				AssertEquals("Orientation", Orientation.Horizontal, reportsSplitContainer.Orientation);
				AssertEquals("SplitterDistance", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(205), reportsSplitContainer.SplitterDistance);
				AssertEquals("Panel1MinSize", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(205), reportsSplitContainer.Panel1MinSize);
			});
		}

		public void TestReportsGridUserControl()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");

			CombineAssertions(() =>
			{
				var gridUserControl = userControl.ReportsSplitContainer.Panel1.FindSingle<ReportsGridUserControl>(nameof(ReportsGridUserControl));
				AssertEquals("Dock", DockStyle.Fill, gridUserControl.Dock);
				AssertEquals("BindingMember", ".", gridUserControl.GetBindingMember());
			});
		}

		public void TestReportTabControl()
		{
			CombineAssertions(() =>
			{
				var reportTabControl = userControl.ReportTabControl;
				AssertEquals("ReportTabControl is within ReportsSplitContainer.Panel2", true, userControl.ReportsSplitContainer.Panel2.Contains(reportTabControl));
				AssertEquals("Dock", DockStyle.Fill, reportTabControl.Dock);
			});
		}

		public void TestReportItemsTabPage_DefaultIsBlank()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");

			var reportItemsUserControl = userControl.ReportItemsTabPage.FindSingleOrDefault<ReportItemsUserControl>(nameof(ReportItemsUserControl));
			AssertNull(reportItemsUserControl);
		}

		public void TestReportItemsTabPage()
		{
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportItemsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			{
				var cusExitHeader = Factory.New<CusExitHeader>();
				userControl.SetDataBinding(cusExitHeader, "");

				var reportItemsTabPage = userControl.ReportItemsTabPage;
				CombineAssertions(() =>
				{
					AssertEquals("ReportItemsTabPage is within ReportTabControl", true, userControl.ReportTabControl.Contains(reportItemsTabPage));
					AssertEquals("Caption", "Items", reportItemsTabPage.CaptionResourceString.Caption);

					var reportItemsUserControl = reportItemsTabPage.FindSingle<ReportItemsUserControl>(nameof(ReportItemsUserControl));
					AssertEquals("Dock", DockStyle.Fill, reportItemsUserControl.Dock);
					AssertEquals("BindingMember", ".", reportItemsUserControl.GetBindingMember());
				});
			}
		}

		public void TestInitializeAdditionalTabs()
		{
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForAdditionalTabsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			{
				var cusExitHeader = Factory.New<CusExitHeader>();
				userControl.SetDataBinding(cusExitHeader, "");
				var reportTabControl = userControl.ReportTabControl;

				CombineAssertions(() =>
				{
					AssertEquals("There are 2 additional tabs", 2, reportTabControl.AllTabPages.Count(t => t.Name.StartsWith("AdditionalReportTabPage")));

					var additionalTab1 = reportTabControl.GetTabPage("AdditionalReportTabPage1");
					AssertEquals("Tab1 Caption", "Additional Tab Page For Test #1", additionalTab1.CaptionResourceString.Caption);

					var additionalTab1UserControl = additionalTab1.FindSingle<AdditionalTabPageForTest1UserControl>("AdditionalReportTabUserControl1");
					AssertEquals("Tab1UserControl BindingMember", "Property1", additionalTab1UserControl.GetBindingMember());
					AssertEquals("Tab1UserControl Dock style", DockStyle.Fill, additionalTab1UserControl.Dock);

					var additionalTab2 = reportTabControl.GetTabPage("AdditionalReportTabPage2");
					AssertEquals("Tab2 Caption", "Additional Tab Page For Test #2", additionalTab2.CaptionResourceString.Caption);

					var additionalTab2UserControl = additionalTab2.FindSingle<AdditionalTabPageForTest2UserControl>("AdditionalReportTabUserControl2");
					AssertEquals("Tab2UserControl BindingMember", "Property2", additionalTab2UserControl.GetBindingMember());
					AssertEquals("Tab2UserControl Dock style", DockStyle.Fill, additionalTab2UserControl.Dock);
				});
			}
		}

		public void TestInitializeRemovableTabs()
		{
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForRemovableTabsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			{
				CombineAssertions(() =>
				{
					var consignmentTabControl = userControl.ReportTabControl;
					AssertNotNull("Before binding, has ReportItemsTabPage", consignmentTabControl.GetTabPage(nameof(ReportsTabUserControl.ReportItemsTabPage)));

					var cusExitHeader = Factory.New<CusExitHeader>();
					userControl.SetDataBinding(cusExitHeader, "");

					AssertNull("After binding, ReportItemsTabPage removed", consignmentTabControl.GetTabPage(nameof(ReportsTabUserControl.ReportItemsTabPage)));
				});
			}
		}

		public void TestAddAdditionalReportsGridMenuItems()
		{
			var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ "LV", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
			{
				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				var exitHeader = Factory.New<CusExitHeader>();
				exitHeader.CXH_GC_Company = company.PK;
				exitHeader.CusExitReports.AddNew();

				using (var form = new ExitControlForm(exitHeader))
				{
					form.Show();
					var exitControlUserControl = form.ExitControlUserControl;
					var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
					exitControlTabControl.SelectedTab = exitControlUserControl.ReportsTabPage;
					var selectReportItemsMenuItem = exitControlUserControl.ReportsTabUserControl.ReportsGrid.ContextMenu.MenuItems.FindByText("&Select/Edit Report Items");
					AssertNotNull("Should have been added to the context menu of Reports grid", selectReportItemsMenuItem);
				}
			}
		}

		public void TestAddAdditionalReportsMainMenuItems()
		{
			var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ "LV", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
			{
				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				var exitHeader = Factory.New<CusExitHeader>();
				exitHeader.CXH_GC_Company = company.PK;
				exitHeader.CusExitReports.AddNew();

				using (var form = new ExitControlForm(exitHeader))
				{
					form.Show();
					var exitControlUserControl = form.ExitControlUserControl;
					var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
					exitControlTabControl.SelectedTab = exitControlUserControl.ReportsTabPage;
					var exitControlMenu = form.Menu.MenuItems.FindByText("E&xit Control");
					var selectReportItemsMenuItem = exitControlMenu.MenuItems.FindByText("&Select/Edit Report Items");
					AssertNotNull("Should have been added to the menu of main form", selectReportItemsMenuItem);
				}
			}
		}

		public void TestAdditionalTabs_Visibility_CER_TypeValueChanged()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			var cusExitReport = cusExitHeader.CusExitReports.AddNew();
			cusExitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			var cusExitReport2 = cusExitHeader.CusExitReports.AddNew();
			cusExitReport2.CER_Type = ExitReportTypeList.Codes.Presentation;
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForAdditionalTabsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var form = new ExitControlForm(cusExitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var reportsTabPage = form.ExitControlUserControl.ReportsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = reportsTabPage;
				var reportsTabUserControl = reportsTabPage.FindSingle<ReportsTabUserControl>(nameof(ReportsTabUserControl));
				var reportTabControl = reportsTabUserControl.ReportTabControl;
				var reportsGrid = reportsTabUserControl.ReportsGrid;

				CombineAssertions(() =>
				{
					reportsGrid.ListManager.Position = 0;
					AssertNull("Tab1 TabVisible", reportTabControl.GetTabPage("AdditionalReportTabPage1"));
					AssertNotNull("Tab2 TabVisible", reportTabControl.GetTabPage("AdditionalReportTabPage2"));

					reportsGrid.ListManager.Position = 1;
					AssertNotNull("CER_Type = 'PRE', Tab1 TabVisible", reportTabControl.GetTabPage("AdditionalReportTabPage1"));
					AssertNotNull("CER_Type = 'PRE', Tab2 TabVisible", reportTabControl.GetTabPage("AdditionalReportTabPage2"));
				});
			}
		}

		public void TestReportItemsUserControl_OnExitReportChanged()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			var cusExitReport = cusExitHeader.CusExitReports.AddNew();
			cusExitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			var cusExitReport2 = cusExitHeader.CusExitReports.AddNew();
			cusExitReport2.CER_Type = ExitReportTypeList.Codes.Presentation;

			var layoutProviderForTest = new ExitControlLayoutProviderForReportItemsUserControlOnExitReportChangedTest();
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(layoutProviderForTest) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var form = new ExitControlForm(cusExitHeader))
			{
				form.Show();

				var exitControlUserControl = form.ExitControlUserControl;
				var reportsTabPage = form.ExitControlUserControl.ReportsTabPage;
				exitControlUserControl.ExitControlTabControl.SelectedTab = reportsTabPage;
				var reportsTabUserControl = reportsTabPage.FindSingle<ReportsTabUserControl>(nameof(ReportsTabUserControl));
				var reportTabControl = reportsTabUserControl.ReportTabControl;
				var reportsGrid = reportsTabUserControl.ReportsGrid;

				CombineAssertions(() =>
				{
					reportsGrid.ListManager.Position = 0;
					var cusExitReportPassedThrough = layoutProviderForTest.ReportItemsUserControlForTest.CusExitReport;
					AssertSame(cusExitReport, cusExitReportPassedThrough);

					reportsGrid.ListManager.Position = 1;
					cusExitReportPassedThrough = layoutProviderForTest.ReportItemsUserControlForTest.CusExitReport;
					AssertSame(cusExitReport2, cusExitReportPassedThrough);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ReportsTabUserControl();
		}
		ReportsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		sealed class ExitControlLayoutProviderForAdditionalTabsTest : ExitControlLayoutProvider, IExitControlLayoutProvider
		{
			IEnumerable<ITabPageWithVisibility<CusExitReport>> IExitControlLayoutProvider.AdditionalReportTabPages
			{
				get
				{
					yield return new AdditionalTabPageWithVisibilityForTest1();
					yield return new AdditionalTabPageWithVisibilityForTest2();
				}
			}
		}

		sealed class ExitControlLayoutProviderForRemovableTabsTest : ExitControlLayoutProvider, IExitControlLayoutProvider
		{
			IEnumerable<ZString> IExitControlLayoutProvider.RemovableReportTabPageNames
			{
				get
				{
					yield return nameof(ReportsTabUserControl.ReportItemsTabPage);
				}
			}
		}

		sealed class AdditionalTabPageWithVisibilityForTest1 : AdditionalTabPageForTest1, ITabPageWithVisibility<CusExitReport>
		{
			public Func<CusExitReport, bool> IsVisible => (x) => x.CER_Type == ExitReportTypeList.Codes.Presentation;
		}

		sealed class AdditionalTabPageWithVisibilityForTest2 : AdditionalTabPageForTest2, ITabPageWithVisibility<CusExitReport>
		{
			public Func<CusExitReport, bool> IsVisible => null;
		}

		sealed class ExitControlLayoutProviderForReportItemsUserControlOnExitReportChangedTest : ExitControlLayoutProvider, IExitControlLayoutProvider
		{
			IReportItemsUserControl IExitControlLayoutProvider.CreateReportItemsUserControl() => ReportItemsUserControlForTest;

			public ReportItemsUserControlForTest ReportItemsUserControlForTest = new ReportItemsUserControlForTest();
		}

		sealed class ReportItemsUserControlForTest : ZUserControl, IReportItemsUserControl
		{
			public CusExitReport CusExitReport;

			public void OnExitReportChanged(CusExitReport cusExitReport)
			{
				CusExitReport = cusExitReport;
			}
		}
	}
}
