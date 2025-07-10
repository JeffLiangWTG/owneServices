using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentCollection<CusExitConsignment>), userControl.BindingSource.DataSourceType);
		}

		public void TestConsignmentsSplitContainer()
		{
			CombineAssertions(() =>
			{
				var consignmentsSplitContainer = userControl.ConsignmentsSplitContainer;
				AssertEquals("Orientation", Orientation.Horizontal, consignmentsSplitContainer.Orientation);
				AssertEquals("SplitterDistance", ControlDpiScalingHelper.ScaleToCurrentDpiY(260), consignmentsSplitContainer.SplitterDistance);
			});
		}

		public void TestConsignmentsGridUserControl()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");

			CombineAssertions(() =>
			{
				var consignmentsGridUserControl = userControl.ConsignmentsSplitContainer.Panel1.FindSingle<ConsignmentsGridUserControl>("ConsignmentsGridUserControl");
				AssertEquals("Dock", DockStyle.Fill, consignmentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", consignmentsGridUserControl.GetBindingMember());
			});
		}

		public void TestAdditionalConsignmentsGridMenuItems_ExitHeaderCountryHasProvider()
		{
			AssertAdditionalConsignmentsGridMenuItems(Core.Constants.CountryCodes.Spain, true);
		}

		public void TestAdditionalConsignmentsGridMenuItems_ExitHeaderCountryHasNoProvider()
		{
			AssertAdditionalConsignmentsGridMenuItems(Core.Constants.CountryCodes.France, false);
		}

		public void AssertAdditionalConsignmentsGridMenuItems(string exitHeaderCompany, bool menuItemPresent)
		{
			var exitControlMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ "ES", new TestObjectHandle(new ExitControlMenuProviderForTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlMenuProviders", exitControlMenuProviders))
			{
				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = exitHeaderCompany;
				var exitHeader = Factory.New<CusExitHeader>();
				exitHeader.CXH_GC_Company = company.PK;
				exitHeader.CusExitConsignments.AddNew();
				userControl.SetDataBinding(exitHeader, "");
				var consignmentsGridUserControl = userControl.ConsignmentsSplitContainer.Panel1.FindSingle<ConsignmentsGridUserControl>("ConsignmentsGridUserControl");
				consignmentsGridUserControl.SetDataBinding(null, "");
				var grid = consignmentsGridUserControl.ConsignmentsGrid;

				AssertEquals(menuItemPresent, grid.ContextMenu.MenuItems.Cast<MenuItem>().Any(x => x.Name == "CreateExitReportMenuItem"));
			}
		}

		public void TestAddAdditionalConsignmentsMainMenuItems()
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
				exitHeader.CusExitConsignments.AddNew();

				using (var form = new ExitControlForm(exitHeader))
				{
					form.Show();
					var exitControlUserControl = form.ExitControlUserControl;
					var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
					exitControlTabControl.SelectedTab = exitControlUserControl.ConsignmentsTabPage;
					var exitControlMenu = form.Menu.MenuItems.FindByText("E&xit Control");
					var createExitReportMenuItem = exitControlMenu.MenuItems.FindByText("&Create Exit Report");
					AssertNotNull("Should have been added to the menu of main form", createExitReportMenuItem);
				}
			}
		}

		public void TestConsignmentTabControl()
		{
			CombineAssertions(() =>
			{
				var consignmentTabControl = userControl.ConsignmentTabControl;
				AssertEquals("ConsignmentTabControl is within ConsignmentsSplitContainer.Panel2", true, userControl.ConsignmentsSplitContainer.Panel2.Contains(consignmentTabControl));
				AssertEquals("Dock", DockStyle.Fill, consignmentTabControl.Dock);
			});
		}

		public void TestConsignmentItemsTabPage()
		{
			var consignmentItemsTabPage = userControl.ConsignmentItemsTabPage;
			var consignmentItemsTabUserControl = userControl.ConsignmentItemsTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("ConsignmentItemsTabPage is within ConsignmentTabControl", true, userControl.ConsignmentTabControl.Contains(consignmentItemsTabPage));
				AssertEquals("Caption", "Items", consignmentItemsTabPage.CaptionResourceString.Caption);

				AssertEquals("ConsignmentItemsTabUserControl is within ConsignmentItemsTabPage", true, consignmentItemsTabPage.Controls.Contains(consignmentItemsTabUserControl));
				AssertEquals("ConsignmentItemsTabUserControl.Dock", DockStyle.Fill, consignmentItemsTabUserControl.Dock);
				AssertEquals("ConsignmentItemsTabUserControl.BindingMember", nameof(CusExitConsignment.CusExitConsignmentItems), consignmentItemsTabUserControl.GetBindingMember());
			});
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
				var consignmentTabControl = userControl.ConsignmentTabControl;

				CombineAssertions(() =>
				{
					AssertEquals("There are 2 additional tabs", 2, consignmentTabControl.AllTabPages.Count(t => t.Name.StartsWith("AdditionalConsignmentTabPage")));

					var additionalTab1 = consignmentTabControl.GetTabPage("AdditionalConsignmentTabPage1");
					AssertEquals("Tab1 Caption", "Additional Tab Page For Test #1", additionalTab1.CaptionResourceString.Caption);

					var additionalTab1UserControl = additionalTab1.FindSingle<AdditionalTabPageForTest1UserControl>("AdditionalConsignmentTabUserControl1");
					AssertEquals("Tab1UserControl BindingMember", "Property1", additionalTab1UserControl.GetBindingMember());
					AssertEquals("Tab1UserControl Dock style", DockStyle.Fill, additionalTab1UserControl.Dock);

					var additionalTab2 = consignmentTabControl.GetTabPage("AdditionalConsignmentTabPage2");
					AssertEquals("Tab2 Caption", "Additional Tab Page For Test #2", additionalTab2.CaptionResourceString.Caption);

					var additionalTab2UserControl = additionalTab2.FindSingle<AdditionalTabPageForTest2UserControl>("AdditionalConsignmentTabUserControl2");
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
					var consignmentTabControl = userControl.ConsignmentTabControl;
					AssertNotNull("Before binding, has ConsignmentItemsTabPage", consignmentTabControl.GetTabPage(nameof(ConsignmentsTabUserControl.ConsignmentItemsTabPage)));

					var cusExitHeader = Factory.New<CusExitHeader>();
					userControl.SetDataBinding(cusExitHeader, "");

					AssertNull("After binding, ConsignmentItemsTabPage removed", consignmentTabControl.GetTabPage(nameof(ConsignmentsTabUserControl.ConsignmentItemsTabPage)));
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentsTabUserControl();
		}
		ConsignmentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		sealed class ExitControlLayoutProviderForAdditionalTabsTest : ExitControlLayoutProvider, IExitControlLayoutProvider
		{
			IEnumerable<ITabPage> IExitControlLayoutProvider.AdditionalConsignmentTabPages
			{
				get
				{
					yield return new AdditionalTabPageForTest1();
					yield return new AdditionalTabPageForTest2();
				}
			}
		}

		sealed class ExitControlLayoutProviderForRemovableTabsTest : ExitControlLayoutProvider, IExitControlLayoutProvider
		{
			IEnumerable<ZString> IExitControlLayoutProvider.RemovableConsignmentTabPageNames
			{
				get
				{
					yield return nameof(ConsignmentsTabUserControl.ConsignmentItemsTabPage);
				}
			}
		}
	}
}
