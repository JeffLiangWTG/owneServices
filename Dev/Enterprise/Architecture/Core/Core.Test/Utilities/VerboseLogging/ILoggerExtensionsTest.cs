using System;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ILoggerExtensionsTest : TestCase
	{
		public void TestLoggingWithExtensionMethods()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				string testFile = Path.Combine(tempDir, "forverbosetesting.txt");
				Assert("Precondition: !File.Exists(FilenameForTesting)", !File.Exists(testFile));
				var logger = new VerboseLogger(testFile);
				Assert("Prewcondition: string.IsNullOrEmpty(File.ReadAllText(FilenameForTesting))", string.IsNullOrEmpty(File.ReadAllText(testFile)));

				logger.Log(LogType.Error, delegate
				{ return "DANIEL"; });
				string text = File.ReadAllText(testFile);
				Assert(text.Contains("DANIEL"));

				logger.Log(LogType.Error, delegate
				{ throw new Exception("DANIEL CRASH"); });
				text = File.ReadAllText(testFile);
				Assert(text.Contains("DANIEL CRASH"));

				AssertEquals("Logging failure reported to CW", true, ErrorReporter.LastMessageReported.Contains("Could not get the text to be logged, exception was thrown. DANIEL CRASH"));
				ErrorReporter.Clear();
			}
		}

		public void TestWarningWithSummary()
		{
			Assert("Precondition: default language should be left to right", !Res.IsRightToLeft(Res.CurrentLanguage));

			var warningMessages = new[]
			{
				"Same code",
				"May want to reconsider this",
				"This is a warning"
			};

			var logger = new SimpleLogger();
			logger.WarningWithSummary("Things you may want to consider", warningMessages);

			var expected = @"Warning: Things you may want to consider
•  Same code
•  May want to reconsider this
•  This is a warning";

			AssertMultilineASCIIEquals(expected, logger.ToString());
		}

		public void TestWarningWithSummary_RightToLeftLanguage()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Arabic))
			{
				Assert("Precondition: using a RTL language", Res.IsRightToLeft(Res.CurrentLanguage));

				var warningMessages = new[]
				{
					"Same code",
					"May want to reconsider this",
					"This is a warning"
				};

				var logger = new SimpleLogger();
				logger.WarningWithSummary("Things you may want to consider", warningMessages);

				var expected = @"Warning: Things you may want to consider
Same code  •  
May want to reconsider this  •  
This is a warning  •  ";

				AssertMultilineASCIIEquals("Bulletpoints should be on right for RTL languages", expected, logger.ToString());
			}
		}

		public void TestErrorWithSummary()
		{
			Assert("Precondition: default language should be left to right", !Res.IsRightToLeft(Res.CurrentLanguage));

			var errorMessages = new[]
			{
				"Off by one error",
				"Divide by zero",
				"Null reference"
			};

			var logger = new SimpleLogger();
			logger.ErrorWithSummary("Things not to do", errorMessages);

			var expected = @"Error: Things not to do
•  Off by one error
•  Divide by zero
•  Null reference";

			AssertMultilineASCIIEquals(expected, logger.ToString());
		}

		public void TestErrorWithSummary_RightToLeftLanguage()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Arabic))
			{
				Assert("Precondition: using a RTL language", Res.IsRightToLeft(Res.CurrentLanguage));

				var errorMessages = new[]
				{
					"Off by one error",
					"Divide by zero",
					"Null reference"
				};

				var logger = new SimpleLogger();
				logger.ErrorWithSummary("Things not to do", errorMessages);

				var expected = @"Error: Things not to do
Off by one error  •  
Divide by zero  •  
Null reference  •  ";

				AssertMultilineASCIIEquals("Bulletpoints should be on right for RTL languages", expected, logger.ToString());
			}
		}
	}
}
