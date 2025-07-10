using System;

namespace CargoWise.Common
{
	/// <summary>
	/// Summary description for StringLineBreaker.
	/// </summary>
	public class StringLineBreaker
	{
		public const string AllowedCharsDefaultSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.,-()/=!\"%&*;<>"; // allowed characters set
		public const string AllowedCharsCaseNotCaseSensitiveSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890.,-()/=!\"%&*;<>"; // allowed characters set

		public StringLineBreaker(string valueToBreak, bool lineBreakOnCR, string allowedCharacters)
		{
			Argument.NotNull(valueToBreak, nameof(valueToBreak));
			Argument.NotNull(allowedCharacters, nameof(allowedCharacters));
			this.AllowedCharacters = allowedCharacters;
			this.ValueToBreak = StripCharacters(valueToBreak, lineBreakOnCR);
		}

		public StringLineBreaker(string valueToBreak, bool lineBreakOnCR)
			: this(valueToBreak, lineBreakOnCR, AllowedCharsDefaultSet)
		{
			Argument.NotNull(valueToBreak, nameof(valueToBreak));
		}

		public StringLineBreaker(string valueToBreak)
			: this(valueToBreak, false)
		{
			Argument.NotNull(valueToBreak, nameof(valueToBreak));
		}

		public string GetNextLine(int maxLength)
		{
			if (maxLength < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(maxLength));
			}

			string result = "";
			int cRIndex = this.ValueToBreak.IndexOf("\r", System.StringComparison.OrdinalIgnoreCase);
			if (cRIndex > -1 && cRIndex <= maxLength)
			{
				result = this.ValueToBreak.Substring(0, cRIndex);
				this.ValueToBreak = this.ValueToBreak.Substring(cRIndex + 1);
			}
			else
			{
				if (maxLength >= this.ValueToBreak.Length)
				{
					result = this.ValueToBreak;
					this.ValueToBreak = "";
				}
				else
				{
					int lastSpace = this.ValueToBreak.LastIndexOf(" ", maxLength, System.StringComparison.OrdinalIgnoreCase);
					if (lastSpace == -1)
					{
						result = this.ValueToBreak.Substring(0, maxLength);
						this.ValueToBreak = this.ValueToBreak.Substring(maxLength);
					}
					else
					{
						result = this.ValueToBreak.Substring(0, lastSpace);
						this.ValueToBreak = this.ValueToBreak.Substring(lastSpace + 1);
					}
				}
			}

			return result.Trim();
		}

		public bool IsEmpty()
		{
			return (string.IsNullOrEmpty(this.ValueToBreak));
		}

		string allowedCharacters;
		protected internal string AllowedCharacters
		{
			get { return allowedCharacters; }
			set
			{
				Argument.NotNull(value, nameof(value));
				allowedCharacters = value;
			}
		}

		string valueToBreak;
		protected internal string ValueToBreak
		{
			get { return valueToBreak; }
			set
			{
				Argument.NotNull(value, nameof(value));
				valueToBreak = value;
			}
		}

		protected internal string StripCharacters(string input, bool lineBreakOnCR)
		{
			Argument.NotNull(input, nameof(input)); // Suggested By ReviewBot 
			string lineBreak = "\r";
			string lineBreaksToRemove = "\n";
			if (!lineBreakOnCR)
			{
				lineBreaksToRemove += lineBreak;
			}

			string result = "";
			char[] valueCharList = input.Trim().ToCharArray();
			bool lastCharWasBlank = false;

			foreach (char forChar in valueCharList)
			{
				char thisChar = forChar;

				if (lineBreakOnCR && thisChar.ToString() == lineBreak)
				{
					if (lastCharWasBlank && result.Length >= 1)
					{
						result = result.Substring(0, result.Length - 1);
					}
					result += lineBreak;
					lastCharWasBlank = true;
				}
				else
				{
					if (thisChar == ' ')
					{
						if (!lastCharWasBlank)
						{
							result += " ";
						}

						lastCharWasBlank = true;
					}
					else
					{
						if (this.AllowedCharacters.IndexOf(thisChar) != -1)
						{
							result += thisChar.ToString();
							lastCharWasBlank = false;
						}
					}
				}
			}

			return result.Trim();
		}
	}
}
