using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTemplateTabControlTest : TestCaseWithDummy
	{
		public void TestAdd()
		{
			ZStmNoteTabPage stmNoteTabPage = new ZStmNoteTabPage();
			ZLogsTabPage eventTabPage = new ZLogsTabPage();
			ZAutoSizedTabPagePlugIn documentTabPage = new ZAutoSizedTabPagePlugIn(PlugIn);
			ZTabPage testTabPage1 = new ZTabPage();
			ZTabPage testTabPage2 = new ZTabPage();
			ZTabPage testTabPage3 = new ZTabPage();

			testTabPage1.Text = "Test1";
			testTabPage2.Text = "Test2";
			testTabPage3.Text = "Test3";

			TabControl.TabPages.Add(stmNoteTabPage);
			TabControl.TabPages.Add(testTabPage1);
			TabControl.TabPages.Add(eventTabPage);
			TabControl.TabPages.Add(testTabPage2);
			TabControl.TabPages.Add(documentTabPage);
			TabControl.TabPages.Add(testTabPage3);

			AssertEquals("TabPage Count", 6, TabControl.TabPages.Count);

			AssertEquals("First TabPage", testTabPage1, TabControl.TabPages[0]);
			AssertEquals("Second TabPage", testTabPage2, TabControl.TabPages[1]);
			AssertEquals("Third TabPage", testTabPage3, TabControl.TabPages[2]);
			AssertEquals("Fourth TabPage", documentTabPage, TabControl.TabPages[3]);
			AssertEquals("Fifth TabPage", stmNoteTabPage, TabControl.TabPages[4]);
			AssertEquals("Sixth TabPage", eventTabPage, TabControl.TabPages[5]);
		}

		public void TestInsert()
		{
			ZStmNoteTabPage stmNoteTabPage = new ZStmNoteTabPage();
			ZLogsTabPage eventTabPage = new ZLogsTabPage();
			ZAutoSizedTabPagePlugIn documentTabPage = new ZAutoSizedTabPagePlugIn(PlugIn);
			ZTabPage testTabPage1 = new ZTabPage();
			ZTabPage testTabPage2 = new ZTabPage();
			ZTabPage testTabPage3 = new ZTabPage();

			testTabPage1.Text = "Test1";
			testTabPage2.Text = "Test2";
			testTabPage3.Text = "Test3";

			AssertEquals("TabPage Count", 0, TabControl.TabPages.Count);

			TabControl.TabPages.Insert(stmNoteTabPage, 0);
			AssertEquals("TabPage Count", 1, TabControl.TabPages.Count);
			AssertEquals("First TabPage", stmNoteTabPage, TabControl.TabPages[0]);

			TabControl.TabPages.Insert(testTabPage1, 1);
			AssertEquals("TabPage Count", 2, TabControl.TabPages.Count);
			AssertEquals("First TabPage", testTabPage1, TabControl.TabPages[0]);
			AssertEquals("Second TabPage", stmNoteTabPage, TabControl.TabPages[1]);

			TabControl.TabPages.Insert(eventTabPage, 1);
			AssertEquals("TabPage Count", 3, TabControl.TabPages.Count);
			AssertEquals("First TabPage", testTabPage1, TabControl.TabPages[0]);
			AssertEquals("Second TabPage", stmNoteTabPage, TabControl.TabPages[1]);
			AssertEquals("Third TabPage", eventTabPage, TabControl.TabPages[2]);

			TabControl.TabPages.Insert(testTabPage2, 5);
			AssertEquals("TabPage Count", 4, TabControl.TabPages.Count);
			AssertEquals("First TabPage", testTabPage1, TabControl.TabPages[0]);
			AssertEquals("Second TabPage", testTabPage2, TabControl.TabPages[1]);
			AssertEquals("Third TabPage", stmNoteTabPage, TabControl.TabPages[2]);
			AssertEquals("Fourth TabPage", eventTabPage, TabControl.TabPages[3]);

			TabControl.TabPages.Insert(documentTabPage, 0);
			AssertEquals("TabPage Count", 5, TabControl.TabPages.Count);
			AssertEquals("First TabPage", testTabPage1, TabControl.TabPages[0]);
			AssertEquals("Second TabPage", testTabPage2, TabControl.TabPages[1]);
			AssertEquals("Third TabPage", documentTabPage, TabControl.TabPages[2]);
			AssertEquals("Fourth TabPage", stmNoteTabPage, TabControl.TabPages[3]);
			AssertEquals("Fifth TabPage", eventTabPage, TabControl.TabPages[4]);

			TabControl.TabPages.Insert(testTabPage3, 1);
			AssertEquals("TabPage Count", 6, TabControl.TabPages.Count);
			AssertEquals("First TabPage", testTabPage1, TabControl.TabPages[0]);
			AssertEquals("Second TabPage", testTabPage3, TabControl.TabPages[1]);
			AssertEquals("Third TabPage", testTabPage2, TabControl.TabPages[2]);
			AssertEquals("Fourth TabPage", documentTabPage, TabControl.TabPages[3]);
			AssertEquals("Fifth TabPage", stmNoteTabPage, TabControl.TabPages[4]);
			AssertEquals("Sixth TabPage", eventTabPage, TabControl.TabPages[5]);
		}

		public void TestInsert_IfTabIsSelectedTabThenItShouldBeBound_WhenFirstTabInserted()
		{
			ZStmNoteTabPage stmNoteTabPage = new ZStmNoteTabPage();
			Form.Controls.Add(TabControl);
			Form.Show();

			TabControl.TabPages.Insert(stmNoteTabPage, 0);
			AssertEquals(true, stmNoteTabPage.IsBound);
		}

		public void TestInsert_IfTabIsSelectedTabThenItShouldBeBound_WhenReplacingExistingTab()
		{
			TestBindingTabPage testTabPage1 = new TestBindingTabPage();
			testTabPage1.Text = "Test1";
			TabControl.TabPages.Add(testTabPage1);

			Form.Controls.Add(TabControl);
			Form.Show();

			ZTabPage testTabPage2 = new ZTabPage();
			testTabPage1.Text = "Test2";
			TabControl.TabPages.Insert(testTabPage2, 0);
			AssertEquals(true, testTabPage1.IsBound);
		}

		public void TestSelectedIndexNotChangedWhenModifyingAfterSelectedTab()
		{
			ZStmNoteTabPage stmNoteTabPage = new ZStmNoteTabPage();
			ZAutoSizedTabPagePlugIn documentTabPage = new ZAutoSizedTabPagePlugIn(PlugIn);
			ZTabPage testTabPage1 = new ZTabPage();
			testTabPage1.Text = "Test1";

			ZTemplateTabControl tabControl = new ZTemplateTabControl();
			bool selectedIndexChanged = false;
			tabControl.SelectedIndexChanged += delegate
			{ selectedIndexChanged = true; };

			Form.Controls.Add(tabControl);
			Form.Show();

			tabControl.TabPages.Add(testTabPage1);
			tabControl.TabPages.Add(documentTabPage);
			tabControl.TabPages.Insert(stmNoteTabPage, 0);
			tabControl.TabPages.Remove(stmNoteTabPage);

			AssertEquals("Add, Insert, Remove doesn't change SelectedIndex when after selected tab", false, selectedIndexChanged);
		}

		public void TestVisibleNotChangedWhenModifyingAfterSelectedTab()
		{
			ZStmNoteTabPage stmNoteTabPage = new ZStmNoteTabPage();
			ZAutoSizedTabPagePlugIn documentTabPage = new ZAutoSizedTabPagePlugIn(PlugIn);

			TextBox textBox1 = new TextBox();
			textBox1.Text = "TextBox1";
			stmNoteTabPage.Controls.Add(textBox1);
			object eventSender = null;
			textBox1.VisibleChanged += delegate(object sender, EventArgs e)
			{ eventSender = sender; };

			TextBox textBox2 = new TextBox();
			textBox2.Text = "TextBox2";
			documentTabPage.Controls.Add(textBox2);
			textBox2.VisibleChanged += delegate(object sender, EventArgs e)
			{ eventSender = sender; };

			ZTabPage testTabPage1 = new ZTabPage();
			testTabPage1.Text = "Test1";

			Form.Controls.Add(TabControl);
			Form.Show();

			TabControl.TabPages.Add(testTabPage1);
			TabControl.TabPages.Insert(stmNoteTabPage, 1);
			TabControl.TabPages.Add(documentTabPage);
			TabControl.TabPages.Remove(stmNoteTabPage);

			AssertEquals("Controls on Tab[1] and Tab[2] not visible by adding or removing them", null, eventSender);
		}

		public void TestActiveTabPageFocusesFirstControlOnLoad()
		{
			ZTabPage tabPage = new ZTabPage();
			TabControl.TabPages.Insert(tabPage, 0);
			Form.Controls.Add(TabControl);

			TextBox textBox = new TextBox();
			textBox.Text = "focusme";
			tabPage.Controls.Add(textBox);
			Form.Show();
			Application.DoEvents();
			AssertEquals("Should focus the first control on the initial tab control", true, Form.ActiveControl.Text == "focusme");
		}

		public void TestFormExpandedAndShrunkToFitNotesTab()
		{
			Form.FormBorderStyle = FormBorderStyle.FixedSingle;

			ZTabPage tabPage = new ZTabPage();
			ZStmNoteTabPage notesTabPage = new ZStmNoteTabPage();
			TabControl.TabPages.Add(tabPage);
			TabControl.TabPages.Add(notesTabPage);

			TabControl.Dock = DockStyle.Fill;
			Form.Controls.Add(TabControl);
			Form.Show();
			Application.DoEvents();

			int initialFormHeight = Form.Height;
			TabControl.SelectedIndex = 1;
			Assert("Form should be enlarged to fit the notes tab", Form.Height > initialFormHeight);
			TabControl.SelectedIndex = 0;
			Assert("Form reverted to original size when non-notes tab selected", Form.Height == initialFormHeight);
		}

		public void TestReportNullReferenceExceptionWhenGetSelectedTabAfterDispose()
		{
			using (var form = new ZTestForm())
			{
				var templateTabControl = new ZTemplateTabControl();
				templateTabControl.Visible = true;
				form.Controls.Add(templateTabControl);

				templateTabControl.SelectedIndexChanging += (sender, e) =>
				{
					form.Dispose();
				};

				templateTabControl.Controls.Add(new ZTabPage());
				templateTabControl.Controls.Add(new ZTabPage());
				form.Show();

				AssertNoExceptionThrown(() => templateTabControl.SelectTab(1));
			}
		}

		#region Test Classes

		class TestBindingTabPage : ZBindingTabPage { }

		sealed class TestPlugIn : ZPlugIn
		{
			public TestPlugIn(IBusiness hostBusinessEntity)
				: base(hostBusinessEntity)
			{
			}

			protected override LicenceCheckpoint LicenceCheckPoint
			{
				get { return Env.Licence.AlwaysAllow; }
			}

			public override string Name
			{
				get { return "TestPlugIn"; }
			}

			protected override ZBool HasUserControl
			{
				get { return true; }
			}

			protected override Control GetNewUserControl()
			{
				return new Control();
			}

			protected override IBusiness GetBusinessEntityForPlugIn()
			{
				return this.HostBusinessEntity;
			}
		}

		#endregion

		#region Implementation

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyEnterpriseBusinessObject); }
		}

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(Dummy);
				}
				return form;
			}
		}
		ZForm form;

		ZTemplateTabControl TabControl
		{
			get
			{
				if (tabControl == null)
				{
					tabControl = new ZTemplateTabControl();
				}
				return tabControl;
			}
		}
		ZTemplateTabControl tabControl;

		TestPlugIn PlugIn
		{
			get
			{
				if (plugIn == null)
				{
					plugIn = new TestPlugIn(Dummy);
				}
				return plugIn;
			}
		}
		TestPlugIn plugIn;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (tabControl != null)
			{
				tabControl.Dispose();
			}
			if (plugIn != null)
			{
				plugIn.Dispose();
			}
		}

		#endregion
	}
}
