using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class OverflowNoteTest : TestCase
	{
		public void TestConstructor()
		{
			var info = new OverflowNote("My Heading", "My Text");
			AssertEquals("info.Title", "My Heading", info.Title);
			AssertEquals("info.Contents", "My Text", info.Contents);
		}
	}
}
