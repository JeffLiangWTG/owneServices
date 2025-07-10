using Enterprise.ZArchitecture.GUI.Scanning;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Scanning
{
	public class BarcodeScanEventArgsTest : TestCase
	{
		#region TestBarcode

		public void TestBarcode()
		{
			AssertEquals("123", new BarcodeScanEventArgs("123").Barcode);
		}

		#endregion

		#region TestHandled

		public void TestHandled()
		{
			var args = new BarcodeScanEventArgs("123");
			AssertEquals(true, args.Handled);

			args.Handled = false;
			AssertEquals(false, args.Handled);
		}

		#endregion
	}
}
