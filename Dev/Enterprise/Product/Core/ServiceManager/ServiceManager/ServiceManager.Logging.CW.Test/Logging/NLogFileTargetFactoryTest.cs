using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	class NLogFileTargetFactoryTest : TransactionedTestCase
	{
		public void TestCommonParams()
		{
			// Arrange
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (TemporarilyEnableLoggingInRegistry(LoggingMethods.FSL))
			{
				CombineAssertions(() =>
				{
					Test("name1", "path1", true);
					Test("name2", "path2", false);
				});
			}

			void Test(string name, string path, bool archiveLogFiles)
			{
				// Arrange
				var expectedFileName = $"{Path.Combine(path, "${logger}_${date:format=yyyyMMdd}.txt")}";
				const string expectedLayout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff:padding=-25}${event-properties:severity:padding=-25}${event-properties:process:objectpath=pid:padding=-25}${when:when='${scopeproperty:exe}' == 'Runner':inner=PID=${event-properties:process:objectpath=pid} *FROM RUNNER*\\: ${message}:else=${message}}";
				var expectedArchiveFileName = $@"{path}\${{logger}}_{{#}}.txt";

				// Act
				var target = (FileTarget)new NLogFileTargetFactory(name, path, archiveLogFiles).GetOrCreateTarget();

				// Assert
				AssertEquals(nameof(target.Name), NLogFileTargetFactory.GetLogfileTargetName(name), target.Name);
				AssertEquals(nameof(target.FileName), expectedFileName, target.FileName?.ToString());
				AssertEquals(nameof(target.Layout), expectedLayout, target.Layout?.ToString());
				AssertEquals(nameof(target.KeepFileOpen), true, target.KeepFileOpen);
				AssertEquals(nameof(target.OpenFileCacheSize), 3, target.OpenFileCacheSize);
				AssertEquals(nameof(target.ConcurrentWrites), true, target.ConcurrentWrites);
				AssertEquals(nameof(target.ConcurrentWriteAttempts), 10, target.ConcurrentWriteAttempts);
				AssertEquals(nameof(target.ConcurrentWriteAttemptDelay), 5, target.ConcurrentWriteAttemptDelay);
				AssertEquals(nameof(target.ArchiveFileName), expectedArchiveFileName, target.ArchiveFileName?.ToString());
				AssertEquals(nameof(target.ArchiveDateFormat), "yyyyMMdd.HHmmss-fff", target.ArchiveDateFormat);
				AssertEquals(nameof(target.ArchiveNumbering), ArchiveNumberingMode.DateAndSequence, target.ArchiveNumbering);
				AssertEquals(nameof(target.ArchiveEvery), FileArchivePeriod.None, target.ArchiveEvery);
			}
		}

		public void TestEnableArchive()
		{
			// Arrange
			var path = CommonProgramData.GetCargoWiseDirectory("Process Controller", System.Environment.MachineName, Db.DatabaseName);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (TemporarilyEnableLoggingInRegistry(LoggingMethods.FSL))
			{
				// Act
				var target = (FileTarget)new NLogFileTargetFactory(LoggingMethods.FSL, path, true).GetOrCreateTarget();

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(1024 * 1024 * 10, target.ArchiveAboveSize);
					AssertEquals(nameof(target.OpenFileCacheTimeout), 60, target.OpenFileCacheTimeout);
				});
			}
		}

		public void TestDisableArchive()
		{
			// Arrange
			var path = CommonProgramData.GetCargoWiseDirectory("Process Controller", System.Environment.MachineName, Db.DatabaseName);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (TemporarilyEnableLoggingInRegistry(LoggingMethods.FSL))
			{
				// Act
				var target = (FileTarget)new NLogFileTargetFactory(LoggingMethods.FSL, path, false).GetOrCreateTarget();

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(nameof(target.ArchiveAboveSize), -1, target.ArchiveAboveSize);
					AssertEquals(nameof(target.OpenFileCacheTimeout), 5, target.OpenFileCacheTimeout);
				});
			}
		}

		static IDisposable TemporarilyEnableLoggingInRegistry(ZString loggingMethod)
		{
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = loggingMethod,
					Bool2 = true,
					SystemDefined = true,
				},
			};
			newValue.SetDefaultCode(loggingMethod, true);

			return SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
		}
	}
}
