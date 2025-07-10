using System.IO;
using NUnit.Framework;

namespace CargoWise.Common
{
	class StreamExtensionsTest : TestCase
	{
		public void TestReadFully()
		{
			var myBytes = new byte[] { 1, 2, 3, 4, 5, 6 };
			using (var memoryStream = new MemoryStream(myBytes))
			{
				AssertEquals("Should return the full value", myBytes, memoryStream.ReadFully());
			}
		}
	}
}