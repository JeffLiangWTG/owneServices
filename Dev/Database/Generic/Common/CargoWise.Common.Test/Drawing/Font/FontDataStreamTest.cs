using System.Drawing;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class FontDataStreamTest : TestCase
	{
		public void TestFontDataStreamForInvalidFontTable()
		{
			using (var stream = new FontDataStream(12345678, new FontFamily("Courier New"), FontStyle.Regular))
			{
				AssertEquals("Stream should be empty", 0L, stream.Length);
			}
		}
	}
}