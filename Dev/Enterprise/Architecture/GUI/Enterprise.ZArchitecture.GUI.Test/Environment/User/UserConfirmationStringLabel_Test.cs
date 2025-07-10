using NUnit.Framework;

namespace Enterprise.Core.Environment
{
	sealed class UserConfirmationStringLabel_Test : TestCase
	{
		public void TestCalculateCorrectAndWrongPart()
		{
			using (var label = new UserConfirmationStringLabel())
			{
				label.Text = "I AM A TESTING STRING";

				label.CalculateCorrectAndWrongPart("");
				AssertEquals("Empty input", "", label.correctPartToHighlight);
				AssertEquals("Empty input", "", label.wrongPartToHighlight);

				label.CalculateCorrectAndWrongPart("I AM");
				AssertEquals("Current input has no error", "I AM", label.correctPartToHighlight);
				AssertEquals("Current input has no error", "", label.wrongPartToHighlight);

				label.CalculateCorrectAndWrongPart("I AM B");
				AssertEquals("Current input has error", "I AM ", label.correctPartToHighlight);
				AssertEquals("Current input has error", "A TESTING STRING", label.wrongPartToHighlight);

				label.CalculateCorrectAndWrongPart("I AM A TESTING STRING!");
				AssertEquals("Input is longer than expected text", "", label.correctPartToHighlight);
				AssertEquals("Input is longer than expected text", "I AM A TESTING STRING", label.wrongPartToHighlight);
			}
		}
	}
}
