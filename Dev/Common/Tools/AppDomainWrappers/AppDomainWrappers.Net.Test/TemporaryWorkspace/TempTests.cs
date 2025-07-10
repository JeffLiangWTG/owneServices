using System;
using System.IO;
using NUnit.Framework;

namespace AppDomainWrappers.Net.Test
{
	public class TempTests : TestCase
	{
		public void TestTempPathWithoutCreating_ReturnsWiseTechTempDirectory()
		{
			// Arrange
			// Attempts to get the temp path in this order: TMP > TEMP > LocalApplicationData
			var tempDirectory = Environment.GetEnvironmentVariable("TMP")
								?? Environment.GetEnvironmentVariable("TEMP")
								?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

			// Ensure the temp path ends with "Temp" if it doesn't already
			if (!tempDirectory.EndsWith("Temp", StringComparison.InvariantCultureIgnoreCase))
			{
				tempDirectory = Path.Combine(tempDirectory, "Temp");
			}

			var expectedPath = Path.Combine(tempDirectory, "WiseTechGlobal");

			// Act
			var tempPath = Temp.TempPathWithoutCreating;
			var actual = tempPath.StartsWith(expectedPath, StringComparison.InvariantCultureIgnoreCase);

			// Assert
			AssertEquals("Expected path to not be empty, but it was.", expected: false, string.IsNullOrWhiteSpace(tempPath));
			AssertEquals($"Expected path to start with {expectedPath}, but it did not.", expected: true, actual);
		}
	}
}
