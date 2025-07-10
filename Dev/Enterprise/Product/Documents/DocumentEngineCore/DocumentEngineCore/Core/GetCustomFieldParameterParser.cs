using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.DocumentEngineCore
{
	public static class GetCustomFieldParameterParser
	{
		public static (string FieldName, string TypeName) ExtractParameters(string parameters)
		{
			var reversed = StringReverse(parameters);

			try
			{
				Stack<char> possibleType;
				bool hasType = TryExtractType(reversed, out possibleType);

				string fieldValue;
				string typeValue = null;

				if (hasType)
				{
					var sbType = new StringBuilder(possibleType.Count);
					foreach (var aChar in possibleType)
					{
						sbType.Append(aChar);
					}

					typeValue = sbType.ToString();

					var reversedFieldValue = new Stack<char>(reversed.SkipWhile(x => char.IsWhiteSpace(x)));
					var sbField = new StringBuilder(reversedFieldValue.Count);

					foreach (var aChar in reversedFieldValue)
					{
						sbField.Append(aChar);
					}

					fieldValue = sbField.ToString();
				}
				else
				{
					fieldValue = parameters;
				}

				return fieldValue.Length > 0 ? (fieldValue, typeValue) : (parameters, null);
			}
			catch (InvalidOperationException)
			{
				return (parameters, null);
			}
		}

		static bool TryExtractType(IEnumerable<char> reversed, out Stack<char> possibleType)
		{
			var first = reversed.First();
			bool quoted = first == '"';
			var hasType = false;

			if (quoted)
			{
				possibleType = new Stack<char>(reversed.SkipWhile(x => char.IsWhiteSpace(x)).Take(3));
				if (reversed.SkipWhile(x => char.IsWhiteSpace(x)).First() == '"')
				{
					hasType = reversed.SkipWhile(x => char.IsWhiteSpace(x)).First() == ',' && !possibleType.Any(x => char.IsPunctuation(x) || char.IsWhiteSpace(x));
				}
			}
			else
			{
				possibleType = new Stack<char>();
				possibleType.Push(first);
				foreach (var aChar in reversed.Take(2))
				{
					possibleType.Push(aChar);
				}

				hasType = reversed.SkipWhile(x => char.IsWhiteSpace(x)).First() == ',' && !possibleType.Any(x => char.IsPunctuation(x) || char.IsWhiteSpace(x));
			}

			return hasType;
		}

		class StringReverser : IEnumerable<char>
		{
			int index;
			readonly string source;

			public StringReverser(string source)
			{
				this.index = source.Length - 1;
				this.source = source;
			}

			public IEnumerator<char> GetEnumerator()
			{
				while (this.index >= 0)
				{
					--this.index;
					yield return source[this.index + 1];
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}

		static IEnumerable<char> StringReverse(string source)
		{
			return new StringReverser(source);
		}
	}
}
