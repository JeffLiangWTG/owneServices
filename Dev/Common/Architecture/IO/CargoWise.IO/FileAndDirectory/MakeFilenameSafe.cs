using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace CargoWise.IO
{
	public static class MakeFilenameSafe
	{
		/// <summary>
		/// Replaces all invalid filename chars, such as "<>/:, with a space.
		/// </summary>
		public static string MakeSafe(string potentiallyUnsafeFileName)
		{
			Argument.NotNull(potentiallyUnsafeFileName, nameof(potentiallyUnsafeFileName));
			return MakeSafe(potentiallyUnsafeFileName, ' ');
		}

		/// <summary>
		/// Replaces all invalid filename chars, such as "<>/:, with nothing.
		/// </summary>
		public static string MakeSafeAndFixFileExtension(string potentiallyUnsafeFileName, char replacementChar)
		{
			Argument.NotNull(potentiallyUnsafeFileName, nameof(potentiallyUnsafeFileName));

			int fileExtensionStart = potentiallyUnsafeFileName.LastIndexOf('.');
			if (fileExtensionStart >= 0)
			{
				var stringBuilder = new StringBuilder(potentiallyUnsafeFileName);
				foreach (var ch in Path.GetInvalidFileNameChars())
				{
					stringBuilder.Replace(ch, replacementChar, 0, fileExtensionStart);
					stringBuilder.Replace(new string(ch, 1), string.Empty, fileExtensionStart, stringBuilder.Length - fileExtensionStart);
				}

				return stringBuilder.ToString();
			}
			else
			{
				return MakeSafe(potentiallyUnsafeFileName, replacementChar);
			}
		}

		/// <summary>
		/// Replaces all invalid filename chars, such as "<>/:, with your spacified char.
		/// </summary>	 	
		public static string MakeSafe(string potentiallyUnsafeFileName, char replacementChar)
		{
			Argument.NotNull(potentiallyUnsafeFileName, nameof(potentiallyUnsafeFileName));
			var newString = ReplaceAll(new StringBuilder(potentiallyUnsafeFileName), replacementChar, Path.GetInvalidFileNameChars());

			var whitespaceBeforeLastDotRegex = new Regex("\\s+\\.(?=\\w+$)");
			return whitespaceBeforeLastDotRegex.Replace(newString.ToString().Trim(), ".");
		}

		public static string MakeSafePath(string potentiallyUnsafeFileName, char replacementChar)
		{
			Argument.NotNull(potentiallyUnsafeFileName, nameof(potentiallyUnsafeFileName));
			return ReplaceAll(new StringBuilder(potentiallyUnsafeFileName), replacementChar, Path.GetInvalidPathChars()).ToString();
		}

		/// <summary>
		/// Checks that a filename does not contain invalid chars, such as "<>/:
		/// </summary>
		public static bool IsSafe(string filename)
		{
			Argument.NotNull(filename, nameof(filename));
			return !Path.GetInvalidFileNameChars().Any(invalidChar => filename.IndexOf(invalidChar) > -1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Array used as readonly only but still .NET 4 so immutable collections are not available")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Reserved file name.")]
		static readonly string[] WindowsReservedFileNames = new string[]
		{
			"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM1", "LPT1", "LPT2", "LPT3", "COM2", "COM3", "COM4"
		};

		public static bool IsWindowsReservedFileName(string fileName)
		{
			Argument.NotNull(fileName, nameof(fileName));
			if (!IsSafe(fileName))
			{
				fileName = MakeSafe(fileName);
			}
			return (WindowsReservedFileNames.Contains(Path.GetFileNameWithoutExtension(fileName).Trim().ToUpper()));
		}

		static StringBuilder ReplaceAll(StringBuilder builder, char replacement, char[] charsToReplace)
		{
			Argument.NotNull(builder, nameof(builder));
			Argument.NotNull(charsToReplace, nameof(charsToReplace));

			var result = charsToReplace.Aggregate(builder, (sb, badChar) => sb.Replace(badChar, replacement));
			return result;
		}
	}
}
