using System;
using System.Collections.Generic;
using System.IO;

namespace CWNetCoreTest.TestAdapter.Utilities
{
	public class AssemblyResourceProvider : IEmbeddedResourceProvider
	{
		public const string FileName = "Explicit";
		public const string FileExtension = ".txt";
		public static readonly string FileSearchPattern = $"{FileName}*{FileExtension}";

		public HashSet<string> LoadExplicitTestsFromContentFiles(string relativeDirectoryName, Func<string, bool>? filter = null)
		{
			if (string.IsNullOrWhiteSpace(relativeDirectoryName))
			{
				throw new ArgumentException("Directory name cannot be null or empty.", nameof(relativeDirectoryName));
			}

			var relativeDirectory = Path.Combine(AppContext.BaseDirectory, relativeDirectoryName);

			if (!Directory.Exists(relativeDirectoryName))
			{
				throw new DirectoryNotFoundException($"Directory not found: {relativeDirectory}");
			}

			var explicitTestNames = new HashSet<string>(StringComparer.Ordinal);

			var files = Directory.GetFiles(relativeDirectory, FileSearchPattern, SearchOption.TopDirectoryOnly);

			foreach (var file in files)
			{
				foreach (var line in File.ReadLines(file))
				{
					var trimmedLine = line.Trim();

					if (filter != null && filter(trimmedLine))
					{
						continue;
					}

					_ = explicitTestNames.Add(trimmedLine);
				}
			}

			return explicitTestNames;
		}

		public static bool ShouldSkipLine(string input)
		{
			return string.IsNullOrWhiteSpace(input) || (input.Trim() is string trimmedString && trimmedString.StartsWith('#'));
		}

		public static string RemoveCommentFromLine(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return string.Empty;
			}

			input = input.Trim();

			if (input.StartsWith('#'))
			{
				return string.Empty;
			}

			var commentIndex = input.IndexOf('#');
			if (commentIndex > 0)
			{
				return input[..commentIndex].Trim();
			}

			return input;
		}
	}
}
