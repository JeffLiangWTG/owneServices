using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared.Interfaces;
using Moq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	class LoggerNLogRegistryConfigurationTest : TransactionedTestCase
	{
		public class RefreshTest : TransactionedTestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				disposableConfiguration = LoggerTestHelper.TemporaryLoggingConfiguration();
			}

			protected override void TearDown()
			{
				disposableConfiguration?.Dispose();
				base.TearDown();
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestVerboseEnablesDebugLevel()
			{
				foreach (var code in Source.Codes)
				{
					Test(code);
				}

				void Test(string code)
				{
					// Arrange
					using var target = new MemoryTarget();

					LogManager.Configuration.RemoveRuleByName(code);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, target) { RuleName = code, });
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection { new VerboseLoggingBusinessObject { Code = code, Value = ZDateTime.UtcNow.AddDays(1), SystemDefined = true, }, }))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(code);
						AssertEquals(code, true, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestVerboseDoesNotModifyOthers()
			{
				foreach (var code in Source.Codes)
				{
					foreach (var nameTemplate in Source.NotMatchingNameTemplates)
					{
						foreach (var logLevel in LogLevel.AllLoggingLevels)
						{
							Test($"{code} {nameTemplate}", code, nameTemplate, logLevel);
						}
					}
				}

				void Test(string testCaseName, string code, string nameTemplate, LogLevel level)
				{
					// Arrange
					using var target = new MemoryTarget();
					var ruleName = string.Format(nameTemplate, code);
					LogManager.Configuration.RemoveRuleByName(ruleName);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", level, target)
					{
						RuleName = ruleName,
					});
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection { new VerboseLoggingBusinessObject { Code = code, Value = ZDateTime.UtcNow.AddDays(1), SystemDefined = true, }, }))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(ruleName);
						AssertEquals(testCaseName, true, rule?.IsLoggingEnabledForLevel(level));
						AssertEquals(testCaseName, level <= LogLevel.Debug, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestVerboseKeepsOriginalHighLevel()
			{
				foreach (var code in Source.Codes)
				{
					foreach (var logLevel in LogLevel.AllLoggingLevels
								.Where(level => level > LogLevel.Debug))
					{
						Test($"{code} {logLevel:G}", code, logLevel);
					}
				}

				void Test(string testCaseName, string code, LogLevel logLevel)
				{
					// Arrange
					using var target = new MemoryTarget();
					LogManager.Configuration.RemoveRuleByName(code);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", logLevel, logLevel, target)
					{
						RuleName = code,
					});
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection { new VerboseLoggingBusinessObject { Code = code, Value = ZDateTime.UtcNow.AddDays(1), SystemDefined = true, }, }))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(code);
						AssertEquals(testCaseName, true, rule?.IsLoggingEnabledForLevel(logLevel));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestExpiredVerboseDisablesDebugLevelForServiceTaskCodes()
			{
				foreach (var code in Source.Codes)
				{
					Test(code);
				}

				void Test(string code)
				{
					// Arrange
					using var target = new MemoryTarget();

					LogManager.Configuration.RemoveRuleByName(code);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, LogLevel.Fatal, target)
					{
						RuleName = code,
					});
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection { new VerboseLoggingBusinessObject { Code = code, Value = ZDateTime.UtcNow.AddDays(-1), SystemDefined = true, }, }))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(code);
						AssertEquals(code, false, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestNonVerboseDisablesDebugLevelForServiceTaskCodes()
			{
				foreach (var code in Source.Codes)
				{
					Test(code);
				}

				void Test(string code)
				{
					// Arrange
					using var target = new MemoryTarget();

					LogManager.Configuration.RemoveRuleByName(code);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, LogLevel.Fatal, target)
					{
						RuleName = code,
					});
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection()))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(code);
						AssertEquals(code, false, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestNonVerboseDoesNotDisableDebugLevelForNonServiceTaskCodes()
			{
				foreach (var code in Source.NonCodes)
				{
					Test(code);
				}

				void Test(string code)
				{
					// Arrange
					using var target = new MemoryTarget();

					LogManager.Configuration.RemoveRuleByName(code);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, LogLevel.Fatal, target)
					{
						RuleName = code,
					});
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection()))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(code);
						AssertEquals(code, true, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestNoTaskRecordInCollectionDisablesDebugLevel()
			{
				foreach (var code in Source.Codes)
				{
					Test(code);
				}

				void Test(string code)
				{
					// Arrange
					using var target = new MemoryTarget();

					LogManager.Configuration.RemoveRuleByName(code);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, LogLevel.Fatal, target)
					{
						RuleName = code,
					});
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection { new VerboseLoggingBusinessObject { Code = "OTHR", Value = ZDateTime.UtcNow, SystemDefined = true, }, }))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(code);
						AssertEquals(code, false, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestNonVerboseDoesNotModifyOthers()
			{
				foreach (var code in Source.Codes)
				{
					Test(code);
				}

				void Test(string code)
				{
					// Arrange
					using var target = new MemoryTarget();
					const string ruleName = "XXX";
					LogManager.Configuration.RemoveRuleByName(ruleName);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target)
					{
						RuleName = ruleName,
					});
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target)
					{
						RuleName = code,
					});
					LogManager.Configuration.Reload();

					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection
								{
									new VerboseLoggingBusinessObject { Code = code, Value = ZDateTime.UtcNow.AddDays(-1), SystemDefined = true, },
									new VerboseLoggingBusinessObject { Code = "XXX", Value = ZDateTime.UtcNow.AddDays(1), SystemDefined = true, },
								}))
					{
						var configuration = new LoggerNLogRegistryConfiguration();

						// Act
						configuration.Refresh();

						// Assert
						var rule = LogManager.Configuration.FindRuleByName(ruleName);
						AssertEquals(code, true, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			public void TestDoesNotUpdateUntilTimeout()
			{
				// Arrange
				using var target = new MemoryTarget();
				const string code = "TST";
				LogManager.Configuration.RemoveRuleByName(code);
				LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, target) { RuleName = code, });
				LogManager.Configuration.Reload();

				var configuration = new LoggerNLogRegistryConfiguration();
				FirstRefreshEnablesVerboseLogging();

				using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
							Guid.Empty,
							Guid.Empty,
							new VerboseLoggingCollection { new VerboseLoggingBusinessObject { Code = code, Value = ZDateTime.UtcNow.AddDays(-1), SystemDefined = true, }, }))
				{
					// Act
					configuration.Refresh();

					// Assert
					var rule = LogManager.Configuration.FindRuleByName(code);
					AssertEquals(true, rule?.IsLoggingEnabledForLevel(LogLevel.Debug));
				}

				void FirstRefreshEnablesVerboseLogging()
				{
					using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty,
								Guid.Empty,
								Guid.Empty,
								new VerboseLoggingCollection { new VerboseLoggingBusinessObject { Code = code, Value = ZDateTime.UtcNow.AddDays(1), SystemDefined = true, }, }))
					{
						configuration.Refresh();
					}
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			[TestUtcOffset(11, 0, 0)]
			[UseSnapshotProtection(true)]
			public void TestDoesNotUpdateFromAnotherThread()
			{
				// Arrange
				using var target = new MemoryTarget();
				const string code = "TST";
				LogManager.Configuration.RemoveRuleByName(code);
				LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, target) { RuleName = code, });
				LogManager.Configuration.Reload();

				var registrySettings = SharedRegistry.Instance;
				using (var waitingOnValue = new ManualResetEvent(false))
				using (var readyToProceed = new ManualResetEvent(false))
				using (new DisposableAction(() => SharedRegistry.Instance = registrySettings))
				{
					var thread = FirstRefreshStartsAndWaits(waitingOnValue, readyToProceed);
					Assert(waitingOnValue.WaitOne(TimeSpan.FromSeconds(15)));

					var configuration = new LoggerNLogRegistryConfiguration();
					var registrySettingsMock = new Mock<ISharedRegistrySettings>();
					SharedRegistry.Instance = registrySettingsMock.Object;

					// Act
					configuration.Refresh();

					// Assert
					readyToProceed.Set();
					Assert(thread.Join(TimeSpan.FromSeconds(15)));
					AssertNoExceptionThrown(() => registrySettingsMock.VerifyNoOtherCalls());
				}

				Thread FirstRefreshStartsAndWaits(ManualResetEvent reachedThePointEvent, ManualResetEvent readyToProceed)
				{
					var thread = new Thread(() =>
					{
						var businessObject = new Mock<VerboseLoggingBusinessObject>
						{
							CallBase = true,
							Object =
							{
								Code = code,
								SystemDefined = true,
								Value = ZDateTime.UtcNow.AddDays(1),
							},
						};

						var configuration = new LoggerNLogRegistryConfiguration();

						var sharedRegistryMock = new Mock<ISharedRegistrySettings>();
						sharedRegistryMock
							.As<ILoggerRegistrySettings>()
							.Setup(settings => settings.ProcessControllerVerboseLogging)
							.Returns(new VerboseLoggingCollection { businessObject.Object, });

						SharedRegistry.Instance = sharedRegistryMock.Object;

						businessObject
							.SetupGet(o => o.Code)
							.Returns(() =>
							{
								reachedThePointEvent.Set();
								Assert(readyToProceed.WaitOne(TimeSpan.FromSeconds(15)));
								return code;
							});
						configuration.Refresh();
					});
					thread.Start();
					return thread;
				}
			}

			IDisposable disposableConfiguration;

			public static class Source
			{
				public static IEnumerable<string> Codes => new[]
				{
					"TS1",
					"TS2",
					"HOST"
				};

				public static IEnumerable<string> NonCodes => new[]
				{
					"*",
					"&^",
					"$#&"
				};

				public static IEnumerable<string> NotMatchingNameTemplates => new[]
				{
					"name",
					"{0}suffix",
					"prefix{0}",
					"prefix{0}suffix",
				};
			}
		}
	}
}
