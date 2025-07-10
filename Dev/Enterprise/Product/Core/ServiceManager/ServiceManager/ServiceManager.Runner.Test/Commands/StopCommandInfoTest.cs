using System;
using System.Collections.Generic;
using Enterprise.ServiceManager.Runner;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Commands
{
	[TestedType(typeof(StopCommandInfo))]
	class StopCommandInfoTest : RequestInfoTest
	{
		protected override Dictionary<RunnerLogMessageStage, (string regMessage, object[] parameters)> ExpectedLogMessages
		{
			get
			{
				return new Dictionary<RunnerLogMessageStage, (string regMessage, object[] parameters)>()
				{
					{
						RunnerLogMessageStage.ReceivedCommand,
						(
							$"\\[{GuidRegExTemplate}\\] Received command \\[Stop\\].",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.PreparingExecution,
						(
							$"\\[{GuidRegExTemplate}\\] Preparing for Stopping Runner.",
							Array.Empty<object>()
						)
					}
				};
			}
		}

		[Test]
		public void TestToString()
		{
			var commandInfo = new StopCommandInfo();
			var result = commandInfo.ToString();
			Assert.That(result, Is.EqualTo("command [Stop]"));
		}

		protected override ICommandInfo CreateCommand(string code, string assembly, params object[] parameter)
		{
			return new StopCommandInfo();
		}
	}
}
