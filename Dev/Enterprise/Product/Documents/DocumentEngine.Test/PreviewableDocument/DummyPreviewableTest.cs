using System.Drawing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.PreviewableDocument.Testing
{
	sealed class DummyPreviewableTest : TestCase
	{
		public void TestRenderCreateException()
		{
			using (var document = new DummyPreviewable("Some test Message"))
			{
				var size = document.GetPageSize(1);
				var bitmap = new Bitmap(size.Width, size.Height);
				using (var graphics = Graphics.FromImage(bitmap))
				{
					AssertNoExceptionThrown(() => document.Render(graphics, -1, size));
					AssertNoExceptionThrown(() => document.Render(graphics, -1, Size.Empty));
					AssertNoExceptionThrown(() => document.Render(graphics, -1, new Size(-1, -1)));
				}
			}
		}
	}
}
