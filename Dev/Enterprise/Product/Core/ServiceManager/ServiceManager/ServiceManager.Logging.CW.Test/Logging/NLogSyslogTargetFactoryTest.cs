using System;
using Enterprise.Registry.Business;
using NLog;
using NLog.Targets.Wrappers;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public class NLogSyslogTargetFactoryTest : TransactionedTestCase
	{
		public void TestReturnsBufferedTargetIfExists()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true,
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
				SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
				using var target = new BufferingTargetWrapper { Name = "syslog" };
				LogManager.Configuration.AddTarget(target);

				// Act
				var result = new NLogSyslogTargetFactory().GetOrCreateTarget();

				// Assert
				AssertType<BufferingTargetWrapper>(result);
				AssertEquals(target, result);
			}
		}
	}
}
