using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class ZTabControlExtensionsTest : TestCaseWithFactory
	{
		public void TestAddAdditionalTabs()
		{
			using (var tabControl = new ZTabControl())
			{
				var initialTabCount = 2;
				var additionalTabPage1 = new AdditionalTabPageForTest1();
				var additionalTabPage2 = new AdditionalTabPageForTest2();

				InitializeTabControl(tabControl, initialTabCount);

				tabControl.AddAdditionalTabs(new ZBindingSource(), new ITabPage[] { additionalTabPage1, additionalTabPage2 });

				CombineAssertions(() =>
				{
					AssertEquals("Total number of Tabs", initialTabCount + 2, tabControl.TabCount);

					AssertTabPageAndControl(tabControl, additionalTabPage1, typeof(AdditionalTabPageForTest1UserControl), initialTabCount + 0);
					AssertTabPageAndControl(tabControl, additionalTabPage2, typeof(AdditionalTabPageForTest2UserControl), initialTabCount + 1);
				});
			}
		}

		public void TestReorderTabs()
		{
			var tabPageNames = new[] { "ExistingTabPage3", "ExistingTabPage1", "ExistingTabPage0" };

			var initialTabCount = 4;
			using (var tabControl = new ZTabControl())
			{
				InitializeTabControl(tabControl, initialTabCount);

				var defaultTabPageNames = tabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray();
				var existingTabPage2 = tabControl.TabPages[2];

					tabControl.ReorderTabs(_ => null);
				AssertTabPagesInOrder(defaultTabPageNames);

				tabControl.ReorderTabs(_ => Array.Empty<string>());
				AssertTabPagesInOrder(defaultTabPageNames);

				tabControl.ReorderTabs(_ => _);
				AssertTabPagesInOrder(defaultTabPageNames);

				tabControl.ReorderTabs(_ => tabPageNames);
				AssertTabPagesInOrder(tabPageNames);

				Assert("Removed Tab should be disposed", existingTabPage2.IsDisposed);

				void AssertTabPagesInOrder(string[] tabPageNamesInOrder)
				{
					CombineAssertions(() =>
					{
						AssertEquals("Total Tabs Count", tabPageNamesInOrder.Length, tabControl.TabCount);
						for (var i = 0; i < tabPageNamesInOrder.Length; i++)
						{
							AssertEquals($"Tab page at index {i}", tabPageNamesInOrder[i], tabControl.TabPages[i].Name);
						}
					});
				}
			}
		}

		void AssertTabPageAndControl(ZTabControl tabControl, ITabPage nctsTabPageToAdd, Type userControlType, int expectedIndex)
		{
			var additionalTab = tabControl.GetTabPage(nctsTabPageToAdd.GetType().Name);
			AssertEquals("Tab index", expectedIndex, tabControl.TabPages.IndexOf(additionalTab));
			AssertEquals("Tab Caption", nctsTabPageToAdd.Caption.Caption, additionalTab.CaptionResourceString.Caption);
			AssertEquals("Tab DockStyle", DockStyle.Fill, additionalTab.Dock);

			var additionalTabUserControl = additionalTab.Controls.Cast<Control>().FirstOrDefault(x => x.GetType() == userControlType);
			AssertEquals("TabUserControl BindingMember", nctsTabPageToAdd.UserControlBindingMember, additionalTabUserControl.GetBindingMember());
			AssertEquals("TabUserControl DockStyle", DockStyle.Fill, additionalTabUserControl.Dock);
		}

		void InitializeTabControl(ZTabControl tabControl, int numberOfTabs)
		{
			for (int i = 0; i < numberOfTabs; i++)
			{
				var tabPage = new ZTabPage();
				tabPage.Name = $"ExistingTabPage{i}";
				tabControl.Controls.Add(tabPage);
			}
		}
	}

	sealed class AdditionalTabPageForTest1 : ITabPage
	{
		public ResourceStringData Caption => NoResourceStringData.GetData("Additional Tab Page For Test #1");

		public ZString UserControlBindingMember => "Property1";

		public ZUserControl CreateUserControl() => new AdditionalTabPageForTest1UserControl();
	}

	sealed class AdditionalTabPageForTest2 : ITabPage
	{
		public ResourceStringData Caption => NoResourceStringData.GetData("Additional Tab Page For Test #2");

		public ZString UserControlBindingMember => "Property2";

		public ZUserControl CreateUserControl() => new AdditionalTabPageForTest2UserControl();
	}

	sealed class AdditionalTabPageForTest3 : ITabPage
	{
		public ResourceStringData Caption => NoResourceStringData.GetData("Additional Tab Page For Test #3");

		public ZString UserControlBindingMember => "Property3";

		public ZUserControl CreateUserControl() => new AdditionalTabPageForTest3UserControl();
	}

	sealed class AdditionalTabPageForTest1UserControl : ZUserControl
	{
	}

	sealed class AdditionalTabPageForTest2UserControl : ZUserControl
	{
	}

	sealed class AdditionalTabPageForTest3UserControl : ZUserControl
	{
	}
}
