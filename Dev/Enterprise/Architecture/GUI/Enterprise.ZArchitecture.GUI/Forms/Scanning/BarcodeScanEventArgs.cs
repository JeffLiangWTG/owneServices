using System;

namespace Enterprise.ZArchitecture.GUI.Scanning
{
	public class BarcodeScanEventArgs : EventArgs
	{
		public BarcodeScanEventArgs(string barcode)
		{
			Barcode = barcode;
			Handled = true;
		}

		public readonly string Barcode;
		public bool Handled { get; set; }
	}
}
