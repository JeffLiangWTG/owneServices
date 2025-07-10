using System;
using System.Collections.Generic;
using Enterprise.ServiceManager.Runner;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Commands
{
	[TestedType(typeof(DirectRunCommandInfo))]
	class DirectRunCommandInfoTest : RequestInfoTest
	{
		[Test]
		public void TestWrongParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = new DirectRunCommandInfo(null, string.Empty, Guid.Empty));
				Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("assemblyName"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new DirectRunCommandInfo(string.Empty, null, Guid.Empty));
				Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("code"));
			});
		}

		[TestCase("value1")]
		[TestCase("value2")]
		public void TestAssemblyName(string value)
		{
			var commandInfo = new DirectRunCommandInfo(value, string.Empty, Guid.Empty);
			var result = commandInfo.AssemblyName;
			Assert.That(result, Is.EqualTo(value));
		}

		[TestCase("value1")]
		[TestCase("value2")]
		public void TestCode(string value)
		{
			var commandInfo = new DirectRunCommandInfo(string.Empty, value, Guid.Empty);
			var result = commandInfo.Code;
			Assert.That(result, Is.EqualTo(value));
		}

		[TestCase("value1")]
		[TestCase("value2")]
		public void TestConfigString(string value)
		{
			var commandInfo = new DirectRunCommandInfo(string.Empty, string.Empty, Guid.Empty, value);
			var result = commandInfo.ConfigString;
			Assert.That(result, Is.EqualTo(value));
		}

		[Test]
		public void TestToString()
		{
			Assert.Multiple(() =>
			{
				Test("assembly1", "code1", Guid.Parse("bab0bef0-775f-4bf5-9f3b-21ad480d96fb"), "config1", "[code1/bab0bef0-775f-4bf5-9f3b-21ad480d96fb], assembly [assembly1], config string [config1]");
				Test("assembly2", "code2", Guid.Parse("5f903125-effd-4371-88cf-23dc8e79ff4b"), "config2", "[code2/5f903125-effd-4371-88cf-23dc8e79ff4b], assembly [assembly2], config string [config2]");
			});

			void Test(string assemblyName, string code, Guid id, string configString, string expected)
			{
				var commandInfo = new DirectRunCommandInfo(assemblyName, code, id, configString);
				var result = commandInfo.ToString();
				Assert.That(result, Is.EqualTo(expected));
				Assert.That(result, Contains.Substring(code));
				Assert.That(result, Contains.Substring($"{id}"));
			}
		}

		protected override ICommandInfo CreateCommand(string code, string assembly, params object[] parameter)
		{
			return parameter.Length == 0
				? new DirectRunCommandInfo(assembly, code, Guid.Empty)
				: new DirectRunCommandInfo(assembly, code, Guid.Empty, (string)parameter[0]);
		}

		protected override Dictionary<RunnerLogMessageStage, (string regMessage, object[] parameters)> ExpectedLogMessages
		{
			get
			{
				return new Dictionary<RunnerLogMessageStage, (string regMessage, object[] parameters)>()
				{
					{
						RunnerLogMessageStage.ReceivedCommand,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run command received: assembly \\[TestAssembly\\], config string \\[\\].",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.PreparingExecution,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Preparing for command execution.",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.AssemblyLoaded,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Assembly is loaded for Nudge run command.",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.ServiceTaskLockAcquired,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock is acquired for single instance service task.",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.ServiceTaskLockReleased,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock released for single instance Service task.",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.ServiceTaskLockNotAcquired,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock for task could not be acquired, because the other Runner was already running the task. Reenqueueing Nudge run command.",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.GroupLockAcquired,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock is acquired for mutual exclusive group AAA.",
							new string[] { "AAA" }
						)
					},
					{
						RunnerLogMessageStage.GroupLockReleased,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock released for mutual exclusive group AAA.",
							new string[] { "AAA" }
						)
					},
					{
						RunnerLogMessageStage.GroupLockNotAcquired,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock for mutual exclusive group AAA could not be acquired, because the other Runner was already running the other task from this group. Reenqueueing Nudge run command.",
							new string[] { "AAA" }
						)
					},
					{
						RunnerLogMessageStage.ExecutingCommand,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Executing command.",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.CompletedCommand,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Command is completed.",
							Array.Empty<object>()
						)
					},
					{
						RunnerLogMessageStage.CorruptedEnvironment,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Environment is corrupted by command. Runner is stopping.",
							Array.Empty<object>()
						)
					},
				};
			}
		}
	}
}
