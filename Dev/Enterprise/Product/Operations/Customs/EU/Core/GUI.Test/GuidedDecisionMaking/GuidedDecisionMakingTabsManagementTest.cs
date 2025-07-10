using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
#if WINZOR
using System.Drawing;
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class GuidedDecisionMakingTabsManagementTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNavigateToPrevious()
		{
			using (var form = new ZForm())
			using (var gdmTabsManagement = PrepareTest(form))
			{
				form.Show();

				gdmTabsManagement.tabControl.SelectedIndex = gdmTabsManagement.tabs[1].Tab.TabIndex;

				gdmTabsManagement.NavigateToPrevious();
				Assert(gdmTabsManagement.tabControl.SelectedIndex == gdmTabsManagement.tabs[0].Tab.TabIndex);
			}
		}

		[RequiresSTA]
		public void TestNavigateToNext()
		{
			using (var form = new ZForm())
			using (var gdmTabsManagement = PrepareTest(form))
			{
				form.Show();

				gdmTabsManagement.tabControl.SelectTab(0);
				gdmTabsManagement.tabControl.SelectedIndex = gdmTabsManagement.tabs[0].Tab.TabIndex;

				gdmTabsManagement.NavigateToNext();
				Assert(gdmTabsManagement.tabControl.SelectedIndex == gdmTabsManagement.tabs[1].Tab.TabIndex);
			}
		}

		[RequiresSTA]
		public void TestInvalidateTabControl()
		{
			using (var form = new ZForm())
			using (var gdmTabsManagement = PrepareTest(form))
			{
				form.Show();

				gdmTabsManagement.tabControl.SelectTab(0);
				gdmTabsManagement.tabControl.SelectedIndex = gdmTabsManagement.tabs[0].Tab.TabIndex;

				gdmTabsManagement.InvalidateTabControl();
				AssertEquals(GuidedDecisionMakingTabStatus.Current, gdmTabsManagement.tabs[0].Status);

				gdmTabsManagement.NavigateToNext();
				AssertEquals(GuidedDecisionMakingTabStatus.Completed, gdmTabsManagement.tabs[0].Status);

				gdmTabsManagement.NavigateToPrevious();
				AssertEquals(GuidedDecisionMakingTabStatus.Incomplete, gdmTabsManagement.tabs[1].Status);
			}
		}

		[RequiresSTA]
		public void TestGetUnsatisfiedGroupDescription()
		{
			using (var form = new ZForm())
			using (var gdmTabsManagement = PrepareTest(form))
			{
				gdmTabsManagement.tabs[0].GetUnsatisfiedGroupDescriptions = () => "Test unsatisfied description";

				form.Show();

				gdmTabsManagement.tabControl.SelectTab(0);
				gdmTabsManagement.tabControl.SelectedIndex = gdmTabsManagement.tabs[0].Tab.TabIndex;

				gdmTabsManagement.NavigateToNext();

				Assert(gdmTabsManagement.tabControl.SelectedIndex == gdmTabsManagement.tabs[1].Tab.TabIndex);
				AssertEquals("First tab should have status CompletedWithWarning", GuidedDecisionMakingTabStatus.CompletedWithWarning, gdmTabsManagement.tabs[0].Status);
			}
		}

		[RequiresSTA]
		public void TestAddTab()
		{
			using (var form = new ZForm())
			{
				var guidedDecisionMakingTabControl = new ZTabControl();
				form.Controls.Add(guidedDecisionMakingTabControl);
				var gdmTabsManagement = new GuidedDecisionMakingTabsManagementForTest(guidedDecisionMakingTabControl);
				gdmTabsManagement.Removetabs();
				var tabpage = new ZTabPage();
				guidedDecisionMakingTabControl.TabPages.Add(tabpage);

				form.Show();
				gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
				{
					Tab = tabpage
				});

				AssertEquals(1, gdmTabsManagement.tabs.Count);
			}
		}

		static GuidedDecisionMakingTabsManagementForTest PrepareTest(ZForm form)
		{
			var guidedDecisionMakingTabControl = new ZTabControl();
			form.Controls.Add(guidedDecisionMakingTabControl);
			var gdmTabsManagement = new GuidedDecisionMakingTabsManagementForTest(guidedDecisionMakingTabControl);
			gdmTabsManagement.Removetabs();
			var tabpage = new ZTabPage();
			var tabpage1 = new ZTabPage();
			guidedDecisionMakingTabControl.TabPages.Add(tabpage);
			guidedDecisionMakingTabControl.TabPages.Add(tabpage1);

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Tab = tabpage,
				IsApplicable = () => true,
				CanNavigateToNext = () => true
			});

			gdmTabsManagement.AddTab(new GuidedDecisionMakingTab
			{
				Tab = tabpage1,
				IsApplicable = () => true,
				CanNavigateToNext = () => true
			});
			return gdmTabsManagement;
		}

#if WINZOR
		public void TestTabControlAttributesAreSetWhenInvalidateTabControl()
		{
			using (var form = new ZForm())
			using (var gdmTabsManagement = PrepareTest(form))
			{
				form.Show();

				gdmTabsManagement.tabControl.SelectTab(0);
				gdmTabsManagement.tabControl.SelectedIndex = gdmTabsManagement.tabs[0].Tab.TabIndex;

				var tab1 = gdmTabsManagement.tabs[0];
				var tab2 = gdmTabsManagement.tabs[1];
				tab1.Caption = "1. Tab";
				tab2.Caption = "2. Tab";
				gdmTabsManagement.NavigateToNext();

				CombineAssertions(() =>
				{
					AssertEquals("Text for Tab1 ", "  1. Tab\r\n(Completed)", tab1.Tab.Text);
					AssertEquals("Text for Tab2", "2. Tab", tab2.Tab.Text);
					AssertEquals("Caption Background for Completed Tab1", Color.FromArgb(198, 236, 198), tab1.CaptionBackground);
					AssertEquals("Caption Background for Current Tab2", Color.White, tab2.CaptionBackground);
					AssertGreaterThan("Icon Index for Completed Tab1", tab1.IconIndex, -1);
					AssertEquals("No Icon Index for Current Tab2", -1, tab2.IconIndex);
				});
			}
		}
#endif

		public class GuidedDecisionMakingTabsManagementForTest : GuidedDecisionMakingTabsManagement, IDisposable
		{
			public GuidedDecisionMakingTabsManagementForTest(ZTabControl tabControl) : base(tabControl)
			{
			}
			public new ZTabControl tabControl => base.tabControl;
			public new List<GuidedDecisionMakingTab> tabs => base.tabs;

			public void Dispose()
			{
				tabControl.Dispose();
			}

			public void Removetabs()
			{
				tabs.RemoveAll(x => true);
			}
		}
	}
}
