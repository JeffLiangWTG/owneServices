using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Enterprise.Registry.Business;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public class NLogCombinedFileTargetFactoryTest : TransactionedTestCase
	{
		public void TestReturnsNullIfDisabled()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.CFL, Bool2 = false, SystemDefined = true,
				},
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
				},
			};
			newValue.SetDefaultCode(LoggingMethods.FSL, true);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
			{
				// Act
				var result = new NLogCombinedFileTargetFactory(string.Empty).GetOrCreateTarget();

				// Assert
				AssertNull(result);
			}
		}

		public void TestTargetParams()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
				},
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.FSL, SystemDefined = true,
				},
			};
			newValue.SetDefaultCode(LoggingMethods.FSL, true);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
			{
				// Act
				using (var target = new NLogCombinedFileTargetFactory(string.Empty).GetOrCreateTarget())
				{
					// Assert
					var fileTarget = (FileTarget)target;
					CombineAssertions(() =>
					{
						AssertEquals(nameof(fileTarget.KeepFileOpen), true, fileTarget.KeepFileOpen);
						AssertEquals(nameof(fileTarget.ConcurrentWrites), true, fileTarget.ConcurrentWrites);
						AssertEquals(nameof(fileTarget.ConcurrentWriteAttemptDelay), 15, fileTarget.ConcurrentWriteAttemptDelay);
						AssertEquals(nameof(fileTarget.OpenFileCacheTimeout), 5, fileTarget.OpenFileCacheTimeout);
					});
				}
			}
		}

		public void TestTargetFileName()
		{
			Test(
				"Code1",
				Path.Combine(CommonProgramData.GetCargoWiseDirectory("Process Controller", string.Empty, string.Empty), "Code1_${date:format=yyyyMMdd}.log"));
			Test(
				"Code2",
				Path.Combine(CommonProgramData.GetCargoWiseDirectory("Process Controller", string.Empty, string.Empty), "Code2_${date:format=yyyyMMdd}.log"));
			Test(
				"EDISYD",
				Path.Combine(CommonProgramData.GetCargoWiseDirectory("Process Controller", string.Empty, string.Empty), "EDISYD_${date:format=yyyyMMdd}.log"));

			void Test(string installationCode, string fileName)
			{
				// Arrange
				var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
				{
					new SystemDefinableCodeDescriptionBoolWithExtraBool
					{
						Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
					},
					new SystemDefinableCodeDescriptionBoolWithExtraBool
					{
						Code = LoggingMethods.FSL, SystemDefined = true,
					},
				};
				newValue.SetDefaultCode(LoggingMethods.FSL, true);

				using (LoggerTestHelper.TemporaryLoggingConfiguration())
				using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
				{
					// Act
					using (var target = new NLogCombinedFileTargetFactory(installationCode).GetOrCreateTarget())
					{
						// Assert
						var fileTarget = (FileTarget)target;
						AssertEquals(fileName, fileTarget.FileName.ToString());
					}
				}
			}
		}

		public void TestRetentionPeriod()
		{
			Test(1);
			Test(15);
			Test(60);

			void Test(int value)
			{
				// Arrange
				var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
				{
					new SystemDefinableCodeDescriptionBoolWithExtraBool
					{
						Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
					},
					new SystemDefinableCodeDescriptionBoolWithExtraBool
					{
						Code = LoggingMethods.FSL, SystemDefined = true,
					},
				};
				newValue.SetDefaultCode(LoggingMethods.FSL, true);

				using (LoggerTestHelper.TemporaryLoggingConfiguration())
				using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
				using (SystemDataRegistry.Instance.ProcessControllerCombinedFileRetentionPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				{
					// Act
					using (var target = new NLogCombinedFileTargetFactory(string.Empty).GetOrCreateTarget())
					{
						// Assert
						var fileTarget = (FileTarget)target;
						AssertEquals(value, fileTarget.MaxArchiveFiles);
					}
				}
			}
		}

		public void TestLayoutParams()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
				},
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.FSL, SystemDefined = true,
				},
			};
			newValue.SetDefaultCode(LoggingMethods.FSL, true);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
			{
				// Act
				using (var target = (FileTarget)new NLogCombinedFileTargetFactory(string.Empty).GetOrCreateTarget())
				{
					// Assert
					var layout = (JsonLayout)target.Layout;
					CombineAssertions(() =>
					{
						AssertEquals(nameof(layout.IncludeEventProperties), true, layout.IncludeEventProperties);
						AssertContainsExactElementsInAnyOrder(
							nameof(layout.Attributes),
							new (string name, string value)[]
							{
								("eventTime", "${date:universalTime=true:format=yyyy-MM-dd\\THH\\:mm\\:ss.fffK}"),
								("message", "${message}"),
							},
							layout
								.Attributes
								.Select(attribute => (attribute.Name, attribute.Layout.ToString())));
					});
				}
			}
		}

		public void TestReturnsTargetIfExists()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
				},
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.FSL, SystemDefined = true,
				},
			};
			newValue.SetDefaultCode(LoggingMethods.FSL, true);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
			{
				using var target = new FileTarget { Name = "combinedFile" };
				LogManager.Configuration.AddTarget(target);

				// Act
				var result = new NLogCombinedFileTargetFactory(string.Empty).GetOrCreateTarget();

				// Assert
				AssertType<FileTarget>(result);
				AssertEquals(target, result);
			}
		}
	}
}
