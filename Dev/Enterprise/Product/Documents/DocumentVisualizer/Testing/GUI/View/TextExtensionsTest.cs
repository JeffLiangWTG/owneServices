using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class TextExtensionsTest : TestCase
	{
		public void TestUrl()
		{
			var mock = new Mock<IText>();
			mock.SetupGet(m => m.Content).Returns("https://google.com");

			var res = mock.Object.GetUrls();

			AssertEquals("https://google.com", string.Join(", ", res));
		}

		public void TestUrl_MultipleUrls()
		{
			var mock = new Mock<IText>();
			mock.SetupGet(m => m.Content).Returns("https://google.com and https://amazon.com");

			var res = mock.Object.GetUrls();

			AssertEquals("https://google.com, https://amazon.com", string.Join(", ", res));
		}

		public void TestUrl_WithTextBeforeUrl()
		{
			var mock = new Mock<IText>();
			mock.SetupGet(m => m.Content).Returns("hello, https://google.com");

			var res = mock.Object.GetUrls();

			AssertEquals("https://google.com", string.Join(", ", res));
		}

		public void TestUrl_WithTextAfterUrl()
		{
			var mock = new Mock<IText>();
			mock.SetupGet(m => m.Content).Returns("https://google.com rocks!");

			var res = mock.Object.GetUrls();

			AssertEquals("https://google.com", string.Join(", ", res));
		}

		public void TestUrl_WithTextAroundUrl()
		{
			var mock = new Mock<IText>();
			mock.SetupGet(m => m.Content).Returns("hello, https://google.com rocks!");

			var res = mock.Object.GetUrls();

			AssertEquals("https://google.com", string.Join(", ", res));
		}

		public void TestNoUrl()
		{
			var mock = new Mock<IText>();
			mock.SetupGet(m => m.Content).Returns("abcdefg 1234");

			var res = mock.Object.GetUrls();

			AssertEquals(0, res.Length);
		}
	}
}
