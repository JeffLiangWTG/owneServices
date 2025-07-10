using System.Collections.Generic;
using System.Linq;
using CargoWise.Tools.SpellCheck.GUI;
using Moq;
using WTG.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools.SpellCheck.Testing
{
	public static class SpellCheckerTestHelpers
	{
		public static ISpellCheckerFormSpellingError CreateFormSpellingError(string text, string badWord, params string[] suggestions)
		{
			var mock = new Mock<ISpellCheckerFormSpellingError>();
			mock.Setup(m => m.Text).Returns(text);
			mock.Setup(m => m.SpellingError).Returns(CreateSpellingError(text, badWord, suggestions));
			return mock.Object;
		}

		public static ISpellingErrorWithSuggestions CreateSpellingError(string text, string badWord, params string[] suggestions)
		{
			var mock = new Mock<ISpellingErrorWithSuggestions>();
			mock.Setup(m => m.Suggestions).Returns(suggestions.ToList());
			mock.Setup(m => m.Word).Returns(badWord);
			mock.Setup(m => m.WordIndex).Returns(text.IndexOf(badWord));
			mock.Setup(m => m.Flags).Returns(new List<object>(0));

			return mock.Object;
		}

		public static ISpellChecker CreateSpellChecker(params ISpellingErrorWithSuggestions[] errors)
		{
			var mock = new Mock<ISpellChecker>();
			mock.Setup(s => s.CheckSpelling(It.IsAny<string>())).Returns(new List<ISpellingError>(errors));
			mock.Setup(s => s.GetSuggestions(It.IsAny<ISpellingError>()))
				.Returns((ISpellingError moq) => ((ISpellingErrorWithSuggestions)moq).Suggestions);

			return mock.Object;
		}
	}
}
