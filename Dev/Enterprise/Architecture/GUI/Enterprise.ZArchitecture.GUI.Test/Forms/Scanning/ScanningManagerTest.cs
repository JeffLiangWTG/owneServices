using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Scanning.Testing
{
	sealed class ScanningManagerTest : TransactionedTestCase
	{
		#region TestIsStartOrStopScan

		public void TestIsStartOrStopScan()
		{
			AssertEquals(false, ScanningManager.IsStartOrStopScan(Keys.A));
			AssertEquals(false, ScanningManager.IsStartOrStopScan(Keys.D1));
			AssertEquals(true, ScanningManager.IsStartOrStopScan(ScanningManager.ScannerPreAndPostamble_ForTesting));
		}

		public void TestIsStartOrStopScan_Legacy()
		{
			AssertEquals(true, ScanningManager.IsStartOrStopScan(Keys.Control | Keys.OemCloseBrackets));
		}

		#endregion

		#region TestConstructor

		public void TestConstructor()
		{
			using (var form = new ZForm())
			{
				new ScanningManager(form, new BarcodeManager(), () => true); // no exception
				AssertExceptionThrown(typeof(ArgumentNullException), () => new ScanningManager(null, new BarcodeManager(), () => true));
				AssertExceptionThrown(typeof(ArgumentNullException), () => new ScanningManager(form, null, () => true));
			}
		}

		#endregion

		#region TestBarcodeScan_InvokesCorrespondingAction

		public void TestBarcodeScan_InvokesCorrespondingAction()
		{
			using (var form = new ZForm())
			{
				var button = new ZButton();
				form.Controls.Add(button);

				var abcInvoked = false;
				var xyzInvoked = false;

				var barcodes = new BarcodeManager();
				var scanner = new ScanningManager(form, barcodes, () => true);
				barcodes.AddBarcode("abc", () => abcInvoked = true);
				barcodes.AddBarcode("xyz", () => xyzInvoked = true);

				form.Show();
				AssertEquals("Precondition", false, abcInvoked);
				AssertEquals("Precondition", false, xyzInvoked);

				// send non-matching barcode "abcd" to the form
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, Keys.B, Keys.C, Keys.D, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(false, abcInvoked);
				AssertEquals(false, xyzInvoked);

				// send matching barcode "abc" to the form
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, Keys.B, Keys.C, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(true, abcInvoked);
				AssertEquals(false, xyzInvoked);

				// send incomplete barcode "xyz" (no postamble) to a child control
				SendKeys(button, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.X, Keys.Y, Keys.Z);
				AssertEquals("Postamble not sent, thus the barcode action should not be invoked.", false, xyzInvoked);

				// send the remainder of the above barcode (the postamble) to a child control
				SendKeys(button, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(true, xyzInvoked);
			}
		}

		#endregion

		#region TestBarcodeScan_IsOkToScanIsFalse_DoesNotScan

		public void TestBarcodeScan_IsOkToScanIsFalse_DoesNotScan()
		{
			using (var form = new ZForm())
			{
				var isOkToScan = false;
				var abcInvoked = false;

				var barcodes = new BarcodeManager();
				var scanner = new ScanningManager(form, barcodes, () => isOkToScan);
				barcodes.AddBarcode("abc", () => abcInvoked = true);

				form.Show();
				AssertEquals("Precondition", false, abcInvoked);

				// send matching barcode "abc" to the form
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, Keys.B, Keys.C, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(false, abcInvoked);

				isOkToScan = true;
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, Keys.B, Keys.C, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(true, abcInvoked);
			}
		}

		#endregion

		#region TestBarcodeScan_NonSystemBarcode_IsSentToActiveTextControl

		public void TestBarcodeScan_NonSystemBarcode_IsSentToActiveTextControl()
		{
			using (var form = new ZForm())
			{
				var textbox = new ZTextBox();
				textbox.Text = "123";
				form.Controls.Add(textbox);

				var barcodes = new BarcodeManager();
				var scanner = new ScanningManager(form, barcodes, () => true);
				barcodes.AddBarcode("abc", () => { });

				form.Show();

				// send a matching barcode to the text box
				textbox.Focus();
				AssertEquals("Precondition - the textbox must have focus otherwise we are not testing that it ignores keys.", true, textbox.Focused);
				SendKeys(textbox, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, Keys.B, Keys.C, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals("Text controls should not receive system barcodes (such as 'Add Package').", "123", textbox.Text);

				// send a non-matching barcode to the text box
				SendKeys(textbox, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.E, Keys.F, Keys.G, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals("Text controls should receive non-system barcode (such as a Package ID), and existing text should be overridden.", "EFG", textbox.Text);
			}
		}

		#endregion

		#region TestBarcodeScan_FiresScanEvent

		public void TestBarcodeScan_FiresScanEvent()
		{
			using (var form = new ZForm())
			{
				var barcodes = new BarcodeManager();
				barcodes.AddBarcode("abc", () => { });

				var barcodeScanFired = 0;
				var scanner = new ScanningManager(form, barcodes, () => true);
				scanner.BarcodeScan += delegate
				{
					barcodeScanFired++;
				};

				form.Show();

				// send non-matching barcode "abcd" to the form
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, Keys.B, Keys.C, Keys.D, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(1, barcodeScanFired);

				// send matching barcode "abc" to the form
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, Keys.B, Keys.C, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(2, barcodeScanFired);
			}
		}

		#endregion

		#region TestIgnoreEmptyBarcodeScan

		public void TestIgnoreEmptyBarcodeScan()
		{
			using (var form = new ZForm())
			{
				var scannedBarcodes = new List<string>();
				var barcodes = new BarcodeManager();
				barcodes.NonSystemBarcodeScanned += (s, e) => scannedBarcodes.Add(e.Barcode);
				var scanner = new ScanningManager(form, barcodes, () => true);
				form.Show();

				var barcodeScanFired = 0;
				scanner.BarcodeScan += delegate
				{
					barcodeScanFired++;
				};

				var ctrlL = Keys.Control | Keys.L;

				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, ctrlL, ctrlL, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(0, barcodeScanFired);
				AssertEquals(0, scannedBarcodes.Count);

				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, ctrlL, Keys.B, ctrlL, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(1, barcodeScanFired);
				AssertEquals("b", scannedBarcodes.Single());
				scannedBarcodes.Clear();

				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, ctrlL, Keys.B, ctrlL, ctrlL, Keys.C, Keys.D, ctrlL, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(3, barcodeScanFired);
				AssertContainsExactElementsInAnyOrder(new[] { "b", "cd" }, scannedBarcodes);
				scannedBarcodes.Clear();

				// 3 CTRL L's in a row
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, ctrlL, Keys.B, ctrlL, ctrlL, ctrlL, Keys.C, Keys.D, ctrlL, Keys.Space, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(5, barcodeScanFired);
				AssertContainsExactElementsInAnyOrder(new[] { "b", "cd" }, scannedBarcodes);
				scannedBarcodes.Clear();

				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, ctrlL, Keys.Space, ctrlL, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(5, barcodeScanFired);
				AssertEquals(0, scannedBarcodes.Count);
			}
		}

		#endregion

		#region Implementation

		void SendKeys(Control control, params Keys[] keys)
		{
			foreach (var key in keys)
			{
				KeySender.SendKeyDown(control, control.Handle, key);
			}
		}

		#endregion
	}
}
