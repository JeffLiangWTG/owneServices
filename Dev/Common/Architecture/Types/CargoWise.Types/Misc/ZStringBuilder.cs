using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Types
{
	/// <summary>
	/// ZArchitecture StringBuilder that avoids large blobs of strings from being allocated until the very end when ToString is called.
	/// </summary>
	public sealed class ZStringBuilder
	{
		public ZStringBuilder()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "str")]
		public ZStringBuilder(string str)
		{
			Argument.NotNull(str, nameof(str));
			Append(str);
		}

		public ZStringBuilder(IEnumerable<string> strings)
		{
			if (strings != null)
			{
				AppendStrings(strings);
			}
		}

		public ZStringBuilder(IEnumerable<ZString> strings)
		{
			if (strings != null)
			{
				AppendStrings(strings);
			}
		}

		void AppendStrings<T>(IEnumerable<T> strings)
		{
			Argument.NotNull(strings, nameof(strings));
			foreach (var value in strings)
			{
				if (value != null)
				{
					Append(value.ToString());
				}
			}
		}

		public ZStringBuilder Prepend(string value)
		{
			Argument.NotNull(value, nameof(value));
			length += value.Length;
			stringList.Insert(0, value);
			return this;
		}

		public ZStringBuilder Append(string value)
		{
			Argument.NotNull(value, nameof(value));
			length += value.Length;
			stringList.Add(value);
			return this;
		}

		public ZStringBuilder AppendFormat(string value, params string[] args)
		{
			Argument.NotNull(value, nameof(value));
			if (!(args != null && args.Length > 0))
			{
				throw new ArgumentException("Cannot append a string with no args", nameof(args));
			}

			var formatted = string.Format(CultureInfo.InvariantCulture, value, args);
			length += formatted.Length;
			stringList.Add(formatted);
			return this;
		}

		public ZStringBuilder AppendLine()
		{
			length += Environment.NewLine.Length;
			stringList.Add(Environment.NewLine);
			return this;
		}

		public ZStringBuilder AppendLine(string value)
		{
			Argument.NotNull(value, nameof(value));
			length += value.Length + Environment.NewLine.Length;
			stringList.Add(value);
			stringList.Add(Environment.NewLine);
			return this;
		}

		public ZStringBuilder Append(ZStringBuilder stringBuilder)
		{
			Argument.NotNull(stringBuilder, nameof(stringBuilder));
			length += stringBuilder.Length;
			stringList.AddRange(stringBuilder.stringList);
			return this;
		}

		public ZStringBuilder AppendIfNotEmpty(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				length += value.Length;
				stringList.Add(value);
			}
			return this;
		}

		/// <summary>
		/// Only adds if the value is not empty (does not consider whether or not prefix is empty).
		/// </summary>
		public ZStringBuilder AppendIfNotEmpty(string prefix, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(prefix))
				{
					length += prefix.Length + value.Length;
					stringList.Add(prefix + value);
				}
				else
				{
					length += value.Length;
					stringList.Add(value);
				}
			}
			return this;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder(Length);
			foreach (string value in stringList)
			{
				builder.Append(value);
			}
			return builder.ToString();
		}

		public string ToStringWithDelimiterBetweenAppends(string delimiter)
		{
			Argument.NotNull(delimiter, nameof(delimiter));
			string result = string.Empty;
			if (stringList.Count > 0)
			{
				StringBuilder builder = new StringBuilder(Length + (stringList.Count - 1) * delimiter.Length);
				builder.Append(stringList[0]);
				for (int i = 1; i < stringList.Count; i++)
				{
					builder.Append(delimiter);
					builder.Append(stringList[i]);
				}
				result = builder.ToString();
			}
			return result;
		}

		public string ToStringWithNewLineBetweenAppends()
		{
			return ToStringWithDelimiterBetweenAppends(Environment.NewLine);
		}

		public ZStringBuilder Clear()
		{
			stringList.Clear();
			length = 0;
			return this;
		}

		public bool IsEmpty
		{
			get { return Length == 0; }
		}

		public int Length
		{
			get
			{
				return length;
			}
		}

		int length;

		readonly List<string> stringList = new List<string>();
	}
}
