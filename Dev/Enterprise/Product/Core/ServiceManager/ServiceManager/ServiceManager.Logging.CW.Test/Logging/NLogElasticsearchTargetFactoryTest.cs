using System;
using Enterprise.Registry.Business;
using NLog;
using NLog.Targets.Wrappers;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public class NLogElasticsearchTargetFactoryTest : TransactionedTestCase
	{
		public void TestReturnsBufferedTargetIfExists()
		{
			// Arrange
			var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
				{
					Code = LoggingMethods.ELK,
					Bool2 = true,
					SystemDefined = true,
				},
			};
			newValue.SetDefaultCode(LoggingMethods.ELK, true);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
			{
				using var target = new BufferingTargetWrapper { Name = "elasticsearch" };
				LogManager.Configuration.AddTarget(target);

				// Act
				var result = new NLogElasticsearchTargetFactory().GetOrCreateTarget();

				// Assert
				AssertType<BufferingTargetWrapper>(result);
				AssertEquals(target, result);
			}
		}
	}
}
