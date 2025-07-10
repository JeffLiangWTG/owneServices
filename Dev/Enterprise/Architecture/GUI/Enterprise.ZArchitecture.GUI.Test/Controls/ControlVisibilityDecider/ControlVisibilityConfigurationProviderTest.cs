using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI.Layout;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControlVisibilityConfigurationProviderTest : TestCaseWithFactory
	{
		public void TestFormCustomisationSettings_NegativeRowNumber()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var rowPanel = new RowLayoutPanel())
			using (var label = new Label())
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			using (var panel3 = new ZPanel())
			using (var panel4 = new ZPanel())
			{
				label.Name = "Label1";

				rowPanel.Controls.Add(label);

				rowPanel.SetRow(label, 0);

				panel1.Name = "Details";
				panel4.Name = "LeftTopPanel";
				tabPage.Name = "Tab1";
				tabControl.TabPages.Add(tabPage);
				Form.Controls.Add(tabControl);

				panel1.Controls.Add(panel2);
				panel2.Controls.Add(panel3);
				panel3.Controls.Add(rowPanel);
				tabPage.Controls.Add(panel1);
				tabPage.Controls.Add(panel4);

				AssertEquals(0, rowPanel.GetRow(label));

				var formSettings = new TestFormCustomisationSettings_RowLayoutPanel();
				formSettings.Label1RowNumber = -10;
				ConfigurationProvider.SetIsVisibilityConfigured(rowPanel, true);
				var configuration = ConfigurationProvider.configurations[rowPanel];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;

				CombineAssertions(() =>
				{
					AssertNoExceptionThrown(() => configuration.Refresh());
					Assert(rowPanel.GetRow(label) >= 0);
				});
			}
		}

		public void TestFormCustomisationSettings_DuplicateRowNumber()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var rowPanel = new RowLayoutPanel())
			using (var label = new Label())
			using (var label2 = new Label())
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			using (var panel3 = new ZPanel())
			using (var panel4 = new ZPanel())
			{
				label.Name = "Label1";
				label2.Name = "Label2";

				rowPanel.Controls.Add(label);
				rowPanel.Controls.Add(label2);

				rowPanel.SetRow(label, 0);
				rowPanel.SetRow(label2, 1);

				panel1.Name = "Details";
				panel4.Name = "LeftTopPanel";
				tabPage.Name = "Tab1";
				tabControl.TabPages.Add(tabPage);
				Form.Controls.Add(tabControl);

				panel1.Controls.Add(panel2);
				panel2.Controls.Add(panel3);
				panel3.Controls.Add(rowPanel);
				tabPage.Controls.Add(panel1);
				tabPage.Controls.Add(panel4);

				AssertEquals(0, rowPanel.GetRow(label));
				AssertEquals(1, rowPanel.GetRow(label2));

				var formSettings = new TestFormCustomisationSettings_RowLayoutPanel();
				formSettings.Label1RowNumber = 5;
				formSettings.Label2RowNumber = 5;
				ConfigurationProvider.SetIsVisibilityConfigured(rowPanel, true);
				var configuration = ConfigurationProvider.configurations[rowPanel];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;
				Form.Show();

				configuration.Refresh();
				Assert(rowPanel.GetRow(label) != rowPanel.GetRow(label2));
			}
		}

		public void TestFormCustomisationSettings_DuplicateRowWithNotVisibleGUI()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var rowPanel = new RowLayoutPanel())
			using (var label = new Label())
			using (var labelNotVisible = new Label())
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			using (var panel3 = new ZPanel())
			using (var panel4 = new ZPanel())
			{
				label.Name = "Label1";
				labelNotVisible.Name = "LabelNotVisible";

				rowPanel.Controls.Add(labelNotVisible);
				rowPanel.Controls.Add(label);

				rowPanel.SetRow(label, 0);
				rowPanel.SetRow(labelNotVisible, 1);

				panel1.Name = "Details";
				panel4.Name = "LeftTopPanel";
				tabPage.Name = "Tab1";
				tabControl.TabPages.Add(tabPage);
				Form.Controls.Add(tabControl);

				panel1.Controls.Add(panel2);
				panel2.Controls.Add(panel3);
				panel3.Controls.Add(rowPanel);
				tabPage.Controls.Add(panel1);
				tabPage.Controls.Add(panel4);

				AssertEquals(0, rowPanel.GetRow(label));
				AssertEquals(1, rowPanel.GetRow(labelNotVisible));

				var formSettings = new TestFormCustomisationSettings_RowLayoutPanel();
				formSettings.Label1RowNumber = 5;
				formSettings.LabelNotVisibleRowNumber = 5;
				ConfigurationProvider.SetIsVisibilityConfigured(rowPanel, true);
				var configuration = ConfigurationProvider.configurations[rowPanel];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;
				Form.Show();

				configuration.Refresh();

				CombineAssertions(() =>
				{
					AssertEquals(5, rowPanel.GetRow(label));
					AssertEquals(5, rowPanel.GetRow(labelNotVisible));
				});
			}
		}

		public void TestFormCustomisationSettings_DuplicateRowWithControlEmptyPlacement()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var rowPanel = new RowLayoutPanel())
			using (var label = new Label())
			using (var labelWithoutPlacement = new Label())
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			using (var panel3 = new ZPanel())
			using (var panel4 = new ZPanel())
			{
				label.Name = "Label1";
				labelWithoutPlacement.Name = "LabelWithoutPlacement";

				rowPanel.Controls.Add(labelWithoutPlacement);
				rowPanel.Controls.Add(label);

				rowPanel.SetRow(label, 0);
				rowPanel.SetRow(labelWithoutPlacement, 1);

				panel1.Name = "Details";
				panel4.Name = "LeftTopPanel";
				tabPage.Name = "Tab1";
				tabControl.TabPages.Add(tabPage);
				Form.Controls.Add(tabControl);

				panel1.Controls.Add(panel2);
				panel2.Controls.Add(panel3);
				panel3.Controls.Add(rowPanel);
				tabPage.Controls.Add(panel1);
				tabPage.Controls.Add(panel4);

				AssertEquals(0, rowPanel.GetRow(label));
				AssertEquals(1, rowPanel.GetRow(labelWithoutPlacement));

				var formSettings = new TestFormCustomisationSettings_RowLayoutPanel();
				formSettings.Label1RowNumber = 5;
				formSettings.LabelWithoutPlacementRowNumber = 5;
				ConfigurationProvider.SetIsVisibilityConfigured(rowPanel, true);
				var configuration = ConfigurationProvider.configurations[rowPanel];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;
				Form.Show();

				configuration.Refresh();

				CombineAssertions(() =>
				{
					AssertEquals(5, rowPanel.GetRow(label));
					AssertEquals(5, rowPanel.GetRow(labelWithoutPlacement));
				});
			}
		}

		public void TestLayoutPerformed()
		{
			using (var label = new Label())
			using (var label2 = new Label())
			using (var label3 = new Label())
			{
				var formSettings = new TestFormCustomisationSettings();

				ConfigurationProvider.SetIsVisibilityConfigured(ContainerControl, true);
				Form.Controls.Add(ContainerControl);
				var configuration = ConfigurationProvider.configurations[containerControl];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;

				label.Name = "RightControl";
				ContainerControl.Controls.Add(label);

				label2.Name = "WrongControl";
				ContainerControl.Controls.Add(label2);

				label3.Name = "ThirdControl";
				ContainerControl.Controls.Add(label3);

				Form.Controls.Add(ContainerControl);
				Form.Show();
				ContainerControl.SetDataBinding(ConfigurationMode, "");

				AssertEquals("Precondition", true, label3.Visible);

				formSettings.ElementVisibleName = "RightControl";
				formSettings.ElementUndecidedName = "ThirdControl";
				configuration.Refresh();
				AssertEquals(true, label.Visible);
				AssertEquals(false, label2.Visible);
				AssertEquals(true, label3.Visible);

				label3.Visible = false;
				configuration.Refresh();
				AssertEquals(true, label.Visible);
				AssertEquals(false, label2.Visible);
				AssertEquals(false, label3.Visible);

				formSettings.ElementVisibleName = "WrongControl";
				configuration.Refresh();
				AssertEquals(false, label.Visible);
				AssertEquals(true, label2.Visible);
				AssertEquals(false, label3.Visible);
			}
		}

		public void TestParentControlChanged()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var tabPage2 = new ZTabPage())
			using (var textBox = new ZTextBox())
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			{
				textBox.Name = "Label1";

				panel1.Name = "LeftTopPanel";
				panel2.Name = "LeftTopPanel";
				tabPage1.Name = "Tab1";
				tabPage2.Name = "Tab2";
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(tabPage2);
				Form.Controls.Add(tabControl);

				panel1.Controls.Add(textBox);
				tabPage1.Controls.Add(panel1);
				tabPage2.Controls.Add(panel2);

				Assert(panel1.Controls.Contains(textBox));
				Assert(panel1.BindingContext == textBox.BindingContext);

				var formSettings = new TestFormCustomisationSettings_TabPages();
				ConfigurationProvider.SetIsVisibilityConfigured(panel1, true);
				ConfigurationProvider.SetIsVisibilityConfigured(panel2, true);
				var configuration = ConfigurationProvider.configurations[panel1];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;
				configuration.Refresh();
				var configuration2 = ConfigurationProvider.configurations[panel2];
				configuration2.FormCustomisationSettings = formSettings;
				configuration2.ForceRefreshEvenIfTemplateNotChanged = true;
				configuration2.Refresh();

				Assert(!panel1.Controls.Contains(textBox));
				Assert(panel2.Controls.Contains(textBox));
				Assert(panel2.BindingContext == textBox.BindingContext);
			}
		}

		public void TestLayoutPerformed_RowLayoutPanel()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var rowPanel = new RowLayoutPanel())
			using (var label = new Label())
			using (var label2 = new Label())
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			using (var panel3 = new ZPanel())
			using (var panel4 = new ZPanel())
			{
				label.Name = "Label1";
				label2.Name = "Label2";

				rowPanel.Controls.Add(label);
				rowPanel.Controls.Add(label2);

				rowPanel.SetRow(label, 0);
				rowPanel.SetRow(label2, 1);

				panel1.Name = "Details";
				panel4.Name = "LeftTopPanel";
				tabPage.Name = "Tab1";
				tabControl.TabPages.Add(tabPage);
				Form.Controls.Add(tabControl);

				panel1.Controls.Add(panel2);
				panel2.Controls.Add(panel3);
				panel3.Controls.Add(rowPanel);
				tabPage.Controls.Add(panel1);
				tabPage.Controls.Add(panel4);

				AssertEquals(0, rowPanel.GetRow(label));
				AssertEquals(1, rowPanel.GetRow(label2));

				var formSettings = new TestFormCustomisationSettings_RowLayoutPanel();
				ConfigurationProvider.SetIsVisibilityConfigured(rowPanel, true);
				var configuration = ConfigurationProvider.configurations[rowPanel];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;
				configuration.Refresh();

				CombineAssertions(() =>
				{
					AssertEquals(1, rowPanel.GetRow(label));
					AssertEquals(0, rowPanel.GetRow(label2));
				});
			}
		}

		public void TestInitializeWorkflowTabs()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var tabPage2 = new ZTabPage())
			using (var panel = new ZPanel())
			{
				tabPage1.Name = "Tab1";
				panel.Name = "LeftTopPanel";
				tabPage1.Controls.Add(panel);
				tabPage2.Name = "Tab2";
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(tabPage2);
				Form.Controls.Add(tabControl);

				var tab1Initialised = false;
				var tab2Initialised = false;

				tabPage1.VisibleChanged += (s, e) => tab1Initialised = true;
				tabPage2.VisibleChanged += (s, e) => tab2Initialised = true;

				var formSettings = new TestFormCustomisationSettings_TabPages();
				formSettings.CustomisableTabPageNames = new[] { tabPage1.Name, tabPage2.Name };

				ConfigurationProvider.SetIsVisibilityConfigured(panel, true);
				var configuration = ConfigurationProvider.configurations[panel];

				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;
				configuration.Refresh();

				Form.Show();
				Application.DoEvents();

				tabControl.SelectedTab = tabPage1; // It turns out that setting visibility to true is a big waste of time, since when you access the tab it will do that anyway.
				Application.DoEvents();
				tabControl.SelectedTab = tabPage2; // It turns out that setting visibility to true is a big waste of time, since when you access the tab it will do that anyway.
				Application.DoEvents();
				Assert(tab2Initialised);
				Assert(tab1Initialised);

				Assert("weird code requires weird tests and this is one of them... the test failed because ControlVisibilityConfiguration did not initialised tabs by " +
					"setting their Visibility to 'true' which in turn adds child controls to tabs and sets bindings. This is necessary prequisite to properly set control placements",
					tab1Initialised && tab2Initialised);
			}
		}

		public void TestRefresh_NoExceptionThrown()
		{
			var panel = new ZPanel();
			ConfigurationProvider.SetIsVisibilityConfigured(panel, true);

			var configuration = ConfigurationProvider.configurations[panel];
			panel.Disposed += ((sender, args) => configuration.Refresh());

			AssertNoExceptionThrown(() => panel.Dispose());
		}

		public void TestLabelShownOnDifferentParentTab()
		{
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var tabPage2 = new ZTabPage())
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			using (var userControl1 = new ZUserControl())
			using (var userControl2 = new ZUserControl())
			using (var dropEdit = new ZDropEdit())
			{
				dropEdit.Name = "Label1";

				panel1.Name = "LeftTopPanel";
				panel2.Name = "LeftTopPanel";
				tabPage1.Name = "Tab1";
				tabPage2.Name = "Tab2";
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(tabPage2);
				Form.Controls.Add(tabControl);

				panel1.Controls.Add(dropEdit);

				tabPage1.Controls.Add(userControl1);
				tabPage2.Controls.Add(userControl2);

				userControl1.Controls.Add(panel1);
				userControl2.Controls.Add(panel2);
				userControl1.SetDataBinding(ConfigurationMode, "");
				userControl2.SetDataBinding(ConfigurationMode, "");
				userControl1.BindingSource.SetBindingMember(dropEdit, "testtable.testfield");

				AssertEquals("dropEdit BindTo precondition", "testtable.testfield", dropEdit.BindTo);
				Assert(panel1.Controls.Contains(dropEdit));

				var formSettings = new TestFormCustomisationSettings_TabPages();
				ConfigurationProvider.SetIsVisibilityConfigured(panel1, true);
				ConfigurationProvider.SetIsVisibilityConfigured(panel2, true);

				var configuration = ConfigurationProvider.configurations[panel1];
				configuration.FormCustomisationSettings = formSettings;
				configuration.ForceRefreshEvenIfTemplateNotChanged = true;
				configuration.Refresh();

				var configuration2 = ConfigurationProvider.configurations[panel2];
				configuration2.FormCustomisationSettings = formSettings;
				configuration2.ForceRefreshEvenIfTemplateNotChanged = true;
				configuration2.Refresh();

				Assert(!panel1.Controls.Contains(dropEdit));
				Assert(panel2.Controls.Contains(dropEdit));
				AssertEquals("BindTo must be copied from the previous usercontrol's BindingSource.SetBindingMember", "testtable.testfield", dropEdit.BindTo);
			}
		}

		#region Test Classes

		class TestFormCustomisationSettings_RowLayoutPanel : IFormCustomisationSettings
		{
			public bool AllFieldsAreOnDefaultTabs()
			{
				return false;
			}

			public int Label1RowNumber { get; set; } = 1;
			public int Label2RowNumber { get; set; }
			public int LabelNotVisibleRowNumber { get; set; } = 2;
			public int LabelWithoutPlacementRowNumber { get; set; }

			TabPlacement IFormCustomisationSettings.GetTabPlacement(string elementName)
			{
				if (elementName == "Label1")
				{
					return new TabPlacement("Tab1", TabPlacement.Placements.TopLeft, Label1RowNumber);
				}
				else if (elementName == "Label2")
				{
					return new TabPlacement("Tab1", TabPlacement.Placements.TopLeft, Label2RowNumber);
				}
				else if (elementName == "LabelNotVisible")
				{
					return new TabPlacement("Tab1", TabPlacement.Placements.TopLeft, LabelNotVisibleRowNumber);
				}
				else if (elementName == "LabelWithoutPlacement")
				{
					return new TabPlacement("Tab1", "", LabelWithoutPlacementRowNumber);
				}
				else if (elementName == "ZForm")
				{
					return new TabPlacement("Tab1", TabPlacement.Placements.TopLeft, 0);
				}
				else if (elementName == "Details")
				{
					return new TabPlacement("Tab1", TabPlacement.Placements.TopLeft, 0);
				}
				else
				{
					return new TabPlacement();
				}
			}

			bool? IFormCustomisationSettings.IsElementVisible(string elementName, ElementType elementType)
			{
				if (elementName == "LabelNotVisible")
				{
					return false;
				}
				return true;
			}

			string[] IFormCustomisationSettings.CustomisableTabPageNames
			{
				get { return System.Array.Empty<string>(); }
			}
		}

		class TestFormCustomisationSettings : IFormCustomisationSettings
		{
			public bool AllFieldsAreOnDefaultTabs()
			{
				return false;
			}

			TabPlacement IFormCustomisationSettings.GetTabPlacement(string elementName)
			{
				return new TabPlacement();
			}

			bool? IFormCustomisationSettings.IsElementVisible(string elementName, ElementType elementType)
			{
				if (elementName == ElementVisibleName)
				{
					return true;
				}
				else if (elementName == ElementUndecidedName)
				{
					return null;
				}
				else
				{
					return false;
				}
			}

			public string ElementVisibleName = "";
			public string ElementUndecidedName = "";

			string[] IFormCustomisationSettings.CustomisableTabPageNames
			{
				get { return System.Array.Empty<string>(); }
			}
		}

		class TestFormCustomisationSettings_TabPages : IFormCustomisationSettings
		{
			public bool AllFieldsAreOnDefaultTabs()
			{
				return false;
			}

			TabPlacement IFormCustomisationSettings.GetTabPlacement(string elementName)
			{
				if (elementName == "Label1")
				{
					return new TabPlacement("Tab2", TabPlacement.Placements.TopLeft, 0);
				}
				else
				{
					return new TabPlacement();
				}
			}

			bool? IFormCustomisationSettings.IsElementVisible(string elementName, ElementType elementType)
			{
				return true;
			}

			public string[] CustomisableTabPageNames { get; set; }

			string[] IFormCustomisationSettings.CustomisableTabPageNames
			{
				get { return CustomisableTabPageNames ?? System.Array.Empty<string>(); }
			}
		}

		class TestContainerControl : ZUserControl
		{
			public bool IsControlVisible(Control control)
			{
				return control.Name == "RightControl";
			}
		}

		class TestJobConfigurationMode : DummyBusinessObject, IWorkflowProviderCore
		{
			public TestJobConfigurationMode(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IWorkflowProviderCore Members

			IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			ZString IWorkflowProviderCore.WorkflowType
			{
				get { return "XXX"; }
			}

			#endregion
		}

		#endregion

		#region Implementation

		ZForm Form
		{
			get { return form ?? (form = new ZForm(ConfigurationMode)); }
		}
		ZForm form;

		TestContainerControl ContainerControl
		{
			get { return containerControl ?? (containerControl = new TestContainerControl()); }
		}
		TestContainerControl containerControl;

		TestJobConfigurationMode ConfigurationMode
		{
			get { return configurationMode ?? (configurationMode = Factory.New<TestJobConfigurationMode>()); }
		}
		TestJobConfigurationMode configurationMode;

		ControlVisibilityConfigurationProvider ConfigurationProvider
		{
			get { return configurationProvider ?? (configurationProvider = new ControlVisibilityConfigurationProvider()); }
		}
		ControlVisibilityConfigurationProvider configurationProvider;

		protected override void TearDown()
		{
			base.TearDown();
			if (containerControl != null)
			{
				containerControl.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
