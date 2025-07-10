using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CaptionFormatterTest : TestCase
	{
		public void TestWithoutHotKeyFormatting()
		{
			AssertEquals("test", CaptionFormatter.WithoutHotKeyFormatting("test"));
			AssertEquals("test", CaptionFormatter.WithoutHotKeyFormatting("&test"));
			AssertEquals("&test", CaptionFormatter.WithoutHotKeyFormatting("&&test"));
			AssertEquals("&test", CaptionFormatter.WithoutHotKeyFormatting("&&&test"));
			AssertEquals("&&test", CaptionFormatter.WithoutHotKeyFormatting("&&&&test"));
			AssertEquals("test & test", CaptionFormatter.WithoutHotKeyFormatting("test && test"));
		}
	}
}
