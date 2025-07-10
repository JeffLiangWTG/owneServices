using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCheckBoxTest : ZControlBaseTestCase<ZCheckBox>
	{
		public void TestBind()
		{
			CheckBox.BindTo = DummyBizoSchema.Constants.Z0_Bool;
			Form.Controls.Add(CheckBox);
			Form.SetDataBinding(Dummy, "");
			Form.Show();

			Dummy.Z0_Bool = ZBool.True;
			Assert(CheckBox.Checked);

			Dummy.Z0_Bool = ZBool.False;
			Assert(!CheckBox.Checked);

			Dummy.Z0_Bool = ZBool.True;
			Assert(CheckBox.Checked);
		}

		public void TestColors()
		{
			using (var theForm = new ZForm())
			{
				var testControl2 = new ZCheckBox();
				CheckBox.Parent = theForm;
				testControl2.Parent = theForm;
				theForm.Show();

				CheckBox.Focus();
				AssertEquals("Must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, CheckBox.BackColor);
				AssertEquals("Must be colored in standard color", theForm.BackColor, testControl2.BackColor);

				testControl2.Focus();
				AssertEquals("Must be colored in standard color", theForm.BackColor, CheckBox.BackColor);
				AssertEquals("Must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, testControl2.BackColor);

				var originalForeColor = CheckBox.ForeColor;

				CheckBox.ReadOnly = true;
				AssertEquals("Back color must not be grey when read only", theForm.BackColor, CheckBox.BackColor);
				AssertEquals("ForeColor should be gray when read only", SystemColors.GrayText, CheckBox.ForeColor);

				CheckBox.ReadOnly = false;
				AssertEquals("Original forecolor should be restored", originalForeColor, CheckBox.ForeColor);
			}
		}

		public void TestPaintCheckBox()
		{
			using (var theForm = new ZForm())
			{
				var testCheckEdit = new ZCheckBox();
				var testCheckEdit2 = new ZCheckBox { Location = new Point(20, 20), ReadOnly = true };

				theForm.Controls.Add(testCheckEdit);
				theForm.Controls.Add(testCheckEdit2);
				testCheckEdit.CheckedChanged += TestCheckEdit_CheckedChanged;
				theForm.Show();
				testCheckEdit.Checked = true;

				AssertEquals("CheckedValue should of been called", true, CheckedValueChangedFired);
			}
		}

#if !WINZOR

		public void TestCatchZCheckBoxAlreadyDisposedInWndProc()
		{
			using (var localForm = new ZForm())
			{
				var testCheckBox = new DisposedZCheckBoxTest { Name = "testCheckBox", Location = new Point(20, 20), TrackDisposedAccess = true };

				localForm.Controls.Add(testCheckBox);
				localForm.Show();
				testCheckBox.Checked = true;

				MouseSender.SendMessage(testCheckBox, testCheckBox.Handle, WindowsMessage.WM_LBUTTONDOWN, 0, 0);

				AssertNull(ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();
			}
		}

#endif

		class TestValidationExtension : ValidationExtension
		{
			public bool ValidatePropertyWasCalled;

			protected override void ValidateProperty(BusinessObject obj, string property)
			{
				base.ValidateProperty(obj, property);
				ValidatePropertyWasCalled = true;
			}
		}

		public void TestValidateShouldNotBeTriggeredWhenTheCheckedValueChanged()
		{
			using (var zForm = new ZForm())
			{
				using (var zCheckBox = new ZCheckBox())
				{
					var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
					zCheckBox.BindTo = DummyBizoSchema.Constants.Z0_Bool;
					zForm.Controls.Add(zCheckBox);
					zForm.SetDataBinding(dummy, "");
					zForm.Show();

					var extension = new TestValidationExtension();
					extension.Initialize(zCheckBox);
					AssertEquals(false, extension.ValidatePropertyWasCalled);

					zCheckBox.Checked = true;
					AssertEquals(false, extension.ValidatePropertyWasCalled);
				}
			}
		}

		public void TestShowFocusCues()
		{
			using (var testForm = new ZForm())
			{
				var testCheckEdit = new ZCheckBox();

				testForm.Controls.Add(testCheckEdit);
				testCheckEdit.Focus();
				testForm.Show();

				// the focus cue should show without having to hit the Tab key
				Assert("No Focus cue was shown", testCheckEdit.ShowFocusCues_Exposed);
			}
		}

		public void TestControlIsEditableInViewMode()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZChildForm(dummy))
			using (var control = new ZCheckBox())
			{
				control.EditableInViewMode = true;
				form.Controls.Add(control);

				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(false, control.ReadOnly);
			}
		}

		bool CheckedValueChangedFired;
		void TestCheckEdit_CheckedChanged(object sender, EventArgs e)
		{
			CheckedValueChangedFired = true;
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZCheckBox.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZCheckBox)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZCheckBox).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		#region Implementation

		ZForm Form
		{
			get { return form ?? (form = new ZForm()); }
		}
		ZForm form;

		ZCheckBox CheckBox
		{
			get { return checkBox ?? (checkBox = new ZCheckBox()); }
		}
		ZCheckBox checkBox;

		protected override void TearDown()
		{
			base.TearDown();
			DesignModeFinder.SetIsDesigningForTest(false);
			if (checkBox != null)
			{
				checkBox.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		protected override string[] BindablePropertyNames
		{
			get { return new[] { "Checked", "ReadOnly", "Text" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

#if !WINZOR

		class DisposedZCheckBoxTest : ZCheckBox
		{
			protected override void WndProc(ref Message m)
			{
				switch (m.Msg)
				{
					case WindowsMessage.WM_LBUTTONDOWN:
						this.Dispose();
						base.WndProc(ref m);
						return;
				}

				base.WndProc(ref m);
			}
		}

#endif

		#endregion
	}
}
