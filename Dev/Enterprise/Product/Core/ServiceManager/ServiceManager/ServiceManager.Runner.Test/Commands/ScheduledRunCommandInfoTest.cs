using System;
using System.Collections.Generic;
using Enterprise.ServiceManager.Runner;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Commands
{
	[TestedType(typeof(ScheduledRunCommandInfo))]
	class ScheduledRunCommandInfoTest : RequestInfoTest
	{
		[Test]
		public void TestWrongParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = new ScheduledRunCommandInfo(null, string.Empty, Guid.Empty, DateTime.Now, DateTime.Now));
				Assert.That(result.ParamName, Is.EqualTo("assemblyName"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new ScheduledRunCommandInfo(string.Empty, null, Guid.Empty, DateTime.Now, DateTime.Now));
				Assert.That(result.ParamName, Is.EqualTo("code"));
			});
		}

		[TestCase("value1")]
		[TestCase("value2")]
		public void TestAssemblyName(string assemblyName)
		{
			var commandInfo = new ScheduledRunCommandInfo(assemblyName, string.Empty, Guid.Empty, DateTime.Now, DateTime.Now);
			var result = commandInfo.AssemblyName;
			Assert.That(result, Is.EqualTo(assemblyName));
		}

		[TestCase("value1")]
		[TestCase("value2")]
		public void TestCode(string code)
		{
			var commandInfo = new ScheduledRunCommandInfo(string.Empty, code, Guid.Empty, DateTime.Now, DateTime.Now);
			var result = commandInfo.Code;
			Assert.That(result, Is.EqualTo(code));
		}

		[Test]
		public void TestExpectedNextRunTime()
		{
			Assert.Multiple(() =>
			{
				Test(new DateTime(2006, 12, 26, 13, 0, 0));
				Test(new DateTime(2019, 12, 23, 20, 20, 0));
			});

			void Test(DateTime expectedNextRunTime)
			{
				var commandInfo = new ScheduledRunCommandInfo(string.Empty, string.Empty, Guid.Empty, expectedNextRunTime, DateTime.Now);
				var result = commandInfo.ExpectedNextRunTime;
				Assert.That(result, Is.EqualTo(expectedNextRunTime));
			}
		}

		[Test]
		public void TestNextRunTime()
		{
			Assert.Multiple(() =>
			{
				Test(new DateTime(2006, 12, 26, 13, 0, 0));
				Test(new DateTime(2019, 12, 23, 20, 20, 0));
			});

			void Test(DateTime nextRunTime)
			{
				var commandInfo = new ScheduledRunCommandInfo(string.Empty, string.Empty, Guid.Empty, DateTime.Now, nextRunTime);
				var result = commandInfo.NextRunTime;
				Assert.That(result, Is.EqualTo(nextRunTime));
			}
		}

		[TestCase("value1")]
		[TestCase("value2")]
		public void TestConfigString(string configString)
		{
			var commandInfo = new ScheduledRunCommandInfo(string.Empty, string.Empty, Guid.Empty, DateTime.Now, DateTime.Now, configString);
			var result = commandInfo.ConfigString;
			Assert.That(result, Is.EqualTo(configString));
		}

		[Test]
		public void TestToString()
		{
			Assert.Multiple(() =>
			{
				Test("assembly1", "code1", Guid.Parse("bab0bef0-775f-4bf5-9f3b-21ad480d96fb"), "config1", new DateTime(2006, 12, 26, 13, 0, 0), new DateTime(2019, 12, 23, 20, 20, 0), "[code1/bab0bef0-775f-4bf5-9f3b-21ad480d96fb], assembly [assembly1], config string [config1], scheduled from [2006-12-26 13:00:00] to [2019-12-23 20:20:00]");
				Test("assembly2", "code2", Guid.Parse("5f903125-effd-4371-88cf-23dc8e79ff4b"), "config2", new DateTime(2019, 12, 23, 20, 20, 0), new DateTime(2006, 12, 26, 13, 0, 0), "[code2/5f903125-effd-4371-88cf-23dc8e79ff4b], assembly [assembly2], config string [config2], scheduled from [2019-12-23 20:20:00] to [2006-12-26 13:00:00]");
			});

			void Test(string assemblyName, string code, Guid id, string configString, DateTime expectedNextRunTime, DateTime nextRunTime, string expected)
			{
				var commandInfo = new ScheduledRunCommandInfo(assemblyName, code, id, expectedNextRunTime, nextRunTime, configString);
				var result = commandInfo.ToString();
				Assert.That(result, Is.EqualTo(expected));
				Assert.That(result, Contains.Substring(code));
				Assert.That(result, Contains.Substring($"{id}"));
			}
		}

		protected override ICommandInfo CreateCommand(string code, string assembly, params object[] parameter)
		{
			return parameter.Length == 0
				? new ScheduledRunCommandInfo(assembly, code, Guid.Empty, ExpectedNextRunTime, NextRunTime)
				: new ScheduledRunCommandInfo(assembly, code, Guid.Empty, ExpectedNextRunTime, NextRunTime, (string)parameter[0]);
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
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run command received: assembly \\[TestAssembly\\], config string \\[\\], scheduled from \\[0001-01-01 00:00:00\\] to \\[0001-01-01 00:00:00\\].",
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
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Assembly is loaded for Scheduled run command",
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
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock for task could not be acquired, because the other Runner was already running the task.",
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
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Lock for mutual exclusive group AAA could not be acquired, because the other Runner was already running the other task from this group. Rescheduling Scheduled run command.",
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
					}
				};
			}
		}

		readonly DateTime ExpectedNextRunTime;
		readonly DateTime NextRunTime;
	}
}
