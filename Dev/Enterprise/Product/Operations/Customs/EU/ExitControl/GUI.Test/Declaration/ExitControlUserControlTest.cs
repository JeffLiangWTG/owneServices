using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ExitControlUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsTabPage()
		{
			var detailsTabPage = userControl.DetailsTabPage;

			CombineAssertions(() =>
			{
				AssertEquals("DetailsTabPage is in TabControl", true, userControl.ExitControlTabControl.Contains(detailsTabPage));
				AssertEquals("Caption", "Details", detailsTabPage.CaptionResourceString.Caption);
			});
		}

		public void TestDetailsTabUserControl()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");
			var detailsTabUserControl = userControl.DetailsTabUserControl;

			CombineAssertions(() =>
			{
				AssertEquals("DetailsTabPage is in TabControl", true, userControl.DetailsTabPage.Contains(detailsTabUserControl));
				AssertEquals("Dock", DockStyle.Fill, detailsTabUserControl.Dock);
				AssertEquals("DataMember", ".", detailsTabUserControl.GetBindingMember());
			});
		}

		public void TestConsignmentsTabPage()
		{
			var consignmentsTabPage = userControl.ConsignmentsTabPage;

			CombineAssertions(() =>
			{
				AssertEquals("ConsignmentsTabPage is in TabControl", true, userControl.ExitControlTabControl.Contains(consignmentsTabPage));
				AssertEquals("Caption", "Declarations/Entries", consignmentsTabPage.CaptionResourceString.Caption);
			});
		}

		public void TestConsignmentsTabUserControl()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");
			var consignmentsTabUserControl = userControl.ConsignmentsTabUserControl;

			CombineAssertions(() =>
			{
				AssertEquals("ConsignmentsTabUserControl is in In ConsignmentsTabPage", true, userControl.ConsignmentsTabPage.Contains(consignmentsTabUserControl));
				AssertEquals("Dock", DockStyle.Fill, consignmentsTabUserControl.Dock);
				AssertEquals("DataMember", "CusExitConsignments", consignmentsTabUserControl.GetBindingMember());
			});
		}

		public void TestReportsTabPage()
		{
			var reportsTabPage = userControl.ReportsTabPage;

			CombineAssertions(() =>
			{
				AssertEquals("ReportsTabPage is in TabControl", true, userControl.ExitControlTabControl.Contains(reportsTabPage));
				AssertEquals("Caption", "Exit Reports", reportsTabPage.CaptionResourceString.Caption);
			});
		}

		public void TestReportsTabUserControl()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");
			var reportsTabUserControl = userControl.ReportsTabUserControl;

			CombineAssertions(() =>
			{
				AssertEquals("ReportsTabUserControl is in In ReportsTabPage", true, userControl.ReportsTabPage.Contains(reportsTabUserControl));
				AssertEquals("Dock", DockStyle.Fill, reportsTabUserControl.Dock);
				AssertEquals("DataMember", "CusExitReports", reportsTabUserControl.GetBindingMember());
			});
		}

		public void TestInitializeAdditionalTabs()
		{
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForAdditionalTabsTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			using (var userControl = new ExitControlUserControl())
			{
				var exitHeader = Factory.New<CusExitHeader>();
				userControl.SetDataBinding(exitHeader, "");
				var exitControlTabControl = userControl.ExitControlTabControl;

				CombineAssertions(() =>
				{
					AssertEquals("There are 2 additional tabs", 2, exitControlTabControl.AllTabPages.Count(t => t.Name.StartsWith("AdditionalDecalrationTabPage")));

					var additionalTab1 = exitControlTabControl.GetTabPage("AdditionalDecalrationTabPage1");
					AssertEquals("Tab1 Caption", "Additional Tab Page For Test #1", additionalTab1.CaptionResourceString.Caption);

					var additionalTab1UserControl = additionalTab1.FindSingle<AdditionalTabPageForTest1UserControl>("AdditionalDecalrationTabUserControl1");
					AssertEquals("Tab1UserControl BindingMember", "Property1", additionalTab1UserControl.GetBindingMember());
					AssertEquals("Tab1UserControl Dock style", DockStyle.Fill, additionalTab1UserControl.Dock);

					var additionalTab2 = exitControlTabControl.GetTabPage("AdditionalDecalrationTabPage2");
					AssertEquals("Tab2 Caption", "Additional Tab Page For Test #2", additionalTab2.CaptionResourceString.Caption);

					var additionalTab2UserControl = additionalTab2.FindSingle<AdditionalTabPageForTest2UserControl>("AdditionalDecalrationTabUserControl2");
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
			using (var userControl = new ExitControlUserControl())
			{
				CombineAssertions(() =>
				{
					var exitControlTabControl = userControl.ExitControlTabControl;
					AssertNotNull("Before binding, has DetailsTabUserControl", exitControlTabControl.GetTabPage(nameof(userControl.DetailsTabPage)));
					AssertNotNull("Before binding, has ReportsTabPage", exitControlTabControl.GetTabPage(nameof(userControl.ReportsTabPage)));

					var exitHeader = Factory.New<CusExitHeader>();
					userControl.SetDataBinding(exitHeader, "");

					AssertNotNull("After binding, DetailsTabUserControl not removed", exitControlTabControl.GetTabPage(nameof(userControl.DetailsTabPage)));
					AssertNull("After binding, ReportsTabPage removed", exitControlTabControl.GetTabPage(nameof(userControl.ReportsTabPage)));
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ExitControlUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		ExitControlUserControl userControl;
	}

	sealed class ExitControlLayoutProviderForAdditionalTabsTest : ExitControlLayoutProvider, IExitControlLayoutProvider
	{
		IEnumerable<ITabPage> IExitControlLayoutProvider.AdditionalDeclarationTabPages
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
		IEnumerable<ZString> IExitControlLayoutProvider.RemovableDeclarationTabPageNames
		{
			get
			{
				yield return nameof(ExitControlUserControl.ReportsTabPage);
			}
		}
	}
}
