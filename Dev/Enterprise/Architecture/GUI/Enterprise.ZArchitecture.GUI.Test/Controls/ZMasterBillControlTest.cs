using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZMasterBillControlTest : ZControlBaseTestCase<ZMasterBillControl>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "FormattedMasterBill", "ReadOnly" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Text"; }
		}

		public void TestIsVisibleForBindingAddedIntoPropertyDescriptors()
		{
			var descriptors = ZMasterBillControl.GetPropertyDescriptors().Select(x => x.Name);
			AssertCollectionContains("[IsVisibleForBinding] should be in ZMasterBillControl's property descriptors", "IsVisibleForBinding", descriptors);
		}

		public void TestAllowAlphaInMAWP()
		{
			using (var masterBillControl = new ZMasterBillControl())
			{
				var textBox1 = FindFirstChildRecursively<OMasterBill1TextBox>(masterBillControl);
				KeySender.PostKeyDown(textBox1, Keys.X);
				Application.DoEvents();
				AssertEquals("", textBox1.Text);
				masterBillControl.AllowAlphaInMAWP = true;
				KeySender.PostKeyDown(textBox1, Keys.X);
				Application.DoEvents();
				AssertEquals("X", textBox1.Text);
			}
		}

		[DeveloperOnlyTest]
		public void TestEditing()
		{
			using (var masterBillControl = new ZMasterBillControl())
			{
				masterBillControl.MasterBillText = "12345678901";

				var textBox1 = FindFirstChildRecursively<OMasterBill1TextBox>(masterBillControl);
				var textBox2 = FindFirstChildRecursively<OMasterBill2TextBox>(masterBillControl);

				AssertEquals("123", textBox1.Text);
				AssertEquals(false, textBox1.TabStop);
				AssertEquals(0, textBox1.TabIndex);
				AssertEquals("45678901", textBox2.Text);
				AssertEquals(true, textBox2.TabStop);
				AssertEquals(1, textBox2.TabIndex);

				KeySender.PostKeyDown(textBox1, Keys.Delete);
				Application.DoEvents();
				AssertEquals("234", textBox1.Text);
				AssertEquals("5678901", textBox2.Text);

				textBox1.Text = ""; // ensure key handler copes when text is changed underneath it
				KeySender.PostKeyDown(textBox1, Keys.Delete);
				Application.DoEvents();
				AssertEquals("567", textBox1.Text);
				AssertEquals("8901", textBox2.Text);

				textBox1.SelectionStart = 2;
				textBox1.SelectionLength = 1;
				UnsafeNativeMethods.SendMessage(new HandleRef(textBox1, textBox1.Handle), WindowsMessage.WM_DELETE, 0, 0);
				AssertEquals("568", textBox1.Text);
				AssertEquals("901", textBox2.Text);

				textBox1.SelectionStart = 0;
				textBox1.SelectionLength = 2;

				var action = new Action(() => UnsafeNativeMethods.SendMessage(new HandleRef(textBox1, textBox1.Handle), WindowsMessage.WM_CUT, 0, 0));
				action.Invoke();
				ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);

				AssertEquals("890", textBox1.Text);
				AssertEquals("1", textBox2.Text);

				UnsafeNativeMethods.SendMessage(new HandleRef(textBox1, textBox1.Handle), WindowsMessage.WM_PASTE, 0, 0);
				AssertEquals("568", textBox1.Text);
				AssertEquals("901", textBox2.Text);

				AssertEquals("56", SafeClipboard.GetDataObject().GetData(typeof(string)));

				masterBillControl.MasterBillText = "12345";
				textBox1.SelectionStart = 0;
				textBox1.SelectionLength = 3;
				UnsafeNativeMethods.SendMessage(new HandleRef(textBox1, textBox1.Handle), WindowsMessage.WM_DELETE, 0, 0);
				AssertEquals("45", textBox1.Text);
				AssertEquals("", textBox2.Text);
			}
		}

		public void TestBackspaceWithEmptyContent()
		{
			using (var masterBillControl = new ZMasterBillControl())
			{
				masterBillControl.MasterBillText = "";
				var textBox = FindFirstChildRecursively<OMasterBill2TextBox>(masterBillControl);

				KeySender.PostKeyDown(textBox, Keys.Back);
				Application.DoEvents();

				AssertEquals("", textBox.Text);
				AssertEquals(true, textBox.IsCaretAtStart());
			}
		}

		T FindFirstChildRecursively<T>(Control control) where T : Control
		{
			T result = null;
			foreach (Control childControl in control.Controls)
			{
				result = (childControl is T) ? (T)childControl : FindFirstChildRecursively<T>(childControl);
				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		public void TestBinding()
		{
			using (var form = new ZForm(Dummy))
			{
				var masterBillControl = new ZMasterBillControl();
				masterBillControl.BindTo = DummyBizoSchema.Constants.Z0_VarCharMax;

				var textBox = new ZTextBox();
				textBox.Top = masterBillControl.Bottom + 10;

				form.Controls.Add(masterBillControl);
				form.Controls.Add(textBox);
				form.Show();

				masterBillControl.FormattedMasterBill = "";
				Dummy.Z0_VarCharMax = "12345789012";

				AssertEquals("123-4578 9012", masterBillControl.FormattedMasterBill);

				masterBillControl.Focus();
				masterBillControl.FormattedMasterBill = "";
				textBox.Focus();

				AssertEquals("", Dummy.Z0_VarCharMax);

				masterBillControl.Focus();
				masterBillControl.FormattedMasterBill = "123-4578 9013";
				textBox.Focus();

				AssertEquals("123-4578 9013", Dummy.Z0_VarCharMax);
			}
		}

		[DeveloperOnlyTest]
		public void TestTabPressedOnOMasterBill2TextBox_EndCurrentEditMustBeTriggeredOnMasterBillControl()
		{
			using (var form = new ZForm(Dummy))
			{
				var testControl = new ZUserControlForTest();
				testControl.Dock = DockStyle.Fill;

				var masterBillControl = new ZMasterBillControl();
				var oMasterBill1TextBox = FindFirstChildRecursively<OMasterBill1TextBox>(masterBillControl);
				var oMasterBill2TextBox = FindFirstChildRecursively<OMasterBill2TextBox>(masterBillControl);
				masterBillControl.TransportType = OTransportType.Air;
				masterBillControl.BindTo = DummyBizoSchema.Constants.Z0_VarCharMax;
				masterBillControl.TabIndex = 1;

				var textBoxInvisible = new ZTextBox();
				textBoxInvisible.BindTo = DummyBizoSchema.Constants.Z0_Number;
				textBoxInvisible.Top = masterBillControl.Bottom + 10;
				textBoxInvisible.Visible = false;
				textBoxInvisible.TabIndex = 2;

				var textBox = new ZTextBox();
				textBox.BindTo = DummyBizoSchema.Constants.Z0_NVarChar;
				textBox.Top = textBoxInvisible.Bottom + 10;
				textBox.TabIndex = 3;

				var textBox2 = new ZTextBox();
				textBox2.BindTo = DummyBizoSchema.Constants.Z0_Description;
				textBox2.Top = textBox.Bottom + 10;
				textBox2.TabIndex = 4;

				form.Controls.Add(testControl);
				testControl.Controls.Add(masterBillControl);
				testControl.Controls.Add(textBoxInvisible);
				testControl.Controls.Add(textBox);
				testControl.Controls.Add(textBox2);
				form.Show();

				KeySender.PostKeyDown(oMasterBill1TextBox, Keys.D1);
				KeySender.PostKeyDown(oMasterBill1TextBox, Keys.D2);
				KeySender.PostKeyDown(oMasterBill1TextBox, Keys.D3);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D4);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D5);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D6);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D7);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D8);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D9);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D0);
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.D1);
				oMasterBill2TextBox.Select();
				KeySender.PostKeyDown(oMasterBill2TextBox, Keys.Tab);
				Application.DoEvents();
				KeySender.PostKeyDown(textBox, Keys.D1);
				KeySender.PostKeyDown(textBox, Keys.D2);
				KeySender.PostKeyDown(textBox, Keys.D3);
				KeySender.PostKeyDown(textBox, Keys.Tab);
				Application.DoEvents();
				Dummy.RefreshBinding();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("Dummy.Z0_VarCharMax", "123-4567 8901", Dummy.Z0_VarCharMax);
					AssertEquals("masterBillControl.FormattedMasterBill", "123-4567 8901", masterBillControl.FormattedMasterBill);
				});
			}
		}

		[DeveloperOnlyTest]
		public void TestOMasterBill2TextBox_CopyCutPaste()
		{
			using (var form = new ZForm(Dummy))
			{
				var testControl = new ZUserControlForTest();
				testControl.Dock = DockStyle.Fill;

				var masterBillControl = new ZMasterBillControl();
				var oMasterBill1TextBox = FindFirstChildRecursively<OMasterBill1TextBox>(masterBillControl);
				var oMasterBill2TextBox = FindFirstChildRecursively<OMasterBill2TextBox>(masterBillControl);
				masterBillControl.TransportType = OTransportType.Air;
				masterBillControl.BindTo = DummyBizoSchema.Constants.Z0_VarCharMax;
				masterBillControl.TabIndex = 1;

				form.Controls.Add(testControl);
				testControl.Controls.Add(masterBillControl);
				form.Show();

				CombineAssertions(() =>
				{
					SafeClipboard.Clear();
					oMasterBill1TextBox.Text = "123";
					oMasterBill2TextBox.Text = "4567890";
					oMasterBill2TextBox.Focus();
					oMasterBill2TextBox.SelectAll();

					var action = new Action(() =>
					{
						KeySender.PostKeyDown(oMasterBill2TextBox, Keys.C | Keys.Control);
						Application.DoEvents();
					});
					action.Invoke();
					var text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
					AssertEquals("copy", "4567890", text);

					SafeClipboard.Clear();
					oMasterBill2TextBox.Text = "45678901";
					oMasterBill2TextBox.Focus();
					oMasterBill2TextBox.SelectAll();

					action = () =>
					{
						KeySender.PostKeyDown(oMasterBill2TextBox, Keys.X | Keys.Control);
						Application.DoEvents();
					};
					action.Invoke();

					text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
					AssertEquals("cut", "45678901", text);
					AssertEquals("cut", ZString.Empty, oMasterBill2TextBox.Text);

					oMasterBill2TextBox.Select();
					oMasterBill2TextBox.Focus();
					KeySender.PostKeyDown(oMasterBill2TextBox, Keys.P | Keys.Control);
					Application.DoEvents();
					Application.DoEvents();
					AssertEquals("paste", "45678901", oMasterBill2TextBox.Text);
				});
			}
		}

#if !WINZOR

		public void TestoMasterBill2TextBoxPaintExtensionSkip()
		{
			using (var form = new ZForm(Dummy))
			{
				var testControl = new ZUserControlForTest();
				testControl.Dock = DockStyle.Fill;

				var masterBillControl = new ZMasterBillControl();
				var oMasterBill1TextBox = FindFirstChildRecursively<OMasterBill1TextBox>(masterBillControl);
				var oMasterBill2TextBox = FindFirstChildRecursively<OMasterBill2TextBox>(masterBillControl);
				masterBillControl.TransportType = OTransportType.Air;
				masterBillControl.BindTo = DummyBizoSchema.Constants.Z0_VarCharMax;
				masterBillControl.TabIndex = 1;

				form.Controls.Add(testControl);
				testControl.Controls.Add(masterBillControl);
				form.Show();

				SafeClipboard.Clear();
				oMasterBill1TextBox.Text = "123";
				oMasterBill2TextBox.Focus();
				AssertEquals(true, oMasterBill2TextBox.PaintExtension.Skip);

				oMasterBill1TextBox.Focus();
				oMasterBill1TextBox.Text = "";
				oMasterBill2TextBox.Focus();
				AssertEquals(false, oMasterBill2TextBox.PaintExtension.Skip);
			}
		}

#endif

		protected override bool UsesControlDataBindings => false;
	}
}
