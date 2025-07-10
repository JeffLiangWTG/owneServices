using System.IO;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public static class TestingConstants
	{
		static string TestDirectory => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs");

		public static string TIF_Resolution200x100 => Path.Combine(TestDirectory, "Resolution200x100.tif");
		public static string TIF_Squares_100dpi => Path.Combine(TestDirectory, "Squares_100dpi.tif");
		public static string TIF_TwoBarcodes => Path.Combine(TestDirectory, "TwoBarcodes.tif");

		public static string PDF_Sample => Path.Combine(TestDirectory, "Sample.PDF");
	}
}
