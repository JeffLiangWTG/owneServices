using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TextLayoutTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Initial Vertical Position", 1, layout.VerticalPosition);
			AssertEquals("Initial Horizontal Position", 1, layout.HorizontalPosition);
		}

		public void TestSimplePrint()
		{
			layout.PrintAt(1, 1, "HELLO");
			AssertEquals("HELLO", layout.Text);
		}

		public void TestComplexPrint()
		{
			layout.PrintAt(1, 3, "LLO");
			layout.PrintAt(1, 1, "HE");
			AssertEquals("HELLO", layout.Text);
		}

		public void TestMultiLinePrint()
		{
			layout.PrintAt(1, 1, "HELLO");
			layout.PrintAt(3, 1, "GOODBYE");
			AssertEquals("HELLO" + System.Environment.NewLine + System.Environment.NewLine + "GOODBYE", layout.Text);
		}

		public void TestCR()
		{
			layout.PrintAt(1, 1, "HELLO");
			AssertEquals("Precondition", 6, layout.HorizontalPosition);
			layout.CR();
			AssertEquals("After CR", 1, layout.HorizontalPosition);
		}

		public void TestLF()
		{
			layout.PrintAt(1, 1, "HELLO");
			AssertEquals("Precondition", 6, layout.HorizontalPosition);
			layout.CR();
			AssertEquals("After CR", 1, layout.HorizontalPosition);
		}

		public void TestSimplePrintAtVerticalHorizontalTextLayout()
		{
			var innerLayout = new TextLayout();
			innerLayout.Print("HELLO");
			layout.PrintAt(1, 1, innerLayout);
			AssertEquals("HELLO", layout.Text);
		}

		public void TestComplexPrintAtVerticalHorizontalTextLayout()
		{
			var innerLayout = new TextLayout();
			innerLayout.Print("X");
			layout.Print("*****");
			layout.Print("*   *");
			layout.Print("*   *");
			layout.Print("*   *");
			layout.Print("*****");
			layout.PrintAt(3, 3, innerLayout);
			AssertEquals("*****" + System.Environment.NewLine +
						 "*   *" + System.Environment.NewLine +
						 "* X *" + System.Environment.NewLine +
						 "*   *" + System.Environment.NewLine +
						 "*****", layout.Text);
		}

		protected override void SetUp()
		{
			layout = new TextLayout();
		}
		TextLayout layout;
	}
}
