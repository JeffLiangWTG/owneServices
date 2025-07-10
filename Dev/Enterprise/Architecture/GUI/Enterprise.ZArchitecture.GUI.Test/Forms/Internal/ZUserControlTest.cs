using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Internal
{
	class ZUserControlVisibleForNotificationsTestCase : SetVisibleOnControlUpdatesNotificationsTestCase
	{
		protected override Control GetNewControl()
		{
			return new ZUserControl();
		}
	}

	class ZUserControlTest : ZControlBaseTestCase<ZUserControl>
	{
		#region TestRegisterControlToBeBoundOnPreSaveValidation

		public void TestRegisterControlToBeBoundOnPreSaveValidation()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				var control1BindingWasHit = false;
				var control2BindingWasHit = false;

				var control1 = new TestUserControl();
				control1.ShouldRegisterToBeBoundOnPreSaveValidationForTesting = true;
				control1.AfterFirstBinding += (sender, e) => control1BindingWasHit = true;
				var control2 = new TestUserControl();
				control2.AfterFirstBinding += (sender, e) => control2BindingWasHit = true;
				var tabControl = new ZTabControl();
				tabControl.TabPages.Add(new ZTabPage { Name = "Test1" });
				tabControl.TabPages.Add(new ZTabPage { Name = "Test2" });
				tabControl.TabPages.Add(new ZTabPage { Name = "Test3" });
				var tabPage1 = tabControl.GetTabPage("Test2");
				tabPage1.Controls.Add(control1);
				var tabPage2 = tabControl.GetTabPage("Test3");
				tabPage2.Controls.Add(control2);
				form.Controls.Add(tabControl);

				form.Show();
				AssertEquals("Precondition:", false, control1BindingWasHit);
				AssertEquals("Precondition:", false, control2BindingWasHit);

				Db.Connection.BeginTransaction();
				try
				{
					form.FireSaveButton();
					AssertEquals(true, control1BindingWasHit);
					AssertEquals(false, control2BindingWasHit);
				}
				finally
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}

		#endregion

		#region Tab Skipping ReadOnly Fields

		public void TestProcessTabKey()
		{
			Form.Show();
			Form.Controls.Add(UserControl);

			var userControl2 = new TestUserControl(DockStyle.Fill, new Point(0, 0));
			var userControl3 = new TestUserControl(DockStyle.Fill);
			userControl3.Controls.Add(TextBox1);
			userControl3.Controls.Add(TextBox2);
			TextBox2.Location = new Point(100, 0);
			userControl2.Controls.Add(userControl3);
			UserControl.Controls.Add(userControl2);

			var userControl4 = new TestUserControl(DockStyle.Fill, new Point(200, 0));
			userControl4.Controls.Add(TextBox3);
			UserControl.Controls.Add(userControl4);
			Application.DoEvents();

			TextBox1.TabIndex = 1;
			TextBox2.TabIndex = 2;
			TextBox3.TabIndex = 3;

			TextBox1.Focus();
			TextBox2.ReadOnly = true;
			UserControl.ProcessTabKey(true);
			AssertEquals("TextBox2 is read only, thus should be skipped (forwards)", true, TextBox3.Focused);

			UserControl.ProcessTabKey(false);
			AssertEquals("TextBox2 is read only, thus should be skipped (backwards)", true, TextBox1.Focused);
			AssertEquals(true, TextBox1.Focused);
		}

		public void TestValidationTimesWhenProcessTabKey()
		{
			Form.SetDataBinding(Dummy, "");

			TextBox1.BindTo = DummyBizoSchema.Constants.Z0_Code;
			TextBox1.TabIndex = 1;
			TextBox2.TabIndex = 2;
			UserControl.Controls.Add(TextBox1);
			UserControl.Controls.Add(TextBox2);
			Form.Controls.Add(UserControl);
			Form.Show();
			Application.DoEvents();

			TextBox1.Focus();
			TextBox1.Text = "text";
			UserControl.ProcessTabKey(true);
			AssertEquals(2, Dummy.ValidationTimesForZ0_Code);
		}

		public void TestValidationTimesWhenNotEnteredAndProcessTabKey()
		{
			Form.SetDataBinding(Dummy, "");

			TextBox1.BindTo = DummyBizoSchema.Constants.Z0_Code;
			TextBox1.TabIndex = 1;
			TextBox2.TabIndex = 2;
			UserControl.Controls.Add(TextBox1);
			UserControl.Controls.Add(TextBox2);
			Form.Controls.Add(UserControl);
			Form.Show();
			Application.DoEvents();

			TextBox1.Focus();
			UserControl.ProcessTabKey(true);
			AssertEquals(1, Dummy.ValidationTimesForZ0_Code);
		}

		public void TestValidationTimesForContainControlWhenNotEnteredAndProcessTabKey()
		{
			var dummy1 = Factory.New<DummyWithCodes>();
			dummy1.Z0_Description = "AAADescription";

			var zCodeFindBox = new ZCodeFindBox();
			zCodeFindBox.BindTo = "Z0_Code";
			zCodeFindBox.SetDataBinding(dummy1, "Z0_Code");

			zCodeFindBox.TabIndex = 1;
			TextBox1.TabIndex = 2;
			UserControl.Controls.Add(zCodeFindBox);
			UserControl.Controls.Add(TextBox1);
			Form.Controls.Add(UserControl);
			Form.Show();
			Application.DoEvents();

			zCodeFindBox.CodeBox.Focus();
			UserControl.ProcessTabKey(true);
			AssertEquals(1, dummy1.ValidationTimesForZ0_Code);
		}

		public void TestSelectNextControlCustomHandling()
		{
			Form.Show();
			Form.Controls.Add(UserControl);

			TextBox1.Text = "Text Box 1";
			TextBox2.Text = "Text Box 2";
			TextBox3.Text = "Text Box 3";

			UserControl.Controls.Add(TextBox1);
			UserControl.Controls.Add(TextBox2);
			UserControl.Controls.Add(TextBox3);

			TextBox1.TabIndex = 1;
			TextBox2.TabIndex = 2;
			TextBox3.TabIndex = 3;

			UserControl.OnTabSelectNextControl += (s, e) =>
			{
				if (UserControl.ActiveControl == TextBox1)
				{
					TextBox3.Select();
					e.Handled = true;
				}
			};

			TextBox1.Focus();

			UserControl.ProcessTabKey(true);

			AssertEquals("Custom handling of selecting next control on tab", UserControl.ActiveControl, TextBox3);
		}

		public void TestControlIsEditableInViewMode()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZChildForm(dummy))
			using (var control = new ZDateEdit())
			{
				control.EditableInViewMode = true;
				form.Controls.Add(control);

				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(false, control.ReadOnly);
			}
		}

		#endregion

		#region OnParentChanged calls OnBackColorChanged

		public void TestOnParentChangedCallsOnBackColorChanged()
		{
			using (var parent = new ZUserControl())
			using (var child = new ZUserControl())
			{
				var counterOnBackColorChanged = 0;
				child.BackColorChanged += (sender, e) => counterOnBackColorChanged++;

				parent.BackColor = Color.Yellow;
				parent.Controls.Add(child);

				parent.SetDataBinding(new BusinessObjectFactory().New<DummyBusinessObject>(), "");

				AssertEquals("Called from Control.AssignParent() and in ZUserControl.SetDataBinding()", 2, counterOnBackColorChanged);
			}

			using (var parent = new ZUserControl())
			using (var child = new ZUserControl())
			{
				var counterOnBackColorChanged = 0;
				child.BackColorChanged += (sender, e) => counterOnBackColorChanged++;

				parent.Controls.Add(child);

				parent.SetDataBinding(new BusinessObjectFactory().New<DummyBusinessObject>(), "");

				AssertEquals("Called from ZUserControl.SetDataBinding()", 1, counterOnBackColorChanged);
			}
		}

		#endregion

		#region ReportBindingException

		public void TestReportBindingException()
		{
			using (var form = new ZForm(Dummy))
			using (var userControl = new UserControlWithBindingException { Name = "UserControl1" })
			{
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();

				AssertEquals(2, userControl.Controls.Count);
				AssertEquals("Link label should be added.", 1, userControl.Controls.OfType<ZLinkLabel>().Count());

				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(
					"Exception during (re)binding control 'UserControl1' (Enterprise.ZArchitecture.GUI.Test.Forms.Internal.ZUserControlTest+UserControlWithBindingException) to data source 'DummyBizo' (CargoWise.EntityFramework.Testing.DummyBusinessObject), data member ''.",
					ExceptionReporterTestListener.Instance[0].Message);

				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		class UserControlWithBindingException : ZUserControl
		{
			public UserControlWithBindingException()
			{
				Controls.Add(new ControlWithBindingException());
			}

			protected override void OnBindingContextChanged(EventArgs e)
			{
				base.OnBindingContextChanged(new ThrowExceptionEventArgs());
			}
		}

		class ControlWithBindingException : ZPanel
		{
			protected override void OnBindingContextChanged(EventArgs e)
			{
				if (e is ThrowExceptionEventArgs)
				{
					throw new ArgumentException("Test");
				}
				base.OnBindingContextChanged(e);
			}
		}

		class ThrowExceptionEventArgs : EventArgs { }

		#endregion

		#region Top Level Data Source Type

		public void TestTopLevelDataSourceType()
		{
			using (var testForm = new TestForm())
			{
				DesignModeFinder.SetIsDesigningForTest(true);
				AssertEquals(typeof(DummyBaseBusinessObject), ((ITopLevelDataSourceType)testForm.TestControl).DataSourceType);
				DesignModeFinder.SetIsDesigningForTest(false);
				AssertEquals(typeof(DummyBaseBusinessObject), ((ITopLevelDataSourceType)testForm.TestControl).DataSourceType);
			}

			using (var testForm = new TestForm(Factory.New<DummyBusinessObjectWithList>()))
			{
				DesignModeFinder.SetIsDesigningForTest(true);
				AssertEquals(typeof(DummyBaseBusinessObject), ((ITopLevelDataSourceType)testForm.TestControl).DataSourceType);

				DesignModeFinder.SetIsDesigningForTest(false);
				testForm.Show();
				AssertEquals(typeof(DummyBusinessObjectWithList), ((ITopLevelDataSourceType)testForm.TestControl).DataSourceType);
			}

			var dummy = Factory.New<DummyWithRelatedAsDummyBusinessObjectWithList>();
			var related = Factory.New<DummyBusinessObjectWithList>();
			dummy.Z0_Guid = related.PK;
			using (var testForm = new TestForm(dummy))
			{
				testForm.BindControlToRelated();
				testForm.Show();
				AssertEquals(typeof(DummyBusinessObjectWithList), ((ITopLevelDataSourceType)testForm.TestControl).DataSourceType);
			}
		}

		class TestControl : ZUserControl
		{
			public TestControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				BindingSource.DataSourceType = typeof(DummyBaseBusinessObject);
			}
		}

		class TestForm : ZForm
		{
			public TestForm()
			{
				InitializeComponent();
			}

			public TestForm(DummyBaseBusinessObject bizo)
				: base(bizo)
			{
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				TestControl = new TestControl();
				this.Controls.Add(TestControl);
			}

			public void BindControlToRelated()
			{
				this.BindingSource.SetBindingMember(this.TestControl, "RelatedDummy");
			}
			public TestControl TestControl;
		}

		class DummyWithRelatedAsDummyBusinessObjectWithList : DummyBaseBusinessObject
		{
			public DummyWithRelatedAsDummyBusinessObjectWithList(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{ }

			public override DummyBusinessObject RelatedDummy
			{
				get { return Factory.Load<DummyBusinessObjectWithList>(Z0_Guid); }
			}
		}

		#endregion

		#region Test Classes

		class TestUserControl : ZUserControl
		{
			public TestUserControl()
			{
			}

			public TestUserControl(DockStyle dock)
			{
				this.Dock = dock;
			}

			public TestUserControl(DockStyle dock, Point location)
			{
				this.Dock = dock;
				this.Location = location;
			}

			public new bool ProcessTabKey(bool forward)
			{
				return base.ProcessTabKey(forward);
			}

			protected override bool ShouldRegisterToBeBoundOnPreSaveValidation
			{
				get { return ShouldRegisterToBeBoundOnPreSaveValidationForTesting; }
			}

			public bool ShouldRegisterToBeBoundOnPreSaveValidationForTesting { private get; set; }
		}

		#endregion

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZUserControl.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZUserControl)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZUserControl).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		#region Implementation

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		ZTextBox TextBox1
		{
			get { return textBox1 ?? (textBox1 = new ZTextBox()); }
		}
		ZTextBox textBox1;

		ZTextBox TextBox2
		{
			get { return textBox2 ?? (textBox2 = new ZTextBox()); }
		}
		ZTextBox textBox2;

		ZTextBox TextBox3
		{
			get { return textBox3 ?? (textBox3 = new ZTextBox()); }
		}
		ZTextBox textBox3;

		ZForm Form
		{
			get { return form ?? (form = new ZForm()); }
		}
		ZForm form;

		TestUserControl UserControl
		{
			get { return userControl ?? (userControl = new TestUserControl()); }
		}
		TestUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (userControl != null)
			{
				userControl.Dispose();
			}
		}

		protected override bool IsDragDropHandledByEDocs
		{
			get { return true; }
		}

		protected override bool RequiresTypeDescriptor
		{
			get { return false; }
		}

		#endregion
	}
}
