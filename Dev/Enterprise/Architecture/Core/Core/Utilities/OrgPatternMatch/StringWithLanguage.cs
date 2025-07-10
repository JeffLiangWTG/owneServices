using System;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.Core
{
	public class StringWithLanguage
	{
		public StringWithLanguage(string value, string languageCode)
		{
			Value = value;
			LanguageCode = languageCode;
		}

		public override bool Equals(object obj)
		{
			StringWithLanguage result = obj as StringWithLanguage;
			return result != null && result.Value == Value && result.LanguageCode == LanguageCode;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public readonly string Value;
		public readonly string LanguageCode;

		public override string ToString()
		{
			return Value;
		}

		public bool IsEmpty
		{
			get { return Value == null || Value.Length == 0; }
		}

		public static StringWithLanguage Empty
		{
			get { return empty ?? (empty = new StringWithLanguage("", Constants.Languages.English)); }
		}
		[ThreadStatic]
		static StringWithLanguage empty;
	}
}
