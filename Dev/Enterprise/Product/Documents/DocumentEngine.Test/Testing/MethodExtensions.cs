using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.Testing
{
	static class MethodExtensions
	{
		internal static int GetPageCount(this Image image)
		{
			return image.GetFrameCount(FrameDimension.Page);
		}

		internal static void LoadExcelFile(this ExcelInterface excelInterface, byte[] contents)
		{
			using (var stream = new MemoryStream(contents))
			{
				excelInterface.LoadExcelFile(stream);
			}
		}
	}
}
