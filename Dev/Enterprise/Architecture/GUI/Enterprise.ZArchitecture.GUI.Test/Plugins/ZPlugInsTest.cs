using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	internal class ZPlugInsTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestDisposeDoesNotCreateThePlugInIfItDoesNotAlreadyExist()
		{
			using (DummyController1.GetPlugInThrowsException())
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				// plug-ins are lazy created, test that dispose doesn't create the plugins by making the ctor throw an exception
				plugIns.Add(DummyControllerIDs.Dummy1);
			}
		}

		public void TestAddPlugInAtTabPageIndexSetsValueInPlugIn()
		{
			using (var plugIns1 = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns1.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy1, 10);
				AssertEquals("RequestedTabPageIndex", 10, plugIns1.Instances[0].RequestedTabPageIndex);
			}

			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy1, () => 10);
				AssertEquals("RequestedTabPageIndex", 10, plugIns.Instances[0].RequestedTabPageIndex);
			}
		}

		public void TestSetMenusVisible()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.Add(DummyControllerIDs.Dummy1);
				plugIns.Add(DummyControllerIDs.Dummy2);
				plugIns.Add(DummyControllerIDs.Dummy3);

				Assert("Visible", plugIns.Instances[0].TopLevelMenu.Visible);
				Assert("Visible", plugIns.Instances[1].TopLevelMenu.Visible);
				Assert("Visible", plugIns.Instances[2].TopLevelMenusInternal[0].Visible);
				Assert("Visible", plugIns.Instances[2].TopLevelMenusInternal[1].Visible);

				plugIns.SetMenusVisible(false);
				Assert("Not Visible", !plugIns.Instances[0].TopLevelMenu.Visible);
				Assert("Not Visible", !plugIns.Instances[1].TopLevelMenu.Visible);
				Assert("Not Visible", !plugIns.Instances[2].TopLevelMenusInternal[0].Visible);
				Assert("Not Visible", !plugIns.Instances[2].TopLevelMenusInternal[1].Visible);

				plugIns.SetMenusVisible(true);
				Assert("Visible", plugIns.Instances[0].TopLevelMenu.Visible);
				Assert("Visible", plugIns.Instances[1].TopLevelMenu.Visible);
				Assert("Visible", plugIns.Instances[2].TopLevelMenusInternal[0].Visible);
				Assert("Visible", plugIns.Instances[2].TopLevelMenusInternal[1].Visible);
			}
		}

		public void TestCurrentDependentGetsGrid()
		{
			using (var form = new CurrentDepForm(Dummy, 2))
			{
				form.Show();
				var plugIn = form.GetPlugIn();
				AssertEquals(plugIn.CurrentGrid, form.Grid);
				AssertEquals(plugIn.RequestedTabPageIndex, 2);
			}
		}

		class CurrentDepForm : ZTestForm
		{
			public CurrentDepForm(BusinessObject bO, int requestedTabIndex = -1)
				: base(bO)
			{
				PlugIns.AddCurrentDependentPlugIn(DummyControllerIDs.Dummy3, Grid, requestedTabIndex);
			}

			public DummyPlugIn3 GetPlugIn()
			{
				return (DummyPlugIn3)PlugIns.Instances[0];
			}
		}

		public void TestPlugInCreation()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.Add(DummyControllerIDs.Dummy1);
				AssertEquals("Number of Plugins Instances", 1, plugIns.Instances.Length);

				plugIns.Add(DummyControllerIDs.Dummy2);
				AssertEquals("Number of Plugins Instances", 2, plugIns.Instances.Length);

				plugIns.Add(DummyControllerIDs.Dummy3);
				AssertEquals("Number of Plugins Instances", 3, plugIns.Instances.Length);

				AssertEquals("Type of PlugIn", typeof(DummyPlugIn1), plugIns.Instances[0].GetType());
				AssertEquals("Type of PlugIn", typeof(DummyPlugIn2), plugIns.Instances[1].GetType());
				AssertEquals("Type of PlugIn", typeof(DummyPlugIn3), plugIns.Instances[2].GetType());
			}
		}

		[ExpectException(typeof(ZException))]
		public void TestDuplicateControllerIDs()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.Add(DummyControllerIDs.Dummy1);
				plugIns.Add(DummyControllerIDs.Dummy1);

				Assert("Exception should have been thrown. Should not be here!", false);
			}
		}

		public void TestAddingAfterInstancesQueriedAndOrder()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.ParentFormDisplayMode = ODisplayMode.Edit;

				plugIns.Add(DummyControllerIDs.Dummy1);
				Assert("Editable", ((DummyPlugIn1)plugIns.Instances[0]).IsFormEditable);

				plugIns.ParentFormDisplayMode = ODisplayMode.Delete;
				Assert("Not editable", !((DummyPlugIn1)plugIns.Instances[0]).IsFormEditable);

				plugIns.Add(DummyControllerIDs.Dummy2);
				Assert("Not editable", !((DummyPlugIn2)plugIns.Instances[1]).IsFormEditable);
			}
		}

		public void TestDisplayMode()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.Add(DummyControllerIDs.Dummy1);

				AssertEquals("Number of Plugins Instances", 1, plugIns.Instances.Length);
				AssertEquals("Type of PlugIn", typeof(DummyPlugIn1), plugIns.Instances[0].GetType());

				plugIns.Add(DummyControllerIDs.Dummy2);

				AssertEquals("Number of Plugins Instances After Querying Instances", 2, plugIns.Instances.Length);
				AssertEquals("Type of PlugIn", typeof(DummyPlugIn1), plugIns.Instances[0].GetType());
				AssertEquals("Type of PlugIn", typeof(DummyPlugIn2), plugIns.Instances[1].GetType());
			}
		}

		public void TestSecurity()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				try
				{
					DummyCheckPoint.SecurityAllowed = false;
					plugIns.Add(DummyControllerIDs.Dummy1);
					plugIns.Add(DummyControllerIDs.Dummy2);
					plugIns.Add(DummyControllerIDs.Dummy3);

					AssertEquals("Number of Plugins Instances", 3, plugIns.Instances.Length);
					AssertEquals("Type of PlugIn", typeof(DummyPlugIn1), plugIns.Instances[0].GetType());
					AssertEquals("Type of PlugIn", typeof(DummyPlugIn2), plugIns.Instances[1].GetType());
					AssertEquals("Type of PlugIn", typeof(DummyPlugIn3), plugIns.Instances[2].GetType());
				}
				finally
				{
					DummyCheckPoint.SecurityAllowed = true;
				}
			}
		}

		public void TestSecurityOverride()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				try
				{
					DummyCheckPoint.SecurityAllowed = false;

					plugIns.Add(DummyControllerIDs.Dummy1, DummyCheckPoint.Instance);
					plugIns.Add(DummyControllerIDs.Dummy3, null);
					plugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy2, DummyCheckPoint.Instance, 2);

					AssertEquals("Number of Plugins Instances", 3, plugIns.Instances.Length);
					AssertEquals("Type of PlugIn", typeof(DummyPlugIn1), plugIns.Instances[0].GetType());
					AssertEquals("Check Point", DummyCheckPoint.Instance, plugIns.Instances[0].SecurityCheckpoint);
					AssertEquals("Type of PlugIn", -1, plugIns.Instances[0].RequestedTabPageIndex);

					AssertEquals("Type of PlugIn", typeof(DummyPlugIn3), plugIns.Instances[1].GetType());
					AssertEquals("Check Point", EnvProxy.Instance.Security.None, plugIns.Instances[1].SecurityCheckpoint);
					AssertEquals("Type of PlugIn", -1, plugIns.Instances[1].RequestedTabPageIndex);

					AssertEquals("Type of PlugIn", typeof(DummyPlugIn2), plugIns.Instances[2].GetType());
					AssertEquals("Check Point", DummyCheckPoint.Instance, plugIns.Instances[2].SecurityCheckpoint);
					AssertEquals("Type of PlugIn", 2, plugIns.Instances[2].RequestedTabPageIndex);
				}
				finally
				{
					DummyCheckPoint.SecurityAllowed = true;
				}
			}
		}

		public void TestSelectPlugInTabWithControllerID()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			{
				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy1);
				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy2);
				var dummy2 = dummyForm.PlugIns.Instances[1];
				AssertEquals("PreCondition: The second instance of PlugIns is Dummy2", typeof(DummyPlugIn2), dummy2.GetType());

				dummyForm.PlugInIDToSelectOnLoaded = DummyControllerIDs.Dummy2;
				dummyForm.Show();
				// don't call UserIdleWorker.Flush() here, the user must see the tab selected immediately
				AssertEquals("Selected Tab page is Dummy2's", dummy2.TabPage, dummyForm.TabControl.SelectedTab);
			}
		}

		public void TestSelectPlugInTabWithControllerID_SubPlugIns()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			using (var subTabControl = new ZTabControl())
			{
				var parentTabPage = new ZTabPage();
				parentTabPage.Controls.Add(subTabControl);
				dummyForm.TabControl.Controls.Add(parentTabPage);

				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy1);
				dummyForm.PlugIns.Add(DummyControllerIDs.DummyControllerNoTabControl);

				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy2, null, () => subTabControl);
				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy3, null, () => subTabControl);

				var dummy3 = dummyForm.PlugIns.Instances[3];
				AssertEquals("PreCondition: The second instance of sub PlugIns is Dummy3", typeof(DummyPlugIn3), dummy3.GetType());

				AssertNotEquals("PreCondition: Selected Tab page is NOT Dummy3's", dummy3.TabPage, subTabControl.SelectedTab);

				dummyForm.PlugInIDToSelectOnLoaded = DummyControllerIDs.Dummy3;
				dummyForm.Show();

				AssertEquals("Selected parent tab page", parentTabPage, dummyForm.TabControl.SelectedTab);
				AssertEquals("Selected Dummy3 sub tab page", dummy3.TabPage, subTabControl.SelectedTab);
			}
		}

		public void TestInstances()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			using (var subTabControl1 = new ZTabControl())
			using (var subTabControl2 = new ZTabControl())
			{
				PrepareSubPlugInsTestData(dummyForm, subTabControl1, subTabControl2);

				AssertEquals(4, dummyForm.PlugIns.Instances.Length);
				AssertSame(dummyForm.PlugIns.Instances[0], dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy1));
				AssertSame(dummyForm.PlugIns.Instances[1], dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.DummyControllerNoTabControl));
				AssertSame(dummyForm.PlugIns.Instances[2], dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy2));
				AssertSame(dummyForm.PlugIns.Instances[3], dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy3));

				dummyForm.Show();

				AssertEquals(2, dummyForm.TopLevelTabControl.TabPages.Count);
				AssertEquals("First", dummyForm.TopLevelTabControl.TabPages[0].Text);
				AssertEquals("PlugIn1", dummyForm.TopLevelTabControl.TabPages[1].Text);
				AssertEquals(0, subTabControl1.TabPages.Count);
				AssertEquals(2, subTabControl2.TabPages.Count);
				AssertEquals("PlugIn2", subTabControl2.TabPages[0].Text);
				AssertEquals("PlugIn3", subTabControl2.TabPages[1].Text);
			}
		}

		public void TestAddSubPlugIn()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			using (var subTabControl1 = new ZTabControl())
			{
				dummyForm.Controls.Add(subTabControl1);

				AssertEquals("Pre-condition", 0, dummyForm.PlugIns.Instances.Length);

				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy1, null, () => subTabControl1);

				AssertEquals(1, dummyForm.PlugIns.Instances.Length);
				var plugIn = dummyForm.PlugIns.Instances[0];
				AssertEquals(DummyControllerIDs.Dummy1, plugIn.ControllerID);
				AssertEquals(subTabControl1, plugIn.TopLevelTabControl);

				dummyForm.PlugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy2, null, () => 0, () => null, () => subTabControl1);

				AssertEquals(2, dummyForm.PlugIns.Instances.Length);
				plugIn = dummyForm.PlugIns.Instances[0];
				AssertEquals(DummyControllerIDs.Dummy1, plugIn.ControllerID);
				AssertEquals(subTabControl1, plugIn.TopLevelTabControl);
				plugIn = dummyForm.PlugIns.Instances[1];
				AssertEquals(DummyControllerIDs.Dummy2, plugIn.ControllerID);
				AssertEquals(subTabControl1, plugIn.TopLevelTabControl);

				dummyForm.Show();

				AssertEquals(2, subTabControl1.TabPages.Count);
				AssertEquals("PlugIn2 was added with RequestedTabPageIndex 0", "PlugIn2", subTabControl1.TabPages[0].Text);
				AssertEquals("PlugIn1", subTabControl1.TabPages[1].Text);
			}
		}

		public void TestTopLevelMenus()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			using (var subTabControl = new ZTabControl())
			{
				dummyForm.Controls.Add(subTabControl);

				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy1);

				AssertEquals("Pre-conditions", 1, dummyForm.PlugIns.TopLevelMenus.Length);
				AssertEquals(1, dummyForm.PlugIns.TopLevelMenus[0].MenuItems.Length);
				AssertEquals("PlugIn1", dummyForm.PlugIns.TopLevelMenus[0].MenuItems[0].Text);

				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy2, null, () => subTabControl);

				AssertEquals("Sub menu item is added", 2, dummyForm.PlugIns.TopLevelMenus.Length);
				AssertEquals(1, dummyForm.PlugIns.TopLevelMenus[0].MenuItems.Length);
				AssertEquals(1, dummyForm.PlugIns.TopLevelMenus[1].MenuItems.Length);
				AssertEquals("PlugIn1", dummyForm.PlugIns.TopLevelMenus[0].MenuItems[0].Text);
				AssertEquals("PlugIn2", dummyForm.PlugIns.TopLevelMenus[1].MenuItems[0].Text);
			}
		}

		public void TestSetDisplayMode()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			using (var subTabControl1 = new ZTabControl())
			using (var subTabControl2 = new ZTabControl())
			{
				PrepareSubPlugInsTestData(dummyForm, subTabControl1, subTabControl2);

				AssertEquals("Pre-condition", ODisplayMode.Undefined, dummyForm.DisplayMode);
				AssertEquals(ODisplayMode.Undefined, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy1).DisplayMode);
				AssertEquals(ODisplayMode.Undefined, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.DummyControllerNoTabControl).DisplayMode);
				AssertEquals(ODisplayMode.Undefined, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy2).DisplayMode);
				AssertEquals(ODisplayMode.Undefined, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy3).DisplayMode);

				dummyForm.DisplayMode = ODisplayMode.New;

				AssertEquals("DisplayMode of all PlugIns gets updated", ODisplayMode.New, dummyForm.DisplayMode);
				AssertEquals(ODisplayMode.New, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy1).DisplayMode);
				AssertEquals(ODisplayMode.New, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.DummyControllerNoTabControl).DisplayMode);
				AssertEquals(ODisplayMode.New, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy2).DisplayMode);
				AssertEquals(ODisplayMode.New, dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy3).DisplayMode);
			}
		}

		void PrepareSubPlugInsTestData(TestFormWithTabControl dummyForm, ZTabControl subTabControl1, ZTabControl subTabControl2)
		{
			dummyForm.Controls.Add(subTabControl1);
			dummyForm.Controls.Add(subTabControl2);

			dummyForm.PlugIns.Add(DummyControllerIDs.Dummy1);
			dummyForm.PlugIns.Add(DummyControllerIDs.DummyControllerNoTabControl, null, () => subTabControl1);
			dummyForm.PlugIns.Add(DummyControllerIDs.Dummy2, null, () => subTabControl2);
			dummyForm.PlugIns.Add(DummyControllerIDs.Dummy3, null, () => subTabControl2);
		}

		public void TestGetPlugInForTesting()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			{
				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy1);
				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy2);
				AssertSame(dummyForm.PlugIns.Instances[0], dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy1));
				AssertSame(dummyForm.PlugIns.Instances[1], dummyForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy2));
			}
		}

		public void TestShowTheFirstTabPageIfPlugInIDToSelectNotSpecified()
		{
			using (var dummyForm = new TestFormWithTabControl(Dummy))
			{
				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy1);
				dummyForm.PlugIns.Add(DummyControllerIDs.Dummy2);

				var dummy2 = dummyForm.PlugIns.Instances[1];
				AssertEquals("PreCondition: The second instance of PlugIns is Dummy2", typeof(DummyPlugIn2), dummy2.GetType());

				dummyForm.Show();
				AssertEquals("Selected Tab page is the first one as nothing is specified", dummyForm.FirstTabPage, dummyForm.TabControl.SelectedTab);
			}
		}

		public void TestAddPlugInTabPages()
		{
			using (var tabControl = new ZTabControl())
			using (var plugIns = tabControl.PlugIns)
			{
				plugIns.Add(DummyControllerIDs.Dummy1);
				plugIns.Add(DummyControllerIDs.Dummy2);
				plugIns.Add(DummyControllerIDs.Dummy3);

				tabControl.TabPages.Add(plugIns.GetPlugIn(DummyControllerIDs.Dummy3).TabPage);

				AssertEquals("Precondition: three plugins on the tab control", 3, tabControl.PlugIns.Instances.Length);
				AssertEquals("Precondition: one tab page already added", 1, tabControl.TabPages.Count);

				plugIns.AddPlugInTabPages(tabControl);
				AssertEquals("New tab pages added, not duplicating the existing ones", 3, tabControl.TabPages.Count);

				plugIns.AddPlugInTabPages(tabControl);
				AssertEquals("No new tab pages added: all tabpages already existed", 3, tabControl.TabPages.Count);
			}
		}

		public void TestAddWhenPlugInsHaveBeenAddedToTabPages()
		{
			using (var tabControl = new ZTabControl())
			using (var plugIns = tabControl.PlugIns)
			{
				try
				{
					tabControl.Name = "TestTabControl";
					plugIns.Add(DummyControllerIDs.Dummy1);
					tabControl.TabPages.Add(plugIns.GetPlugIn(DummyControllerIDs.Dummy1).TabPage);
					plugIns.AddPlugInTabPages(tabControl);
					AssertExceptionThrown<ZException>(() => plugIns.Add(DummyControllerIDs.Dummy1));
					AssertEquals("AttemptedToAddPlugInAfterPlugInsAddedToTabPages:Dummy1:TestTabControl", ErrorReporter.LastKeyReported);
					AssertContains("Cannot add a plug in after plug ins have been added to tab pages. Consider adding the plug in at an earlier time. AddPlugInTabPages CallStack:", ErrorReporter.LastMessageReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
		}

		public void TestGetPlugIn()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.Add(DummyControllerIDs.Dummy1);
				plugIns.Add(DummyControllerIDs.Dummy2);

				AssertEquals(typeof(DummyPlugIn1), plugIns.GetPlugIn(DummyControllerIDs.Dummy1).GetType());
				AssertEquals(typeof(DummyPlugIn2), plugIns.GetPlugIn(DummyControllerIDs.Dummy2).GetType());
			}
		}

		public void TestIsPluginAvailable()
		{
			using (var form = new TestFormWithTabControl(Dummy))
			{
				form.PlugIns.Add(DummyControllerIDs.Dummy1);
				form.PlugIns.Add(DummyControllerIDs.Dummy2);

				Assert("Plugin should be available", form.PlugIns.IsPlugInAvailable(DummyControllerIDs.Dummy1));
				Assert("Plugin should be available", form.PlugIns.IsPlugInAvailable(DummyControllerIDs.Dummy2));
				Assert("Plugin shouldn't be available", !form.PlugIns.IsPlugInAvailable(DummyControllerIDs.Dummy3));
			}
		}

		public void TestOnBusinessObjectIsCancelledChanged()
		{
			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.Add(DummyControllerIDs.Dummy1);
				plugIns.Add(DummyControllerIDs.Dummy2);
				plugIns.Add(DummyControllerIDs.Dummy3);

				var plugin1 = (DummyPlugIn1)plugIns.GetPlugIn(DummyControllerIDs.Dummy1);
				var plugin2 = (DummyPlugIn2)plugIns.GetPlugIn(DummyControllerIDs.Dummy2);
				var plugin3 = (DummyPlugIn3)plugIns.GetPlugIn(DummyControllerIDs.Dummy3);

				Assert("False by default", !plugin1.IsCancelledCurrentValue);
				Assert("False by default", !plugin2.IsCancelledCurrentValue);
				Assert("False by default", !plugin3.IsCancelledCurrentValue);

				plugIns.OnBusinessObjectIsCancelledChanged(ZBool.True);
				Assert("Value should change", plugin1.IsCancelledCurrentValue);
				Assert("Value should change", plugin2.IsCancelledCurrentValue);
				Assert("Value should change", plugin3.IsCancelledCurrentValue);

				plugIns.OnBusinessObjectIsCancelledChanged(ZBool.False);
				Assert("Value should change", !plugin1.IsCancelledCurrentValue);
				Assert("Value should change", !plugin2.IsCancelledCurrentValue);
				Assert("Value should change", !plugin3.IsCancelledCurrentValue);
			}
		}

		public void TestGetBusinessEntityOverride()
		{
			var anotherDummy = Factory.New<DummyBusinessObject>();

			using (var plugIns = new PlugIns(Dummy, (ZTabControl)null))
			{
				plugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy1, 1);
				plugIns.AddPlugInAtTabPageIndex(DummyControllerIDs.Dummy2, 2, () => anotherDummy);

				var plugin1 = (DummyPlugIn1)plugIns.GetPlugIn(DummyControllerIDs.Dummy1);
				var plugin2 = (DummyPlugIn2)plugIns.GetPlugIn(DummyControllerIDs.Dummy2);
				AssertEquals(Dummy, plugin1.HostBusinessEntity);
				AssertEquals(anotherDummy, plugin2.HostBusinessEntity);
			}
		}

		#region Implementation

		public class TestFormWithTabControl : ZForm
		{
			public ZTabControl TabControl;
			public ZTabPage FirstTabPage;

			public TestFormWithTabControl(IBusiness bizO)
				: base(bizO)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				TabControl = new ZTabControl();
				FirstTabPage = new ZTabPage();
				FirstTabPage.Text = "First";
				TabControl.Controls.Add(FirstTabPage);
				Controls.Add(TabControl);
			}

			public void OnDragDrop_ForTesting(DragEventArgs e)
			{
				OnDragDrop(e);
			}

			public void OnDragOver_ForTesting(DragEventArgs e)
			{
				OnDragOver(e);
			}

			public void OnDataObjectPasted_ForTesting(IDataObject dataToPaste)
			{
				OnDataObjectPasted(new DataObjectPastedEventArgs(dataToPaste));
			}
		}

		#endregion
	}
}
