using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZStringTypeConverter))]
	[DebuggerStepThrough]
	[Serializable]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZString : IZType, IZTypeInternals
	{
		public ZString(object value)
		{
			if (value == null)
			{
				this = Empty;
			}
			else
			{
				if (value is string v)
				{
					this = new ZString(v);
				}
				else
				{
					TypeConverter converter = ZStringTypeConverter.Instance;
					if (converter.CanConvertFrom(value.GetType()))
					{
						var converted = converter.ConvertFrom(value);
						this = (ZString)converted;
					}
					else
					{
						throw new ZTypeValueException(typeof(ZString), value);
					}
				}
			}
		}

		public ZString(string value)
		{
			if (value == null)
			{
				this = Empty;
			}
			else
			{
				fValue_DoNotAccessMeDirectly = value;
			}
		}

		public ZString(char value, int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Cannot initialize a string with negative number of chars.", nameof(count));
			}

			this = new ZString(new string(value, count));
		}

		[DebuggerStepThrough]
		public ZString(char value)
			: this(value, 1)
		{
		}

		#region Object Overrides
		public override bool Equals(object obj)
		{
			bool result;
			if (obj is ZString @string && @string == this)
			{
				result = true;
			}
			else
			{
				result = obj is string o && o == fValue;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return fValue.GetHashCode();
		}

		public override string ToString()
		{
			return fValue;
		}

		#endregion

		#region Operator Overloads

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZString lhs, ZString rhs)
		{
			return lhs.fValue.CompareTo(rhs.fValue) == -1;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZString lhs, ZString rhs)
		{
			return lhs.fValue.CompareTo(rhs.fValue) == 1;
		}

		#region ZString == and != ZString

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator ==(ZString lhs, ZString rhs)
		{
			// For better performance
			return
				lhs.fValue_DoNotAccessMeDirectly != null && rhs.fValue_DoNotAccessMeDirectly != null
					? lhs.fValue_DoNotAccessMeDirectly == rhs.fValue_DoNotAccessMeDirectly
					: lhs.fValue == rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZString lhs, ZString rhs)
		{
			// For better performance
			return
				lhs.fValue_DoNotAccessMeDirectly != null && rhs.fValue_DoNotAccessMeDirectly != null
					? lhs.fValue_DoNotAccessMeDirectly != rhs.fValue_DoNotAccessMeDirectly
					: lhs.fValue != rhs.fValue;
		}

		#endregion

		#region ZString == and != string

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator ==(ZString lhs, string rhs)
		{
			return lhs.fValue == rhs;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator !=(ZString lhs, string rhs)
		{
			return lhs.fValue != rhs;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator ==(string lhs, ZString rhs)
		{
			return lhs == rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator !=(string lhs, ZString rhs)
		{
			return lhs != rhs.fValue;
		}

		#endregion

		#region ZString + ZString

		// This overload is not necessary but is in place to work around a compiler bug (see TestPlusEqualsOperator())
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static string operator +(ZString lhs, ZString rhs)
		{
			return lhs.fValue + rhs.fValue;
		}

		#endregion

		#endregion
		#region Casting

		[DebuggerStepThrough]
		public static implicit operator ZString(string value)
		{
			return new ZString(value);
		}

		[DebuggerStepThrough]
		public static implicit operator string(ZString value)
		{
			return value.fValue;
		}

		#endregion

		#region Wrapping string Functions

		#region Static

		public static ZString Join(ZString separator, ZString[] values)
		{
			Argument.NotNull(values, nameof(values));
			return string.Join(separator, ToStringArray(values));
		}

		public static ZString Join(ZString separator, ZString[] values, int startIndex, int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(count));
			}

			if (startIndex < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(startIndex));
			}

			Argument.NotNull(values, nameof(values));
			if ((startIndex + count) > values.Length)
			{
				throw new ArgumentException("Invalid argument.", nameof(startIndex));
			}

			return string.Join(separator, ToStringArray(values), startIndex, count);
		}

		public static ZString Join(ZString[] values)
		{
			Argument.NotNull(values, nameof(values));
			return Join("", values);
		}

		public static ZString Format(ZString stringToFormat, params object[] args)
		{
			Argument.NotNull(args, nameof(args));
			return string.Format(stringToFormat, args);
		}

		#endregion

		/// <summary>
		/// Does the end of this instance match the specified string?
		/// </summary>
		public bool EndsWith(string value)
		{
			Argument.NotNull(value, nameof(value));

			return fValue.EndsWith(value);
		}

		/// <summary>
		/// Does the end of this instance match the specified string?
		/// </summary>
		/// <param name="value">The string to expect at the end.</param>
		/// <param name="comparisonType">Comparison Type.</param>
		public bool EndsWith(string value, StringComparison comparisonType)
		{
			Argument.NotNull(value, nameof(value));
			if (!Enum.IsDefined(typeof(StringComparison), comparisonType))
			{
				throw new ArgumentException("Invalid argument.", nameof(comparisonType));
			}

			return fValue.EndsWith(value, comparisonType);
		}

		/// <summary>
		/// The index of the first occurrence of the specified string.
		/// </summary>
		/// <param name="value">The string to seek.</param>
		public int IndexOf(ZString value)
		{
			return fValue.IndexOf(value.fValue);
		}

		/// <summary>
		/// The index of the first occurrence of the specified string.
		/// </summary>
		/// <param name="value">The string to seek.</param>
		/// <param name="comparisonType">Comparison Type.</param>
		public int IndexOf(ZString value, StringComparison comparisonType)
		{
			if (!Enum.IsDefined(typeof(StringComparison), comparisonType))
			{
				throw new ArgumentException("Invalid argument.", nameof(comparisonType));
			}

			return fValue.IndexOf(value.fValue, comparisonType);
		}

		/// <summary>
		/// The index of the first occurrence of the specified string.
		/// </summary>
		/// <param name="value">The string to seek.</param>
		/// <param name="startIndex">Index to start seeking.</param>
		/// <param name="comparisonType">Comparison Type.</param>
		public int IndexOf(ZString value, int startIndex, StringComparison comparisonType)
		{
			if (!(startIndex >= 0 && startIndex <= Length))
			{
				throw new ArgumentException("An invalid index provided (IndexOf).", nameof(startIndex));
			}

			return fValue.IndexOf(value.fValue, startIndex, comparisonType);
		}

		/// <summary>
		/// The index of the first occurrence of the specified character.
		/// </summary>
		/// <param name="value">A Unicode character to seek.</param>
		public int IndexOf(char value)
		{
			return fValue.IndexOf(value);
		}

		/// <summary>
		/// The Position of a character value at the nth Occurrence within a string. will return -1 if not found
		/// </summary>
		/// <param name="value">The character to seek</param>
		/// <param name="occurrence">The nth occurrence to seek.</param>
		/// <returns></returns>
		public int IndexOf(char value, int occurrence)
		{
			if (occurrence <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(occurrence), "Cannot return a string position for a negative occurrence.");
			}

			int result = -1;
			int matchCount = 0;
			int position = 0;
			while (matchCount < occurrence && position < Length)
			{
				if (this[position] == value)
				{
					matchCount++;
				}

				position++;
			}
			if (matchCount == occurrence)
			{
				result = position - 1;
			}

			return result;
		}

		/// <summary>
		/// The index of the first occurrence of any character in the specified array.
		/// </summary>
		/// <param name="anyOf">A Unicode character array (not null) containing zero or more characters to seek</param>
		public int IndexOfAny(char[] anyOf)
		{
			Argument.NotNull(anyOf, nameof(anyOf));

			return fValue.IndexOfAny(anyOf);
		}

		/// <summary>
		/// The index of the first occurrence of any character in the specified array.
		/// </summary>
		/// <param name="anyOf">A Unicode character array (not null) containing zero or more characters to seek</param>
		/// <param name="startIndex">Index to start seeking.</param>
		public int IndexOfAny(char[] anyOf, int startIndex)
		{
			Argument.NotNull(anyOf, nameof(anyOf));
			if (!(startIndex >= 0 && startIndex < Length))
			{
				throw new ArgumentException("An invalid start index provided (IndexOfAny).", nameof(startIndex));
			}

			return fValue.IndexOfAny(anyOf, startIndex);
		}

		/// <summary>
		/// The index of the last occurrence of the specified string.
		/// </summary>
		/// <param name="value">The string to seek</param>
		/// <returns></returns>
		/// 
		public int LastIndexOf(ZString value)
		{
			return fValue.LastIndexOf(value);
		}

		/// <summary>
		/// The index of the last occurrence of the specified string.
		/// </summary>
		/// <param name="value">The string to seek</param>
		/// <param name="comparisonType">Comparison Type</param>
		/// <returns></returns>
		public int LastIndexOf(ZString value, StringComparison comparisonType)
		{
			if (!Enum.IsDefined(typeof(StringComparison), comparisonType))
			{
				throw new ArgumentException("Invalid argument.", nameof(comparisonType));
			}

			return fValue.LastIndexOf(value, comparisonType);
		}

		/// <summary>
		/// The index of the last occurrence of the specified character.
		/// </summary>
		/// <param name="value">The character to seek</param>
		/// <returns></returns>
		public int LastIndexOf(char value)
		{
			return fValue.LastIndexOf(value);
		}

		/// <summary>
		/// This instance with a specified string inserted at a specified index position.
		/// </summary>
		/// <param name="startIndex">The index position of the insertion.</param>
		/// <param name="value">The string to insert.</param>
		/// <returns></returns>
		public string Insert(int startIndex, string value)
		{
			Argument.NotNull(value, nameof(value));
			if (startIndex < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(startIndex));
			}

			if (startIndex > Length)
			{
				throw new ArgumentException("Invalid argument.", nameof(startIndex));
			}

			return fValue.Insert(startIndex, value);
		}

		/// <summary>
		/// The number of characters in this instance.
		/// </summary>
		[XmlIgnore]
		public int Length
		{
			get
			{
				return fValue.Length;
			}
		}

		public ZString PadLeft(int totalWidth)
		{
			if (totalWidth < 0)
			{
				throw new ArgumentException("An invalid width for padding left.", nameof(totalWidth));
			}

			return new ZString(fValue.PadLeft(totalWidth));
		}

		public ZString PadLeft(int totalWidth, char paddingCharacter)
		{
			if (totalWidth < 0)
			{
				throw new ArgumentException("An invalid width for padding left with a character.", nameof(totalWidth));
			}

			return new ZString(fValue.PadLeft(totalWidth, paddingCharacter));
		}

		public ZString PadRight(int totalWidth)
		{
			if (totalWidth < 0)
			{
				throw new ArgumentException("An invalid width for padding right.", nameof(totalWidth));
			}

			return new ZString(fValue.PadRight(totalWidth));
		}

		public ZString PadRight(int totalWidth, char paddingCharacter)
		{
			if (totalWidth < 0)
			{
				throw new ArgumentException("An invalid width for padding right with a character.", nameof(totalWidth));
			}

			return new ZString(fValue.PadRight(totalWidth, paddingCharacter));
		}

		public ZString Remove(int startIndex, int count)
		{
			if (startIndex < 0)
			{
				throw new ArgumentException("An invalid start index for string removal.", nameof(startIndex));
			}

			if (count < 0)
			{
				throw new ArgumentException("An invalid length of a string to remove.", nameof(count));
			}

			if (startIndex + count > Length)
			{
				throw new ArgumentException("Too many characters to remove.", nameof(startIndex));
			}

			return new ZString(fValue.Remove(startIndex, count));
		}

		public ZString RemoveSafe(int startIndex, int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(count));
			}

			ZString result;

			if (startIndex < 0 || startIndex > Length)
			{
				result = fValue; // don't remove anything
			}
			else
			{
				if (startIndex + count > Length)
				{
					count = Length - startIndex;
				}

				result = fValue.Remove(startIndex, count);
			}

			return result;
		}

		public ZString Replace(char oldValue, char newValue)
		{
			return fValue.Replace(oldValue, newValue);
		}

		public ZString Replace(ZString oldValue, ZString newValue)
		{
			if (oldValue.Length <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(oldValue));
			}

			return fValue.Replace(oldValue, newValue);
		}

		public ZString ReplaceIgnoringCase(ZString oldValue, ZString newValue)
		{
			return Regex.Replace(fValue, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
		}

		public ZString[] Split(params string[] separator)
		{
			var result = ((string)this).Split(separator, StringSplitOptions.None);
			return result.Select(x => new ZString(x)).ToArray();
		}

		public ZString[] Split(params char[] separator)
		{
			string[] splitText = fValue.Split(separator);
			ZString[] result = new ZString[splitText.Length];

			for (int i = 0; i < splitText.Length; i++)
			{
				result[i] = new ZString(splitText[i]);
			}

			return result;
		}

		public ZString[] Split(char[] separator, int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Cannot get a negative amount of splitted elements.", nameof(count));
			}

			string[] splitText = fValue.Split(separator, count);
			ZString[] result = new ZString[splitText.Length];

			for (int i = 0; i < splitText.Length; i++)
			{
				result[i] = splitText[i];
			}

			return result;
		}

		public ZString[] Split(int lengthToSplit)
		{
			if (lengthToSplit <= 0)
			{
				throw new ArgumentException("Cannot split with a negative or zero split length.", nameof(lengthToSplit));
			}

			List<ZString> result = new List<ZString>();

			for (int i = 0; i < Length; i += lengthToSplit)
			{
				result.Add(SubstringSafe(i, lengthToSplit));
			}

			return result.ToArray();
		}

		public bool StartsWith(string value)
		{
			Argument.NotNull(value, nameof(value));

			return fValue.StartsWith(value);
		}

		/// <summary>
		/// Does the start of this instance match the specified string?
		/// </summary>
		/// <param name="value">The string to expect at the start.</param>
		/// <param name="comparisonType">Comparison Type.</param>
		public bool StartsWith(string value, StringComparison comparisonType)
		{
			Argument.NotNull(value, nameof(value));

			return fValue.StartsWith(value, comparisonType);
		}

		public ZString Substring(int startIndex)
		{
			if (startIndex > Length)
			{
				throw new ArgumentException("Invalid argument.", nameof(startIndex));
			}

			return (startIndex > 0) ? new ZString(fValue.Substring(startIndex)) : this;
		}

		public ZString Substring(int startIndex, int length)
		{
			if (!((startIndex <= 0 && length >= Length) || (startIndex >= 0 && length >= 0 && (length + startIndex) <= Length)))
			{
				throw new ArgumentException("Invalid argument.", nameof(startIndex));
			}

			if (startIndex <= 0 && length >= fValue.Length)
			{
				return this;
			}
			else
			{
				return new ZString(fValue.Substring(startIndex, length));
			}
		}

		public ZString ToLower()
		{
			return new ZString(fValue.ToLower());
		}

		public ZString ToUpper()
		{
			var result = fValue.ToUpper();
			return result != fValue ? new ZString(result) : new ZString(fValue);
		}

		public ZString ToUpperInvariant()
		{
			var result = fValue.ToUpperInvariant();
			return result != fValue ? new ZString(result) : new ZString(fValue);
		}

		public ZString ToTitleCase()
		{
			ZString[] words = Split(' ');
			ZString result = Empty;
			foreach (ZString word in words)
			{
				result += word.SubstringSafe(0, 1).ToUpper() + word.SubstringSafe(1).ToLower() + " ";
			}
			return result.Trim();
		}

		public ZString InsertSafe(int insertAt, ZString valueToInsert)
		{
			ZString result = fValue;
			if (Length >= insertAt && insertAt >= 0)
			{
				result = Insert(insertAt, valueToInsert);
			}
			return result;
		}

		public ZString RemoveNonASCIICharacters()
		{
			return new ZString(Regex.Replace(fValue, @"[^\u0000-\u007F]+", string.Empty));
		}

		public ZString ReplaceNonBreakingSpaceWithNormalSpace()
		{
			return new ZString(Regex.Replace(fValue, @"[\u00A0]+", " "));
		}

		public char this[int index]
		{
			get
			{
				if (!(index >= 0 && index < Length))
				{
					throw new ArgumentException("String indexer is out of bound.", nameof(index));
				}

				return fValue[index];
			}
		}

		public ZString Truncate(int maxLength)
		{
			if (maxLength <= 3)
			{
				throw new ArgumentException("Invalid length for truncating.", nameof(maxLength));
			}

			var result = fValue.Length > maxLength
				? (fValue.Substring(0, maxLength - 3) + "...")
				: fValue;
			return (ZString)result;
		}

		#region Trim

		public ZString Trim()
		{
			return fValue.Trim();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "chars")]
		public ZString Trim(params char[] trimChars)
		{
			return fValue.Trim(trimChars);
		}

		public ZString TrimStart()
		{
			return fValue.TrimStart(null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "chars")]
		public ZString TrimStart(params char[] trimChars)
		{
			return fValue.TrimStart(trimChars);
		}

		public ZString TrimEnd()
		{
			return fValue.TrimEnd(null);
		}

		public ZString TrimEnd(char trimChar)
		{
			if (trimChar == ' ')
			{
				return TrimEnd(singleSpaceCharArray);
			}
			else
			{
				return TrimEnd(new[] { trimChar });
			}
		}

		public ZString TrimEndSpaceTab()
		{
			return fValue.TrimEnd(spaceTabCharArray);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Avoiding extra allocations for performance reasons, this array will be passed directly into string.Format()")]
		static readonly char[] singleSpaceCharArray = new[] { ' ' };
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Avoiding extra allocations for performance reasons, this array will be passed directly into string.Format()")]
		static readonly char[] spaceTabCharArray = new[] { ' ', '\t' };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "chars")]
		public ZString TrimEnd(params char[] trimChars)
		{
			return fValue.TrimEnd(trimChars);
		}

		/// <summary>
		/// Removes whitespace plus characterToRemove from the end of a string.
		/// </summary>
		public ZString TrimEndIncludingWhiteSpace(char characterToRemove)
		{
			return TrimEnd(GetWhiteSpacePlusChar(characterToRemove));
		}

		/// <summary>
		/// Removes whitespace plus characterToRemove from the beginning and end of a string.
		/// </summary>
		public ZString TrimIncludingWhiteSpace(char characterToRemove)
		{
			return Trim(GetWhiteSpacePlusChar(characterToRemove));
		}

		char[] GetWhiteSpacePlusChar(char character)
		{
			return new char[]
			{
				'\t', '\n', '\v', '\f', '\r', ' ', '\x00a0', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008',
				'\u2009', '\u200a', '\u200b', '\u3000', '\ufeff', // decompiled from System.String.WhitespaceChars which is internal.
				character
			};
		}

		#endregion

		#endregion

		#region New String Methods

		// Please add only *new* ZString functionality here in alphabetical order.

		#region Static

		/// <summary>
		/// The empty ZString.
		/// </summary>
		public static readonly ZString Empty = new ZString(string.Empty);

		/// <summary>
		/// Generates a ZString containing the requested count of a particular character
		/// </summary>
		public static ZString Replicate(char value, int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Cannot replicate a negative number of characters.", nameof(count));
			}

			return Empty.PadRight(count, value);
		}

		/// <summary>
		/// The given ZString array converted to an array of strings.
		/// </summary>
		public static string[] ToStringArray(ZString[] values)
		{
			if (values == null)
			{
				return null;
			}

			string[] result = new string[values.Length];

			for (int i = 0; i < values.Length; i++)
			{
				result[i] = values[i];
			}

			return result;
		}

		#endregion

		public bool Contains(ZString value)
		{
			return IndexOf(value) != -1;
		}

		public bool Contains(ZString value, StringComparison comparisonType)
		{
			return IndexOf(value, comparisonType) != -1;
		}

		public bool Contains(char value)
		{
			return IndexOf(value) != -1;
		}

		public bool ContainsAnyChar(string characters)
		{
			Argument.NotNull(characters, nameof(characters));
			foreach (char c in characters)
			{
				if (Contains(c))
				{
					return true;
				}
			}

			return false;
		}

		[XmlIgnore]
		public bool ContainsAnyLetters
		{
			get
			{
				foreach (char c in fValue)
				{
					if (char.IsLetter(c))
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool EqualsIgnoringCase(string other)
		{
			return string.Equals(fValue, other, StringComparison.OrdinalIgnoreCase);
		}

		public bool EqualsIgnoringCase(ZString other)
		{
			return string.Equals(fValue, other.fValue, StringComparison.OrdinalIgnoreCase);
		}

		public ZString KeepChars(ZString charactersToKeep)
		{
			return KeepChars(charactersToKeep, "");
		}

		public ZString KeepChars(ZString charactersToKeep, ZString replaceCharacter)
		{
			var result = new StringBuilder(fValue.Length);

			foreach (char c in fValue)
			{
				if (charactersToKeep.Contains(c))
				{
					result.Append(c);
				}
				else
				{
					result.Append(replaceCharacter);
				}
			}

			return result.ToString();
		}

		public ZString KeepCharsXMLFormatting()
		{
			if (!string.IsNullOrEmpty(fValue))
			{
				var xmlFormattedValue = ReplacingSpecialCharactereForXMLFormatting();
				try
				{
					return XmlConvert.EncodeName(xmlFormattedValue.KeepXmlElementCharacters());
				}
				catch (XmlException)
				{
					return Empty;
				}
			}
			return Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dictionary value")]
		ZString ReplacingSpecialCharactereForXMLFormatting()
		{
			var result = new StringBuilder(fValue.Length);
			var specialCharacter = new Dictionary<char, ZString> { { '%', "Percent" } };
			foreach (var c in fValue)
			{
				if (specialCharacter.ContainsKey(c))
				{
					result.Append(specialCharacter[c]);
				}
				else
				{
					result.Append(c);
				}
			}
			return result.ToString();
		}

		ZString KeepXmlElementCharacters()
		{
			var result = string.Concat(fValue.Where(XmlConvert.IsNCNameChar));
			return Regex.Replace(result, @"^[\d-.]*\s*", "");
		}

		public ZString KeepCharsUntil(ZString charactersToKeep, char[] charactersToStopOnEncountering)
		{
			Argument.NotNull(charactersToStopOnEncountering, nameof(charactersToStopOnEncountering));

			int indexOfChars = fValue.IndexOfAny(charactersToStopOnEncountering);

			if (indexOfChars < 0)
			{
				return KeepChars(charactersToKeep);
			}
			else
			{
				string newValue = fValue.Substring(0, indexOfChars);
				ZString zStringValue = newValue;
				return zStringValue.KeepChars(charactersToKeep);
			}
		}

		public ZString ExcludeChars(ZString charactersToExclude)
		{
			StringBuilder result = new StringBuilder(fValue.Length);

			foreach (char c in fValue)
			{
				if (!charactersToExclude.Contains(c))
				{
					result.Append(c);
				}
			}

			return result.ToString();
		}

		public ZString ExcludeNonValidXMLCharacters()
		{
			if (IsEmpty)
			{
				return fValue;
			}

			var result = new StringBuilder(fValue.Length);
			for (var i = 0; i < fValue.Length; i++)
			{
				if (char.IsSurrogatePair(fValue, i))
				{
					int c = char.ConvertToUtf32(fValue, i);
					i++;
					if (IsValidCharacter(c))
					{
						result.Append(char.ConvertFromUtf32(c));
					}
				}
				else
				{
					char c = fValue[i];
					if (IsValidCharacter(c))
					{
						result.Append(c);
					}
				}
			}
			return result.ToString();
		}

		static bool IsValidCharacter(int c)
		{
			//http://www.w3.org/TR/REC-xml/#charsets, valid characters are:
			// #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]	/* any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. */
			//We can't use a regex as it doesn't handle UTF-32

			return c == 0x9 || c == 0xA || c == 0xD || (c >= 0x20 && c <= 0xD7FF) || (c >= 0xE000 && c <= 0xFFFD) || (c >= 0x10000 && c <= 0x10FFFF);
		}

		/// <summary>
		/// Will return a line of line numbers upon which the text (first arg) is found. e.g. seeking "Dan" in "Dan\r\nSays\r\nthat Dan\r\nRocks" will give back {1, 3}
		/// </summary>
		/// <param name="seek">The text you're trying to find</param>
		/// <returns>The 1-based (for humans) line(s) on which your string appears</returns>
		public int[] FindLineNumbersUponWhichStringAppears(string seek)
		{
			Argument.NotNull(seek, nameof(seek));
			return FindLineNumbersUponWhichStringAppears(seek, Environment.NewLine);
		}

		/// <param name="seek">The text you're trying to find</param>
		/// <param name="lineSeparator">Another separator if not Environment.NewLine</param>
		/// <returns>The 1-based (for humans) line(s) on which your string appears</returns>
		public int[] FindLineNumbersUponWhichStringAppears(string seek, ZString lineSeparator)
		{
			Argument.NotNull(seek, nameof(seek));

			List<int> list = new List<int>();
			var sections = Regex.Split(this, seek);
			int runningTally = 1;
			for (int i = 0; i < sections.Length - 1; i++)
			{
				ZString fullSection = sections[i] + seek;
				int occurencesOfSeparator = fullSection.Occurrences(lineSeparator);
				runningTally += occurencesOfSeparator;
				list.Add(runningTally);
			}
			return list.ToArray();
		}

		/// <param name="seek">The text you're trying to find</param>
		/// <returns>The 1-based (for humans) line(s) which start with your string</returns>
		public int[] FindLineNumbersWhichStartWithString(string seek)
		{
			Argument.NotNull(seek, nameof(seek));
			return FindLineNumbersWhichStartWithString(seek, Environment.NewLine);
		}

		/// <param name="seek">The text you're trying to find</param>
		/// <param name="lineSeparator">Another separator if not Environment.NewLine</param>
		/// <returns>The 1-based (for humans) line(s) which start with your string</returns>
		public int[] FindLineNumbersWhichStartWithString(string seek, ZString lineSeparator)
		{
			Argument.NotNull(seek, nameof(seek));

			var arrText = Regex.Split(this, lineSeparator);
			List<int> list = new List<int>();
			int lineIndex = 1;

			foreach (var txt in arrText)
			{
				if (txt.IndexOf(seek, StringComparison.Ordinal) == 0)
				{
					list.Add(lineIndex);
				}
				lineIndex++;
			}
			return list.ToArray();
		}

		/// <summary>
		/// The left of this instance, up to the specified maximum length.
		/// </summary>
		public ZString Left(int length)
		{
			return SubstringSafe(0, length);
		}

		/// <summary>
		/// The right of this instance, up to the specified maximum length.
		/// </summary>
		public ZString Right(int length)
		{
			return SubstringSafe(fValue.Length - length, length);
		}

		/// <summary>
		/// The number of times a given string exists within this string (case sensitive). "aaa".Occurences("aa") will return 1.
		/// </summary>
		public ZInt Occurrences(ZString text)
		{
			return Regex.Matches(this, Regex.Escape(text)).Count;
		}

		/// <summary>
		/// The number of times a given string exists within this string, ignoring case. "aAa".Occurences("aa") will return 1.
		/// </summary>
		public ZInt OccurrencesIgnoringCase(ZString text)
		{
			return Regex.Matches(this, Regex.Escape(text), RegexOptions.IgnoreCase).Count;
		}

		/// <summary>
		/// Split a string by a delimiter, but ignoring cases where the delimiter is escaped by another character.
		/// </summary>
		public ZString[] SplitIgnoringEscapedDelimiter(char delimiter, char escapeCharacter)
		{
			return SplitIgnoringEscapedDelimiter(delimiter, escapeCharacter, false);
		}

		/// <summary>
		/// Split a string by a delimiter, but ignoring cases where the delimiter is escaped by another character.
		/// </summary>
		public ZString[] SplitIgnoringEscapedDelimiter(char delimiter, char escapeCharacter, bool leaveSplitCharacter, bool leaveEmptyLastValue = false)
		{
			List<ZString> result = new List<ZString>();

			int startPos = 0;
			for (int i = 0; i < fValue.Length; i++)
			{
				char currentCharacter = fValue[i];

				if (currentCharacter == escapeCharacter)
				{
					i++;
				}
				else if (currentCharacter == delimiter)
				{
					var subStringLength = i - startPos;

					string splittedString = Substring(startPos, subStringLength);
					if (leaveSplitCharacter)
					{
						splittedString += delimiter;
					}
					result.Add(splittedString);
					startPos = i + 1;
				}
			}

			if (startPos < Length)
			{
				string remainingString = Substring(startPos);
				result.Add(remainingString);
			}

			if (leaveEmptyLastValue && !leaveSplitCharacter && startPos == Length)
			{
				result.Add(ZString.Empty);
			}

			return result.ToArray();
		}

		/// <summary>
		/// A substring of this instance, from the specified position (empty if beyond the end of the string).
		/// </summary>
		public ZString SubstringSafe(int startIndex)
		{
			return SubstringSafe(startIndex, int.MaxValue);
		}

		/// <summary>
		/// A substring of this instance, from the specified position (empty if beyond the end of the string), up to the specified maximum length.
		/// </summary>
		public ZString SubstringSafe(int startIndex, int length)
		{
			ZString result = Empty;

			if (startIndex < 0)
			{
				startIndex = 0;
			}

			int actualLength = Length - startIndex;
			if (actualLength > 0)
			{
				if (actualLength > length && length >= 0)
				{
					actualLength = length;
				}

				result = Substring(startIndex, actualLength);
			}

			return result;
		}

		/// <summary>
		/// Remove diacritics from a string. "ÂÇ".RemoveDiacritics() will return "AC"
		/// </summary>
		public ZString RemoveDiacritics()
		{
			var normalizedString = fValue.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			if (!string.IsNullOrEmpty(normalizedString))
			{
				for (int i = 0; i < normalizedString.Length; i++)
				{
					char c = normalizedString[i];
					if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
					{
						stringBuilder.Append(c);
					}
				}
			}

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Do any characters in the string contain diacritics? Eg. ÂÇ
		/// </summary>
		[XmlIgnore]
		public bool ContainsAnyDiacritics
		{
			get
			{
				var normalizedString = fValue.Normalize(NormalizationForm.FormD);
				return !string.IsNullOrEmpty(normalizedString) && normalizedString.Any(c => CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark);
			}
		}

		#region IsWesternEuropeanOrEmpty

		/// <summary>
		/// Are all characters in this string within the ASCII range of 9, 10, 11, 12, 13, 32 through 127, and the Western European code page (1252) 128 through 255?
		/// </summary>
		[XmlIgnore]
		public bool IsWesternEuropeanOrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					if (!IsWesternEuropeanCharacter(c))
					{
						return false;
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Are all characters in this string within the ASCII range of 0 through 127, and the Western European code page (1252) 128 through 255?
		/// </summary>
		[XmlIgnore]
		public bool IsWindows1252OrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					if (c >> 7 != 0) // test for 0 - 127
					{
						if (!WesternEuropeanChars128Through255.Contains(c)) // test for 128 - 255 (Code Page 1252)
						{
							return false;
						}
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Are all characters in this string within the ASCII range of 32-126?
		/// </summary>
		[XmlIgnore]
		public bool IsPrintableASCIIOrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					if (!IsPrintableASCIICharacter(c))
					{
						return false;
					}
				}

				return true;
			}
		}

		public ZString StripNonWesternEuropeanCharacters()
		{
			if (!IsWesternEuropeanOrEmpty)
			{
				var stringBuilder = new StringBuilder();

				foreach (char c in this)
				{
					if (IsWesternEuropeanCharacter(c))
					{
						stringBuilder.Append(c);
					}
				}

				return stringBuilder.ToString();
			}

			return this;
		}

		static bool IsWesternEuropeanCharacter(char c)
		{
			int code = c;
			return !(code < 9 || (code > 13 && code < 32) || (code > 127 && !WesternEuropeanChars128Through255.Contains(c)));
		}

		public ZString StripNonPrintableASCIICharacters()
		{
			if (!IsPrintableASCIIOrEmpty)
			{
				var stringBuilder = new StringBuilder();

				foreach (char c in this)
				{
					if (IsPrintableASCIICharacter(c))
					{
						stringBuilder.Append(c);
					}
				}

				return stringBuilder.ToString();
			}

			return this;
		}
		static bool IsPrintableASCIICharacter(char c)
		{
			int code = c;
			return code >= 32 && code <= 126;
		}
		public ZString ConvertToWesternEuropeanCharacters()
		{
			byte[] stringIn1252 = Encoding.GetEncoding(1252).GetBytes(this);
			return new string(Encoding.GetEncoding(1252).GetChars(stringIn1252));
		}

		static ImmutableHashSet<char> WesternEuropeanChars128Through255
		{
			get
			{
				var returnVal = westernEuropeanChars128Through255.Value;
				return returnVal;
			}
		}

		static ImmutableHashSet<char> GetWesternEuropeanChars128Through255()
		{
			var westernEuroBytes = new byte[128];
			for (byte b = 0; b < westernEuroBytes.Length; b++)
			{
				westernEuroBytes[b] = (byte)(b + 128);
			}
			var returnVal = Encoding.GetEncoding(1252).GetChars(westernEuroBytes).ToImmutableHashSet();
			return returnVal;
		}

		static readonly Lazy<ImmutableHashSet<char>> westernEuropeanChars128Through255 = new Lazy<ImmutableHashSet<char>>(GetWesternEuropeanChars128Through255);

		#endregion

		#region IsEnglishOnlyOrEmpty

		/// <summary>
		/// Are all characters in this string within the ASCII range of 9, 10, 11, 12, 13, 32 through 127, and the Western European code page (1252) 128 through 255?
		/// </summary>
		[XmlIgnore]
		public bool IsEnglishOnlyOrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					int code = c;
					if (code < 9 || (code > 13 && code < 32) || (code > 127))
					{
						return false;
					}
				}

				return true;
			}
		}

		#endregion

		[XmlIgnore]
		public bool IsNumbersOnlyOrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					if (!char.IsDigit(c))
					{
						return false;
					}
				}

				return true;
			}
		}

		[XmlIgnore]
		public bool IsLettersOnlyOrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					if (!char.IsLetter(c))
					{
						return false;
					}
				}

				return true;
			}
		}

		[XmlIgnore]
		public bool IsLettersAndNumbersOnlyOrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					if (!char.IsLetterOrDigit(c))
					{
						return false;
					}
				}
				return true;
			}
		}

		[XmlIgnore]
		public bool IsLettersAndNumbersAndPunctuationOnlyOrEmpty
		{
			get
			{
				foreach (char c in this)
				{
					if (!IsLetterOrNumberOrPunctuation(c))
					{
						return false;
					}
				}

				return true;
			}
		}

		public ZString ConvertToOnlyLettersAndNumbersAndPunctuation()
		{
			string oldString = ToString();
			var newString = new string(oldString.Where(x => IsLetterOrNumberOrPunctuation(x)).ToArray());
			return newString;
		}

		static bool IsLetterOrNumberOrPunctuation(char c)
		{
			if (!char.IsWhiteSpace(c) &&
				!char.IsPunctuation(c) &&
				!char.IsLetterOrDigit(c) &&
				!char.IsSeparator(c) &&
				!char.IsSymbol(c) &&
				c != '\r' &&
				c != '\n')
			{
				return false;
			}

			return true;
		}

		public bool IsControlCharacter(string value)
		{
			Argument.NotNull(value, nameof(value));

			foreach (char c in value)
			{
				if (!char.IsControl(c))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Keeps ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz 0123456789
		/// </summary>
		/// <returns></returns>
		public ZString KeepAlphanumericCharacters()
		{
			return KeepChars(AlphanumericCharacters);
		}

		/// <summary>
		/// formats according to XML standards
		/// </summary>
		/// <returns></returns>
		public ZString KeepAlphanumericCharactersXMLFormatting()
		{
			return KeepCharsXMLFormatting();
		}

		/// <summary>
		/// Keeps 0123456789
		/// </summary>
		/// <returns></returns>
		public ZString KeepNumericCharacters()
		{
			return KeepChars(NumericCharacters);
		}

		/// <summary>
		/// Keeps ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz
		/// </summary>
		/// <returns></returns>
		public ZString KeepAlphabeticCharacters()
		{
			return KeepChars(AlphabeticCharacters);
		}

		public const string NumericCharacters = "0123456789";
		public const string AlphabeticCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		public const string AlphanumericCharacters = AlphabeticCharacters + NumericCharacters;

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string XmlSerializedValue
		{
			get { return this; }
			set { this = value; }
		}

		#endregion

		#region IZType Members

		[XmlIgnore]
		public ZDataType DataType
		{
			get { return ZDataType.NonNumeric; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(string); }
		}

		[XmlIgnore]
		public bool IsEmpty
		{
			get { return fValue.Length == 0 || fValue.Trim().Length == 0 || IsControlCharacter(fValue); }
		}

		[XmlIgnore]
		public bool IsValid
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZString)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return Empty; }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is IZTypeInternals zTypeInternals)
			{
				obj = zTypeInternals.GetValueForLogicalDataLayer(false);
			}

			return obj is DBNull && IsEmpty ? 0 : fValue.CompareTo(obj);
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator<char> GetEnumerator()
		{
			return fValue.GetEnumerator();
		}

		#endregion

		#region IZTypeInternals Members

		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			return IsEmpty && isNullable ? DBNull.Value : fValue;
		}

		#endregion

		#region Implementation

		// See ms-help://MS.VSCC.2003/MS.MSDNQTR.2003APR.1033/csref/html/vcrefStructTypes.htm
		// "It is an error to declare a default (parameterless) constructor for a struct. A default
		// constructor is always provided to initialize the struct members to their default values."

		// As a result, .NET will not use our constructors when initialising the struct
		// with no parameter (ie. new ZString()), thus we need to set fValue when accessed.
		// To simplify the code, all bool fields contain correct default values for a parameterless
		// constructor (ie. fIsNotEmpty = false is correct for a new ZString()).

		readonly string fValue_DoNotAccessMeDirectly;

		string fValue
		{
			get
			{
				return fValue_DoNotAccessMeDirectly ?? Empty.fValue;
			}
		}

		#endregion
	}
}
