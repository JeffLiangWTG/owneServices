using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class StreamSourceTest : TestCase
	{
		public void TestStreamSource()
		{
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.Write("HELLO WORLD");
				writer.Flush();
				var source = new StreamSource(stream);
				AssertEquals("HELLO WORLD", source.GetStream().WriteToString());
				AssertEquals("The position should be set to the beginning each time the GetStream is called", "HELLO WORLD", source.GetStream().WriteToString());
			}
		}
	}
}
