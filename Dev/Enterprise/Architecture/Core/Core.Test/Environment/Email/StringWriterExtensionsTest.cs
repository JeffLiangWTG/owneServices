using System.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class StringWriterExtensionsTest : TestCase
	{
		public void TestWriteBeginTag()
		{
			var writer = new StringWriter();
			writer.WriteBeginTag("a");
			AssertEquals(writer.ToString(), "<a");
		}

		public void TestWriteEndTag()
		{
			var writer = new StringWriter();
			writer.WriteEndTag("a");
			AssertEquals(writer.ToString(), "</a>");
		}

		public void TestWriteBreak()
		{
			var writer = new StringWriter();
			writer.WriteBreak();
			AssertEquals(writer.ToString(), "<br />");
		}

		public void TestWriteAttribute()
		{
			var writer = new StringWriter();
			writer.WriteAttribute("style", "margin: 1em");
			AssertEquals(writer.ToString(), " style=\"margin: 1em\"");
		}

		public void TestWriteEncodedText()
		{
			var writer = new StringWriter();
			writer.WriteEncodedText("<a>");
			AssertEquals(writer.ToString(), "&lt;a&gt;");
		}
	}
}
