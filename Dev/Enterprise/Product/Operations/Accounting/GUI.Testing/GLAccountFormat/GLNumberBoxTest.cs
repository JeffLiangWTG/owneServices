using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GLAccountFormat.Testing
{
	public class GLNumberBoxTest : TestCase
	{
		[ExpectNoExceptions()]
		public void TestConstructor()
		{
			new GLNumberBox().Dispose();
			new GLNumberBox("XX.XX").Dispose();
		}

		public void TestCaption()
		{
			using (GLNumberBox testBox = new GLNumberBox("XX"))
			{
				AssertEquals("XX", testBox.Caption);
			}
		}

		public void TestIndex()
		{
			using (GLNumberBox testBox = new GLNumberBox("XX"))
			{
				testBox.Index = 1;
				AssertEquals(1, testBox.Index);
			}
		}

		public void TestHighlight()
		{
			using (GLNumberBox testBox = new GLNumberBox("XX"))
			{
				Assert(!testBox.IsHighlighted);

				testBox.Highlight();
				Assert(testBox.IsHighlighted);

				testBox.UnHighlight();
				Assert(!testBox.IsHighlighted);
			}
		}
	}
}
