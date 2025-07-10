using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace AppDomainWrappers.Net.Test
{
	public class DirectoryHelperTests : TestCase
	{
		public void TestGenerateRandomNumberShouldReturnFiveDigitString()
		{
			// Act
			var result = DirectoryHelper.GenerateRandomNumber(10000, 100000);

			// Assert
			AssertEquals(expected: 5, result.Length);
			AssertEquals(expected: true, int.TryParse(result, out var _)); // Ensure it's a valid number.
		}

		public void TestGenerateRandomNumberShouldReturnUniqueFiveDigitStrings()
		{
			// Arrange
			HashSet<string> generatedNumbers = new();
			var iterations = 1000;

			// Act
			for (var i = 0; i < iterations; i++)
			{
				var result = DirectoryHelper.GenerateRandomNumber(10000, 100000);
				generatedNumbers.Add(result);
			}

			// Assert
			AssertCloseEnough("The number of unique five-digit strings generated does not match the expected count.", iterations, generatedNumbers.Count);
		}

		public void TestGetDirectoryNameShouldReturnCorrectDirectoryName()
		{
			List<(string Input, string Expected)> testCases = new()
			{
				(@"C:\Users\Test\Documents\File.txt", "Documents"),
				(@"C:\Users\Test\Documents\", "Documents"),
				(@"C:\File.txt", string.Empty),
				(@"Documents\File.txt", "Documents"),
				(@"File.txt", "File.txt"),
				(@"Documents", "Documents"),
			};

			// Act & Assert
			CombineAssertions(() =>
			{
				for (var i = 0; i < testCases.Count; i++)
				{
					var (input, expected) = testCases[i];
					var actual = DirectoryHelper.GetDirectoryName(input);
					AssertEquals(expected, actual);
				}
			});
		}

		public void TestCreateTempDirectoryShouldCreateDirectoryWithSecuritySettings()
		{
			// Arrange
			var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			try
			{
				// Act
				var result = DirectoryHelper.CreateLowSecurityDirectory(tempDirPath);

				// Assert
				AssertEquals(expected: true, Directory.Exists(tempDirPath));
				AssertEquals(expected: tempDirPath, result.FullName);
			}
			finally
			{
				// Cleanup
				if (Directory.Exists(tempDirPath))
				{
					Directory.Delete(tempDirPath, recursive: true);
				}
			}
		}
	}
}
