using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ResourceStrings.Vocabulary;

namespace CargoWise.ResourceStrings.Grammar
{
	class EnglishGrammar : IGrammar
	{
		#region SuppressResourceStringsCheckRegion

		public string IndefiniteArticlePrefix(string subject)
		{
			string prefix;
			if (string.IsNullOrEmpty(subject))
			{
				prefix = string.Empty;
			}
			else if ((StringStartsWithVowel(subject) && !IsVowelException(subject)) || IsConsonantException(subject))
			{
				prefix = "an ";
			}
			else
			{
				prefix = "a ";
			}

			return prefix;
		}

		bool StringStartsWithVowel(string value)
		{
			Argument.NotNullOrEmpty(value, nameof(value));
			return (value[0] == 'a' || value[0] == 'e' || value[0] == 'i' || value[0] == 'o' || value[0] == 'u' ||
				value[0] == 'A' || value[0] == 'E' || value[0] == 'I' || value[0] == 'O' || value[0] == 'U');
		}

		/// <summary>
		/// The rule states that “a” should be used before words that begin with consonants (e.g. b, c, d)
		/// while “an” should be used before words that begin with vowels (a, e, i, o & u). 
		/// Notice, however, that the usage is determined by the pronunciation and not by the spelling
		/// 
		/// Exceptions therefore exist, for example:
		/// You should say, therefore, “an hour” (because hour begins with a vowel sound) and “a history” (because history begins with a consonant sound).
		/// Similarly you should say “a union” even though union begins with a “u”. That is because the pronunciation begins with “yu”, which is a consonant sound.
		/// 
		/// Add any special case terms needed for validation messages to appropriate exception lists below.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		bool IsVowelException(string value)
		{
			Argument.NotNull(value, nameof(value)); // Suggested By ReviewBot 
			var exceptions = new[] { "unit", "uq", "UNDG", "United", "universal", "union", "European" };
			return exceptions.Any(vowelException => value.StartsWith(vowelException, StringComparison.OrdinalIgnoreCase));
		}

		bool IsConsonantException(string value)
		{
			Argument.NotNull(value, nameof(value)); // Suggested By ReviewBot 
			var exceptions = new[] { "hour" };
			return exceptions.Any(consonentException => value.StartsWith(consonentException, StringComparison.OrdinalIgnoreCase));
		}

		public string Pluralize(string singularNoun)
		{
			return EnglishVocabulary.Instance.ApplyRules(singularNoun);
		}

		#endregion
	}
}
