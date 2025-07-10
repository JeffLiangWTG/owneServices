using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZControlExtensionsTest : TestCaseWithFactory
	{
		#region SetReadOnlyIncludingChildren

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
		public void TestSetReadOnlyIncludingChildren()
		{
			using (var control = new ZUserControl())
			using (var button = new Button())
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var tabPage2 = new ZTabPage())
			using (var textBox = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			using (var checkBoxDontIgnoreMe = new ZCheckBox() { Name = "Don't IgnoreMe" })
			using (var checkBoxIgnoreMe = new ZCheckBox() { Name = "IgnoreMe" })
			using (var linkLabel = new ZLinkLabel())
			using (var dummyIReadOnlyToggleControl = new DummyIReadOnlyToggleControl())
			using (var toolStrip = new ToolStrip())
			using (var toolStripButton1 = new ToolStripButton())
			using (var toolStripButton2 = new ToolStripButton())
			using (var dataGridView = new DataGridView())
			{
				tabControl.TabPages.Add(tabPage);
				tabControl.TabPages.Add(tabPage2);
				tabPage2.ShouldBeReadOnlyInViewMode = false;
				tabPage.RunWhenBindingOrFirstShown((_, __) => tabPage.Controls.Add(textBox));
				tabPage2.RunWhenBindingOrFirstShown((_, __) => tabPage.Controls.Add(textBox2));
				control.Controls.Add(tabControl);
				control.Controls.Add(button);
				control.Controls.Add(checkBoxDontIgnoreMe);
				control.Controls.Add(checkBoxIgnoreMe);
				control.Controls.Add(linkLabel);
				control.Controls.Add(dummyIReadOnlyToggleControl);
				control.Controls.Add(dataGridView);
				TypeDescriptor.AddAttributes(toolStripButton1, new CanBeReadOnlyUIAttribute());
				toolStrip.Items.Add(toolStripButton1);
				toolStrip.Items.Add(toolStripButton2);
				control.Controls.Add(toolStrip);

				control.SetReadOnlyIncludingChildren(new List<string> { checkBoxIgnoreMe.Name });
				tabPage.NotifyBindingOrShowing();
				AssertEquals("TextBox ultimately made ReadOnly = true", true, textBox.ReadOnly);
				AssertEquals("TextBox2 still editable", false, textBox2.ReadOnly);
				AssertEquals("CheckBox made ReadOnly = true", true, checkBoxDontIgnoreMe.ReadOnly);
				AssertEquals("checkBoxIgnoreMe should not be made ReadOnly", false, checkBoxIgnoreMe.ReadOnly);
				AssertEquals("Button not affected", true, button.Enabled);
				AssertEquals("LinkLabel made disabled", false, linkLabel.Enabled);
				AssertEquals("dummyIReadOnlyToggleControl made ReadOnly", true, dummyIReadOnlyToggleControl.ReadOnly);
				AssertEquals("dummyIReadOnlyToggleControl.ZButton made ReadOnly", true, dummyIReadOnlyToggleControl.AButton.ReadOnly);
				AssertEquals("dummyIReadOnlyToggleControl.ZTextBox made ReadOnly", true, dummyIReadOnlyToggleControl.ATextBox.ReadOnly);
				AssertEquals("ToolStripButton with attribute ReadOnlyUIAttribute made ReadOnly = true", false, toolStripButton1.Enabled);
				AssertEquals("ToolStripButton not affected", true, toolStripButton2.Enabled);
				AssertEquals("DataGridView made disabled", false, dataGridView.Enabled);

				var newTextBox = new ZTextBox();
				control.Controls.Add(newTextBox);
				AssertEquals("Newly added controls are also made ReadOnly=true", true, newTextBox.ReadOnly);
			}
		}

		public void TestSetReadOnlyIncludingChildren_WithBinding()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			using (var form = new ZChildForm(dummy))
			using (var tabPage = new ZTabPage())
			using (var textBox = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name })
			{
				tabPage.Controls.Add(textBox);
				tabPage.SetReadOnlyIncludingChildren(true);

				using (var testTabControl = new ZTabControl())
				{
					testTabControl.TabPages.Add(tabPage);
					form.Controls.Add(testTabControl);
					form.Show();

					Assert(textBox.ReadOnly);

					form.SetDataBinding(dummy, null);
					Assert("The readonly binding should still be removed, even after rebinding", textBox.ReadOnly);
				}
			}
		}

		public void TestSetReadOnlyIncludingChildren_IReadOnlyAutomationOptional()
		{
			using (var button = new Button())
			{
				AssertNoExceptionThrown("Although Button doesn't have a ReadOnly property, calling SetReadOnlyIncludingChildren shouldn't cause disaster, and yet...", () => button.SetReadOnlyIncludingChildren());
			}

			using (var button = new ZButton())
			{
				button.EditableInViewMode = true;
				button.SetReadOnlyIncludingChildren();
				AssertEquals("ReadOnly should not have been set to true because ShouldSetReadOnlyWhenSettingIncludingChildren is false, and yet...", false, button.ReadOnly);

				button.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
				button.EditableInViewMode = true;
				button.SetReadOnlyIncludingChildren();
				AssertEquals("ReadOnly should have been set to true because ShouldSetReadOnlyWhenSettingIncludingChildren is true, and yet...", true, button.ReadOnly);
			}
		}

		public void TestSetReadOnlyIncludingChildren_DoesntCauseMemoryLeaks()
		{
			var textboxReference = CreateAndDisposeFormForTestReadOnly();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert("There should be no remaining references to any controls", !textboxReference.TryGetTarget(out var ignored));
		}

		WeakReference<ZTextBox> CreateAndDisposeFormForTestReadOnly()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var form = new ZChildForm(dummy);
			var textBox = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name };
			form.Controls.Add(textBox);
			form.SetReadOnlyIncludingChildren(true);

			form.Show();
			Application.DoEvents();
			Application.DoEvents();

			form.Dispose();
			return new WeakReference<ZTextBox>(textBox);
		}

		#endregion

		#region SkipSettingChildControlRealOnly

		public void TestSkipSettingChildControlRealOnly()
		{
			using (var parentControl = new ZUserControl())
			using (var childControl1 = new ZFilterStrip())
			using (var childControl2 = new ZLinkLabel())
			{
				parentControl.Controls.Add(childControl1);
				parentControl.Controls.Add(childControl2);

				parentControl.SkipSettingChildControlReadOnly = true;
				parentControl.SetReadOnlyIncludingChildren();

				AssertEquals("childControl1 not set ReadOnly", false, childControl1.GetReadOnly());
				AssertEquals("childControl2 not set ReadOnly", false, childControl2.GetReadOnly());

				parentControl.SkipSettingChildControlReadOnly = false;
				parentControl.SetReadOnlyIncludingChildren();
				AssertEquals("childControl1 set ReadOnly", true, childControl1.GetReadOnly());
				AssertEquals("childControl2 set ReadOnly", true, childControl2.GetReadOnly());
			}
		}
		#endregion

		#region UpdateEditableIncludingChildren

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
		public void TestUpdateEditableIncludingChildren()
		{
			using (var frm = new ZForm())
			using (var control = new ZUserControl())
			using (var button = new Button())
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var tabPage2 = new ZTabPage())
			using (var textBox = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			using (var checkBoxDontIgnoreMe = new ZCheckBox() { Name = "Don't IgnoreMe" })
			using (var checkBoxIgnoreMe = new ZCheckBox() { Name = "IgnoreMe" })
			using (var linkLabel = new ZLinkLabel())
			using (var toolStrip = new ToolStrip())
			using (var dropEdit = new ZCalcDropEdit())
			using (var toolStripButton = new ToolStripButton())
			using (var dataGrid = new ZGrid())
			{
				tabControl.TabPages.Add(tabPage);
				tabControl.TabPages.Add(tabPage2);
				tabPage2.ShouldBeReadOnlyInViewMode = false;
				tabPage.RunWhenBindingOrFirstShown((_, __) => tabPage.Controls.Add(textBox));
				tabPage2.RunWhenBindingOrFirstShown((_, __) => tabPage.Controls.Add(textBox2));
				control.Controls.Add(tabControl);
				control.Controls.Add(button);
				control.Controls.Add(checkBoxDontIgnoreMe);
				control.Controls.Add(checkBoxIgnoreMe);
				control.Controls.Add(linkLabel);
				control.Controls.Add(dropEdit);
				control.Controls.Add(dataGrid);
				toolStrip.Items.Add(toolStripButton);
				control.Controls.Add(toolStrip);

				frm.Controls.Add(control);
				frm.Show();
				Application.DoEvents();

				var ignorControlNames = new[] { checkBoxIgnoreMe.Name };
				control.UpdateEditableIncludingChildren(false, ignorControlNames);
				tabPage.NotifyBindingOrShowing();

				AssertEquals(false, checkBoxIgnoreMe.ReadOnly);
				AssertEditableIncludingChildren(control, false, ignorControlNames);

				var newTextBox = new ZTextBox();
				control.Controls.Add(newTextBox);

				AssertEquals("Newly added controls are also not editable.", true, newTextBox.ReadOnly);

				checkBoxIgnoreMe.ReadOnly = true;
				control.UpdateEditableIncludingChildren(true, ignorControlNames);

				AssertEquals(true, checkBoxIgnoreMe.ReadOnly);
				AssertEditableIncludingChildren(control, true, ignorControlNames);

				newTextBox = new ZTextBox();
				control.Controls.Add(newTextBox);

				AssertEquals("Newly added controls should not affected.", false, newTextBox.ReadOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestDoNotThrowCollectionWasModifiedWhenAddingNewExtenstion()
		{
			var dummy = Factory.New<DummyWithCodes>();
			dummy.CodeWithFormat = "INVALID";
			dummy.CodeWithFormatInfo.AddWarningWithoutValidationCheck("The code is invalid");

			using (var form = new ZForm(dummy))
			using (var control = new ZUserControl())
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var tabPage2 = new ZTabPage())
			using (var dropEdit = new ZDropEdit())
			{
				dropEdit.BindTo = "CodeWithFormat";

				tabControl.TabPages.Add(tabPage);
				tabControl.TabPages.Add(tabPage2);
				tabPage2.Controls.Add(dropEdit);
				control.Controls.Add(tabControl);
				form.Controls.Add(control);

				form.Show();
				Application.DoEvents();

				control.UpdateEditableIncludingChildren(true);
				tabControl.SelectedIndex = 1;
			}
		}

		public void TestUpdateEditableIncludingChildren_DoesntCauseMemoryLeaks()
		{
			var textboxReference = CreateAndDisposeFormForTestEditable();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert("There should be no remaining references to any controls", !textboxReference.TryGetTarget(out var ignored));
		}

		public void TestUpdateEditableIncludingChildren_ZGridReadOnlyShouldBeRestored()
		{
			using var form = new ZForm();
			using var userControl = new ZUserControl();
			using var grid = new ZGrid();
			using var readOnlyGrid = new ZGrid();

			userControl.Controls.Add(grid);
			userControl.Controls.Add(readOnlyGrid);
			form.Controls.Add(userControl);
			readOnlyGrid.ReadOnly = true;
			form.Show();

			AssertEquals("PRE-CONDITION ZGrid ReadOnly = false", expected: false, grid.ReadOnly);
			AssertEquals("PRE-CONDITION RO ZGrid ReadOnly = true", expected: true, readOnlyGrid.ReadOnly);

			form.UpdateEditableIncludingChildren(false);

			AssertEquals("When ZGrid not editable, ReadOnly = true", expected: true, grid.ReadOnly);
			AssertEquals("When RO ZGrid not editable, ReadOnly = true", expected: true, readOnlyGrid.ReadOnly);

			form.UpdateEditableIncludingChildren(true);

			AssertEquals("When ZGrid editable, ReadOnly = false", expected: false, grid.ReadOnly);
			AssertEquals("When RO ZGrid editable, ReadOnly = true", expected: true, readOnlyGrid.ReadOnly);
		}

		WeakReference<ZTextBox> CreateAndDisposeFormForTestEditable()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var form = new ZChildForm(dummy);
			var textBox = new ZTextBox { BindTo = DummyBizoSchema.Z0_Description.Name };
			form.Controls.Add(textBox);
			form.UpdateEditableIncludingChildren(false);

			form.Show();
			Application.DoEvents();
			Application.DoEvents();

			form.Dispose();
			return new WeakReference<ZTextBox>(textBox);
		}

		public static void AssertEditableIncludingChildren(Control control, bool isEditable, string[] ignoreControlNames = null)
		{
			AssertEditableIncludingChildren(control, control.Name, isEditable, ignoreControlNames);
		}

		static void AssertEditableIncludingChildren(Control control, string parentControlName, bool isEditable, string[] ignoreControlNames = null)
		{
			if (control == null || control.IsDisposed || (ignoreControlNames != null && ignoreControlNames.Any(c => c == control.Name)))
			{
				return;
			}

			control.Show();

			var toolStrip = control as ToolStrip;
			if (toolStrip != null)
			{
				foreach (ToolStripItem toolStripItem in toolStrip.Items)
				{
					AssertEquals($"The Enabled of {toolStripItem.Name} should be {isEditable}. (Parent: {parentControlName})", isEditable, toolStripItem.Enabled);
				}

				return;
			}

			var grid = control as ZGrid;
			if (grid != null)
			{
				AssertEquals($"The ReadOnly of {control.Name} should be {!isEditable}. (Parent: {parentControlName})", !isEditable, grid.ReadOnly);
				return;
			}

			var optionalControl = control as IReadOnlyAutomationOptional;
			if (optionalControl == null || optionalControl.ShouldSetReadOnlyWhenSettingIncludingChildren)
			{
				var readOnlyProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(control.GetType(), MetaDataTypes.ReadOnly);
				if (readOnlyProperty != null)
				{
					AssertEquals($"The ReadOnly of {control.Name} should be {!isEditable}. (Parent: {parentControlName})", !isEditable, control.GetReadOnly());
				}
				else
				{
					foreach (Control child in control.Controls)
					{
						AssertEditableIncludingChildren(child, parentControlName + "." + control.Name, isEditable, ignoreControlNames);
					}
				}
			}
		}

		#endregion

		#region SetDoubleBuffered

		public void TestSetDoubleBuffered()
		{
			using (var panel = new ZPanel())
			{
				var property = panel.GetType().GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertEquals(false, (bool)property.GetValue(panel, null));

				panel.SetDoubleBuffered(true);
				AssertEquals(true, (bool)property.GetValue(panel, null));

				panel.SetDoubleBuffered(false);
				AssertEquals(false, (bool)property.GetValue(panel, null));
			}
		}

		#endregion

		#region TemporarilyDrawAsBitmap

		public void TestTemporarilyDrawAsBitmap_WhenControlAlreadyDisposed()
		{
			var label = new Label();
			label.Dispose();
			AssertNoExceptionThrown(() => label.TemporarilyDrawAsBitmap().Dispose());
		}

		#endregion

		#region IsDisposedOrHasDisposedParent

		public void TestIsDisposedOrHasDisposedParent()
		{
			Assert("Should treat null as disposed", ZControlExtensions.IsDisposedOrHasDisposedParent(null));
			using (var control = new ZTextBox())
			{
				Assert("Control with null parent should not be treated as disposed (maybe just created and not yet placed)", !control.IsDisposedOrHasDisposedParent());
			}

			using (var form = new ZForm())
			{
				var panel1 = new ZPanel();
				form.Controls.Add(panel1);
				var control1 = new ZTextBox();
				panel1.Controls.Add(control1);

				var panel2 = new ZPanel();
				form.Controls.Add(panel2);
				var control2 = new ZTextBox();
				panel2.Controls.Add(control2);
				var control3 = new ZTextBox();
				panel2.Controls.Add(control3);

				Assert(!form.IsDisposedOrHasDisposedParent());
				Assert(!panel1.IsDisposedOrHasDisposedParent());
				Assert(!panel2.IsDisposedOrHasDisposedParent());
				Assert(!control1.IsDisposedOrHasDisposedParent());
				Assert(!control2.IsDisposedOrHasDisposedParent());
				Assert(!control3.IsDisposedOrHasDisposedParent());

				panel1.Dispose();
				Assert(!form.IsDisposedOrHasDisposedParent());
				Assert(panel1.IsDisposedOrHasDisposedParent());
				Assert(!panel2.IsDisposedOrHasDisposedParent());
				Assert(control1.IsDisposedOrHasDisposedParent());
				Assert(!control2.IsDisposedOrHasDisposedParent());
				Assert(!control3.IsDisposedOrHasDisposedParent());

				control2.Dispose();
				Assert(!form.IsDisposedOrHasDisposedParent());
				Assert(panel1.IsDisposedOrHasDisposedParent());
				Assert(!panel2.IsDisposedOrHasDisposedParent());
				Assert(control1.IsDisposedOrHasDisposedParent());
				Assert(control2.IsDisposedOrHasDisposedParent());
				Assert(!control3.IsDisposedOrHasDisposedParent());

				form.Dispose();
				Assert(form.IsDisposedOrHasDisposedParent());
				Assert(panel1.IsDisposedOrHasDisposedParent());
				Assert(panel2.IsDisposedOrHasDisposedParent());
				Assert(control1.IsDisposedOrHasDisposedParent());
				Assert(control2.IsDisposedOrHasDisposedParent());
				Assert(control3.IsDisposedOrHasDisposedParent());
			}
		}

		#endregion

		#region Control Finding

		public void TestFindSingle_WithName()
		{
			using (var button1 = new ZButton { Name = "First" })
			using (var button2 = new ZButton { Name = "Second" })
			using (var panel = new ZPanel { Name = "Second" })
			using (var form = new ZForm())
			{
				form.Controls.Add(button1);
				form.Controls.Add(button2);
				form.Controls.Add(panel);

				var result = form.FindSingle<ZButton>("Second");
				AssertEquals(button2, result);
			}
		}

		public void TestFindSingleOrDefault_WithName()
		{
			using (var button1 = new ZButton { Name = "First" })
			using (var button2 = new ZButton { Name = "Second" })
			using (var panel = new ZPanel { Name = "Second" })
			using (var form = new ZForm())
			{
				form.Controls.Add(button1);
				form.Controls.Add(button2);
				form.Controls.Add(panel);

				var result = form.FindSingleOrDefault<ZButton>("Second");
				AssertEquals(button2, result);

				result = form.FindSingleOrDefault<ZButton>("Third");
				AssertNull(result);
			}
		}

		public void TestFindSingle_WithFunc()
		{
			using (var button1 = new ZButton { Name = "First" })
			using (var button2 = new ZButton { Name = "Second" })
			using (var panel = new ZPanel { Name = "Second" })
			using (var form = new ZForm())
			{
				form.Controls.Add(button1);
				form.Controls.Add(button2);
				form.Controls.Add(panel);

				var result = form.FindSingle<ZButton>(x => x.Name.StartsWith("S"));
				AssertEquals(button2, result);
			}
		}

		public void TestFindSingleOrDefault_WithFunc()
		{
			using (var button1 = new ZButton { Name = "First" })
			using (var button2 = new ZButton { Name = "Second" })
			using (var panel = new ZPanel { Name = "Second" })
			using (var form = new ZForm())
			{
				form.Controls.Add(button1);
				form.Controls.Add(button2);
				form.Controls.Add(panel);

				var result = form.FindSingleOrDefault<ZButton>(x => x.Name.StartsWith("S"));
				AssertEquals(button2, result);

				result = form.FindSingleOrDefault<ZButton>(x => x.Name.StartsWith("T"));
				AssertNull(result);
			}
		}

		public void TestFindAll()
		{
			using (var button1 = new ZButton { Name = "First" })
			using (var button2 = new ZButton { Name = "Second" })
			using (var button3 = new ZButton { Name = "Seventh" })
			using (var panel = new ZPanel { Name = "Second" })
			using (var form = new ZForm())
			{
				form.Controls.Add(button1);
				form.Controls.Add(button2);
				form.Controls.Add(button3);
				form.Controls.Add(panel);

				var result = form.FindAll<ZButton>(x => x.Name.StartsWith("S"));
				AssertContainsExactElementsInAnyOrder(new[] { button2, button3 }, result);
			}
		}

		#endregion

		#region DoNoOverrideButtonEditableModeInViewOnly

		public void TestDoNoOverrideButtonEditableModeInViewOnly()
		{
			using (var button1 = new ZButton { Name = "First" })
			using (var button2 = new ZButton { Name = "Second" })
			using (var form = new ZForm())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				((DummyFilterGridModule)form.GetModule()).SetAllowEdit(false);
				button1.Enabled = false;
				button2.Enabled = false;
				button1.DoNoOverrideMyEditableMode = true;
				button2.DoNoOverrideMyEditableMode = true;
				form.Controls.Add(button1);
				form.Controls.Add(button2);
				form.SetReadOnlyIncludingChildren();

				Assert("Button1 disabled", !button1.Enabled);
				Assert("Button2 disabled", !button2.Enabled);

				form.Controls.Remove(button1);
				form.Controls.Remove(button2);
				button1.DoNoOverrideMyEditableMode = false;
				button2.DoNoOverrideMyEditableMode = false;
				form.Controls.Add(button1);
				form.Controls.Add(button2);
				form.SetReadOnlyIncludingChildren();

				Assert("Button1 enabled", button1.Enabled);
				Assert("Button2 enabled", button2.Enabled);
			}
		}

		#endregion

		#region Implementation

		class DummyIReadOnlyToggleControl : ZUserControl, IReadOnlyToggleControl
		{
			public DummyIReadOnlyToggleControl()
			{
				InitializeComponent();
			}

			public ZButton AButton;
			public ZTextBox ATextBox;

			void InitializeComponent()
			{
				AButton = new ZButton();
				ATextBox = new ZTextBox();
				this.Controls.Add(AButton);
				this.Controls.Add(ATextBox);
			}

			#region IReadOnlyToggleControl Members

			public bool ReadOnly
			{
				get { return readOnly; }
				set
				{
					readOnly = value;
					AButton.ReadOnly = value;
				}
			}
			bool readOnly;

			#endregion
		}

		#endregion
	}
}
