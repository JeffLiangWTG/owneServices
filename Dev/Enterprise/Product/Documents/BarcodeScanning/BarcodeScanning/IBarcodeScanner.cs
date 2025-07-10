using System.Collections.Generic;
using System.Drawing;

namespace Enterprise.Barcode.Business
{
	public interface IBarcodeScanner
	{
		IEnumerable<string> Read(Bitmap inputImage);
	}
}
