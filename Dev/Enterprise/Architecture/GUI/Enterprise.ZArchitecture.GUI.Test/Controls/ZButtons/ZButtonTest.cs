using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZButtonTest : ZControlBaseTestCase<ZButton>
	{
		public void TestIButton_ShouldSetImage()
		{
			using (var button = new ZButton())
			{
				AssertEquals("Should not render image when ZButton is used as a posting control", false, ((IButton)button).ShouldSetImage);
			}
		}

		public void TestDefaults()
		{
			using (var myButton = new ZButton())
			{
				AssertEquals("Should be FlatStyle.Standard. FlatStyle.System is not needed in .NET 2.0 due to Application.EnableVisualStyles().", FlatStyle.Standard, myButton.FlatStyle);
			}
		}

		public void TestControlIsEditableInViewMode()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZChildForm(dummy))
			using (var control = new ZButton())
			{
				control.EditableInViewMode = true;
				form.Controls.Add(control);

				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(false, control.ReadOnly);
				// Will currently always pass as buttons are enabled on View Forms already when they probably shouldn't be
			}
		}

#if !WINZOR
		// Equivalent test for Winzor is in Enterprise.Winzor.Architecture.Test\ZButtonTest.cs 
		public void TestButtonClickReEntrancy()
		{
			using (var button = new ZButton())
			{
				var i = 0;
				KForm testForm = null;

				button.Click += delegate
				{
					// if re-entrant, the second call to this method will cause an ObjectDisposedException when trying to show.

					i++;
					testForm.Show();
					testForm.Dispose();
					Application.DoEvents(); // this causes the second message to be processed while the first is still executing.
				};

				using (testForm = new KForm())
				{
					// {msg=0x2111 (WM_REFLECT + WM_COMMAND) hwnd=0xcb0724 wparam=0x724 lparam=0xcb0724 result=0x0} from a break
					UnsafeNativeMethods.PostMessage(new HandleRef(button, button.Handle), 0x2111, new IntPtr(0x724), new IntPtr(0xCB0724));
					UnsafeNativeMethods.PostMessage(new HandleRef(button, button.Handle), 0x2111, new IntPtr(0x724), new IntPtr(0xCB0724));
					Application.DoEvents();
				}

				AssertEquals("Click handler invoke count", 1, i);
			}
		}
#endif
		public void TestDispose()
		{
			var button = new ZButton();
			button.ImageList = Icons.ImageList;
			button.Dispose();
			AssertNull("Should be cleared", button.ImageList);
		}

		public void TestGetsGCed()
		{
			var buttonRef = GetWeakReferenceToZButton();
			GC.Collect();
			Assert("Won't be collected as reference to static image list", buttonRef.IsAlive);

			DisposeWeaklyReferencedZButton(buttonRef);
			GC.Collect();
			Assert("Should be collected as image list has been disconnected", !buttonRef.IsAlive);
		}

		WeakReference GetWeakReferenceToZButton()
		{
			var button = new ZButton();
			button.ImageList = Icons.ImageList;

			var buttonRef = new WeakReference(button);
			button = null;

			return buttonRef;
		}

		void DisposeWeaklyReferencedZButton(WeakReference weakReference)
		{
			((ZButton)weakReference.Target).Dispose();
		}

		public void TestFocusButtonOnClickWithHotKey()
		{
			using (var form = new ZForm(Dummy))
			{
				Dummy.Collection.AddNew();

				var grid = new ZGrid();
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Z0_Decimal.Name, 50, 0));
				form.Controls.Add(grid);
				grid.SetDataBinding(Dummy, "Collection");

				var testButton = new ZButton();
				testButton.Text = "&Yes";
				form.Controls.Add(testButton);

				form.DataSourceType = typeof(DummyBusinessObject);
				form.SetDataBinding(Dummy, "");

				form.Show();
				Application.DoEvents();
				grid.LastFocusedColumn.EditControl.Focus();

				AssertEquals("Precondition: Z0_Decimal is zero.", 0M, Dummy.Collection[0].Z0_Decimal);
				AssertEquals("Precondition: CalcEdit Text is zero character.", "0", grid.LastFocusedColumn.EditControl.Text);
				AssertEquals("Precondition: control of grid cell is focused.", true, grid.LastFocusedColumn.EditControl.Focused);
				AssertEquals("Precondition: button is not focused.", false, testButton.Focused);

				KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D1);
				KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D0);
				Application.DoEvents();
				AssertEquals("Z0_Decimal is still zero, because focus was not changed.", 0M, Dummy.Collection[0].Z0_Decimal);
				AssertEquals("CalcEdit Text has new value.", "10", grid.LastFocusedColumn.EditControl.Text);

				KeySender.PostKeyDown(form, Keys.Alt | Keys.Y);
				Application.DoEvents();
				AssertEquals("Z0_Decimal has new value, because focus must be changed now.", 10M, Dummy.Collection[0].Z0_Decimal);
				AssertEquals("Postcondition: Button must be focused after using its hotkey.", true, testButton.Focused);
				AssertEquals("Postcondition: CalcEdit must lose focuse.", false, grid.LastFocusedColumn.EditControl.Focused);
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZButton.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZButton)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZButton).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
#if !WINZOR
		public void TestOnPaintError()
		{
			try
			{
				ZButton.OnPaintShouldThrowException_ForTest.Value = true;
				Globals.SetIsUnitTestingProductionFunctionality(true);
				Globals.IsTest_ForTest.Value = false;
				AssertNoExceptionThrown(() =>
				{
					using (var form = new ZChildForm())
					{
						form.Shown += async (s, e) =>
						{
							await Task.Delay(1000);
							form.Close();
						};
						using (var button = new ZButton())
						{
							form.Controls.Add(button);
							form.Show();
							Application.DoEvents();
						}
					}
				});
			}
			finally
			{
				Globals.IsTest_ForTest.ResetValue();
				Globals.SetIsUnitTestingProductionFunctionality(false);
				ZButton.OnPaintShouldThrowException_ForTest.Value = false;
			}
		}
#endif
	}
}
