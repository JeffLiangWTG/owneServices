using System.Diagnostics;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class ProcessPriorityOptionsTest : TestCase
	{
		public void TestDefault()
		{
			// Arrange
			var option = new ProcessPriorityOptions();

			// Act
			// Assert
			AssertEquals(nameof(ProcessPriorityClass.BelowNormal), option.DefaultCode);
		}

		public void TestOptionsDescription()
		{
			CombineAssertions(() =>
			{
				TestOption("Normal", "Normal Mode. Default setting of normal process.");
				TestOption("BelowNormal", "Non Blocking Mode. The threads of the Runner may let the threads of higher priority process run first.");
				TestOption("Idle", "Idle Mode. It makes runner run only when CPU is free.");
			});

			void TestOption(string priorityOption, string priorityDescription)
			{
				// Arrange
				var options = new ProcessPriorityOptions();

				// Act
				var optionDescription = options.GetDescriptionFromCode(priorityOption);
				var optionCode = options.GetCodeFromDescription(priorityDescription);

				// Assert
				AssertEquals(priorityDescription, optionDescription);
				AssertEquals(optionCode, priorityOption);
			}
		}
	}
}
