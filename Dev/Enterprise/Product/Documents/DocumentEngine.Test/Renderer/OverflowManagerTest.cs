using System.Drawing;
using Enterprise.DocumentEngine.Renderer;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class OverflowManagerTest : TestCase
	{
		public void TestOverflowBehaviourWrapOverflowWithContinued()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 1, OverflowBehaviour.WrapOverflowWithContinued), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", "This is some long string that (continued...)", manager.FirstPageText[0]);
			AssertEquals("Overflow text contains the rest of the text", "needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		public void TestOverflowBehaviourWrapOverflowWithoutContinued()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 1, OverflowBehaviour.WrapOverflowWithoutContinued), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", "This is some long string that needs to be", manager.FirstPageText[0]);
			AssertEquals("Overflow text contains the rest of the text", "wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		public void TestOverflowBehaviourMoveAllContentWithContinued()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 1, OverflowBehaviour.MoveAllContentWithContinued), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", "(continued...)", manager.FirstPageText[0]);
			AssertEquals("Overflow text contains the rest of the text", "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		public void TestOverflowBehaviourMoveAllContentWithoutContinued()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 1, OverflowBehaviour.MoveAllContentWithoutContinued), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", string.Empty, manager.FirstPageText[0]);
			AssertEquals("Overflow text contains the rest of the text", "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		public void TestOverflowBehaviourMoveAllContentAndKeepOriginal()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 1, OverflowBehaviour.MoveAllContentAndKeepOriginal), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", "This is some long string that needs to be", manager.FirstPageText[0]);
			AssertEquals("Overflow text contains the rest of the text", "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		public void TestOverflowFunctionalityWithMaximumLinesLessThanNeededForText()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 1, OverflowBehaviour.WrapOverflowWithContinued), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", "This is some long string that (continued...)", manager.FirstPageText[0]);
			AssertEquals("Overflow text contains the rest of the text", "needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		public void TestOverflowFunctionalityWithMaximumLinesMoreThanNeededForText()
		{
			var testTitle = "Test Title";
			var testText = "This is some short string.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 5, OverflowBehaviour.WrapOverflowWithContinued), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text", "This is some short string.", manager.FirstPageText[0]);
			AssertEquals("manager.OverflowText", string.Empty, manager.OverflowText);
		}

		public void TestOverflowFunctionalityWhenThereAreBlankLinesInTheTextToBeWrapped()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string\nthat needs to be wrapped by the overflow\nmanager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 2, OverflowBehaviour.WrapOverflowWithContinued), font, 10000);
			manager.ManagerOverflow(testText);

			CombineAssertions(delegate
			{
				AssertEquals("Number of elements in WrappedFirstPageText", 2, manager.FirstPageText.Count);
				AssertEquals("Wrapped original text (first line only)", "This is some long string", manager.FirstPageText[0]);
				AssertEquals("Wrapped original text (second line only)", "that needs to be wrapped by (continued...)", manager.FirstPageText[1]);
				AssertEquals("Overflow text contains the rest of the text", "the overflow\r\nmanager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
			});
		}

		public void TestOverflowFunctionalityWhenTheCellIsLessWideThanRequiredForFullContinuedStatement()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 1, OverflowBehaviour.WrapOverflowWithContinued), font, 1000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 1, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", "(cont.)", manager.FirstPageText[0]);
			AssertEquals("Overflow text contains the rest of the text", "This is some long string that needs to be wrapped by the overflow manager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		public void TestOverflowFunctionalityWhenThereAreMultipleBlankLinesInTheTextToBeWrapped()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string that needs to be wrapped by the overflow\nmanager before being inserted back\n\n\n\n\ninto a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 6, OverflowBehaviour.WrapOverflowWithContinued), font, 10000);
			manager.ManagerOverflow(testText);

			CombineAssertions(delegate
			{
				AssertEquals("Number of elements in WrappedFirstPageText", 6, manager.FirstPageText.Count);
				AssertEquals("Wrapped original text (first line only)", "This is some long string that needs to be", manager.FirstPageText[0]);
				AssertEquals("Wrapped original text (second line only)", "wrapped by the overflow", manager.FirstPageText[1]);
				AssertEquals("Wrapped original text (third line only)", "manager before being inserted back", manager.FirstPageText[2]);
				AssertEquals("Wrapped original text (fourth line only)", "", manager.FirstPageText[3]);
				AssertEquals("Wrapped original text (fifth line only)", "", manager.FirstPageText[4]);
				AssertEquals("Wrapped original text (sixth line only)", " (continued...)", manager.FirstPageText[5]);
				AssertEquals("Overflow text contains the rest of the text", "into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
			});
		}

		public void TestOverflowFunctionalityWhenThereAreTabCharactersInTheTextToBeWrapped()
		{
			var testTitle = "Test Title";
			var testText = "This is some long string\tthat needs to be wrapped by the overflow\tmanager before being inserted back into a report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.";
			var font = new Font(new FontFamily("Arial"), 10);

			var manager = new OverflowManager(GetMacro(testTitle, 3, OverflowBehaviour.WrapOverflowWithContinued), font, 10000);
			manager.ManagerOverflow(testText);

			AssertEquals("Number of elements in WrappedFirstPageText", 3, manager.FirstPageText.Count);
			AssertEquals("Wrapped original text (first line only)", "This is some long string that needs to be", manager.FirstPageText[0]);
			AssertEquals("Wrapped original text (second line only)", "wrapped by the overflow manager before", manager.FirstPageText[1]);
			AssertEquals("Wrapped original text (third line only)", "being inserted back into a (continued...)", manager.FirstPageText[2]);
			AssertEquals("Overflow text contains the rest of the text", "report. Yes, this string is quite long. The quick brown fox jumps over the lazy dog.", manager.OverflowText);
		}

		string GetMacro(string notesTitle, int maximumRows, OverflowBehaviour overflowBehaviour)
		{
			return string.Format("<OverFlowToFollowPage(\"{0}\", {1}, {2})>", notesTitle, maximumRows.ToString(), overflowBehaviour.ToString());
		}
	}
}
