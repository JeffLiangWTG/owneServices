using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.Scanning;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Scanning
{
	public class BarcodeManagerTest : TestCaseWithFactory
	{
		#region TestTrimBarcode

		public void TestTrimBarcode()
		{
			var barcodes = new BarcodeManager();
			var wasTrimmed = false;
			barcodes.NonSystemBarcodeScanned += (sender, e) => wasTrimmed = e.Barcode == "ABC";

			AssertEquals(true, barcodes.ExecuteBarcode("ABC "));
			AssertEquals(true, wasTrimmed);

			var isInvoked = false;
			barcodes.AddBarcode("ABC", () => isInvoked = true);

			AssertEquals(true, barcodes.ExecuteBarcode("ABC "));
			AssertEquals(true, isInvoked);
		}

		#endregion

		#region TestAddBarcode_WithInvalidParams

		public void TestAddBarcode_WithInvalidParams()
		{
			var barcodes = new BarcodeManager();

			// null delegate
			AssertExceptionThrown(typeof(ArgumentNullException), () => barcodes.AddBarcode("ABC", null));

			// empty barcode
			AssertExceptionThrown(typeof(ArgumentException), () => barcodes.AddBarcode("", () => { }));

			// valid barcode
			barcodes.AddBarcode("ABC", () => { });

			// duplicate barcode
			AssertExceptionThrown(typeof(ArgumentException), () => barcodes.AddBarcode("ABC", () => { }));
		}

		#endregion

		#region TestAddAndExecuteBarcode

		public void TestAddAndExecuteBarcode()
		{
			var barcodes = new BarcodeManager();

			var isInvoked = false;
			barcodes.AddBarcode("ABC", () => isInvoked = true);

			AssertEquals(false, barcodes.ExecuteBarcode("XYZ"));
			AssertEquals(false, isInvoked);

			AssertEquals(true, barcodes.ExecuteBarcode("ABC"));
			AssertEquals(true, isInvoked);
		}

		#endregion

		#region TestNonSystemBarcodeScanned

		public void TestNonSystemBarcodeScanned()
		{
			var barcodes = new BarcodeManager();
			barcodes.AddBarcode("ABC", () => { });

			var isNonSystemBarcodeScannedInvoked = false;
			barcodes.NonSystemBarcodeScanned += delegate(object sender, BarcodeScanEventArgs e)
			{
				isNonSystemBarcodeScannedInvoked = true;
				AssertEquals("BAD", e.Barcode);
			};

			AssertEquals(true, barcodes.ExecuteBarcode("ABC"));
			AssertEquals("System barcode sent, NonSystemBarcodeScanned event should not be invoked.", false, isNonSystemBarcodeScannedInvoked);

			AssertEquals(true, barcodes.ExecuteBarcode("BAD"));
			AssertEquals("Non-System barcode sent, NonSystemBarcodeScanned event should be invoked.", true, isNonSystemBarcodeScannedInvoked);
		}

		#endregion
	}
}
