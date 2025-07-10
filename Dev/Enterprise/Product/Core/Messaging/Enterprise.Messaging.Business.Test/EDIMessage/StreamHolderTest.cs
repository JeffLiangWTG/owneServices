using System.IO;
using System.Text;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	sealed class StreamHolderTest : TestCase
	{
		public void TestStreamHolder()
		{
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.Write("HELLO WORLD");
				writer.Flush();
				var holder = new TextReaderSource(stream);
				AssertEquals("HELLO WORLD", holder.GetReader().ReadToEnd());
				AssertEquals("The position should be set to the beginning each time the GetReader is called", "HELLO WORLD", holder.GetReader().ReadToEnd());
			}
		}

		public void TestStreamHolder_WithEncoding()
		{
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream, encoding: Encoding.UTF8);
				writer.Write("HELLO WORLD,你好世界");
				writer.Flush();
				var holder = new TextReaderSource(stream, Encoding.UTF8);
				AssertEquals("HELLO WORLD,你好世界", holder.GetReader().ReadToEnd());
				AssertEquals("The position should be set to the beginning each time the GetReader is called", "HELLO WORLD,你好世界", holder.GetReader().ReadToEnd());
			}
		}
	}
}
