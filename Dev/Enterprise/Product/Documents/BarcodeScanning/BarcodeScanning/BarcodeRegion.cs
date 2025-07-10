
namespace Enterprise.Barcode.Business
{
	internal struct BarcodeRegion
	{
		public int X;
		public int Y;
		public int Height;
		public int Width;

		public BarcodeRegion(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Height = height;
			Width = width;
		}
	}
}
