using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Commands
{
	[TestsSubclassesOf(typeof(ICommandInfo))]
	abstract class RequestInfoTest
	{
		public const string GuidRegExTemplate = "[0-9a-z]{8}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{12}";
		protected const string FormatLogMessagesTestCode = "TES";
		protected const string FormatLogMessagesTestAssembly = "TestAssembly";

		protected abstract Dictionary<RunnerLogMessageStage, (string regMessage, object[] parameters)> ExpectedLogMessages { get; }
		protected abstract ICommandInfo CreateCommand(string code, string assembly, params object[] parameter);

		[Test]
		public void TestFormatLogMessages()
		{
			// Arrange
			var command = CreateCommand(FormatLogMessagesTestCode, FormatLogMessagesTestAssembly);

			Assert.Multiple(() =>
			{
				ExpectedLogMessages
					.ToList()
					.ForEach(x =>
					{
						var expectMessage = x.Value;

						// Act
						var result = command.FormatRequestToLogMessage(x.Key, expectMessage.parameters);

						// Assert
						var reg = new Regex(expectMessage.regMessage);
						Assert.That(result, Does.Match(reg), $"{expectMessage.regMessage} is not regular express of {result}");
					});
			});
		}
	}
}
